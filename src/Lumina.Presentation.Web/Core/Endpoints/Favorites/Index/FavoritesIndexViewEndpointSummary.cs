#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Presentation.Web.Core.Endpoints.Favorites.Index;

/// <summary>
/// Class used for providing a textual description for the <see cref="FavoritesIndexViewEndpoint"/> endpoint, for OpenAPI.
/// </summary>
[ExcludeFromCodeCoverage]
public class FavoritesIndexViewEndpointSummary : Summary<FavoritesIndexViewEndpoint>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="FavoritesIndexViewEndpointSummary"/> class.
    /// </summary>
    public FavoritesIndexViewEndpointSummary()
    {
        Summary = "Renders the favorites page.";
        Description = "Renders the page that lists the favorites of the current user across media kinds and removes them.";

        Response(200, "The favorites page is rendered.");
    }
}
