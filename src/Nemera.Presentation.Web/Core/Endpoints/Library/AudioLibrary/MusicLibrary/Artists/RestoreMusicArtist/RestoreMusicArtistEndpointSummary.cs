#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Presentation.Web.Core.Endpoints.Library.AudioLibrary.MusicLibrary.Artists.RestoreMusicArtist;

/// <summary>
/// Class used for providing a textual description for the <see cref="RestoreMusicArtistEndpoint"/> endpoint, for OpenAPI.
/// </summary>
[ExcludeFromCodeCoverage]
public class RestoreMusicArtistEndpointSummary : Summary<RestoreMusicArtistEndpoint>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="RestoreMusicArtistEndpointSummary"/> class.
    /// </summary>
    public RestoreMusicArtistEndpointSummary()
    {
        Summary = "Restores a deleted music artist.";
        Description = "Restores the deleted music artist identified by the route, together with the metadata and artwork kept for it.";

        Response(200, "The music artist was restored.");
    }
}
