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
/// <param name="DownloadMedatadaFromWeb">Whether this media library should update the metadata of its elements from the web, or not. Optional.</param>
/// <param name="SaveMetadataInMediaDirectories">Whether this media library should copy the downloaded metadata into the media library content locations, or not. Optional.</param>
[DebuggerDisplay("Title: {Title}")]
public record AddLibraryRequest(
    string? Title,
    string? LibraryType,
    string[]? ContentLocations,
    string? CoverImage,
    bool IsEnabled,
    bool IsLocked,
    bool DownloadMedatadaFromWeb,
    bool SaveMetadataInMediaDirectories
);
