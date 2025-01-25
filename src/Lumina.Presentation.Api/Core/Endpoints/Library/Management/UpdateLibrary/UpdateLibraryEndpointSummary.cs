#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Contracts.Requests.MediaLibrary.Management;
using Lumina.Contracts.Responses.MediaLibrary.Management;
using Lumina.Domain.Common.Enums.MediaLibrary;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Presentation.Api.Core.Endpoints.Library.Management.UpdateLibrary;

/// <summary>
/// Class used for providing a textual description for the <see cref="UpdateLibraryEndpoint"/> API endpoint, for OpenAPI.
/// </summary>
[ExcludeFromCodeCoverage]
public class UpdateLibraryEndpointSummary : Summary<UpdateLibraryEndpoint, UpdateLibraryRequest>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="UpdateLibraryEndpointSummary"/> class.
    /// </summary>
    public UpdateLibraryEndpointSummary()
    {
        Summary = "Updates an existing media library.";
        Description = "Updates a media library and returns its details, if the user making the request is an Admin, or the owner of the library.";

        ExampleRequest = new UpdateLibraryRequest(
            Id: Guid.NewGuid(),
            UserId: Guid.NewGuid(),
            Title: "TV Shows",
            LibraryType: "TvShow",
            ContentLocations: [
                "/media/tv shows/drama/",
                "/media/tv shows/SCI-FI/",
            ],
            CoverImage: "/media/posters/myPoster.jpg",
            IsEnabled: true,
            IsLocked: false,
            DownloadMedatadaFromWeb: true,
            SaveMetadataInMediaDirectories: false
        );

        RequestParam(r => r.Id, "The Id of the media library. Required.");
        RequestParam(r => r.UserId, "The Id of the user owning the media library. Required.");
        RequestParam(r => r.Title, "The title of the media library. Required.");
        RequestParam(r => r.LibraryType, "The type of the media library. Required.");
        RequestParam(r => r.ContentLocations, ">The file system paths of the directories where the media library elements are located. Required.");
        RequestParam(r => r.CoverImage, "The path of the image file used as the cover for the library. Optional.");
        RequestParam(r => r.IsEnabled, "Whether this media library is enabled or not. A disabled media library is never shown or changed. Optional.");
        RequestParam(r => r.IsLocked, "Whether this media library is locked or not. A locked media library is displayed, but is never changed or updated. Optional.");
        RequestParam(r => r.DownloadMedatadaFromWeb, "Whether this media library should update the metadata of its elements from the web, or not. Optional.");
        RequestParam(r => r.SaveMetadataInMediaDirectories, "Whether this media library should copy the downloaded metadata into the media library content locations, or not. Optional.");

        ResponseParam<LibraryResponse>(r => r.Id, "The unique identifier of the entity.");
        ResponseParam<LibraryResponse>(r => r.UserId, "The Id of the user owning the media library.");
        ResponseParam<LibraryResponse>(r => r.Title, "The title of the media library.");
        ResponseParam<LibraryResponse>(r => r.LibraryType, "The type of the media library.");
        ResponseParam<LibraryResponse>(r => r.ContentLocations, "The file system paths of the directories where the media library elements are located.");
        ResponseParam<LibraryResponse>(r => r.CoverImage, "The path of the image file used as the cover for the library.");
        ResponseParam<LibraryResponse>(r => r.IsEnabled, "Whether this media library is enabled or not. A disabled media library is never shown or changed.");
        ResponseParam<LibraryResponse>(r => r.IsLocked, "Whether this media library is locked or not. A locked media library is displayed, but is never changed or updated.");
        ResponseParam<LibraryResponse>(r => r.DownloadMedatadaFromWeb, "Whether this media library should update the metadata of its elements from the web, or not.");
        ResponseParam<LibraryResponse>(r => r.SaveMetadataInMediaDirectories, "Whether this media library should copy the downloaded metadata into the media library content locations, or not.");
        ResponseParam<LibraryResponse>(r => r.CreatedOnUtc, "The date and time when the entity was created.");
        ResponseParam<LibraryResponse>(r => r.UpdatedOnUtc, "The date and time when the entity was last updated.");

        Response(200, "The media library was successfuly updated.", example:
            new LibraryResponse(
                Id: Guid.NewGuid(),
                UserId: Guid.NewGuid(),
                Title: "TV Shows",
                LibraryType: LibraryType.TvShow,
                ContentLocations: ["/media/tv shows/drama/", "/media/tv shows/SCI-FI/"],
                CoverImage: "/media/myPoster.jpg",
                IsEnabled: true,
                IsLocked: false,
                DownloadMedatadaFromWeb: true,
                SaveMetadataInMediaDirectories: false,
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
                instance = "/api/v1/libraries",
                errors = new Dictionary<string, string[]>
                {
                    {
                        "General.Validation", new[]
                        {
                            "UserIdCannotBeEmpty",
                            "LibraryTypeCannotBeNull",
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
