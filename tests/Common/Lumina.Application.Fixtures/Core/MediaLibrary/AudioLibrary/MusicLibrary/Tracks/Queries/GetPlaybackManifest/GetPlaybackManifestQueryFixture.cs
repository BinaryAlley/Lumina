#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Application.Core.MediaLibrary.AudioLibrary.MusicLibrary.Tracks.Queries.GetPlaybackManifest;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Application.Fixtures.Core.MediaLibrary.AudioLibrary.MusicLibrary.Tracks.Queries.GetPlaybackManifest;

/// <summary>
/// Fixture class for the <see cref="GetPlaybackManifestQuery"/> record.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetPlaybackManifestQueryFixture
{
    private readonly Faker _faker = new();

    /// <summary>
    /// Creates a random valid query to get the playback manifest of a track.
    /// </summary>
    /// <param name="libraryId">Optional. The Id of the media library the track belongs to.</param>
    /// <param name="artistId">Optional. The Id of the artist the album of the track belongs to.</param>
    /// <param name="albumId">Optional. The Id of the album the track belongs to.</param>
    /// <param name="trackId">Optional. The Id of the track whose playback manifest is retrieved.</param>
    /// <param name="capabilities">Optional. The container and codec pairs the player advertises it can demux and decode directly.</param>
    /// <param name="streamPath">Optional. The path of the track stream, relative to the API.</param>
    /// <param name="segmentStreamPathTemplate">Optional. The path template of the segment route, relative to the API.</param>
    /// <param name="includeLibraryId">Whether the library Id should be included, or forced to <see langword="null"/>.</param>
    /// <param name="includeArtistId">Whether the artist Id should be included, or forced to <see langword="null"/>.</param>
    /// <param name="includeAlbumId">Whether the album Id should be included, or forced to <see langword="null"/>.</param>
    /// <param name="includeTrackId">Whether the track Id should be included, or forced to <see langword="null"/>.</param>
    /// <param name="includeCapabilities">Whether the capabilities should be included, or forced to <see langword="null"/>.</param>
    /// <returns>The created <see cref="GetPlaybackManifestQuery"/>.</returns>
    public GetPlaybackManifestQuery Create(
        string? libraryId = null,
        string? artistId = null,
        string? albumId = null,
        string? trackId = null,
        string? capabilities = null,
        string? streamPath = null,
        string? segmentStreamPathTemplate = null,
        bool includeLibraryId = true,
        bool includeArtistId = true,
        bool includeAlbumId = true,
        bool includeTrackId = true,
        bool includeCapabilities = true)
    {
        return new GetPlaybackManifestQuery(
            includeLibraryId ? (libraryId ?? Guid.NewGuid().ToString()) : null,
            includeArtistId ? (artistId ?? Guid.NewGuid().ToString()) : null,
            includeAlbumId ? (albumId ?? Guid.NewGuid().ToString()) : null,
            includeTrackId ? (trackId ?? Guid.NewGuid().ToString()) : null,
            includeCapabilities ? (capabilities ?? "mp3/mp3,flac/flac") : null,
            streamPath ?? "/api/v1/music/libraries/" + _faker.Random.AlphaNumeric(8) + "/tracks/" + _faker.Random.AlphaNumeric(8) + "/stream",
            segmentStreamPathTemplate ?? "/api/v1/music/libraries/" + _faker.Random.AlphaNumeric(8) + "/tracks/" + _faker.Random.AlphaNumeric(8) + "/stream/segments/{deliveryId}/{segmentIndex}");
    }

    /// <summary>
    /// Creates a list of <see cref="GetPlaybackManifestQuery"/> instances.
    /// </summary>
    /// <param name="count">Number of instances to create.</param>
    /// <returns>List of configured <see cref="GetPlaybackManifestQuery"/> instances.</returns>
    public List<GetPlaybackManifestQuery> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
