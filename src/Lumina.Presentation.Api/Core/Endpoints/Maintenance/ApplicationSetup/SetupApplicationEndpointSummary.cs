#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Contracts.Requests.Authentication;
using Lumina.Contracts.Responses.Authentication;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Presentation.Api.Core.Endpoints.Maintenance.ApplicationSetup;

/// <summary>
/// Class used for providing a textual description for the <see cref="SetupApplicationEndpoint"/> API endpoint, for OpenAPI.
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

        Response(201, "The Admin account of the application is returned.", example:
            new RegistrationResponse(
                Id: Guid.NewGuid(),
                Username: "JohnDoe",
                TotpSecret: "data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAABCQAAAQkAQAAAACN7fKkAAAFW0lEQVR4nO3bQW4cOQwF0L6B73/L3MABjCp/ilR1BphkFAl..."
            ));

        Response(403, "The request failed because the application is already initialized (Admin account exists).", "application/problem+json",
            example: new
            {
                type = "https://tools.ietf.org/html/rfc9110#section-15.5.4",
                title = "General.Unauthorized",
                status = 403,
                detail = "AdminAccountAlreadyCreated",
                instance = "/api/v1/initialization",
                traceId = "00-4d94289ee3cddd8d1a3da9ef35bd37a8-585fbdf2b5ad5e03-00"
            }
        );

        Response(422, "The request did not pass validation checks.", "application/problem+json",
            example: new
            {
                type = "https://tools.ietf.org/html/rfc4918#section-11.2",
                title = "General.Validation",
                status = 422,
                detail = "OneOrMoreValidationErrorsOccurred",
                instance = "/api/v1/initialization",
                errors = new Dictionary<string, string[]>
                {
                    {
                        "General.Validation", new[]
                        {
                            "UsernameCannotBeEmpty",
                            "PassordCannotBeEmpty",
                            "InvalidPassword",
                            "PasswordConfirmCannotBeEmpty",
                            "PasswordsNotMatch"
                        }
                    }
                },
                traceId = "00-2470be4248a2a5a0c6f70579975a6954-b9c3ba9544a03500-00"
            }
        );
    }
}
