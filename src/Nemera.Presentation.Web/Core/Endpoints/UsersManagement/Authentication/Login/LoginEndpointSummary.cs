#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Presentation.Web.Common.DTO.Common;
using Lumina.Presentation.Web.Common.Requests.UsersManagement;
using System;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Presentation.Web.Core.Endpoints.UsersManagement.Authentication.Login;

/// <summary>
/// Class used for providing a textual description for the <see cref="LoginEndpoint"/> endpoint, for OpenAPI.
/// </summary>
[ExcludeFromCodeCoverage]
public class LoginEndpointSummary : Summary<LoginEndpoint, LoginRequest>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="LoginEndpointSummary"/> class.
    /// </summary>
    public LoginEndpointSummary()
    {
        Summary = "Authenticates a user.";
        Description = "Authenticates an account and signs the user in, returning the URL to redirect to.";
        RequestParam(r => r.Username, "The username of the account. Required.");
        RequestParam(r => r.Password, "The password of the account. Required.");
        RequestParam(r => r.TotpCode, "The TOTP (Time-Based One-Time Password) code used for two-factor authentication. Optional.");
        RequestParam(r => r.ReturnUrl, "The URL to return to, after login. Optional.");

        ExampleRequest = new LoginRequest(
            Username: "JohnDoe",
            Password: "Abcd123$",
            TotpCode: "123456",
            ReturnUrl: "/"
        );

        Response(200, "The user is authenticated, and the URL to redirect to is returned.", example: new SuccessResponse<string>(true, default));
    }
}
