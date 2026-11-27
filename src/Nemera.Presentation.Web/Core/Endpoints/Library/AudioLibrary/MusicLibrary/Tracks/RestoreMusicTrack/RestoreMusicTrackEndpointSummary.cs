#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Nemera.Presentation.Web.Core.Endpoints.Library.AudioLibrary.MusicLibrary.Tracks.RestoreMusicTrack;

/// <summary>
/// Class used for providing a textual description for the <see cref="RestoreMusicTrackEndpoint"/> endpoint, for OpenAPI.
/// </summary>
[ExcludeFromCodeCoverage]
public class RestoreMusicTrackEndpointSummary : Summary<RestoreMusicTrackEndpoint>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="RestoreMusicTrackEndpointSummary"/> class.
    /// </summary>
    public RestoreMusicTrackEndpointSummary()
    {
        Summary = "Restores a deleted music track.";
        Description = "Restores the deleted music track identified by the route, together with the metadata and artwork kept for it.";

        Response(200, "The music track was restored.");
    }
}
