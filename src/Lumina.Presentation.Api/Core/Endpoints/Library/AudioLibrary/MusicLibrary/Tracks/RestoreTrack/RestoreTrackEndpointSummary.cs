#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Presentation.Api.Core.Endpoints.Library.AudioLibrary.MusicLibrary.Tracks.RestoreTrack;

/// <summary>
/// Class used for providing a textual description for the <see cref="RestoreTrackEndpoint"/> API endpoint, for OpenAPI.
/// </summary>
[ExcludeFromCodeCoverage]
public class RestoreTrackEndpointSummary : Summary<RestoreTrackEndpoint, EmptyRequest>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="RestoreTrackEndpointSummary"/> class.
    /// </summary>
    public RestoreTrackEndpointSummary()
    {
        Summary = "Restores a deleted track.";
        Description = "Restores the track identified by the route, together with its stored metadata and artwork. The track is restored by an Admin, who can restore the tracks of all libraries, or by the owner of the library of the track.";

        Response(200, "The track was successfully restored.");

        Response(401, "Authentication required.", "application/problem+json",
            example: new
            {
                type = "https://tools.ietf.org/html/rfc7235#section-3.1",
                status = 401,
                title = "Unauthorized",
                detail = "Authentication failed",
                instance = $"/api/v1/libraries/{Guid.NewGuid()}/artists/{Guid.NewGuid()}/albums/{Guid.NewGuid()}/tracks/{Guid.NewGuid()}/restore"
            }
        );

        Response(403, "The request failed because the user making the request is not an Admin, or the owner of the media library.", "application/problem+json",
            example: new
            {
                type = "https://tools.ietf.org/html/rfc9110#section-15.5.4",
                title = "General.Unauthorized",
                status = 403,
                detail = "NotAuthorized",
                instance = $"/api/v1/libraries/{Guid.NewGuid()}/artists/{Guid.NewGuid()}/albums/{Guid.NewGuid()}/tracks/{Guid.NewGuid()}/restore",
                traceId = "00-a712bbf99ca8ab485f86a762ae5ae74d-b3a2eb78813b0a5d-00"
            }
        );

        Response(404, "The request failed because the requested track does not exist, is not deleted, or its album or artist is deleted.", "application/problem+json",
            example: new
            {
                type = "https://tools.ietf.org/html/rfc9110#section-15.5.5",
                title = "General.NotFound",
                status = 404,
                detail = "TrackNotFound",
                instance = $"/api/v1/libraries/{Guid.NewGuid()}/artists/{Guid.NewGuid()}/albums/{Guid.NewGuid()}/tracks/{Guid.NewGuid()}/restore",
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
                instance = $"/api/v1/libraries/{Guid.NewGuid()}/artists/{Guid.NewGuid()}/albums/{Guid.NewGuid()}/tracks/{Guid.NewGuid()}/restore",
                errors = new Dictionary<string, string[]>
                {
                    {
                        "General.Validation", new[]
                        {
                            "LibraryIdCannotBeEmpty",
                            "ArtistIdCannotBeEmpty",
                            "AlbumIdCannotBeEmpty",
                            "TrackIdCannotBeEmpty"
                        }
                    }
                },
                traceId = "00-2470be4248a2a5a0c6f70579975a6954-b9c3ba9544a03500-00"
            }
        );
    }
}
