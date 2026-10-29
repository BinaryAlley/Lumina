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

namespace Lumina.Presentation.Web.Core.Endpoints.Library.Statistics.GetRecentlyConsumed;

/// <summary>
/// Class used for providing a textual description for the <see cref="GetRecentlyConsumedEndpoint"/> endpoint, for OpenAPI.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetRecentlyConsumedEndpointSummary : Summary<GetRecentlyConsumedEndpoint, GetRecentlyConsumedRequest>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GetRecentlyConsumedEndpointSummary"/> class.
    /// </summary>
    public GetRecentlyConsumedEndpointSummary()
    {
        Summary = "Gets the recently consumed media items of the selected user.";
        Description = "Proxies the request that gets a page of the recently consumed media items of the selected user to the API server. When userId is omitted, the history of the current user is returned; an administrator can name another user. Music and book sessions appear in the same list. The date range is inclusive. An item whose media item was deleted, or whose library is no longer accessible, is returned with isAvailable set to false and empty display fields. A failure is returned as a JSON object with success set to false and a localized errorMessage.";

        ExampleRequest = new GetRecentlyConsumedRequest(
            UserId: null,
            LibraryId: null,
            MediaKind: null,
            FromUtc: new DateTime(2026, 10, 1, 0, 0, 0, DateTimeKind.Utc),
            ToUtc: new DateTime(2026, 10, 31, 23, 59, 59, DateTimeKind.Utc),
            CurrentPage: 1,
            PerPage: 10
        );

        RequestParam(request => request.UserId, "The Id of the user whose history is retrieved. Optional; when omitted, the current user is used.");
        RequestParam(request => request.LibraryId, "The Id of the media library to restrict the history to. Optional.");
        RequestParam(request => request.MediaKind, "The kind of media to restrict the history to. Optional.");
        RequestParam(request => request.FromUtc, "The inclusive start of the date range. Optional.");
        RequestParam(request => request.ToUtc, "The inclusive end of the date range. Optional.");
        RequestParam(request => request.CurrentPage, "The one based page of media items to return. Optional. Defaults to 1.");
        RequestParam(request => request.PerPage, "The number of media items per page. Optional. Defaults to 10.");

        ResponseParam<SuccessResponse<PaginatedDto<ConsumedItemDto>>>(response => response.IsSuccess, "Whether the operation succeeded.");
        ResponseParam<PaginatedDto<ConsumedItemDto>>(response => response.Data, "The recently consumed media items of the requested page, from the most recently consumed.");
        ResponseParam<PaginatedDto<ConsumedItemDto>>(response => response.CurrentPage, "The current page number.");
        ResponseParam<PaginatedDto<ConsumedItemDto>>(response => response.PerPage, "The number of media items retrieved per page.");
        ResponseParam<PaginatedDto<ConsumedItemDto>>(response => response.Count, "The total number of distinct media items that match the filters.");
        ResponseParam<PaginatedDto<ConsumedItemDto>>(response => response.NumberOfPages, "The total number of pages.");
        ResponseParam<ConsumedItemDto>(response => response.MediaKind, "The kind of the consumed media item.");
        ResponseParam<ConsumedItemDto>(response => response.MediaItemId, "The Id of the consumed media item.");
        ResponseParam<ConsumedItemDto>(response => response.LibraryId, "The Id of the media library that contains the media item.");
        ResponseParam<ConsumedItemDto>(response => response.IsAvailable, "Whether the media item still exists and its library is accessible.");
        ResponseParam<ConsumedItemDto>(response => response.Title, "The title of the media item, or an empty string when the item is not available.");
        ResponseParam<ConsumedItemDto>(response => response.CollectionTitle, "The title of the collection that contains the media item, or an empty string when there is none.");
        ResponseParam<ConsumedItemDto>(response => response.CreatorName, "The name of the creator of the media item, or an empty string when there is none.");
        ResponseParam<ConsumedItemDto>(response => response.CoverPath, "The path of the cover art of the media item, if available.");
        ResponseParam<ConsumedItemDto>(response => response.LastConsumedOnUtc, "The date and time when the media item was last consumed.");
        ResponseParam<ConsumedItemDto>(response => response.CountedSessionCount, "The number of consumption sessions of the media item that count towards statistics.");
        ResponseParam<ConsumedItemDto>(response => response.CompletedSessionCount, "The number of consumption sessions of the media item that completed.");
        ResponseParam<ConsumedItemDto>(response => response.ConsumedDurationInSeconds, "The active duration consumed in the media item, over all of its sessions, in seconds.");

        Response(200, "The requested page of recently consumed media items is returned, or a JSON error with success set to false.",
            example: new SuccessResponse<PaginatedDto<ConsumedItemDto>>(true, new PaginatedDto<ConsumedItemDto>(
                Data:
                [
                    new ConsumedItemDto(
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
                        ConsumedDurationInSeconds: 1424.5),
                    new ConsumedItemDto(
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
                        ConsumedDurationInSeconds: 7320)
                ],
                CurrentPage: 1,
                PerPage: 10,
                Count: 2,
                NumberOfPages: 1))
        );
    }
}
