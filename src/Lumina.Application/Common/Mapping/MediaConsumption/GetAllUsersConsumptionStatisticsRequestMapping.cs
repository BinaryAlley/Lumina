#region ========================================================================= USING =====================================================================================
using Lumina.Application.Core.MediaConsumption.Queries.GetAllUsersConsumptionStatistics;
using Lumina.Contracts.Requests.MediaConsumption;
#endregion

namespace Lumina.Application.Common.Mapping.MediaConsumption;

/// <summary>
/// Extension methods for converting <see cref="GetAllUsersConsumptionStatisticsRequest"/>.
/// </summary>
public static class GetAllUsersConsumptionStatisticsRequestMapping
{
    /// <summary>
    /// Converts <paramref name="request"/> to <see cref="GetAllUsersConsumptionStatisticsQuery"/>.
    /// </summary>
    /// <param name="request">The request to be converted.</param>
    /// <returns>The converted query.</returns>
    public static GetAllUsersConsumptionStatisticsQuery ToQuery(this GetAllUsersConsumptionStatisticsRequest request)
    {
        return new GetAllUsersConsumptionStatisticsQuery(
            request.LibraryId,
            request.MediaKind,
            request.FromUtc,
            request.ToUtc
        );
    }
}
