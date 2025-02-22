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
using Mediator;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
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
            parentsPayloadsExecuted++; // increment the number of parents that finished their execution and called this job
            // only execute this job's payload when it has no parents, or when all the parents finished their execution
            if (Parents.Count == 0 || parentsPayloadsExecuted == Parents.Count)
            {
                // this needs to be wrapped in a task because even though this job is processed in a "fire and forget" async manner, it still does synchronous
                // file system processing that takes time, and would block the processing of scan jobs in the in-memory queue 
                await Task.Run(async () =>
                {
                    Status = LibraryScanJobStatus.Running;
                    Console.WriteLine("started repo discovering");
                    // see docs/technical/achitecture/architecture-knowledge-management/architecture-decision-log/architecture-decission-record-0001.md for details:
                    await using AsyncServiceScope asyncServiceScope = _serviceScopeFactory.CreateAsyncScope();
                    IUnitOfWork? unitOfWork = asyncServiceScope.ServiceProvider.GetService<IUnitOfWork>();
                    IPublisher? publisher = asyncServiceScope.ServiceProvider.GetService<IPublisher>();
                    ILibraryRepository libraryRepository = unitOfWork!.GetRepository<ILibraryRepository>();
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

                    // set the initial progress of the scan job
                    ErrorOr<MediaLibraryScanJobProgress> scanJobProgressResult = MediaLibraryScanJobProgress.Create(0, domainLibraryResult.Value.ContentLocations.Count, "RetrievingPastScanData");
                    if (scanJobProgressResult.IsError)
                    {
                        await publisher!.Publish(new LibraryScanFailedDomainEvent(Guid.NewGuid(), LibraryId, compositeKey, DateTime.UtcNow), cancellationToken).ConfigureAwait(false);
                        return;
                    }

                    await publisher!.Publish(new LibraryScanJobProgressChangedDomainEvent(
                        Guid.NewGuid(), LibraryId, compositeKey, scanJobProgressResult.Value, DateTime.UtcNow), cancellationToken).ConfigureAwait(false);

                    List<int> ints = [1, 2, 3];
                    foreach (int nr in ints)
                    {
                        await Task.Delay(1000);
                        // increment the number of processed elements progress
                        scanJobProgressResult = MediaLibraryScanJobProgress.Create(
                            scanJobProgressResult.Value.CompletedItems + 1, domainLibraryResult.Value.ContentLocations.Count, "RetrievingPastScanData");
                        if (scanJobProgressResult.IsError)
                        {
                            await publisher!.Publish(
                                new LibraryScanFailedDomainEvent(Guid.NewGuid(), LibraryId, compositeKey, DateTime.UtcNow), cancellationToken).ConfigureAwait(false);
                            return;
                        }
                        await publisher!.Publish(new LibraryScanJobProgressChangedDomainEvent(
                            Guid.NewGuid(), LibraryId, compositeKey, scanJobProgressResult.Value, DateTime.UtcNow), cancellationToken).ConfigureAwait(false);
                    }
                    // this job finished, increment the number of processed jobs progress
                    await publisher!.Publish(new LibraryScanProgressChangedDomainEvent(
                        Guid.NewGuid(), LibraryId, compositeKey, DateTime.UtcNow), cancellationToken).ConfigureAwait(false);
                    Status = LibraryScanJobStatus.Completed;
                    Console.WriteLine("ended repo discovering");
                    // call each linked child with the obtained payload
                    foreach (IMediaLibraryScanJob children in Children)
                        await children.ExecuteAsync(id, ints, cancellationToken).ConfigureAwait(false);
                }, cancellationToken).ConfigureAwait(false);
            }
        }
        catch (OperationCanceledException)
        {
            Status = LibraryScanJobStatus.Canceled;
            throw;
        }
    }
}
