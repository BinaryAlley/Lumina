#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Contracts.Requests.Favorites;
using Lumina.Contracts.Responses.Common;
using Lumina.Contracts.Responses.Favorites;
using Lumina.Domain.SharedKernel.Common.Enums.Favorites;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Presentation.Api.Core.Endpoints.Favorites.GetFavorites;

/// <summary>
/// Class used for providing a textual description for the <see cref="GetFavoritesEndpoint"/> API endpoint, for OpenAPI.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetFavoritesEndpointSummary : Summary<GetFavoritesEndpoint, GetFavoritesRequest>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GetFavoritesEndpointSummary"/> class.
    /// </summary>
    public GetFavoritesEndpointSummary()
    {
        Summary = "Gets the favorites of the current user.";
        Description = "Gets the favorites of the currently authenticated user, from the most recently favorited, across every kind of media item or restricted to one kind. The currentPage and perPage query parameters select the page, and count reports the number of favorites that match the filter. When neither parameter is provided, every matching favorite is returned in a single page. Each favorite carries the display details of the media item it references: the title, the collection, the creator and the cover art. A favorite whose media item was deleted, or whose media library the user can no longer access, is still returned, with isAvailable set to false and empty display details, so the user can see it and remove it. A favorite of another user is never returned.";

        ExampleRequest = new GetFavoritesRequest(
            MediaKind: FavoriteMediaKind.MusicTrack,
            CurrentPage: 1,
            PerPage: 200
        );
        RequestParam(request => request.MediaKind, "The kind of media item to restrict the favorites to. Optional; every kind is returned when omitted.");
        RequestParam(request => request.CurrentPage, "The one based page of favorites to return. Optional; defaults to 1 when a page size is provided.");
        RequestParam(request => request.PerPage, "The number of favorites per page. Optional; defaults to 200 when a page is provided. When both pagination parameters are omitted, every matching favorite is returned.");

        ResponseParam<PaginatedResponse<FavoriteItemResponse>>(response => response.Data, "The favorites of the requested page, from the most recently favorited.");
        ResponseParam<PaginatedResponse<FavoriteItemResponse>>(response => response.CurrentPage, "The current page number.");
        ResponseParam<PaginatedResponse<FavoriteItemResponse>>(response => response.PerPage, "The number of favorites retrieved per page.");
        ResponseParam<PaginatedResponse<FavoriteItemResponse>>(response => response.Count, "The total number of favorites that match the filter.");
        ResponseParam<PaginatedResponse<FavoriteItemResponse>>(response => response.NumberOfPages, "The total number of pages.");

        ResponseParam<FavoriteItemResponse>(response => response.MediaKind, "The kind of the favorited media item.");
        ResponseParam<FavoriteItemResponse>(response => response.MediaItemId, "The Id of the favorited media item.");
        ResponseParam<FavoriteItemResponse>(response => response.IsAvailable, "Whether the media item still exists and its library is accessible to the current user.");
        ResponseParam<FavoriteItemResponse>(response => response.LibraryId, "The Id of the media library that contains the media item.");
        ResponseParam<FavoriteItemResponse>(response => response.Title, "The title of the media item.");
        ResponseParam<FavoriteItemResponse>(response => response.CollectionId, "The Id of the collection that contains the media item, or the empty Guid when there is none.");
        ResponseParam<FavoriteItemResponse>(response => response.CollectionTitle, "The title of the collection that contains the media item, or an empty string when there is none.");
        ResponseParam<FavoriteItemResponse>(response => response.CreatorId, "The Id of the creator of the media item, or the empty Guid when there is none.");
        ResponseParam<FavoriteItemResponse>(response => response.CreatorName, "The name of the creator of the media item, or an empty string when there is none.");
        ResponseParam<FavoriteItemResponse>(response => response.CoverPath, "The path of the cover art of the media item, if available.");

        Response(200, "The favorites of the current user are returned.",
            example: new PaginatedResponse<FavoriteItemResponse>
            {
                Data =
                [
                    new FavoriteItemResponse(
                        MediaKind: FavoriteMediaKind.MusicTrack,
                        MediaItemId: Guid.Parse("9f0e1d2c-3b4a-4c6d-8e7f-9a0b1c2d3e4f"),
                        IsAvailable: true,
                        LibraryId: Guid.Parse("3b3a19f3-1f5a-4d5a-9a3a-5c5a4a3a2a1a"),
                        Title: "The Show Must Go On",
                        CollectionId: Guid.Parse("e8d52f7b-0a3c-4c2d-ab4f-8a7b6c5d4e3f"),
                        CollectionTitle: "Innuendo",
                        CreatorId: Guid.Parse("d7c41e6a-9f2b-4b1c-9a3e-7f6e5d4c3b2a"),
                        CreatorName: "Queen",
                        CoverPath: "/artwork/albums/e8d52f7b-0a3c-4c2d-ab4f-8a7b6c5d4e3f/cover.jpg"
                    ),
                    new FavoriteItemResponse(
                        MediaKind: FavoriteMediaKind.Book,
                        MediaItemId: Guid.Parse("0a1b2c3d-4e5f-4a6b-8c7d-9e0f1a2b3c4d"),
                        IsAvailable: true,
                        LibraryId: Guid.Parse("6c7d8e9f-0a1b-4c2d-9e3f-4a5b6c7d8e9f"),
                        Title: "The Fellowship of the Ring",
                        CollectionId: Guid.Empty,
                        CollectionTitle: string.Empty,
                        CreatorId: Guid.Parse("8e9f0a1b-2c3d-4e4f-9a5b-6c7d8e9f0a1b"),
                        CreatorName: "J. R. R. Tolkien",
                        CoverPath: "/artwork/books/0a1b2c3d-4e5f-4a6b-8c7d-9e0f1a2b3c4d/cover.jpg"
                    ),
                    new FavoriteItemResponse(
                        MediaKind: FavoriteMediaKind.MusicAlbum,
                        MediaItemId: Guid.Parse("9f0a1b2c-3d4e-4f5a-8b6c-7d8e9f0a1b2c"),
                        IsAvailable: false,
                        LibraryId: Guid.Empty,
                        Title: string.Empty,
                        CollectionId: Guid.Empty,
                        CollectionTitle: string.Empty,
                        CreatorId: Guid.Empty,
                        CreatorName: string.Empty,
                        CoverPath: null
                    )
                ],
                CurrentPage = 1,
                PerPage = 200,
                Count = 3,
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
                    instance = "/api/v1/users/me/favorites"
                },
                new
                {
                    type = "https://tools.ietf.org/html/rfc7235#section-3.1",
                    status = 401,
                    title = "Unauthorized",
                    detail = "The token has expired",
                    instance = "/api/v1/users/me/favorites"
                },
                new
                {
                    type = "https://tools.ietf.org/html/rfc7235#section-3.1",
                    status = 401,
                    title = "Unauthorized",
                    detail = "The token is invalid",
                    instance = "/api/v1/users/me/favorites"
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
                instance = "/api/v1/users/me/favorites",
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
                instance = "/api/v1/users/me/favorites",
                errors = new Dictionary<string, string[]>
                {
                    {
                        "General.Validation", new[]
                        {
                            "FavoriteMediaKindIsInvalid"
                        }
                    }
                },
                traceId = "00-2470be4248a2a5a0c6f70579975a6954-b9c3ba9544a03500-00"
            }
        );
    }
}
