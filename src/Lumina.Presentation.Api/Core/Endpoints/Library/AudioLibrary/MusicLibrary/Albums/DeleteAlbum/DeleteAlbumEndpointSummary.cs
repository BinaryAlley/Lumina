#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Contracts.Requests.MediaLibrary.AudioLibrary.MusicLibrary.Albums;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Presentation.Api.Core.Endpoints.Library.AudioLibrary.MusicLibrary.Albums.DeleteAlbum;

/// <summary>
/// Class used for providing a textual description for the <see cref="DeleteAlbumEndpoint"/> API endpoint, for OpenAPI.
/// </summary>
[ExcludeFromCodeCoverage]
public class DeleteAlbumEndpointSummary : Summary<DeleteAlbumEndpoint, DeleteAlbumRequest>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DeleteAlbumEndpointSummary"/> class.
    /// </summary>
    public DeleteAlbumEndpointSummary()
    {
        Summary = "Deletes an existing album.";
        Description = "Deletes the album identified by the request, together with its tracks. The album is deleted by an Admin, who can delete the albums of all libraries, or by the owner of the library of the album.";

        ExampleRequest = new DeleteAlbumRequest(
            ArtistId: Guid.NewGuid(),
            AlbumId: Guid.NewGuid()
        );

        RequestParam(r => r.ArtistId, "The unique identifier of the artist the album belongs to, taken from the route.");
        RequestParam(r => r.AlbumId, "The Id of the album to delete, taken from the route. Required.");

        Response(200, "The album was successfully deleted.");

        Response(401, "Authentication required.", "application/problem+json",
            example: new[]
            {
                new
                {
                    type = "https://tools.ietf.org/html/rfc7235#section-3.1",
                    status = 401,
                    title = "Unauthorized",
                    detail = "You are not authorized",
                    instance = "/api/v1/libraries/{libraryId}/artists/{artistId}/albums/{albumId}"
                },
                new
                {
                    type = "https://tools.ietf.org/html/rfc7235#section-3.1",
                    status = 401,
                    title = "Unauthorized",
                    detail = "Invalid token: The token expired at '01/01/2024 01:00:00'",
                    instance = "/api/v1/libraries/{libraryId}/artists/{artistId}/albums/{albumId}"
                },
                new
                {
                    type = "https://tools.ietf.org/html/rfc7235#section-3.1",
                    status = 401,
                    title = "Unauthorized",
                    detail = "The token is invalid",
                    instance = "/api/v1/libraries/{libraryId}/artists/{artistId}/albums/{albumId}"
                }
            }
        );

        Response(403, "The request failed because the user making the request is not an Admin, or the owner of the media library.", "application/problem+json",
            example: new
            {
                type = "https://tools.ietf.org/html/rfc9110#section-15.5.4",
                title = "General.Unauthorized",
                status = 403,
                detail = "NotAuthorized",
                instance = "/api/v1/libraries/{libraryId}/artists/{artistId}/albums/{albumId}",
                traceId = "00-a712bbf99ca8ab485f86a762ae5ae74d-b3a2eb78813b0a5d-00"
            }
        );

        Response(404, "The request failed because the requested album does not exist.", "application/problem+json",
            example: new
            {
                type = "https://tools.ietf.org/html/rfc9110#section-15.5.5",
                title = "General.NotFound",
                status = 404,
                detail = "AlbumNotFound",
                instance = "/api/v1/libraries/{libraryId}/artists/{artistId}/albums/{albumId}",
                traceId = "00-57d15dadd702dbd4aeb5dc9b7cee68ee-9330237dbb2ce0e5-00"
            }
        );

        Response(422, "The request did not pass validation checks.", "application/problem+json",
            example: new
            {
                type = "https://tools.ietf.org/html/rfc4918#section-11.2",
                title = "General.Validation",
                status = 422,
                detail = "OneOrMoreValidationErrorsOccurred",
                instance = "/api/v1/libraries/{libraryId}/artists/{artistId}/albums/{albumId}",
                errors = new Dictionary<string, string[]>
                {
                    {
                        "General.Validation", new[]
                        {
                            "AlbumIdCannotBeEmpty"
                        }
                    }
                },
                traceId = "00-2470be4248a2a5a0c6f70579975a6954-b9c3ba9544a03500-00"
            }
        );
    }
}
