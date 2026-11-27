#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Presentation.Web.Common.DTO.Common;
using Lumina.Presentation.Web.Common.Requests.Authorization;
using Lumina.Presentation.Web.Common.Responses.Authorization;
using System;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Presentation.Web.Core.Endpoints.Admin.UsersManagement.Authorization.UpdateUserRoleAndPermissions;

/// <summary>
/// Class used for providing a textual description for the <see cref="UpdateUserRoleAndPermissionsEndpoint"/> endpoint, for OpenAPI.
/// </summary>
[ExcludeFromCodeCoverage]
public class UpdateUserRoleAndPermissionsEndpointSummary : Summary<UpdateUserRoleAndPermissionsEndpoint, UpdateUserRoleAndPermissionsRequest>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="UpdateUserRoleAndPermissionsEndpointSummary"/> class.
    /// </summary>
    public UpdateUserRoleAndPermissionsEndpointSummary()
    {
        Summary = "Updates the role and the permissions of a user.";
        Description = "Updates the authorization role and the permissions of a user identified by its unique identifier.";
        RequestParam(r => r.UserId, "The unique identifier of the user. Required.");
        RequestParam(r => r.RoleId, "The unique identifier of the role. Optional.");
        RequestParam(r => r.Permissions, "The collection of Ids of the permissions of the role. Optional.");

        ExampleRequest = new UpdateUserRoleAndPermissionsRequest(
            UserId: Guid.NewGuid(),
            RoleId: Guid.NewGuid(),
            Permissions: [
                Guid.NewGuid(),
                Guid.NewGuid(),
            ]
        );

        Response(200, "The role and the permissions of the user are updated.", example: new SuccessResponse<GetAuthorizationResponse>(true, default));
    }
}
