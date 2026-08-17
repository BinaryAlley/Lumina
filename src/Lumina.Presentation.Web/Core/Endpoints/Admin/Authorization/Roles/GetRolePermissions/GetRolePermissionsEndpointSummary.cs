#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Presentation.Web.Common.DTO.Authorization;
using Lumina.Presentation.Web.Common.DTO.Common;
using Lumina.Presentation.Web.Common.Requests.Authorization;
using System;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Presentation.Web.Core.Endpoints.Admin.Authorization.Roles.GetRolePermissions;

/// <summary>
/// Class used for providing a textual description for the <see cref="GetRolePermissionsEndpoint"/> endpoint, for OpenAPI.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetRolePermissionsEndpointSummary : Summary<GetRolePermissionsEndpoint, GetRolePermissionsRequest>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GetRolePermissionsEndpointSummary"/> class.
    /// </summary>
    public GetRolePermissionsEndpointSummary()
    {
        Summary = "Gets the permissions of a role.";
        Description = "Retrieves the permissions of an existing authorization role identified by its unique identifier.";

        RequestParam(r => r.RoleId, "The unique identifier of the role whose permissions are retrieved.");

        Response(200, "The permissions of the role are returned.", example: new SuccessResponse<RolePermissionsDto>(true, default));
    }
}
