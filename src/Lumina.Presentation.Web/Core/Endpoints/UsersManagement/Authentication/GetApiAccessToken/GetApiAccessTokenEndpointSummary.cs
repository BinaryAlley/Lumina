#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using System;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Presentation.Web.Core.Endpoints.UsersManagement.Authentication.GetApiAccessToken;

/// <summary>
/// Class used for providing a textual description for the <see cref="GetApiAccessTokenEndpoint"/> endpoint, for OpenAPI.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetApiAccessTokenEndpointSummary : Summary<GetApiAccessTokenEndpoint, EmptyRequest>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GetApiAccessTokenEndpointSummary"/> class.
    /// </summary>
    public GetApiAccessTokenEndpointSummary()
    {
        Summary = "Gets the API access token of the current user.";
        Description = "Gets the API access token of the current user, used to authenticate the real time SignalR connections of the current page.";

        Response(200, "The API access token of the current user is returned.",
            example: new
            {
                success = true,
                data = new
                {
                    token = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxIn0.example"
                }
            });
    }
}
