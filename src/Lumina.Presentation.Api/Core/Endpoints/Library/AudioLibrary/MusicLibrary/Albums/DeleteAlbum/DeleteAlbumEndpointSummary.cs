#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Presentation.Api.Core.Endpoints.Library.AudioLibrary.MusicLibrary.Albums.DeleteAlbum;

/// <summary>
/// Class used for providing a textual description for the <see cref="DeleteAlbumEndpoint"/> API endpoint, for OpenAPI.
/// </summary>
[ExcludeFromCodeCoverage]
public class DeleteAlbumEndpointSummary : Summary<DeleteAlbumEndpoint, EmptyRequest>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DeleteAlbumEndpointSummary"/> class.
    /// </summary>
    public DeleteAlbumEndpointSummary()
    {
        Summary = "Deletes an existing album.";
        Description = "Deletes the album identified by the route, together with its tracks. An artist must always keep at least one album. The album is deleted by an Admin, who can delete the albums of all libraries, or by the owner of the library of the album.";

        Response(200, "The album was successfully deleted.");

        Response(401, "Authentication required.", "application/problem+json",
            example: new[]
            {
                new
                {
                    type = "https://tools.ietf.org/html/rfc7235#section-3.1",
                    status = 401,
                    title = "Unauthorized",
                    detail = "Authentication failed",
                    instance = $"/api/v1/libraries/{Guid.NewGuid()}/artists/{Guid.NewGuid()}/albums/{Guid.NewGuid()}"
                },
                new
                {
                    type = "https://tools.ietf.org/html/rfc7235#section-3.1",
                    status = 401,
                    title = "Unauthorized",
                    detail = "The token has expired",
                    instance = $"/api/v1/libraries/{Guid.NewGuid()}/artists/{Guid.NewGuid()}/albums/{Guid.NewGuid()}"
                },
                new
                {
                    type = "https://tools.ietf.org/html/rfc7235#section-3.1",
                    status = 401,
                    title = "Unauthorized",
                    detail = "The token is invalid",
                    instance = $"/api/v1/libraries/{Guid.NewGuid()}/artists/{Guid.NewGuid()}/albums/{Guid.NewGuid()}"
                }
            }
        );

        Response(403, "The request failed because the user making the request is not an Admin or the owner of the media library, or because deleting the album would leave its artist without any albums.", "application/problem+json",
            example: new[]
            {
                new
                {
                    type = "https://tools.ietf.org/html/rfc9110#section-15.5.4",
                    title = "General.Unauthorized",
                    status = 403,
                    detail = "NotAuthorized",
                    instance = $"/api/v1/libraries/{Guid.NewGuid()}/artists/{Guid.NewGuid()}/albums/{Guid.NewGuid()}",
                    traceId = "00-a712bbf99ca8ab485f86a762ae5ae74d-b3a2eb78813b0a5d-00"
                },
                new
                {
                    type = "https://tools.ietf.org/html/rfc9110#section-15.5.4",
                    title = "General.Forbidden",
                    status = 403,
                    detail = "ArtistMustHaveAtLeastOneAlbum",
                    instance = $"/api/v1/libraries/{Guid.NewGuid()}/artists/{Guid.NewGuid()}/albums/{Guid.NewGuid()}",
                    traceId = "00-a712bbf99ca8ab485f86a762ae5ae74d-b3a2eb78813b0a5d-00"
                }
            }
        );

        Response(404, "The request failed because the requested artist, album, or their media library does not exist.", "application/problem+json",
            example: new[]
            {
                new
                {
                    type = "https://tools.ietf.org/html/rfc9110#section-15.5.5",
                    title = "General.NotFound",
                    status = 404,
                    detail = "ArtistNotFound",
                    instance = $"/api/v1/libraries/{Guid.NewGuid()}/artists/{Guid.NewGuid()}/albums/{Guid.NewGuid()}",
                    traceId = "00-57d15dadd702dbd4aeb5dc9b7cee68ee-9330237dbb2ce0e5-00"
                },
                new
                {
                    type = "https://tools.ietf.org/html/rfc9110#section-15.5.5",
                    title = "General.NotFound",
                    status = 404,
                    detail = "AlbumNotFound",
                    instance = $"/api/v1/libraries/{Guid.NewGuid()}/artists/{Guid.NewGuid()}/albums/{Guid.NewGuid()}",
                    traceId = "00-57d15dadd702dbd4aeb5dc9b7cee68ee-9330237dbb2ce0e5-00"
                },
                new
                {
                    type = "https://tools.ietf.org/html/rfc9110#section-15.5.5",
                    title = "General.NotFound",
                    status = 404,
                    detail = "LibraryNotFound",
                    instance = $"/api/v1/libraries/{Guid.NewGuid()}/artists/{Guid.NewGuid()}/albums/{Guid.NewGuid()}",
                    traceId = "00-57d15dadd702dbd4aeb5dc9b7cee68ee-9330237dbb2ce0e5-00"
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
                instance = $"/api/v1/libraries/{Guid.NewGuid()}/artists/{Guid.NewGuid()}/albums/{Guid.NewGuid()}",
                errors = new Dictionary<string, string[]>
                {
                    {
                        "General.Validation", new[]
                        {
                            "LibraryIdCannotBeEmpty",
                            "ArtistIdCannotBeEmpty",
                            "AlbumIdCannotBeEmpty"
                        }
                    }
                },
                traceId = "00-2470be4248a2a5a0c6f70579975a6954-b9c3ba9544a03500-00"
            }
        );
    }
}
