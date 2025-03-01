#region ========================================================================= USING =====================================================================================
using ErrorOr;
using Lumina.Application.Common.DataAccess.Entities.MediaLibrary.Management;
using Lumina.Application.Common.DataAccess.Repositories.MediaLibrary;
using Lumina.Application.Common.DataAccess.UoW;
using Lumina.Application.Common.Infrastructure.Models.MediaLibraryScanJobPayloads;
using Lumina.Application.Common.Infrastructure.Security;
using Lumina.Application.Common.Mapping.MediaLibrary.Management;
using Lumina.Domain.Common.Enums.MediaLibrary;
using Lumina.Domain.Core.BoundedContexts.FileSystemManagementBoundedContext.FileSystemManagementAggregate.Services;
using Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryAggregate;
using Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryScanAggregate.Events;
using Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryScanAggregate.Services.Jobs;
using Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryScanAggregate.ValueObjects;
using Lumina.Infrastructure.Common.Errors;
using Mediator;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Infrastructure.Core.MediaLibrary.Management.Scanning.Jobs.Common;

/// <summary>
/// Media library scan job for comparing hashes.
/// </summary>
internal sealed class HashComparerJob : MediaLibraryScanJob, IHashComparerJob
{
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private Dictionary<string, LibraryScanResultEntity> libraryScanResultEntities = [];
    private List<FileInfo> files = [];

    /// <summary>
    /// Initializes a new instance of the <see cref="HashComparerJob"/> class.
    /// </summary>
    /// <param name="serviceScopeFactory">
    /// Injected factory for creating scopes in which services are requested. 
    /// See docs/technical/achitecture/architecture-knowledge-management/architecture-decision-log/architecture-decission-record-0001.md for details.
    /// </param>
    public HashComparerJob(IServiceScopeFactory serviceScopeFactory)
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
            
            // store the incoming payloads as they arrive from parent jobs
            if (input is Dictionary<string, LibraryScanResultEntity> libraryScanResultsPayload)
                libraryScanResultEntities = libraryScanResultsPayload;
            else if (input is List<FileInfo> filesPayload)
                files = filesPayload;
            
            // only execute this job's payload when it has no parents, or when all the parents finished their execution
            if (Parents.Count == 0 || parentsCompleted == Parents.Count)
            {
                // this needs to be wrapped in a task because even though this job is processed in a "fire and forget" async manner, it still does synchronous
                // processing that takes time, and would block the processing of scan jobs in the in-memory queue 
                await Task.Run(async () =>
                {
                    Status = LibraryScanJobStatus.Running;
                    Stopwatch sw = Stopwatch.StartNew();
                    Console.WriteLine("Started file hashing...");
                    // see docs/technical/achitecture/architecture-knowledge-management/architecture-decision-log/architecture-decission-record-0001.md for details:
                    await using AsyncServiceScope asyncServiceScope = _serviceScopeFactory.CreateAsyncScope();
                    IUnitOfWork unitOfWork = asyncServiceScope.ServiceProvider.GetService<IUnitOfWork>()!;
                    IPublisher publisher = asyncServiceScope.ServiceProvider.GetService<IPublisher>()!;
                    ILibraryRepository libraryRepository = unitOfWork!.GetRepository<ILibraryRepository>();
                    IFileService fileService = asyncServiceScope.ServiceProvider.GetService<IFileService>()!;
                    IFileHashService fileHashService = asyncServiceScope.ServiceProvider.GetService<IFileHashService>()!;
                    MediaLibraryScanCompositeId compositeKey = MediaLibraryScanCompositeId.Create(ScanId, UserId);

                    // get the library from the repository
                    ErrorOr<LibraryEntity?> getLibraryResult = await libraryRepository.GetByIdAsync(LibraryId.Value, cancellationToken).ConfigureAwait(false);
                    if (getLibraryResult.IsError || getLibraryResult.Value is null)
                    {
                        await publisher!.Publish(new LibraryScanFailedDomainEvent(Guid.NewGuid(), LibraryId, compositeKey, DateTime.UtcNow), cancellationToken).ConfigureAwait(false);
                        return;
                    }

                    // convert it to a domain object
                    ErrorOr<Library> domainLibraryResult = getLibraryResult.Value.ToDomainEntity();
                    if (domainLibraryResult.IsError)
                    {
                        await publisher!.Publish(new LibraryScanFailedDomainEvent(Guid.NewGuid(), LibraryId, compositeKey, DateTime.UtcNow), cancellationToken).ConfigureAwait(false);
                        return;
                    }

                    // set the initial progress of the scan job; total progress is going twice through the list of files, once to determine what needs to be hashed, second to actually hash
                    ErrorOr<Success> publishJobProgressResult = await PublishJobProgress(publisher, compositeKey, 0, files.Count * 2, cancellationToken).ConfigureAwait(false);
                    if (publishJobProgressResult.IsError)
                        return;

                    DateTime lastUpdateTime = DateTime.UtcNow;
                    int minUpdateIntervalMs = 100;
                    // pre-allocate with worst case scenario, to avoid memory copying and garbage collection
                    List<FileInfo> filesToHash = new(files.Count);

                    for (int i = 0; i < files.Count; i++)
                    {
                        cancellationToken.ThrowIfCancellationRequested();

                        FileInfo file = files[i];
                        // fastest path - file does not exist in past scan, so it's a new file
                        if (!libraryScanResultEntities.TryGetValue(file.FullName, out LibraryScanResultEntity? previousScanFile))
                            filesToHash.Add(file);
                        else if (file.Length != previousScanFile.FileSize) // next best comparison, check if file size or the last modified date changed
                            filesToHash.Add(file); // definitely changed
                        else if (file.LastWriteTimeUtc != previousScanFile.LastModified) // potential change - requires hash verification
                            filesToHash.Add(file);

                        // remove any found items, to speed up subsequent lookups (if needed, all the remaining entries in this dictionary at the end of the scan are deleted filed)
                        libraryScanResultEntities.Remove(file.FullName);

                        // check if enough time has passed since last update
                        DateTime now = DateTime.UtcNow;
                        if ((now - lastUpdateTime).TotalMilliseconds >= minUpdateIntervalMs)
                        {
                            // increment the number of processed elements progress
                            publishJobProgressResult = await PublishJobProgress(publisher, compositeKey, i, files.Count, cancellationToken).ConfigureAwait(false);
                            if (publishJobProgressResult.IsError)
                                return;

                            // update the last update time
                            lastUpdateTime = now;
                        }
                    }
                    int processedFilesCount = 0;
                    List<ChangedFileSystemFile> processedFiles = await fileHashService.HashFilesAsync(filesToHash, libraryScanResultEntities,
                        async () =>
                        {
                            // check if enough time has passed since last update
                            DateTime now = DateTime.UtcNow;
                            if ((now - lastUpdateTime).TotalMilliseconds >= minUpdateIntervalMs)
                            {
                                // increment the number of processed elements progress
                                publishJobProgressResult = await PublishJobProgress(
                                    publisher, compositeKey, Interlocked.Increment(ref processedFilesCount), files.Count, cancellationToken).ConfigureAwait(false);
                                if (publishJobProgressResult.IsError)
                                    return;
                                    // update the last update time
                                lastUpdateTime = now;
                            }
                        }, cancellationToken);

                    // this job finished, increment the number of processed jobs progress
                    await publisher!.Publish(new LibraryScanProgressChangedDomainEvent(Guid.NewGuid(), LibraryId, compositeKey, DateTime.UtcNow), cancellationToken).ConfigureAwait(false);
                    Status = LibraryScanJobStatus.Completed;
                    sw.Stop();
                    Console.WriteLine($"Ended file hashing in {sw.ElapsedMilliseconds}ms, hashed {processedFiles.Count} files.");

                    // call each linked child with the obtained payload
                    foreach (IMediaLibraryScanJob child in Children)
                        await child.ExecuteAsync(id, processedFiles, cancellationToken).ConfigureAwait(false);
                }, cancellationToken).ConfigureAwait(false);
            }
        }
        catch (OperationCanceledException)
        {
            Status = LibraryScanJobStatus.Canceled;
            throw;
        }
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
        ErrorOr<MediaLibraryScanJobProgress> scanJobProgressResult = MediaLibraryScanJobProgress.Create(currentProgress, totalProgress, "ComparingFileHashes");
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
