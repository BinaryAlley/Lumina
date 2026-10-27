#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Application.Common.DTO.MediaConsumption;
using Lumina.Domain.SharedKernel.Common.Enums.MediaConsumption;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Application.Fixtures.Common.DTO.MediaConsumption;

/// <summary>
/// Fixture class for the <see cref="MediaPositionDto"/> record.
/// </summary>
[ExcludeFromCodeCoverage]
public class MediaPositionDtoFixture
{
    private const int MAXIMUM_OFFSET_MILLISECONDS = 7_200_000;

    private readonly Faker _faker = new();
    private readonly MediaPositionKind[] _positionKinds =
    [
        MediaPositionKind.TimeOffset,
        MediaPositionKind.Location
    ];

    /// <summary>
    /// Creates a random valid <see cref="MediaPositionDto"/>.
    /// </summary>
    /// <param name="kind">Optional. The kind of representation used by the position. Defaults to a random valid kind.</param>
    /// <param name="timeOffsetMilliseconds">Optional. The position within the media item, in milliseconds. Defaults to a bounded random offset for a time offset position.</param>
    /// <param name="location">Optional. The opaque locator within the media item. Defaults to a random word for a location position.</param>
    /// <param name="includeTimeOffsetMilliseconds">Whether the time offset should be included, or forced to <see langword="null"/>.</param>
    /// <param name="includeLocation">Whether the location should be included, or forced to <see langword="null"/>.</param>
    /// <returns>The created <see cref="MediaPositionDto"/>.</returns>
    public MediaPositionDto Create(
        MediaPositionKind? kind = null,
        long? timeOffsetMilliseconds = null,
        string? location = null,
        bool includeTimeOffsetMilliseconds = true,
        bool includeLocation = true)
    {
        MediaPositionKind resolvedKind = kind ?? _faker.Random.ArrayElement(_positionKinds);
        bool isTimeOffset = resolvedKind == MediaPositionKind.TimeOffset;
        long? resolvedTimeOffsetMilliseconds = includeTimeOffsetMilliseconds
            ? timeOffsetMilliseconds ?? (isTimeOffset ? _faker.Random.Long(0, MAXIMUM_OFFSET_MILLISECONDS) : (long?)null)
            : null;
        string? resolvedLocation = includeLocation
            ? location ?? (isTimeOffset ? (string?)null : _faker.Lorem.Word())
            : null;

        return new MediaPositionDto(resolvedKind, resolvedTimeOffsetMilliseconds, resolvedLocation);
    }

    /// <summary>
    /// Creates a list of <see cref="MediaPositionDto"/> instances with randomized test data.
    /// </summary>
    /// <param name="count">Number of instances to create.</param>
    /// <returns>List of configured <see cref="MediaPositionDto"/> instances.</returns>
    public List<MediaPositionDto> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
