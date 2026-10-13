#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Presentation.Web.Common.Requests.Library.AudioLibrary.MusicLibrary.Tracks;
using System;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Presentation.Web.Core.Endpoints.Library.AudioLibrary.MusicLibrary.Tracks.EditMusicTrack;

/// <summary>
/// Class used for providing a textual description for the <see cref="EditMusicTrackViewEndpoint"/> endpoint, for OpenAPI.
/// </summary>
[ExcludeFromCodeCoverage]
public class EditMusicTrackViewEndpointSummary : Summary<EditMusicTrackViewEndpoint, GetMusicTrackViewRequest>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="EditMusicTrackViewEndpointSummary"/> class.
    /// </summary>
    public EditMusicTrackViewEndpointSummary()
    {
        Summary = "Renders the music track details and editing view.";
        Description = "Renders the view for displaying and editing the music track identified by the route.";

        RequestParam(r => r.ArtistId, "The unique identifier of the artist the album belongs to. Required.");
        RequestParam(r => r.AlbumId, "The unique identifier of the album the track belongs to. Required.");
        RequestParam(r => r.TrackId, "The unique identifier of the track to display. Required.");
        RequestParam(r => r.LibraryId, "The unique identifier of the media library the track belongs to. Required.");

        Response(200, "The view for displaying and editing the music track is rendered.");
    }
}
