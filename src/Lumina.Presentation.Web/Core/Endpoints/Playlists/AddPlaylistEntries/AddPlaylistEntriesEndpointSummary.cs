#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Presentation.Web.Common.DTO.Common;
using Lumina.Presentation.Web.Common.DTO.Playlists;
using Lumina.Presentation.Web.Common.Requests.Playlists;
using System;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Presentation.Web.Core.Endpoints.Playlists.AddPlaylistEntries;

/// <summary>
/// Class used for providing a textual description for the <see cref="AddPlaylistEntriesEndpoint"/> endpoint, for OpenAPI.
/// </summary>
[ExcludeFromCodeCoverage]
public class AddPlaylistEntriesEndpointSummary : Summary<AddPlaylistEntriesEndpoint, AddPlaylistEntriesRequest>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AddPlaylistEntriesEndpointSummary"/> class.
    /// </summary>
    public AddPlaylistEntriesEndpointSummary()
    {
        Summary = "Adds several tracks to a playlist of the current user.";
        Description = "Proxies the request that adds several tracks, in order, to a playlist to the API server. The audio player uses it to add its current queue to a playlist. The number of added entries and the playlist activity time are returned.";

        Params["playlistId"] = "The Id of the playlist to add the tracks to.";

        ExampleRequest = new AddPlaylistEntriesRequest(
            TrackIds:
            [
                Guid.Parse("9f0e1d2c-3b4a-4c6d-8e7f-9a0b1c2d3e4f"),
                Guid.Parse("8e1f2a3b-4c5d-4e6f-8a9b-0c1d2e3f4a5b")
            ]
        );
        RequestParam(request => request.TrackIds, "The Ids of the tracks to add, in the order they must take. Required.");

        Response(200, "The tracks were added and the number of added entries is returned.",
            example: new SuccessResponse<PlaylistEntriesAddedDto>(true, default)
        );
    }
}
