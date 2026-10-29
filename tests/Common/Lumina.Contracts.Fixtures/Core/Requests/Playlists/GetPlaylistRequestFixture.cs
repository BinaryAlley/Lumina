#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Contracts.Requests.Playlists;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Contracts.Fixtures.Core.Requests.Playlists;

/// <summary>
/// Fixture class for the <see cref="GetPlaylistRequest"/> record.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetPlaylistRequestFixture
{
    private readonly Faker _faker = new();

    /// <summary>
    /// Creates a random valid <see cref="GetPlaylistRequest"/>.
    /// </summary>
    /// <param name="currentPage">Optional. The one based page of entries to return. Defaults to a bounded random page.</param>
    /// <param name="perPage">Optional. The number of entries per page. Defaults to a bounded random page size.</param>
    /// <param name="includeCurrentPage">Whether the current page should be included, or forced to <see langword="null"/>.</param>
    /// <param name="includePerPage">Whether the per page count should be included, or forced to <see langword="null"/>.</param>
    /// <returns>The created <see cref="GetPlaylistRequest"/>.</returns>
    public GetPlaylistRequest Create(
        int? currentPage = null,
        int? perPage = null,
        bool includeCurrentPage = true,
        bool includePerPage = true)
    {
        return new GetPlaylistRequest(
            includeCurrentPage ? (currentPage ?? _faker.Random.Int(1, 100)) : null,
            includePerPage ? (perPage ?? _faker.Random.Int(1, 200)) : null);
    }

    /// <summary>
    /// Creates a list of <see cref="GetPlaylistRequest"/> instances with randomized test data.
    /// </summary>
    /// <param name="count">Number of instances to create.</param>
    /// <returns>List of configured <see cref="GetPlaylistRequest"/> instances.</returns>
    public List<GetPlaylistRequest> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
