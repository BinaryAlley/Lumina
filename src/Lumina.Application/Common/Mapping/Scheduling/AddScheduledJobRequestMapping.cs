#region ========================================================================= USING =====================================================================================
using Lumina.Application.Core.Scheduling.Commands.AddScheduledJob;
using Lumina.Contracts.Requests.Scheduling;
#endregion

namespace Lumina.Application.Common.Mapping.Scheduling;

/// <summary>
/// Extension methods for converting <see cref="AddScheduledJobRequest"/>.
/// </summary>
public static class AddScheduledJobRequestMapping
{
    /// <summary>
    /// Converts <paramref name="request"/> to <see cref="AddScheduledJobCommand"/>.
    /// </summary>
    /// <param name="request">The request to be converted.</param>
    /// <returns>The converted command.</returns>
    public static AddScheduledJobCommand ToCommand(this AddScheduledJobRequest request)
    {
        return new AddScheduledJobCommand(
            request.Name,
            request.TaskType,
            request.ScheduleType,
            request.IntervalMinutes,
            request.Hour,
            request.Minute
        );
    }
}
