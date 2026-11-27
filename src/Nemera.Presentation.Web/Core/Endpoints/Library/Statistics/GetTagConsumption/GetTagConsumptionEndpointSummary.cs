#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Presentation.Web.Common.DTO.Common;
using Lumina.Presentation.Web.Common.DTO.MediaConsumption;
using Lumina.Presentation.Web.Common.Requests.Library.Statistics;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Presentation.Web.Core.Endpoints.Library.Statistics.GetTagConsumption;

/// <summary>
/// Class used for providing a textual description for the <see cref="GetTagConsumptionEndpoint"/> endpoint, for OpenAPI.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetTagConsumptionEndpointSummary : Summary<GetTagConsumptionEndpoint, GetTagConsumptionRequest>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GetTagConsumptionEndpointSummary"/> class.
    /// </summary>
    public GetTagConsumptionEndpointSummary()
    {
        Summary = "Gets the tag consumption of the selected user.";
        Description = "Proxies the request that gets the tags carried by the media items the selected user consumed to the API server. When userId is omitted, the statistics of the current user are returned; an administrator can name another user. The names are lowercase and grouped across media kinds, so a tag shared by a book and a track appears in one bucket. The date range is inclusive. A failure is returned as a JSON object with success set to false and a localized errorMessage.";

        ExampleRequest = new GetTagConsumptionRequest(
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

        ResponseParam<SuccessResponse<TagConsumptionDto[]>>(response => response.IsSuccess, "Whether the operation succeeded.");
        ResponseParam<TagConsumptionDto>(response => response.Name, "The lowercase name of the tag.");
        ResponseParam<TagConsumptionDto>(response => response.ConsumedItemCount, "The number of distinct media items that carry the tag and were consumed.");
        ResponseParam<TagConsumptionDto>(response => response.ConsumedDurationInSeconds, "The active duration consumed in the media items that carry the tag, in seconds.");

        Response(200, "The tag consumption of the selected user is returned, or a JSON error with success set to false.",
            example: new SuccessResponse<TagConsumptionDto[]>(true,
            [
                new TagConsumptionDto(
                    Name: "favourite",
                    ConsumedItemCount: 3,
                    ConsumedDurationInSeconds: 5400),
                new TagConsumptionDto(
                    Name: "book-club",
                    ConsumedItemCount: 2,
                    ConsumedDurationInSeconds: 7320),
                new TagConsumptionDto(
                    Name: "comfort-read",
                    ConsumedItemCount: 1,
                    ConsumedDurationInSeconds: 1424.5)
            ])
        );
    }
}
