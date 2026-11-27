#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using System;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Presentation.Web.Core.Endpoints.Home;

/// <summary>
/// Class used for providing a textual description for the <see cref="HomeErrorEndpoint"/> endpoint, for OpenAPI.
/// </summary>
[ExcludeFromCodeCoverage]
public class HomeErrorEndpointSummary : Summary<HomeErrorEndpoint, EmptyRequest>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="HomeErrorEndpointSummary"/> class.
    /// </summary>
    public HomeErrorEndpointSummary()
    {
        Summary = "Renders the error view.";
        Description = "Renders the error page of the application.";

        Response(200, "The error view is rendered.");
    }
}
