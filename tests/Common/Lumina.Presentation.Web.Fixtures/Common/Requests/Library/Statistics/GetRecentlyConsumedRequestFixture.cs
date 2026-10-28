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
/// Fixture class for the <see cref="GetRecentlyConsumedRequest"/> record.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetRecentlyConsumedRequestFixture
{
    private readonly Faker _faker = new();

    /// <summary>
    /// Creates a new <see cref="GetRecentlyConsumedRequest"/> instance with randomized test data.
    /// </summary>
    /// <param name="userId">Optional Id of the user whose history is retrieved.</param>
    /// <param name="libraryId">Optional Id of the media library to restrict the history to.</param>
    /// <param name="mediaKind">Optional kind of media to restrict the history to.</param>
    /// <param name="fromUtc">Optional inclusive start of the date range.</param>
    /// <param name="toUtc">Optional inclusive end of the date range.</param>
    /// <param name="currentPage">Optional one based page of media items to return.</param>
    /// <param name="perPage">Optional number of media items per page.</param>
    /// <param name="includeUserId">Whether the user Id should be included, or forced to <see langword="null"/>.</param>
    /// <param name="includeLibraryId">Whether the library Id should be included, or forced to <see langword="null"/>.</param>
    /// <param name="includeMediaKind">Whether the media kind should be included, or forced to <see langword="null"/>.</param>
    /// <param name="includeFromUtc">Whether the start of the date range should be included, or forced to <see langword="null"/>.</param>
    /// <param name="includeToUtc">Whether the end of the date range should be included, or forced to <see langword="null"/>.</param>
    /// <param name="includeCurrentPage">Whether the current page should be included, or forced to <see langword="null"/>.</param>
    /// <param name="includePerPage">Whether the page size should be included, or forced to <see langword="null"/>.</param>
    /// <returns>A configured <see cref="GetRecentlyConsumedRequest"/> instance.</returns>
    public GetRecentlyConsumedRequest Create(
        string? userId = null,
        string? libraryId = null,
        MediaKind? mediaKind = null,
        DateTime? fromUtc = null,
        DateTime? toUtc = null,
        int? currentPage = null,
        int? perPage = null,
        bool includeUserId = true,
        bool includeLibraryId = true,
        bool includeMediaKind = true,
        bool includeFromUtc = true,
        bool includeToUtc = true,
        bool includeCurrentPage = true,
        bool includePerPage = true)
    {
        return new GetRecentlyConsumedRequest(
            UserId: includeUserId ? (userId ?? _faker.Random.Guid().ToString()) : null,
            LibraryId: includeLibraryId ? (libraryId ?? _faker.Random.Guid().ToString()) : null,
            MediaKind: includeMediaKind ? (mediaKind ?? _faker.PickRandom(MediaKind.MusicTrack, MediaKind.PodcastEpisode, MediaKind.Audiobook, MediaKind.Movie, MediaKind.TvEpisode, MediaKind.Book, MediaKind.Photo, MediaKind.Video)) : null,
            FromUtc: includeFromUtc ? (fromUtc ?? _faker.Date.Between(new DateTime(2024, 1, 1), new DateTime(2025, 12, 31))) : null,
            ToUtc: includeToUtc ? (toUtc ?? _faker.Date.Between(new DateTime(2024, 1, 1), new DateTime(2025, 12, 31))) : null,
            CurrentPage: includeCurrentPage ? (currentPage ?? _faker.Random.Int(1, 10)) : null,
            PerPage: includePerPage ? (perPage ?? _faker.Random.Int(1, 100)) : null
        );
    }

    /// <summary>
    /// Creates multiple <see cref="GetRecentlyConsumedRequest"/> instances with randomized test data.
    /// </summary>
    /// <param name="count">Number of instances to create.</param>
    /// <returns>List of configured <see cref="GetRecentlyConsumedRequest"/> instances.</returns>
    public List<GetRecentlyConsumedRequest> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
