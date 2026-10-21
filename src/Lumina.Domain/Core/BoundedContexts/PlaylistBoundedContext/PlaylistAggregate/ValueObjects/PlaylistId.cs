#region ========================================================================= USING =====================================================================================
using Lumina.Domain.Common.Models.Core;
using System;
using System.Collections.Generic;
using System.Diagnostics;
#endregion

namespace Lumina.Domain.Core.BoundedContexts.PlaylistBoundedContext.PlaylistAggregate.ValueObjects;

/// <summary>
/// Value Object for the Id of a playlist.
/// </summary>
[DebuggerDisplay("{Value}")]
public sealed class PlaylistId : EntityId<Guid>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="PlaylistId"/> class.
    /// </summary>
    /// <param name="value">The value representing this object.</param>
    private PlaylistId(Guid value) : base(value)
    {
    }

    /// <summary>
    /// Creates a new instance of the <see cref="PlaylistId"/> class.
    /// </summary>
    /// <returns>The created <see cref="PlaylistId"/> instance.</returns>
    public static PlaylistId CreateUnique()
    {
        return new PlaylistId(Guid.NewGuid());
    }

    /// <summary>
    /// Creates a new instance of the <see cref="PlaylistId"/> class, from a pre-existing <paramref name="value"/>.
    /// </summary>
    /// <param name="value">The value used to create the <see cref="PlaylistId"/> instance.</param>
    /// <returns>The created <see cref="PlaylistId"/> instance.</returns>
    public static PlaylistId Create(Guid value)
    {
        return new PlaylistId(value);
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
