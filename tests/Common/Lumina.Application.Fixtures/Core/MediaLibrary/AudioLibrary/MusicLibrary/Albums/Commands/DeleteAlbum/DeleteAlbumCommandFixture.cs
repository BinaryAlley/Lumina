#region ========================================================================= USING =====================================================================================
using Lumina.Application.Core.MediaLibrary.AudioLibrary.MusicLibrary.Albums.Commands.DeleteAlbum;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Application.Fixtures.Core.MediaLibrary.AudioLibrary.MusicLibrary.Albums.Commands.DeleteAlbum;

/// <summary>
/// Fixture class for the <see cref="DeleteAlbumCommand"/> record.
/// </summary>
[ExcludeFromCodeCoverage]
public class DeleteAlbumCommandFixture
{
    /// <summary>
    /// Creates a random valid <see cref="DeleteAlbumCommand"/>.
    /// </summary>
    /// <param name="libraryId">Optional. The Id of the media library the album belongs to, taken from the route.</param>
    /// <param name="artistId">Optional. The Id of the artist the album belongs to, taken from the route.</param>
    /// <param name="albumId">Optional. The Id of the album to delete, taken from the route.</param>
    /// <param name="includeLibraryId">Whether the library Id should be included, or forced to <see langword="null"/>.</param>
    /// <param name="includeArtistId">Whether the artist Id should be included, or forced to <see langword="null"/>.</param>
    /// <param name="includeAlbumId">Whether the album Id should be included, or forced to <see langword="null"/>.</param>
    /// <returns>The created <see cref="DeleteAlbumCommand"/>.</returns>
    public DeleteAlbumCommand Create(
        string? libraryId = null,
        string? artistId = null,
        string? albumId = null,
        bool includeLibraryId = true,
        bool includeArtistId = true,
        bool includeAlbumId = true)
    {
        return new DeleteAlbumCommand(
            includeLibraryId ? (libraryId ?? Guid.NewGuid().ToString()) : null,
            includeArtistId ? (artistId ?? Guid.NewGuid().ToString()) : null,
            includeAlbumId ? (albumId ?? Guid.NewGuid().ToString()) : null);
    }

    /// <summary>
    /// Creates a list of <see cref="DeleteAlbumCommand"/>.
    /// </summary>
    /// <param name="count">Number of instances to create.</param>
    /// <returns>List of configured <see cref="DeleteAlbumCommand"/> instances.</returns>
    public List<DeleteAlbumCommand> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
