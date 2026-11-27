#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Contracts.Requests.MediaConsumption;
using Lumina.Domain.SharedKernel.Common.Enums.MediaConsumption;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Contracts.Fixtures.Core.Requests.MediaConsumption;

/// <summary>
/// Fixture class for the <see cref="MediaPositionRequest"/> record.
/// </summary>
[ExcludeFromCodeCoverage]
public class MediaPositionRequestFixture
{
    private const double MAXIMUM_OFFSET_SECONDS = 7_200;

    private readonly Faker _faker = new();

    /// <summary>
    /// Creates a random valid <see cref="MediaPositionRequest"/>.
    /// </summary>
    /// <param name="kind">Optional. The kind of the position, as the name of a <see cref="MediaPositionKind"/> member. Defaults to a random valid kind.</param>
    /// <param name="timeOffsetSeconds">Optional. The position within the media item, in seconds. Defaults to a bounded random offset for a time offset position.</param>
    /// <param name="location">Optional. The opaque locator within the media item. Defaults to a random word for a location position.</param>
    /// <param name="includeKind">Whether the kind should be included, or forced to <see langword="null"/>.</param>
    /// <param name="includeTimeOffsetSeconds">Whether the time offset should be included, or forced to <see langword="null"/>.</param>
    /// <param name="includeLocation">Whether the location should be included, or forced to <see langword="null"/>.</param>
    /// <returns>The created <see cref="MediaPositionRequest"/>.</returns>
    public MediaPositionRequest Create(
        string? kind = null,
        double? timeOffsetSeconds = null,
        string? location = null,
        bool includeKind = true,
        bool includeTimeOffsetSeconds = true,
        bool includeLocation = true)
    {
        string? resolvedKind = includeKind ? kind ?? _faker.PickRandom<MediaPositionKind>().ToString() : null;
        bool isTimeOffset = resolvedKind == nameof(MediaPositionKind.TimeOffset);
        double? resolvedTimeOffsetSeconds = includeTimeOffsetSeconds
            ? timeOffsetSeconds ?? (isTimeOffset ? _faker.Random.Double(0, MAXIMUM_OFFSET_SECONDS) : (double?)null)
            : null;
        string? resolvedLocation = includeLocation
            ? location ?? (isTimeOffset ? (string?)null : _faker.Lorem.Word())
            : null;

        return new MediaPositionRequest(resolvedKind, resolvedTimeOffsetSeconds, resolvedLocation);
    }

    /// <summary>
    /// Creates a list of <see cref="MediaPositionRequest"/> instances with randomized test data.
    /// </summary>
    /// <param name="count">Number of instances to create.</param>
    /// <returns>List of configured <see cref="MediaPositionRequest"/> instances.</returns>
    public List<MediaPositionRequest> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
