#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Presentation.Web.Common.DTO.Common;
using Lumina.Presentation.Web.Common.DTO.MediaConsumption;
using Lumina.Presentation.Web.Common.Enums.MediaConsumption;
using Lumina.Presentation.Web.Common.Requests.MediaConsumption;
using System;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Presentation.Web.Core.Endpoints.MediaConsumption.ReportConsumptionProgress;

/// <summary>
/// Class used for providing a textual description for the <see cref="ReportConsumptionProgressEndpoint"/> endpoint, for OpenAPI.
/// </summary>
[ExcludeFromCodeCoverage]
public class ReportConsumptionProgressEndpointSummary : Summary<ReportConsumptionProgressEndpoint, ReportConsumptionProgressRequest>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ReportConsumptionProgressEndpointSummary"/> class.
    /// </summary>
    public ReportConsumptionProgressEndpointSummary()
    {
        Summary = "Reports the progress of a consumption session of the current user.";
        Description = "Proxies the latest position, progress fraction, and active consumed duration of an in-progress consumption session to the API server. The position and the progress fraction are the latest reported values and may move backwards. The consumed duration is the only monotonic measurement. A record of another user is reported with the same not found result as a missing record.";

        Params["recordId"] = "The Id of the consumption record.";

        ExampleRequest = new ReportConsumptionProgressRequest(
            Position: new MediaPositionRequest("TimeOffset", 92.5, null),
            ProgressFraction: 0.31,
            ConsumedDurationInSeconds: 92.5,
            ConsecutivePagesRead: 0
        );
        RequestExamples.Add(new RequestExample(new ReportConsumptionProgressRequest(
            Position: new MediaPositionRequest("Location", null, "epubcfi(/6/4[chapter-3]!/4/2/2)"),
            ProgressFraction: 0.42,
            ConsumedDurationInSeconds: 240,
            ConsecutivePagesRead: 2
        )));
        RequestParam(request => request.Position, "The last position reported for the media item, if the client knows it. It carries the position Kind, and either TimeOffsetSeconds or Location. Optional.");
        RequestParam(request => request.ProgressFraction, "The fraction of the media item that the client reached, between 0 and 1. Required.");
        RequestParam(request => request.ConsumedDurationInSeconds, "The active duration consumed by the session, in seconds. Required.");
        RequestParam(request => request.ConsecutivePagesRead, "The number of consecutive pages read in the current session, used by page based media such as books. It must be zero for other kinds; a positive value is rejected with a forbidden error. Optional, defaults to 0.");

        Response(200, "The progress of the consumption session is reported and the updated record is returned.",
            example: new SuccessResponse<ConsumptionRecordDto>(true, new ConsumptionRecordDto(
                Id: Guid.Parse("2c1b0a9f-8e7d-4c6b-5a4f-3e2d1c0b9a8f"),
                LibraryId: Guid.Parse("3b3a19f3-1f5a-4d5a-9a3a-5c5a4a3a2a1a"),
                MediaItemId: Guid.Parse("9f0e1d2c-3b4a-4c6d-8e7f-9a0b1c2d3e4f"),
                MediaKind: MediaKind.MusicTrack,
                Status: ConsumptionStatus.InProgress,
                StartedOnUtc: new DateTime(2026, 10, 24, 12, 0, 0, DateTimeKind.Utc),
                LastActivityOnUtc: new DateTime(2026, 10, 24, 12, 1, 32, 500, DateTimeKind.Utc),
                EndedOnUtc: null,
                ConsumedDurationInSeconds: 92.5,
                LastPosition: new MediaPositionDto(MediaPositionKind.TimeOffset, 92.5, null),
                LastProgressFraction: 0.31,
                ConsecutivePagesRead: 0))
        );
    }
}
