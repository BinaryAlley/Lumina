#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using System;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Presentation.Web.Core.Endpoints.Home;

/// <summary>
/// Class used for providing a textual description for the <see cref="HomeNotFoundViewEndpoint"/> endpoint, for OpenAPI.
/// </summary>
[ExcludeFromCodeCoverage]
public class HomeNotFoundViewEndpointSummary : Summary<HomeNotFoundViewEndpoint, EmptyRequest>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="HomeNotFoundViewEndpointSummary"/> class.
    /// </summary>
    public HomeNotFoundViewEndpointSummary()
    {
        Summary = "Renders the not-found view.";
        Description = "Renders the page displayed when a requested resource is not found.";

        Response(200, "The not-found view is rendered.");
    }
}
