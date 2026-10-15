#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.CQRS;
using System.Diagnostics;
#endregion

namespace Lumina.Application.Core.MediaLibrary.AudioLibrary.MusicLibrary.Tracks.Queries.GetPlaybackManifest;

/// <summary>
/// Query for getting the playback manifest of a track.
/// </summary>
/// <param name="LibraryId">The unique identifier of the media library the track belongs to, taken from the route.</param>
/// <param name="ArtistId">The unique identifier of the artist the album of the track belongs to, taken from the route.</param>
/// <param name="AlbumId">The unique identifier of the album the track belongs to, taken from the route.</param>
/// <param name="TrackId">The unique identifier of the track whose playback manifest is retrieved, taken from the route.</param>
/// <param name="Capabilities">The container and codec pairs the player advertises it can demux and decode directly, taken from the request, or <see langword="null"/> when it advertises none.</param>
/// <param name="StreamPath">The path of the track stream, relative to the API, taken from the endpoint so that it always matches the stream route.</param>
[DebuggerDisplay("TrackId: {TrackId}")]
public record GetPlaybackManifestQuery(
    string? LibraryId,
    string? ArtistId,
    string? AlbumId,
    string? TrackId,
    string? Capabilities,
    string StreamPath
) : IQuery;
