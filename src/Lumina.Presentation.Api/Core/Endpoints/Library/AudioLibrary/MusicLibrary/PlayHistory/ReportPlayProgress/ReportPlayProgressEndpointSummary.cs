#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Contracts.Requests.MediaLibrary.AudioLibrary.MusicLibrary.PlayHistory;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Presentation.Api.Core.Endpoints.Library.AudioLibrary.MusicLibrary.PlayHistory.ReportPlayProgress;

/// <summary>
/// Class used for providing a textual description for the <see cref="ReportPlayProgressEndpoint"/> API endpoint, for OpenAPI.
/// </summary>
[ExcludeFromCodeCoverage]
public class ReportPlayProgressEndpointSummary : Summary<ReportPlayProgressEndpoint, ReportPlayProgressRequest>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ReportPlayProgressEndpointSummary"/> class.
    /// </summary>
    public ReportPlayProgressEndpointSummary()
    {
        Summary = "Reports the progress of a play of a track by the current user.";
        Description = "Reports the last position and the listened duration of a play of a track by the current user. Only an in-progress play of the current user can be progressed, and the listened duration can never decrease. The reports are stored so that the play history, statistics and recommendations can be built from them.";

        ExampleRequest = new ReportPlayProgressRequest(
            PositionInSeconds: 92.5,
            ListenedDurationInSeconds: 92.5
        );

        RequestParam(request => request.PositionInSeconds, "The last position reported by the player, in seconds. Required.");
        RequestParam(request => request.ListenedDurationInSeconds, "The duration the user has listened to, in seconds. Required.");

        Response(200, "The progress of the play was reported.", example: new { });

        Response(401, "Authentication required.", "application/problem+json",
            example: new[]
            {
                new
                {
                    type = "https://tools.ietf.org/html/rfc7235#section-3.1",
                    status = 401,
                    title = "Unauthorized",
                    detail = "Authentication failed",
                    instance = $"/api/v1/users/me/music-play-history/plays/{Guid.NewGuid()}/progress"
                },
                new
                {
                    type = "https://tools.ietf.org/html/rfc7235#section-3.1",
                    status = 401,
                    title = "Unauthorized",
                    detail = "The token has expired",
                    instance = $"/api/v1/users/me/music-play-history/plays/{Guid.NewGuid()}/progress"
                },
                new
                {
                    type = "https://tools.ietf.org/html/rfc7235#section-3.1",
                    status = 401,
                    title = "Unauthorized",
                    detail = "The token is invalid",
                    instance = $"/api/v1/users/me/music-play-history/plays/{Guid.NewGuid()}/progress"
                }
            }
        );

        Response(404, "The request failed because the play record does not exist, or it does not belong to the current user.", "application/problem+json",
            example: new
            {
                type = "https://tools.ietf.org/html/rfc9110#section-15.5.5",
                title = "General.NotFound",
                status = 404,
                detail = "MusicPlayRecordNotFound",
                instance = $"/api/v1/users/me/music-play-history/plays/{Guid.NewGuid()}/progress",
                traceId = "00-57d15dadd702dbd4aeb5dc9b7cee68ee-9330237dbb2ce0e5-00"
            }
        );

        Response(409, "The request failed because the play is not in progress anymore, or because another report changed it after this request read it.", "application/problem+json",
            example: new[]
            {
                new
                {
                    type = "https://tools.ietf.org/html/rfc9110#section-15.5.10",
                    title = "General.Conflict",
                    status = 409,
                    detail = "PlayIsNotInProgress",
                    instance = $"/api/v1/users/me/music-play-history/plays/{Guid.NewGuid()}/progress",
                    traceId = "00-a712bbf99ca8ab485f86a762ae5ae74d-b3a2eb78813b0a5d-00"
                },
                new
                {
                    type = "https://tools.ietf.org/html/rfc9110#section-15.5.10",
                    title = "General.Conflict",
                    status = 409,
                    detail = "ConcurrencyConflict",
                    instance = $"/api/v1/users/me/music-play-history/plays/{Guid.NewGuid()}/progress",
                    traceId = "00-a712bbf99ca8ab485f86a762ae5ae74d-b3a2eb78813b0a5d-00"
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
                instance = $"/api/v1/users/me/music-play-history/plays/{Guid.NewGuid()}/progress",
                errors = new Dictionary<string, string[]>
                {
                    {
                        "General.Validation", new[]
                        {
                            "PlayRecordIdCannotBeEmpty",
                            "PlaybackPositionCannotBeNull",
                            "PlaybackPositionMustBeNonNegative",
                            "PlaybackPositionIsOutOfRange",
                            "ListenedDurationCannotBeNull",
                            "ListenedDurationMustBeNonNegative",
                            "ListenedDurationIsOutOfRange",
                            "ListenedDurationCannotDecrease"
                        }
                    }
                },
                traceId = "00-2470be4248a2a5a0c6f70579975a6954-b9c3ba9544a03500-00"
            }
        );
    }
}
