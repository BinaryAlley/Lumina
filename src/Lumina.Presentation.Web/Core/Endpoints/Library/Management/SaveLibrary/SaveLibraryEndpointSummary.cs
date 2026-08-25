#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Presentation.Web.Common.DTO.Common;
using Lumina.Presentation.Web.Common.DTO.Libraries;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Presentation.Web.Core.Endpoints.Library.Management.SaveLibrary;

/// <summary>
/// Class used for providing a textual description for the <see cref="SaveLibraryEndpoint"/> endpoint, for OpenAPI.
/// </summary>
[ExcludeFromCodeCoverage]
public class SaveLibraryEndpointSummary : Summary<SaveLibraryEndpoint, LibraryDto>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SaveLibraryEndpointSummary"/> class.
    /// </summary>
    public SaveLibraryEndpointSummary()
    {
        Summary = "Creates or updates a media library.";
        Description = "Creates or updates the media library provided in the request.";
        RequestParam(r => r.Id, "The unique identifier of the media library. Optional, when creating a new media library.");
        RequestParam(r => r.UserId, "The unique identifier of the user owning the media library. Optional.");
        RequestParam(r => r.Title, "The title of the media library. Required.");
        RequestParam(r => r.LibraryType, "The type of the media library. Required.");
        RequestParam(r => r.CoverImage, "The path of the image file used as the cover of the media library. Optional.");
        RequestParam(r => r.ContentLocations, "The collection of directories that contain the media library files. Required.");
        RequestParam(r => r.IsEnabled, "Whether the media library is enabled or not. Optional.");
        RequestParam(r => r.IsLocked, "Whether the media library is locked or not. Optional.");
        RequestParam(r => r.CanDownloadMetadataFromWeb, "Whether the metadata of the media library items should be downloaded from the web, or not. Optional.");
        RequestParam(r => r.ShouldSaveMetadataInMediaDirectories, "Whether the downloaded metadata should be copied into the media library content locations, or not. Optional.");
        RequestParam(r => r.ShouldSkipUnchangedDirectoriesDuringScan, "Whether the directories whose contents have not changed since the last scan should be skipped during the scan, or not. Optional.");

        ExampleRequest = new LibraryDto();

        Response(200, "The media library is created or updated.", example: new SuccessResponse<LibraryDto>(true, default));
    }
}
