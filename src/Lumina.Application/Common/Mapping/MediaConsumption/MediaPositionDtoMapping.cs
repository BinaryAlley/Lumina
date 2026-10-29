#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DTO.MediaConsumption;
using Lumina.Application.Common.Utilities;
using Lumina.Contracts.Responses.MediaConsumption;
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
            data.TimeOffsetMilliseconds.HasValue ? data.TimeOffsetMilliseconds.Value.ToClampedTimeSpan().TotalSeconds : null,
            data.Location
        );
    }
}
