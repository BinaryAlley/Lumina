#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Contracts.Requests.MediaLibrary.AudioLibrary.MusicLibrary.Artists;
using Lumina.Domain.SharedKernel.Common.Enums.Common;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Contracts.Fixtures.Core.Requests.MediaLibrary.AudioLibrary.MusicLibrary.Artists;

/// <summary>
/// Fixture class for the <see cref="GetArtistsLiteRequest"/> record.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetArtistsLiteRequestFixture
{
    private readonly Faker _faker = new();

    /// <summary>
    /// Creates a random valid request to get the lightweight read models of the artists of a media library.
    /// </summary>
    /// <param name="currentPage">Optional. The page of results to retrieve.</param>
    /// <param name="perPage">Optional. The maximum number of artists to retrieve per page.</param>
    /// <param name="searchTerm">Optional. The search term used to filter the artists by name.</param>
    /// <param name="filterAlphaKey">Optional. The alpha key used to filter the results by the first character of their name, for the alpha picker.</param>
    /// <param name="shouldIgnoreThePrefixForAlphaPicker">Optional. Whether the leading "The " prefix of a name should be ignored when computing the alpha key, or not.</param>
    /// <param name="sortBy">Optional. The name of the field by which to sort the results.</param>
    /// <param name="sortOrder">Optional. The direction in which to sort the results.</param>
    /// <param name="includeCurrentPage">Whether the current page should be included, or forced to <see langword="null"/>.</param>
    /// <param name="includePerPage">Whether the per page count should be included, or forced to <see langword="null"/>.</param>
    /// <param name="includeSearchTerm">Whether the search term should be included, or forced to <see langword="null"/>.</param>
    /// <param name="includeFilterAlphaKey">Whether the filter alpha key should be included, or forced to <see langword="null"/>.</param>
    /// <param name="includeSortBy">Whether the sort field should be included, or forced to <see langword="null"/>.</param>
    /// <param name="includeSortOrder">Whether the sort order should be included, or forced to <see langword="null"/>.</param>
    /// <returns>The created request to get the lightweight read models of the artists of a media library.</returns>
    public GetArtistsLiteRequest Create(
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
        return new GetArtistsLiteRequest(
            includeCurrentPage ? (currentPage ?? _faker.Random.Number(1, 100)) : null,
            includePerPage ? (perPage ?? _faker.Random.Number(1, 200)) : null,
            includeSearchTerm ? (searchTerm ?? _faker.Name.FullName()) : null,
            includeFilterAlphaKey ? (filterAlphaKey ?? CreateFilterAlphaKey()) : null,
            shouldIgnoreThePrefixForAlphaPicker,
            includeSortBy ? (sortBy ?? _faker.Lorem.Word()) : null,
            includeSortOrder ? (sortOrder ?? _faker.PickRandom<SortOrder>()) : null);
    }

    /// <summary>
    /// Creates a list of <see cref="GetArtistsLiteRequest"/>.
    /// </summary>
    /// <param name="count">The number of elements to create.</param>
    /// <returns>The created list.</returns>
    public List<GetArtistsLiteRequest> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }

    /// <summary>
    /// Creates a random valid alpha key: a single ASCII letter, "#" for names starting with a digit, or "*" for any other first character.
    /// </summary>
    /// <returns>The created alpha key.</returns>
    private string CreateFilterAlphaKey()
    {
        return _faker.Random.Bool()
            ? _faker.Random.Char('A', 'Z').ToString()
            : _faker.Random.ListItem(new[] { "#", "*" });
    }
}
