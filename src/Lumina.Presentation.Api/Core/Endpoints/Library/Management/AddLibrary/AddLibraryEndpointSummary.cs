#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Contracts.Requests.MediaLibrary.Management;
using Lumina.Contracts.Responses.MediaLibrary.Management;
using Lumina.Domain.Common.Enums.MediaLibrary;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Presentation.Api.Core.Endpoints.Library.Management.AddLibrary;

/// <summary>
/// Class used for providing a textual description for the <see cref="AddLibraryEndpoint"/> API endpoint, for OpenAPI.
/// </summary>
[ExcludeFromCodeCoverage]
public class AddLibraryEndpointSummary : Summary<AddLibraryEndpoint, AddLibraryRequest>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AddLibraryEndpointSummary"/> class.
    /// </summary>
    public AddLibraryEndpointSummary()
    {
        Summary = "Adds a new media library.";
        Description = "Creates a new media library and returns its details, including the location of the newly created resource.";

        ExampleRequest = new AddLibraryRequest(
            Title: "TV Shows",
            LibraryType: "TvShow",
            ContentLocations: [
                "/media/tv shows/drama/",
                "/media/tv shows/SCI-FI/",
            ]
        );

        RequestParam(r => r.Title, "The title of the media library. Required.");
        RequestParam(r => r.LibraryType, "The type of the media library. Required.");
        RequestParam(r => r.ContentLocations, ">The file system paths of the directories where the media library elements are located. Required.");

        ResponseParam<LibraryResponse>(r => r.Id, "The unique identifier of the entity.");
        ResponseParam<LibraryResponse>(r => r.UserId, "The Id of the user owning the media library.");
        ResponseParam<LibraryResponse>(r => r.Title, "The title of the media library.");
        ResponseParam<LibraryResponse>(r => r.LibraryType, "The type of the media library.");
        ResponseParam<LibraryResponse>(r => r.ContentLocations, "The file system paths of the directories where the media library elements are located.");
        ResponseParam<LibraryResponse>(r => r.CreatedOnUtc, "The date and time when the entity was created.");
        ResponseParam<LibraryResponse>(r => r.UpdatedOnUtc, "The date and time when the entity was last updated.");


        Response(201, "The media library was successfully created.", example:
            new LibraryResponse(
                Id: Guid.NewGuid(),
                UserId: Guid.NewGuid(),
                Title: "TV Shows",
                LibraryType: LibraryType.TvShow,
                ContentLocations: ["/media/tv shows/drama/", "/media/tv shows/SCI-FI/"],
                CreatedOnUtc: DateTime.UtcNow,
                UpdatedOnUtc: default
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
                    instance = "/api/v1/libraries"
                },
                new
                {
                    type = "https://tools.ietf.org/html/rfc7235#section-3.1",
                    status = 401,
                    title = "Unauthorized",
                    detail = "Invalid token: The token expired at '01/01/2024 01:00:00'",
                    instance = "/api/v1/libraries"
                },
                new
                {
                    type = "https://tools.ietf.org/html/rfc7235#section-3.1",
                    status = 401,
                    title = "Unauthorized",
                    detail = "The token is invalid",
                    instance = "/api/v1/libraries"
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
                instance = "/api/v1/libraries",
                errors = new Dictionary<string, string[]>
                {
                    {
                        "General.Validation", new[]
                        {
                            "UserIdCannotBeEmpty",
                            "LibraryTypeCannotBeNull",
                            "UnknownLibraryType",
                            "PathsListCannotBeNull",
                            "PathsListCannotBeEmpty",
                            "PathCannotBeEmpty",
                            "TitleCannotBeEmpty",
                            "TitleMustBeMaximum255CharactersLong"
                        }
                    }
                },
                traceId = "00-2470be4248a2a5a0c6f70579975a6954-b9c3ba9544a03500-00"
            }
        );
    }
}
