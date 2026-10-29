#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DTO.MediaConsumption;
using Lumina.Application.Common.Utilities;
using Lumina.Contracts.Responses.MediaConsumption;
using System.Linq;
#endregion

namespace Lumina.Application.Common.Mapping.MediaConsumption;

/// <summary>
/// Extension methods for converting <see cref="ConsumptionTotalsDto"/>.
/// </summary>
public static class ConsumptionTotalsDtoMapping
{
    /// <summary>
    /// Converts <paramref name="data"/> to <see cref="ConsumptionTotalsResponse"/>.
    /// </summary>
    /// <param name="data">The data transfer object to be converted.</param>
    /// <returns>The converted response.</returns>
    public static ConsumptionTotalsResponse ToResponse(this ConsumptionTotalsDto data)
    {
        return new ConsumptionTotalsResponse(
            data.TotalConsumedDurationMilliseconds.ToClampedTimeSpan().TotalSeconds,
            data.CountedSessionCount,
            data.CompletedSessionCount,
            data.StoppedSessionCount,
            data.CompletionRate,
            [.. data.MediaKindSummaries.Select(mediaKindSummary => mediaKindSummary.ToResponse())]
        );
    }
}
