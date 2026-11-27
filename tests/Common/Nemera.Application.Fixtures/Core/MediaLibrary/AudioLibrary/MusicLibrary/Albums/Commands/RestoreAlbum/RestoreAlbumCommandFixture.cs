#region ========================================================================= USING =====================================================================================
using Lumina.Application.Core.MediaLibrary.AudioLibrary.MusicLibrary.Albums.Commands.RestoreAlbum;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Application.Fixtures.Core.MediaLibrary.AudioLibrary.MusicLibrary.Albums.Commands.RestoreAlbum;

/// <summary>
/// Fixture class for the <see cref="RestoreAlbumCommand"/> record.
/// </summary>
[ExcludeFromCodeCoverage]
public class RestoreAlbumCommandFixture
{
    /// <summary>
    /// Creates a random valid <see cref="RestoreAlbumCommand"/>.
    /// </summary>
    /// <param name="libraryId">Optional. The Id of the media library the album belongs to, taken from the route.</param>
    /// <param name="artistId">Optional. The Id of the artist the album belongs to, taken from the route.</param>
    /// <param name="albumId">Optional. The Id of the deleted album to restore, taken from the route.</param>
    /// <param name="includeLibraryId">Whether the library Id should be included, or forced to <see langword="null"/>.</param>
    /// <param name="includeArtistId">Whether the artist Id should be included, or forced to <see langword="null"/>.</param>
    /// <param name="includeAlbumId">Whether the album Id should be included, or forced to <see langword="null"/>.</param>
    /// <returns>The created <see cref="RestoreAlbumCommand"/>.</returns>
    public RestoreAlbumCommand Create(
        string? libraryId = null,
        string? artistId = null,
        string? albumId = null,
        bool includeLibraryId = true,
        bool includeArtistId = true,
        bool includeAlbumId = true)
    {
        return new RestoreAlbumCommand(
            includeLibraryId ? (libraryId ?? Guid.NewGuid().ToString()) : null,
            includeArtistId ? (artistId ?? Guid.NewGuid().ToString()) : null,
            includeAlbumId ? (albumId ?? Guid.NewGuid().ToString()) : null);
    }

    /// <summary>
    /// Creates a list of <see cref="RestoreAlbumCommand"/>.
    /// </summary>
    /// <param name="count">Number of instances to create.</param>
    /// <returns>List of configured <see cref="RestoreAlbumCommand"/> instances.</returns>
    public List<RestoreAlbumCommand> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
