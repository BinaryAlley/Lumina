#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Contracts.Requests.Favorites;
using Lumina.Contracts.Responses.Favorites;
using Lumina.Domain.SharedKernel.Common.Enums.Favorites;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Presentation.Api.Core.Endpoints.Favorites.GetFavoriteStatus;

/// <summary>
/// Class used for providing a textual description for the <see cref="GetFavoriteStatusEndpoint"/> API endpoint, for OpenAPI.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetFavoriteStatusEndpointSummary : Summary<GetFavoriteStatusEndpoint, GetFavoriteStatusRequest>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GetFavoriteStatusEndpointSummary"/> class.
    /// </summary>
    public GetFavoriteStatusEndpointSummary()
    {
        Summary = "Finds out which media items are favorites of the current user.";
        Description = "Checks up to 100 media items of one kind and returns the Ids, among the checked ones, that are favorites of the currently authenticated user. The media item Ids are sent by repeating the mediaItemIds query parameter once per Id, and the limit of 100 keeps the request line within the size limit of the server. Only the favorites of the current user are consulted, so the answer reveals nothing about the media items of other users. An Id that does not exist, or that is not a favorite, is simply absent from the answer.";

        ExampleRequest = new GetFavoriteStatusRequest(
            MediaKind: FavoriteMediaKind.MusicTrack,
            MediaItemIds:
            [
                Guid.Parse("9f0e1d2c-3b4a-4c6d-8e7f-9a0b1c2d3e4f"),
                Guid.Parse("a1b2c3d4-e5f6-4a7b-8c9d-0e1f2a3b4c5d")
            ]
        );
        RequestParam(request => request.MediaKind, "The kind of the media items to check. Required. One of MusicArtist, MusicAlbum, MusicTrack or Book.");
        RequestParam(request => request.MediaItemIds, "The Ids of the media items to check, from 1 to 100, sent as a repeated query parameter. Required.");

        ResponseParam<FavoriteStatusResponse>(response => response.FavoritedMediaItemIds, "The Ids, among the checked ones, of the media items that are favorites of the current user.");

        Response(200, "The favorite status of the checked media items is returned.",
            example: new FavoriteStatusResponse(
                FavoritedMediaItemIds: [Guid.Parse("9f0e1d2c-3b4a-4c6d-8e7f-9a0b1c2d3e4f")]
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
                    instance = "/api/v1/users/me/favorites/status"
                },
                new
                {
                    type = "https://tools.ietf.org/html/rfc7235#section-3.1",
                    status = 401,
                    title = "Unauthorized",
                    detail = "The token has expired",
                    instance = "/api/v1/users/me/favorites/status"
                },
                new
                {
                    type = "https://tools.ietf.org/html/rfc7235#section-3.1",
                    status = 401,
                    title = "Unauthorized",
                    detail = "The token is invalid",
                    instance = "/api/v1/users/me/favorites/status"
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
                instance = "/api/v1/users/me/favorites/status",
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
                instance = "/api/v1/users/me/favorites/status",
                errors = new Dictionary<string, string[]>
                {
                    {
                        "General.Validation", new[]
                        {
                            "FavoriteMediaKindIsInvalid",
                            "FavoriteMediaItemIdsCannotBeEmpty",
                            "FavoriteMediaItemIdsExceedMaximumCount",
                            "FavoriteMediaItemIdCannotBeEmpty"
                        }
                    }
                },
                traceId = "00-2470be4248a2a5a0c6f70579975a6954-b9c3ba9544a03500-00"
            }
        );
    }
}
