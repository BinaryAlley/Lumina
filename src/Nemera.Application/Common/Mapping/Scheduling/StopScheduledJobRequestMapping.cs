#region ========================================================================= USING =====================================================================================
using Lumina.Application.Core.Scheduling.Commands.StopScheduledJob;
using Lumina.Contracts.Requests.Scheduling;
#endregion

namespace Lumina.Application.Common.Mapping.Scheduling;

/// <summary>
/// Extension methods for converting <see cref="StopScheduledJobRequest"/>.
/// </summary>
public static class StopScheduledJobRequestMapping
{
    /// <summary>
    /// Converts <paramref name="request"/> to <see cref="StopScheduledJobCommand"/>.
    /// </summary>
    /// <param name="request">The request to be converted.</param>
    /// <returns>The converted command.</returns>
    public static StopScheduledJobCommand ToCommand(this StopScheduledJobRequest request)
    {
        return new StopScheduledJobCommand(
            request.ScheduledJobId
        );
    }
}
