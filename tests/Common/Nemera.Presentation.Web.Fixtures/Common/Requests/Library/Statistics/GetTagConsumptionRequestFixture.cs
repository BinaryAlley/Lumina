#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Presentation.Web.Common.Enums.MediaConsumption;
using Lumina.Presentation.Web.Common.Requests.Library.Statistics;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Presentation.Web.Fixtures.Common.Requests.Library.Statistics;

/// <summary>
/// Fixture class for the <see cref="GetTagConsumptionRequest"/> record.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetTagConsumptionRequestFixture
{
    private readonly Faker _faker = new();

    /// <summary>
    /// Creates a new <see cref="GetTagConsumptionRequest"/> instance with randomized test data.
    /// </summary>
    /// <param name="userId">Optional Id of the user whose statistics are retrieved.</param>
    /// <param name="libraryId">Optional Id of the media library to restrict the statistics to.</param>
    /// <param name="mediaKind">Optional kind of media to restrict the statistics to.</param>
    /// <param name="fromUtc">Optional inclusive start of the date range.</param>
    /// <param name="toUtc">Optional inclusive end of the date range.</param>
    /// <param name="includeUserId">Whether the user Id should be included, or forced to <see langword="null"/>.</param>
    /// <param name="includeLibraryId">Whether the library Id should be included, or forced to <see langword="null"/>.</param>
    /// <param name="includeMediaKind">Whether the media kind should be included, or forced to <see langword="null"/>.</param>
    /// <param name="includeFromUtc">Whether the start of the date range should be included, or forced to <see langword="null"/>.</param>
    /// <param name="includeToUtc">Whether the end of the date range should be included, or forced to <see langword="null"/>.</param>
    /// <returns>A configured <see cref="GetTagConsumptionRequest"/> instance.</returns>
    public GetTagConsumptionRequest Create(
        string? userId = null,
        string? libraryId = null,
        MediaKind? mediaKind = null,
        DateTime? fromUtc = null,
        DateTime? toUtc = null,
        bool includeUserId = true,
        bool includeLibraryId = true,
        bool includeMediaKind = true,
        bool includeFromUtc = true,
        bool includeToUtc = true)
    {
        return new GetTagConsumptionRequest(
            UserId: includeUserId ? (userId ?? _faker.Random.Guid().ToString()) : null,
            LibraryId: includeLibraryId ? (libraryId ?? _faker.Random.Guid().ToString()) : null,
            MediaKind: includeMediaKind ? (mediaKind ?? _faker.PickRandom(MediaKind.MusicTrack, MediaKind.PodcastEpisode, MediaKind.Audiobook, MediaKind.Movie, MediaKind.TvEpisode, MediaKind.Book, MediaKind.Photo, MediaKind.Video)) : null,
            FromUtc: includeFromUtc ? (fromUtc ?? _faker.Date.Between(new DateTime(2024, 1, 1), new DateTime(2025, 12, 31))) : null,
            ToUtc: includeToUtc ? (toUtc ?? _faker.Date.Between(new DateTime(2024, 1, 1), new DateTime(2025, 12, 31))) : null
        );
    }

    /// <summary>
    /// Creates multiple <see cref="GetTagConsumptionRequest"/> instances with randomized test data.
    /// </summary>
    /// <param name="count">Number of instances to create.</param>
    /// <returns>List of configured <see cref="GetTagConsumptionRequest"/> instances.</returns>
    public List<GetTagConsumptionRequest> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
