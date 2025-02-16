#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Contracts.Requests.Authorization;
using Lumina.Contracts.Responses.Authorization;
using Lumina.Domain.Common.Enums.Authorization;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Presentation.Api.Core.Endpoints.UsersManagement.Authorization.GetUserRole;

/// <summary>
/// Class used for providing a textual description for the <see cref="GetUserRoleEndpoint"/> API endpoint, for OpenAPI.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetUserRoleEndpointSummary : Summary<GetUserRoleEndpoint, GetUserRoleRequest>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GetUserRoleEndpointSummary"/> class.
    /// </summary>
    public GetUserRoleEndpointSummary()
    {
        Summary = "Gets the authorization role of a user.";
        Description = "Gets the authorization role of a user.";

        ExampleRequest = new GetUserRoleRequest(
            UserId: Guid.NewGuid()
        );

        RequestParam(r => r.UserId, "The unique identifier of the user for whom to get the authorization role. Required.");

        Response(200, "The authorization role of the user is returned.",
            example: new RoleResponse(
                Id: Guid.NewGuid(),
                RoleName: "Admin"
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
                    instance = "/api/v1/auth/users/{userId}/role"
                },
                new
                {
                    type = "https://tools.ietf.org/html/rfc7235#section-3.1",
                    status = 401,
                    title = "Unauthorized",
                    detail = "Invalid token: The token expired at '01/01/2024 01:00:00'",
                    instance = "/api/v1/auth/users/{userId}/role"
                },
                new
                {
                    type = "https://tools.ietf.org/html/rfc7235#section-3.1",
                    status = 401,
                    title = "Unauthorized",
                    detail = "The token is invalid",
                    instance = "/api/v1/auth/users/{userId}/role"
                }
            }
        );

        Response(403, "The request failed because the current user is not an Admin.", "application/problem+json",
            example: new
            {
                type = "https://tools.ietf.org/html/rfc9110#section-15.5.4",
                title = "General.Failure",
                status = 403,
                detail = "NotAuthorized",
                instance = "/api/v1/auth/users/{userId}/role",
                traceId = "00-a712bbf99ca8ab485f86a762ae5ae74d-b3a2eb78813b0a5d-00"
            }
        );

        Response(404, "The request failed because the provided user does not exist.", "application/problem+json",
            example: new
            {
                type = "https://tools.ietf.org/html/rfc9110#section-15.5.5",
                title = "General.NotFound",
                status = 404,
                detail = "UsernameDoesNotExist",
                instance = "/api/v1/auth/users/{userId}/role",
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
                instance = "/api/v1/auth/users/{userId}/role",
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
