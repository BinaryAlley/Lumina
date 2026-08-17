#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Presentation.Web.Common.DTO.Authorization;
using Lumina.Presentation.Web.Common.DTO.Common;
using Lumina.Presentation.Web.Common.Requests.Authorization;
using System;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Presentation.Web.Core.Endpoints.Admin.Authorization.Roles.AddRole;

/// <summary>
/// Class used for providing a textual description for the <see cref="AddRoleEndpoint"/> endpoint, for OpenAPI.
/// </summary>
[ExcludeFromCodeCoverage]
public class AddRoleEndpointSummary : Summary<AddRoleEndpoint, AddRoleRequest>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AddRoleEndpointSummary"/> class.
    /// </summary>
    public AddRoleEndpointSummary()
    {
        Summary = "Adds a new role.";
        Description = "Creates a new authorization role with the specified permissions.";
        RequestParam(r => r.RoleName, "The name of the role. Required.");
        RequestParam(r => r.Permissions, "The collection of Ids of the permissions of the role. Required.");

        ExampleRequest = new AddRoleRequest(
            RoleName: "Editor",
            Permissions: [
                Guid.NewGuid(),
                Guid.NewGuid(),
            ]
        );

        Response(200, "The role is created.", example: new SuccessResponse<RolePermissionsDto>(true, default));
    }
}
