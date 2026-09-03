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
/// Represents a daily schedule for a scheduled job.
/// </summary>
public class DailySchedule : Schedule
{
    /// <summary>
    /// Gets the hour of the day at which the scheduled job runs.
    /// </summary>
    public int Hour { get; }

    /// <summary>
    /// Gets the minute of the hour at which the scheduled job runs.
    /// </summary>
    public int Minute { get; }

    /// <summary>
    /// Gets the type of the schedule.
    /// </summary>
    public override ScheduleType ScheduleType { get; } = ScheduleType.DailyAtHourAndMinute;

    /// <summary>
    /// Initializes a new instance of the <see cref="DailySchedule"/> class.
    /// </summary>
    /// <param name="hour">The hour of the day at which the scheduled job runs.</param>
    /// <param name="minute">The minute of the hour at which the scheduled job runs.</param>
    private DailySchedule(int hour, int minute)
    {
        Hour = hour;
        Minute = minute;
    }

    /// <summary>
    /// Creates a new instance of the <see cref="DailySchedule"/> class.
    /// </summary>
    /// <param name="hour">The hour of the day at which the scheduled job runs.</param>
    /// <param name="minute">The minute of the hour at which the scheduled job runs.</param>
    /// <returns>
    /// An <see cref="Result{TValue}"/> containing either a successfully created <see cref="DailySchedule"/>, or an error message.
    /// </returns>
    public static Result<DailySchedule> Create(int hour, int minute)
    {
        if (hour < 0 || hour > 23)
            return Errors.Scheduling.HourMustBeBetweenZeroAndTwentyThree;
        if (minute < 0 || minute > 59)
            return Errors.Scheduling.MinuteMustBeBetweenZeroAndFiftyNine;
        return new DailySchedule(hour, minute);
    }

    /// <summary>
    /// Calculates the delay until the next execution of the scheduled job, relative to <paramref name="utcNow"/>.
    /// </summary>
    /// <param name="utcNow">The current UTC date and time.</param>
    /// <param name="timeZone">The time zone in which the daily hour and minute of the schedule are expressed.</param>
    /// <returns>The delay until the next execution of the scheduled job.</returns>
    public override TimeSpan GetDelayUntilNextExecution(DateTime utcNow, TimeZoneInfo timeZone)
    {
        DateTime localNow = TimeZoneInfo.ConvertTimeFromUtc(utcNow, timeZone);
        DateTime nextRunLocal = new(localNow.Year, localNow.Month, localNow.Day, Hour, Minute, 0, DateTimeKind.Unspecified);
        DateTime nextRunUtc = TimeZoneInfo.ConvertTimeToUtc(nextRunLocal, timeZone);
        // When the next run time already passed today, schedule the execution for the next day.
        if (nextRunUtc <= utcNow)
            nextRunUtc = nextRunUtc.AddDays(1);
        return nextRunUtc - utcNow;
    }

    /// <summary>
    /// Gets the list of items that define equality of the object.
    /// </summary>
    /// <returns>A list of items defining the equality.</returns>
    public override IEnumerable<object> GetEqualityComponents()
    {
        yield return Hour;
        yield return Minute;
    }
}
