#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Contracts.Requests.MediaLibrary.AudioLibrary.MusicLibrary.Albums;
using Lumina.Contracts.Responses.Common;
using Lumina.Contracts.Responses.MediaLibrary.AudioLibrary.MusicLibrary.Tracks;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Presentation.Api.Core.Endpoints.Library.AudioLibrary.MusicLibrary.Albums.GetAlbumTracksLite;

/// <summary>
/// Class used for providing a textual description for the <see cref="GetAlbumTracksLiteEndpoint"/> API endpoint, for OpenAPI.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetAlbumTracksLiteEndpointSummary : Summary<GetAlbumTracksLiteEndpoint, GetAlbumTracksLiteRequest>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GetAlbumTracksLiteEndpointSummary"/> class.
    /// </summary>
    public GetAlbumTracksLiteEndpointSummary()
    {
        Summary = "Retrieves the lightweight read models of the tracks of an album.";
        Description = "Returns the lightweight read models of the tracks of the album identified by its Id, projecting only the fields needed to display the songs of an album, without loading the full track entities and their related data. The page is returned to an Admin, who can see the tracks of the albums of all libraries, or to the owner of the library of the album.";

        ExampleRequest = new GetAlbumTracksLiteRequest(
            CurrentPage: 1,
            PerPage: 48
        );

        RequestParam(r => r.CurrentPage, "The page of results to retrieve. Optional.");
        RequestParam(r => r.PerPage, "The maximum number of tracks to retrieve per page. Optional.");

        ResponseParam<TrackLiteResponse>(r => r.Id, "The Id of the track.");
        ResponseParam<TrackLiteResponse>(r => r.Title, "The title of the track.");
        ResponseParam<TrackLiteResponse>(r => r.TrackNumber, "The number of the track on its disc.");
        ResponseParam<TrackLiteResponse>(r => r.DiscNumber, "The number of the disc the track belongs to, if applicable.");

        Response(200, "The paginated list of the lightweight read models of the tracks of the album is returned.",
            example: new PaginatedResponse<TrackLiteResponse>
            {
                Data =
                [
                    new TrackLiteResponse(
                        Id: Guid.NewGuid(),
                        Title: "Bohemian Rhapsody",
                        TrackNumber: 1,
                        DiscNumber: 1
                    ),
                    new TrackLiteResponse(
                        Id: Guid.NewGuid(),
                        Title: "You're My Best Friend",
                        TrackNumber: 2,
                        DiscNumber: 1
                    )
                ],
                CurrentPage = 1,
                PerPage = 10,
                Count = 2,
                NumberOfPages = 1
            }
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
                    instance = $"/api/v1/libraries/{Guid.NewGuid()}/artists/{Guid.NewGuid()}/albums/{Guid.NewGuid()}/tracks/lite"
                },
                new
                {
                    type = "https://tools.ietf.org/html/rfc7235#section-3.1",
                    status = 401,
                    title = "Unauthorized",
                    detail = "The token has expired",
                    instance = $"/api/v1/libraries/{Guid.NewGuid()}/artists/{Guid.NewGuid()}/albums/{Guid.NewGuid()}/tracks/lite"
                },
                new
                {
                    type = "https://tools.ietf.org/html/rfc7235#section-3.1",
                    status = 401,
                    title = "Unauthorized",
                    detail = "The token is invalid",
                    instance = $"/api/v1/libraries/{Guid.NewGuid()}/artists/{Guid.NewGuid()}/albums/{Guid.NewGuid()}/tracks/lite"
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
                instance = $"/api/v1/libraries/{Guid.NewGuid()}/artists/{Guid.NewGuid()}/albums/{Guid.NewGuid()}/tracks/lite",
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
                instance = $"/api/v1/libraries/{Guid.NewGuid()}/artists/{Guid.NewGuid()}/albums/{Guid.NewGuid()}/tracks/lite",
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
                instance = $"/api/v1/libraries/{Guid.NewGuid()}/artists/{Guid.NewGuid()}/albums/{Guid.NewGuid()}/tracks/lite",
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
