#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Contracts.Requests.MediaConsumption;
using Lumina.Contracts.Responses.MediaConsumption;
using Lumina.Domain.SharedKernel.Common.Enums.MediaLibrary;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Presentation.Api.Core.Endpoints.Library.Statistics.GetConsumptionStatistics;

/// <summary>
/// Class used for providing a textual description for the <see cref="GetUserConsumptionStatisticsEndpoint"/> API endpoint, for OpenAPI.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetUserConsumptionStatisticsEndpointSummary : Summary<GetUserConsumptionStatisticsEndpoint, GetConsumptionStatisticsRequest>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GetUserConsumptionStatisticsEndpointSummary"/> class.
    /// </summary>
    public GetUserConsumptionStatisticsEndpointSummary()
    {
        Summary = "Gets the consumption statistics of a user.";
        Description = "Gets the consolidated consumption statistics of a user, across every kind of media or restricted to one kind, one library, and a date range. A user can read only their own statistics, while an Admin can select any user with the userId route parameter; a regular user targeting another user receives the same not authorized result as a route that names no accessible user. A session counts only when it completed, or when it reached the counting threshold of its media kind: 30 consecutive seconds of a music track or a podcast episode, 60 consecutive seconds of an audiobook, 3 consecutive minutes of a movie, a TV episode or a video, more than 5 consecutive pages of a book, or every view of a photo. The consumed duration totals every recorded session, including the sessions that do not count, while the counted, completed, and stopped session counts follow the counting rules. The completion rate is the fraction of the counted sessions that completed. Music and book consumption are reported in the same summary, broken down by media kind. The statistics are derived from the recorded sessions, so deleting history changes them immediately.";

        ExampleRequest = new GetConsumptionStatisticsRequest(
            LibraryId: null,
            MediaKind: null,
            FromUtc: new DateTime(2026, 10, 1, 0, 0, 0, DateTimeKind.Utc),
            ToUtc: new DateTime(2026, 10, 31, 23, 59, 59, DateTimeKind.Utc)
        );

        Params["userId"] = "The Id of the user whose statistics are retrieved. An Admin can name any user; a regular user can only name themselves.";
        RequestParam(request => request.LibraryId, "The Id of the media library to restrict the statistics to. Optional.");
        RequestParam(request => request.MediaKind, "The kind of media to restrict the statistics to. Optional.");
        RequestParam(request => request.FromUtc, "The inclusive start of the date range. Optional.");
        RequestParam(request => request.ToUtc, "The inclusive end of the date range. Optional.");

        ResponseParam<ConsumptionSummaryResponse>(response => response.UserId, "The Id of the user the statistics belong to.");
        ResponseParam<ConsumptionSummaryResponse>(response => response.TotalConsumedDurationInSeconds, "The total duration consumed over every recorded session, in seconds.");
        ResponseParam<ConsumptionSummaryResponse>(response => response.CountedSessionCount, "The number of recorded sessions that count towards statistics.");
        ResponseParam<ConsumptionSummaryResponse>(response => response.CompletedSessionCount, "The number of recorded sessions that completed.");
        ResponseParam<ConsumptionSummaryResponse>(response => response.StoppedSessionCount, "The number of recorded sessions that stopped before they completed and count towards statistics.");
        ResponseParam<ConsumptionSummaryResponse>(response => response.CompletionRate, "The fraction of the counted sessions that completed, between 0 and 1, or 0 when there is no counted session.");
        ResponseParam<ConsumptionSummaryResponse>(response => response.MediaKindSummaries, "The consumption statistics broken down by media kind. Each entry carries the media kind, the consumed duration, the counted, completed and stopped session counts, and the completion rate of that kind.");

        Response<ConsumptionSummaryResponse>(statusCode: 200, description: "The consumption statistics of the user are returned.", contentType: "application/json",
            example: new ConsumptionSummaryResponse(
                UserId: Guid.Parse("1a2b3c4d-5e6f-4a1b-8c2d-3e4f5a6b7c8d"),
                TotalConsumedDurationInSeconds: 8744.5,
                CountedSessionCount: 6,
                CompletedSessionCount: 4,
                StoppedSessionCount: 2,
                CompletionRate: 0.6667,
                MediaKindSummaries:
                [
                    new MediaKindConsumptionSummaryResponse(
                        MediaKind: MediaKind.MusicTrack,
                        TotalConsumedDurationInSeconds: 1424.5,
                        CountedSessionCount: 4,
                        CompletedSessionCount: 3,
                        StoppedSessionCount: 1,
                        CompletionRate: 0.75
                    ),
                    new MediaKindConsumptionSummaryResponse(
                        MediaKind: MediaKind.Book,
                        TotalConsumedDurationInSeconds: 7320,
                        CountedSessionCount: 2,
                        CompletedSessionCount: 1,
                        StoppedSessionCount: 1,
                        CompletionRate: 0.5
                    )
                ]
            )
        );

        Response(401, "Authentication required.", "application/problem+json",
            example: new[]
            {
                new
                {
                    type = "https://tools.ietf.org/html/rfc7235#section-3.1",
                    status = 401,
                    title = "Unauthorized",
                    detail = "Authentication failed",
                    instance = "/api/v1/users/{userId}/consumption-statistics"
                },
                new
                {
                    type = "https://tools.ietf.org/html/rfc7235#section-3.1",
                    status = 401,
                    title = "Unauthorized",
                    detail = "The token has expired",
                    instance = "/api/v1/users/{userId}/consumption-statistics"
                }
            }
        );

        Response(403, "The request failed because the authenticated request does not carry the identity of a user, or because the current user is neither the target user nor an Admin.", "application/problem+json",
            example: new
            {
                type = "https://tools.ietf.org/html/rfc9110#section-15.5.4",
                title = "General.Unauthorized",
                status = 403,
                detail = "NotAuthorized",
                instance = "/api/v1/users/{userId}/consumption-statistics",
                traceId = "00-a712bbf99ca8ab485f86a762ae5ae74d-b3a2eb78813b0a5d-00"
            }
        );

        Response(422, "The request did not pass validation checks.", "application/problem+json",
            example: new
            {
                type = "https://tools.ietf.org/html/rfc4918#section-11.2",
                title = "General.Validation",
                status = 422,
                detail = "OneOrMoreValidationErrorsOccurred",
                instance = "/api/v1/users/{userId}/consumption-statistics",
                errors = new Dictionary<string, string[]>
                {
                    {
                        "General.Validation", new[]
                        {
                            "TargetUserIdIsInvalid",
                            "LibraryIdCannotBeEmpty",
                            "UnsupportedMediaKind",
                            "ConsumptionDateRangeIsInvalid"
                        }
                    }
                },
                traceId = "00-2470be4248a2a5a0c6f70579975a6954-b9c3ba9544a03500-00"
            }
        );
    }
}
