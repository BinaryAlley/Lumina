#region ========================================================================= USING =====================================================================================
using Lumina.Application.Core.MediaLibrary.AudioLibrary.MusicLibrary.Tracks.Queries.GetTrack;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Application.Fixtures.Core.MediaLibrary.AudioLibrary.MusicLibrary.Tracks.Queries.GetTrack;

/// <summary>
/// Fixture class for the <see cref="GetTrackQuery"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetTrackQueryFixture
{
    /// <summary>
    /// Creates a random valid <see cref="GetTrackQuery"/>.
    /// </summary>
    /// <param name="libraryId">Optional. The Id of the media library the track belongs to.</param>
    /// <param name="artistId">Optional. The Id of the artist the album of the track belongs to.</param>
    /// <param name="albumId">Optional. The Id of the album the track belongs to.</param>
    /// <param name="trackId">Optional. The Id of the track to get.</param>
    /// <param name="includeLibraryId">Whether the library Id should be included, or forced to <see langword="null"/>.</param>
    /// <param name="includeArtistId">Whether the artist Id should be included, or forced to <see langword="null"/>.</param>
    /// <param name="includeAlbumId">Whether the album Id should be included, or forced to <see langword="null"/>.</param>
    /// <param name="includeTrackId">Whether the track Id should be included, or forced to <see langword="null"/>.</param>
    /// <returns>The created <see cref="GetTrackQuery"/>.</returns>
    public GetTrackQuery Create(
        string? libraryId = null,
        string? artistId = null,
        string? albumId = null,
        string? trackId = null,
        bool includeLibraryId = true,
        bool includeArtistId = true,
        bool includeAlbumId = true,
        bool includeTrackId = true)
    {
        return new GetTrackQuery(
            includeLibraryId ? (libraryId ?? Guid.NewGuid().ToString()) : null,
            includeArtistId ? (artistId ?? Guid.NewGuid().ToString()) : null,
            includeAlbumId ? (albumId ?? Guid.NewGuid().ToString()) : null,
            includeTrackId ? (trackId ?? Guid.NewGuid().ToString()) : null);
    }

    /// <summary>
    /// Creates a list of <see cref="GetTrackQuery"/> instances with randomized test data.
    /// </summary>
    /// <param name="count">Number of instances to create.</param>
    /// <returns>List of configured <see cref="GetTrackQuery"/> instances.</returns>
    public List<GetTrackQuery> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
