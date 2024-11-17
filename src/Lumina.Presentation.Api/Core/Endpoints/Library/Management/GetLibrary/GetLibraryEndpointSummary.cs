#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Contracts.Requests.MediaLibrary.Management;
using Lumina.Contracts.Responses.MediaLibrary.Management;
using Lumina.Domain.Common.Enums.MediaLibrary;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Presentation.Api.Core.Endpoints.Library.Management.GetLibrary;

/// <summary>
/// Class used for providing a textual description for the <see cref="GetLibraryEndpoint"/> API endpoint, for OpenAPI.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetLibraryEndpointSummary : Summary<GetLibraryEndpoint, GetLibraryRequest>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GetLibraryEndpointSummary"/> class.
    /// </summary>
    public GetLibraryEndpointSummary()
    {
        Summary = "Retrieves a media library by its Id.";
        Description = "Retrieves a media library by its Id, if the user making the request is an Admin, or the owner of the library.";

        ExampleRequest = new GetLibraryRequest(
            Id: Guid.NewGuid()
        );

        RequestParam(r => r.Id, "The Id of the media library. Required.");

        ResponseParam<LibraryResponse>(r => r.Id, "The unique identifier of the entity.");
        ResponseParam<LibraryResponse>(r => r.UserId, "The Id of the user owning the media library.");
        ResponseParam<LibraryResponse>(r => r.Title, "The title of the media library.");
        ResponseParam<LibraryResponse>(r => r.LibraryType, "The type of the media library.");
        ResponseParam<LibraryResponse>(r => r.ContentLocations, "The file system paths of the directories where the media library elements are located.");
        ResponseParam<LibraryResponse>(r => r.Created, "The date and time when the entity was created.");
        ResponseParam<LibraryResponse>(r => r.Updated, "The date and time when the entity was last updated.");

        Response(200, "The media library is returned.", example: new LibraryResponse(
            Id: Guid.NewGuid(),
            UserId: Guid.NewGuid(),
            Title: "TV Shows",
            LibraryType: LibraryType.TvShow,
            ContentLocations: ["/media/tv shows/drama/", "/media/tv shows/SCI-FI/"],
            Created: DateTime.UtcNow,
            Updated: default
        ));


        Response(401, "Authentication required.", "application/problem+json",
            example: new[]
            {
                new
                {
                    type = "https://tools.ietf.org/html/rfc7235#section-3.1",
                    status = 401,
                    title = "Unauthorized",
                    detail = "You are not authorized",
                    instance = "/api/v1/libraries/{id}"
                },
                new
                {
                    type = "https://tools.ietf.org/html/rfc7235#section-3.1",
                    status = 401,
                    title = "Unauthorized",
                    detail = "Invalid token: The token expired at '01/01/2024 01:00:00'",
                    instance = "/api/v1/libraries/{id}"
                },
                new
                {
                    type = "https://tools.ietf.org/html/rfc7235#section-3.1",
                    status = 401,
                    title = "Unauthorized",
                    detail = "The token is invalid",
                    instance = "/api/v1/libraries/{id}"
                }
            }
        );

        Response(403, "The request failed because the user making the request is not an Admin, or the owner of the media library.", "application/problem+json",
            example: new
            {
                type = "https://tools.ietf.org/html/rfc9110#section-15.5.4",
                title = "General.Failure",
                status = 403,
                detail = "NotAuthorized",
                instance = "/api/v1/libraries/{id}",
                traceId = "00-a712bbf99ca8ab485f86a762ae5ae74d-b3a2eb78813b0a5d-00"
            }
        );

        Response(404, "The request failed because the requested media library does not exist.", "application/problem+json",
            example: new
            {
                type = "https://tools.ietf.org/html/rfc9110#section-15.5.5",
                title = "General.NotFound",
                status = 404,
                detail = "LibraryNotFound",
                instance = "/api/v1/libraries/{id}",
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
                instance = "/api/v1/libraries/{id}",
                errors = new Dictionary<string, string[]>
                {
                    {
                        "General.Validation", new[]
                        {
                            "UserIdCannotBeEmpty",
                            "LibraryIdCannotBeEmpty"
                        }
                    }
                },
                traceId = "00-2470be4248a2a5a0c6f70579975a6954-b9c3ba9544a03500-00"
            }
        );

    }
}
