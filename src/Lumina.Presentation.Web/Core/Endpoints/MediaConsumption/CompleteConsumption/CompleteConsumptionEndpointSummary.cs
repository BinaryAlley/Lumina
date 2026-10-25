#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Presentation.Web.Common.DTO.Common;
using Lumina.Presentation.Web.Common.DTO.MediaConsumption;
using Lumina.Presentation.Web.Common.Enums.MediaConsumption;
using Lumina.Presentation.Web.Common.Requests.MediaConsumption;
using System;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Presentation.Web.Core.Endpoints.MediaConsumption.CompleteConsumption;

/// <summary>
/// Class used for providing a textual description for the <see cref="CompleteConsumptionEndpoint"/> endpoint, for OpenAPI.
/// </summary>
[ExcludeFromCodeCoverage]
public class CompleteConsumptionEndpointSummary : Summary<CompleteConsumptionEndpoint, CompleteConsumptionRequest>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CompleteConsumptionEndpointSummary"/> class.
    /// </summary>
    public CompleteConsumptionEndpointSummary()
    {
        Summary = "Completes a consumption session of the current user.";
        Description = "Proxies the completion of an in-progress consumption session to the API server. The final position, progress fraction, and active consumed duration are stored, and the resume point of the media item is deleted. A record of another user is reported with the same not found result as a missing record.";

        Params["recordId"] = "The Id of the consumption record.";

        ExampleRequest = new CompleteConsumptionRequest(
            Position: new MediaPositionRequest("TimeOffset", 100, null),
            ProgressFraction: 1,
            ConsumedDurationInSeconds: 100
        );
        RequestParam(request => request.Position, "The final position reported for the media item, if the client knows it. It carries the position Kind, and either TimeOffsetSeconds or Location. Optional.");
        RequestParam(request => request.ProgressFraction, "The fraction of the media item that the client reached, between 0 and 1. Required.");
        RequestParam(request => request.ConsumedDurationInSeconds, "The active duration consumed by the session, in seconds. Required.");

        Response(200, "The consumption session is completed and the updated record is returned.",
            example: new SuccessResponse<ConsumptionRecordDto>(true, new ConsumptionRecordDto(
                Id: Guid.Parse("2c1b0a9f-8e7d-4c6b-5a4f-3e2d1c0b9a8f"),
                LibraryId: Guid.Parse("3b3a19f3-1f5a-4d5a-9a3a-5c5a4a3a2a1a"),
                MediaItemId: Guid.Parse("9f0e1d2c-3b4a-4c6d-8e7f-9a0b1c2d3e4f"),
                MediaKind: MediaKind.MusicTrack,
                Status: ConsumptionStatus.Completed,
                StartedOnUtc: new DateTime(2026, 10, 24, 12, 0, 0, DateTimeKind.Utc),
                LastActivityOnUtc: new DateTime(2026, 10, 24, 12, 1, 40, DateTimeKind.Utc),
                EndedOnUtc: new DateTime(2026, 10, 24, 12, 1, 40, DateTimeKind.Utc),
                ConsumedDurationInSeconds: 100,
                LastPosition: new MediaPositionDto(MediaPositionKind.TimeOffset, 100, null),
                LastProgressFraction: 1,
                ConsecutivePagesRead: 0))
        );
    }
}
