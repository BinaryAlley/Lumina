#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DTO.MediaConsumption;
using Lumina.Contracts.Responses.MediaConsumption;
using System;
#endregion

namespace Lumina.Application.Common.Mapping.MediaConsumption;

/// <summary>
/// Extension methods for converting <see cref="MediaPositionDto"/>. This mapping is the position response builder for repository-driven reads.
/// </summary>
public static class MediaPositionDtoMapping
{
    /// <summary>
    /// Converts <paramref name="data"/> to <see cref="MediaPositionResponse"/>.
    /// </summary>
    /// <param name="data">The data transfer object to be converted.</param>
    /// <returns>The converted response.</returns>
    public static MediaPositionResponse ToResponse(this MediaPositionDto data)
    {
        return new MediaPositionResponse(
            data.Kind,
            data.TimeOffsetMilliseconds.HasValue ? TimeSpan.FromMilliseconds(data.TimeOffsetMilliseconds.Value).TotalSeconds : null,
            data.Location
        );
    }
}
