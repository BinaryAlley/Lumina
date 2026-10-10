#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Contracts.Responses.MediaLibrary.Management;
using Lumina.Domain.SharedKernel.Common.Enums.MediaLibrary;
using System;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Presentation.Api.Core.Endpoints.Library.Management.GetEnabledLibraries;

/// <summary>
/// Class used for providing a textual description for the <see cref="GetEnabledLibrariesEndpoint"/> API endpoint, for OpenAPI.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetEnabledLibrariesEndpointSummary : Summary<GetEnabledLibrariesEndpoint, EmptyRequest>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GetEnabledLibrariesEndpointSummary"/> class.
    /// </summary>
    public GetEnabledLibrariesEndpointSummary()
    {
        Summary = "Retrieves the list of enabled media libraries.";
        Description = "Retrieves the entire list of media libraries that are marked as enabled, if the user making the request is an Admin, or just the library owned by them, for regular users.";

        ResponseParam<LibraryResponse>(r => r.Id, "The unique identifier of the entity.");
        ResponseParam<LibraryResponse>(r => r.UserId, "The unique identifier of the user owning the media library.");
        ResponseParam<LibraryResponse>(r => r.Title, "The title of the media library.");
        ResponseParam<LibraryResponse>(r => r.LibraryType, "The type of the media library.");
        ResponseParam<LibraryResponse>(r => r.ContentLocations, "The file system paths of the directories where the media library elements are located.");
        ResponseParam<LibraryResponse>(r => r.CoverImage, "The path of the image file used as the cover for the library.");
        ResponseParam<LibraryResponse>(r => r.IsEnabled, "Whether this media library is enabled or not. A disabled media library is never shown or changed.");
        ResponseParam<LibraryResponse>(r => r.IsLocked, "Whether this media library is locked or not. A locked media library is displayed, but is never changed or updated.");
        ResponseParam<LibraryResponse>(r => r.CanDownloadMetadataFromWeb, "Whether this media library should update the metadata of its elements from the web, or not.");
        ResponseParam<LibraryResponse>(r => r.ShouldSaveMetadataInMediaDirectories, "Whether this media library should copy the downloaded metadata into the media library content locations, or not.");
        ResponseParam<LibraryResponse>(r => r.ShouldSkipUnchangedDirectoriesDuringScan, "Whether this media library should skip the directories whose contents have not changed since the last scan, during the scan, or not.");
        ResponseParam<LibraryResponse>(r => r.PathTemplateParts, "The ordered parts of the template describing the structure of the media library on disk.");
        ResponseParam<LibraryResponse>(r => r.CreatedOnUtc, "The date and time when the entity was created.");
        ResponseParam<LibraryResponse>(r => r.UpdatedOnUtc, "The date and time when the entity was last updated.");

        Response(200, "The media libraries are returned.",
            example: new LibraryResponse[] {
            new (
                Id: Guid.NewGuid(),
                UserId: Guid.NewGuid(),
                Title: "TV Shows",
                LibraryType: LibraryType.TvShow,
                ContentLocations: ["/media/tv shows/drama/", "/media/tv shows/SCI-FI/"],
                CoverImage: "/media/myTvShowPoster.jpg",
                IsEnabled: true,
                IsLocked: false,
                CanDownloadMetadataFromWeb: true,
                ShouldSaveMetadataInMediaDirectories: false,
                ShouldSkipUnchangedDirectoriesDuringScan: false,
                PathTemplateParts: [],
                CreatedOnUtc: DateTime.UtcNow,
                UpdatedOnUtc: default
            ),
            new (
                Id: Guid.NewGuid(),
                UserId: Guid.NewGuid(),
                Title: "Movies",
                LibraryType: LibraryType.TvShow,
                ContentLocations: ["/media/movies/"],
                CoverImage: "/media/myMoviePoster.jpg",
                IsEnabled: true,
                IsLocked: true,
                CanDownloadMetadataFromWeb: true,
                ShouldSaveMetadataInMediaDirectories: true,
                ShouldSkipUnchangedDirectoriesDuringScan: false,
                PathTemplateParts: [],
                CreatedOnUtc: DateTime.UtcNow,
                UpdatedOnUtc: default
            )
        });

        Response(401, "Authentication required.", "application/problem+json",
            example: new[]
            {
                new
                {
                    type = "https://tools.ietf.org/html/rfc7235#section-3.1",
                    status = 401,
                    title = "Unauthorized",
                    detail = "Authentication failed",
                    instance = "/api/v1/libraries/enabled"
                },
                new
                {
                    type = "https://tools.ietf.org/html/rfc7235#section-3.1",
                    status = 401,
                    title = "Unauthorized",
                    detail = "The token has expired",
                    instance = "/api/v1/libraries/enabled"
                },
                new
                {
                    type = "https://tools.ietf.org/html/rfc7235#section-3.1",
                    status = 401,
                    title = "Unauthorized",
                    detail = "The token is invalid",
                    instance = "/api/v1/libraries/enabled"
                }
            }
        );
    }
}
