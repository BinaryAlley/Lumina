#region ========================================================================= USING =====================================================================================
using System.Diagnostics;
#endregion

namespace Lumina.Contracts.Requests.MediaLibrary.Management;

/// <summary>
/// Represents a request to add a media library.
/// </summary>
/// <param name="Title">The title of the media library. Required.</param>
/// <param name="LibraryType">The type of the media library. Required.</param>
/// <param name="ContentLocations">The file system paths of the directories where the media library elements are located. Required.</param>
/// <param name="CoverImage">The path of the image file used as the cover for the library. Optional.</param>
/// <param name="IsEnabled">Whether this media library is enabled or not. A disabled media library is never shown or changed. Optional.</param>
/// <param name="IsLocked">Whether this media library is locked or not. A locked media library is displayed, but is never changed or updated. Optional.</param>
/// <param name="CanDownloadMetadataFromWeb">Whether this media library should update the metadata of its elements from the web, or not. Optional.</param>
/// <param name="ShouldSaveMetadataInMediaDirectories">Whether this media library should copy the downloaded metadata into the media library content locations, or not. Optional.</param>
/// <param name="ShouldSkipUnchangedDirectoriesDuringScan">Whether this media library should skip the directories whose contents have not changed since the last scan, during the scan, or not. Optional.</param>
/// <param name="IsContentOnSsd">Whether the content of this media library is stored on a solid state drive, in which case the file discovery and content hashing stages read the files in parallel. Optional.</param>
/// <param name="ShouldTrackConsumptionProgress">Whether the consumption progress of the media library items should be tracked, or not. Optional, the default of the library type is used when absent.</param>
/// <param name="CompletionThresholdPercentage">The percentage of a media library item that must be consumed for its session to be considered completed. Optional, the default of the library type is used when absent.</param>
/// <param name="PathTemplateParts">The ordered parts of the template describing the structure of the media library on disk. Optional, the ideal structure of the library type is used when absent.</param>
[DebuggerDisplay("Title: {Title}")]
public record AddLibraryRequest(
    string? Title,
    string? LibraryType,
    string[]? ContentLocations,
    string? CoverImage,
    bool IsEnabled,
    bool IsLocked,
    bool CanDownloadMetadataFromWeb,
    bool ShouldSaveMetadataInMediaDirectories,
    bool ShouldSkipUnchangedDirectoriesDuringScan,
    bool IsContentOnSsd,
    bool? ShouldTrackConsumptionProgress,
    int? CompletionThresholdPercentage,
    LibraryPathTemplatePartRequest[]? PathTemplateParts
);
