#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Presentation.Web.Common.Requests.Library.AudioLibrary.MusicLibrary.Tracks;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Presentation.Web.Core.Endpoints.Library.AudioLibrary.MusicLibrary.Tracks.StreamMusicTrackSegment;

/// <summary>
/// Class used for providing a textual description for the <see cref="StreamMusicTrackSegmentEndpoint"/> endpoint, for OpenAPI.
/// </summary>
[ExcludeFromCodeCoverage]
public class StreamMusicTrackSegmentEndpointSummary : Summary<StreamMusicTrackSegmentEndpoint, StreamMusicTrackSegmentRequest>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="StreamMusicTrackSegmentEndpointSummary"/> class.
    /// </summary>
    public StreamMusicTrackSegmentEndpointSummary()
    {
        Summary = "Streams one segment of the normalized audio of a music track.";
        Description = "Streams one fixed duration segment of the normalized audio of the music track identified by the route, through the API. A track the player cannot play directly is delivered as time addressed segments, so the player seeks by segment index instead of by byte range. The delivery handle is the opaque value the playback manifest reported, and it binds the segment to exactly the delivery that manifest declared.";

        RequestParam(r => r.ArtistId, "The unique identifier of the artist the album belongs to. Required.");
        RequestParam(r => r.AlbumId, "The unique identifier of the album the track belongs to. Required.");
        RequestParam(r => r.TrackId, "The unique identifier of the track to stream. Required.");
        RequestParam(r => r.DeliveryId, "The opaque handle of the normalized delivery, reported by the playback manifest. Required.");
        RequestParam(r => r.SegmentIndex, "The zero based index of the segment of the normalized audio. Required.");
        RequestParam(r => r.LibraryId, "The unique identifier of the media library the track belongs to. Required.");

        Response(200, "The requested segment of the normalized audio of the track is streamed.");
        Response(401, "The user is not authenticated.");
        Response(403, "The user is not allowed to stream the track of the media library.");
        Response(404, "The track, the album, the artist, the media library, the delivery handle, or the requested segment was not found, or is out of the scope of the route.");
        Response(409, "The delivery handle no longer describes the delivery, or the source file changed. The player requests the manifest again.");
        Response(429, "The user is already holding the maximum number of concurrent track streams.");
        Response(502, "The remote API did not return the segment of the normalized audio of the track.");
        Response(503, "The normalization pipeline is temporarily busy. The player retries the request.");
    }
}
