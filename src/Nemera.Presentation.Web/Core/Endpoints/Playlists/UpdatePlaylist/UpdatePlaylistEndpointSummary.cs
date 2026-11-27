#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Presentation.Web.Common.DTO.Common;
using Lumina.Presentation.Web.Common.DTO.Playlists;
using Lumina.Presentation.Web.Common.Requests.Playlists;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Presentation.Web.Core.Endpoints.Playlists.UpdatePlaylist;

/// <summary>
/// Class used for providing a textual description for the <see cref="UpdatePlaylistEndpoint"/> endpoint, for OpenAPI.
/// </summary>
[ExcludeFromCodeCoverage]
public class UpdatePlaylistEndpointSummary : Summary<UpdatePlaylistEndpoint, UpdatePlaylistRequest>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="UpdatePlaylistEndpointSummary"/> class.
    /// </summary>
    public UpdatePlaylistEndpointSummary()
    {
        Summary = "Updates the details of a playlist of the current user.";
        Description = "Proxies the request that updates the name and description of a playlist to the API server. The ordered entries of the playlist are left untouched.";

        Params["playlistId"] = "The Id of the playlist to update.";

        ExampleRequest = new UpdatePlaylistRequest(
            Name: "Evening listening",
            Description: "Quiet tracks for the end of the day."
        );
        RequestParam(request => request.Name, "The name of the playlist. Required, maximum 255 characters.");
        RequestParam(request => request.Description, "The description of the playlist. Optional, maximum 2000 characters.");

        Response(200, "The updated playlist details are returned.",
            example: new SuccessResponse<PlaylistDetailsDto>(true, default)
        );
    }
}
