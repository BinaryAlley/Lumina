#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Application.Common.DTO.Pagination;
using Lumina.Application.Core.MediaConsumption.Queries.GetRecentlyConsumed;
using Lumina.Application.Fixtures.Common.DTO.Pagination;
using Lumina.Domain.SharedKernel.Common.Enums.MediaLibrary;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Application.Fixtures.Core.MediaConsumption.Queries.GetRecentlyConsumed;

/// <summary>
/// Fixture class for the <see cref="GetRecentlyConsumedQuery"/> record.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetRecentlyConsumedQueryFixture
{
    private readonly Faker _faker = new();
    private readonly PaginationDataDtoFixture _paginationDataDtoFixture = new();
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
    /// Creates a random valid <see cref="GetRecentlyConsumedQuery"/>.
    /// </summary>
    /// <param name="targetUserId">Optional. The Id of the user whose history is retrieved. Defaults to a random Id.</param>
    /// <param name="libraryId">Optional. The Id of the media library to restrict the history to. Defaults to a random Id.</param>
    /// <param name="mediaKind">Optional. The kind of media to restrict the history to. Defaults to a random supported kind.</param>
    /// <param name="fromUtc">Optional. The inclusive start of the date range. Defaults to a bounded moment in the past.</param>
    /// <param name="toUtc">Optional. The inclusive end of the date range. Defaults to a bounded moment after the start.</param>
    /// <param name="paginationData">Optional. The requested pagination data. Defaults to a random page when it is included.</param>
    /// <param name="includeTargetUserId">Whether the target user Id should be included, or forced to <see langword="null"/>.</param>
    /// <param name="includeLibraryId">Whether the library Id should be included, or forced to <see langword="null"/>.</param>
    /// <param name="includeMediaKind">Whether the media kind should be included, or forced to <see langword="null"/>.</param>
    /// <param name="includeFromUtc">Whether the start of the date range should be included, or forced to <see langword="null"/>.</param>
    /// <param name="includeToUtc">Whether the end of the date range should be included, or forced to <see langword="null"/>.</param>
    /// <param name="includePaginationData">Whether the pagination data should be included, or forced to <see langword="null"/>.</param>
    /// <returns>The created <see cref="GetRecentlyConsumedQuery"/>.</returns>
    public GetRecentlyConsumedQuery Create(
        string? targetUserId = null,
        string? libraryId = null,
        MediaKind? mediaKind = null,
        DateTime? fromUtc = null,
        DateTime? toUtc = null,
        PaginationDataDto? paginationData = null,
        bool includeTargetUserId = true,
        bool includeLibraryId = true,
        bool includeMediaKind = true,
        bool includeFromUtc = true,
        bool includeToUtc = true,
        bool includePaginationData = false)
    {
        DateTime resolvedFromUtc = fromUtc ?? (toUtc.HasValue
            ? toUtc.Value.AddDays(-_faker.Random.Int(1, 30))
            : DateTime.UtcNow.AddDays(-_faker.Random.Int(1, 30)));
        DateTime resolvedToUtc = toUtc ?? resolvedFromUtc.AddDays(_faker.Random.Int(1, 30));

        return new GetRecentlyConsumedQuery(
            includeTargetUserId ? targetUserId ?? _faker.Random.Guid().ToString() : null,
            includeLibraryId ? libraryId ?? _faker.Random.Guid().ToString() : null,
            includeMediaKind ? mediaKind ?? _faker.Random.ArrayElement(_supportedMediaKinds) : null,
            includeFromUtc ? resolvedFromUtc : null,
            includeToUtc ? resolvedToUtc : null,
            includePaginationData ? paginationData ?? _paginationDataDtoFixture.Create() : null);
    }

    /// <summary>
    /// Creates a list of <see cref="GetRecentlyConsumedQuery"/> instances with randomized test data.
    /// </summary>
    /// <param name="count">Number of instances to create.</param>
    /// <returns>List of configured <see cref="GetRecentlyConsumedQuery"/> instances.</returns>
    public List<GetRecentlyConsumedQuery> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
