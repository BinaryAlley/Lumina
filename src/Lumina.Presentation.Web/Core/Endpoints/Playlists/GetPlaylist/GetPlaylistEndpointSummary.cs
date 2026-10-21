#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Presentation.Web.Common.DTO.Common;
using Lumina.Presentation.Web.Common.DTO.Playlists;
using Lumina.Presentation.Web.Common.Requests.Playlists;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Presentation.Web.Core.Endpoints.Playlists.GetPlaylist;

/// <summary>
/// Class used for providing a textual description for the <see cref="GetPlaylistEndpoint"/> endpoint, for OpenAPI.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetPlaylistEndpointSummary : Summary<GetPlaylistEndpoint, GetPlaylistRequest>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GetPlaylistEndpointSummary"/> class.
    /// </summary>
    public GetPlaylistEndpointSummary()
    {
        Summary = "Gets a playlist of the current user.";
        Description = "Proxies the request that gets a playlist and a window of its ordered entries to the API server.";

        Params["playlistId"] = "The Id of the playlist to get.";

        ExampleRequest = new GetPlaylistRequest(
            Offset: 0,
            Limit: 200
        );
        RequestParam(request => request.Offset, "The zero based position of the first entry to return. Optional; defaults to 0.");
        RequestParam(request => request.Limit, "The maximum number of entries to return, from 1 to 1000. Optional; defaults to 200.");

        Response(200, "The playlist and its ordered entries are returned.",
            example: new SuccessResponse<PlaylistDto>(true, default)
        );
    }
}
