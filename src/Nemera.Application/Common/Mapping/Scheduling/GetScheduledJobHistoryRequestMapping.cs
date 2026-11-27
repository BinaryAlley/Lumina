#region ========================================================================= USING =====================================================================================
using Nemera.Application.Core.Scheduling.Queries.GetScheduledJobHistory;
using Nemera.Contracts.Requests.Scheduling;
#endregion

namespace Nemera.Application.Common.Mapping.Scheduling;

/// <summary>
/// Extension methods for converting <see cref="GetScheduledJobHistoryRequest"/>.
/// </summary>
public static class GetScheduledJobHistoryRequestMapping
{
    /// <summary>
    /// Converts <paramref name="request"/> to <see cref="GetScheduledJobHistoryQuery"/>.
    /// </summary>
    /// <param name="request">The request to be converted.</param>
    /// <returns>The converted query.</returns>
    public static GetScheduledJobHistoryQuery ToQuery(this GetScheduledJobHistoryRequest request)
    {
        return new GetScheduledJobHistoryQuery(
            request.From,
            request.To
        );
    }
}
