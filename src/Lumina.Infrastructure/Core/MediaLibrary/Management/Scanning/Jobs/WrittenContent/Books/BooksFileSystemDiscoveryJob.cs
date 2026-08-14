#region ========================================================================= USING =====================================================================================
using Lumina.Domain.Common.Primitives;
using Lumina.Application.Common.DataAccess.Entities.MediaLibrary.Management;
using Lumina.Application.Common.DataAccess.Repositories.MediaLibrary;
using Lumina.Application.Common.DataAccess.UoW;
using Lumina.Application.Common.Mapping.MediaLibrary.Management;
using Lumina.Domain.SharedKernel.Common.Enums.MediaLibrary;
using Lumina.Domain.Common.Events;
using Lumina.Domain.Core.BoundedContexts.FileSystemManagementBoundedContext.FileSystemManagementAggregate.ValueObjects;
using Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryAggregate;
using Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryScanAggregate.Events;
using Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryScanAggregate.Services.Jobs;
using Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryScanAggregate.ValueObjects;
using Lumina.Domain.Core.BoundedContexts.WrittenContentLibraryBoundedContext.BookLibraryAggregate.Services.Jobs;
using Lumina.Infrastructure.Core.MediaLibrary.Management.Scanning.Jobs.Common;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Infrastructure.Core.MediaLibrary.Management.Scanning.Jobs.WrittenContent.Books;

/// <summary>
/// Media library scan job for discovering book file system items.
/// </summary>
internal sealed class BooksFileSystemDiscoveryJob : MediaLibraryScanJob, IBooksFileSystemDiscoveryJob
{
    private const int STAGING_BATCH_SIZE = 1000; // the number of discovered files that are written to the staging results in a single batch, keeping the peak memory bounded regardless of the library size
    private static readonly HashSet<string> s_bookExtensions = new(
        [
            ".pdf", ".epub", ".mobi", ".azw", ".azw3",
            ".cbz", ".cbr", ".djvu", ".fb2", ".lit",
            ".prc", ".txt", ".doc", ".docx", ".rtf"
        ],
        StringComparer.OrdinalIgnoreCase
    );
    private static readonly EnumerationOptions s_enumerationOptions = new()
    {
        AttributesToSkip = FileAttributes.None, // hidden files and directories are included in the discovery
        IgnoreInaccessible = true,
        RecurseSubdirectories = false
    };
    private readonly IServiceScopeFactory _serviceScopeFactory;

    /// <summary>
    /// Initializes a new instance of the <see cref="BooksFileSystemDiscoveryJob"/> class.
    /// </summary>
    /// <param name="serviceScopeFactory">
    /// Injected factory for creating scopes in which services are requested.
    /// See docs/technical/architecture/architecture-knowledge-management/architecture-decision-log/architecture-decision-record-0001.md for details.
    /// </param>
    public BooksFileSystemDiscoveryJob(IServiceScopeFactory serviceScopeFactory)
    {
        _serviceScopeFactory = serviceScopeFactory;
    }

    /// <inheritdoc/>
    public override async Task ExecuteAsync<TInput>(Guid id, TInput input, CancellationToken cancellationToken)
    {
        try
        {
            // increment the number of parents that finished their execution and called this job (beware race conditions, jobs run in parallel)
            int parentsCompleted = Interlocked.Increment(ref parentsPayloadsExecuted);
            // only execute this job's payload when it has no parents, or when all the parents finished their execution
            if (Parents.Count == 0 || parentsCompleted == Parents.Count)
            {
                // this needs to be wrapped in a task because even though this job is processed in a "fire and forget" async manner, it still does synchronous
                // file system processing that takes time, and would block the processing of scan jobs in the in-memory queue
                await Task.Run(async () =>
                {
                    Status = LibraryScanJobStatus.Running;
                    // see docs/technical/architecture/architecture-knowledge-management/architecture-decision-log/architecture-decision-record-0001.md for details:
                    await using AsyncServiceScope asyncServiceScope = _serviceScopeFactory.CreateAsyncScope();
                    IUnitOfWork unitOfWork = asyncServiceScope.ServiceProvider.GetService<IUnitOfWork>()!;
                    IDomainEventPublisher domainEventPublisher = asyncServiceScope.ServiceProvider.GetService<IDomainEventPublisher>()!;
                    ILibraryRepository libraryRepository = unitOfWork.GetRepository<ILibraryRepository>()!;
                    ILibraryScanStagingResultsRepository stagingResultsRepository = unitOfWork.GetRepository<ILibraryScanStagingResultsRepository>();
                    IDirectoryScanFingerprintRepository directoryScanFingerprintRepository = unitOfWork.GetRepository<IDirectoryScanFingerprintRepository>();

                    MediaLibraryScanCompositeId compositeKey = MediaLibraryScanCompositeId.Create(ScanId, UserId);

                    // get the library from the repository
                    Result<LibraryEntity?> getLibraryResult = await libraryRepository.GetByIdAsync(LibraryId.Value, cancellationToken).ConfigureAwait(false);
                    if (getLibraryResult.IsFailure || getLibraryResult.Value is null)
                        throw new InvalidOperationException(getLibraryResult.IsFailure ? getLibraryResult.FirstError.Description : "The media library was not found.");

                    // convert it to a domain object
                    Result<Library> domainLibraryResult = getLibraryResult.Value.ToDomainEntity();
                    if (domainLibraryResult.IsFailure)
                        throw new InvalidOperationException(domainLibraryResult.FirstError.Description);

                    // when the fast skip is enabled, load the directory scan fingerprints of the library, used to skip the directories that have not changed since the last scan
                    Dictionary<string, DirectoryScanFingerprintEntity>? fingerprintsByPath = null;
                    if (domainLibraryResult.Value.ShouldSkipUnchangedDirectoriesDuringScan)
                    {
                        Result<Dictionary<string, DirectoryScanFingerprintEntity>> getFingerprintsResult = await directoryScanFingerprintRepository.GetMappedByLibraryIdAsync(LibraryId.Value, cancellationToken).ConfigureAwait(false);
                        if (getFingerprintsResult.IsFailure)
                            throw new InvalidOperationException(getFingerprintsResult.FirstError.Description);
                        fingerprintsByPath = getFingerprintsResult.Value;
                    }

                    // set the initial progress of the scan job
                    Result<Success> publishJobProgressResult = await PublishJobProgressAsync(domainEventPublisher, compositeKey, 0, domainLibraryResult.Value.ContentLocations.Count, cancellationToken).ConfigureAwait(false);
                    if (publishJobProgressResult.IsFailure)
                        throw new InvalidOperationException(publishJobProgressResult.FirstError.Description);

                    TimeSpan heartbeatInterval = TimeSpan.FromSeconds(1);
                    DateTime lastHeartbeat = DateTime.UtcNow;
                    int processedContentLocations = 0;

                    List<LibraryScanStagingResultsEntity> stagingBatch = [];
                    List<DirectoryScanFingerprintEntity> fingerprintBatch = [];

                    // get the files for each of the media library content locations
                    foreach (FileSystemPathId contentLocation in domainLibraryResult.Value.ContentLocations)
                    {
                        cancellationToken.ThrowIfCancellationRequested();

                        await DiscoverContentLocationAsync(
                            contentLocation.Path,
                            fingerprintsByPath is not null,
                            fingerprintsByPath,
                            stagingBatch,
                            fingerprintBatch,
                            stagingResultsRepository,
                            directoryScanFingerprintRepository,
                            cancellationToken).ConfigureAwait(false);
                        processedContentLocations++;

                        // increment the number of processed elements progress in a high-frequency counter with low overhead check
                        DateTime now = DateTime.UtcNow;
                        if (now - lastHeartbeat >= heartbeatInterval)
                        {
                            publishJobProgressResult = await PublishJobProgressAsync(
                                domainEventPublisher, compositeKey, processedContentLocations, domainLibraryResult.Value.ContentLocations.Count, cancellationToken).ConfigureAwait(false);
                            if (publishJobProgressResult.IsFailure)
                                throw new InvalidOperationException(publishJobProgressResult.FirstError.Description);
                            lastHeartbeat = now;
                        }
                    }

                    // flush any remaining discovered files and directory scan fingerprints that did not reach the batch size
                    if (stagingBatch.Count > 0)
                    {
                        Result<Created> insertResult = await stagingResultsRepository.InsertRangeAsync(stagingBatch, cancellationToken).ConfigureAwait(false);
                        if (insertResult.IsFailure)
                            throw new InvalidOperationException(insertResult.FirstError.Description);
                    }
                    if (fingerprintBatch.Count > 0)
                    {
                        Result<Updated> upsertResult = await directoryScanFingerprintRepository.UpsertRangeAsync(fingerprintBatch, cancellationToken).ConfigureAwait(false);
                        if (upsertResult.IsFailure)
                            throw new InvalidOperationException(upsertResult.FirstError.Description);
                    }

                    // this job finished, increment the number of processed jobs progress
                    await domainEventPublisher.PublishAsync(new LibraryScanProgressChangedDomainEvent(Guid.NewGuid(), LibraryId, compositeKey, DateTime.UtcNow), cancellationToken).ConfigureAwait(false);
                    Status = LibraryScanJobStatus.Completed;

                    // call each linked child with the obtained payload
                    foreach (IMediaLibraryScanJob child in Children)
                        await child.ExecuteAsync(id, input, cancellationToken).ConfigureAwait(false);
                }, cancellationToken).ConfigureAwait(false);
            }
        }
        catch (OperationCanceledException)
        {
            Status = LibraryScanJobStatus.Canceled;
            throw;
        }
        catch (Exception exception)
        {
            Status = LibraryScanJobStatus.Failed;
            await ScanFailurePublisher.PublishAsync(_serviceScopeFactory, LibraryId, MediaLibraryScanCompositeId.Create(ScanId, UserId), exception, cancellationToken).ConfigureAwait(false);
        }
    }

    /// <summary>
    /// Discovers the book files of a content location of the media library, using a breadth-first traversal, and writes them in batches to the staging results.
    /// </summary>
    /// <param name="rootDirectoryPath">The path of the root directory from which to start collecting files.</param>
    /// <param name="skipUnchangedDirectories">Whether to skip the directories whose last write time has not changed since the last scan, or not.</param>
    /// <param name="fingerprintsByPath">The directory scan fingerprints of the library, mapped by the directory path. Can be <see langword="null"/> when the fast skip is disabled.</param>
    /// <param name="stagingBatch">The batch of discovered files that is written to the staging results when it reaches the batch size.</param>
    /// <param name="fingerprintBatch">The batch of directory scan fingerprints that is written to the storage medium when it reaches the batch size.</param>
    /// <param name="stagingResultsRepository">The repository used to write the discovered files to the staging results.</param>
    /// <param name="directoryScanFingerprintRepository">The repository used to write the directory scan fingerprints to the storage medium.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <remarks>
    /// See docs/technical/architecture/architecture-knowledge-management/architecture-decision-log/architecture-decision-record-0003.md for details.
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
    private async Task DiscoverContentLocationAsync(
        string rootDirectoryPath,
        bool skipUnchangedDirectories,
        Dictionary<string, DirectoryScanFingerprintEntity>? fingerprintsByPath,
        List<LibraryScanStagingResultsEntity> stagingBatch,
        List<DirectoryScanFingerprintEntity> fingerprintBatch,
        ILibraryScanStagingResultsRepository stagingResultsRepository,
        IDirectoryScanFingerprintRepository directoryScanFingerprintRepository,
        CancellationToken cancellationToken)
    {
        Queue<DirectoryInfo> directoryQueue = new();
        directoryQueue.Enqueue(new DirectoryInfo(rootDirectoryPath));

        // use a breadth-first traversal iterative approach, instead of recursion, which is heavier on call frames and memory
        // and could result in stack overflow on deeply nested directories
        while (directoryQueue.Count > 0)
        {
            cancellationToken.ThrowIfCancellationRequested();
            DirectoryInfo currentDirectory = directoryQueue.Dequeue();

            // when the fast skip is enabled, skip the whole subtree of a directory whose last write time has not changed since the last scan
            if (skipUnchangedDirectories)
            {
                DateTime currentLastWriteTimeUtc = currentDirectory.LastWriteTimeUtc;
                if (fingerprintsByPath is not null &&
                    fingerprintsByPath.TryGetValue(currentDirectory.FullName, out DirectoryScanFingerprintEntity? fingerprint) &&
                    fingerprint.LastWriteTimeUtc == currentLastWriteTimeUtc)
                    continue;

                fingerprintBatch.Add(new DirectoryScanFingerprintEntity()
                {
                    Id = Guid.NewGuid(),
                    LibraryId = LibraryId.Value,
                    Path = currentDirectory.FullName,
                    LastWriteTimeUtc = currentLastWriteTimeUtc
                });
                // flush the collected directory scan fingerprints in bounded batches, so that the memory usage stays proportional to the batch size, no matter how many directories the library has
                if (fingerprintBatch.Count >= STAGING_BATCH_SIZE)
                {
                    Result<Updated> upsertFingerprintsResult = await directoryScanFingerprintRepository.UpsertRangeAsync(fingerprintBatch, cancellationToken).ConfigureAwait(false);
                    if (upsertFingerprintsResult.IsFailure)
                        throw new InvalidOperationException(upsertFingerprintsResult.FirstError.Description);
                    fingerprintBatch.Clear();
                }
            }

            // process the files of the current directory
            IEnumerable<FileInfo>? files = null;
            try
            {
                files = currentDirectory.EnumerateFiles("*", s_enumerationOptions)
                    .Where(file => s_bookExtensions.Contains(file.Extension));
            }
            catch (IOException ex)
            {
                Debug.WriteLine(ex.Message);
            }

            if (files is not null)
            {
                try
                {
                    foreach (FileInfo file in files)
                    {
                        cancellationToken.ThrowIfCancellationRequested();
                        stagingBatch.Add(new LibraryScanStagingResultsEntity()
                        {
                            Id = Guid.NewGuid(),
                            LibraryScanId = ScanId.Value,
                            Path = file.FullName,
                            Size = file.Length,
                            Ticks = file.LastWriteTimeUtc.Ticks,
                            ContentHash = 0,
                            PreviousContentHash = 0,
                            NeedsRehash = true,
                            IsNew = true
                        });
                        if (stagingBatch.Count >= STAGING_BATCH_SIZE)
                        {
                            Result<Created> insertBatchResult = await stagingResultsRepository.InsertRangeAsync(stagingBatch, cancellationToken).ConfigureAwait(false);
                            if (insertBatchResult.IsFailure)
                                throw new InvalidOperationException(insertBatchResult.FirstError.Description);
                            stagingBatch.Clear();
                        }
                    }
                }
                catch (IOException ex)
                {
                    // a file might become inaccessible between enumeration and processing, so the remaining files of the directory are skipped
                    Debug.WriteLine(ex.Message);
                }
            }

            // process the subdirectories - each new subdirectory is added to the same stack queue, for later processing
            IEnumerable<DirectoryInfo>? subdirectories = null;
            try
            {
                subdirectories = currentDirectory.EnumerateDirectories("*", s_enumerationOptions);
            }
            catch (IOException ex)
            {
                Debug.WriteLine(ex.Message);
            }

            if (subdirectories is not null)
            {
                try
                {
                    foreach (DirectoryInfo subdirectory in subdirectories)
                    {
                        cancellationToken.ThrowIfCancellationRequested();
                        directoryQueue.Enqueue(subdirectory);
                    }
                }
                catch (IOException ex)
                {
                    Debug.WriteLine(ex.Message);
                }
            }
        }
    }

    /// <summary>
    /// Publishes a job progress update.
    /// </summary>
    /// <param name="domainEventPublisher">The service used to publish the progress update.</param>
    /// <param name="compositeKey">The composite unique identifier of a media library scan.</param>
    /// <param name="currentProgress">The current job progress.</param>
    /// <param name="totalProgress">The total job progress.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="Result{TValue}"/> representing either a successful operation, or an error.</returns>
    private async Task<Result<Success>> PublishJobProgressAsync(IDomainEventPublisher domainEventPublisher, MediaLibraryScanCompositeId compositeKey, int currentProgress, int totalProgress, CancellationToken cancellationToken)
    {
        Result<MediaLibraryScanJobProgress> scanJobProgressResult = MediaLibraryScanJobProgress.Create(currentProgress, totalProgress, "DiscoveringFiles");
        if (scanJobProgressResult.IsFailure)
            return scanJobProgressResult.Errors;

        await domainEventPublisher.PublishAsync(new LibraryScanJobProgressChangedDomainEvent(
            Guid.NewGuid(), LibraryId, compositeKey, scanJobProgressResult.Value, DateTime.UtcNow), cancellationToken).ConfigureAwait(false);

        return Result.Success;
    }
}
