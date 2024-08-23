namespace Lumina.Domain.Common.Models.Core;

/// <summary>
/// Base class for all domain Aggregate Roots.
/// </summary>
/// <typeparam name="TId">The type representing the unique identifier for the Aggregate Root. It should be a non-nullable type.</typeparam>
/// <typeparam name="TIdType">The type of the unique identifier for the Aggregate Root. It should be a non-nullable type.</typeparam>
public abstract class AggregateRoot<TId, TIdType> : Entity<TId> where TId : notnull, AggregateRootId<TIdType>
                                                                where TIdType : notnull
{
    #region ==================================================================== PROPERTIES =================================================================================
    /// <summary>
    /// Gets the unique identifier of the Aggregate Root.
    /// </summary>
    public new AggregateRootId<TIdType> Id { get; protected set; }
    #endregion

    #region ====================================================================== CTOR =====================================================================================
    /// <summary>
    /// Initializes a new instance of the <see cref="AggregateRoot{TId, TIdType}"/> class.
    /// </summary>
    /// <param name="id">The id of the entity.</param>
    protected AggregateRoot(TId id) : base(id)
    {
        Id = id;
    }

#pragma warning disable CS8618
    /// <summary>
    /// Initializes a new instance of the <see cref="AggregateRoot{TId, TIdType}"/> class.
    /// </summary>
    protected AggregateRoot() // only needed during reflection
    {
        
    }
#pragma warning restore CS8618
    #endregion
}