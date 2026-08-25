#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.CQRS;
using System.Diagnostics;
#endregion

namespace Lumina.Application.Core.MediaLibrary.Management.Commands.AddLibrary;

/// <summary>
/// Command for adding a media library.
/// </summary>
/// <param name="Title">The title of the media library.</param>
/// <param name="LibraryType">The type of the media library.</param>
/// <param name="ContentLocations">The file system paths of the directories where the media library elements are located.</param>
/// <param name="CoverImage">The path of the image file used as the cover for the library.</param>
/// <param name="IsEnabled">Whether this media library is enabled or not. A disabled media library is never shown or changed.</param>
/// <param name="IsLocked">Whether this media library is locked or not. A locked media library is displayed, but is never changed or updated.</param>
/// <param name="CanDownloadMetadataFromWeb">Whether this media library should update the metadata of its elements from the web, or not.</param>
/// <param name="ShouldSaveMetadataInMediaDirectories">Whether this media library should copy the downloaded metadata into the media library content locations, or not.</param>
/// <param name="ShouldSkipUnchangedDirectoriesDuringScan">Whether this media library should skip the directories whose contents have not changed since the last scan, during the scan, or not.</param>
[DebuggerDisplay("Title: {Title}")]
public record AddLibraryCommand(
    string? Title,
    string? LibraryType,
    string[]? ContentLocations,
    string? CoverImage,
    bool IsEnabled,
    bool IsLocked,
    bool CanDownloadMetadataFromWeb,
    bool ShouldSaveMetadataInMediaDirectories,
    bool ShouldSkipUnchangedDirectoriesDuringScan
) : ICommand;
