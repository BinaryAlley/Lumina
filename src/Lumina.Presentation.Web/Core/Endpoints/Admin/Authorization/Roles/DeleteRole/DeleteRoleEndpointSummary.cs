#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Presentation.Web.Common.DTO.Common;
using Lumina.Presentation.Web.Common.Requests.Authorization;
using System;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Presentation.Web.Core.Endpoints.Admin.Authorization.Roles.DeleteRole;

/// <summary>
/// Class used for providing a textual description for the <see cref="DeleteRoleEndpoint"/> endpoint, for OpenAPI.
/// </summary>
[ExcludeFromCodeCoverage]
public class DeleteRoleEndpointSummary : Summary<DeleteRoleEndpoint, DeleteRoleRequest>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DeleteRoleEndpointSummary"/> class.
    /// </summary>
    public DeleteRoleEndpointSummary()
    {
        Summary = "Deletes a role.";
        Description = "Deletes an existing authorization role identified by its unique identifier.";

        RequestParam(r => r.RoleId, "The unique identifier of the role to delete.");

        Response(200, "The role is deleted.", example: new SuccessResponse(true));
    }
}
