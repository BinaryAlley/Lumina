#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Contracts.Requests.Authentication;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Presentation.Api.Core.Endpoints.UsersManagement.Authentication.Register;

/// <summary>
/// Class used for providing a textual description for the <see cref="RegisterEndpoint"/> API endpoint, for Swagger.
/// </summary>
[ExcludeFromCodeCoverage]
public class RegisterEndpointSummary : Summary<RegisterEndpoint, RegistrationRequest>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="RegisterEndpointSummary"/> class.
    /// </summary>
    public RegisterEndpointSummary()
    {
        Summary = "Registers a new account.";
        Description = "Registes a new application account.";

        RequestParam(r => r.Username, "The desired username for the new account.");
        RequestParam(r => r.Password, "The password for the new account.");
        RequestParam(r => r.PasswordConfirm, "The confirmation of the entered password, used to ensure consistency.");
        RequestParam(r => r.Use2fa, "Indicates whether two-factor authentication (2FA) should be enabled for the new account.");

        Response(200, "The new account of the application is returned.");
        Response<ProblemDetails>(422, "The request did not pass validation checks.", "application/problem+json");
    }
}
