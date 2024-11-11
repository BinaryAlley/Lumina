#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Contracts.Requests.Authentication;
#endregion

namespace Lumina.Presentation.Api.Core.Endpoints.UsersManagement.Authentication.Login;

/// <summary>
/// Class used for providing a textual description for the <see cref="LoginEndpoint"/> API endpoint, for Swagger.
/// </summary>
public class LoginEndpointSummary : Summary<LoginEndpoint, LoginRequest>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="LoginEndpointSummary"/> class.
    /// </summary>
    public LoginEndpointSummary()
    {
        Summary = "Authenticates an account.";
        Description = "Authenticates an application account.";

        RequestParam(r => r.Username, "The username of the account to authenticate.");
        RequestParam(r => r.Password, "The password of the account to authenticate.");
        RequestParam(r => r.TotpCode, "The TOTP (Time-Based One-Time Password) of the account to authenticate.");

        Response(200, "The JWT authentication token is returned.");
        Response<ProblemDetails>(422, "The request did not pass validation checks.", "application/problem+json");
    }
}
