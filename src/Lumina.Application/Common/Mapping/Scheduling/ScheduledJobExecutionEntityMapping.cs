#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.Scheduling;
using Lumina.Contracts.Responses.Scheduling;
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
}
