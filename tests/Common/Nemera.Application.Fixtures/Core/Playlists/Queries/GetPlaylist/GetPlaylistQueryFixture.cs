#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DTO.Pagination;
using Lumina.Application.Core.Playlists.Queries.GetPlaylist;
using Lumina.Application.Fixtures.Common.DTO.Pagination;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Application.Fixtures.Core.Playlists.Queries.GetPlaylist;

/// <summary>
/// Fixture class for the <see cref="GetPlaylistQuery"/> record.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetPlaylistQueryFixture
{
    private readonly PaginationDataDtoFixture _paginationDataDtoFixture = new();

    /// <summary>
    /// Creates a random valid <see cref="GetPlaylistQuery"/>.
    /// </summary>
    /// <param name="playlistId">Optional. The unique identifier of the playlist. Defaults to a random Guid.</param>
    /// <param name="paginationData">Optional. The object containing the requested pagination data. Defaults to a random page.</param>
    /// <param name="includePlaylistId">Whether the playlist Id should be included, or forced to <see langword="null"/>.</param>
    /// <param name="includePaginationData">Whether the pagination data should be included, or forced to <see langword="null"/>.</param>
    /// <returns>The created <see cref="GetPlaylistQuery"/>.</returns>
    public GetPlaylistQuery Create(
        string? playlistId = null,
        PaginationDataDto? paginationData = null,
        bool includePlaylistId = true,
        bool includePaginationData = true)
    {
        return new GetPlaylistQuery(
            includePlaylistId ? (playlistId ?? Guid.NewGuid().ToString()) : null,
            includePaginationData ? (paginationData ?? _paginationDataDtoFixture.Create()) : null);
    }

    /// <summary>
    /// Creates a list of <see cref="GetPlaylistQuery"/> instances with randomized test data.
    /// </summary>
    /// <param name="count">Number of instances to create.</param>
    /// <returns>List of configured <see cref="GetPlaylistQuery"/> instances.</returns>
    public List<GetPlaylistQuery> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
