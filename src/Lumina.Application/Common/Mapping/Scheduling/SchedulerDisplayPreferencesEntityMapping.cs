#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.Scheduling;
using Lumina.Contracts.Responses.Scheduling;
using Lumina.Domain.SharedKernel.Common.Enums.Scheduling;
using System;
#endregion

namespace Lumina.Application.Common.Mapping.Scheduling;

/// <summary>
/// Extension methods for converting <see cref="SchedulerDisplayPreferencesEntity"/>.
/// </summary>
public static class SchedulerDisplayPreferencesEntityMapping
{
    private const int DEFAULT_DISPLAY_TIME_SPAN = 10;
    private const SchedulerDisplayTimeUnit DEFAULT_DISPLAY_TIME_UNIT = SchedulerDisplayTimeUnit.Minutes;

    /// <summary>
    /// Converts <paramref name="repositoryEntity"/> to <see cref="SchedulerDisplayPreferencesResponse"/>.
    /// </summary>
    /// <param name="repositoryEntity">The repository entity to be converted.</param>
    /// <returns>The converted response.</returns>
    public static SchedulerDisplayPreferencesResponse ToResponse(this SchedulerDisplayPreferencesEntity repositoryEntity)
    {
        return new SchedulerDisplayPreferencesResponse(
            repositoryEntity.UserId,
            repositoryEntity.JobTypeFilter,
            repositoryEntity.DisplayTimeSpan,
            repositoryEntity.DisplayTimeUnit
        );
    }

    /// <summary>
    /// Converts the lack of stored display preferences to the default <see cref="SchedulerDisplayPreferencesResponse"/> of the user identified by <paramref name="userId"/>.
    /// </summary>
    /// <param name="repositoryEntity">The missing repository entity, which is <see langword="null"/>.</param>
    /// <param name="userId">The Id of the user that owns the display preferences.</param>
    /// <returns>The converted response with the default display preferences.</returns>
    public static SchedulerDisplayPreferencesResponse ToDefaultResponse(this SchedulerDisplayPreferencesEntity? repositoryEntity, Guid userId)
    {
        return new SchedulerDisplayPreferencesResponse(
            userId,
            null,
            DEFAULT_DISPLAY_TIME_SPAN,
            DEFAULT_DISPLAY_TIME_UNIT
        );
    }
}
