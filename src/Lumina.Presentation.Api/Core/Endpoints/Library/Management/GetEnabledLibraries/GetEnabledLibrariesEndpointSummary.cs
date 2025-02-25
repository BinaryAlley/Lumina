#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Contracts.Responses.MediaLibrary.Management;
using Lumina.Domain.Common.Enums.MediaLibrary;
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
                DownloadMedatadaFromWeb: true,
                SaveMetadataInMediaDirectories: false,
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
                DownloadMedatadaFromWeb: true,
                SaveMetadataInMediaDirectories: true,
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
                    detail = "You are not authorized",
                    instance = "/api/v1/libraries/enabled"
                },
                new
                {
                    type = "https://tools.ietf.org/html/rfc7235#section-3.1",
                    status = 401,
                    title = "Unauthorized",
                    detail = "Invalid token: The token expired at '01/01/2024 01:00:00'",
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
