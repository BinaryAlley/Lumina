#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Contracts.Responses.MediaLibrary.AudioLibrary.MusicLibrary.Albums;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Presentation.Api.Core.Endpoints.Library.AudioLibrary.MusicLibrary.Albums.GetDeletedAlbums;

/// <summary>
/// Class used for providing a textual description for the <see cref="GetDeletedAlbumsEndpoint"/> API endpoint, for OpenAPI.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetDeletedAlbumsEndpointSummary : Summary<GetDeletedAlbumsEndpoint, EmptyRequest>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GetDeletedAlbumsEndpointSummary"/> class.
    /// </summary>
    public GetDeletedAlbumsEndpointSummary()
    {
        Summary = "Retrieves the deleted albums of a media library.";
        Description = "Returns the albums of the media library identified by the route that the user deleted and that can still be restored, ordered by the time they were deleted, most recent first. An album whose artist is also deleted is not returned until the artist is restored. The list is returned to an Admin, who can see the deleted albums of all libraries, or to the owner of the library.";

        ResponseParam<DeletedAlbumResponse>(response => response.Id, "The Id of the deleted album.");
        ResponseParam<DeletedAlbumResponse>(response => response.LibraryId, "The Id of the media library the deleted album belongs to.");
        ResponseParam<DeletedAlbumResponse>(response => response.ArtistId, "The Id of the artist the deleted album belongs to.");
        ResponseParam<DeletedAlbumResponse>(response => response.ArtistName, "The name of the artist the deleted album belongs to.");
        ResponseParam<DeletedAlbumResponse>(response => response.Title, "The title of the deleted album.");
        ResponseParam<DeletedAlbumResponse>(response => response.DeletedOnUtc, "The date and time when the album was deleted.");
        ResponseParam<DeletedAlbumResponse>(response => response.CoverPath, "The path of the cover image of the deleted album, if available.");

        Response(200, "The deleted albums are returned.",
            example: new DeletedAlbumResponse[]
            {
                new(
                    Id: Guid.Parse("b2c3d4e5-f6a7-4b8c-9d0e-1f2a3b4c5d6e"),
                    LibraryId: Guid.Parse("3b3a19f3-1f5a-4d5a-9a3a-5c5a4a3a2a1a"),
                    ArtistId: Guid.Parse("a1b2c3d4-e5f6-4a7b-8c9d-0e1f2a3b4c5d"),
                    ArtistName: "Queen",
                    Title: "A Night at the Opera",
                    DeletedOnUtc: DateTime.UtcNow,
                    CoverPath: "/media/music/queen/a-night-at-the-opera/cover.jpg"
                )
            }
        );

        Response(401, "Authentication required.", "application/problem+json",
            example: new
            {
                type = "https://tools.ietf.org/html/rfc7235#section-3.1",
                status = 401,
                title = "Unauthorized",
                detail = "Authentication failed",
                instance = $"/api/v1/libraries/{Guid.NewGuid()}/albums/deleted-items"
            }
        );

        Response(403, "The request failed because the user making the request is not an Admin, or the owner of the media library.", "application/problem+json",
            example: new
            {
                type = "https://tools.ietf.org/html/rfc9110#section-15.5.4",
                title = "General.Unauthorized",
                status = 403,
                detail = "NotAuthorized",
                instance = $"/api/v1/libraries/{Guid.NewGuid()}/albums/deleted-items",
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
                instance = $"/api/v1/libraries/{Guid.NewGuid()}/albums/deleted-items",
                errors = new Dictionary<string, string[]>
                {
                    {
                        "General.Validation", new[]
                        {
                            "LibraryIdCannotBeEmpty"
                        }
                    }
                },
                traceId = "00-2470be4248a2a5a0c6f70579975a6954-b9c3ba9544a03500-00"
            }
        );
    }
}
