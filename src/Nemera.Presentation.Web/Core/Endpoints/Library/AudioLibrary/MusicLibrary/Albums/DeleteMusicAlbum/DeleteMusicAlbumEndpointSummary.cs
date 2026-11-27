#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Nemera.Presentation.Web.Core.Endpoints.Library.AudioLibrary.MusicLibrary.Albums.DeleteMusicAlbum;

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
        Description = "Soft deletes the music album identified by the route, so its metadata, artwork and track paths are kept and the album can be restored.";

        Response(200, "The music album was deleted.");
    }
}
