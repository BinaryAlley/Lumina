#region ========================================================================= USING =====================================================================================
using Nemera.Application.Core.Scheduling.Commands.UpdateSchedulerDisplayPreferences;
using Nemera.Contracts.Requests.Scheduling;
#endregion

namespace Nemera.Application.Common.Mapping.Scheduling;

/// <summary>
/// Extension methods for converting <see cref="UpdateSchedulerDisplayPreferencesRequest"/>.
/// </summary>
public static class UpdateSchedulerDisplayPreferencesRequestMapping
{
    /// <summary>
    /// Converts <paramref name="request"/> to <see cref="UpdateSchedulerDisplayPreferencesCommand"/>.
    /// </summary>
    /// <param name="request">The request to be converted.</param>
    /// <returns>The converted command.</returns>
    public static UpdateSchedulerDisplayPreferencesCommand ToCommand(this UpdateSchedulerDisplayPreferencesRequest request)
    {
        return new UpdateSchedulerDisplayPreferencesCommand(
            request.JobTypeFilter,
            request.DisplayTimeSpan,
            request.DisplayTimeUnit
        );
    }
}
