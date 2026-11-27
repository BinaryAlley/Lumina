#region ========================================================================= USING =====================================================================================
using Lumina.Application.Core.MediaLibrary.AudioLibrary.MusicLibrary.Albums.Queries.GetAlbum;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Application.Fixtures.Core.MediaLibrary.AudioLibrary.MusicLibrary.Albums.Queries.GetAlbum;

/// <summary>
/// Fixture class for the <see cref="GetAlbumQuery"/> record.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetAlbumQueryFixture
{
    /// <summary>
    /// Creates a random valid <see cref="GetAlbumQuery"/>.
    /// </summary>
    /// <param name="libraryId">Optional. The Id of the media library the album belongs to, taken from the route.</param>
    /// <param name="artistId">Optional. The Id of the artist the album belongs to, taken from the route.</param>
    /// <param name="albumId">Optional. The Id of the album to get, taken from the route.</param>
    /// <param name="includeLibraryId">Whether the library Id should be included, or forced to <see langword="null"/>.</param>
    /// <param name="includeArtistId">Whether the artist Id should be included, or forced to <see langword="null"/>.</param>
    /// <param name="includeAlbumId">Whether the album Id should be included, or forced to <see langword="null"/>.</param>
    /// <returns>The created <see cref="GetAlbumQuery"/>.</returns>
    public GetAlbumQuery Create(
        string? libraryId = null,
        string? artistId = null,
        string? albumId = null,
        bool includeLibraryId = true,
        bool includeArtistId = true,
        bool includeAlbumId = true)
    {
        return new GetAlbumQuery(
            includeLibraryId ? (libraryId ?? Guid.NewGuid().ToString()) : null,
            includeArtistId ? (artistId ?? Guid.NewGuid().ToString()) : null,
            includeAlbumId ? (albumId ?? Guid.NewGuid().ToString()) : null);
    }

    /// <summary>
    /// Creates a list of <see cref="GetAlbumQuery"/>.
    /// </summary>
    /// <param name="count">Number of instances to create.</param>
    /// <returns>List of configured <see cref="GetAlbumQuery"/> instances.</returns>
    public List<GetAlbumQuery> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
