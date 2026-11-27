#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Presentation.Web.Common.Requests.Playlists;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Presentation.Web.Fixtures.Common.Requests.Playlists;

/// <summary>
/// Fixture class for the <see cref="GetPlaylistRequest"/> record.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetPlaylistRequestFixture
{
    private readonly Faker _faker = new();

    /// <summary>
    /// Creates a new <see cref="GetPlaylistRequest"/> instance with randomized test data.
    /// </summary>
    /// <param name="currentPage">Optional one based page of entries to return.</param>
    /// <param name="perPage">Optional number of entries per page.</param>
    /// <param name="includeCurrentPage">Whether the page should be included, or forced to <see langword="null"/>.</param>
    /// <param name="includePerPage">Whether the page size should be included, or forced to <see langword="null"/>.</param>
    /// <returns>A configured <see cref="GetPlaylistRequest"/> instance.</returns>
    public GetPlaylistRequest Create(
        int? currentPage = null,
        int? perPage = null,
        bool includeCurrentPage = true,
        bool includePerPage = true)
    {
        return new GetPlaylistRequest(
            CurrentPage: includeCurrentPage ? (currentPage ?? _faker.Random.Int(1, 10)) : null,
            PerPage: includePerPage ? (perPage ?? _faker.Random.Int(1, 200)) : null
        );
    }

    /// <summary>
    /// Creates multiple <see cref="GetPlaylistRequest"/> instances with randomized test data.
    /// </summary>
    /// <param name="count">Number of instances to create.</param>
    /// <returns>List of configured <see cref="GetPlaylistRequest"/> instances.</returns>
    public List<GetPlaylistRequest> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
