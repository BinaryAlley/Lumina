#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DTO.MediaConsumption;
using Lumina.Application.Common.Utilities;
using Lumina.Contracts.Responses.MediaConsumption;
#endregion

namespace Lumina.Application.Common.Mapping.MediaConsumption;

/// <summary>
/// Extension methods for converting <see cref="MediaKindConsumptionStatisticsDto"/>.
/// </summary>
public static class MediaKindConsumptionStatisticsDtoMapping
{
    /// <summary>
    /// Converts <paramref name="data"/> to <see cref="MediaKindConsumptionSummaryResponse"/>.
    /// </summary>
    /// <param name="data">The data transfer object to be converted.</param>
    /// <returns>The converted response.</returns>
    public static MediaKindConsumptionSummaryResponse ToResponse(this MediaKindConsumptionStatisticsDto data)
    {
        return new MediaKindConsumptionSummaryResponse(
            data.MediaKind,
            data.TotalConsumedDurationMilliseconds.ToClampedTimeSpan().TotalSeconds,
            data.CountedSessionCount,
            data.CompletedSessionCount,
            data.StoppedSessionCount,
            data.CompletionRate
        );
    }
}
