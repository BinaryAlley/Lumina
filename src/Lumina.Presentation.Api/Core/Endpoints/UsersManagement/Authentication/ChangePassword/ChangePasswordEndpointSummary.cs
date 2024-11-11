#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Contracts.Requests.Authentication;
#endregion

namespace Lumina.Presentation.Api.Core.Endpoints.UsersManagement.Authentication.ChangePassword;

/// <summary>
/// Class used for providing a textual description for the <see cref="ChangePasswordEndpoint"/> API endpoint, for Swagger.
/// </summary>
public class ChangePasswordEndpointSummary : Summary<ChangePasswordEndpoint, ChangePasswordRequest>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ChangePasswordEndpointSummary"/> class.
    /// </summary>
    public ChangePasswordEndpointSummary()
    {
        Summary = "Changes the password of an account.";
        Description = "Changes the password of an account by reseting its password, after verifying TOTP code associated with the account.";

        RequestParam(r => r.CurrentPassword, "The current password of the account for which to change the password.");
        RequestParam(r => r.NewPassword, "The new password of the account for which to change the password.");
        RequestParam(r => r.NewPasswordConfirm, "The confirmation of the new password of the account for which to change the password.");

        Response(200, "The password of the account is changed.");
        Response<ProblemDetails>(422, "The request did not pass validation checks.", "application/problem+json");
    }
}
