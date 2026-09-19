#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Contracts.Requests.MediaLibrary.AudioLibrary.MusicLibrary.Artists;
using Lumina.Contracts.Responses.Common;
using Lumina.Contracts.Responses.MediaLibrary.AudioLibrary.MusicLibrary.Albums;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Presentation.Api.Core.Endpoints.Library.AudioLibrary.MusicLibrary.Artists.GetArtistAlbumsLite;

/// <summary>
/// Class used for providing a textual description for the <see cref="GetArtistAlbumsLiteEndpoint"/> API endpoint, for OpenAPI.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetArtistAlbumsLiteEndpointSummary : Summary<GetArtistAlbumsLiteEndpoint, GetArtistAlbumsLiteRequest>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GetArtistAlbumsLiteEndpointSummary"/> class.
    /// </summary>
    public GetArtistAlbumsLiteEndpointSummary()
    {
        Summary = "Retrieves the lightweight read models of the albums of an artist.";
        Description = "Returns the lightweight read models of the albums of the artist identified by its Id, projecting only the fields needed to display the albums in a card-based grid, without loading the full album entities and their related data. The page is returned to an Admin, who can see the albums of the artists of all libraries, or to the owner of the library of the artist.";

        ExampleRequest = new GetArtistAlbumsLiteRequest(
            CurrentPage: 1,
            PerPage: 48
        );

        RequestParam(r => r.CurrentPage, "The page of results to retrieve. Optional.");
        RequestParam(r => r.PerPage, "The maximum number of albums to retrieve per page. Optional.");

        ResponseParam<AlbumLiteResponse>(r => r.Id, "The Id of the album.");
        ResponseParam<AlbumLiteResponse>(r => r.Title, "The title of the album.");
        ResponseParam<AlbumLiteResponse>(r => r.TotalTracks, "The number of tracks of the release.");

        Response(200, "The paginated list of the lightweight read models of the albums of the artist is returned.",
            example: new PaginatedResponse<AlbumLiteResponse>
            {
                Data =
                [
                    new AlbumLiteResponse(
                        Id: Guid.NewGuid(),
                        Title: "A Night at the Opera",
                        TotalTracks: 12
                    ),
                    new AlbumLiteResponse(
                        Id: Guid.NewGuid(),
                        Title: "The Works",
                        TotalTracks: 9
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
                    detail = "You are not authorized",
                    instance = $"/api/v1/libraries/{Guid.NewGuid()}/artists/{Guid.NewGuid()}/albums/lite"
                },
                new
                {
                    type = "https://tools.ietf.org/html/rfc7235#section-3.1",
                    status = 401,
                    title = "Unauthorized",
                    detail = "Invalid token: The token expired at '01/01/2024 01:00:00'",
                    instance = $"/api/v1/libraries/{Guid.NewGuid()}/artists/{Guid.NewGuid()}/albums/lite"
                },
                new
                {
                    type = "https://tools.ietf.org/html/rfc7235#section-3.1",
                    status = 401,
                    title = "Unauthorized",
                    detail = "The token is invalid",
                    instance = $"/api/v1/libraries/{Guid.NewGuid()}/artists/{Guid.NewGuid()}/albums/lite"
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
                instance = $"/api/v1/libraries/{Guid.NewGuid()}/artists/{Guid.NewGuid()}/albums/lite",
                traceId = "00-a712bbf99ca8ab485f86a762ae5ae74d-b3a2eb78813b0a5d-00"
            }
        );

        Response(404, "The request failed because the requested artist does not exist.", "application/problem+json",
            example: new
            {
                type = "https://tools.ietf.org/html/rfc9110#section-15.5.5",
                title = "General.NotFound",
                status = 404,
                detail = "ArtistNotFound",
                instance = $"/api/v1/libraries/{Guid.NewGuid()}/artists/{Guid.NewGuid()}/albums/lite",
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
                instance = $"/api/v1/libraries/{Guid.NewGuid()}/artists/{Guid.NewGuid()}/albums/lite",
                errors = new Dictionary<string, string[]>
                {
                    {
                        "General.Validation", new[]
                        {
                            "ArtistIdCannotBeEmpty"
                        }
                    }
                },
                traceId = "00-2470be4248a2a5a0c6f70579975a6954-b9c3ba9544a03500-00"
            }
        );
    }
}
