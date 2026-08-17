#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Presentation.Web.Common.DTO.Common;
using Lumina.Presentation.Web.Common.Requests.UsersManagement;
using Lumina.Presentation.Web.Common.Responses.UsersManagement;
using System;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Presentation.Web.Core.Endpoints.UsersManagement.Authentication.RecoverPassword;

/// <summary>
/// Class used for providing a textual description for the <see cref="RecoverPasswordEndpoint"/> endpoint, for OpenAPI.
/// </summary>
[ExcludeFromCodeCoverage]
public class RecoverPasswordEndpointSummary : Summary<RecoverPasswordEndpoint, RecoverPasswordRequest>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="RecoverPasswordEndpointSummary"/> class.
    /// </summary>
    public RecoverPasswordEndpointSummary()
    {
        Summary = "Recovers the account password.";
        Description = "Recovers the password of an account by verifying the provided TOTP code.";
        RequestParam(r => r.Username, "The username of the account. Required.");
        RequestParam(r => r.TotpCode, "The TOTP (Time-Based One-Time Password) code used for two-factor authentication. Optional.");

        ExampleRequest = new RecoverPasswordRequest(
            Username: "JohnDoe",
            TotpCode: "123456"
        );

        Response(200, "The account password is recovered.", example: new SuccessResponse<RecoverPasswordResponse>(true, default));
    }
}
