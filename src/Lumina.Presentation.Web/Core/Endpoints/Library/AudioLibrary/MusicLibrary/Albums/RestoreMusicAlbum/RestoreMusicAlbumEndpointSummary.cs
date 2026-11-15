#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Presentation.Web.Core.Endpoints.Library.AudioLibrary.MusicLibrary.Albums.RestoreMusicAlbum;

/// <summary>
/// Class used for providing a textual description for the <see cref="RestoreMusicAlbumEndpoint"/> endpoint, for OpenAPI.
/// </summary>
[ExcludeFromCodeCoverage]
public class RestoreMusicAlbumEndpointSummary : Summary<RestoreMusicAlbumEndpoint>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="RestoreMusicAlbumEndpointSummary"/> class.
    /// </summary>
    public RestoreMusicAlbumEndpointSummary()
    {
        Summary = "Restores a deleted music album.";
        Description = "Restores the deleted music album identified by the route, together with the metadata and artwork kept for it.";

        Response(200, "The music album was restored.");
    }
}
