#region ========================================================================= USING =====================================================================================
using System;
#endregion

namespace Lumina.Domain.Common.Models.Core;

/// <summary>
/// Base class for all domain Entities.
/// </summary>
/// <typeparam name="TId">The type representing the unique identifier for the Entity. It should be a non-nullable type.</typeparam>
public abstract class Entity<TId> : IEquatable<Entity<TId>> where TId : notnull
{
    /// <summary>
    /// Gets the unique identifier of the entity.
    /// </summary>
    public TId Id { get; protected set; }

    /// <summary>
    /// Gets the date and time when the entity was created.
    /// </summary>
    public DateTime CreatedOnUtc { get; set; }

    /// <summary>
    /// Gets the date and time when the entity was last updated.
    /// </summary>
    public DateTime? UpdatedOnUtc { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="Entity{TId}"/> class.
    /// </summary>
    /// <param name="id">The id of the entity.</param>
    protected Entity(TId id)
    {
        Id = id;
    }

#pragma warning disable CS8618
    /// <summary>
    /// Initializes a new instance of the <see cref="Entity{TId}"/> class.
    /// </summary>
    protected Entity() // only needed during reflection
    {

    }
#pragma warning restore CS8618

    /// <summary>
    /// Indicates whether the current object is equal to another object of the same type.
    /// </summary>
    /// <param name="other">An object to compare with this object.</param>
    /// <returns><see langword="true"/> if the current object is equal to the other parameter, <see langword="false"/> otherwise.</returns>
    public bool Equals(Entity<TId>? other)
    {
        return Equals((object?)other);
    }

    /// <summary>
    /// Determines whether the specified objects' Id is equal to the current objects' Id.
    /// </summary>
    /// <param name="obj">The object whose Id is compared with the current objects' Id.</param>
    /// <returns><see langword="true"/> if the specified objects' Id is equal to the current objects' Id, <see langword="false"/> otherwise.</returns>
    public override bool Equals(object? obj)
    {
        return obj is Entity<TId> entity && Id.Equals(entity.Id);
    }

    /// <summary>
    /// Custom implementation of the equality operator.
    /// </summary>
    /// <param name="left">The left operand of equality.</param>
    /// <param name="right">The right operand of equality.</param>
    /// <returns><see langword="true"/> if <paramref name="left"/> is equal to <paramref name="right"/>, <see langword="false"/> otherwise.</returns>
    public static bool operator ==(Entity<TId> left, Entity<TId> right)
    {
        return Equals(left, right);
    }

    /// <summary>
    /// Custom implementation of the inequality operator.
    /// </summary>
    /// <param name="left">The left operand of equality.</param>
    /// <param name="right">The right operand of equality.</param>
    /// <returns><see langword="true"/> if <paramref name="left"/> is not equal to <paramref name="right"/>, <see langword="false"/> otherwise.</returns>
    public static bool operator !=(Entity<TId> left, Entity<TId> right)
    {
        return !Equals(left, right);
    }

    /// <summary>
    /// Override of the default hash function.
    /// </summary>
    /// <returns>A hash code for the current object.</returns>
    public override int GetHashCode()
    {
        return Id.GetHashCode();
    }
}