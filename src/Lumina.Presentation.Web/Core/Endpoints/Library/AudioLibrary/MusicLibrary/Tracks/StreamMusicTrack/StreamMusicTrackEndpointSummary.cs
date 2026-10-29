#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Presentation.Web.Common.Requests.Library.AudioLibrary.MusicLibrary.Tracks;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Presentation.Web.Core.Endpoints.Library.AudioLibrary.MusicLibrary.Tracks.StreamMusicTrack;

/// <summary>
/// Class used for providing a textual description for the <see cref="StreamMusicTrackEndpoint"/> endpoint, for OpenAPI.
/// </summary>
[ExcludeFromCodeCoverage]
public class StreamMusicTrackEndpointSummary : Summary<StreamMusicTrackEndpoint, StreamMusicTrackRequest>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="StreamMusicTrackEndpointSummary"/> class.
    /// </summary>
    public StreamMusicTrackEndpointSummary()
    {
        Summary = "Streams the audio file of a music track.";
        Description = "Streams the audio file of the music track identified by the route, forwarding the range and the conditional headers the browser sends, so that the player can start playback before the whole file is downloaded, resume a partial download, and revalidate a cached copy.";

        RequestParam(r => r.ArtistId, "The unique identifier of the artist the album belongs to. Required.");
        RequestParam(r => r.AlbumId, "The unique identifier of the album the track belongs to. Required.");
        RequestParam(r => r.TrackId, "The unique identifier of the track to stream. Required.");
        RequestParam(r => r.LibraryId, "The unique identifier of the media library the track belongs to. Required.");

        Response(200, "The whole audio file of the track is streamed.");
        Response(206, "The requested range of the audio file of the track is streamed, with its content range.");
        Response(304, "The audio file of the track did not change since the copy the browser already holds, and its validators are returned.");
        Response(416, "The requested range cannot be satisfied, and the total length of the audio file is returned in the content range.");
        Response(401, "The user is not authenticated.");
        Response(403, "The user is not allowed to stream the track of the media library.");
        Response(404, "The track, the album, the artist, or the media library was not found, or is out of the scope of the route.");
        Response(429, "The user is already holding the maximum number of concurrent track streams.");
        Response(502, "The remote API did not return the audio file of the track.");
    }
}
