#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Presentation.Web.Common.DTO.Common;
using Lumina.Presentation.Web.Common.Requests.UsersManagement;
using Lumina.Presentation.Web.Common.Responses.UsersManagement;
using System;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Presentation.Web.Core.Endpoints.UsersManagement.Authentication.Register;

/// <summary>
/// Class used for providing a textual description for the <see cref="RegisterEndpoint"/> endpoint, for OpenAPI.
/// </summary>
[ExcludeFromCodeCoverage]
public class RegisterEndpointSummary : Summary<RegisterEndpoint, RegisterRequest>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="RegisterEndpointSummary"/> class.
    /// </summary>
    public RegisterEndpointSummary()
    {
        Summary = "Registers an account.";
        Description = "Registers an account, or sets up the initial application admin account.";
        RequestParam(r => r.Username, "The username of the account. Required.");
        RequestParam(r => r.Password, "The password of the account. Required.");
        RequestParam(r => r.PasswordConfirm, "The confirmation of the password of the account. Required.");
        RequestParam(r => r.RegistrationType, "The type of the registration, either 'Admin' or 'User'. Required.");
        RequestParam(r => r.Use2fa, "Whether two-factor authentication should be enabled for the account. Optional.");

        ExampleRequest = new RegisterRequest(
            Username: "JohnDoe",
            Password: "Abcd123$",
            PasswordConfirm: "Abcd123$",
            RegistrationType: "User"
        );

        Response(200, "The account is registered.", example: new SuccessResponse<RegisterResponse>(true, default));
    }
}
