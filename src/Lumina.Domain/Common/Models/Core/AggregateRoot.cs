#region ========================================================================= USING =====================================================================================
using Lumina.Domain.Common.Events;
using System.Collections.Generic;
#endregion

namespace Lumina.Domain.Common.Models.Core;

/// <summary>
/// Base class for all domain Aggregate Roots.
/// </summary>
/// <typeparam name="TId">The type representing the unique identifier for the Aggregate Root. It should be a non-nullable type.</typeparam>
public abstract class AggregateRoot<TId> : Entity<TId> where TId : notnull
{
    protected readonly List<IDomainEvent> _domainEvents = [];

    /// <summary>
    /// Initializes a new instance of the <see cref="AggregateRoot{TId}"/> class.
    /// </summary>
    /// <param name="id">The id of the entity.</param>
    protected AggregateRoot(TId id) : base(id)
    {
        Id = id;
    }

    /// <summary>
    /// Retrieves and clears all domain events associated with this aggregate root.
    /// </summary>
    /// <returns>A list of all domain events that were raised by this aggregate root.</returns>
    public List<IDomainEvent> GetDomainEvents()
    {
        List<IDomainEvent> domainEventsCopy = [.. _domainEvents];
        _domainEvents.Clear();
        return domainEventsCopy;
    }
}
