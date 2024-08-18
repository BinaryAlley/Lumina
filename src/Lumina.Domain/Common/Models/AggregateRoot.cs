namespace Lumina.Domain.Common.Models;

/// <summary>
/// Base class for all domain Aggregate Roots.
/// </summary>
/// <typeparam name="TId">The type representing the unique identifier for the Aggregate Root. It should be a non-nullable type.</typeparam>
public abstract class AggregateRoot<TId, TIdType> : Entity<TId> where TId : notnull, AggregateRootId<TIdType>
                                                                where TIdType : notnull
{
    #region ==================================================================== PROPERTIES =================================================================================
    public new AggregateRootId<TIdType> Id { get; protected set; }
    #endregion

    #region ====================================================================== CTOR =====================================================================================
    /// <summary>
    /// Overload C-tor.
    /// </summary>
    /// <param name="id">The id of the entity.</param>
    protected AggregateRoot(TId id) : base(id)
    {
        Id = id;
    }

#pragma warning disable CS8618
    protected AggregateRoot() // only needed during reflection
    {
        
    }
#pragma warning restore CS8618
    #endregion
}