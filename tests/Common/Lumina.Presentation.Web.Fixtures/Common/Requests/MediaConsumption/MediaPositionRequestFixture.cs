#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Presentation.Web.Common.Enums.MediaConsumption;
using Lumina.Presentation.Web.Common.Requests.MediaConsumption;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Presentation.Web.Fixtures.Common.Requests.MediaConsumption;

/// <summary>
/// Fixture class for the <see cref="MediaPositionRequest"/> record.
/// </summary>
[ExcludeFromCodeCoverage]
public class MediaPositionRequestFixture
{
    private readonly Faker _faker = new();

    /// <summary>
    /// Creates a new <see cref="MediaPositionRequest"/> instance with randomized test data.
    /// </summary>
    /// <param name="kind">Optional name of the kind of the position.</param>
    /// <param name="timeOffsetSeconds">Optional position within the media item, in seconds.</param>
    /// <param name="location">Optional opaque locator within the media item.</param>
    /// <param name="includeTimeOffsetSeconds">Whether the time offset should be included, or forced to <see langword="null"/>.</param>
    /// <param name="includeLocation">Whether the location should be included, or forced to <see langword="null"/>.</param>
    /// <returns>A configured <see cref="MediaPositionRequest"/> instance.</returns>
    public MediaPositionRequest Create(
        string? kind = null,
        double? timeOffsetSeconds = null,
        string? location = null,
        bool includeTimeOffsetSeconds = true,
        bool includeLocation = true)
    {
        MediaPositionKind resolvedRandomKind = _faker.PickRandom(MediaPositionKind.TimeOffset, MediaPositionKind.Location);
        bool isTimeOffset = kind is null
            ? resolvedRandomKind == MediaPositionKind.TimeOffset
            : string.Equals(kind, nameof(MediaPositionKind.TimeOffset), StringComparison.OrdinalIgnoreCase);
        bool isLocation = kind is null
            ? resolvedRandomKind == MediaPositionKind.Location
            : string.Equals(kind, nameof(MediaPositionKind.Location), StringComparison.OrdinalIgnoreCase);
        return new MediaPositionRequest(
            Kind: kind ?? resolvedRandomKind.ToString(),
            TimeOffsetSeconds: includeTimeOffsetSeconds ? (timeOffsetSeconds ?? (isTimeOffset ? _faker.Random.Double(0, 100000) : null)) : null,
            Location: includeLocation ? (location ?? (isLocation ? _faker.Random.AlphaNumeric(24) : null)) : null
        );
    }

    /// <summary>
    /// Creates multiple <see cref="MediaPositionRequest"/> instances with randomized test data.
    /// </summary>
    /// <param name="count">Number of instances to create.</param>
    /// <returns>List of configured <see cref="MediaPositionRequest"/> instances.</returns>
    public List<MediaPositionRequest> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
