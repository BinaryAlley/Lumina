#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Contracts.Requests.MediaConsumption;
using Lumina.Contracts.Responses.Common;
using Lumina.Contracts.Responses.MediaConsumption;
using Lumina.Domain.SharedKernel.Common.Enums.MediaLibrary;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Presentation.Api.Core.Endpoints.Library.Statistics.GetMostConsumed;

/// <summary>
/// Class used for providing a textual description for the <see cref="GetMostConsumedEndpoint"/> API endpoint, for OpenAPI.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetMostConsumedEndpointSummary : Summary<GetMostConsumedEndpoint, GetMostConsumedRequest>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GetMostConsumedEndpointSummary"/> class.
    /// </summary>
    public GetMostConsumedEndpointSummary()
    {
        Summary = "Gets the most consumed media items of the current user.";
        Description = "Gets one page of the media items the current user consumed the most, across every kind of media or restricted to one kind, one library, and a date range. The items are ordered by the number of sessions that count towards statistics, then by the consumed duration, then by the last consumption date, and finally by the media item Id so the order is stable. Music and book sessions appear in the same list, and an item consumed many times appears once. An item whose sessions all fall below the counting threshold of its kind still appears in this list with a countedSessionCount of zero, but it is ordered after every item that has at least one counted session, so it normally appears on a later page. Each item carries the display details of the media item it references: the title, the collection, the creator and the cover art. An item whose media item was deleted, or whose media library the user can no longer access, is still returned, with isAvailable set to false and empty display details, so the user can see it and delete it from the history. The history of another user is never returned by this route.";

        ExampleRequest = new GetMostConsumedRequest(
            LibraryId: null,
            MediaKind: nameof(MediaKind.MusicTrack),
            CurrentPage: 1,
            PerPage: 50,
            FromUtc: "2026-10-01T00:00:00Z",
            ToUtc: "2026-10-31T23:59:59Z"
        );

        RequestParam(request => request.LibraryId, "The Id of the media library to restrict the history to. Optional.");
        RequestParam(request => request.MediaKind, "The name of the kind of media to restrict the history to. Optional. Only the member names of the media kind enumeration are accepted.");
        RequestParam(request => request.CurrentPage, "The one based page of media items to return. Optional.");
        RequestParam(request => request.PerPage, "The number of media items per page. Optional.");
        RequestParam(request => request.FromUtc, "The inclusive start of the date range. Optional.");
        RequestParam(request => request.ToUtc, "The inclusive end of the date range. Optional.");

        ResponseParam<PaginatedResponse<ConsumedItemResponse>>(response => response.Data, "The most consumed media items of the requested page, ordered by counted sessions, consumed duration, and last consumption date.");
        ResponseParam<PaginatedResponse<ConsumedItemResponse>>(response => response.CurrentPage, "The current page number.");
        ResponseParam<PaginatedResponse<ConsumedItemResponse>>(response => response.PerPage, "The number of media items retrieved per page.");
        ResponseParam<PaginatedResponse<ConsumedItemResponse>>(response => response.Count, "The total number of distinct media items that match the filters.");
        ResponseParam<PaginatedResponse<ConsumedItemResponse>>(response => response.NumberOfPages, "The total number of pages.");
        ResponseParam<ConsumedItemResponse>(response => response.MediaKind, "The kind of the consumed media item.");
        ResponseParam<ConsumedItemResponse>(response => response.MediaItemId, "The Id of the consumed media item.");
        ResponseParam<ConsumedItemResponse>(response => response.LibraryId, "The Id of the media library that contains the media item.");
        ResponseParam<ConsumedItemResponse>(response => response.IsAvailable, "Whether the media item still exists and its library is accessible to the current user.");
        ResponseParam<ConsumedItemResponse>(response => response.Title, "The title of the media item, or an empty string when the item is not available.");
        ResponseParam<ConsumedItemResponse>(response => response.CollectionTitle, "The title of the collection that contains the media item, or an empty string when there is none.");
        ResponseParam<ConsumedItemResponse>(response => response.CreatorName, "The name of the creator of the media item, or an empty string when there is none.");
        ResponseParam<ConsumedItemResponse>(response => response.CoverPath, "The path of the cover art of the media item, if available.");
        ResponseParam<ConsumedItemResponse>(response => response.LastConsumedOnUtc, "The date and time when the media item was last consumed.");
        ResponseParam<ConsumedItemResponse>(response => response.CountedSessionCount, "The number of consumption sessions of the media item that count towards statistics.");
        ResponseParam<ConsumedItemResponse>(response => response.CompletedSessionCount, "The number of consumption sessions of the media item that completed.");
        ResponseParam<ConsumedItemResponse>(response => response.ConsumedDurationInSeconds, "The active duration consumed in the media item, over all of its sessions, in seconds.");

        Response(200, "The most consumed media items of the current user are returned.", "application/json",
            example: new PaginatedResponse<ConsumedItemResponse>
            {
                Data =
                [
                    new ConsumedItemResponse(
                        MediaKind: MediaKind.MusicTrack,
                        MediaItemId: Guid.Parse("9f0e1d2c-3b4a-4c6d-8e7f-9a0b1c2d3e4f"),
                        LibraryId: Guid.Parse("3b3a19f3-1f5a-4d5a-9a3a-5c5a4a3a2a1a"),
                        IsAvailable: true,
                        Title: "The Show Must Go On",
                        CollectionTitle: "Innuendo",
                        CreatorName: "Queen",
                        CoverPath: "/artwork/albums/e8d52f7b-0a3c-4c2d-ab4f-8a7b6c5d4e3f/cover.jpg",
                        LastConsumedOnUtc: new DateTime(2026, 10, 24, 12, 12, 0, DateTimeKind.Utc),
                        CountedSessionCount: 4,
                        CompletedSessionCount: 3,
                        ConsumedDurationInSeconds: 1424.5
                    ),
                    new ConsumedItemResponse(
                        MediaKind: MediaKind.Book,
                        MediaItemId: Guid.Parse("7a6b5c4d-3e2f-4a1b-8c9d-0e1f2a3b4c5d"),
                        LibraryId: Guid.Parse("6c7d8e9f-0a1b-4c2d-9e3f-4a5b6c7d8e9f"),
                        IsAvailable: true,
                        Title: "The Fellowship of the Ring",
                        CollectionTitle: string.Empty,
                        CreatorName: "J. R. R. Tolkien",
                        CoverPath: "/artwork/books/7a6b5c4d-3e2f-4a1b-8c9d-0e1f2a3b4c5d/cover.jpg",
                        LastConsumedOnUtc: new DateTime(2026, 10, 23, 20, 40, 0, DateTimeKind.Utc),
                        CountedSessionCount: 2,
                        CompletedSessionCount: 1,
                        ConsumedDurationInSeconds: 7320
                    )
                ],
                CurrentPage = 1,
                PerPage = 50,
                Count = 2,
                NumberOfPages = 1
            }
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
                    instance = "/api/v1/users/me/consumption-history/most-consumed"
                },
                new
                {
                    type = "https://tools.ietf.org/html/rfc7235#section-3.1",
                    status = 401,
                    title = "Unauthorized",
                    detail = "The token has expired",
                    instance = "/api/v1/users/me/consumption-history/most-consumed"
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
                instance = "/api/v1/users/me/consumption-history/most-consumed",
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
                instance = "/api/v1/users/me/consumption-history/most-consumed",
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
