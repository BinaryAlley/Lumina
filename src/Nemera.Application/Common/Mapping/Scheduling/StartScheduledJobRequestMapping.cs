#region ========================================================================= USING =====================================================================================
using Lumina.Application.Core.Scheduling.Commands.StartScheduledJob;
using Lumina.Contracts.Requests.Scheduling;
#endregion

namespace Lumina.Application.Common.Mapping.Scheduling;

/// <summary>
/// Extension methods for converting <see cref="StartScheduledJobRequest"/>.
/// </summary>
public static class StartScheduledJobRequestMapping
{
    /// <summary>
    /// Converts <paramref name="request"/> to <see cref="StartScheduledJobCommand"/>.
    /// </summary>
    /// <param name="request">The request to be converted.</param>
    /// <returns>The converted command.</returns>
    public static StartScheduledJobCommand ToCommand(this StartScheduledJobRequest request)
    {
        return new StartScheduledJobCommand(
            request.ScheduledJobId
        );
    }
}
