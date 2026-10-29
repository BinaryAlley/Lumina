#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Presentation.Web.Common.DTO.Common;
using Lumina.Presentation.Web.Common.DTO.Playlists;
using Lumina.Presentation.Web.Common.Requests.Playlists;
using System;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Presentation.Web.Core.Endpoints.Playlists.AddPlaylistEntry;

/// <summary>
/// Class used for providing a textual description for the <see cref="AddPlaylistEntryEndpoint"/> endpoint, for OpenAPI.
/// </summary>
[ExcludeFromCodeCoverage]
public class AddPlaylistEntryEndpointSummary : Summary<AddPlaylistEntryEndpoint, AddPlaylistEntryRequest>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AddPlaylistEntryEndpointSummary"/> class.
    /// </summary>
    public AddPlaylistEntryEndpointSummary()
    {
        Summary = "Adds a track to a playlist of the current user.";
        Description = "Proxies the request that adds a track to a playlist to the API server. The added entry and the playlist activity time are returned.";

        Params["playlistId"] = "The Id of the playlist to add the track to.";

        ExampleRequest = new AddPlaylistEntryRequest(
            TrackId: Guid.Parse("9f0e1d2c-3b4a-4c6d-8e7f-9a0b1c2d3e4f")
        );
        RequestParam(request => request.TrackId, "The Id of the track to add. Required.");

        Response(200, "The track was added and the resulting entry is returned.",
            example: new SuccessResponse<PlaylistEntryAddedDto>(true, default)
        );
    }
}
