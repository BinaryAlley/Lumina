#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Presentation.Web.Common.DTO.Common;
using Lumina.Presentation.Web.Common.DTO.Favorites;
using Lumina.Presentation.Web.Common.Enums.Favorites;
using Lumina.Presentation.Web.Common.Requests.Favorites;
using System;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Presentation.Web.Core.Endpoints.Favorites.AddFavorite;

/// <summary>
/// Class used for providing a textual description for the <see cref="AddFavoriteEndpoint"/> endpoint, for OpenAPI.
/// </summary>
[ExcludeFromCodeCoverage]
public class AddFavoriteEndpointSummary : Summary<AddFavoriteEndpoint, AddFavoriteRequest>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AddFavoriteEndpointSummary"/> class.
    /// </summary>
    public AddFavoriteEndpointSummary()
    {
        Summary = "Marks a media item as a favorite of the current user.";
        Description = "Proxies the request that marks a media item as a favorite to the API server. Adding an item that is already a favorite is not an error: the existing favorite is returned.";

        ExampleRequest = new AddFavoriteRequest(
            MediaKind: FavoriteMediaKind.MusicTrack,
            MediaItemId: Guid.Parse("9f0e1d2c-3b4a-4c6d-8e7f-9a0b1c2d3e4f")
        );
        RequestParam(request => request.MediaKind, "The kind of the media item to favorite: MusicArtist, MusicAlbum, MusicTrack or Book. Required.");
        RequestParam(request => request.MediaItemId, "The Id of the media item to favorite. Required.");

        Response(200, "The media item is a favorite and the favorite is returned.",
            example: new SuccessResponse<FavoriteDto>(true, new FavoriteDto(
                Id: Guid.Parse("4b7a2c10-5d3e-4f68-9a1b-0c2d3e4f5a6b"),
                MediaKind: FavoriteMediaKind.MusicTrack,
                MediaItemId: Guid.Parse("9f0e1d2c-3b4a-4c6d-8e7f-9a0b1c2d3e4f"),
                FavoritedOnUtc: new DateTime(2026, 10, 21, 9, 30, 0, DateTimeKind.Utc)))
        );
    }
}
