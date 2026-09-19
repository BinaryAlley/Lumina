#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Contracts.Requests.MediaLibrary.AudioLibrary.MusicLibrary.Artists;
using Lumina.Contracts.Responses.Common;
using Lumina.Contracts.Responses.MediaLibrary.AudioLibrary.MusicLibrary.Artists;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Presentation.Api.Core.Endpoints.Library.AudioLibrary.MusicLibrary.Artists.GetArtistsLite;

/// <summary>
/// Class used for providing a textual description for the <see cref="GetArtistsLiteEndpoint"/> API endpoint, for OpenAPI.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetArtistsLiteEndpointSummary : Summary<GetArtistsLiteEndpoint, GetArtistsLiteRequest>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GetArtistsLiteEndpointSummary"/> class.
    /// </summary>
    public GetArtistsLiteEndpointSummary()
    {
        Summary = "Retrieves the lightweight read models of the artists of a media library.";
        Description = "Returns the lightweight read models of the artists of a media library, projecting only the fields needed to display the artists in a card-based grid, without loading the full artist entities and their related data. The page is returned to an Admin, who can see the artists of all libraries, or to the owner of the library.";

        ExampleRequest = new GetArtistsLiteRequest(
            CurrentPage: 1,
            PerPage: 48,
            SearchTerm: "queen"
        );

        RequestParam(r => r.CurrentPage, "The page of results to retrieve. Optional.");
        RequestParam(r => r.PerPage, "The maximum number of artists to retrieve per page. Optional.");
        RequestParam(r => r.SearchTerm, "The search term used to filter the artists by name. Optional.");

        ResponseParam<ArtistLiteResponse>(r => r.Id, "The Id of the artist.");
        ResponseParam<ArtistLiteResponse>(r => r.Name, "The name of the artist.");

        Response(200, "The paginated list of the lightweight read models of the artists is returned.",
            example: new PaginatedResponse<ArtistLiteResponse>
            {
                Data =
                [
                    new ArtistLiteResponse(
                        Id: Guid.NewGuid(),
                        Name: "Queen"
                    ),
                    new ArtistLiteResponse(
                        Id: Guid.NewGuid(),
                        Name: "David Bowie"
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
                    instance = $"/api/v1/libraries/{Guid.NewGuid()}/artists/lite"
                },
                new
                {
                    type = "https://tools.ietf.org/html/rfc7235#section-3.1",
                    status = 401,
                    title = "Unauthorized",
                    detail = "Invalid token: The token expired at '01/01/2024 01:00:00'",
                    instance = $"/api/v1/libraries/{Guid.NewGuid()}/artists/lite"
                },
                new
                {
                    type = "https://tools.ietf.org/html/rfc7235#section-3.1",
                    status = 401,
                    title = "Unauthorized",
                    detail = "The token is invalid",
                    instance = $"/api/v1/libraries/{Guid.NewGuid()}/artists/lite"
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
                instance = $"/api/v1/libraries/{Guid.NewGuid()}/artists/lite",
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
                instance = $"/api/v1/libraries/{Guid.NewGuid()}/artists/lite",
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
