#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Contracts.Requests.Authentication;
#endregion

namespace Lumina.Presentation.Api.Core.Endpoints.UsersManagement.Authentication.RecoverPassword;

/// <summary>
/// Class used for providing a textual description for the <see cref="RecoverPasswordEndpoint"/> API endpoint, for Swagger.
/// </summary>
public class RecoverPasswordEndpointSummary : Summary<RecoverPasswordEndpoint, RecoverPasswordRequest>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="RecoverPasswordEndpointSummary"/> class.
    /// </summary>
    public RecoverPasswordEndpointSummary()
    {
        Summary = "Recovers the password of an account.";
        Description = "Recovers the password of an account by reseting its password, after verifying TOTP code associated with the account.";

        RequestParam(r => r.Username, "The username of the account for which to recover the password.");
        RequestParam(r => r.TotpCode, "The TOTP (Time-Based One-Time Password) of the account for which to recover the password.");

        Response(200, "The password of the account is reset.");
        Response<ProblemDetails>(422, "The request did not pass validation checks.", "application/problem+json");
    }
}
