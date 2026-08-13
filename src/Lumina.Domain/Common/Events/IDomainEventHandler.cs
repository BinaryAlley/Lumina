#region ========================================================================= USING =====================================================================================
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Domain.Common.Events;

/// <summary>
/// Defines a contract for handling a domain event type in the application layer.
/// </summary>
/// <typeparam name="TDomainEvent">The type of domain event to handle.</typeparam>
public interface IDomainEventHandler<TDomainEvent> where TDomainEvent : IDomainEvent
{
    /// <summary>
    /// Handles the specified <paramref name="domainEvent"/>.
    /// </summary>
    /// <param name="domainEvent">The domain event to handle.</param>
    /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    ValueTask HandleAsync(TDomainEvent domainEvent, CancellationToken cancellationToken);
}
