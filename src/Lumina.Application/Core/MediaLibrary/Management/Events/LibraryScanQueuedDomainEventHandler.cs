#region ========================================================================= USING =====================================================================================
using Lumina.Domain.Common.Primitives;
using Lumina.Application.Common.DataAccess.Entities.MediaLibrary.Management;
using Lumina.Application.Common.DataAccess.Repositories.MediaLibrary;
using Lumina.Application.Common.DataAccess.UoW;
using Lumina.Application.Common.DomainEvents;
using Lumina.Application.Common.Infrastructure.Authentication;
using Lumina.Application.Common.Mapping.MediaLibrary.Management;
using Lumina.Domain.Common.Events;
using Lumina.Domain.Common.Exceptions;
using Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryScanAggregate;
using Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryScanAggregate.Events;
using Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryScanAggregate.Services;
using Lumina.Domain.Core.BoundedContexts.UserManagementBoundedContext.UserAggregate.ValueObjects;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Application.Core.MediaLibrary.Management.Events;

/// <summary>
/// Handler for the domain event raised when a media library scan is queued.
/// </summary>
public class LibraryScanQueuedDomainEventHandler : IDomainEventHandler<LibraryScanQueuedDomainEvent>
{
    private readonly IMediaLibraryScanningService _mediaLibraryScanningService;
    private readonly IDomainEventsQueue _domainEventsQueue;
    private readonly IUnitOfWork _unitOfWork;

    /// <summary>
    /// Initializes a new instance of the <see cref="LibraryScanQueuedDomainEventHandler"/> class.
    /// </summary>
    /// <param name="mediaLibraryScanningService">Injected service for scanning media libraries.</param>
    /// <param name="domainEventsQueue">Injected unit of work for interacting with the data access layer repositories.</param>
    /// <param name="domainEventsQueue">Injected service for the queue of domain events.</param>
    /// <param name="unitOfWork">Injected unit of work for interacting with the data access layer repositories.</param>
    public LibraryScanQueuedDomainEventHandler(IMediaLibraryScanningService mediaLibraryScanningService, IDomainEventsQueue domainEventsQueue, IUnitOfWork unitOfWork)
    {
        _mediaLibraryScanningService = mediaLibraryScanningService;
        _domainEventsQueue = domainEventsQueue;
        _unitOfWork = unitOfWork;
    }

    /// <summary>
    /// Handles the event raised when a media library scan is queued.
    /// </summary>
    /// <param name="domainEvent">The domain event to be handled.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    public async ValueTask HandleAsync(LibraryScanQueuedDomainEvent domainEvent, CancellationToken cancellationToken)
    {
        // get the library scan that was queued, from the repository
        Result<LibraryScanEntity?> getLibraryScanResult = await _unitOfWork.LibraryScanRepository.GetByIdAsync(domainEvent.ScanId.Value, cancellationToken).ConfigureAwait(false);
        if (getLibraryScanResult.IsFailure || getLibraryScanResult.Value is null)
            throw new EventualConsistencyException(getLibraryScanResult.FirstError, getLibraryScanResult.Errors);

        // convert the repository entity to a domain entity
        Result<LibraryScan> libraryScanDomainResult = getLibraryScanResult.Value.ToDomainEntity();
        if (libraryScanDomainResult.IsFailure)
            throw new EventualConsistencyException(libraryScanDomainResult.FirstError, libraryScanDomainResult.Errors);

        // start the media library scan
        Result<Success> startScanResult = await _mediaLibraryScanningService.StartScanAsync(
            libraryScanDomainResult.Value, getLibraryScanResult.Value.Library.LibraryType, getLibraryScanResult.Value.Library.DownloadMetadataFromWeb, cancellationToken).ConfigureAwait(false);
        if (startScanResult.IsFailure)
            throw new EventualConsistencyException(startScanResult.FirstError, startScanResult.Errors);

        // queue any domain events
        foreach (IDomainEvent queuedDomainEvent in libraryScanDomainResult.Value.GetDomainEvents())
            _domainEventsQueue.Enqueue(queuedDomainEvent);
    }
}
