#region ========================================================================= USING =====================================================================================
using Lumina.Presentation.Web.Common.Requests.Library.AudioLibrary.MusicLibrary.Tracks;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Presentation.Web.Fixtures.Common.Requests.Library.AudioLibrary.MusicLibrary.Tracks;

/// <summary>
/// Fixture class for the <see cref="StreamMusicTrackSegmentRequest"/> record.
/// </summary>
[ExcludeFromCodeCoverage]
public class StreamMusicTrackSegmentRequestFixture
{
    /// <summary>
    /// Creates a new <see cref="StreamMusicTrackSegmentRequest"/> instance.
    /// </summary>
    /// <param name="artistId">Optional unique identifier of the artist the album belongs to.</param>
    /// <param name="albumId">Optional unique identifier of the album the track belongs to.</param>
    /// <param name="trackId">Optional unique identifier of the track to stream.</param>
    /// <param name="deliveryId">Optional opaque handle of the normalized delivery.</param>
    /// <param name="segmentIndex">Optional zero based index of the segment.</param>
    /// <param name="libraryId">Optional unique identifier of the media library the track belongs to.</param>
    /// <param name="includeDeliveryId">Whether the delivery handle should be included, or forced to <see langword="null"/>.</param>
    /// <returns>A configured <see cref="StreamMusicTrackSegmentRequest"/> instance.</returns>
    public StreamMusicTrackSegmentRequest Create(
        string? artistId = null,
        string? albumId = null,
        string? trackId = null,
        string? deliveryId = null,
        int? segmentIndex = null,
        Guid? libraryId = null,
        bool includeDeliveryId = true)
    {
        return new StreamMusicTrackSegmentRequest(
            artistId ?? Guid.NewGuid().ToString(),
            albumId ?? Guid.NewGuid().ToString(),
            trackId ?? Guid.NewGuid().ToString(),
            includeDeliveryId ? (deliveryId ?? Guid.NewGuid().ToString("N")) : null!,
            segmentIndex ?? Random.Shared.Next(0, 100),
            libraryId ?? Guid.NewGuid());
    }

    /// <summary>
    /// Creates a list of <see cref="StreamMusicTrackSegmentRequest"/> instances.
    /// </summary>
    /// <param name="count">Number of instances to create.</param>
    /// <returns>List of configured <see cref="StreamMusicTrackSegmentRequest"/> instances.</returns>
    public List<StreamMusicTrackSegmentRequest> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
