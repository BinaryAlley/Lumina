#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Presentation.Web.Common.Enums.Common;
using Lumina.Presentation.Web.Common.Requests.Library.AudioLibrary.MusicLibrary.Artists;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Presentation.Web.Fixtures.Common.Requests.Library.AudioLibrary.MusicLibrary.Artists;

/// <summary>
/// Fixture class for the <see cref="GetMusicArtistsLiteRequest"/> record.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetMusicArtistsLiteRequestFixture
{
    private readonly Faker _faker = new();

    /// <summary>
    /// Creates a new <see cref="GetMusicArtistsLiteRequest"/> instance with randomized test data.
    /// </summary>
    /// <param name="libraryId">Optional Id of the media library whose artists are retrieved.</param>
    /// <param name="currentPage">Optional page of results to retrieve.</param>
    /// <param name="perPage">Optional maximum number of artists to retrieve per page.</param>
    /// <param name="searchTerm">Optional search term used to filter the artists by name.</param>
    /// <param name="filterAlphaKey">Optional alpha key used to filter the results by the first character of their name.</param>
    /// <param name="shouldIgnoreThePrefixForAlphaPicker">Optional whether the leading prefix of a name is ignored when computing the alpha key.</param>
    /// <param name="sortBy">Optional name of the field by which to sort the results.</param>
    /// <param name="sortOrder">Optional direction in which to sort the results.</param>
    /// <param name="includeCurrentPage">Whether the current page should be included, or forced to <see langword="null"/>.</param>
    /// <param name="includePerPage">Whether the per page count should be included, or forced to <see langword="null"/>.</param>
    /// <param name="includeSearchTerm">Whether the search term should be included, or forced to <see langword="null"/>.</param>
    /// <param name="includeFilterAlphaKey">Whether the alpha key should be included, or forced to <see langword="null"/>.</param>
    /// <param name="includeSortBy">Whether the sort field should be included, or forced to <see langword="null"/>.</param>
    /// <param name="includeSortOrder">Whether the sort order should be included, or forced to <see langword="null"/>.</param>
    /// <returns>A configured <see cref="GetMusicArtistsLiteRequest"/> instance.</returns>
    public GetMusicArtistsLiteRequest Create(
        Guid? libraryId = null,
        int? currentPage = null,
        int? perPage = null,
        string? searchTerm = null,
        string? filterAlphaKey = null,
        bool shouldIgnoreThePrefixForAlphaPicker = false,
        string? sortBy = null,
        SortOrder? sortOrder = null,
        bool includeCurrentPage = true,
        bool includePerPage = true,
        bool includeSearchTerm = true,
        bool includeFilterAlphaKey = true,
        bool includeSortBy = true,
        bool includeSortOrder = true)
    {
        return new GetMusicArtistsLiteRequest(
            LibraryId: libraryId ?? _faker.Random.Guid(),
            CurrentPage: includeCurrentPage ? (currentPage ?? _faker.Random.Int(1, 100)) : null,
            PerPage: includePerPage ? (perPage ?? _faker.Random.Int(1, 100)) : null,
            SearchTerm: includeSearchTerm ? (searchTerm ?? _faker.Lorem.Word()) : null,
            FilterAlphaKey: includeFilterAlphaKey ? (filterAlphaKey ?? _faker.Random.String2(1, "ABCDEFGHIJKLMNOPQRSTUVWXYZ")) : null,
            ShouldIgnoreThePrefixForAlphaPicker: shouldIgnoreThePrefixForAlphaPicker,
            SortBy: includeSortBy ? (sortBy ?? _faker.Lorem.Word()) : null,
            SortOrder: includeSortOrder ? (sortOrder ?? _faker.PickRandom<SortOrder>()) : null
        );
    }

    /// <summary>
    /// Creates multiple <see cref="GetMusicArtistsLiteRequest"/> instances with randomized test data.
    /// </summary>
    /// <param name="count">Number of instances to create.</param>
    /// <returns>List of configured <see cref="GetMusicArtistsLiteRequest"/> instances.</returns>
    public List<GetMusicArtistsLiteRequest> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
