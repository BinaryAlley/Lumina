#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Presentation.Web.Core.Endpoints.Library.AudioLibrary.MusicLibrary.Albums.DeleteMusicAlbum;

/// <summary>
/// Class used for providing a textual description for the <see cref="DeleteMusicAlbumEndpoint"/> endpoint, for OpenAPI.
/// </summary>
[ExcludeFromCodeCoverage]
public class DeleteMusicAlbumEndpointSummary : Summary<DeleteMusicAlbumEndpoint>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DeleteMusicAlbumEndpointSummary"/> class.
    /// </summary>
    public DeleteMusicAlbumEndpointSummary()
    {
        Summary = "Deletes a music album.";
        Description = "Deletes the music album identified by the route, together with its tracks.";

        Response(200, "The music album was deleted.");
    }
}
