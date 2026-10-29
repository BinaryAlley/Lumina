#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Presentation.Web.Common.Requests.Library.AudioLibrary.MusicLibrary.Tracks;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Presentation.Web.Core.Endpoints.Library.AudioLibrary.MusicLibrary.Tracks.GetPlaybackManifest;

/// <summary>
/// Class used for providing a textual description for the <see cref="GetPlaybackManifestEndpoint"/> endpoint, for OpenAPI.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetPlaybackManifestEndpointSummary : Summary<GetPlaybackManifestEndpoint, GetPlaybackManifestRequest>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GetPlaybackManifestEndpointSummary"/> class.
    /// </summary>
    public GetPlaybackManifestEndpointSummary()
    {
        Summary = "Retrieves the playback manifest of a music track.";
        Description = "Retrieves the playback manifest of the music track identified by the route, with the stream path and the segment path template replaced by the URLs of the streaming proxies of this application. A normalized delivery also carries the opaque delivery handle the segment proxy forwards, so the segments are addressed by the delivery the manifest declared.";

        RequestParam(r => r.ArtistId, "The unique identifier of the artist the album belongs to. Required.");
        RequestParam(r => r.AlbumId, "The unique identifier of the album the track belongs to. Required.");
        RequestParam(r => r.TrackId, "The unique identifier of the track whose manifest is requested. Required.");
        RequestParam(r => r.LibraryId, "The unique identifier of the media library the track belongs to. Required.");
        RequestParam(r => r.Capabilities, "The comma separated list of the container and codec pairs the player advertises it can play directly. Optional.");

        Response(200, "The playback manifest of the track is retrieved.");
    }
}
