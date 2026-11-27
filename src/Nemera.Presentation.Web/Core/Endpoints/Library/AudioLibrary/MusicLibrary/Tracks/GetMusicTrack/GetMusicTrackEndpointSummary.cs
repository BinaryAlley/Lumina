#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Presentation.Web.Common.Requests.Library.AudioLibrary.MusicLibrary.Tracks;
using System;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Presentation.Web.Core.Endpoints.Library.AudioLibrary.MusicLibrary.Tracks.GetMusicTrack;

/// <summary>
/// Class used for providing a textual description for the <see cref="GetMusicTrackEndpoint"/> endpoint, for OpenAPI.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetMusicTrackEndpointSummary : Summary<GetMusicTrackEndpoint, GetMusicTrackViewRequest>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GetMusicTrackEndpointSummary"/> class.
    /// </summary>
    public GetMusicTrackEndpointSummary()
    {
        Summary = "Retrieves the full details of a music track.";
        Description = "Retrieves the full details of the music track identified by the route.";

        RequestParam(r => r.ArtistId, "The unique identifier of the artist the album belongs to. Required.");
        RequestParam(r => r.AlbumId, "The unique identifier of the album the track belongs to. Required.");
        RequestParam(r => r.TrackId, "The unique identifier of the track to retrieve. Required.");
        RequestParam(r => r.LibraryId, "The unique identifier of the media library the track belongs to. Required.");

        Response(200, "The full details of the music track are retrieved.");
    }
}
