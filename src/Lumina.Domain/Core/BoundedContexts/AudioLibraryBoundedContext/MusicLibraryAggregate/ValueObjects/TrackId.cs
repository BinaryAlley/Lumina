#region ========================================================================= USING =====================================================================================
using Lumina.Domain.Common.Models.Core;
using System;
using System.Collections.Generic;
using System.Diagnostics;
#endregion

namespace Lumina.Domain.Core.BoundedContexts.AudioLibraryBoundedContext.MusicLibraryAggregate.ValueObjects;

/// <summary>
/// Value Object for the Id of a track.
/// </summary>
[DebuggerDisplay("{Value}")]
public sealed class TrackId : EntityId<Guid>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="TrackId"/> class.
    /// </summary>
    /// <param name="value">The value representing this object.</param>
    private TrackId(Guid value) : base(value)
    {
    }

    /// <summary>
    /// Creates a new instance of the <see cref="TrackId"/> class.
    /// </summary>
    /// <returns>The created <see cref="TrackId"/> instance.</returns>
    public static TrackId CreateUnique()
    {
        return new TrackId(Guid.NewGuid());
    }

    /// <summary>
    /// Creates a new instance of the <see cref="TrackId"/> class, from a pre-existing <paramref name="value"/>.
    /// </summary>
    /// <param name="value">The value used to create the <see cref="TrackId"/> instance.</param>
    /// <returns>The created <see cref="TrackId"/> instance.</returns>
    public static TrackId Create(Guid value)
    {
        return new TrackId(value);
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
