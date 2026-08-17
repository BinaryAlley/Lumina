#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Presentation.Web.Common.Requests.Tools;
using System;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Presentation.Web.Core.Endpoints.Tools.Language.SetLanguage;

/// <summary>
/// Class used for providing a textual description for the <see cref="SetLanguageEndpoint"/> endpoint, for OpenAPI.
/// </summary>
[ExcludeFromCodeCoverage]
public class SetLanguageEndpointSummary : Summary<SetLanguageEndpoint, SetLanguageRequest>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SetLanguageEndpointSummary"/> class.
    /// </summary>
    public SetLanguageEndpointSummary()
    {
        Summary = "Sets the culture of the application.";
        Description = "Sets the culture used by the application, storing the preference in a cookie, and redirects the browser back.";

        RequestParam(r => r.NewCulture, "The new culture to set.");
        RequestParam(r => r.ReturnUrl, "The URL to return to, after setting the new culture.");

        Response(200, "The culture preference is stored in a cookie, and the browser is redirected back.");
    }
}
