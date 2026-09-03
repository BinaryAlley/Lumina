#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.Scheduling;
using Lumina.Contracts.Responses.Scheduling;
using Lumina.Domain.Common.Errors;
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.Core.BoundedContexts.SchedulingBoundedContext.ExternalIdentifiers.UserManagementBoundedContext.UserAggregate;
using Lumina.Domain.Core.BoundedContexts.SchedulingBoundedContext.ScheduledJobAggregate;
using Lumina.Domain.Core.BoundedContexts.SchedulingBoundedContext.ScheduledJobAggregate.ValueObjects;
using Lumina.Domain.SharedKernel.Common.Enums.Scheduling;
using System;
using System.Collections.Generic;
using System.Linq;
#endregion

namespace Lumina.Application.Common.Mapping.Scheduling;

/// <summary>
/// Extension methods for converting <see cref="ScheduledJobEntity"/>.
/// </summary>
public static class ScheduledJobEntityMapping
{
    /// <summary>
    /// Converts <paramref name="repositoryEntity"/> to <see cref="ScheduledJob"/>.
    /// </summary>
    /// <param name="repositoryEntity">The repository entity to be converted.</param>
    /// <returns>
    /// An <see cref="Result{TValue}"/> containing either a converted <see cref="ScheduledJob"/>, or an error message.
    /// </returns>
    public static Result<ScheduledJob> ToDomainEntity(this ScheduledJobEntity repositoryEntity)
    {
        Result<Schedule> scheduleResult = CreateSchedule(repositoryEntity);
        if (scheduleResult.IsFailure)
            return scheduleResult.Errors;

        return ScheduledJob.Create(
            ScheduledJobId.Create(repositoryEntity.Id),
            repositoryEntity.Name,
            repositoryEntity.TaskType,
            scheduleResult.Value,
            UserId.Create(repositoryEntity.OwnerUserId),
            repositoryEntity.Status,
            Optional<DateTime>.FromNullable(repositoryEntity.LastStartedOnUtc),
            Optional<DateTime>.FromNullable(repositoryEntity.LastCompletedOnUtc)
        );
    }

    /// <summary>
    /// Converts <paramref name="repositoryEntities"/> to a collection of <see cref="ScheduledJob"/>.
    /// </summary>
    /// <param name="repositoryEntities">The repository entities to be converted.</param>
    /// <returns>
    /// An <see cref="Result{TValue}"/> containing either a collection of converted <see cref="ScheduledJob"/>, or an error message.
    /// </returns>
    public static IEnumerable<Result<ScheduledJob>> ToDomainEntities(this IEnumerable<ScheduledJobEntity> repositoryEntities)
    {
        return repositoryEntities.Select(repositoryEntity => repositoryEntity.ToDomainEntity());
    }

    /// <summary>
    /// Converts <paramref name="repositoryEntity"/> to <see cref="ScheduledJobResponse"/>.
    /// </summary>
    /// <param name="repositoryEntity">The repository entity to be converted.</param>
    /// <returns>The converted scheduled job response.</returns>
    public static ScheduledJobResponse ToResponse(this ScheduledJobEntity repositoryEntity)
    {
        return new ScheduledJobResponse(
            repositoryEntity.Id,
            repositoryEntity.Name,
            repositoryEntity.TaskType,
            repositoryEntity.ScheduleType,
            repositoryEntity.IntervalMinutes,
            repositoryEntity.Hour,
            repositoryEntity.Minute,
            repositoryEntity.Status,
            repositoryEntity.LastStartedOnUtc,
            repositoryEntity.LastCompletedOnUtc
        );
    }

    /// <summary>
    /// Creates the schedule of <paramref name="repositoryEntity"/> from its flattened schedule properties.
    /// </summary>
    /// <param name="repositoryEntity">The repository entity whose schedule is created.</param>
    /// <returns>
    /// An <see cref="Result{TValue}"/> containing either a created <see cref="Schedule"/>, or an error message.
    /// </returns>
    private static Result<Schedule> CreateSchedule(ScheduledJobEntity repositoryEntity)
    {
        if (repositoryEntity.ScheduleType == ScheduleType.WithIntervalInMinutes)
        {
            Result<IntervalSchedule> intervalScheduleResult = IntervalSchedule.Create(repositoryEntity.IntervalMinutes ?? 0);
            if (intervalScheduleResult.IsFailure)
                return intervalScheduleResult.Errors;
            return Result.From<Schedule>(intervalScheduleResult.Value);
        }

        Result<DailySchedule> dailyScheduleResult = DailySchedule.Create(repositoryEntity.Hour ?? 0, repositoryEntity.Minute ?? 0);
        if (dailyScheduleResult.IsFailure)
            return dailyScheduleResult.Errors;
        return Result.From<Schedule>(dailyScheduleResult.Value);
    }
}
