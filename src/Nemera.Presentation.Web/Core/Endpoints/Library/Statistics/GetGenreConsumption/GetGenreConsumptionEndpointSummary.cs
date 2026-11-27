#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Presentation.Web.Common.DTO.Common;
using Lumina.Presentation.Web.Common.DTO.MediaConsumption;
using Lumina.Presentation.Web.Common.Requests.Library.Statistics;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Presentation.Web.Core.Endpoints.Library.Statistics.GetGenreConsumption;

/// <summary>
/// Class used for providing a textual description for the <see cref="GetGenreConsumptionEndpoint"/> endpoint, for OpenAPI.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetGenreConsumptionEndpointSummary : Summary<GetGenreConsumptionEndpoint, GetGenreConsumptionRequest>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GetGenreConsumptionEndpointSummary"/> class.
    /// </summary>
    public GetGenreConsumptionEndpointSummary()
    {
        Summary = "Gets the genre consumption of the selected user.";
        Description = "Proxies the request that gets the genres carried by the media items the selected user consumed to the API server. When userId is omitted, the statistics of the current user are returned; an administrator can name another user. The names are lowercase and grouped across media kinds, so a genre shared by a book and a track appears in one bucket. The date range is inclusive. A failure is returned as a JSON object with success set to false and a localized errorMessage.";

        ExampleRequest = new GetGenreConsumptionRequest(
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

        ResponseParam<SuccessResponse<GenreConsumptionDto[]>>(response => response.IsSuccess, "Whether the operation succeeded.");
        ResponseParam<GenreConsumptionDto>(response => response.Name, "The lowercase name of the genre.");
        ResponseParam<GenreConsumptionDto>(response => response.ConsumedItemCount, "The number of distinct media items that carry the genre and were consumed.");
        ResponseParam<GenreConsumptionDto>(response => response.ConsumedDurationInSeconds, "The active duration consumed in the media items that carry the genre, in seconds.");

        Response(200, "The genre consumption of the selected user is returned, or a JSON error with success set to false.",
            example: new SuccessResponse<GenreConsumptionDto[]>(true,
            [
                new GenreConsumptionDto(
                    Name: "horror",
                    ConsumedItemCount: 3,
                    ConsumedDurationInSeconds: 5400),
                new GenreConsumptionDto(
                    Name: "fantasy",
                    ConsumedItemCount: 2,
                    ConsumedDurationInSeconds: 7320),
                new GenreConsumptionDto(
                    Name: "science fiction",
                    ConsumedItemCount: 1,
                    ConsumedDurationInSeconds: 1424.5)
            ])
        );
    }
}
