#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Contracts.Requests.Authorization;
using Lumina.Contracts.Responses.Authorization;
using Lumina.Domain.Common.Enums.Authorization;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Presentation.Api.Core.Endpoints.UsersManagement.Authorization.GetAuthorization;

/// <summary>
/// Class used for providing a textual description for the <see cref="GetAuthorizationEndpoint"/> API endpoint, for OpenAPI.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetAuthorizationEndpointSummary : Summary<GetAuthorizationEndpoint, GetAuthorizationRequest>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GetAuthorizationEndpointSummary"/> class.
    /// </summary>
    public GetAuthorizationEndpointSummary()
    {
        Summary = "Gets the authorization details of an account.";
        Description = "Gets the authorization role and permissions of an account.";

        ExampleRequest = new GetAuthorizationRequest(
            UserId: Guid.NewGuid()
        );

        RequestParam(r => r.UserId, "The Id of the user for whom to get the authorization. Required.");

        Response(200, "The authorization role and permissions of the account are returned.",
            example: new AuthorizationResponse(
                UserId: Guid.NewGuid(),
                Role: "Admin",
                Permissions: new HashSet<AuthorizationPermission>() { AuthorizationPermission.CanRegisterUsers, AuthorizationPermission.CanDeleteUsers }
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
                    instance = "/api/v1/auth/get-authorization"
                },
                new
                {
                    type = "https://tools.ietf.org/html/rfc7235#section-3.1",
                    status = 401,
                    title = "Unauthorized",
                    detail = "Invalid token: The token expired at '01/01/2024 01:00:00'",
                    instance = "/api/v1/auth/get-authorization"
                },
                new
                {
                    type = "https://tools.ietf.org/html/rfc7235#section-3.1",
                    status = 401,
                    title = "Unauthorized",
                    detail = "The token is invalid",
                    instance = "/api/v1/auth/get-authorization"
                }
            }
        );

        Response(403, "The request failed because either the current user is not an Admin, or the requested roles and permissions do not belong to the current user making the request.", "application/problem+json",
            example: new
            {
                type = "https://tools.ietf.org/html/rfc9110#section-15.5.4",
                title = "General.Failure",
                status = 403,
                detail = "NotAuthorized",
                instance = "/api/v1/auth/get-authorization",
                traceId = "00-a712bbf99ca8ab485f86a762ae5ae74d-b3a2eb78813b0a5d-00"
            }
        );

        Response(422, "The request did not pass validation checks.", "application/problem+json",
            example: new
            {
                type = "https://tools.ietf.org/html/rfc4918#section-11.2",
                title = "General.Validation",
                status = 422,
                detail = "OneOrMoreValidationErrorsOccurred",
                instance = "/api/v1/auth/get-authorization",
                errors = new Dictionary<string, string[]>
                {
                    {
                        "General.Validation", new[]
                        {
                            "UserIdCannotBeEmpty"
                        }
                    }
                },
                traceId = "00-839f81e411d7eb91ed5aa91e56b00bbb-7c8bd5dfabdaf2dc-00"
            }
        );
    }
}
