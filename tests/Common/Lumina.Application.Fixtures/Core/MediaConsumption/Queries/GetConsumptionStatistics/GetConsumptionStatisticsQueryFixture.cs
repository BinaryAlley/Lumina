#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Application.Core.MediaConsumption.Queries.GetConsumptionStatistics;
using Lumina.Domain.SharedKernel.Common.Enums.MediaLibrary;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Application.Fixtures.Core.MediaConsumption.Queries.GetConsumptionStatistics;

/// <summary>
/// Fixture class for the <see cref="GetConsumptionStatisticsQuery"/> record.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetConsumptionStatisticsQueryFixture
{
    private readonly Faker _faker = new();
    private readonly MediaKind[] _supportedMediaKinds =
    [
        MediaKind.MusicTrack,
        MediaKind.PodcastEpisode,
        MediaKind.Audiobook,
        MediaKind.Movie,
        MediaKind.TvEpisode,
        MediaKind.Book,
        MediaKind.Photo,
        MediaKind.Video
    ];

    /// <summary>
    /// Creates a random valid <see cref="GetConsumptionStatisticsQuery"/>.
    /// </summary>
    /// <param name="targetUserId">Optional. The Id of the user whose statistics are retrieved. Defaults to a random Id.</param>
    /// <param name="libraryId">Optional. The Id of the media library to restrict the statistics to. Defaults to a random Id.</param>
    /// <param name="mediaKind">Optional. The name of the kind of media to restrict the statistics to. Defaults to the name of a random supported kind.</param>
    /// <param name="fromUtc">Optional. The inclusive start of the date range. Defaults to a bounded UTC moment in the past, as a text value.</param>
    /// <param name="toUtc">Optional. The inclusive end of the date range. Defaults to a bounded UTC moment, as a text value.</param>
    /// <param name="includeTargetUserId">Whether the target user Id should be included, or forced to <see langword="null"/>.</param>
    /// <param name="includeLibraryId">Whether the library Id should be included, or forced to <see langword="null"/>.</param>
    /// <param name="includeMediaKind">Whether the media kind should be included, or forced to <see langword="null"/>.</param>
    /// <param name="includeFromUtc">Whether the start of the date range should be included, or forced to <see langword="null"/>.</param>
    /// <param name="includeToUtc">Whether the end of the date range should be included, or forced to <see langword="null"/>.</param>
    /// <returns>The created <see cref="GetConsumptionStatisticsQuery"/>.</returns>
    public GetConsumptionStatisticsQuery Create(
        string? targetUserId = null,
        string? libraryId = null,
        string? mediaKind = null,
        string? fromUtc = null,
        string? toUtc = null,
        bool includeTargetUserId = true,
        bool includeLibraryId = true,
        bool includeMediaKind = true,
        bool includeFromUtc = true,
        bool includeToUtc = true)
    {
        string resolvedFromUtc = fromUtc ?? DateTime.UtcNow.AddDays(-_faker.Random.Int(1, 30)).ToString("o", CultureInfo.InvariantCulture);
        string resolvedToUtc = toUtc ?? DateTime.UtcNow.AddDays(_faker.Random.Int(1, 30)).ToString("o", CultureInfo.InvariantCulture);

        return new GetConsumptionStatisticsQuery(
            includeTargetUserId ? targetUserId ?? _faker.Random.Guid().ToString() : null,
            includeLibraryId ? libraryId ?? _faker.Random.Guid().ToString() : null,
            includeMediaKind ? mediaKind ?? _faker.Random.ArrayElement(_supportedMediaKinds).ToString() : null,
            includeFromUtc ? resolvedFromUtc : null,
            includeToUtc ? resolvedToUtc : null);
    }

    /// <summary>
    /// Creates a list of <see cref="GetConsumptionStatisticsQuery"/> instances with randomized test data.
    /// </summary>
    /// <param name="count">Number of instances to create.</param>
    /// <returns>List of configured <see cref="GetConsumptionStatisticsQuery"/> instances.</returns>
    public List<GetConsumptionStatisticsQuery> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
