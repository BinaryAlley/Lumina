#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using System;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Presentation.Web.Core.Endpoints.Home;

/// <summary>
/// Class used for providing a textual description for the <see cref="HomeIndexEndpoint"/> endpoint, for OpenAPI.
/// </summary>
[ExcludeFromCodeCoverage]
public class HomeIndexEndpointSummary : Summary<HomeIndexEndpoint, EmptyRequest>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="HomeIndexEndpointSummary"/> class.
    /// </summary>
    public HomeIndexEndpointSummary()
    {
        Summary = "Renders the home page.";
        Description = "Renders the home page of the application.";

        Response(200, "The home page is rendered.");
    }
}
