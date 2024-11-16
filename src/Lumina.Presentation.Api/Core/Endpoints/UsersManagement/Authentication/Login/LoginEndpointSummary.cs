#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Contracts.Requests.Authentication;
using Lumina.Contracts.Responses.Authentication;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Presentation.Api.Core.Endpoints.UsersManagement.Authentication.Login;

/// <summary>
/// Class used for providing a textual description for the <see cref="LoginEndpoint"/> API endpoint, for Swagger.
/// </summary>
[ExcludeFromCodeCoverage]
public class LoginEndpointSummary : Summary<LoginEndpoint, LoginRequest>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="LoginEndpointSummary"/> class.
    /// </summary>
    public LoginEndpointSummary()
    {
        Summary = "Authenticates an account.";
        Description = "Authenticates an application account.";

        ExampleRequest = new LoginRequest(
            Username: "JohnDoe",
            Password: "Abcd123$",
            TotpCode: "123456"
        );
        RequestExamples.Add(new RequestExample(new LoginRequest(
            Username: "JohnDoe",
            Password: "Abcd123$"
        )));
        RequestParam(r => r.Username, "The username of the account to authenticate. Required.");
        RequestParam(r => r.Password, "The password of the account to authenticate. Required.");
        RequestParam(r => r.TotpCode, "The TOTP (Time-Based One-Time Password) of the account to authenticate. Optional.");

        Response(200, "The JWT authentication token is returned.",
            example: new LoginResponse(
                Guid.NewGuid(),
                Username: "JohnDoe",
                Token: "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIwNzE2Y2E1ZC1hZjhkLT...",
                UsesTotp: true
            ));

        Response(403, "For regular login, the request failed because a either the username or the password is invalid. For password recovery, this is returned when the temporary password used to login expires (more than 15 minutes passed since its generation).", "application/problem+json",
            example: new
            {
                type = "https://tools.ietf.org/html/rfc9110#section-15.5.4",
                title = "General.Failure",
                status = 403,
                detail = "InvalidUsernameOrPassword",
                instance = "/api/v1/auth/login",
                traceId = "00-7abff4d7d261fa2fa503fef236b2a139-2198f929121f0707-00"
            }
        );

        Response(422, "The request did not pass validation checks.", "application/problem+json",
            example: new
            {
                type = "https://tools.ietf.org/html/rfc4918#section-11.2",
                title = "General.Validation",
                status = 422,
                detail = "OneOrMoreValidationErrorsOccurred",
                instance = "/api/v1/auth/login",
                errors = new Dictionary<string, string[]>
                {
                    {
                        "General.Validation", new[]
                        {
                            "UsernameCannotBeEmpty",
                            "PasswordCannotBeEmpty",
                            "InvalidPassword",
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
