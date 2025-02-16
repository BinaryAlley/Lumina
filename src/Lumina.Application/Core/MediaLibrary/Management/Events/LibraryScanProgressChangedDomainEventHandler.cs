#region ========================================================================= USING =====================================================================================
using Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryScanAggregate.Events;
using Mediator;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Application.Core.MediaLibrary.Management.Events;

/// <summary>
/// Handler for the event raised when a media library's scan progress changes.
/// </summary>
public class LibraryScanProgressChangedDomainEventHandler : INotificationHandler<LibraryScanProgressChangedDomainEvent>
{
    /// <summary>
    /// Handles the event raised when a media library's scan progress changes.
    /// </summary>
    /// <param name="domainEvent">The domain event to be handled.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    public async ValueTask Handle(LibraryScanProgressChangedDomainEvent domainEvent, CancellationToken cancellationToken)
    {
        await Task.CompletedTask;
    }
}
