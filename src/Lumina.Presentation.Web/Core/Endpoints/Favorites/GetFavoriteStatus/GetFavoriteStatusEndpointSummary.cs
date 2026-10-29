#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Presentation.Web.Common.DTO.Common;
using Lumina.Presentation.Web.Common.DTO.Favorites;
using Lumina.Presentation.Web.Common.Enums.Favorites;
using Lumina.Presentation.Web.Common.Requests.Favorites;
using System;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Presentation.Web.Core.Endpoints.Favorites.GetFavoriteStatus;

/// <summary>
/// Class used for providing a textual description for the <see cref="GetFavoriteStatusEndpoint"/> endpoint, for OpenAPI.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetFavoriteStatusEndpointSummary : Summary<GetFavoriteStatusEndpoint, GetFavoriteStatusRequest>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GetFavoriteStatusEndpointSummary"/> class.
    /// </summary>
    public GetFavoriteStatusEndpointSummary()
    {
        Summary = "Gets which media items are favorites of the current user.";
        Description = "Proxies the request that finds out which of the given media items of one kind are favorites of the current user to the API server. The mediaItemIds query parameter is repeated, once per Id.";

        ExampleRequest = new GetFavoriteStatusRequest(
            MediaKind: FavoriteMediaKind.MusicTrack,
            MediaItemIds:
            [
                Guid.Parse("9f0e1d2c-3b4a-4c6d-8e7f-9a0b1c2d3e4f"),
                Guid.Parse("5a4b3c2d-1e0f-4a9b-8c7d-6e5f4a3b2c1d")
            ]
        );
        RequestParam(request => request.MediaKind, "The kind of the media items to check: MusicArtist, MusicAlbum, MusicTrack or Book. Required.");
        RequestParam(request => request.MediaItemIds, "The Ids of the media items to check, from 1 to 100, sent as a repeated mediaItemIds query parameter. Required.");

        Response(200, "The Ids, among the checked ones, of the media items that are favorites of the current user are returned.",
            example: new SuccessResponse<FavoriteStatusDto>(true, new FavoriteStatusDto(
                FavoritedMediaItemIds: [Guid.Parse("9f0e1d2c-3b4a-4c6d-8e7f-9a0b1c2d3e4f")]))
        );
    }
}
