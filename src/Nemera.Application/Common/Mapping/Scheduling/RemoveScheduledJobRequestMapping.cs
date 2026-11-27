#region ========================================================================= USING =====================================================================================
using Nemera.Application.Core.Scheduling.Commands.RemoveScheduledJob;
using Nemera.Contracts.Requests.Scheduling;
#endregion

namespace Nemera.Application.Common.Mapping.Scheduling;

/// <summary>
/// Extension methods for converting <see cref="RemoveScheduledJobRequest"/>.
/// </summary>
public static class RemoveScheduledJobRequestMapping
{
    /// <summary>
    /// Converts <paramref name="request"/> to <see cref="RemoveScheduledJobCommand"/>.
    /// </summary>
    /// <param name="request">The request to be converted.</param>
    /// <returns>The converted command.</returns>
    public static RemoveScheduledJobCommand ToCommand(this RemoveScheduledJobRequest request)
    {
        return new RemoveScheduledJobCommand(
            request.ScheduledJobId
        );
    }
}
