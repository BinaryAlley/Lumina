namespace Lumina.Domain.Common.Models.Core;

/// <summary>
/// Base identity class for all domain Aggregate Roots.
/// </summary>
/// <typeparam name="TId">The type representing the unique identifier for the Aggregate Root. It should be a non-nullable type.</typeparam>
public abstract class AggregateRootId<TId> : ValueObject where TId : notnull
{
    #region ==================================================================== PROPERTIES =================================================================================
    /// <summary>
    /// Gets the unique identifier of the Aggregate Root.
    /// </summary>
    public abstract TId Value { get; protected set; }
    #endregion
}