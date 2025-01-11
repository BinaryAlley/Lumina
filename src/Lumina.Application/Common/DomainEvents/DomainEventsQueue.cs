#region ========================================================================= USING =====================================================================================
using Lumina.Domain.Common.Events;
using System.Collections.Concurrent;
#endregion

namespace Lumina.Application.Common.DomainEvents;

/// <summary>
/// Represents a thread-safe queue for managing domain events.
/// </summary>
public class DomainEventsQueue : IDomainEventsQueue
{
    private readonly ConcurrentQueue<IDomainEvent> _queue = new();

    /// <summary>
    /// Adds a domain event to the queue.
    /// </summary>
    /// <param name="domainEvent">The domain event to enqueue.</param>
    public void Enqueue(IDomainEvent domainEvent)
    {
        _queue.Enqueue(domainEvent);
    }

    /// <summary>
    /// Attempts to remove and return the domain event at the beginning of the queue.
    /// </summary>
    /// <param name="domainEvent">When this method returns, contains the domain event removed from the queue, or the default value of <see cref="IDomainEvent"/> if the queue is empty.
    /// </param>
    /// <returns><see langword="true"/> if a domain event was successfuly dequeued, <see langword="false"/> otherwise.</returns>
    public bool TryDequeue(out IDomainEvent domainEvent)
    {
        return _queue.TryDequeue(out domainEvent!);
    }
}
