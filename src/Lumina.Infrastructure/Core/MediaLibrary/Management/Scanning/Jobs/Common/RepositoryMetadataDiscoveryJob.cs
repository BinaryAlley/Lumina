#region ========================================================================= USING =====================================================================================
using ErrorOr;
using Lumina.Application.Common.DataAccess.Entities.MediaLibrary.Management;
using Lumina.Application.Common.DataAccess.Repositories.MediaLibrary;
using Lumina.Application.Common.DataAccess.UoW;
using Lumina.Application.Common.Mapping.MediaLibrary.Management;
using Lumina.Domain.Common.Enums.MediaLibrary;
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
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Infrastructure.Core.MediaLibrary.Management.Scanning.Jobs.Common;

/// <summary>
/// Media library scan job for retrieving persistence data from any eventual previous scans.
/// </summary>
internal sealed class RepositoryMetadataDiscoveryJob : MediaLibraryScanJob, IRepositoryMetadataDiscoveryJob
{
    private readonly IServiceScopeFactory _serviceScopeFactory;

    /// <summary>
    /// Initializes a new instance of the <see cref="RepositoryMetadataDiscoveryJob"/> class.
    /// </summary>
    /// <param name="serviceScopeFactory">
    /// Injected factory for creating scopes in which services are requested. 
    /// See docs/technical/achitecture/architecture-knowledge-management/architecture-decision-log/architecture-decission-record-0001.md for details.
    /// </param>
    public RepositoryMetadataDiscoveryJob(IServiceScopeFactory serviceScopeFactory)
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
                // processing that takes time, and would block the processing of scan jobs in the in-memory queue 
                await Task.Run(async () =>
                {
                    Status = LibraryScanJobStatus.Running;
                    Stopwatch sw = Stopwatch.StartNew();
                    Console.WriteLine("Started repo discovering...");
                    // see docs/technical/achitecture/architecture-knowledge-management/architecture-decision-log/architecture-decission-record-0001.md for details:
                    await using AsyncServiceScope asyncServiceScope = _serviceScopeFactory.CreateAsyncScope();
                    IUnitOfWork unitOfWork = asyncServiceScope.ServiceProvider.GetService<IUnitOfWork>()!;
                    IPublisher publisher = asyncServiceScope.ServiceProvider.GetService<IPublisher>()!;
                    ILibraryRepository libraryRepository = unitOfWork.GetRepository<ILibraryRepository>();
                    ILibraryScanResultRepository libraryScanResultRepository = unitOfWork.GetRepository<ILibraryScanResultRepository>();

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

                    // set the initial progress of the scan job, it's a 1 step job - retrieving stuff from repository
                    ErrorOr<Success> publishJobProgressResult = await PublishJobProgress(publisher, compositeKey, 0, 1, cancellationToken).ConfigureAwait(false);
                    if (publishJobProgressResult.IsError)
                        return;

                    // get the results of the last scan of the media library
                    ErrorOr<Dictionary<string, LibraryScanResultEntity>> getScanResultsResult = 
                        await libraryScanResultRepository.GetPathMappedByLibraryIdAsync(LibraryId.Value, cancellationToken).ConfigureAwait(false);
                    if (getScanResultsResult.IsError)
                        return;

                    // increment the number of processed elements progress
                    publishJobProgressResult = await PublishJobProgress(publisher, compositeKey, 1, 1, cancellationToken).ConfigureAwait(false);
                    if (publishJobProgressResult.IsError)
                        return;

                    // this job finished, increment the number of processed jobs progress
                    await publisher.Publish(new LibraryScanProgressChangedDomainEvent(
                        Guid.NewGuid(), LibraryId, compositeKey, DateTime.UtcNow), cancellationToken).ConfigureAwait(false);
                    Status = LibraryScanJobStatus.Completed;
                    sw.Stop();
                    Console.WriteLine($"Ended repository discovery scan job in {sw.ElapsedMilliseconds}ms, discovered {getScanResultsResult.Value.Count} items.");
                    // call each linked child with the obtained payload
                    foreach (IMediaLibraryScanJob child in Children)
                        await child.ExecuteAsync(id, getScanResultsResult.Value, cancellationToken).ConfigureAwait(false);
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
        ErrorOr<MediaLibraryScanJobProgress> scanJobProgressResult = MediaLibraryScanJobProgress.Create(currentProgress, totalProgress, "RetrievingPastScanData");
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
