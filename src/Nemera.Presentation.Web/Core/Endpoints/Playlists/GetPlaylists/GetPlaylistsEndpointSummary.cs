#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Presentation.Web.Common.DTO.Common;
using Lumina.Presentation.Web.Common.DTO.Playlists;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Presentation.Web.Core.Endpoints.Playlists.GetPlaylists;

/// <summary>
/// Class used for providing a textual description for the <see cref="GetPlaylistsEndpoint"/> endpoint, for OpenAPI.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetPlaylistsEndpointSummary : Summary<GetPlaylistsEndpoint>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GetPlaylistsEndpointSummary"/> class.
    /// </summary>
    public GetPlaylistsEndpointSummary()
    {
        Summary = "Gets the playlists of the current user.";
        Description = "Proxies the request that lists the playlists of the current user to the API server.";

        Response(200, "The playlists of the current user are returned.",
            example: new SuccessResponse<PlaylistSummaryDto[]>(true, default)
        );
    }
}
