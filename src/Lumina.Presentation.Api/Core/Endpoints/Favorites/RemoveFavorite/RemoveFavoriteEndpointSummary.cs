#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Presentation.Api.Core.Endpoints.Favorites.RemoveFavorite;

/// <summary>
/// Class used for providing a textual description for the <see cref="RemoveFavoriteEndpoint"/> API endpoint, for OpenAPI.
/// </summary>
[ExcludeFromCodeCoverage]
public class RemoveFavoriteEndpointSummary : Summary<RemoveFavoriteEndpoint, EmptyRequest>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="RemoveFavoriteEndpointSummary"/> class.
    /// </summary>
    public RemoveFavoriteEndpointSummary()
    {
        Summary = "Removes a media item from the favorites of the current user.";
        Description = "Removes the media item identified by its kind and its Id from the favorites of the currently authenticated user. The kind is the name of a kind of media item, such as MusicTrack, and is matched without regard to case. The media item itself is never deleted. The favorite is always the one of the authenticated user, so this route can never remove another user's favorite. The media library of the item is deliberately not checked, so a user can always remove their own favorite whose item was deleted or is no longer accessible. Removing is idempotent: an item that is not a favorite of the user already has the desired state, so the request succeeds without changing anything and without raising an event.";

        Params["mediaKind"] = "The name of the kind of media item, matched case insensitively: MusicArtist, MusicAlbum, MusicTrack or Book.";
        Params["mediaItemId"] = "The Id of the media item to remove from the favorites.";

        Response(200, "The media item was successfully removed from the favorites.");

        Response(401, "Authentication required.", "application/problem+json",
            example: new[]
            {
                new
                {
                    type = "https://tools.ietf.org/html/rfc7235#section-3.1",
                    status = 401,
                    title = "Unauthorized",
                    detail = "Authentication failed",
                    instance = "/api/v1/users/me/favorites/MusicTrack/9f0e1d2c-3b4a-4c6d-8e7f-9a0b1c2d3e4f"
                },
                new
                {
                    type = "https://tools.ietf.org/html/rfc7235#section-3.1",
                    status = 401,
                    title = "Unauthorized",
                    detail = "The token has expired",
                    instance = "/api/v1/users/me/favorites/MusicTrack/9f0e1d2c-3b4a-4c6d-8e7f-9a0b1c2d3e4f"
                },
                new
                {
                    type = "https://tools.ietf.org/html/rfc7235#section-3.1",
                    status = 401,
                    title = "Unauthorized",
                    detail = "The token is invalid",
                    instance = "/api/v1/users/me/favorites/MusicTrack/9f0e1d2c-3b4a-4c6d-8e7f-9a0b1c2d3e4f"
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
                instance = "/api/v1/users/me/favorites/MusicTrack/9f0e1d2c-3b4a-4c6d-8e7f-9a0b1c2d3e4f",
                traceId = "00-a712bbf99ca8ab485f86a762ae5ae74d-b3a2eb78813b0a5d-00"
            }
        );

        Response(409, "The request failed because the database is busy or locked, or the favorite was changed by another request in a way that could not be reconciled. Removing a favorite that another request removed first succeeds.", "application/problem+json",
            example: new
            {
                type = "https://tools.ietf.org/html/rfc9110#section-15.5.10",
                title = "General.Conflict",
                status = 409,
                detail = "ConcurrencyConflict",
                instance = "/api/v1/users/me/favorites/MusicTrack/9f0e1d2c-3b4a-4c6d-8e7f-9a0b1c2d3e4f",
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
                instance = "/api/v1/users/me/favorites/Unknown/not-a-guid",
                errors = new Dictionary<string, string[]>
                {
                    {
                        "General.Validation", new[]
                        {
                            "FavoriteMediaKindIsInvalid",
                            "FavoriteMediaItemIdCannotBeEmpty"
                        }
                    }
                },
                traceId = "00-2470be4248a2a5a0c6f70579975a6954-b9c3ba9544a03500-00"
            }
        );
    }
}
