#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.CQRS;
using System.Diagnostics;
#endregion

namespace Lumina.Application.Core.MediaLibrary.AudioLibrary.MusicLibrary.Tracks.Queries.GetTrackSegment;

/// <summary>
/// Query for getting a single segment of the normalized audio of a track, for streaming.
/// </summary>
/// <param name="LibraryId">The unique identifier of the media library the track belongs to, taken from the route.</param>
/// <param name="ArtistId">The unique identifier of the artist the album of the track belongs to, taken from the route.</param>
/// <param name="AlbumId">The unique identifier of the album the track belongs to, taken from the route.</param>
/// <param name="TrackId">The unique identifier of the track whose normalized audio is streamed, taken from the route.</param>
/// <param name="DeliveryId">The opaque handle of the normalized delivery the segment belongs to, taken from the route.</param>
/// <param name="SegmentIndex">The zero based index of the segment of the normalized audio, taken from the route.</param>
[DebuggerDisplay("TrackId: {TrackId} DeliveryId: {DeliveryId} SegmentIndex: {SegmentIndex}")]
public record GetTrackSegmentQuery(
    string? LibraryId,
    string? ArtistId,
    string? AlbumId,
    string? TrackId,
    string? DeliveryId,
    string? SegmentIndex
) : IQuery;
