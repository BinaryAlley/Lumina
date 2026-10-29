#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Contracts.Requests.UsersManagement.Settings;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Presentation.Api.Core.Endpoints.UsersManagement.Settings.UpdatePlaybackState;

/// <summary>
/// Class used for providing a textual description for the <see cref="UpdatePlaybackStateEndpoint"/> API endpoint, for OpenAPI.
/// </summary>
[ExcludeFromCodeCoverage]
public class UpdatePlaybackStateEndpointSummary : Summary<UpdatePlaybackStateEndpoint, UpdatePlaybackStateRequest>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="UpdatePlaybackStateEndpointSummary"/> class.
    /// </summary>
    public UpdatePlaybackStateEndpointSummary()
    {
        Summary = "Updates the playback state the player reported for the current user.";
        Description = "Updates the last volume and mute state the player reported for the current user. This is written automatically by the player and is not part of the settings the user edits. When no settings are stored for the user yet, they are created with the provided playback state.";

        ExampleRequest = new UpdatePlaybackStateRequest(
            Volume: 0.75,
            IsMuted: false
        );

        RequestParam(r => r.Volume, "The last volume the player reported, as a value between 0 and 1. Required.");
        RequestParam(r => r.IsMuted, "The last mute state the player reported. Required.");

        Response(200, "The playback state of the current user was successfully updated.");

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

        Response(422, "The request did not pass validation checks.", "application/problem+json",
            example: new
            {
                type = "https://tools.ietf.org/html/rfc4918#section-11.2",
                title = "General.Validation",
                status = 422,
                detail = "OneOrMoreValidationErrorsOccurred",
                instance = "/api/v1/users/me/settings/playback-state",
                errors = new Dictionary<string, string[]>
                {
                    {
                        "General.Validation", new[]
                        {
                            "VolumeCannotBeNull",
                            "VolumeMustBeBetweenZeroAndOne",
                            "IsMutedCannotBeNull"
                        }
                    }
                },
                traceId = "00-2470be4248a2a5a0c6f70579975a6954-b9c3ba9544a03500-00"
            }
        );
    }
}
