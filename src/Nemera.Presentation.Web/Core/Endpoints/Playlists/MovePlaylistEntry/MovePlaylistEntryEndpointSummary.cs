#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Presentation.Web.Common.DTO.Common;
using Lumina.Presentation.Web.Common.DTO.Playlists;
using Lumina.Presentation.Web.Common.Requests.Playlists;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Presentation.Web.Core.Endpoints.Playlists.MovePlaylistEntry;

/// <summary>
/// Class used for providing a textual description for the <see cref="MovePlaylistEntryEndpoint"/> endpoint, for OpenAPI.
/// </summary>
[ExcludeFromCodeCoverage]
public class MovePlaylistEntryEndpointSummary : Summary<MovePlaylistEntryEndpoint, MovePlaylistEntryRequest>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="MovePlaylistEntryEndpointSummary"/> class.
    /// </summary>
    public MovePlaylistEntryEndpointSummary()
    {
        Summary = "Moves one entry of a playlist of the current user.";
        Description = "Proxies the request that moves one entry of a playlist to a new position to the API server. The moved entry and its new position are returned.";

        Params["playlistId"] = "The Id of the playlist whose entry is moved.";
        Params["playlistEntryId"] = "The Id of the entry to move.";

        ExampleRequest = new MovePlaylistEntryRequest(
            TargetPosition: 3
        );
        RequestParam(request => request.TargetPosition, "The new zero based position of the entry. Required, zero or greater.");

        Response(200, "The moved entry and its new position are returned.",
            example: new SuccessResponse<PlaylistEntryMovedDto>(true, default)
        );
    }
}
