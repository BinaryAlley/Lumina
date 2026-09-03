#region ========================================================================= USING =====================================================================================
using Lumina.Domain.Common.Models.Core;
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.SharedKernel.Common.Enums.Scheduling;
using System;
#endregion

namespace Lumina.Domain.Core.BoundedContexts.SchedulingBoundedContext.ScheduledJobAggregate.ValueObjects;

/// <summary>
/// Represents an abstract schedule for a scheduled job.
/// </summary>
public abstract class Schedule : ValueObject
{
    /// <summary>
    /// Gets the type of the schedule.
    /// </summary>
    public abstract ScheduleType ScheduleType { get; }

    /// <summary>
    /// Calculates the delay until the next execution of the scheduled job, relative to <paramref name="utcNow"/>.
    /// </summary>
    /// <param name="utcNow">The current UTC date and time.</param>
    /// <param name="timeZone">The time zone in which the daily hour and minute of the schedule are expressed.</param>
    /// <returns>The delay until the next execution of the scheduled job.</returns>
    public abstract TimeSpan GetDelayUntilNextExecution(DateTime utcNow, TimeZoneInfo timeZone);
}
