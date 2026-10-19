#region ========================================================================= USING =====================================================================================
using Lumina.Domain.Common.Models.Core;
using System;
using System.Collections.Generic;
#endregion

namespace Lumina.Domain.Core.BoundedContexts.MusicPlayHistoryBoundedContext.MusicPlayRecordAggregate.ValueObjects;

/// <summary>
/// Value Object for the last position reported by a player for a music play record.
/// </summary>
public sealed class MusicPlayPosition : ValueObject
{
    /// <summary>
    /// Gets the position reported by the player.
    /// </summary>
    public TimeSpan Offset { get; }

    /// <summary>
    /// Gets the position at the very start of a track.
    /// </summary>
    public static MusicPlayPosition Zero { get; } = new(TimeSpan.Zero);

    /// <summary>
    /// Initializes a new instance of the <see cref="MusicPlayPosition"/> class.
    /// </summary>
    /// <param name="offset">The position reported by the player.</param>
    private MusicPlayPosition(TimeSpan offset)
    {
        Offset = offset;
    }

    /// <summary>
    /// Creates a new instance of the <see cref="MusicPlayPosition"/> class.
    /// </summary>
    /// <param name="offset">The position reported by the player.</param>
    /// <returns>The created <see cref="MusicPlayPosition"/> instance.</returns>
    public static MusicPlayPosition Create(TimeSpan offset)
    {
        return new MusicPlayPosition(offset);
    }

    /// <summary>
    /// Gets the list of items that define equality of the object.
    /// </summary>
    /// <returns>A list of items defining the equality.</returns>
    public override IEnumerable<object> GetEqualityComponents()
    {
        yield return Offset;
    }
}
