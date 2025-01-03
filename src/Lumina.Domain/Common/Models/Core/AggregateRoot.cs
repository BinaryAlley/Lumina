namespace Lumina.Domain.Common.Models.Core;

/// <summary>
/// Base class for all domain Aggregate Roots.
/// </summary>
/// <typeparam name="TId">The type representing the unique identifier for the Aggregate Root. It should be a non-nullable type.</typeparam>
public abstract class AggregateRoot<TId> : Entity<TId> where TId : notnull
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AggregateRoot{TId}"/> class.
    /// </summary>
    /// <param name="id">The id of the entity.</param>
    protected AggregateRoot(TId id) : base(id)
    {
        Id = id;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="AggregateRoot{TId}"/> class.
    /// </summary>
    protected AggregateRoot() // only needed during reflection
    {

    }
}
