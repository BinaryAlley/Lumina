#region ========================================================================= USING =====================================================================================
using Lumina.Domain.Common.Models.Core;
using System;
using System.Collections.Generic;
#endregion

namespace Lumina.Domain.Core.Aggregates.VideoLibrary.TvShowLibraryAggregate.ValueObjects;

/// <summary>
/// Value Object for the Id of a season.
/// </summary>
public sealed class SeasonId : EntityId<Guid>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SeasonId"/> class.
    /// </summary>
    /// <param name="value">The value representing this object.</param>
    private SeasonId(Guid value) : base(value)
    {
    }

    /// <summary>
    /// Creates a new instance of the <see cref="SeasonId"/> class.
    /// </summary>
    /// <returns>The created <see cref="SeasonId"/> instance.</returns>
    public static SeasonId CreateUnique()
    {
        // TODO: enforce invariants
        return new SeasonId(Guid.NewGuid());
    }

    /// <summary>
    /// Creates a new instance of the <see cref="SeasonId"/> class, from a pre-existing <paramref name="value"/>.
    /// </summary>
    /// <param name="value">The value used to create the <see cref="SeasonId"/> instance.</param>
    /// <returns>The created <see cref="SeasonId"/> instance.</returns>
    public static SeasonId Create(Guid value)
    {
        // TODO: enforce invariants
        return new SeasonId(value);
    }

    /// <inheritdoc/>
    public override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }
}