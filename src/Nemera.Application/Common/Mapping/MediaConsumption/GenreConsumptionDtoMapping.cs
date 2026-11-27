#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DTO.MediaConsumption;
using Lumina.Application.Common.Utilities;
using Lumina.Contracts.Responses.MediaConsumption;
using System.Collections.Generic;
using System.Linq;
#endregion

namespace Lumina.Application.Common.Mapping.MediaConsumption;

/// <summary>
/// Extension methods for converting <see cref="GenreConsumptionDto"/>.
/// </summary>
public static class GenreConsumptionDtoMapping
{
    /// <summary>
    /// Converts <paramref name="data"/> to <see cref="GenreConsumptionResponse"/>.
    /// </summary>
    /// <param name="data">The data transfer object to be converted.</param>
    /// <returns>The converted response.</returns>
    public static GenreConsumptionResponse ToResponse(this GenreConsumptionDto data)
    {
        return new GenreConsumptionResponse(
            data.Name,
            data.ConsumedItemCount,
            data.ConsumedDurationMilliseconds.ToClampedTimeSpan().TotalSeconds
        );
    }

    /// <summary>
    /// Converts <paramref name="data"/> to a collection of <see cref="GenreConsumptionResponse"/>.
    /// </summary>
    /// <param name="data">The data transfer objects to be converted.</param>
    /// <returns>The converted responses.</returns>
    public static IReadOnlyList<GenreConsumptionResponse> ToResponses(this IReadOnlyList<GenreConsumptionDto> data)
    {
        return [.. data.Select(item => item.ToResponse())];
    }
}
