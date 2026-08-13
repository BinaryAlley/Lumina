#region ========================================================================= USING =====================================================================================
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Domain.Common.Events;

/// <summary>
/// Publishes domain events to all handlers registered for the event type.
/// </summary>
public interface IDomainEventPublisher
{
    /// <summary>
    /// Publishes the specified <paramref name="domainEvent"/> to all its registered handlers, in registration order.
    /// </summary>
    /// <param name="domainEvent">The domain event to publish.</param>
    /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    ValueTask PublishAsync(IDomainEvent domainEvent, CancellationToken cancellationToken = default);
}
