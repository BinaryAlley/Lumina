#region ========================================================================= USING =====================================================================================
using Lumina.Domain.Common.Errors;
using Lumina.Domain.Common.Models.Core;
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.SharedKernel.Common.Enums.Scheduling;
using System;
using System.Collections.Generic;
#endregion

namespace Lumina.Domain.Core.BoundedContexts.SchedulingBoundedContext.ScheduledJobAggregate.ValueObjects;

/// <summary>
/// Represents an interval based schedule for a scheduled job.
/// </summary>
public class IntervalSchedule : Schedule
{
    /// <summary>
    /// Gets the interval of the schedule, in minutes.
    /// </summary>
    public int IntervalMinutes { get; }

    /// <summary>
    /// Gets the type of the schedule.
    /// </summary>
    public override ScheduleType ScheduleType { get; } = ScheduleType.WithIntervalInMinutes;

    /// <summary>
    /// Initializes a new instance of the <see cref="IntervalSchedule"/> class.
    /// </summary>
    /// <param name="intervalMinutes">The interval of the schedule, in minutes.</param>
    private IntervalSchedule(int intervalMinutes)
    {
        IntervalMinutes = intervalMinutes;
    }

    /// <summary>
    /// Creates a new instance of the <see cref="IntervalSchedule"/> class.
    /// </summary>
    /// <param name="intervalMinutes">The interval of the schedule, in minutes.</param>
    /// <returns>
    /// An <see cref="Result{TValue}"/> containing either a successfully created <see cref="IntervalSchedule"/>, or an error message.
    /// </returns>
    public static Result<IntervalSchedule> Create(int intervalMinutes)
    {
        if (intervalMinutes <= 0)
            return Errors.Scheduling.IntervalMinutesMustBePositive;
        return new IntervalSchedule(intervalMinutes);
    }

    /// <summary>
    /// Calculates the delay until the next execution of the scheduled job, relative to <paramref name="utcNow"/>.
    /// </summary>
    /// <param name="utcNow">The current UTC date and time.</param>
    /// <param name="timeZone">The time zone in which the daily hour and minute of the schedule are expressed.</param>
    /// <returns>The delay until the next execution of the scheduled job.</returns>
    public override TimeSpan GetDelayUntilNextExecution(DateTime utcNow, TimeZoneInfo timeZone)
    {
        return TimeSpan.FromMinutes(IntervalMinutes);
    }

    /// <summary>
    /// Gets the list of items that define equality of the object.
    /// </summary>
    /// <returns>A list of items defining the equality.</returns>
    public override IEnumerable<object> GetEqualityComponents()
    {
        yield return IntervalMinutes;
    }
}
