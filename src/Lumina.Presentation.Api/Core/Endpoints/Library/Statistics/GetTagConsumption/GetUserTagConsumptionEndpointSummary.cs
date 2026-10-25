#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Contracts.Requests.MediaConsumption;
using Lumina.Contracts.Responses.MediaConsumption;
using Lumina.Domain.SharedKernel.Common.Enums.MediaConsumption;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Presentation.Api.Core.Endpoints.Library.Statistics.GetTagConsumption;

/// <summary>
/// Class used for providing a textual description for the <see cref="GetUserTagConsumptionEndpoint"/> API endpoint, for OpenAPI.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetUserTagConsumptionEndpointSummary : Summary<GetUserTagConsumptionEndpoint, GetTagConsumptionRequest>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GetUserTagConsumptionEndpointSummary"/> class.
    /// </summary>
    public GetUserTagConsumptionEndpointSummary()
    {
        Summary = "Gets the consumption statistics of the tags and genres of a user.";
        Description = "Gets the tags and genres carried by the media items a user consumed, grouped by lowercase name across every media kind, or restricted to one kind, one library, one term kind, and a date range. A user can read only their own statistics, while an Admin can select any user with the userId route parameter; a regular user targeting another user receives the same not authorized result as a route that names no accessible user. Only an item with at least one session that counts towards statistics contributes, and each item contributes its terms once regardless of how many sessions it had, so a multi-genre item adds one to each of its genres, not one per session. The names are always lowercase and culture invariant, so a tag or genre that is shared by a book and a track appears in one bucket. Tags and genres of an item whose media item was deleted, or whose media library the user can no longer access, are not included, because their terms cannot be resolved, while their sessions still count in the summary.";

        ExampleRequest = new GetTagConsumptionRequest(
            LibraryId: null,
            MediaKind: null,
            TagKind: null,
            FromUtc: new DateTime(2026, 10, 1, 0, 0, 0, DateTimeKind.Utc),
            ToUtc: new DateTime(2026, 10, 31, 23, 59, 59, DateTimeKind.Utc)
        );

        Params["userId"] = "The Id of the user whose statistics are retrieved. An Admin can name any user; a regular user can only name themselves.";
        RequestParam(request => request.LibraryId, "The Id of the media library to restrict the statistics to. Optional.");
        RequestParam(request => request.MediaKind, "The kind of media to restrict the statistics to. Optional.");
        RequestParam(request => request.TagKind, "Whether to return genres or tags. Optional.");
        RequestParam(request => request.FromUtc, "The inclusive start of the date range. Optional.");
        RequestParam(request => request.ToUtc, "The inclusive end of the date range. Optional.");

        ResponseParam<TagConsumptionResponse>(response => response.Name, "The lowercase name of the tag or genre.");
        ResponseParam<TagConsumptionResponse>(response => response.TagKind, "Whether the entry is a genre or a tag.");
        ResponseParam<TagConsumptionResponse>(response => response.ConsumedItemCount, "The number of distinct media items that carry the term and were consumed.");
        ResponseParam<TagConsumptionResponse>(response => response.ConsumedDurationInSeconds, "The active duration consumed in the media items that carry the term, in seconds.");

        Response<IReadOnlyList<TagConsumptionResponse>>(statusCode: 200, description: "The consumption statistics of the tags and genres of the user are returned.", contentType: "application/json",
            example:
            [
                new TagConsumptionResponse(
                    Name: "horror",
                    TagKind: TagKind.Genre,
                    ConsumedItemCount: 3,
                    ConsumedDurationInSeconds: 5400
                ),
                new TagConsumptionResponse(
                    Name: "fantasy",
                    TagKind: TagKind.Genre,
                    ConsumedItemCount: 2,
                    ConsumedDurationInSeconds: 7320
                ),
                new TagConsumptionResponse(
                    Name: "favourite",
                    TagKind: TagKind.Tag,
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
                    instance = "/api/v1/users/{userId}/consumption-statistics/tags"
                },
                new
                {
                    type = "https://tools.ietf.org/html/rfc7235#section-3.1",
                    status = 401,
                    title = "Unauthorized",
                    detail = "The token has expired",
                    instance = "/api/v1/users/{userId}/consumption-statistics/tags"
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
                instance = "/api/v1/users/{userId}/consumption-statistics/tags",
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
                instance = "/api/v1/users/{userId}/consumption-statistics/tags",
                errors = new Dictionary<string, string[]>
                {
                    {
                        "General.Validation", new[]
                        {
                            "TargetUserIdIsInvalid",
                            "LibraryIdCannotBeEmpty",
                            "UnsupportedMediaKind",
                            "TagKindIsInvalid",
                            "ConsumptionDateRangeIsInvalid"
                        }
                    }
                },
                traceId = "00-2470be4248a2a5a0c6f70579975a6954-b9c3ba9544a03500-00"
            }
        );
    }
}
