#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Contracts.Requests.MediaLibrary.AudioLibrary.MusicLibrary.Artists;
using Lumina.Contracts.Responses.Common;
using Lumina.Contracts.Responses.MediaLibrary.AudioLibrary.MusicLibrary.Artists;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Presentation.Api.Core.Endpoints.Library.AudioLibrary.MusicLibrary.Artists.GetArtists;

/// <summary>
/// Class used for providing a textual description for the <see cref="GetArtistsEndpoint"/> API endpoint, for OpenAPI.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetArtistsEndpointSummary : Summary<GetArtistsEndpoint, GetArtistsRequest>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GetArtistsEndpointSummary"/> class.
    /// </summary>
    public GetArtistsEndpointSummary()
    {
        Summary = "Retrieves the list of artists of a media library.";
        Description = "Returns the paginated list of the artists of the media library identified by the route, with the full details of each artist. The page is returned to an Admin, who can see the artists of all libraries, or to the owner of the library.";

        ExampleRequest = new GetArtistsRequest(
            CurrentPage: 1,
            PerPage: 48,
            SearchTerm: "queen"
        );

        RequestParam(r => r.CurrentPage, "The page of results to retrieve. Optional.");
        RequestParam(r => r.PerPage, "The maximum number of artists to retrieve per page. Optional.");
        RequestParam(r => r.SearchTerm, "The search term used to filter the artists by name. Optional.");

        ResponseParam<ArtistResponse>(r => r.Id, "The Id of the artist.");
        ResponseParam<ArtistResponse>(r => r.LibraryId, "The Id of the media library this artist belongs to.");
        ResponseParam<ArtistResponse>(r => r.Name, "The name of the artist.");
        ResponseParam<ArtistResponse>(r => r.Website, "The website of the artist, if applicable.");
        ResponseParam<ArtistResponse>(r => r.MusicBrainzArtistId, "The MusicBrainz identifier of the artist, if applicable.");
        ResponseParam<ArtistResponse>(r => r.Contributors, "The list of references to the media contributors that make up the artist, each with the role they played.");
        ResponseParam<ArtistResponse>(r => r.Albums, "The list of albums of the artist, each with its own list of tracks.");
        ResponseParam<ArtistResponse>(r => r.CreatedOnUtc, "The date and time when the artist was created.");
        ResponseParam<ArtistResponse>(r => r.UpdatedOnUtc, "The date and time when the artist was last updated, if applicable.");

        Response(200, "The paginated list of the artists of the media library is returned.",
            example: new PaginatedResponse<ArtistResponse>
            {
                Data =
                [
                    new ArtistResponse(
                        Id: Guid.NewGuid(),
                        LibraryId: Guid.NewGuid(),
                        Name: "Queen",
                        Website: "https://www.queenonline.com",
                        MusicBrainzArtistId: Guid.NewGuid(),
                        Contributors: [],
                        Albums: [],
                        CreatedOnUtc: DateTime.UtcNow,
                        UpdatedOnUtc: DateTime.UtcNow
                    ),
                    new ArtistResponse(
                        Id: Guid.NewGuid(),
                        LibraryId: Guid.NewGuid(),
                        Name: "David Bowie",
                        Website: "https://www.davidbowie.com",
                        MusicBrainzArtistId: Guid.NewGuid(),
                        Contributors: [],
                        Albums: [],
                        CreatedOnUtc: DateTime.UtcNow,
                        UpdatedOnUtc: DateTime.UtcNow
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
                    instance = $"/api/v1/libraries/{Guid.NewGuid()}/artists"
                },
                new
                {
                    type = "https://tools.ietf.org/html/rfc7235#section-3.1",
                    status = 401,
                    title = "Unauthorized",
                    detail = "The token has expired",
                    instance = $"/api/v1/libraries/{Guid.NewGuid()}/artists"
                },
                new
                {
                    type = "https://tools.ietf.org/html/rfc7235#section-3.1",
                    status = 401,
                    title = "Unauthorized",
                    detail = "The token is invalid",
                    instance = $"/api/v1/libraries/{Guid.NewGuid()}/artists"
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
                instance = $"/api/v1/libraries/{Guid.NewGuid()}/artists",
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
                instance = $"/api/v1/libraries/{Guid.NewGuid()}/artists",
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
