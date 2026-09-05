#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.Scheduling;
using Lumina.Contracts.Responses.Scheduling;
using Lumina.Domain.Core.BoundedContexts.SchedulingBoundedContext.ScheduledJobAggregate;
using Lumina.Domain.Core.BoundedContexts.SchedulingBoundedContext.ScheduledJobAggregate.ValueObjects;
using System;
#endregion

namespace Lumina.Application.Common.Mapping.Scheduling;

/// <summary>
/// Extension methods for converting <see cref="ScheduledJob"/>.
/// </summary>
public static class ScheduledJobMapping
{
    /// <summary>
    /// Converts <paramref name="domainEntity"/> to <see cref="ScheduledJobEntity"/>.
    /// </summary>
    /// <param name="domainEntity">The domain entity to be converted.</param>
    /// <returns>The converted repository entity.</returns>
    public static ScheduledJobEntity ToRepositoryEntity(this ScheduledJob domainEntity)
    {
        return new ScheduledJobEntity
        {
            Id = domainEntity.Id.Value,
            Name = domainEntity.Name,
            TaskType = domainEntity.TaskType,
            ScheduleType = domainEntity.Schedule.ScheduleType,
            IntervalMinutes = GetIntervalMinutes(domainEntity),
            Hour = GetHour(domainEntity),
            Minute = GetMinute(domainEntity),
            Status = domainEntity.Status,
            OwnerUserId = domainEntity.OwnerUserId.Value,
            LastStartedOnUtc = domainEntity.LastStartedOnUtc.HasValue ? domainEntity.LastStartedOnUtc.Value : null,
            LastCompletedOnUtc = domainEntity.LastCompletedOnUtc.HasValue ? domainEntity.LastCompletedOnUtc.Value : null,
            CreatedOnUtc = domainEntity.CreatedOnUtc,
            CreatedBy = Guid.Empty,
            UpdatedOnUtc = domainEntity.UpdatedOnUtc,
            UpdatedBy = Guid.Empty
        };
    }

    /// <summary>
    /// Converts <paramref name="domainEntity"/> to <see cref="ScheduledJobResponse"/>.
    /// </summary>
    /// <param name="domainEntity">The domain entity to be converted.</param>
    /// <returns>The converted scheduled job response.</returns>
    public static ScheduledJobResponse ToResponse(this ScheduledJob domainEntity)
    {
        return new ScheduledJobResponse(
            domainEntity.Id.Value,
            domainEntity.Name,
            domainEntity.TaskType,
            domainEntity.Schedule.ScheduleType,
            GetIntervalMinutes(domainEntity),
            GetHour(domainEntity),
            GetMinute(domainEntity),
            domainEntity.Status,
            domainEntity.LastStartedOnUtc.HasValue ? domainEntity.LastStartedOnUtc.Value : null,
            domainEntity.LastCompletedOnUtc.HasValue ? domainEntity.LastCompletedOnUtc.Value : null
        );
    }

    /// <summary>
    /// Gets the interval in minutes of the schedule of <paramref name="domainEntity"/>, when it is an interval schedule.
    /// </summary>
    /// <param name="domainEntity">The scheduled job whose schedule interval is retrieved.</param>
    /// <returns>The interval in minutes of the schedule, or <see langword="null"/> when the schedule is not an interval schedule.</returns>
    private static int? GetIntervalMinutes(ScheduledJob domainEntity)
    {
        return domainEntity.Schedule is IntervalSchedule intervalSchedule ? intervalSchedule.IntervalMinutes : null;
    }

    /// <summary>
    /// Gets the hour of the schedule of <paramref name="domainEntity"/>, when it is a daily schedule.
    /// </summary>
    /// <param name="domainEntity">The scheduled job whose schedule hour is retrieved.</param>
    /// <returns>The hour of the schedule, or <see langword="null"/> when the schedule is not a daily schedule.</returns>
    private static int? GetHour(ScheduledJob domainEntity)
    {
        return domainEntity.Schedule is DailySchedule dailySchedule ? dailySchedule.Hour : null;
    }

    /// <summary>
    /// Gets the minute of the schedule of <paramref name="domainEntity"/>, when it is a daily schedule.
    /// </summary>
    /// <param name="domainEntity">The scheduled job whose schedule minute is retrieved.</param>
    /// <returns>The minute of the schedule, or <see langword="null"/> when the schedule is not a daily schedule.</returns>
    private static int? GetMinute(ScheduledJob domainEntity)
    {
        return domainEntity.Schedule is DailySchedule dailySchedule ? dailySchedule.Minute : null;
    }
}
