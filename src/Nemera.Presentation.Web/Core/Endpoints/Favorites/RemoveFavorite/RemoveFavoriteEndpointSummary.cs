#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Presentation.Web.Common.DTO.Common;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Presentation.Web.Core.Endpoints.Favorites.RemoveFavorite;

/// <summary>
/// Class used for providing a textual description for the <see cref="RemoveFavoriteEndpoint"/> endpoint, for OpenAPI.
/// </summary>
[ExcludeFromCodeCoverage]
public class RemoveFavoriteEndpointSummary : Summary<RemoveFavoriteEndpoint>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="RemoveFavoriteEndpointSummary"/> class.
    /// </summary>
    public RemoveFavoriteEndpointSummary()
    {
        Summary = "Removes a media item from the favorites of the current user.";
        Description = "Proxies the request that removes a favorite to the API server. The media item itself is not deleted. The favorite belongs to the current user, so this route cannot remove another user's favorite. Removing is idempotent: a media item that is not a favorite of the current user already has the desired state, so the request succeeds.";

        Params["mediaKind"] = "The kind of the favorited media item: MusicArtist, MusicAlbum, MusicTrack or Book.";
        Params["mediaItemId"] = "The Id of the favorited media item.";

        Response(200, "The media item was removed from the favorites, or the item was already not a favorite.",
            example: new SuccessResponse(true)
        );
    }
}
