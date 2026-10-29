#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Contracts.Requests.MediaConsumption;
using Lumina.Contracts.Responses.MediaConsumption;
using Lumina.Domain.SharedKernel.Common.Enums.MediaLibrary;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Presentation.Api.Core.Endpoints.Library.Statistics.GetAllUsersConsumptionStatistics;

/// <summary>
/// Class used for providing a textual description for the <see cref="GetAllUsersConsumptionStatisticsEndpoint"/> API endpoint, for OpenAPI.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetAllUsersConsumptionStatisticsEndpointSummary : Summary<GetAllUsersConsumptionStatisticsEndpoint, GetAllUsersConsumptionStatisticsRequest>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GetAllUsersConsumptionStatisticsEndpointSummary"/> class.
    /// </summary>
    public GetAllUsersConsumptionStatisticsEndpointSummary()
    {
        Summary = "Gets the consumption statistics of every user.";
        Description = "Gets the consumption statistics of every user of the server, together with their combined totals, across every kind of media or restricted to one kind, one library, and a date range. Only an Admin can call this route; a regular user receives the same not authorized result as any other administration route. A session counts only when it completed, or when it reached the counting threshold of its media kind: 30 consecutive seconds of a music track or a podcast episode, 60 consecutive seconds of an audiobook, 3 consecutive minutes of a movie, a TV episode or a video, more than 5 consecutive pages of a book, or every view of a photo. The consumed duration totals every recorded session, including the sessions that do not count, while the counted, completed, and stopped session counts follow the counting rules. Each user summary carries its own breakdown by media kind, and the combined totals are derived from those summaries, so they can never disagree with the sum of the users.";

        ExampleRequest = new GetAllUsersConsumptionStatisticsRequest(
            LibraryId: null,
            MediaKind: null,
            FromUtc: "2026-10-01T00:00:00Z",
            ToUtc: "2026-10-31T23:59:59Z"
        );

        RequestParam(request => request.LibraryId, "The Id of the media library to restrict the statistics to. Optional.");
        RequestParam(request => request.MediaKind, "The name of the kind of media to restrict the statistics to. Optional. Only the member names of the media kind enumeration are accepted.");
        RequestParam(request => request.FromUtc, "The inclusive start of the date range. Optional.");
        RequestParam(request => request.ToUtc, "The inclusive end of the date range. Optional.");

        ResponseParam<AllUsersConsumptionStatisticsResponse>(response => response.Total, "The combined consumption statistics of every user.");
        ResponseParam<AllUsersConsumptionStatisticsResponse>(response => response.Users, "The consumption statistics of each user, ordered by user Id. Each summary carries the user Id and the same fields as a single user summary.");
        ResponseParam<ConsumptionTotalsResponse>(response => response.TotalConsumedDurationInSeconds, "The total duration consumed over every recorded session of every user, in seconds.");
        ResponseParam<ConsumptionTotalsResponse>(response => response.CountedSessionCount, "The number of recorded sessions of every user that count towards statistics.");
        ResponseParam<ConsumptionTotalsResponse>(response => response.CompletedSessionCount, "The number of recorded sessions of every user that completed.");
        ResponseParam<ConsumptionTotalsResponse>(response => response.StoppedSessionCount, "The number of recorded sessions of every user that stopped before they completed and count towards statistics.");
        ResponseParam<ConsumptionTotalsResponse>(response => response.CompletionRate, "The fraction of the counted sessions of every user that completed, between 0 and 1, or 0 when there is no counted session.");
        ResponseParam<ConsumptionTotalsResponse>(response => response.MediaKindSummaries, "The combined consumption statistics broken down by media kind.");
        ResponseParam<ConsumptionSummaryResponse>(response => response.UserId, "The Id of the user the summary belongs to.");
        ResponseParam<ConsumptionSummaryResponse>(response => response.TotalConsumedDurationInSeconds, "The total duration consumed over every recorded session of the user, in seconds.");
        ResponseParam<ConsumptionSummaryResponse>(response => response.CountedSessionCount, "The number of recorded sessions of the user that count towards statistics.");
        ResponseParam<ConsumptionSummaryResponse>(response => response.CompletedSessionCount, "The number of recorded sessions of the user that completed.");
        ResponseParam<ConsumptionSummaryResponse>(response => response.StoppedSessionCount, "The number of recorded sessions of the user that stopped before they completed and count towards statistics.");
        ResponseParam<ConsumptionSummaryResponse>(response => response.CompletionRate, "The fraction of the counted sessions of the user that completed, between 0 and 1.");
        ResponseParam<ConsumptionSummaryResponse>(response => response.MediaKindSummaries, "The consumption statistics of the user broken down by media kind.");
        ResponseParam<MediaKindConsumptionSummaryResponse>(response => response.MediaKind, "The kind of media the breakdown entry belongs to.");
        ResponseParam<MediaKindConsumptionSummaryResponse>(response => response.TotalConsumedDurationInSeconds, "The total duration consumed over every recorded session of the media kind, in seconds.");
        ResponseParam<MediaKindConsumptionSummaryResponse>(response => response.CountedSessionCount, "The number of recorded sessions of the media kind that count towards statistics.");
        ResponseParam<MediaKindConsumptionSummaryResponse>(response => response.CompletedSessionCount, "The number of recorded sessions of the media kind that completed.");
        ResponseParam<MediaKindConsumptionSummaryResponse>(response => response.StoppedSessionCount, "The number of recorded sessions of the media kind that stopped before they completed and count towards statistics.");
        ResponseParam<MediaKindConsumptionSummaryResponse>(response => response.CompletionRate, "The fraction of the counted sessions of the media kind that completed, between 0 and 1.");

        Response<AllUsersConsumptionStatisticsResponse>(statusCode: 200, description: "The consumption statistics of every user are returned.", contentType: "application/json",
            example: new AllUsersConsumptionStatisticsResponse(
                Total: new ConsumptionTotalsResponse(
                    TotalConsumedDurationInSeconds: 12874.5,
                    CountedSessionCount: 9,
                    CompletedSessionCount: 6,
                    StoppedSessionCount: 3,
                    CompletionRate: 0.6667,
                    MediaKindSummaries:
                    [
                        new MediaKindConsumptionSummaryResponse(
                            MediaKind: MediaKind.MusicTrack,
                            TotalConsumedDurationInSeconds: 5554.5,
                            CountedSessionCount: 7,
                            CompletedSessionCount: 5,
                            StoppedSessionCount: 2,
                            CompletionRate: 0.7143
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
                ),
                Users:
                [
                    new ConsumptionSummaryResponse(
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
                    ),
                    new ConsumptionSummaryResponse(
                        UserId: Guid.Parse("2b3c4d5e-6f70-4a1b-9c3d-4e5f6a7b8c9d"),
                        TotalConsumedDurationInSeconds: 4130,
                        CountedSessionCount: 3,
                        CompletedSessionCount: 2,
                        StoppedSessionCount: 1,
                        CompletionRate: 0.6667,
                        MediaKindSummaries:
                        [
                            new MediaKindConsumptionSummaryResponse(
                                MediaKind: MediaKind.MusicTrack,
                                TotalConsumedDurationInSeconds: 4130,
                                CountedSessionCount: 3,
                                CompletedSessionCount: 2,
                                StoppedSessionCount: 1,
                                CompletionRate: 0.6667
                            )
                        ]
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
                    instance = "/api/v1/users/consumption-statistics"
                },
                new
                {
                    type = "https://tools.ietf.org/html/rfc7235#section-3.1",
                    status = 401,
                    title = "Unauthorized",
                    detail = "The token has expired",
                    instance = "/api/v1/users/consumption-statistics"
                }
            }
        );

        Response(403, "The request failed because the authenticated request does not carry the identity of a user, or because the current user is not an Admin.", "application/problem+json",
            example: new
            {
                type = "https://tools.ietf.org/html/rfc9110#section-15.5.4",
                title = "General.Unauthorized",
                status = 403,
                detail = "NotAuthorized",
                instance = "/api/v1/users/consumption-statistics",
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
                instance = "/api/v1/users/consumption-statistics",
                errors = new Dictionary<string, string[]>
                {
                    {
                        "General.Validation", new[]
                        {
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
