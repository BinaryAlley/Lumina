#region ========================================================================= USING =====================================================================================
using ErrorOr;
using Lumina.Application.Common.DataAccess.Entities.MediaLibrary.Management;
using Lumina.Application.Common.DataAccess.Repositories.MediaLibrary;
using Lumina.Application.Common.DataAccess.UoW;
using Lumina.Application.Common.Mapping.MediaLibrary.Management;
using Lumina.Domain.Common.Enums.MediaLibrary;
using Lumina.Domain.Core.BoundedContexts.FileSystemManagementBoundedContext.FileSystemManagementAggregate.Services;
using Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryAggregate;
using Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryScanAggregate.Events;
using Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryScanAggregate.Services.Jobs;
using Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryScanAggregate.ValueObjects;
using Lumina.Domain.Core.BoundedContexts.WrittenContentLibraryBoundedContext.BookLibraryAggregate.Services.Jobs;
using Lumina.Infrastructure.Common.Errors;
using Mediator;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Infrastructure.Core.MediaLibrary.Management.Scanning.Jobs.WrittenContent.Books;

/// <summary>
/// Media library scan job for filtering only files that have known book formats extensions.
/// </summary>
internal sealed class BooksFileExtensionsFilterJob : MediaLibraryScanJob, IBooksFileExtensionsFilterJob
{
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly string[] _knownBookFileExtensions =
    [
        ".pdf", ".epub", ".mobi", ".azw", ".azw3",
        ".cbz", ".cbr", ".djvu", ".fb2", ".lit",
        ".prc", ".txt", ".doc", ".docx", ".rtf"
    ];

    /// <summary>
    /// Initializes a new instance of the <see cref="BooksFileExtensionsFilterJob"/> class.
    /// </summary>
    /// <param name="serviceScopeFactory">
    /// Injected factory for creating scopes in which services are requested. 
    /// See docs/technical/achitecture/architecture-knowledge-management/architecture-decision-log/architecture-decission-record-0001.md for details.
    /// </param>
    public BooksFileExtensionsFilterJob(IServiceScopeFactory serviceScopeFactory)
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
                if (input is List<FileInfo> payload)
                {
                    // this needs to be wrapped in a task because even though this job is processed in a "fire and forget" async manner, it still does synchronous
                    // processing that takes time, and would block the processing of scan jobs in the in-memory queue 
                    await Task.Run(async () =>
                    {
                        Status = LibraryScanJobStatus.Running;
                        Stopwatch sw = Stopwatch.StartNew();
                        Console.WriteLine("Started book file filtering...");
                        // see docs/technical/achitecture/architecture-knowledge-management/architecture-decision-log/architecture-decission-record-0001.md for details:
                        await using AsyncServiceScope asyncServiceScope = _serviceScopeFactory.CreateAsyncScope();
                        IUnitOfWork unitOfWork = asyncServiceScope.ServiceProvider.GetService<IUnitOfWork>()!;
                        IPublisher publisher = asyncServiceScope.ServiceProvider.GetService<IPublisher>()!;
                        ILibraryRepository libraryRepository = unitOfWork.GetRepository<ILibraryRepository>();
                        IPathService pathService = asyncServiceScope.ServiceProvider.GetService<IPathService>()!;
                        MediaLibraryScanCompositeId compositeKey = MediaLibraryScanCompositeId.Create(ScanId, UserId);

                        // get the library from the repository
                        ErrorOr<LibraryEntity?> getLibraryResult = await libraryRepository.GetByIdAsync(LibraryId.Value, cancellationToken).ConfigureAwait(false);
                        if (getLibraryResult.IsError || getLibraryResult.Value is null)
                        {
                            await publisher.Publish(new LibraryScanFailedDomainEvent(Guid.NewGuid(), LibraryId, compositeKey, DateTime.UtcNow), cancellationToken).ConfigureAwait(false);
                            return;
                        }

                        // convert it to a domain object
                        ErrorOr<Library> domainLibraryResult = getLibraryResult.Value.ToDomainEntity();
                        if (domainLibraryResult.IsError)
                        {
                            await publisher.Publish(new LibraryScanFailedDomainEvent(Guid.NewGuid(), LibraryId, compositeKey, DateTime.UtcNow), cancellationToken).ConfigureAwait(false);
                            return;
                        }

                        // set the initial progress of the scan job
                        ErrorOr<Success> publishJobProgressResult = await PublishJobProgress(publisher, compositeKey, 0, payload.Count, cancellationToken).ConfigureAwait(false);
                        if (publishJobProgressResult.IsError)
                            return;
                        
                        DateTime lastUpdateTime = DateTime.UtcNow;
                        int minUpdateIntervalMs = 100;

                        List<FileInfo> bookFiles = new(payload.Count / 2); // pre-allocate with estimated capacity (boy, that's an optimistic view on books...)
                        for (int i = 0; i < payload.Count; i++)
                        {
                            cancellationToken.ThrowIfCancellationRequested();

                            FileInfo file = payload[i];

                            if (HasKnownBookExtension(file.Name.AsSpan()))
                                bookFiles.Add(file);
                            // check if enough time has passed since last update
                            DateTime now = DateTime.UtcNow;
                            if ((now - lastUpdateTime).TotalMilliseconds >= minUpdateIntervalMs)
                            {
                                // update job progress report
                                publishJobProgressResult = await PublishJobProgress(publisher, compositeKey, i, payload.Count, cancellationToken).ConfigureAwait(false);
                                if (publishJobProgressResult.IsError)
                                    return;
                                // update the last update time
                                lastUpdateTime = now;
                            }
                        }
                        
                        // this job finished, increment the number of processed jobs progress
                        await publisher.Publish(new LibraryScanProgressChangedDomainEvent(
                            Guid.NewGuid(), LibraryId, compositeKey, DateTime.UtcNow), cancellationToken).ConfigureAwait(false);
                        sw.Stop();
                        Console.WriteLine($"Ended book file filtering in {sw.ElapsedMilliseconds}ms, filtered {bookFiles.Count} files.");
                        Status = LibraryScanJobStatus.Completed;

                        // call each linked child with the obtained payload
                        foreach (IMediaLibraryScanJob child in Children)
                            await child.ExecuteAsync(id, bookFiles, cancellationToken).ConfigureAwait(false);
                    }, cancellationToken).ConfigureAwait(false);
                }
            }
        }
        catch (OperationCanceledException)
        {
            Status = LibraryScanJobStatus.Canceled;
            throw;
        }
    }

    /// <summary>
    /// Checks if <paramref name="filePath"/> has a known book file extension or not.
    /// </summary>
    /// <param name="filePath">The file path to be checked.</param>
    /// <returns><see langword="true"/> if <paramref name="filePath"/> contains a known book file extension, <see langword="false"/> otherwise.</returns>
    public bool HasKnownBookExtension(ReadOnlySpan<char> filePath)
    {
        // find the last dot in the path
        int lastDotIndex = filePath.LastIndexOf('.');

        // if no dot found or it's the last character, return false
        if (lastDotIndex == -1)
            return false;

        // get the extension part (including the dot)
        ReadOnlySpan<char> extension = filePath[lastDotIndex..];

        // check against each known extension
        foreach (string knownExt in _knownBookFileExtensions)
            if (extension.SequenceEqual(knownExt.AsSpan()))
                return true;

        return false;
    }

    /// <summary>
    /// Publishes a job progress update.
    /// </summary>
    /// <param name="publisher">The service used to publish the progress update.</param>
    /// <param name="compositeKey">The composite unique identifier of a media library scan.</param>
    /// <param name="currentProgress">The current job progress.</param>
    /// <param name="totalProgress">The total job progress.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="ErrorOr{TValue}"/> representing either a successful operation, or an error.</returns>
    private async Task<ErrorOr<Success>> PublishJobProgress(IPublisher publisher, MediaLibraryScanCompositeId compositeKey, int currentProgress, int totalProgress, CancellationToken cancellationToken)
    {
        ErrorOr<MediaLibraryScanJobProgress> scanJobProgressResult = MediaLibraryScanJobProgress.Create(currentProgress, totalProgress, "FilteringBookFiles");
        if (scanJobProgressResult.IsError)
        {
            await publisher.Publish(new LibraryScanFailedDomainEvent(Guid.NewGuid(), LibraryId, compositeKey, DateTime.UtcNow), cancellationToken).ConfigureAwait(false);
            return Errors.LibraryScanning.FailedToCreateScanJobProgress;
        }

        await publisher.Publish(new LibraryScanJobProgressChangedDomainEvent(
            Guid.NewGuid(), LibraryId, compositeKey, scanJobProgressResult.Value, DateTime.UtcNow), cancellationToken).ConfigureAwait(false);

        return Result.Success;
    }
}
