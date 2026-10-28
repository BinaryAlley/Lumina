#region ========================================================================= USING =====================================================================================
using Lumina.Presentation.Web.Common.Requests.Library.AudioLibrary.MusicLibrary.Tracks;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Presentation.Web.Fixtures.Common.Requests.Library.AudioLibrary.MusicLibrary.Tracks;

/// <summary>
/// Fixture class for the <see cref="ShuffleTracksRequest"/> record.
/// </summary>
[ExcludeFromCodeCoverage]
public class ShuffleTracksRequestFixture
{
    /// <summary>
    /// Creates a random valid <see cref="ShuffleTracksRequest"/>.
    /// </summary>
    /// <param name="libraryId">Optional. The Id of the media library the tracks are sampled from.</param>
    /// <param name="count">Optional. The maximum number of tracks to return.</param>
    /// <param name="artistId">Optional. The Id of the artist whose tracks are sampled.</param>
    /// <param name="albumId">Optional. The Id of the album whose tracks are sampled.</param>
    /// <param name="excludedTrackIds">Optional. The tracks that must not be returned.</param>
    /// <param name="includeArtistId">Whether the artist Id should be included, or forced to <see langword="null"/>.</param>
    /// <param name="includeAlbumId">Whether the album Id should be included, or forced to <see langword="null"/>.</param>
    /// <param name="includeExcludedTrackIds">Whether the excluded track Ids should be included, or forced to <see langword="null"/>.</param>
    /// <returns>The created <see cref="ShuffleTracksRequest"/>.</returns>
    public ShuffleTracksRequest Create(
        Guid? libraryId = null,
        int? count = null,
        Guid? artistId = null,
        Guid? albumId = null,
        List<Guid>? excludedTrackIds = null,
        bool includeArtistId = true,
        bool includeAlbumId = true,
        bool includeExcludedTrackIds = true)
    {
        return new ShuffleTracksRequest(
            libraryId ?? Guid.NewGuid(),
            count ?? Random.Shared.Next(1, 501),
            includeArtistId ? (artistId ?? Guid.NewGuid()) : null,
            includeAlbumId ? (albumId ?? Guid.NewGuid()) : null,
            includeExcludedTrackIds
                ? (excludedTrackIds ?? [.. Enumerable.Range(0, Random.Shared.Next(0, 3)).Select(_ => Guid.NewGuid())])
                : null);
    }

    /// <summary>
    /// Creates a list of <see cref="ShuffleTracksRequest"/> records with randomized test data.
    /// </summary>
    /// <param name="count">Number of instances to create.</param>
    /// <returns>List of configured <see cref="ShuffleTracksRequest"/> records.</returns>
    public List<ShuffleTracksRequest> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
