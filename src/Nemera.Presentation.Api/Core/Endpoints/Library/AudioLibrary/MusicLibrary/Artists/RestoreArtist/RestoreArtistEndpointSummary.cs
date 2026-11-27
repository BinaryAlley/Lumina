#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Nemera.Presentation.Api.Core.Endpoints.Library.AudioLibrary.MusicLibrary.Artists.RestoreArtist;

/// <summary>
/// Class used for providing a textual description for the <see cref="RestoreArtistEndpoint"/> API endpoint, for OpenAPI.
/// </summary>
[ExcludeFromCodeCoverage]
public class RestoreArtistEndpointSummary : Summary<RestoreArtistEndpoint, EmptyRequest>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="RestoreArtistEndpointSummary"/> class.
    /// </summary>
    public RestoreArtistEndpointSummary()
    {
        Summary = "Restores a deleted artist.";
        Description = "Restores the artist identified by the route, together with its stored metadata, artwork and the tracks that were not deleted on their own. The artist is restored by an Admin, who can restore the artists of all libraries, or by the owner of the library of the artist.";

        Response(200, "The artist was successfully restored.");

        Response(401, "Authentication required.", "application/problem+json",
            example: new
            {
                type = "https://tools.ietf.org/html/rfc7235#section-3.1",
                status = 401,
                title = "Unauthorized",
                detail = "Authentication failed",
                instance = $"/api/v1/libraries/{Guid.NewGuid()}/artists/{Guid.NewGuid()}/restore"
            }
        );

        Response(403, "The request failed because the user making the request is not an Admin, or the owner of the media library.", "application/problem+json",
            example: new
            {
                type = "https://tools.ietf.org/html/rfc9110#section-15.5.4",
                title = "General.Unauthorized",
                status = 403,
                detail = "NotAuthorized",
                instance = $"/api/v1/libraries/{Guid.NewGuid()}/artists/{Guid.NewGuid()}/restore",
                traceId = "00-a712bbf99ca8ab485f86a762ae5ae74d-b3a2eb78813b0a5d-00"
            }
        );

        Response(404, "The request failed because the requested artist does not exist or is not deleted.", "application/problem+json",
            example: new
            {
                type = "https://tools.ietf.org/html/rfc9110#section-15.5.5",
                title = "General.NotFound",
                status = 404,
                detail = "ArtistNotFound",
                instance = $"/api/v1/libraries/{Guid.NewGuid()}/artists/{Guid.NewGuid()}/restore",
                traceId = "00-57d15dadd702dbd4aeb5dc9b7cee68ee-9330237dbb2ce0e5-00"
            }
        );

        Response(409, "The request failed because a visible artist with the same name already exists in the library.", "application/problem+json",
            example: new
            {
                type = "https://tools.ietf.org/html/rfc9110#section-15.5.10",
                title = "General.Conflict",
                status = 409,
                detail = "ArtistAlreadyExists",
                instance = $"/api/v1/libraries/{Guid.NewGuid()}/artists/{Guid.NewGuid()}/restore",
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
                instance = $"/api/v1/libraries/{Guid.NewGuid()}/artists/{Guid.NewGuid()}/restore",
                errors = new Dictionary<string, string[]>
                {
                    {
                        "General.Validation", new[]
                        {
                            "LibraryIdCannotBeEmpty",
                            "ArtistIdCannotBeEmpty"
                        }
                    }
                },
                traceId = "00-2470be4248a2a5a0c6f70579975a6954-b9c3ba9544a03500-00"
            }
        );
    }
}
