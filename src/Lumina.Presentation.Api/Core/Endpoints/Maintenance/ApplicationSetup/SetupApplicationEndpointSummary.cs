#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Contracts.Requests.Authentication;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Presentation.Api.Core.Endpoints.Maintenance.ApplicationSetup;

/// <summary>
/// Class used for providing a textual description for the <see cref="SetupApplicationEndpoint"/> API endpoint, for Swagger.
/// </summary>
[ExcludeFromCodeCoverage]
public class SetupApplicationEndpointSummary : Summary<SetupApplicationEndpoint, RegistrationRequest>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SetupApplicationEndpointSummary"/> class.
    /// </summary>
    public SetupApplicationEndpointSummary()
    {
        Summary = "Performs the initial application setup.";
        Description = "Performs the initial application setup, including creating the Admin account.";

        RequestParam(r => r.Username, "The desired username for the new account.");
        RequestParam(r => r.Password, "The password for the new account.");
        RequestParam(r => r.PasswordConfirm, "The confirmation of the entered password, used to ensure consistency.");
        RequestParam(r => r.Use2fa, "Indicates whether two-factor authentication (2FA) should be enabled for the new account.");

        Response(200, "The Admin account of the application is returned.");
        Response<ProblemDetails>(422, "The request did not pass validation checks.", "application/problem+json");
    }
}
