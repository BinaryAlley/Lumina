#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.MediaLibrary.Management;
using Lumina.Application.Common.DataAccess.UoW;
using Lumina.Application.Common.Mapping.MediaLibrary.Management;
using Lumina.Domain.Common.Events;
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryAggregate;
using Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryAggregate.ValueObjects;
using Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryScanAggregate;
using Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryScanAggregate.ValueObjects;
using Lumina.Domain.Core.BoundedContexts.SchedulingBoundedContext.ScheduledJobAggregate;
using Lumina.Domain.Core.BoundedContexts.UserManagementBoundedContext.UserAggregate.ValueObjects;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Infrastructure.Core.Scheduling.Execution.TaskExecutors;

/// <summary>
/// Task executor that scans all the enabled media libraries, on behalf of the owner of the scheduled job.
/// </summary>
public class MediaLibraryScanTaskExecutor : IScheduledTaskExecutor
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IDomainEventPublisher _domainEventPublisher;
    private readonly ILogger<MediaLibraryScanTaskExecutor> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="MediaLibraryScanTaskExecutor"/> class.
    /// </summary>
    /// <param name="domainEventPublisher">The domain event publisher used to publish events.</param>
    /// <param name="logger">Injected service for logging.</param>
    /// <param name="unitOfWork">Injected unit of work for interacting with the data access layer repositories.</param>
    public MediaLibraryScanTaskExecutor(IDomainEventPublisher domainEventPublisher, ILogger<MediaLibraryScanTaskExecutor> logger, IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
        _domainEventPublisher = domainEventPublisher;
        _logger = logger;
    }

    /// <summary>
    /// Scans all the enabled media libraries, on behalf of the owner of the scheduled job.
    /// </summary>
    /// <param name="scheduledJob">The scheduled job whose task is executed.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="Result{TValue}"/> representing either a successful operation, or an error.</returns>
    public async Task<Result<Success>> ExecutePayloadAsync(ScheduledJob scheduledJob, CancellationToken cancellationToken)
    {
        Guid ownerUserId = scheduledJob.OwnerUserId.Value;

        // Get all the enabled and unlocked media libraries.
        Result<IEnumerable<LibraryEntity>> getLibrariesResult = await _unitOfWork.LibraryRepository.GetAllEnabledAndUnlockedAsync(cancellationToken).ConfigureAwait(false);
        if (getLibrariesResult.IsFailure)
            return getLibrariesResult.Errors;

        IEnumerable<Result<Library>> domainLibrariesResult = getLibrariesResult.Value.ToDomainEntities();
        List<IDomainEvent> domainEvents = [];

        // Start a scan for each media library that is not already being scanned.
        foreach (Result<Library> domainLibraryResult in domainLibrariesResult)
        {
            if (domainLibraryResult.IsFailure)
                return domainLibraryResult.Errors;

            // Get the past month's scans for this library.
            Result<IEnumerable<LibraryScanEntity>> pastLibraryScansResult = await _unitOfWork.LibraryScanRepository
                .GetPastMonthScansByLibraryIdAsync(domainLibraryResult.Value.Id.Value, cancellationToken).ConfigureAwait(false);
            if (pastLibraryScansResult.IsFailure)
                return pastLibraryScansResult.Errors;

            // Convert the repository scans history to domain objects.
            IEnumerable<Result<LibraryScan>> pastLibraryScansDomainResult = pastLibraryScansResult.Value.ToDomainEntities();
            foreach (Result<LibraryScan> pastLibraryScanDomainResult in pastLibraryScansDomainResult)
                if (pastLibraryScanDomainResult.IsFailure)
                    return pastLibraryScanDomainResult.Errors;

            // Create and queue the media library scan.
            Result<LibraryScan> libraryScanResult = LibraryScan.Create(
                LibraryId.Create(domainLibraryResult.Value.Id.Value),
                UserId.Create(ownerUserId),
                [.. pastLibraryScansDomainResult.Select(pastLibraryScanDomainResult => pastLibraryScanDomainResult.Value)]
            );
            if (libraryScanResult.IsFailure)
                return libraryScanResult.Errors;
            // A media library that is already being scanned is simply skipped, so the other libraries are still scanned.
            if (libraryScanResult.Value.QueueScan().IsFailure)
                continue;

            Result<Created> insertLibraryScanResult = await _unitOfWork.LibraryScanRepository.InsertAsync(
                libraryScanResult.Value.ToRepositoryEntity(), cancellationToken).ConfigureAwait(false);
            if (insertLibraryScanResult.IsFailure)
                return insertLibraryScanResult.Errors;

            domainEvents.AddRange(libraryScanResult.Value.GetDomainEvents());
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        // Publish the domain events of the queued media library scans.
        foreach (IDomainEvent domainEvent in domainEvents)
            await _domainEventPublisher.PublishAsync(domainEvent, cancellationToken).ConfigureAwait(false);

        _logger.LogInformation("Scanned all enabled media libraries on behalf of the scheduled job '{ScheduledJobName}'.", scheduledJob.Name);
        return Result.Success;
    }
}
