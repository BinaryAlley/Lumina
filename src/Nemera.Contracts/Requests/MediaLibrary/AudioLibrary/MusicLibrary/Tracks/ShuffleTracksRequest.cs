#region ========================================================================= USING =====================================================================================
using System;
using System.Collections.Generic;
using System.Diagnostics;
#endregion

namespace Lumina.Contracts.Requests.MediaLibrary.AudioLibrary.MusicLibrary.Tracks;

/// <summary>
/// Represents a request to draw a random selection of tracks from a media library, an artist or an album.
/// </summary>
/// <param name="Count">The maximum number of tracks to return. Required.</param>
/// <param name="ArtistId">The unique identifier of the artist whose tracks are sampled. Optional.</param>
/// <param name="AlbumId">The unique identifier of the album whose tracks are sampled. Optional.</param>
/// <param name="ExcludedTrackIds">The tracks that must not be returned, usually the ones already in the queue. Optional.</param>
[DebuggerDisplay("Count: {Count}")]
public record ShuffleTracksRequest(
    int Count,
    Guid? ArtistId,
    Guid? AlbumId,
    List<Guid>? ExcludedTrackIds
);
