#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Presentation.Web.Common.DTO.Authorization;
using Lumina.Presentation.Web.Common.DTO.Common;
using System;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Presentation.Web.Core.Endpoints.Admin.Authorization.Roles.GetRoles;

/// <summary>
/// Class used for providing a textual description for the <see cref="GetRolesEndpoint"/> endpoint, for OpenAPI.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetRolesEndpointSummary : Summary<GetRolesEndpoint, EmptyRequest>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GetRolesEndpointSummary"/> class.
    /// </summary>
    public GetRolesEndpointSummary()
    {
        Summary = "Gets all roles.";
        Description = "Retrieves the collection of existing authorization roles.";

        Response(200, "The collection of authorization roles is returned.", example: new SuccessResponse<RoleDto[]>(true, default));
    }
}
