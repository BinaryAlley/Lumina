#region ========================================================================= USING =====================================================================================
using Lumina.Domain.Common.Models.Core;
using System;
using System.Collections.Generic;
#endregion

namespace Lumina.Domain.Core.Aggregates.MediaContributor.MediaContributorAggregate.ValueObjects;

/// <summary>
/// Value Object for the Id of a media contributor.
/// </summary>
public sealed class MediaContributorId : EntityId<Guid>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="MediaContributorId"/> class.
    /// </summary>
    /// <param name="value">The value representing this object.</param>
    private MediaContributorId(Guid value) : base(value)
    {
    }

    /// <summary>
    /// Creates a new instance of the <see cref="MediaContributorId"/> class.
    /// </summary>
    /// <returns>The created <see cref="MediaContributorId"/> instance.</returns>
    public static MediaContributorId CreateUnique()
    {
        // TODO: enforce invariants
        return new MediaContributorId(Guid.NewGuid());
    }

    /// <summary>
    /// Creates a new instance of the <see cref="MediaContributorId"/> class, from a pre-existing <paramref name="value"/>.
    /// </summary>
    /// <param name="value">The value used to create the <see cref="MediaContributorId"/> instance.</param>
    /// <returns>The created <see cref="MediaContributorId"/> instance.</returns>
    public static MediaContributorId Create(Guid value)
    {
        // TODO: enforce invariants
        return new MediaContributorId(value);
    }

    /// <inheritdoc/>
    public override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }
}