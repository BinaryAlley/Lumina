#region ========================================================================= USING =====================================================================================
using Lumina.Presentation.Web.Common.Requests.Library.AudioLibrary.MusicLibrary.Tracks;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Presentation.Web.Fixtures.Common.Requests.Library.AudioLibrary.MusicLibrary.Tracks;

/// <summary>
/// Fixture class for the <see cref="StreamMusicTrackRequest"/> record.
/// </summary>
[ExcludeFromCodeCoverage]
public class StreamMusicTrackRequestFixture
{
    /// <summary>
    /// Creates a new <see cref="StreamMusicTrackRequest"/> instance.
    /// </summary>
    /// <param name="artistId">Optional unique identifier of the artist the album belongs to.</param>
    /// <param name="albumId">Optional unique identifier of the album the track belongs to.</param>
    /// <param name="trackId">Optional unique identifier of the track to stream.</param>
    /// <param name="libraryId">Optional unique identifier of the media library the track belongs to.</param>
    /// <param name="includeArtistId">Whether the artist Id should be included, or forced to <see langword="null"/>.</param>
    /// <param name="includeAlbumId">Whether the album Id should be included, or forced to <see langword="null"/>.</param>
    /// <param name="includeTrackId">Whether the track Id should be included, or forced to <see langword="null"/>.</param>
    /// <returns>A configured <see cref="StreamMusicTrackRequest"/> instance.</returns>
    public StreamMusicTrackRequest Create(
        string? artistId = null,
        string? albumId = null,
        string? trackId = null,
        Guid? libraryId = null,
        bool includeArtistId = true,
        bool includeAlbumId = true,
        bool includeTrackId = true)
    {
        return new StreamMusicTrackRequest(
            includeArtistId ? (artistId ?? Guid.NewGuid().ToString()) : null!,
            includeAlbumId ? (albumId ?? Guid.NewGuid().ToString()) : null!,
            includeTrackId ? (trackId ?? Guid.NewGuid().ToString()) : null!,
            libraryId ?? Guid.NewGuid());
    }

    /// <summary>
    /// Creates a list of <see cref="StreamMusicTrackRequest"/> instances.
    /// </summary>
    /// <param name="count">Number of instances to create.</param>
    /// <returns>List of configured <see cref="StreamMusicTrackRequest"/> instances.</returns>
    public List<StreamMusicTrackRequest> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
