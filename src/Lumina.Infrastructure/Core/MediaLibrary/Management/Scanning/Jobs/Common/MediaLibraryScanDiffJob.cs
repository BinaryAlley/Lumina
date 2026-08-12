#region ========================================================================= USING =====================================================================================
using ErrorOr;
using Lumina.Application.Common.DataAccess.Repositories.MediaLibrary;
using Lumina.Application.Common.DataAccess.UoW;
using Lumina.Domain.SharedKernel.Common.Enums.MediaLibrary;
using Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryScanAggregate.Events;
using Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryScanAggregate.Services.Jobs;
using Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryScanAggregate.ValueObjects;
using Mediator;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Infrastructure.Core.MediaLibrary.Management.Scanning.Jobs.Common;

/// <summary>
/// Media library scan job for computing the differences between the files on disk and the media library scan snapshot of the previous scan.
/// </summary>
internal sealed class MediaLibraryScanDiffJob : MediaLibraryScanJob, IMediaLibraryScanDiffJob
{
    private readonly IServiceScopeFactory _serviceScopeFactory;

    /// <summary>
    /// Initializes a new instance of the <see cref="MediaLibraryScanDiffJob"/> class.
    /// </summary>
    /// <param name="serviceScopeFactory">
    /// Injected factory for creating scopes in which services are requested.
    /// See docs/technical/architecture/architecture-knowledge-management/architecture-decision-log/architecture-decision-record-0001.md for details.
    /// </param>
    public MediaLibraryScanDiffJob(IServiceScopeFactory serviceScopeFactory)
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

                    MediaLibraryScanCompositeId compositeKey = MediaLibraryScanCompositeId.Create(ScanId, UserId);

                    // set the initial progress of the scan job, it's a 1 step job - comparing the discovered files against the media library scan snapshot
                    ErrorOr<Success> publishJobProgressResult = await PublishJobProgress(publisher, compositeKey, 0, 1, cancellationToken).ConfigureAwait(false);
                    if (publishJobProgressResult.IsError)
                        throw new InvalidOperationException(publishJobProgressResult.FirstError.Description);

                    // mark the staging results against the media library scan snapshot of the previous scan
                    ErrorOr<Updated> markChangesResult = await stagingResultsRepository.MarkChangesAgainstSnapshotAsync(ScanId.Value, LibraryId.Value, cancellationToken).ConfigureAwait(false);
                    if (markChangesResult.IsError)
                        throw new InvalidOperationException(markChangesResult.FirstError.Description);

                    // increment the number of processed elements progress
                    publishJobProgressResult = await PublishJobProgress(publisher, compositeKey, 1, 1, cancellationToken).ConfigureAwait(false);
                    if (publishJobProgressResult.IsError)
                        throw new InvalidOperationException(publishJobProgressResult.FirstError.Description);

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
        ErrorOr<MediaLibraryScanJobProgress> scanJobProgressResult = MediaLibraryScanJobProgress.Create(currentProgress, totalProgress, "ComparingFileHashes");
        if (scanJobProgressResult.IsError)
            return scanJobProgressResult.Errors;

        await publisher.Publish(new LibraryScanJobProgressChangedDomainEvent(
            Guid.NewGuid(), LibraryId, compositeKey, scanJobProgressResult.Value, DateTime.UtcNow), cancellationToken).ConfigureAwait(false);

        return Result.Success;
    }
}
