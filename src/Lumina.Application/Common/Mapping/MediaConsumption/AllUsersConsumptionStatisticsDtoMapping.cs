#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DTO.MediaConsumption;
using Lumina.Contracts.Responses.MediaConsumption;
using System.Linq;
#endregion

namespace Lumina.Application.Common.Mapping.MediaConsumption;

/// <summary>
/// Extension methods for converting <see cref="AllUsersConsumptionStatisticsDto"/>.
/// </summary>
public static class AllUsersConsumptionStatisticsDtoMapping
{
    /// <summary>
    /// Converts <paramref name="data"/> to <see cref="AllUsersConsumptionStatisticsResponse"/>.
    /// </summary>
    /// <param name="data">The data transfer object to be converted.</param>
    /// <returns>The converted response.</returns>
    public static AllUsersConsumptionStatisticsResponse ToResponse(this AllUsersConsumptionStatisticsDto data)
    {
        return new AllUsersConsumptionStatisticsResponse(
            data.Total.ToResponse(),
            [.. data.Users.Select(user => user.ToResponse())]
        );
    }
}
