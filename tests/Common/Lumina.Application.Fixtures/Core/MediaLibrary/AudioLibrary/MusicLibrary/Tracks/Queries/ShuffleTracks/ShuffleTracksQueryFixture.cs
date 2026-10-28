#region ========================================================================= USING =====================================================================================
using Lumina.Application.Core.MediaLibrary.AudioLibrary.MusicLibrary.Tracks.Queries.ShuffleTracks;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Application.Fixtures.Core.MediaLibrary.AudioLibrary.MusicLibrary.Tracks.Queries.ShuffleTracks;

/// <summary>
/// Fixture class for the <see cref="ShuffleTracksQuery"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class ShuffleTracksQueryFixture
{
    /// <summary>
    /// Creates a random valid <see cref="ShuffleTracksQuery"/>.
    /// </summary>
    /// <param name="libraryId">Optional. The Id of the media library the tracks are sampled from, taken from the route.</param>
    /// <param name="count">Optional. The maximum number of tracks to return.</param>
    /// <param name="artistId">Optional. The Id of the artist whose tracks are sampled.</param>
    /// <param name="albumId">Optional. The Id of the album whose tracks are sampled.</param>
    /// <param name="excludedTrackIds">Optional. The tracks that must not be returned.</param>
    /// <param name="includeLibraryId">Whether the library Id should be included, or forced to <see langword="null"/>.</param>
    /// <param name="includeArtistId">Whether the artist Id should be included, or forced to <see langword="null"/>.</param>
    /// <param name="includeAlbumId">Whether the album Id should be included, or forced to <see langword="null"/>.</param>
    /// <param name="includeExcludedTrackIds">Whether the excluded track Ids should be included, or forced to <see langword="null"/>.</param>
    /// <returns>The created <see cref="ShuffleTracksQuery"/>.</returns>
    public ShuffleTracksQuery Create(
        string? libraryId = null,
        int? count = null,
        Guid? artistId = null,
        Guid? albumId = null,
        IReadOnlyList<Guid>? excludedTrackIds = null,
        bool includeLibraryId = true,
        bool includeArtistId = true,
        bool includeAlbumId = true,
        bool includeExcludedTrackIds = true)
    {
        return new ShuffleTracksQuery(
            includeLibraryId ? (libraryId ?? Guid.NewGuid().ToString()) : null,
            count ?? Random.Shared.Next(1, 501),
            includeArtistId ? (artistId ?? Guid.NewGuid()) : null,
            includeAlbumId ? (albumId ?? Guid.NewGuid()) : null,
            includeExcludedTrackIds
                ? (excludedTrackIds ?? [.. Enumerable.Range(0, Random.Shared.Next(0, 3)).Select(_ => Guid.NewGuid())])
                : null);
    }

    /// <summary>
    /// Creates a list of <see cref="ShuffleTracksQuery"/> instances with randomized test data.
    /// </summary>
    /// <param name="count">Number of instances to create.</param>
    /// <returns>List of configured <see cref="ShuffleTracksQuery"/> instances.</returns>
    public List<ShuffleTracksQuery> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
