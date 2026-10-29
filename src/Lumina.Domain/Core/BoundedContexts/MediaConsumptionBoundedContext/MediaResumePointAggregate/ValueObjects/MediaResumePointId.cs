#region ========================================================================= USING =====================================================================================
using Lumina.Domain.Common.Models.Core;
using System;
using System.Collections.Generic;
using System.Diagnostics;
#endregion

namespace Lumina.Domain.Core.BoundedContexts.MediaConsumptionBoundedContext.MediaResumePointAggregate.ValueObjects;

/// <summary>
/// Value Object for the Id of a media resume point.
/// </summary>
[DebuggerDisplay("{Value}")]
public sealed class MediaResumePointId : EntityId<Guid>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="MediaResumePointId"/> class.
    /// </summary>
    /// <param name="value">The value representing this object.</param>
    private MediaResumePointId(Guid value) : base(value)
    {
    }

    /// <summary>
    /// Creates a new instance of the <see cref="MediaResumePointId"/> class.
    /// </summary>
    /// <returns>The created <see cref="MediaResumePointId"/> instance.</returns>
    public static MediaResumePointId CreateUnique()
    {
        return new MediaResumePointId(Guid.NewGuid());
    }

    /// <summary>
    /// Creates a new instance of the <see cref="MediaResumePointId"/> class, from a pre-existing <paramref name="value"/>.
    /// </summary>
    /// <param name="value">The value used to create the <see cref="MediaResumePointId"/> instance.</param>
    /// <returns>The created <see cref="MediaResumePointId"/> instance.</returns>
    public static MediaResumePointId Create(Guid value)
    {
        return new MediaResumePointId(value);
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
