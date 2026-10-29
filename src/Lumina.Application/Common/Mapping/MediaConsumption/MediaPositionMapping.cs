#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DTO.MediaConsumption;
using Lumina.Contracts.Responses.MediaConsumption;
using Lumina.Domain.Core.BoundedContexts.MediaConsumptionBoundedContext.Common.ValueObjects;
#endregion

namespace Lumina.Application.Common.Mapping.MediaConsumption;

/// <summary>
/// Extension methods for converting <see cref="MediaPosition"/>. The position is mapped directly to its DTO and to its response.
/// </summary>
public static class MediaPositionMapping
{
    /// <summary>
    /// Converts <paramref name="domainValueObject"/> to <see cref="MediaPositionDto"/>.
    /// </summary>
    /// <param name="domainValueObject">The domain value object to be converted.</param>
    /// <returns>The converted data transfer object.</returns>
    public static MediaPositionDto ToDto(this MediaPosition domainValueObject)
    {
        return new MediaPositionDto(
            domainValueObject.Kind,
            domainValueObject.TimeOffset.HasValue ? (long)domainValueObject.TimeOffset.Value.TotalMilliseconds : null,
            domainValueObject.Location.HasValue ? domainValueObject.Location.Value : null
        );
    }

    /// <summary>
    /// Converts <paramref name="domainValueObject"/> to <see cref="MediaPositionResponse"/>.
    /// </summary>
    /// <param name="domainValueObject">The domain value object to be converted.</param>
    /// <returns>The converted response.</returns>
    public static MediaPositionResponse ToResponse(this MediaPosition domainValueObject)
    {
        return new MediaPositionResponse(
            domainValueObject.Kind,
            domainValueObject.TimeOffset.HasValue ? domainValueObject.TimeOffset.Value.TotalSeconds : null,
            domainValueObject.Location.HasValue ? domainValueObject.Location.Value : null
        );
    }
}
