#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Presentation.Web.Common.Requests.Library.AudioLibrary.MusicLibrary.Artists;
using System;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Presentation.Web.Core.Endpoints.Library.AudioLibrary.MusicLibrary.Artists.EditMusicArtist;

/// <summary>
/// Class used for providing a textual description for the <see cref="EditMusicArtistViewEndpoint"/> endpoint, for OpenAPI.
/// </summary>
[ExcludeFromCodeCoverage]
public class EditMusicArtistViewEndpointSummary : Summary<EditMusicArtistViewEndpoint, GetMusicArtistViewRequest>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="EditMusicArtistViewEndpointSummary"/> class.
    /// </summary>
    public EditMusicArtistViewEndpointSummary()
    {
        Summary = "Renders the music artist details and editing view.";
        Description = "Renders the view for displaying and editing the music artist identified by the route.";

        RequestParam(r => r.ArtistId, "The unique identifier of the artist to display. Required.");
        RequestParam(r => r.LibraryId, "The unique identifier of the media library the artist belongs to. Required.");

        Response(200, "The view for displaying and editing the music artist is rendered.");
    }
}
