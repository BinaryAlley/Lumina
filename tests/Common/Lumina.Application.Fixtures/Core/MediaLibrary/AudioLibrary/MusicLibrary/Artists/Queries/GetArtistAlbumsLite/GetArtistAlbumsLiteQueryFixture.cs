#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DTO.Pagination;
using Lumina.Application.Core.MediaLibrary.AudioLibrary.MusicLibrary.Artists.Queries.GetArtistAlbumsLite;
using Lumina.Application.Fixtures.Common.DTO.Pagination;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Application.Fixtures.Core.MediaLibrary.AudioLibrary.MusicLibrary.Artists.Queries.GetArtistAlbumsLite;

/// <summary>
/// Fixture class for the <see cref="GetArtistAlbumsLiteQuery"/> record.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetArtistAlbumsLiteQueryFixture
{
    private readonly PaginationDataDtoFixture _paginationDataDtoFixture = new();

    /// <summary>
    /// Creates a random valid <see cref="GetArtistAlbumsLiteQuery"/>.
    /// </summary>
    /// <param name="libraryId">Optional. The Id of the media library the artist belongs to, taken from the route.</param>
    /// <param name="artistId">Optional. The Id of the artist whose albums are retrieved, taken from the route.</param>
    /// <param name="paginationData">Optional. The pagination data of the query.</param>
    /// <param name="includeLibraryId">Whether the library Id should be included, or forced to <see langword="null"/>.</param>
    /// <param name="includeArtistId">Whether the artist Id should be included, or forced to <see langword="null"/>.</param>
    /// <param name="includePaginationData">Whether the pagination data should be included, or forced to <see langword="null"/>.</param>
    /// <returns>The created <see cref="GetArtistAlbumsLiteQuery"/>.</returns>
    public GetArtistAlbumsLiteQuery Create(
        string? libraryId = null,
        string? artistId = null,
        PaginationDataDto? paginationData = null,
        bool includeLibraryId = true,
        bool includeArtistId = true,
        bool includePaginationData = true)
    {
        return new GetArtistAlbumsLiteQuery(
            includeLibraryId ? (libraryId ?? Guid.NewGuid().ToString()) : null,
            includeArtistId ? (artistId ?? Guid.NewGuid().ToString()) : null,
            includePaginationData ? (paginationData ?? _paginationDataDtoFixture.Create()) : null);
    }

    /// <summary>
    /// Creates a list of <see cref="GetArtistAlbumsLiteQuery"/>.
    /// </summary>
    /// <param name="count">Number of instances to create.</param>
    /// <returns>List of configured <see cref="GetArtistAlbumsLiteQuery"/> instances.</returns>
    public List<GetArtistAlbumsLiteQuery> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
