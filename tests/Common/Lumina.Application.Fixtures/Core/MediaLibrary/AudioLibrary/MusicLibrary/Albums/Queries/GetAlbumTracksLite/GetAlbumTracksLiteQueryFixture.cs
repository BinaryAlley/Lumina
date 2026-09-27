#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DTO.Pagination;
using Lumina.Application.Core.MediaLibrary.AudioLibrary.MusicLibrary.Albums.Queries.GetAlbumTracksLite;
using Lumina.Application.Fixtures.Common.DTO.Pagination;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Application.Fixtures.Core.MediaLibrary.AudioLibrary.MusicLibrary.Albums.Queries.GetAlbumTracksLite;

/// <summary>
/// Fixture class for the <see cref="GetAlbumTracksLiteQuery"/> record.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetAlbumTracksLiteQueryFixture
{
    private readonly PaginationDataDtoFixture _paginationDataDtoFixture = new();

    /// <summary>
    /// Creates a random valid <see cref="GetAlbumTracksLiteQuery"/>.
    /// </summary>
    /// <param name="libraryId">Optional. The Id of the media library the album belongs to, taken from the route.</param>
    /// <param name="artistId">Optional. The Id of the artist the album belongs to, taken from the route.</param>
    /// <param name="albumId">Optional. The Id of the album whose tracks are retrieved, taken from the route.</param>
    /// <param name="paginationData">Optional. The pagination data of the query.</param>
    /// <param name="includeLibraryId">Whether the library Id should be included, or forced to <see langword="null"/>.</param>
    /// <param name="includeArtistId">Whether the artist Id should be included, or forced to <see langword="null"/>.</param>
    /// <param name="includeAlbumId">Whether the album Id should be included, or forced to <see langword="null"/>.</param>
    /// <param name="includePaginationData">Whether the pagination data should be included, or forced to <see langword="null"/>.</param>
    /// <returns>The created <see cref="GetAlbumTracksLiteQuery"/>.</returns>
    public GetAlbumTracksLiteQuery Create(
        string? libraryId = null,
        string? artistId = null,
        string? albumId = null,
        PaginationDataDto? paginationData = null,
        bool includeLibraryId = true,
        bool includeArtistId = true,
        bool includeAlbumId = true,
        bool includePaginationData = true)
    {
        return new GetAlbumTracksLiteQuery(
            includeLibraryId ? (libraryId ?? Guid.NewGuid().ToString()) : null,
            includeArtistId ? (artistId ?? Guid.NewGuid().ToString()) : null,
            includeAlbumId ? (albumId ?? Guid.NewGuid().ToString()) : null,
            includePaginationData ? (paginationData ?? _paginationDataDtoFixture.Create()) : null);
    }

    /// <summary>
    /// Creates a list of <see cref="GetAlbumTracksLiteQuery"/>.
    /// </summary>
    /// <param name="count">Number of instances to create.</param>
    /// <returns>List of configured <see cref="GetAlbumTracksLiteQuery"/> instances.</returns>
    public List<GetAlbumTracksLiteQuery> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
