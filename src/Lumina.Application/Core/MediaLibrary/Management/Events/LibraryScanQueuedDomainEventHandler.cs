#region ========================================================================= USING =====================================================================================
using ErrorOr;
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
using Mediator;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Application.Core.MediaLibrary.Management.Events;

/// <summary>
/// Handler for the event raised when a media library scan is queued.
/// </summary>
public class LibraryScanQueuedDomainEventHandler : INotificationHandler<LibraryScanQueuedDomainEvent>
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
    public async ValueTask Handle(LibraryScanQueuedDomainEvent domainEvent, CancellationToken cancellationToken)
    {
        ILibraryScanRepository libraryScanRepository = _unitOfWork.GetRepository<ILibraryScanRepository>();
        
        // get the library scan that was queued, from the repository
        ErrorOr<LibraryScanEntity?> getLibraryScanResult = await libraryScanRepository.GetByIdAsync(domainEvent.ScanId.Value, cancellationToken).ConfigureAwait(false);
        if (getLibraryScanResult.IsError || getLibraryScanResult.Value is null)
            throw new EventualConsistencyException(getLibraryScanResult.FirstError, getLibraryScanResult.Errors);

        // convert the repository entity to a domain entity
        ErrorOr<LibraryScan> libraryScanDomainResult = getLibraryScanResult.Value.ToDomainEntity();
        if (libraryScanDomainResult.IsError)
            throw new EventualConsistencyException(libraryScanDomainResult.FirstError, libraryScanDomainResult.Errors);

        // start the media library scan
        ErrorOr<Success> startScanResult = await _mediaLibraryScanningService.StartScanAsync(
            libraryScanDomainResult.Value, getLibraryScanResult.Value.Library.LibraryType, getLibraryScanResult.Value.Library.DownloadMedatadaFromWeb, cancellationToken).ConfigureAwait(false);
        if (startScanResult.IsError)
            throw new EventualConsistencyException(startScanResult.FirstError, startScanResult.Errors);

        // queue any domain events
        foreach (IDomainEvent queuedDomainEvent in libraryScanDomainResult.Value.GetDomainEvents())
            _domainEventsQueue.Enqueue(queuedDomainEvent);
    }
}
