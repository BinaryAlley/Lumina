#region ========================================================================= USING =====================================================================================
using Lumina.Application.Core.MediaConsumption.Queries.GetConsumptionStatistics;
using Lumina.Contracts.Requests.MediaConsumption;
#endregion

namespace Lumina.Application.Common.Mapping.MediaConsumption;

/// <summary>
/// Extension methods for converting <see cref="GetConsumptionStatisticsRequest"/>.
/// </summary>
public static class GetConsumptionStatisticsRequestMapping
{
    /// <summary>
    /// Converts <paramref name="request"/> to <see cref="GetConsumptionStatisticsQuery"/>.
    /// </summary>
    /// <param name="request">The request to be converted.</param>
    /// <param name="targetUserId">The Id of the user whose statistics are retrieved, taken from the route, or <see langword="null"/> for the current user.</param>
    /// <returns>The converted query.</returns>
    public static GetConsumptionStatisticsQuery ToQuery(this GetConsumptionStatisticsRequest request, string? targetUserId)
    {
        return new GetConsumptionStatisticsQuery(
            targetUserId,
            request.LibraryId,
            request.MediaKind,
            request.FromUtc,
            request.ToUtc
        );
    }
}
