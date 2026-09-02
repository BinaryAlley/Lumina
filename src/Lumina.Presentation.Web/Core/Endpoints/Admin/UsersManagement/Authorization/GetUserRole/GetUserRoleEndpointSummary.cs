#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Presentation.Web.Common.DTO.Authorization;
using Lumina.Presentation.Web.Common.DTO.Common;
using Lumina.Presentation.Web.Common.Requests.Authorization;
using System;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Presentation.Web.Core.Endpoints.Admin.UsersManagement.Authorization.GetUserRole;

/// <summary>
/// Class used for providing a textual description for the <see cref="GetUserRoleEndpoint"/> endpoint, for OpenAPI.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetUserRoleEndpointSummary : Summary<GetUserRoleEndpoint, GetUserRoleRequest>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GetUserRoleEndpointSummary"/> class.
    /// </summary>
    public GetUserRoleEndpointSummary()
    {
        Summary = "Gets the role of a user.";
        Description = "Retrieves the authorization role of a user identified by its unique identifier.";

        RequestParam(r => r.UserId, "The unique identifier of the user for whom to get the authorization role. Required.");

        ExampleRequest = new GetUserRoleRequest(
            UserId: Guid.NewGuid()
        );

        Response(200, "The role of the user is returned.", example: new SuccessResponse<RoleDto?>(true, default));
    }
}
