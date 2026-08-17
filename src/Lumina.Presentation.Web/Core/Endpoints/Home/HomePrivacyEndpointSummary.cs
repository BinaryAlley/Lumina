#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using System;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Presentation.Web.Core.Endpoints.Home;

/// <summary>
/// Class used for providing a textual description for the <see cref="HomePrivacyEndpoint"/> endpoint, for OpenAPI.
/// </summary>
[ExcludeFromCodeCoverage]
public class HomePrivacyEndpointSummary : Summary<HomePrivacyEndpoint, EmptyRequest>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="HomePrivacyEndpointSummary"/> class.
    /// </summary>
    public HomePrivacyEndpointSummary()
    {
        Summary = "Renders the privacy view.";
        Description = "Renders the privacy policy page of the application.";

        Response(200, "The privacy view is rendered.");
    }
}
