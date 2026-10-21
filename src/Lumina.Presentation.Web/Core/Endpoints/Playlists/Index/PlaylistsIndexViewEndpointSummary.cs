#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Presentation.Web.Core.Endpoints.Playlists.Index;

/// <summary>
/// Class used for providing a textual description for the <see cref="PlaylistsIndexViewEndpoint"/> endpoint, for OpenAPI.
/// </summary>
[ExcludeFromCodeCoverage]
public class PlaylistsIndexViewEndpointSummary : Summary<PlaylistsIndexViewEndpoint>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="PlaylistsIndexViewEndpointSummary"/> class.
    /// </summary>
    public PlaylistsIndexViewEndpointSummary()
    {
        Summary = "Renders the playlists page.";
        Description = "Renders the page that lists and deletes the playlists of the current user.";

        Response(200, "The playlists page is rendered.");
    }
}
