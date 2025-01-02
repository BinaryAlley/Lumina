#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Contracts.Requests.Authorization;
using Lumina.Contracts.Responses.Authorization;
using Lumina.Domain.Common.Enums.Authorization;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Presentation.Api.Core.Endpoints.Admin.Authorization.Roles.GetRolePermissions;

/// <summary>
/// Class used for providing a textual description for the <see cref="GetRolePermissionsEndpoint"/> API endpoint, for OpenAPI.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetRolePermissionsEndpointSummary : Summary<GetRolePermissionsEndpoint, GetRolePermissionsRequest>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GetRolePermissionsEndpointSummary"/> class.
    /// </summary>
    public GetRolePermissionsEndpointSummary()
    {
        Summary = "Gets the collection of authorization permissions of a role.";
        Description = "Gets the entire list of authorization permissions of a role.";

        ExampleRequest = new GetRolePermissionsRequest(
            RoleId: Guid.NewGuid()
        );

        RequestParam(r => r.RoleId, "The Id of authorization role. Required.");

        Response(200, "The authorization permissions are returned.",
            example: new[] {
                new PermissionResponse(Id: Guid.NewGuid(), AuthorizationPermission.CanViewUsers),
                new PermissionResponse(Id: Guid.NewGuid(), AuthorizationPermission.CanDeleteUsers),
                new PermissionResponse(Id: Guid.NewGuid(), AuthorizationPermission.CanRegisterUsers)
            });

        Response(401, "Authentication required.", "application/problem+json",
            example: new[]
            {
                new
                {
                    type = "https://tools.ietf.org/html/rfc7235#section-3.1",
                    status = 401,
                    title = "Unauthorized",
                    detail = "You are not authorized",
                    instance = "/api/v1/auth/roles/{roleId}/permissions"
                },
                new
                {
                    type = "https://tools.ietf.org/html/rfc7235#section-3.1",
                    status = 401,
                    title = "Unauthorized",
                    detail = "Invalid token: The token expired at '01/01/2024 01:00:00'",
                    instance = "/api/v1/auth/roles/{roleId}/permissions"
                },
                new
                {
                    type = "https://tools.ietf.org/html/rfc7235#section-3.1",
                    status = 401,
                    title = "Unauthorized",
                    detail = "The token is invalid",
                    instance = "/api/v1/auth/roles/{roleId}/permissions"
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
                instance = "/api/v1/auth/roles/{roleId}/permissions",
                traceId = "00-a712bbf99ca8ab485f86a762ae5ae74d-b3a2eb78813b0a5d-00"
            }
        );

        Response(404, "The request failed because the requested authorization role does not exist.", "application/problem+json",
            example: new
            {
                type = "https://tools.ietf.org/html/rfc9110#section-15.5.5",
                title = "General.NotFound",
                status = 404,
                detail = "RoleNotFound",
                instance = "/api/v1/auth/roles/{roleId}/permissions",
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
                instance = "/api/v1/auth/roles/{roleId}/permissions",
                errors = new Dictionary<string, string[]>
                {
                    {
                        "General.Validation", new[]
                        {
                            "RoleIdCannotBeEmpty"
                        }
                    }
                },
                traceId = "00-2470be4248a2a5a0c6f70579975a6954-b9c3ba9544a03500-00"
            }
        );
    }
}
