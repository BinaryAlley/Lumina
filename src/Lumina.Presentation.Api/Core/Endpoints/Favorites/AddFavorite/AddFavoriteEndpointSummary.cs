#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Contracts.Requests.Favorites;
using Lumina.Contracts.Responses.Favorites;
using Lumina.Domain.SharedKernel.Common.Enums.Favorites;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Presentation.Api.Core.Endpoints.Favorites.AddFavorite;

/// <summary>
/// Class used for providing a textual description for the <see cref="AddFavoriteEndpoint"/> API endpoint, for OpenAPI.
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
        Description = "Marks the media item identified by its kind and its Id as a favorite of the currently authenticated user. The supported kinds are MusicArtist, MusicAlbum, MusicTrack and Book. The kinds Video and Photo exist but are planned: they are rejected with a validation error until video and photo libraries are catalogued. The media item must exist and belong to a media library the current user can access; an item that does not exist and an item of a library the user cannot access are reported with the same not found error, so the response cannot be used to find out what another user holds. The operation is idempotent: adding an item that is already a favorite returns the existing favorite and changes nothing.";

        ExampleRequest = new AddFavoriteRequest(
            MediaKind: FavoriteMediaKind.MusicTrack,
            MediaItemId: Guid.Parse("9f0e1d2c-3b4a-4c6d-8e7f-9a0b1c2d3e4f")
        );
        RequestParam(request => request.MediaKind, "The kind of the media item to favorite. Required. One of MusicArtist, MusicAlbum, MusicTrack or Book.");
        RequestParam(request => request.MediaItemId, "The Id of the media item to favorite. Required.");

        ResponseParam<FavoriteResponse>(response => response.Id, "The Id of the favorite.");
        ResponseParam<FavoriteResponse>(response => response.MediaKind, "The kind of the favorited media item.");
        ResponseParam<FavoriteResponse>(response => response.MediaItemId, "The Id of the favorited media item.");
        ResponseParam<FavoriteResponse>(response => response.FavoritedOnUtc, "The date and time when the media item was favorited.");

        Response(200, "The media item is a favorite of the current user, and the favorite is returned.",
            example: new FavoriteResponse(
                Id: Guid.Parse("4d5e6f70-8192-4a3b-9c4d-5e6f708192a3"),
                MediaKind: FavoriteMediaKind.MusicTrack,
                MediaItemId: Guid.Parse("9f0e1d2c-3b4a-4c6d-8e7f-9a0b1c2d3e4f"),
                FavoritedOnUtc: new DateTime(2026, 10, 21, 12, 0, 0, DateTimeKind.Utc)
            )
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

        Response(404, "The request failed because the media item does not exist, or it belongs to a media library the current user cannot access. The error names the kind of the media item.", "application/problem+json",
            example: new[]
            {
                new
                {
                    type = "https://tools.ietf.org/html/rfc9110#section-15.5.5",
                    title = "General.NotFound",
                    status = 404,
                    detail = "ArtistNotFound",
                    instance = "/api/v1/users/me/favorites",
                    traceId = "00-a712bbf99ca8ab485f86a762ae5ae74d-b3a2eb78813b0a5d-00"
                },
                new
                {
                    type = "https://tools.ietf.org/html/rfc9110#section-15.5.5",
                    title = "General.NotFound",
                    status = 404,
                    detail = "AlbumNotFound",
                    instance = "/api/v1/users/me/favorites",
                    traceId = "00-a712bbf99ca8ab485f86a762ae5ae74d-b3a2eb78813b0a5d-00"
                },
                new
                {
                    type = "https://tools.ietf.org/html/rfc9110#section-15.5.5",
                    title = "General.NotFound",
                    status = 404,
                    detail = "TrackNotFound",
                    instance = "/api/v1/users/me/favorites",
                    traceId = "00-a712bbf99ca8ab485f86a762ae5ae74d-b3a2eb78813b0a5d-00"
                },
                new
                {
                    type = "https://tools.ietf.org/html/rfc9110#section-15.5.5",
                    title = "General.NotFound",
                    status = 404,
                    detail = "BookNotFound",
                    instance = "/api/v1/users/me/favorites",
                    traceId = "00-a712bbf99ca8ab485f86a762ae5ae74d-b3a2eb78813b0a5d-00"
                }
            }
        );

        Response(409, "The request failed because the favorite could not be stored, because the database is busy or locked. Repeating the request is safe.", "application/problem+json",
            example: new[]
            {
                new
                {
                    type = "https://tools.ietf.org/html/rfc9110#section-15.5.10",
                    title = "General.Conflict",
                    status = 409,
                    detail = "ConcurrencyConflict",
                    instance = "/api/v1/users/me/favorites",
                    traceId = "00-a712bbf99ca8ab485f86a762ae5ae74d-b3a2eb78813b0a5d-00"
                }
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
                            "FavoriteMediaKindIsInvalid",
                            "FavoriteMediaItemIdCannotBeEmpty",
                            "FavoriteMediaKindIsNotSupported"
                        }
                    }
                },
                traceId = "00-2470be4248a2a5a0c6f70579975a6954-b9c3ba9544a03500-00"
            }
        );
    }
}
