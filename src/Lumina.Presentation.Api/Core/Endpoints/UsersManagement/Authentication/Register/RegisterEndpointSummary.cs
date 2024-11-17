#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Contracts.Requests.Authentication;
using Lumina.Contracts.Responses.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Presentation.Api.Core.Endpoints.UsersManagement.Authentication.Register;

/// <summary>
/// Class used for providing a textual description for the <see cref="RegisterEndpoint"/> API endpoint, for OpenAPI.
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
        Description = "Registers a new application account.";

        ExampleRequest = new RegistrationRequest(
            Username: "JohnDoe",
            Password: "Abcd123$",
            PasswordConfirm: "Abcd123$",
            Use2fa: true
        );
        RequestExamples.Add(new RequestExample(new RegistrationRequest(
            Username: "JohnDoe",
            Password: "Abcd123$",
            PasswordConfirm: "Abcd123$",
            Use2fa: false
        )));

        RequestParam(r => r.Username, "The desired username for the new account. Required.");
        RequestParam(r => r.Password, "The password for the new account. Required.");
        RequestParam(r => r.PasswordConfirm, "The confirmation of the entered password, used to ensure consistency. Required.");
        RequestParam(r => r.Use2fa, "Indicates whether two-factor authentication (2FA) should be enabled for the new account. Optional.");

        Response(201, "The new account is returned.", example:
            new RegistrationResponse(
                Id: Guid.NewGuid(),
                Username: "JohnDoe",
                TotpSecret: "data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAABCQAAAQkAQAAAACN7fKkAAAFW0lEQVR4nO3bQW4cOQwF0L6B73/L3MABjCp/ilR1BphkFAl..."
            ));

        Response(409, "The request failed due to a conflict, username already exists.", "application/problem+json",
            example: new
            {
                type = "https://tools.ietf.org/html/rfc9110#section-15.5.10",
                title = "General.Conflict",
                status = 409,
                detail = "UsernameAlreadyExists",
                instance = "/api/v1/auth/register",
                traceId = "00-f8784de7cb2d70b81b2529893068bee7-4779e0404fe5f554-00"
            }
        );

        Response(422, "The request did not pass validation checks.", "application/problem+json",
            example: new
            {
                type = "https://tools.ietf.org/html/rfc4918#section-11.2",
                title = "General.Validation",
                status = 422,
                detail = "OneOrMoreValidationErrorsOccurred",
                instance = "/api/v1/auth/register",
                errors = new Dictionary<string, string[]>
                {
                    {
                        "General.Validation", new[]
                        {
                            "UsernameCannotBeEmpty",
                            "PassordCannotBeEmpty",
                            "UsernameMustBeBetween3And255CharactersLong",
                            "InvalidUsername",
                            "InvalidPassword",
                            "PasswordConfirmCannotBeEmpty",
                            "PasswordsNotMatch"
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
