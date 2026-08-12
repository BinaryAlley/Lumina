#region ========================================================================= USING =====================================================================================
using ErrorOr;
using Lumina.Application.Common.DataAccess.Repositories.MediaLibrary;
using Lumina.Application.Common.DataAccess.UoW;
using Lumina.Application.Common.Infrastructure.Security;
using Lumina.Domain.SharedKernel.Common.Enums.MediaLibrary;
using Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryScanAggregate.Events;
using Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryScanAggregate.Services.Jobs;
using Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryScanAggregate.ValueObjects;
using Mediator;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Lumina.Application.Common.Infrastructure.Models.DTO.MediaLibraryScanJobPayloads;
#endregion

namespace Lumina.Infrastructure.Core.MediaLibrary.Management.Scanning.Jobs.Common;

/// <summary>
/// Media library scan job for computing the content hashes of the files that changed since the previous scan.
/// </summary>
internal sealed class MediaLibraryScanHashJob : MediaLibraryScanJob, IMediaLibraryScanHashJob
{
    private const int HASHING_PAGE_SIZE = 1000; // the number of files that are hashed and written back in a single batch, keeping the peak memory bounded regardless of the library size
    private readonly IServiceScopeFactory _serviceScopeFactory;

    /// <summary>
    /// Initializes a new instance of the <see cref="MediaLibraryScanHashJob"/> class.
    /// </summary>
    /// <param name="serviceScopeFactory">
    /// Injected factory for creating scopes in which services are requested.
    /// See docs/technical/architecture/architecture-knowledge-management/architecture-decision-log/architecture-decision-record-0001.md for details.
    /// </param>
    public MediaLibraryScanHashJob(IServiceScopeFactory serviceScopeFactory)
    {
        _serviceScopeFactory = serviceScopeFactory;
    }

    /// <summary>
    /// Executes the payload of the media library scan job.
    /// </summary>
    /// <typeparam name="TInput">The type of the input parameter representing the data to be processed by this payload.</typeparam>
    /// <param name="id">The unique identifier of the media library scan job.</param>
    /// <param name="input">The input data to be processed by this payload.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
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
                // processing that takes time, and would block the processing of scan jobs in the in-memory queue
                await Task.Run(async () =>
                {
                    Status = LibraryScanJobStatus.Running;
                    // see docs/technical/architecture/architecture-knowledge-management/architecture-decision-log/architecture-decision-record-0001.md for details:
                    await using AsyncServiceScope asyncServiceScope = _serviceScopeFactory.CreateAsyncScope();
                    IUnitOfWork unitOfWork = asyncServiceScope.ServiceProvider.GetService<IUnitOfWork>()!;
                    IPublisher publisher = asyncServiceScope.ServiceProvider.GetService<IPublisher>()!;
                    ILibraryScanStagingResultsRepository stagingResultsRepository = unitOfWork.GetRepository<ILibraryScanStagingResultsRepository>();
                    IFileHashService fileHashService = asyncServiceScope.ServiceProvider.GetService<IFileHashService>()!;

                    MediaLibraryScanCompositeId compositeKey = MediaLibraryScanCompositeId.Create(ScanId, UserId);

                    // count the files that need hashing, for progress reporting purposes
                    ErrorOr<int> getFilesToHashCountResult = await stagingResultsRepository.GetFilesToHashCountAsync(ScanId.Value, cancellationToken).ConfigureAwait(false);
                    if (getFilesToHashCountResult.IsError)
                        throw new InvalidOperationException(getFilesToHashCountResult.FirstError.Description);
                    int totalFilesToHash = getFilesToHashCountResult.Value;

                    // set the initial progress of the scan job
                    ErrorOr<Success> publishJobProgressResult = await PublishJobProgress(publisher, compositeKey, 0, totalFilesToHash, cancellationToken).ConfigureAwait(false);
                    if (publishJobProgressResult.IsError)
                        throw new InvalidOperationException(publishJobProgressResult.FirstError.Description);

                    DateTime lastUpdateTime = DateTime.UtcNow;
                    int minUpdateIntervalMs = 100;
                    int processedFilesCount = 0;
                    string? lastPath = null;

                    // process the files that need hashing in pages, keeping the peak memory bounded regardless of the library size
                    while (true)
                    {
                        cancellationToken.ThrowIfCancellationRequested();

                        ErrorOr<IReadOnlyList<HashedFileSystemFileDto>> getFilesToHashPageResult = await stagingResultsRepository.GetFilesToHashPageAsync(ScanId.Value, lastPath, HASHING_PAGE_SIZE, cancellationToken).ConfigureAwait(false);
                        if (getFilesToHashPageResult.IsError)
                            throw new InvalidOperationException(getFilesToHashPageResult.FirstError.Description);
                        IReadOnlyList<HashedFileSystemFileDto> filesToHashPage = getFilesToHashPageResult.Value;
                        if (filesToHashPage.Count == 0)
                            break;

                        // hash the files of the current page in parallel, invoking the progress callback before each file
                        List<HashedFileSystemFileDto> hashedFiles = await fileHashService.HashFilesAsync(filesToHashPage, async () =>
                        {
                            // check if enough time has passed since last update
                            DateTime now = DateTime.UtcNow;
                            if ((now - lastUpdateTime).TotalMilliseconds >= minUpdateIntervalMs)
                            {
                                // increment the number of processed elements progress
                                publishJobProgressResult = await PublishJobProgress(publisher, compositeKey, Interlocked.Increment(ref processedFilesCount), totalFilesToHash, cancellationToken).ConfigureAwait(false);
                                if (publishJobProgressResult.IsError)
                                    throw new InvalidOperationException(publishJobProgressResult.FirstError.Description);
                                lastUpdateTime = now;
                            }
                        }, cancellationToken).ConfigureAwait(false);

                        // write the computed hashes back to the staging results
                        ErrorOr<Updated> updateHashesResult = await stagingResultsRepository.UpdateFileHashesAsync(ScanId.Value, hashedFiles, cancellationToken).ConfigureAwait(false);
                        if (updateHashesResult.IsError)
                            throw new InvalidOperationException(updateHashesResult.FirstError.Description);

                        lastPath = filesToHashPage[^1].Path;
                    }

                    // this job finished, increment the number of processed jobs progress
                    await publisher.Publish(new LibraryScanProgressChangedDomainEvent(Guid.NewGuid(), LibraryId, compositeKey, DateTime.UtcNow), cancellationToken).ConfigureAwait(false);
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
        ErrorOr<MediaLibraryScanJobProgress> scanJobProgressResult = MediaLibraryScanJobProgress.Create(currentProgress, totalProgress, "HashingFileContents");
        if (scanJobProgressResult.IsError)
            return scanJobProgressResult.Errors;

        await publisher.Publish(new LibraryScanJobProgressChangedDomainEvent(
            Guid.NewGuid(), LibraryId, compositeKey, scanJobProgressResult.Value, DateTime.UtcNow), cancellationToken).ConfigureAwait(false);

        return Result.Success;
    }
}
