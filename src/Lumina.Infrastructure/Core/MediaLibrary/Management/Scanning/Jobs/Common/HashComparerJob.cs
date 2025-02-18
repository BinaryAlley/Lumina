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
/// Media library scan job for comparing hashes.
/// </summary>
internal sealed class HashComparerJob : MediaLibraryScanJob, IHashComparerJob
{
    private readonly IServiceScopeFactory _serviceScopeFactory;

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
            parentsPayloadsExecuted++; // increment the number of parents that finished their execution and called this job
            // only execute this job's payload when it has no parents, or when all the parents finished their execution
            if (Parents.Count == 0 || parentsPayloadsExecuted == Parents.Count)
            {
                Status = LibraryScanJobStatus.Running;
                Console.WriteLine("started file hashing");
                // see docs/technical/achitecture/architecture-knowledge-management/architecture-decision-log/architecture-decission-record-0001.md for details:
                await using AsyncServiceScope asyncServiceScope = _serviceScopeFactory.CreateAsyncScope();
                IUnitOfWork? unitOfWork = asyncServiceScope.ServiceProvider.GetService<IUnitOfWork>();
                IPublisher? publisher = asyncServiceScope.ServiceProvider.GetService<IPublisher>();
                ILibraryRepository libraryRepository = unitOfWork!.GetRepository<ILibraryRepository>();

                // get the library from the repository
                ErrorOr<LibraryEntity?> getLibraryResult = await libraryRepository.GetByIdAsync(LibraryId.Value, cancellationToken).ConfigureAwait(false);
                if (getLibraryResult.IsError || getLibraryResult.Value is null)
                {
                    await publisher!.Publish(new LibraryScanFailedDomainEvent(Guid.NewGuid(), ScanId, LibraryId, DateTime.UtcNow), cancellationToken).ConfigureAwait(false);
                    return;
                }

                // convert it to a domain object
                ErrorOr<Library> domainLibraryResult = getLibraryResult.Value.ToDomainEntity();
                if (domainLibraryResult.IsError)
                {
                    await publisher!.Publish(new LibraryScanFailedDomainEvent(Guid.NewGuid(), ScanId, LibraryId, DateTime.UtcNow), cancellationToken).ConfigureAwait(false);
                    return;
                }

                // set the initial progress of the scan job
                ErrorOr<MediaLibraryScanJobProgress> scanJobProgressResult = MediaLibraryScanJobProgress.Create(0, domainLibraryResult.Value.ContentLocations.Count, "ComparingFileHashes");
                if (scanJobProgressResult.IsError)
                {
                    await publisher!.Publish(new LibraryScanFailedDomainEvent(Guid.NewGuid(), ScanId, LibraryId, DateTime.UtcNow), cancellationToken).ConfigureAwait(false);
                    return;
                }

                await publisher!.Publish(new LibraryScanJobProgressChangedDomainEvent(
                    Guid.NewGuid(), ScanId, UserId, scanJobProgressResult.Value, DateTime.UtcNow), cancellationToken).ConfigureAwait(false);

                List<int> ints = [1, 2, 3];
                foreach (int nr in ints)
                {
                    await Task.Delay(5400);
                    // increment the number of processed elements progress
                    scanJobProgressResult = MediaLibraryScanJobProgress.Create(
                        scanJobProgressResult.Value.CompletedItems + 1, domainLibraryResult.Value.ContentLocations.Count, "ComparingFileHashes");
                    if (scanJobProgressResult.IsError)
                    {
                        await publisher!.Publish(new LibraryScanFailedDomainEvent(Guid.NewGuid(), ScanId, LibraryId, DateTime.UtcNow), cancellationToken).ConfigureAwait(false);
                        return;
                    }
                    await publisher!.Publish(new LibraryScanJobProgressChangedDomainEvent(
                        Guid.NewGuid(), ScanId, UserId, scanJobProgressResult.Value, DateTime.UtcNow), cancellationToken).ConfigureAwait(false);
                }
                // this job finished, increment the number of processed jobs progress
                await publisher!.Publish(new LibraryScanProgressChangedDomainEvent(Guid.NewGuid(), ScanId, UserId, DateTime.UtcNow), cancellationToken).ConfigureAwait(false);
                Status = LibraryScanJobStatus.Completed;
                Console.WriteLine("ended file hashing");
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
