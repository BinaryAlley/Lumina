#region ========================================================================= USING =====================================================================================
using Lumina.Domain.Common.Models.Core;
using System;
using System.Collections.Generic;
using System.Diagnostics;
#endregion

namespace Lumina.Domain.Core.BoundedContexts.AudioLibraryBoundedContext.ExternalIdentifiers.MediaContributorBoundedContext.MediaContributorAggregate;

/// <summary>
/// Value Object for the Id of a media contributor, duplicated from the MediaContributor bounded context to keep the cross bounded context references decoupled.
/// </summary>
[DebuggerDisplay("{Value}")]
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
        return new MediaContributorId(Guid.NewGuid());
    }

    /// <summary>
    /// Creates a new instance of the <see cref="MediaContributorId"/> class, from a pre-existing <paramref name="value"/>.
    /// </summary>
    /// <param name="value">The value used to create the <see cref="MediaContributorId"/> instance.</param>
    /// <returns>The created <see cref="MediaContributorId"/> instance.</returns>
    public static MediaContributorId Create(Guid value)
    {
        return new MediaContributorId(value);
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
