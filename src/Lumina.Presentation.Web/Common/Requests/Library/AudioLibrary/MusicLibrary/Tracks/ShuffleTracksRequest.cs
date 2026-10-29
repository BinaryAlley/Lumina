#region ========================================================================= USING =====================================================================================
using System;
using System.Collections.Generic;
using System.Diagnostics;
#endregion

namespace Lumina.Presentation.Web.Common.Requests.Library.AudioLibrary.MusicLibrary.Tracks;

/// <summary>
/// Represents a request to draw a random selection of tracks from a media library, an artist or an album.
/// </summary>
/// <param name="LibraryId">The unique identifier of the media library the tracks are sampled from. Required.</param>
/// <param name="Count">The maximum number of tracks to return. Required.</param>
/// <param name="ArtistId">The unique identifier of the artist whose tracks are sampled. Optional.</param>
/// <param name="AlbumId">The unique identifier of the album whose tracks are sampled. Optional.</param>
/// <param name="ExcludedTrackIds">The tracks that must not be returned, usually the ones already in the queue. Optional.</param>
[DebuggerDisplay("LibraryId: {LibraryId} Count: {Count}")]
public record ShuffleTracksRequest(
    Guid LibraryId,
    int Count,
    Guid? ArtistId,
    Guid? AlbumId,
    List<Guid>? ExcludedTrackIds
);
