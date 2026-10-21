#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Presentation.Web.Core.Endpoints.Playlists.Item;

/// <summary>
/// Class used for providing a textual description for the <see cref="AddPlaylistViewEndpoint"/> endpoint, for OpenAPI.
/// </summary>
[ExcludeFromCodeCoverage]
public class AddPlaylistViewEndpointSummary : Summary<AddPlaylistViewEndpoint>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AddPlaylistViewEndpointSummary"/> class.
    /// </summary>
    public AddPlaylistViewEndpointSummary()
    {
        Summary = "Renders the create playlist page.";
        Description = "Renders the page that creates a playlist of the current user.";

        Response(200, "The create playlist page is rendered.");
    }
}
