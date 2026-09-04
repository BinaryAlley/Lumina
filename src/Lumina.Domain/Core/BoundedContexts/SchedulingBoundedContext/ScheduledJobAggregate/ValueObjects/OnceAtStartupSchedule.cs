#region ========================================================================= USING =====================================================================================
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.SharedKernel.Common.Enums.Scheduling;
using System;
using System.Collections.Generic;
#endregion

namespace Lumina.Domain.Core.BoundedContexts.SchedulingBoundedContext.ScheduledJobAggregate.ValueObjects;

/// <summary>
/// Value Object for the schedule that fires the task of the scheduled job once, at every application startup.
/// </summary>
public class OnceAtStartupSchedule : Schedule
{
    /// <summary>
    /// Gets the type of the schedule.
    /// </summary>
    public override ScheduleType ScheduleType { get; } = ScheduleType.OnceAtStartup;

    /// <summary>
    /// Initializes a new instance of the <see cref="OnceAtStartupSchedule"/> class.
    /// </summary>
    private OnceAtStartupSchedule()
    {
    }

    /// <summary>
    /// Creates a new instance of the <see cref="OnceAtStartupSchedule"/> class.
    /// </summary>
    /// <returns>
    /// An <see cref="Result{TValue}"/> containing either a successfully created <see cref="OnceAtStartupSchedule"/>, or an error message.
    /// </returns>
    public static Result<OnceAtStartupSchedule> Create()
    {
        return new OnceAtStartupSchedule();
    }

    /// <summary>
    /// Calculates the delay until the next execution of the scheduled job, relative to <paramref name="utcNow"/>.
    /// </summary>
    /// <param name="utcNow">The current UTC date and time.</param>
    /// <param name="timeZone">The time zone in which the daily hour and minute of the schedule are expressed.</param>
    /// <returns>The delay until the next execution of the scheduled job.</returns>
    /// <remarks>
    /// A once at startup schedule has no further execution after the one fired at the application startup, so the scheduler
    /// never calls this method for it; a maximum delay is returned so that the contract stays total.
    /// </remarks>
    public override TimeSpan GetDelayUntilNextExecution(DateTime utcNow, TimeZoneInfo timeZone)
    {
        return TimeSpan.MaxValue;
    }

    /// <summary>
    /// Gets the list of items that define equality of the object.
    /// </summary>
    /// <returns>A list of items defining the equality.</returns>
    public override IEnumerable<object> GetEqualityComponents()
    {
        yield break;
    }
}
