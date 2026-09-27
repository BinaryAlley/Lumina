#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Contracts.Requests.MediaLibrary.AudioLibrary.MusicLibrary.Artists;
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
    /// <param name="includeCurrentPage">Whether the current page should be included, or forced to <see langword="null"/>.</param>
    /// <param name="includePerPage">Whether the per page count should be included, or forced to <see langword="null"/>.</param>
    /// <param name="includeSearchTerm">Whether the search term should be included, or forced to <see langword="null"/>.</param>
    /// <returns>The created request to get the lightweight read models of the artists of a media library.</returns>
    public GetArtistsLiteRequest Create(
        int? currentPage = null,
        int? perPage = null,
        string? searchTerm = null,
        bool includeCurrentPage = true,
        bool includePerPage = true,
        bool includeSearchTerm = true)
    {
        return new GetArtistsLiteRequest(
            includeCurrentPage ? (currentPage ?? _faker.Random.Number(1, 100)) : null,
            includePerPage ? (perPage ?? _faker.Random.Number(1, 200)) : null,
            includeSearchTerm ? (searchTerm ?? _faker.Name.FullName()) : null);
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
}
