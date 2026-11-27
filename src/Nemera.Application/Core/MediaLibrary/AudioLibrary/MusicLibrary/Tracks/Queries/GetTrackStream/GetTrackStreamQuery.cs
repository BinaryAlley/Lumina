#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.CQRS;
using System.Diagnostics;
#endregion

namespace Lumina.Application.Core.MediaLibrary.AudioLibrary.MusicLibrary.Tracks.Queries.GetTrackStream;

/// <summary>
/// Query for getting the audio file of a track, for streaming.
/// </summary>
/// <param name="LibraryId">The unique identifier of the media library the track belongs to, taken from the route.</param>
/// <param name="ArtistId">The unique identifier of the artist the album of the track belongs to, taken from the route.</param>
/// <param name="AlbumId">The unique identifier of the album the track belongs to, taken from the route.</param>
/// <param name="TrackId">The unique identifier of the track whose audio file is streamed, taken from the route.</param>
[DebuggerDisplay("TrackId: {TrackId}")]
public record GetTrackStreamQuery(
    string? LibraryId,
    string? ArtistId,
    string? AlbumId,
    string? TrackId
) : IQuery;
