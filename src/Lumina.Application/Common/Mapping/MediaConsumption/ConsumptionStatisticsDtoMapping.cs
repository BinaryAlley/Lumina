#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DTO.MediaConsumption;
using Lumina.Application.Common.Utilities;
using Lumina.Contracts.Responses.MediaConsumption;
using System.Linq;
#endregion

namespace Lumina.Application.Common.Mapping.MediaConsumption;

/// <summary>
/// Extension methods for converting <see cref="ConsumptionStatisticsDto"/>.
/// </summary>
public static class ConsumptionStatisticsDtoMapping
{
    /// <summary>
    /// Converts <paramref name="data"/> to <see cref="ConsumptionSummaryResponse"/>.
    /// </summary>
    /// <param name="data">The data transfer object to be converted.</param>
    /// <returns>The converted response.</returns>
    public static ConsumptionSummaryResponse ToResponse(this ConsumptionStatisticsDto data)
    {
        return new ConsumptionSummaryResponse(
            data.UserId,
            data.TotalConsumedDurationMilliseconds.ToClampedTimeSpan().TotalSeconds,
            data.CountedSessionCount,
            data.CompletedSessionCount,
            data.StoppedSessionCount,
            data.CompletionRate,
            [.. data.MediaKindSummaries.Select(mediaKindSummary => mediaKindSummary.ToResponse())]
        );
    }
}
