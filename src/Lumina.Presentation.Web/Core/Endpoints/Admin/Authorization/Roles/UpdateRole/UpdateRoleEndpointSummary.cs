#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Presentation.Web.Common.DTO.Authorization;
using Lumina.Presentation.Web.Common.DTO.Common;
using Lumina.Presentation.Web.Common.Requests.Authorization;
using System;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Presentation.Web.Core.Endpoints.Admin.Authorization.Roles.UpdateRole;

/// <summary>
/// Class used for providing a textual description for the <see cref="UpdateRoleEndpoint"/> endpoint, for OpenAPI.
/// </summary>
[ExcludeFromCodeCoverage]
public class UpdateRoleEndpointSummary : Summary<UpdateRoleEndpoint, UpdateRoleRequest>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="UpdateRoleEndpointSummary"/> class.
    /// </summary>
    public UpdateRoleEndpointSummary()
    {
        Summary = "Updates a role.";
        Description = "Updates the name and the permissions of an existing authorization role.";
        RequestParam(r => r.RoleId, "The unique identifier of the role. Required.");
        RequestParam(r => r.RoleName, "The name of the role. Required.");
        RequestParam(r => r.Permissions, "The collection of Ids of the permissions of the role. Required.");

        ExampleRequest = new UpdateRoleRequest(
            RoleId: Guid.NewGuid(),
            RoleName: "Editor",
            Permissions: [
                Guid.NewGuid(),
                Guid.NewGuid(),
            ]
        );

        Response(200, "The role is updated.", example: new SuccessResponse<RolePermissionsDto>(true, default));
    }
}
