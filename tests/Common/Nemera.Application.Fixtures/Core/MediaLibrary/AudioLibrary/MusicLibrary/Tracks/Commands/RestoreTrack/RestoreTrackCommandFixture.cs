#region ========================================================================= USING =====================================================================================
using Nemera.Application.Core.MediaLibrary.AudioLibrary.MusicLibrary.Tracks.Commands.RestoreTrack;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Nemera.Application.Fixtures.Core.MediaLibrary.AudioLibrary.MusicLibrary.Tracks.Commands.RestoreTrack;

/// <summary>
/// Fixture class for the <see cref="RestoreTrackCommand"/> record.
/// </summary>
[ExcludeFromCodeCoverage]
public class RestoreTrackCommandFixture
{
    /// <summary>
    /// Creates a random valid <see cref="RestoreTrackCommand"/>.
    /// </summary>
    /// <param name="libraryId">Optional. The Id of the media library the track belongs to, taken from the route.</param>
    /// <param name="artistId">Optional. The Id of the artist the album of the track belongs to, taken from the route.</param>
    /// <param name="albumId">Optional. The Id of the album the track belongs to, taken from the route.</param>
    /// <param name="trackId">Optional. The Id of the deleted track to restore, taken from the route.</param>
    /// <param name="includeLibraryId">Whether the library Id should be included, or forced to <see langword="null"/>.</param>
    /// <param name="includeArtistId">Whether the artist Id should be included, or forced to <see langword="null"/>.</param>
    /// <param name="includeAlbumId">Whether the album Id should be included, or forced to <see langword="null"/>.</param>
    /// <param name="includeTrackId">Whether the track Id should be included, or forced to <see langword="null"/>.</param>
    /// <returns>The created <see cref="RestoreTrackCommand"/>.</returns>
    public RestoreTrackCommand Create(
        string? libraryId = null,
        string? artistId = null,
        string? albumId = null,
        string? trackId = null,
        bool includeLibraryId = true,
        bool includeArtistId = true,
        bool includeAlbumId = true,
        bool includeTrackId = true)
    {
        return new RestoreTrackCommand(
            includeLibraryId ? (libraryId ?? Guid.NewGuid().ToString()) : null,
            includeArtistId ? (artistId ?? Guid.NewGuid().ToString()) : null,
            includeAlbumId ? (albumId ?? Guid.NewGuid().ToString()) : null,
            includeTrackId ? (trackId ?? Guid.NewGuid().ToString()) : null);
    }

    /// <summary>
    /// Creates a list of <see cref="RestoreTrackCommand"/>.
    /// </summary>
    /// <param name="count">Number of instances to create.</param>
    /// <returns>List of configured <see cref="RestoreTrackCommand"/> instances.</returns>
    public List<RestoreTrackCommand> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
