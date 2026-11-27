#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Contracts.Requests.MediaLibrary.AudioLibrary.MusicLibrary.Albums;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Contracts.Fixtures.Core.Requests.MediaLibrary.AudioLibrary.MusicLibrary.Albums;

/// <summary>
/// Fixture class for the <see cref="GetAlbumTracksLiteRequest"/> record.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetAlbumTracksLiteRequestFixture
{
    private readonly Faker _faker = new();

    /// <summary>
    /// Creates a <see cref="GetAlbumTracksLiteRequest"/> with default or random values.
    /// </summary>
    /// <param name="currentPage">Optional. The page of results to retrieve.</param>
    /// <param name="perPage">Optional. The maximum number of tracks to retrieve per page.</param>
    /// <param name="includeCurrentPage">Whether the current page should be included, or forced to <see langword="null"/>.</param>
    /// <param name="includePerPage">Whether the per page count should be included, or forced to <see langword="null"/>.</param>
    /// <returns>The created <see cref="GetAlbumTracksLiteRequest"/>.</returns>
    public GetAlbumTracksLiteRequest Create(
        int? currentPage = null,
        int? perPage = null,
        bool includeCurrentPage = true,
        bool includePerPage = true)
    {
        return new GetAlbumTracksLiteRequest(
            CurrentPage: includeCurrentPage ? (currentPage ?? _faker.Random.Number(1, 100)) : null,
            PerPage: includePerPage ? (perPage ?? _faker.Random.Number(1, 200)) : null);
    }

    /// <summary>
    /// Creates a list of <see cref="GetAlbumTracksLiteRequest"/>.
    /// </summary>
    /// <param name="count">The number of elements to create.</param>
    /// <returns>The created list.</returns>
    public List<GetAlbumTracksLiteRequest> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
