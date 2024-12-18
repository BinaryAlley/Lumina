#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Contracts.Requests.Authorization;
using Lumina.Contracts.Responses.Authorization;
using Lumina.Domain.Common.Enums.Authorization;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Presentation.Api.Core.Endpoints.Admin.Authorization.Permissions.GetPermissions;

/// <summary>
/// Class used for providing a textual description for the <see cref="GetPermissionsEndpoint"/> API endpoint, for OpenAPI.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetPermissionsEndpointSummary : Summary<GetPermissionsEndpoint, EmptyRequest>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GetPermissionsEndpointSummary"/> class.
    /// </summary>
    public GetPermissionsEndpointSummary()
    {
        Summary = "Gets the collection of authorization permissions.";
        Description = "Gets the entire list of authorization permissions.";

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
                    instance = "/api/v1/permissions"
                },
                new
                {
                    type = "https://tools.ietf.org/html/rfc7235#section-3.1",
                    status = 401,
                    title = "Unauthorized",
                    detail = "Invalid token: The token expired at '01/01/2024 01:00:00'",
                    instance = "/api/v1/permissions"
                },
                new
                {
                    type = "https://tools.ietf.org/html/rfc7235#section-3.1",
                    status = 401,
                    title = "Unauthorized",
                    detail = "The token is invalid",
                    instance = "/api/v1/permissions"
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
                instance = "/api/v1/permissions",
                traceId = "00-a712bbf99ca8ab485f86a762ae5ae74d-b3a2eb78813b0a5d-00"
            }
        );
    }
}
