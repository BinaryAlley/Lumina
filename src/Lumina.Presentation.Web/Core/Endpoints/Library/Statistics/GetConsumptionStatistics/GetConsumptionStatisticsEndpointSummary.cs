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

namespace Lumina.Presentation.Web.Core.Endpoints.Library.Statistics.GetConsumptionStatistics;

/// <summary>
/// Class used for providing a textual description for the <see cref="GetConsumptionStatisticsEndpoint"/> endpoint, for OpenAPI.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetConsumptionStatisticsEndpointSummary : Summary<GetConsumptionStatisticsEndpoint, GetConsumptionStatisticsRequest>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GetConsumptionStatisticsEndpointSummary"/> class.
    /// </summary>
    public GetConsumptionStatisticsEndpointSummary()
    {
        Summary = "Gets the consumption statistics of the selected user.";
        Description = "Proxies the request that gets the total consumed duration, the counted sessions, the completed sessions, the stopped sessions, the completion rate, and the per media kind breakdown of the selected user to the API server. When userId is omitted, the statistics of the current user are returned; an administrator can name another user. The date range is inclusive. A failure is returned as a JSON object with success set to false and a localized errorMessage.";

        ExampleRequest = new GetConsumptionStatisticsRequest(
            UserId: null,
            LibraryId: null,
            MediaKind: null,
            FromUtc: new DateTime(2026, 10, 1, 0, 0, 0, DateTimeKind.Utc),
            ToUtc: new DateTime(2026, 10, 31, 23, 59, 59, DateTimeKind.Utc)
        );

        RequestParam(request => request.UserId, "The Id of the user whose statistics are retrieved. Optional; when omitted, the current user is used.");
        RequestParam(request => request.LibraryId, "The Id of the media library to restrict the statistics to. Optional.");
        RequestParam(request => request.MediaKind, "The kind of media to restrict the statistics to. Optional.");
        RequestParam(request => request.FromUtc, "The inclusive start of the date range. Optional.");
        RequestParam(request => request.ToUtc, "The inclusive end of the date range. Optional.");

        ResponseParam<SuccessResponse<ConsumptionSummaryDto>>(response => response.IsSuccess, "Whether the operation succeeded.");
        ResponseParam<ConsumptionSummaryDto>(response => response.UserId, "The Id of the user the statistics belong to.");
        ResponseParam<ConsumptionSummaryDto>(response => response.TotalConsumedDurationInSeconds, "The total duration consumed over every recorded session, in seconds.");
        ResponseParam<ConsumptionSummaryDto>(response => response.CountedSessionCount, "The number of recorded sessions that count towards statistics.");
        ResponseParam<ConsumptionSummaryDto>(response => response.CompletedSessionCount, "The number of recorded sessions that completed.");
        ResponseParam<ConsumptionSummaryDto>(response => response.StoppedSessionCount, "The number of recorded sessions that stopped before they completed and count towards statistics.");
        ResponseParam<ConsumptionSummaryDto>(response => response.CompletionRate, "The fraction of the counted sessions that completed, between 0 and 1, or 0 when there is no counted session.");
        ResponseParam<ConsumptionSummaryDto>(response => response.MediaKindSummaries, "The consumption statistics broken down by media kind.");
        ResponseParam<MediaKindConsumptionSummaryDto>(response => response.MediaKind, "The kind of media the breakdown entry belongs to.");
        ResponseParam<MediaKindConsumptionSummaryDto>(response => response.TotalConsumedDurationInSeconds, "The total duration consumed over every recorded session of the media kind, in seconds.");
        ResponseParam<MediaKindConsumptionSummaryDto>(response => response.CountedSessionCount, "The number of recorded sessions of the media kind that count towards statistics.");
        ResponseParam<MediaKindConsumptionSummaryDto>(response => response.CompletedSessionCount, "The number of recorded sessions of the media kind that completed.");
        ResponseParam<MediaKindConsumptionSummaryDto>(response => response.StoppedSessionCount, "The number of recorded sessions of the media kind that stopped before they completed and count towards statistics.");
        ResponseParam<MediaKindConsumptionSummaryDto>(response => response.CompletionRate, "The fraction of the counted sessions of the media kind that completed, between 0 and 1, or 0 when there is no counted session.");

        Response(200, "The consumption statistics of the selected user are returned, or a JSON error with success set to false.",
            example: new SuccessResponse<ConsumptionSummaryDto>(true, new ConsumptionSummaryDto(
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
                    ),
                    new MediaKindConsumptionSummaryDto(
                        MediaKind: MediaKind.Book,
                        TotalConsumedDurationInSeconds: 7320,
                        CountedSessionCount: 2,
                        CompletedSessionCount: 1,
                        StoppedSessionCount: 1,
                        CompletionRate: 0.5
                    )
                ]))
        );
    }
}
