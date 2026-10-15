#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Contracts.Responses.UsersManagement.Settings;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Presentation.Api.Core.Endpoints.UsersManagement.Settings.GetPlaybackState;

/// <summary>
/// Class used for providing a textual description for the <see cref="GetPlaybackStateEndpoint"/> API endpoint, for OpenAPI.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetPlaybackStateEndpointSummary : Summary<GetPlaybackStateEndpoint, EmptyRequest>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GetPlaybackStateEndpointSummary"/> class.
    /// </summary>
    public GetPlaybackStateEndpointSummary()
    {
        Summary = "Retrieves the playback state the player reported for the current user.";
        Description = "Retrieves the last volume and mute state the player reported for the current user. When no settings are stored for the user yet, the default values are returned.";

        Response(200, "The playback state of the current user is returned.",
            example: new PlaybackStateResponse(
                Volume: 1.0,
                IsMuted: false
            ));

        Response(401, "Authentication required.", "application/problem+json",
            example: new[]
            {
                new
                {
                    type = "https://tools.ietf.org/html/rfc7235#section-3.1",
                    status = 401,
                    title = "Unauthorized",
                    detail = "Authentication failed",
                    instance = "/api/v1/users/me/settings/playback-state"
                },
                new
                {
                    type = "https://tools.ietf.org/html/rfc7235#section-3.1",
                    status = 401,
                    title = "Unauthorized",
                    detail = "The token has expired",
                    instance = "/api/v1/users/me/settings/playback-state"
                },
                new
                {
                    type = "https://tools.ietf.org/html/rfc7235#section-3.1",
                    status = 401,
                    title = "Unauthorized",
                    detail = "The token is invalid",
                    instance = "/api/v1/users/me/settings/playback-state"
                }
            }
        );
    }
}
