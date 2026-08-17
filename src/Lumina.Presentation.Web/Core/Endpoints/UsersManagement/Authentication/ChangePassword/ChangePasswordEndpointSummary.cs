#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Presentation.Web.Common.DTO.Common;
using Lumina.Presentation.Web.Common.Requests.UsersManagement;
using Lumina.Presentation.Web.Common.Responses.UsersManagement;
using System;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Presentation.Web.Core.Endpoints.UsersManagement.Authentication.ChangePassword;

/// <summary>
/// Class used for providing a textual description for the <see cref="ChangePasswordEndpoint"/> endpoint, for OpenAPI.
/// </summary>
[ExcludeFromCodeCoverage]
public class ChangePasswordEndpointSummary : Summary<ChangePasswordEndpoint, ChangePasswordRequest>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ChangePasswordEndpointSummary"/> class.
    /// </summary>
    public ChangePasswordEndpointSummary()
    {
        Summary = "Changes the account password.";
        Description = "Changes the password of the currently logged in account.";
        RequestParam(r => r.Username, "The username of the account. Required.");
        RequestParam(r => r.CurrentPassword, "The current password of the account. Required.");
        RequestParam(r => r.NewPassword, "The new password of the account. Required.");
        RequestParam(r => r.NewPasswordConfirm, "The confirmation of the new password of the account. Required.");

        ExampleRequest = new ChangePasswordRequest(
            Username: "JohnDoe",
            CurrentPassword: "Abcd123$",
            NewPassword: "Abcd1234$",
            NewPasswordConfirm: "Abcd1234$"
        );

        Response(200, "The account password is changed.", example: new SuccessResponse<ChangePasswordResponse>(true, default));
    }
}
