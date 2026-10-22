#region ========================================================================= USING =====================================================================================
using Lumina.Domain.Common.Models.Core;
using System;
using System.Collections.Generic;
using System.Diagnostics;
#endregion

namespace Lumina.Domain.Core.BoundedContexts.FavoriteBoundedContext.FavoriteAggregate.ValueObjects;

/// <summary>
/// Value Object for the Id of a favorite.
/// </summary>
[DebuggerDisplay("{Value}")]
public sealed class FavoriteId : EntityId<Guid>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="FavoriteId"/> class.
    /// </summary>
    /// <param name="value">The value representing this object.</param>
    private FavoriteId(Guid value) : base(value)
    {
    }

    /// <summary>
    /// Creates a new instance of the <see cref="FavoriteId"/> class.
    /// </summary>
    /// <returns>The created <see cref="FavoriteId"/> instance.</returns>
    public static FavoriteId CreateUnique()
    {
        return new FavoriteId(Guid.NewGuid());
    }

    /// <summary>
    /// Creates a new instance of the <see cref="FavoriteId"/> class, from a pre-existing <paramref name="value"/>.
    /// </summary>
    /// <param name="value">The value used to create the <see cref="FavoriteId"/> instance.</param>
    /// <returns>The created <see cref="FavoriteId"/> instance.</returns>
    public static FavoriteId Create(Guid value)
    {
        return new FavoriteId(value);
    }

    /// <summary>
    /// Gets the list of items that define equality of the object.
    /// </summary>
    /// <returns>A list of items defining the equality.</returns>
    public override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }
}
