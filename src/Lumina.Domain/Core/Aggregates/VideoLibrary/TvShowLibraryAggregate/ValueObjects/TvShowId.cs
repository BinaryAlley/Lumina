#region ========================================================================= USING =====================================================================================
using Lumina.Domain.Common.Models.Core;
using System;
using System.Collections.Generic;
#endregion

namespace Lumina.Domain.Core.Aggregates.VideoLibrary.TvShowLibraryAggregate.ValueObjects;

/// <summary>
/// Value Object for the Id of a TV show.
/// </summary>
public sealed class TvShowId : EntityId<Guid>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="TvShowId"/> class.
    /// </summary>
    /// <param name="value">The value representing this object.</param>
    private TvShowId(Guid value) : base(value)
    {
    }

    /// <summary>
    /// Creates a new instance of the <see cref="TvShowId"/> class.
    /// </summary>
    /// <returns>The created <see cref="TvShowId"/> instance.</returns>
    public static TvShowId CreateUnique()
    {
        // TODO: enforce invariants
        return new TvShowId(Guid.NewGuid());
    }

    /// <summary>
    /// Creates a new instance of the <see cref="TvShowId"/> class, from a pre-existing <paramref name="value"/>.
    /// </summary>
    /// <param name="value">The value used to create the <see cref="TvShowId"/> instance.</param>
    /// <returns>The created <see cref="TvShowId"/> instance.</returns>
    public static TvShowId Create(Guid value)
    {
        // TODO: enforce invariants
        return new TvShowId(value);
    }

    /// <inheritdoc/>
    public override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }
}