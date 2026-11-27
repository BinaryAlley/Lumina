#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Presentation.Web.Common.DTO.Common;
using Lumina.Presentation.Web.Common.DTO.Favorites;
using Lumina.Presentation.Web.Common.Enums.Favorites;
using Lumina.Presentation.Web.Common.Requests.Favorites;
using System;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Presentation.Web.Core.Endpoints.Favorites.GetFavorites;

/// <summary>
/// Class used for providing a textual description for the <see cref="GetFavoritesEndpoint"/> endpoint, for OpenAPI.
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
        Description = "Proxies the request that gets a page of the favorites of the current user, from the most recently favorited, to the API server. The currentPage and perPage query parameters select the page, and count reports the total number of favorites that match the filter. Each favorite carries the display details of its media item.";

        ExampleRequest = new GetFavoritesRequest(
            MediaKind: FavoriteMediaKind.MusicTrack,
            CurrentPage: 1,
            PerPage: 200
        );
        RequestParam(request => request.MediaKind, "The kind of media item to restrict the favorites to: MusicArtist, MusicAlbum, MusicTrack or Book. Optional; every kind is returned when omitted.");
        RequestParam(request => request.CurrentPage, "The one based page of favorites to return. Optional; defaults to 1.");
        RequestParam(request => request.PerPage, "The number of favorites per page. Optional; defaults to 200.");

        Response(200, "The requested page of the favorites of the current user is returned.",
            example: new SuccessResponse<PaginatedDto<FavoriteItemDto>>(true, new PaginatedDto<FavoriteItemDto>(
                Data:
                [
                    new FavoriteItemDto(
                        MediaKind: FavoriteMediaKind.MusicTrack,
                        MediaItemId: Guid.Parse("9f0e1d2c-3b4a-4c6d-8e7f-9a0b1c2d3e4f"),
                        IsAvailable: true,
                        LibraryId: Guid.Parse("1a2b3c4d-5e6f-4a7b-8c9d-0e1f2a3b4c5d"),
                        Title: "Time",
                        CollectionId: Guid.Parse("7c6b5a49-3827-4165-9d0e-1f2a3b4c5d6e"),
                        CollectionTitle: "The Dark Side of the Moon",
                        CreatorId: Guid.Parse("2d3e4f5a-6b7c-4d8e-9f0a-1b2c3d4e5f6a"),
                        CreatorName: "Pink Floyd",
                        CoverPath: "covers/dark-side-of-the-moon.jpg")
                ],
                CurrentPage: 1,
                PerPage: 200,
                Count: 1,
                NumberOfPages: 1))
        );
    }
}
