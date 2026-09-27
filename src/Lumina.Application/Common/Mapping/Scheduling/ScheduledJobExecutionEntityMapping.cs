#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.Scheduling;
using Lumina.Contracts.Responses.Scheduling;
using System;
#endregion

namespace Lumina.Application.Common.Mapping.Scheduling;

/// <summary>
/// Extension methods for converting <see cref="ScheduledJobExecutionEntity"/>.
/// </summary>
public static class ScheduledJobExecutionEntityMapping
{
    /// <summary>
    /// Converts <paramref name="repositoryEntity"/> to <see cref="ScheduledJobExecutionResponse"/>.
    /// </summary>
    /// <param name="repositoryEntity">The repository entity to be converted.</param>
    /// <returns>The converted execution response.</returns>
    public static ScheduledJobExecutionResponse ToResponse(this ScheduledJobExecutionEntity repositoryEntity)
    {
        return new ScheduledJobExecutionResponse(
            repositoryEntity.Id,
            repositoryEntity.ScheduledJobId,
            repositoryEntity.TaskType,
            repositoryEntity.IsCycleRun,
            repositoryEntity.StartedOnUtc,
            repositoryEntity.CompletedOnUtc
        );
    }

    /// <summary>
    /// Converts <paramref name="repositoryEntity"/> to a copy that carries the provided completion time.
    /// </summary>
    /// <param name="repositoryEntity">The repository entity to be converted.</param>
    /// <param name="completedOnUtc">The completion time to assign to the converted entity.</param>
    /// <returns>The converted repository entity.</returns>
    public static ScheduledJobExecutionEntity ToUpdatedRepositoryEntity(this ScheduledJobExecutionEntity repositoryEntity, DateTime? completedOnUtc)
    {
        return new ScheduledJobExecutionEntity
        {
            Id = repositoryEntity.Id,
            ScheduledJobId = repositoryEntity.ScheduledJobId,
            TaskType = repositoryEntity.TaskType,
            IsCycleRun = repositoryEntity.IsCycleRun,
            WasCycleActive = repositoryEntity.WasCycleActive,
            StartedOnUtc = repositoryEntity.StartedOnUtc,
            CompletedOnUtc = completedOnUtc,
            CreatedOnUtc = repositoryEntity.CreatedOnUtc,
            CreatedBy = repositoryEntity.CreatedBy,
            UpdatedOnUtc = repositoryEntity.UpdatedOnUtc,
            UpdatedBy = repositoryEntity.UpdatedBy
        };
    }
}
