#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Presentation.Web.Common.DTO.Common;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Presentation.Web.Core.Endpoints.Library.AudioLibrary.MusicLibrary.Artists.UpdateArtistCover;

/// <summary>
/// Class used for providing a textual description for the <see cref="UpdateArtistCoverEndpoint"/> endpoint, for OpenAPI.
/// </summary>
[ExcludeFromCodeCoverage]
public class UpdateArtistCoverEndpointSummary : Summary<UpdateArtistCoverEndpoint, EmptyRequest>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="UpdateArtistCoverEndpointSummary"/> class.
    /// </summary>
    public UpdateArtistCoverEndpointSummary()
    {
        Summary = "Updates the cover image of an artist.";
        Description = "Updates the cover image of the artist identified by the route, with the image uploaded in the multipart form of the request.";

        Response(200, "The relative path of the stored cover image is returned.", example: new SuccessResponse<string>(true, default));
    }
}
