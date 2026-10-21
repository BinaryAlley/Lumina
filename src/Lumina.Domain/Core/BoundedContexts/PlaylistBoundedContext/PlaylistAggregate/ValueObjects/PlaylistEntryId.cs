#region ========================================================================= USING =====================================================================================
using Lumina.Domain.Common.Models.Core;
using System;
using System.Collections.Generic;
using System.Diagnostics;
#endregion

namespace Lumina.Domain.Core.BoundedContexts.PlaylistBoundedContext.PlaylistAggregate.ValueObjects;

/// <summary>
/// Value Object for the Id of an entry of a playlist.
/// </summary>
[DebuggerDisplay("{Value}")]
public sealed class PlaylistEntryId : EntityId<Guid>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="PlaylistEntryId"/> class.
    /// </summary>
    /// <param name="value">The value representing this object.</param>
    private PlaylistEntryId(Guid value) : base(value)
    {
    }

    /// <summary>
    /// Creates a new instance of the <see cref="PlaylistEntryId"/> class.
    /// </summary>
    /// <returns>The created <see cref="PlaylistEntryId"/> instance.</returns>
    public static PlaylistEntryId CreateUnique()
    {
        return new PlaylistEntryId(Guid.NewGuid());
    }

    /// <summary>
    /// Creates a new instance of the <see cref="PlaylistEntryId"/> class, from a pre-existing <paramref name="value"/>.
    /// </summary>
    /// <param name="value">The value used to create the <see cref="PlaylistEntryId"/> instance.</param>
    /// <returns>The created <see cref="PlaylistEntryId"/> instance.</returns>
    public static PlaylistEntryId Create(Guid value)
    {
        return new PlaylistEntryId(value);
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
