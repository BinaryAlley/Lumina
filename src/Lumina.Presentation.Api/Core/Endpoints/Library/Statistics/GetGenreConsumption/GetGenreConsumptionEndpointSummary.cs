#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Contracts.Requests.MediaConsumption;
using Lumina.Contracts.Responses.MediaConsumption;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Presentation.Api.Core.Endpoints.Library.Statistics.GetGenreConsumption;

/// <summary>
/// Class used for providing a textual description for the <see cref="GetGenreConsumptionEndpoint"/> API endpoint, for OpenAPI.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetGenreConsumptionEndpointSummary : Summary<GetGenreConsumptionEndpoint, GetGenreConsumptionRequest>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GetGenreConsumptionEndpointSummary"/> class.
    /// </summary>
    public GetGenreConsumptionEndpointSummary()
    {
        Summary = "Gets the consumption statistics of the genres of the current user.";
        Description = "Gets the genres carried by the media items the current user consumed, grouped by lowercase name across every media kind, or restricted to one kind, one library, and a date range. Only an item with at least one session that counts towards statistics contributes, and each item contributes its genres once regardless of how many sessions it had, so a multi-genre item adds one to each of its genres, not one per session. The names are always lowercase and culture invariant, so a genre that is shared by a book and a track appears in one bucket. Genres of an item whose media item was deleted, or whose media library the user can no longer access, are not included, because their genres cannot be resolved, while their sessions still count in the summary. The statistics of another user are never returned by this route.";

        ExampleRequest = new GetGenreConsumptionRequest(
            LibraryId: null,
            MediaKind: null,
            FromUtc: "2026-10-01T00:00:00.0000000Z",
            ToUtc: "2026-10-31T23:59:59.0000000Z"
        );

        RequestParam(request => request.LibraryId, "The Id of the media library to restrict the statistics to. Optional.");
        RequestParam(request => request.MediaKind, "The kind of media to restrict the statistics to. Optional.");
        RequestParam(request => request.FromUtc, "The inclusive start of the date range. Optional.");
        RequestParam(request => request.ToUtc, "The inclusive end of the date range. Optional.");

        ResponseParam<GenreConsumptionResponse>(response => response.Name, "The lowercase name of the genre.");
        ResponseParam<GenreConsumptionResponse>(response => response.ConsumedItemCount, "The number of distinct media items that carry the genre and were consumed.");
        ResponseParam<GenreConsumptionResponse>(response => response.ConsumedDurationInSeconds, "The active duration consumed in the media items that carry the genre, in seconds.");

        Response<IReadOnlyList<GenreConsumptionResponse>>(statusCode: 200, description: "The consumption statistics of the genres of the current user are returned.", contentType: "application/json",
            example:
            [
                new GenreConsumptionResponse(
                    Name: "horror",
                    ConsumedItemCount: 3,
                    ConsumedDurationInSeconds: 5400
                ),
                new GenreConsumptionResponse(
                    Name: "fantasy",
                    ConsumedItemCount: 2,
                    ConsumedDurationInSeconds: 7320
                ),
                new GenreConsumptionResponse(
                    Name: "science fiction",
                    ConsumedItemCount: 1,
                    ConsumedDurationInSeconds: 1424.5
                )
            ]
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
                    instance = "/api/v1/users/me/consumption-statistics/genres"
                },
                new
                {
                    type = "https://tools.ietf.org/html/rfc7235#section-3.1",
                    status = 401,
                    title = "Unauthorized",
                    detail = "The token has expired",
                    instance = "/api/v1/users/me/consumption-statistics/genres"
                }
            }
        );

        Response(403, "The request failed because the authenticated request does not carry the identity of a user.", "application/problem+json",
            example: new
            {
                type = "https://tools.ietf.org/html/rfc9110#section-15.5.4",
                title = "General.Unauthorized",
                status = 403,
                detail = "NotAuthorized",
                instance = "/api/v1/users/me/consumption-statistics/genres",
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
                instance = "/api/v1/users/me/consumption-statistics/genres",
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
