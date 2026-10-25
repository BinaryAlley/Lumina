#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Presentation.Web.Common.DTO.Common;
using Lumina.Presentation.Web.Common.DTO.MediaConsumption;
using Lumina.Presentation.Web.Common.Enums.MediaConsumption;
using System;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Presentation.Web.Core.Endpoints.MediaConsumption.StartConsumption;

/// <summary>
/// Class used for providing a textual description for the <see cref="StartConsumptionEndpoint"/> endpoint, for OpenAPI.
/// </summary>
[ExcludeFromCodeCoverage]
public class StartConsumptionEndpointSummary : Summary<StartConsumptionEndpoint>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="StartConsumptionEndpointSummary"/> class.
    /// </summary>
    public StartConsumptionEndpointSummary()
    {
        Summary = "Starts a media consumption session of the current user.";
        Description = "Proxies the start of a consumption record to the API server. The audio player calls it when a track actually begins playback. When the library tracks progress, the returned record carries the stored resume position. A missing media item and a media item that belongs to another library are reported with the same not found result.";

        Params["libraryId"] = "The Id of the media library that contains the media item.";
        Params["mediaKind"] = "The kind of media that is consumed. Only the member names of MediaKind are accepted, such as MusicTrack or Book. Numeric values are rejected.";
        Params["mediaItemId"] = "The Id of the consumed media item.";

        Response(200, "The consumption record is started and returned.",
            example: new SuccessResponse<ConsumptionRecordDto>(true, new ConsumptionRecordDto(
                Id: Guid.Parse("2c1b0a9f-8e7d-4c6b-5a4f-3e2d1c0b9a8f"),
                LibraryId: Guid.Parse("3b3a19f3-1f5a-4d5a-9a3a-5c5a4a3a2a1a"),
                MediaItemId: Guid.Parse("9f0e1d2c-3b4a-4c6d-8e7f-9a0b1c2d3e4f"),
                MediaKind: MediaKind.MusicTrack,
                Status: ConsumptionStatus.InProgress,
                StartedOnUtc: new DateTime(2026, 10, 24, 12, 0, 0, DateTimeKind.Utc),
                LastActivityOnUtc: new DateTime(2026, 10, 24, 12, 0, 0, DateTimeKind.Utc),
                EndedOnUtc: null,
                ConsumedDurationInSeconds: 0,
                LastPosition: null,
                LastProgressFraction: 0,
                ConsecutivePagesRead: 0))
        );
    }
}
