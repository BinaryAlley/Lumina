#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Presentation.Web.Common.DTO.Authorization;
using Lumina.Presentation.Web.Common.DTO.Common;
using Lumina.Presentation.Web.Common.Requests.Authorization;
using System;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Presentation.Web.Core.Endpoints.Admin.UsersManagement.Authorization.GetUserPermissions;

/// <summary>
/// Class used for providing a textual description for the <see cref="GetUserPermissionsEndpoint"/> endpoint, for OpenAPI.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetUserPermissionsEndpointSummary : Summary<GetUserPermissionsEndpoint, GetUserPermissionsRequest>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GetUserPermissionsEndpointSummary"/> class.
    /// </summary>
    public GetUserPermissionsEndpointSummary()
    {
        Summary = "Gets the permissions of a user.";
        Description = "Retrieves the authorization permissions of a user identified by its unique identifier.";

        RequestParam(r => r.UserId, "The unique identifier of the user for whom to get the authorization permissions. Required.");

        ExampleRequest = new GetUserPermissionsRequest(
            UserId: Guid.NewGuid()
        );

        Response(200, "The permissions of the user are returned.", example: new SuccessResponse<PermissionDto[]>(true, default));
    }
}
