#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Presentation.Web.Common.Requests.Library.AudioLibrary.MusicLibrary.Albums;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Presentation.Web.Core.Endpoints.Library.AudioLibrary.MusicLibrary.Albums.EditMusicAlbum;

/// <summary>
/// Class used for providing a textual description for the <see cref="EditMusicAlbumViewEndpoint"/> endpoint, for OpenAPI.
/// </summary>
[ExcludeFromCodeCoverage]
public class EditMusicAlbumViewEndpointSummary : Summary<EditMusicAlbumViewEndpoint, GetMusicAlbumViewRequest>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="EditMusicAlbumViewEndpointSummary"/> class.
    /// </summary>
    public EditMusicAlbumViewEndpointSummary()
    {
        Summary = "Renders the music album details and editing view.";
        Description = "Renders the view for displaying and editing the music album identified by the route.";

        RequestParam(r => r.ArtistId, "The unique identifier of the artist the album belongs to. Required.");
        RequestParam(r => r.AlbumId, "The unique identifier of the album to display. Required.");
        RequestParam(r => r.LibraryId, "The unique identifier of the media library the album belongs to. Required.");

        Response(200, "The view for displaying and editing the music album is rendered.");
    }
}
