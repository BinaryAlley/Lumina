#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Presentation.Web.Common.DTO.Common;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Presentation.Web.Core.Endpoints.Playlists.DeletePlaylist;

/// <summary>
/// Class used for providing a textual description for the <see cref="DeletePlaylistEndpoint"/> endpoint, for OpenAPI.
/// </summary>
[ExcludeFromCodeCoverage]
public class DeletePlaylistEndpointSummary : Summary<DeletePlaylistEndpoint>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DeletePlaylistEndpointSummary"/> class.
    /// </summary>
    public DeletePlaylistEndpointSummary()
    {
        Summary = "Deletes a playlist of the current user.";
        Description = "Proxies the request that deletes a playlist and its entries to the API server. The tracks the playlist referenced are not deleted.";

        Params["playlistId"] = "The Id of the playlist to delete.";

        Response(200, "The playlist was deleted.",
            example: new SuccessResponse(true)
        );
    }
}
