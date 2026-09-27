#region ========================================================================= USING =====================================================================================
using Lumina.Application.Core.MediaLibrary.AudioLibrary.MusicLibrary.Tracks.Commands.DeleteTrack;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Application.Fixtures.Core.MediaLibrary.AudioLibrary.MusicLibrary.Tracks.Commands.DeleteTrack;

/// <summary>
/// Fixture class for the <see cref="DeleteTrackCommand"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class DeleteTrackCommandFixture
{
    /// <summary>
    /// Creates a random valid <see cref="DeleteTrackCommand"/>.
    /// </summary>
    /// <param name="libraryId">Optional. The Id of the media library the track belongs to.</param>
    /// <param name="artistId">Optional. The Id of the artist the album of the track belongs to.</param>
    /// <param name="albumId">Optional. The Id of the album the track belongs to.</param>
    /// <param name="trackId">Optional. The Id of the track to delete.</param>
    /// <param name="includeLibraryId">Whether the library Id should be included, or forced to <see langword="null"/>.</param>
    /// <param name="includeArtistId">Whether the artist Id should be included, or forced to <see langword="null"/>.</param>
    /// <param name="includeAlbumId">Whether the album Id should be included, or forced to <see langword="null"/>.</param>
    /// <param name="includeTrackId">Whether the track Id should be included, or forced to <see langword="null"/>.</param>
    /// <returns>The created <see cref="DeleteTrackCommand"/>.</returns>
    public DeleteTrackCommand Create(
        string? libraryId = null,
        string? artistId = null,
        string? albumId = null,
        string? trackId = null,
        bool includeLibraryId = true,
        bool includeArtistId = true,
        bool includeAlbumId = true,
        bool includeTrackId = true)
    {
        return new DeleteTrackCommand(
            includeLibraryId ? (libraryId ?? Guid.NewGuid().ToString()) : null,
            includeArtistId ? (artistId ?? Guid.NewGuid().ToString()) : null,
            includeAlbumId ? (albumId ?? Guid.NewGuid().ToString()) : null,
            includeTrackId ? (trackId ?? Guid.NewGuid().ToString()) : null);
    }

    /// <summary>
    /// Creates a list of <see cref="DeleteTrackCommand"/> instances with randomized test data.
    /// </summary>
    /// <param name="count">The number of elements to create.</param>
    /// <returns>The created list.</returns>
    public List<DeleteTrackCommand> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
