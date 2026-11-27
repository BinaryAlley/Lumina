#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DTO.MediaConsumption;
using Lumina.Application.Common.Utilities;
using Lumina.Contracts.Responses.MediaConsumption;
using System.Collections.Generic;
using System.Linq;
#endregion

namespace Lumina.Application.Common.Mapping.MediaConsumption;

/// <summary>
/// Extension methods for converting <see cref="TagConsumptionDto"/>.
/// </summary>
public static class TagConsumptionDtoMapping
{
    /// <summary>
    /// Converts <paramref name="data"/> to <see cref="TagConsumptionResponse"/>.
    /// </summary>
    /// <param name="data">The data transfer object to be converted.</param>
    /// <returns>The converted response.</returns>
    public static TagConsumptionResponse ToResponse(this TagConsumptionDto data)
    {
        return new TagConsumptionResponse(
            data.Name,
            data.ConsumedItemCount,
            data.ConsumedDurationMilliseconds.ToClampedTimeSpan().TotalSeconds
        );
    }

    /// <summary>
    /// Converts <paramref name="data"/> to a collection of <see cref="TagConsumptionResponse"/>.
    /// </summary>
    /// <param name="data">The data transfer objects to be converted.</param>
    /// <returns>The converted responses.</returns>
    public static IReadOnlyList<TagConsumptionResponse> ToResponses(this IReadOnlyList<TagConsumptionDto> data)
    {
        return [.. data.Select(item => item.ToResponse())];
    }
}
