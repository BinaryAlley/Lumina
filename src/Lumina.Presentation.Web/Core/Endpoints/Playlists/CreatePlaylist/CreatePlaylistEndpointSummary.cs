#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Presentation.Web.Common.DTO.Common;
using Lumina.Presentation.Web.Common.DTO.Playlists;
using Lumina.Presentation.Web.Common.Enums.Playlists;
using Lumina.Presentation.Web.Common.Requests.Playlists;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Presentation.Web.Core.Endpoints.Playlists.CreatePlaylist;

/// <summary>
/// Class used for providing a textual description for the <see cref="CreatePlaylistEndpoint"/> endpoint, for OpenAPI.
/// </summary>
[ExcludeFromCodeCoverage]
public class CreatePlaylistEndpointSummary : Summary<CreatePlaylistEndpoint, CreatePlaylistRequest>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CreatePlaylistEndpointSummary"/> class.
    /// </summary>
    public CreatePlaylistEndpointSummary()
    {
        Summary = "Creates a playlist of the current user.";
        Description = "Proxies the request that creates a playlist of the current user to the API server. The new playlist starts empty.";

        ExampleRequest = new CreatePlaylistRequest(
            Name: "Evening listening",
            Description: "Quiet tracks for the end of the day.",
            Type: PlaylistType.Music
        );
        RequestParam(request => request.Name, "The name of the playlist. Required, maximum 255 characters.");
        RequestParam(request => request.Description, "The description of the playlist. Optional, maximum 2000 characters.");
        RequestParam(request => request.Type, "The kind of media the playlist holds. Required. Only Music is currently supported.");

        Response(200, "The playlist was created and is returned.",
            example: new SuccessResponse<PlaylistDto>(true, default)
        );
    }
}
