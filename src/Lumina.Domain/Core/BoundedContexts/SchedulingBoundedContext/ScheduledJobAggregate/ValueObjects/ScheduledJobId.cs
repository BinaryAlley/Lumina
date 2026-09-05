#region ========================================================================= USING =====================================================================================
using Lumina.Domain.Common.Models.Core;
using System;
using System.Collections.Generic;
using System.Diagnostics;
#endregion

namespace Lumina.Domain.Core.BoundedContexts.SchedulingBoundedContext.ScheduledJobAggregate.ValueObjects;

/// <summary>
/// Value Object for the Id of a scheduled job.
/// </summary>
[DebuggerDisplay("{Value}")]
public sealed class ScheduledJobId : EntityId<Guid>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ScheduledJobId"/> class.
    /// </summary>
    /// <param name="value">The value representing this object.</param>
    private ScheduledJobId(Guid value) : base(value)
    {
    }

    /// <summary>
    /// Creates a new instance of the <see cref="ScheduledJobId"/> class.
    /// </summary>
    /// <returns>The created <see cref="ScheduledJobId"/> instance.</returns>
    public static ScheduledJobId CreateUnique()
    {
        return new ScheduledJobId(Guid.NewGuid());
    }

    /// <summary>
    /// Creates a new instance of the <see cref="ScheduledJobId"/> class, from a pre-existing <paramref name="value"/>.
    /// </summary>
    /// <param name="value">The value used to create the <see cref="ScheduledJobId"/> instance.</param>
    /// <returns>The created <see cref="ScheduledJobId"/> instance.</returns>
    public static ScheduledJobId Create(Guid value)
    {
        return new ScheduledJobId(value);
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
