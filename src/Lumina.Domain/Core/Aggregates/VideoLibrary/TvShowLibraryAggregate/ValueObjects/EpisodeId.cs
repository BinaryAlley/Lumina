#region ========================================================================= USING =====================================================================================
using Lumina.Domain.Common.Models.Core;
using System;
using System.Collections.Generic;
#endregion

namespace Lumina.Domain.Core.Aggregates.VideoLibrary.TvShowLibraryAggregate.ValueObjects;

/// <summary>
/// Value Object for the Id of an episode.
/// </summary>
public sealed class EpisodeId : EntityId<Guid>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="EpisodeId"/> class.
    /// </summary>
    /// <param name="value">The value representing this object.</param>
    private EpisodeId(Guid value) : base(value)
    {
    }

    /// <summary>
    /// Creates a new instance of the <see cref="EpisodeId"/> class.
    /// </summary>
    /// <returns>The created <see cref="EpisodeId"/> instance.</returns>
    public static EpisodeId CreateUnique()
    {
        // TODO: enforce invariants
        return new EpisodeId(Guid.NewGuid());
    }

    /// <summary>
    /// Creates a new instance of the <see cref="EpisodeId"/> class, from a pre-existing <paramref name="value"/>.
    /// </summary>
    /// <param name="value">The value used to create the <see cref="EpisodeId"/> instance.</param>
    /// <returns>The created <see cref="EpisodeId"/> instance.</returns>
    public static EpisodeId Create(Guid value)
    {
        // TODO: enforce invariants
        return new EpisodeId(value);
    }

    /// <inheritdoc/>
    public override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }
}