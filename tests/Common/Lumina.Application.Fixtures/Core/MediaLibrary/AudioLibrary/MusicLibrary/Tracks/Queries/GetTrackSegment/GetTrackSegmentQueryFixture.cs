#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Application.Core.MediaLibrary.AudioLibrary.MusicLibrary.Tracks.Queries.GetTrackSegment;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Application.Fixtures.Core.MediaLibrary.AudioLibrary.MusicLibrary.Tracks.Queries.GetTrackSegment;

/// <summary>
/// Fixture class for the <see cref="GetTrackSegmentQuery"/> record.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetTrackSegmentQueryFixture
{
    private readonly Faker _faker = new();

    /// <summary>
    /// Creates a random valid query to get a single segment of the normalized audio of a track.
    /// </summary>
    /// <param name="libraryId">Optional. The Id of the media library the track belongs to.</param>
    /// <param name="artistId">Optional. The Id of the artist the album of the track belongs to.</param>
    /// <param name="albumId">Optional. The Id of the album the track belongs to.</param>
    /// <param name="trackId">Optional. The Id of the track whose normalized audio is streamed.</param>
    /// <param name="deliveryId">Optional. The opaque handle of the normalized delivery the segment belongs to.</param>
    /// <param name="segmentIndex">Optional. The zero based index of the segment of the normalized audio.</param>
    /// <param name="includeLibraryId">Whether the library Id should be included, or forced to <see langword="null"/>.</param>
    /// <param name="includeArtistId">Whether the artist Id should be included, or forced to <see langword="null"/>.</param>
    /// <param name="includeAlbumId">Whether the album Id should be included, or forced to <see langword="null"/>.</param>
    /// <param name="includeTrackId">Whether the track Id should be included, or forced to <see langword="null"/>.</param>
    /// <param name="includeDeliveryId">Whether the delivery Id should be included, or forced to <see langword="null"/>.</param>
    /// <param name="includeSegmentIndex">Whether the segment index should be included, or forced to <see langword="null"/>.</param>
    /// <returns>The created <see cref="GetTrackSegmentQuery"/>.</returns>
    public GetTrackSegmentQuery Create(
        string? libraryId = null,
        string? artistId = null,
        string? albumId = null,
        string? trackId = null,
        string? deliveryId = null,
        string? segmentIndex = null,
        bool includeLibraryId = true,
        bool includeArtistId = true,
        bool includeAlbumId = true,
        bool includeTrackId = true,
        bool includeDeliveryId = true,
        bool includeSegmentIndex = true)
    {
        return new GetTrackSegmentQuery(
            includeLibraryId ? (libraryId ?? Guid.NewGuid().ToString()) : null,
            includeArtistId ? (artistId ?? Guid.NewGuid().ToString()) : null,
            includeAlbumId ? (albumId ?? Guid.NewGuid().ToString()) : null,
            includeTrackId ? (trackId ?? Guid.NewGuid().ToString()) : null,
            includeDeliveryId ? (deliveryId ?? Guid.NewGuid().ToString("N")) : null,
            includeSegmentIndex ? (segmentIndex ?? _faker.Random.Int(0, 1000).ToString()) : null);
    }

    /// <summary>
    /// Creates a list of <see cref="GetTrackSegmentQuery"/> instances.
    /// </summary>
    /// <param name="count">Number of instances to create.</param>
    /// <returns>List of configured <see cref="GetTrackSegmentQuery"/> instances.</returns>
    public List<GetTrackSegmentQuery> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
