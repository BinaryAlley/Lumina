#region ========================================================================= USING =====================================================================================
using Lumina.Domain.Common.Models.Core;
using System;
using System.Collections.Generic;
using System.Diagnostics;
#endregion

namespace Lumina.Domain.Core.BoundedContexts.MediaConsumptionBoundedContext.MediaConsumptionRecordAggregate.ValueObjects;

/// <summary>
/// Value Object for the Id of a media consumption record.
/// </summary>
[DebuggerDisplay("{Value}")]
public sealed class MediaConsumptionRecordId : EntityId<Guid>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="MediaConsumptionRecordId"/> class.
    /// </summary>
    /// <param name="value">The value representing this object.</param>
    private MediaConsumptionRecordId(Guid value) : base(value)
    {
    }

    /// <summary>
    /// Creates a new instance of the <see cref="MediaConsumptionRecordId"/> class.
    /// </summary>
    /// <returns>The created <see cref="MediaConsumptionRecordId"/> instance.</returns>
    public static MediaConsumptionRecordId CreateUnique()
    {
        return new MediaConsumptionRecordId(Guid.NewGuid());
    }

    /// <summary>
    /// Creates a new instance of the <see cref="MediaConsumptionRecordId"/> class, from a pre-existing <paramref name="value"/>.
    /// </summary>
    /// <param name="value">The value used to create the <see cref="MediaConsumptionRecordId"/> instance.</param>
    /// <returns>The created <see cref="MediaConsumptionRecordId"/> instance.</returns>
    public static MediaConsumptionRecordId Create(Guid value)
    {
        return new MediaConsumptionRecordId(value);
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
