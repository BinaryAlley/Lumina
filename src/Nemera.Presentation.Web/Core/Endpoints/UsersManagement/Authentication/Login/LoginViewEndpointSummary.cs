#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Presentation.Web.Common.Requests.UsersManagement.Authentication;
using System;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Presentation.Web.Core.Endpoints.UsersManagement.Authentication.Login;

/// <summary>
/// Class used for providing a textual description for the <see cref="LoginViewEndpoint"/> endpoint, for OpenAPI.
/// </summary>
[ExcludeFromCodeCoverage]
public class LoginViewEndpointSummary : Summary<LoginViewEndpoint, LoginViewRequest>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="LoginViewEndpointSummary"/> class.
    /// </summary>
    public LoginViewEndpointSummary()
    {
        Summary = "Renders the account login view.";
        Description = "Renders the account login view, using the specified URL to return to after login.";

        RequestParam(r => r.ReturnUrl, "The URL to return to, after login. Optional.");

        ExampleRequest = new LoginViewRequest(
            ReturnUrl: "/"
        );

        Response(200, "The account login view is rendered.");
    }
}
