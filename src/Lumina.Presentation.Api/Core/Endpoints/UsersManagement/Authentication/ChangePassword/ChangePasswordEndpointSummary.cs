#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Contracts.Requests.Authentication;
using Lumina.Contracts.Responses.Authentication;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Presentation.Api.Core.Endpoints.UsersManagement.Authentication.ChangePassword;

/// <summary>
/// Class used for providing a textual description for the <see cref="ChangePasswordEndpoint"/> API endpoint, for OpenAPI.
/// </summary>
[ExcludeFromCodeCoverage]
public class ChangePasswordEndpointSummary : Summary<ChangePasswordEndpoint, ChangePasswordRequest>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ChangePasswordEndpointSummary"/> class.
    /// </summary>
    public ChangePasswordEndpointSummary()
    {
        Summary = "Changes the password of an account.";
        Description = "Changes the password of an account.";

        ExampleRequest = new ChangePasswordRequest(
            Username: "JohnDoe",
            CurrentPassword: "Abcd123$",
            NewPassword: "123$Abcd",
            NewPasswordConfirm: "123$Abcd"
        );

        RequestParam(r => r.Username, "The username of the account. Required.");
        RequestParam(r => r.CurrentPassword, "The current password of the account for which to change the password. Required.");
        RequestParam(r => r.NewPassword, "The new password of the account for which to change the password. Required.");
        RequestParam(r => r.NewPasswordConfirm, "The confirmation of the new password of the account for which to change the password. Required.");

        Response(200, "The password of the account is changed.",
            example: new ChangePasswordResponse(
                IsPasswordChanged: true
            ));

        Response(401, "Authentication required.", "application/problem+json",
            example: new[]
            {
                new
                {
                    type = "https://tools.ietf.org/html/rfc7235#section-3.1",
                    status = 401,
                    title = "Unauthorized",
                    detail = "You are not authorized",
                    instance = "/api/v1/auth/change-password"
                },
                new
                {
                    type = "https://tools.ietf.org/html/rfc7235#section-3.1",
                    status = 401,
                    title = "Unauthorized",
                    detail = "Invalid token: The token expired at '01/01/2024 01:00:00'",
                    instance = "/api/v1/auth/change-password"
                },
                new
                {
                    type = "https://tools.ietf.org/html/rfc7235#section-3.1",
                    status = 401,
                    title = "Unauthorized",
                    detail = "The token is invalid",
                    instance = "/api/v1/auth/change-password"
                }
            }
        );

        Response(403, "The request failed because the current password is invalid.", "application/problem+json",
            example: new
            {
                type = "https://tools.ietf.org/html/rfc9110#section-15.5.4",
                title = "General.Failure",
                status = 403,
                detail = "InvalidCurrentPassword",
                instance = "/api/v1/auth/change-password",
                traceId = "00-a712bbf99ca8ab485f86a762ae5ae74d-b3a2eb78813b0a5d-00"
            }
        );

        Response(404, "The request failed because the provided username does not exist.", "application/problem+json",
            example: new
            {
                type = "https://tools.ietf.org/html/rfc9110#section-15.5.5",
                title = "General.NotFound",
                status = 404,
                detail = "UsernameDoesNotExist",
                instance = "/api/v1/auth/change-password",
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
                instance = "/api/v1/auth/change-password",
                errors = new Dictionary<string, string[]>
                {
                    {
                        "General.Validation", new[]
                        {
                            "UsernameCannotBeEmpty",
                            "CurrentPasswordCannotBeEmpty",
                            "NewPasswordCannotBeEmpty",
                            "NewPasswordConfirmCannotBeEmpty",
                            "InvalidPassword",
                            "PasswordsNotMatch"
                        }
                    }
                },
                traceId = "00-839f81e411d7eb91ed5aa91e56b00bbb-7c8bd5dfabdaf2dc-00"
            }
        );
    }
}
