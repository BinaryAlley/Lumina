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
using Lumina.Domain.Core.BoundedContexts.WrittenContentLibraryBoundedContext.BookLibraryAggregate.Services.Jobs;
using Mediator;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Infrastructure.Core.MediaLibrary.Management.Scanning.Jobs.WrittenContent.Books;

/// <summary>
/// Media library scan job for retrieving written content metadata from GoodReads.
/// </summary>
internal sealed class GoodReadsMetadataScrapJob : MediaLibraryScanJob, IGoodReadsMetadataScrapJob
{
    private readonly IServiceScopeFactory _serviceScopeFactory;

    /// <summary>
    /// Initializes a new instance of the <see cref="GoodReadsMetadataScrapJob"/> class.
    /// </summary>
    /// <param name="serviceScopeFactory">
    /// Injected factory for creating scopes in which services are requested. 
    /// See docs/technical/achitecture/architecture-knowledge-management/architecture-decision-log/architecture-decission-record-0001.md for details.
    /// </param>
    public GoodReadsMetadataScrapJob(IServiceScopeFactory serviceScopeFactory)
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
                Status = LibraryScanJobStatus.Running;
                Console.WriteLine("started goodreads metadata");
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
                ErrorOr<MediaLibraryScanJobProgress> scanJobProgressResult = MediaLibraryScanJobProgress.Create(0, 20, "GoodReadsMetadataDownload");
                //ErrorOr<MediaLibraryScanJobProgress> scanJobProgressResult = MediaLibraryScanJobProgress.Create(0, domainLibraryResult.Value.ContentLocations.Count, "GoodReadsMetadataDownload");
                if (scanJobProgressResult.IsError)
                {
                    await publisher!.Publish(new LibraryScanFailedDomainEvent(Guid.NewGuid(), LibraryId, compositeKey, DateTime.UtcNow), cancellationToken).ConfigureAwait(false);
                    return;
                }

                await publisher!.Publish(new LibraryScanJobProgressChangedDomainEvent(
                    Guid.NewGuid(), LibraryId, compositeKey, scanJobProgressResult.Value, DateTime.UtcNow), cancellationToken).ConfigureAwait(false);

                List<int> ints = Enumerable.Range(0, 20).ToList();
                foreach (int nr in ints)
                {
                    await Task.Delay(100);
                    // increment the number of processed elements progress
                    scanJobProgressResult = MediaLibraryScanJobProgress.Create(
                        nr, 20, "GoodReadsMetadataDownload");
                        //scanJobProgressResult.Value.CompletedItems + 1, domainLibraryResult.Value.ContentLocations.Count, "GoodReadsMetadataDownload");
                    if (scanJobProgressResult.IsError)
                    {
                        await publisher!.Publish(new LibraryScanFailedDomainEvent(Guid.NewGuid(), LibraryId, compositeKey, DateTime.UtcNow), cancellationToken).ConfigureAwait(false);
                        return;
                    }
                    await publisher!.Publish(new LibraryScanJobProgressChangedDomainEvent(
                        Guid.NewGuid(), LibraryId, compositeKey, scanJobProgressResult.Value, DateTime.UtcNow), cancellationToken).ConfigureAwait(false);
                }
                // this job finished, increment the number of processed jobs progress
                await publisher!.Publish(new LibraryScanProgressChangedDomainEvent(
                    Guid.NewGuid(), LibraryId, compositeKey, DateTime.UtcNow), cancellationToken).ConfigureAwait(false);
                Status = LibraryScanJobStatus.Completed;
                Console.WriteLine("ended goodreads metadata");
                // call each linked child with the obtained payload
                foreach (IMediaLibraryScanJob children in Children)
                    await children.ExecuteAsync(id, ints, cancellationToken).ConfigureAwait(false);
            }
        }
        catch (OperationCanceledException)
        {
            Status = LibraryScanJobStatus.Canceled;
            throw;
        }
    }
}
