#region ========================================================================= USING =====================================================================================
using Lumina.Presentation.Web.Common.Requests.Library.AudioLibrary.MusicLibrary.Tracks;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Presentation.Web.Fixtures.Common.Requests.Library.AudioLibrary.MusicLibrary.Tracks;

/// <summary>
/// Fixture class for the <see cref="GetAlbumTracksLiteRequest"/> record.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetAlbumTracksLiteRequestFixture
{
    /// <summary>
    /// Creates a new <see cref="GetAlbumTracksLiteRequest"/> instance with randomized test data.
    /// </summary>
    /// <param name="artistId">Optional Id of the artist the album belongs to.</param>
    /// <param name="albumId">Optional Id of the album whose tracks are retrieved.</param>
    /// <param name="libraryId">Optional Id of the media library the album belongs to.</param>
    /// <param name="currentPage">Optional page of results to retrieve.</param>
    /// <param name="perPage">Optional maximum number of tracks to retrieve per page.</param>
    /// <returns>A configured <see cref="GetAlbumTracksLiteRequest"/> instance.</returns>
    public GetAlbumTracksLiteRequest Create(
        string? artistId = null,
        string? albumId = null,
        Guid? libraryId = null,
        int? currentPage = null,
        int? perPage = null)
    {
        return new GetAlbumTracksLiteRequest(
            ArtistId: artistId ?? Guid.NewGuid().ToString(),
            AlbumId: albumId ?? Guid.NewGuid().ToString(),
            LibraryId: libraryId ?? Guid.NewGuid(),
            CurrentPage: currentPage,
            PerPage: perPage
        );
    }

    /// <summary>
    /// Creates multiple <see cref="GetAlbumTracksLiteRequest"/> instances with randomized test data.
    /// </summary>
    /// <param name="count">Number of instances to create.</param>
    /// <returns>List of configured <see cref="GetAlbumTracksLiteRequest"/> instances.</returns>
    public List<GetAlbumTracksLiteRequest> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
