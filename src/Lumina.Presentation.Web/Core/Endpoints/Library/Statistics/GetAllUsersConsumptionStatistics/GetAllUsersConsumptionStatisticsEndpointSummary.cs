#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Presentation.Web.Common.DTO.Common;
using Lumina.Presentation.Web.Common.DTO.MediaConsumption;
using Lumina.Presentation.Web.Common.Enums.MediaConsumption;
using Lumina.Presentation.Web.Common.Requests.Library.Statistics;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Presentation.Web.Core.Endpoints.Library.Statistics.GetAllUsersConsumptionStatistics;

/// <summary>
/// Class used for providing a textual description for the <see cref="GetAllUsersConsumptionStatisticsEndpoint"/> endpoint, for OpenAPI.
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
        Description = "Proxies the request that gets the combined consumption statistics of every user, together with the statistics of each user, to the API server. Only an administrator can call it. The date range is inclusive. A failure is returned as a JSON object with success set to false and a localized errorMessage.";

        ExampleRequest = new GetAllUsersConsumptionStatisticsRequest(
            LibraryId: null,
            MediaKind: null,
            FromUtc: new DateTime(2026, 10, 1, 0, 0, 0, DateTimeKind.Utc),
            ToUtc: new DateTime(2026, 10, 31, 23, 59, 59, DateTimeKind.Utc)
        );

        RequestParam(request => request.LibraryId, "The Id of the media library to restrict the statistics to. Optional.");
        RequestParam(request => request.MediaKind, "The kind of media to restrict the statistics to. Optional.");
        RequestParam(request => request.FromUtc, "The inclusive start of the date range. Optional.");
        RequestParam(request => request.ToUtc, "The inclusive end of the date range. Optional.");

        ResponseParam<SuccessResponse<AllUsersConsumptionStatisticsDto>>(response => response.IsSuccess, "Whether the operation succeeded.");
        ResponseParam<AllUsersConsumptionStatisticsDto>(response => response.Total, "The combined consumption statistics of every user.");
        ResponseParam<AllUsersConsumptionStatisticsDto>(response => response.Users, "The consumption statistics of each user, ordered by user Id.");
        ResponseParam<ConsumptionTotalsDto>(response => response.TotalConsumedDurationInSeconds, "The total duration consumed over every recorded session of every user, in seconds.");
        ResponseParam<ConsumptionTotalsDto>(response => response.CountedSessionCount, "The number of recorded sessions of every user that count towards statistics.");
        ResponseParam<ConsumptionTotalsDto>(response => response.CompletedSessionCount, "The number of recorded sessions of every user that completed.");
        ResponseParam<ConsumptionTotalsDto>(response => response.StoppedSessionCount, "The number of recorded sessions of every user that stopped before they completed and count towards statistics.");
        ResponseParam<ConsumptionTotalsDto>(response => response.CompletionRate, "The fraction of the counted sessions of every user that completed, between 0 and 1, or 0 when there is no counted session.");
        ResponseParam<ConsumptionTotalsDto>(response => response.MediaKindSummaries, "The combined consumption statistics broken down by media kind.");
        ResponseParam<ConsumptionSummaryDto>(response => response.UserId, "The Id of the user the summary belongs to.");
        ResponseParam<ConsumptionSummaryDto>(response => response.TotalConsumedDurationInSeconds, "The total duration consumed over every recorded session of the user, in seconds.");
        ResponseParam<ConsumptionSummaryDto>(response => response.CountedSessionCount, "The number of recorded sessions of the user that count towards statistics.");
        ResponseParam<ConsumptionSummaryDto>(response => response.CompletedSessionCount, "The number of recorded sessions of the user that completed.");
        ResponseParam<ConsumptionSummaryDto>(response => response.StoppedSessionCount, "The number of recorded sessions of the user that stopped before they completed and count towards statistics.");
        ResponseParam<ConsumptionSummaryDto>(response => response.CompletionRate, "The fraction of the counted sessions of the user that completed, between 0 and 1, or 0 when there is no counted session.");
        ResponseParam<ConsumptionSummaryDto>(response => response.MediaKindSummaries, "The consumption statistics of the user broken down by media kind.");
        ResponseParam<MediaKindConsumptionSummaryDto>(response => response.MediaKind, "The kind of media the breakdown entry belongs to.");
        ResponseParam<MediaKindConsumptionSummaryDto>(response => response.TotalConsumedDurationInSeconds, "The total duration consumed over every recorded session of the media kind, in seconds.");
        ResponseParam<MediaKindConsumptionSummaryDto>(response => response.CountedSessionCount, "The number of recorded sessions of the media kind that count towards statistics.");
        ResponseParam<MediaKindConsumptionSummaryDto>(response => response.CompletedSessionCount, "The number of recorded sessions of the media kind that completed.");
        ResponseParam<MediaKindConsumptionSummaryDto>(response => response.StoppedSessionCount, "The number of recorded sessions of the media kind that stopped before they completed and count towards statistics.");
        ResponseParam<MediaKindConsumptionSummaryDto>(response => response.CompletionRate, "The fraction of the counted sessions of the media kind that completed, between 0 and 1, or 0 when there is no counted session.");

        Response(200, "The consumption statistics of every user are returned, or a JSON error with success set to false.",
            example: new SuccessResponse<AllUsersConsumptionStatisticsDto>(true, new AllUsersConsumptionStatisticsDto(
                Total: new ConsumptionTotalsDto(
                    TotalConsumedDurationInSeconds: 8744.5,
                    CountedSessionCount: 6,
                    CompletedSessionCount: 4,
                    StoppedSessionCount: 2,
                    CompletionRate: 0.6667,
                    MediaKindSummaries:
                    [
                        new MediaKindConsumptionSummaryDto(
                            MediaKind: MediaKind.MusicTrack,
                            TotalConsumedDurationInSeconds: 1424.5,
                            CountedSessionCount: 4,
                            CompletedSessionCount: 3,
                            StoppedSessionCount: 1,
                            CompletionRate: 0.75
                        ),
                        new MediaKindConsumptionSummaryDto(
                            MediaKind: MediaKind.Book,
                            TotalConsumedDurationInSeconds: 7320,
                            CountedSessionCount: 2,
                            CompletedSessionCount: 1,
                            StoppedSessionCount: 1,
                            CompletionRate: 0.5
                        )
                    ]),
                Users:
                [
                    new ConsumptionSummaryDto(
                        UserId: Guid.Parse("1a2b3c4d-5e6f-4a1b-8c2d-3e4f5a6b7c8d"),
                        TotalConsumedDurationInSeconds: 8744.5,
                        CountedSessionCount: 6,
                        CompletedSessionCount: 4,
                        StoppedSessionCount: 2,
                        CompletionRate: 0.6667,
                        MediaKindSummaries:
                        [
                            new MediaKindConsumptionSummaryDto(
                                MediaKind: MediaKind.MusicTrack,
                                TotalConsumedDurationInSeconds: 1424.5,
                                CountedSessionCount: 4,
                                CompletedSessionCount: 3,
                                StoppedSessionCount: 1,
                                CompletionRate: 0.75
                            )
                        ])
                ]))
        );
    }
}
