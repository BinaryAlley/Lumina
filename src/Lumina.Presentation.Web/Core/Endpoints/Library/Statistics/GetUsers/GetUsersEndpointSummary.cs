#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Presentation.Web.Common.DTO.Common;
using Lumina.Presentation.Web.Common.DTO.UsersManagement;
using System;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Presentation.Web.Core.Endpoints.Library.Statistics.GetUsers;

/// <summary>
/// Class used for providing a textual description for the <see cref="GetUsersEndpoint"/> endpoint, for OpenAPI.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetUsersEndpointSummary : Summary<GetUsersEndpoint, EmptyRequest>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GetUsersEndpointSummary"/> class.
    /// </summary>
    public GetUsersEndpointSummary()
    {
        Summary = "Gets the users available for the administrator user selector.";
        Description = "Proxies the request that gets the list of users to the API server. Only an administrator can call it. A failure is returned as a JSON object with success set to false and a localized errorMessage.";

        ResponseParam<SuccessResponse<UserDto[]>>(response => response.IsSuccess, "Whether the operation succeeded.");
        ResponseParam<UserDto>(response => response.Id, "The unique identifier of the user.");
        ResponseParam<UserDto>(response => response.Username, "The username of the user.");

        Response(200, "The users available for the selector are returned, or a JSON error with success set to false.",
            example: new SuccessResponse<UserDto[]>(true,
            [
                new UserDto(
                    Id: Guid.Parse("1a2b3c4d-5e6f-4a1b-8c2d-3e4f5a6b7c8d"),
                    Username: "john.doe"),
                new UserDto(
                    Id: Guid.Parse("2b3c4d5e-6f7a-4b8c-9d0e-1f2a3b4c5d6e"),
                    Username: "jane.doe")
            ])
        );
    }
}
