#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Contracts.DTO.Authentication;
using Lumina.Contracts.Requests.Authorization;
using Lumina.Contracts.Responses.Authorization;
using Lumina.Domain.Common.Enums.Authorization;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Presentation.Api.Core.Endpoints.UsersManagement.Authorization.UpdateUserRoleAndPermissions;

/// <summary>
/// Class used for providing a textual description for the <see cref="UpdateUserRoleAndPermissionsEndpoint"/> API endpoint, for OpenAPI.
/// </summary>
[ExcludeFromCodeCoverage]
public class UpdateUserRoleAndPermissionsEndpointSummary : Summary<UpdateUserRoleAndPermissionsEndpoint, UpdateUserRoleAndPermissionsRequest>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="UpdateUserRoleAndPermissionsEndpointSummary"/> class.
    /// </summary>
    public UpdateUserRoleAndPermissionsEndpointSummary()
    {
        Summary = "Updates the authorization role and permissions of a user.";
        Description = "Updates the authorization role and permissions of a user, and returns the details.";

        ExampleRequest = new UpdateUserRoleAndPermissionsRequest(
            UserId: Guid.NewGuid(),
            RoleId: Guid.NewGuid(),
            Permissions: [
                Guid.NewGuid(),
                Guid.NewGuid(),
            ]
        );

        RequestParam(r => r.UserId, "The Id of the user. Required.");
        RequestParam(r => r.RoleId, "The Id of the role. Optional.");
        RequestParam(r => r.Permissions, "The collection of Ids of the permissions of the role. Optional.");

        ResponseParam<AuthorizationResponse>(r => r.UserId, "The id of the user whose role and permissions were updated.");
        ResponseParam<AuthorizationResponse>(r => r.Role, "The authorization role entity.");
        ResponseParam<AuthorizationResponse>(r => r.Permissions, "The list of permissions of the user.");


        Response(200, "The authorization role and permissions of the user were successfully updated.", example:
            new AuthorizationResponse(
                UserId: Guid.NewGuid(),
                Role: "Editor",
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
                    instance = "/api/v1/auth/users/{userId}/role-and-permissions"
                },
                new
                {
                    type = "https://tools.ietf.org/html/rfc7235#section-3.1",
                    status = 401,
                    title = "Unauthorized",
                    detail = "Invalid token: The token expired at '01/01/2024 01:00:00'",
                    instance = "/api/v1/auth/users/{userId}/role-and-permissions"
                },
                new
                {
                    type = "https://tools.ietf.org/html/rfc7235#section-3.1",
                    status = 401,
                    title = "Unauthorized",
                    detail = "The token is invalid",
                    instance = "/api/v1/auth/users/{userId}/role-and-permissions"
                }
            }
        );

        Response(403, "The request failed because the current user is not an Admin, or changing the user's role would result in no user with an Admin account.", "application/problem+json",
            example: new
            {
                type = "https://tools.ietf.org/html/rfc9110#section-15.5.4",
                title = "General.Failure",
                status = 403,
                detail = "NotAuthorized | CannotRemoveLastAdmin | UserMustHaveRoleOrDirectPermissions",
                instance = "/api/v1/auth/users/{userId}/role-and-permissions",
                traceId = "00-a712bbf99ca8ab485f86a762ae5ae74d-b3a2eb78813b0a5d-00"
            }
        );

        Response(404, "The request failed because the provided role or user do not exist.", "application/problem+json",
            example: new
            {
                type = "https://tools.ietf.org/html/rfc9110#section-15.5.5",
                title = "General.NotFound",
                status = 404,
                detail = "RoleNotFound | UserDoesNotExist",
                instance = "/api/v1/auth/users/{userId}/role-and-permissions",
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
                instance = "/api/v1/auth/users/{userId}/role-and-permissions",
                errors = new Dictionary<string, string[]>
                {
                    {
                        "General.Validation", new[]
                        {
                            "UserIdCannotBeEmpty",
                            "RoleIdCannotBeEmpty",
                            "PermissionIdCannotBeEmpty"
                        }
                    }
                },
                traceId = "00-2470be4248a2a5a0c6f70579975a6954-b9c3ba9544a03500-00"
            }
        );
    }
}
