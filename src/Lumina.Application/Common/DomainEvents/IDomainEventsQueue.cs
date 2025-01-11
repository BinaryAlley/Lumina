#region ========================================================================= USING =====================================================================================
using Lumina.Domain.Common.Events;
#endregion

namespace Lumina.Application.Common.DomainEvents;

/// <summary>
/// Interface for a thread-safe queue for managing domain events.
/// </summary>
public interface IDomainEventsQueue
{
    /// <summary>
    /// Adds a domain event to the queue.
    /// </summary>
    /// <param name="domainEvent">The domain event to enqueue.</param>
    void Enqueue(IDomainEvent domainEvent);

    /// <summary>
    /// Attempts to remove and return the domain event at the beginning of the queue.
    /// </summary>
    /// <param name="domainEvent">When this method returns, contains the domain event removed from the queue, or the default value of <see cref="IDomainEvent"/> if the queue is empty.
    /// </param>
    /// <returns><see langword="true"/> if a domain event was successfuly dequeued, <see langword="false"/> otherwise.</returns>
    bool TryDequeue(out IDomainEvent domainEvent);
}
