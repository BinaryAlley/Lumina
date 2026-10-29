#region ========================================================================= USING =====================================================================================
using System;
using System.Diagnostics;
#endregion

namespace Lumina.Presentation.Web.Common.Requests.Library.AudioLibrary.MusicLibrary.Tracks;

/// <summary>
/// Represents a request to stream one segment of the normalized audio of a track.
/// </summary>
/// <param name="ArtistId">The unique identifier of the artist the album belongs to, taken from the route. Required.</param>
/// <param name="AlbumId">The unique identifier of the album the track belongs to, taken from the route. Required.</param>
/// <param name="TrackId">The unique identifier of the track to stream, taken from the route. Required.</param>
/// <param name="DeliveryId">The opaque handle of the normalized delivery, taken from the route. Required.</param>
/// <param name="SegmentIndex">The zero based index of the segment of the normalized audio, taken from the route. Required.</param>
/// <param name="LibraryId">The Id of the media library the track belongs to. Required.</param>
[DebuggerDisplay("TrackId: {TrackId} DeliveryId: {DeliveryId} SegmentIndex: {SegmentIndex}")]
public record StreamMusicTrackSegmentRequest(
    string ArtistId,
    string AlbumId,
    string TrackId,
    string DeliveryId,
    int SegmentIndex,
    Guid LibraryId
);
