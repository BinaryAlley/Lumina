#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Presentation.Web.Common.DTO.MediaConsumption;
using Lumina.Presentation.Web.Common.Enums.MediaConsumption;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Presentation.Web.Fixtures.Common.DTO.MediaConsumption;

/// <summary>
/// Fixture class for the <see cref="MediaPositionDto"/> record.
/// </summary>
[ExcludeFromCodeCoverage]
public class MediaPositionDtoFixture
{
    private readonly Faker _faker = new();

    /// <summary>
    /// Creates a new <see cref="MediaPositionDto"/> instance with randomized test data.
    /// </summary>
    /// <param name="kind">Optional kind of representation used by the position.</param>
    /// <param name="timeOffsetSeconds">Optional position within the media item, in seconds.</param>
    /// <param name="location">Optional opaque locator within the media item.</param>
    /// <param name="includeTimeOffsetSeconds">Whether the time offset should be included, or forced to <see langword="null"/>.</param>
    /// <param name="includeLocation">Whether the location should be included, or forced to <see langword="null"/>.</param>
    /// <returns>A configured <see cref="MediaPositionDto"/> instance.</returns>
    public MediaPositionDto Create(
        MediaPositionKind? kind = null,
        double? timeOffsetSeconds = null,
        string? location = null,
        bool includeTimeOffsetSeconds = true,
        bool includeLocation = true)
    {
        MediaPositionKind resolvedKind = kind ?? _faker.PickRandom(MediaPositionKind.TimeOffset, MediaPositionKind.Location);
        return new MediaPositionDto(
            Kind: resolvedKind,
            TimeOffsetSeconds: includeTimeOffsetSeconds
                ? (timeOffsetSeconds ?? (resolvedKind == MediaPositionKind.TimeOffset ? _faker.Random.Double(0, 100000) : null))
                : null,
            Location: includeLocation
                ? (location ?? (resolvedKind == MediaPositionKind.Location ? _faker.Random.AlphaNumeric(24) : null))
                : null
        );
    }

    /// <summary>
    /// Creates multiple <see cref="MediaPositionDto"/> instances with randomized test data.
    /// </summary>
    /// <param name="count">Number of instances to create.</param>
    /// <returns>List of configured <see cref="MediaPositionDto"/> instances.</returns>
    public List<MediaPositionDto> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
