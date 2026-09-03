#region ========================================================================= USING =====================================================================================
using Lumina.Application.Core.Scheduling.Commands.FireScheduledJob;
using Lumina.Contracts.Requests.Scheduling;
#endregion

namespace Lumina.Application.Common.Mapping.Scheduling;

/// <summary>
/// Extension methods for converting <see cref="FireScheduledJobRequest"/>.
/// </summary>
public static class FireScheduledJobRequestMapping
{
    /// <summary>
    /// Converts <paramref name="request"/> to <see cref="FireScheduledJobCommand"/>.
    /// </summary>
    /// <param name="request">The request to be converted.</param>
    /// <returns>The converted command.</returns>
    public static FireScheduledJobCommand ToCommand(this FireScheduledJobRequest request)
    {
        return new FireScheduledJobCommand(
            request.ScheduledJobId
        );
    }
}
