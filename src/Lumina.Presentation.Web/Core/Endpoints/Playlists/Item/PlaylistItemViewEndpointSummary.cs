#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Presentation.Web.Core.Endpoints.Playlists.Item;

/// <summary>
/// Class used for providing a textual description for the <see cref="PlaylistItemViewEndpoint"/> endpoint, for OpenAPI.
/// </summary>
[ExcludeFromCodeCoverage]
public class PlaylistItemViewEndpointSummary : Summary<PlaylistItemViewEndpoint>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="PlaylistItemViewEndpointSummary"/> class.
    /// </summary>
    public PlaylistItemViewEndpointSummary()
    {
        Summary = "Renders a playlist page.";
        Description = "Renders the page that edits, reorders, plays and deletes a playlist of the current user.";

        Params["playlistId"] = "The Id of the playlist to display.";

        Response(200, "The playlist page is rendered.");
    }
}
