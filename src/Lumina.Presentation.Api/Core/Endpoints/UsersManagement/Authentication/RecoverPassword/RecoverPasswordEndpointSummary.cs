#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Contracts.Requests.Authentication;
using Lumina.Contracts.Responses.Authentication;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Presentation.Api.Core.Endpoints.UsersManagement.Authentication.RecoverPassword;

/// <summary>
/// Class used for providing a textual description for the <see cref="RecoverPasswordEndpoint"/> API endpoint, for OpenAPI.
/// </summary>
[ExcludeFromCodeCoverage]
public class RecoverPasswordEndpointSummary : Summary<RecoverPasswordEndpoint, RecoverPasswordRequest>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="RecoverPasswordEndpointSummary"/> class.
    /// </summary>
    public RecoverPasswordEndpointSummary()
    {
        Summary = "Recovers the password of an account.";
        Description = "Recovers the password of an account by reseting its password, after verifying TOTP code associated with the account.";

        ExampleRequest = new RecoverPasswordRequest(
            Username: "JohnDoe",
            TotpCode: "123456"
        );

        RequestParam(r => r.Username, "The username of the account for which to recover the password. Required.");
        RequestParam(r => r.TotpCode, "The TOTP (Time-Based One-Time Password) of the account for which to recover the password. Required.");

        Response(200, "The password of the account is reset.", 
            example: new RecoverPasswordResponse(
                IsPasswordReset: true
            ));

        Response(403, "The request failed because a password request was already requested and not finalized.", "application/problem+json",
            example: new
            {
                type = "https://tools.ietf.org/html/rfc9110#section-15.5.4",
                title = "General.Failure",
                status = 403,
                detail = "PasswordResetAlreadyRequested",
                instance = "/api/v1/auth/recover-password",
                traceId = "00-2ffb801e4762c9d3a5ac1a6d05721e61-7d29fda06ae9b4e2-00"
            }
        );

        Response(404, "The request failed because the provided username does not exist.", "application/problem+json",
            example: new
            {
                type = "https://tools.ietf.org/html/rfc9110#section-15.5.5",
                title = "General.NotFound",
                status = 404,
                detail = "UsernameDoesNotExist",
                instance = "/api/v1/auth/recover-password",
                traceId = "00-57d15dadd702dbd4aeb5dc9b7cee68ee-9330237dbb2ce0e5-00"
            }
        );

        Response(422, "The request did not pass validation checks.", "application/problem+json",
            example: new
            {
                type = "https://tools.ietf.org/html/rfc4918#section-11.2",
                title = "General.Validation",
                status = 422,
                detail = "OneOrMoreValidationErrorsOccurred",
                instance = "/api/v1/auth/recover-password",
                errors = new Dictionary<string, string[]>
                {
                    {
                        "General.Validation", new[]
                        {
                            "UsernameCannotBeEmpty",
                            "TotpCannotBeEmpty",
                            "InvalidTotpCode"
                        }
                    }
                },
                traceId = "00-2470be4248a2a5a0c6f70579975a6954-b9c3ba9544a03500-00"
            }
        );

        Response(429, "The request failed due to a more than 10 requests to this endpoint in less than 15 minutes.", "application/problem+json",
            example: new
            {
                type = "https://tools.ietf.org/html/rfc7231#section-6.5.29",
                title = "TooManyRequests",
                status = 429,
                detail = "Too many attempts. Please try again later.",
                retryAfter = "900"
            }
        );
    }
}
