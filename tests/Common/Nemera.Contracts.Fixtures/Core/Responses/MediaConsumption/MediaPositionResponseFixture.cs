#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Contracts.Responses.MediaConsumption;
using Lumina.Domain.SharedKernel.Common.Enums.MediaConsumption;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Contracts.Fixtures.Core.Responses.MediaConsumption;

/// <summary>
/// Fixture class for the <see cref="MediaPositionResponse"/> record.
/// </summary>
[ExcludeFromCodeCoverage]
public class MediaPositionResponseFixture
{
    private const double MAXIMUM_OFFSET_SECONDS = 7_200;

    private readonly Faker _faker = new();
    private readonly MediaPositionKind[] _positionKinds =
    [
        MediaPositionKind.TimeOffset,
        MediaPositionKind.Location
    ];

    /// <summary>
    /// Creates a random valid <see cref="MediaPositionResponse"/>.
    /// </summary>
    /// <param name="kind">Optional. The kind of representation used by the position. Defaults to a random valid kind.</param>
    /// <param name="timeOffsetSeconds">Optional. The position within the media item, in seconds. Defaults to a bounded random offset for a time offset position.</param>
    /// <param name="location">Optional. The opaque locator within the media item. Defaults to a random word for a location position.</param>
    /// <param name="includeTimeOffsetSeconds">Whether the time offset should be included, or forced to <see langword="null"/>.</param>
    /// <param name="includeLocation">Whether the location should be included, or forced to <see langword="null"/>.</param>
    /// <returns>The created <see cref="MediaPositionResponse"/>.</returns>
    public MediaPositionResponse Create(
        MediaPositionKind? kind = null,
        double? timeOffsetSeconds = null,
        string? location = null,
        bool includeTimeOffsetSeconds = true,
        bool includeLocation = true)
    {
        MediaPositionKind resolvedKind = kind ?? _faker.Random.ArrayElement(_positionKinds);
        bool isTimeOffset = resolvedKind == MediaPositionKind.TimeOffset;
        double? resolvedTimeOffsetSeconds = includeTimeOffsetSeconds
            ? timeOffsetSeconds ?? (isTimeOffset ? _faker.Random.Double(0, MAXIMUM_OFFSET_SECONDS) : (double?)null)
            : null;
        string? resolvedLocation = includeLocation
            ? location ?? (isTimeOffset ? null : _faker.Lorem.Word())
            : null;

        return new MediaPositionResponse(resolvedKind, resolvedTimeOffsetSeconds, resolvedLocation);
    }

    /// <summary>
    /// Creates a list of <see cref="MediaPositionResponse"/> instances with randomized test data.
    /// </summary>
    /// <param name="count">Number of instances to create.</param>
    /// <returns>List of configured <see cref="MediaPositionResponse"/> instances.</returns>
    public List<MediaPositionResponse> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
