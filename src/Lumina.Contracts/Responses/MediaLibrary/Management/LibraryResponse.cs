#region ========================================================================= USING =====================================================================================
using Lumina.Domain.SharedKernel.Common.Enums.MediaLibrary;
using System;
using System.Collections.Generic;
using System.Diagnostics;
#endregion

namespace Lumina.Contracts.Responses.MediaLibrary.Management;

/// <summary>
/// Represents a media library response.
/// </summary>
/// <param name="Id">The unique identifier of the media library.</param>
/// <param name="UserId">The unique identifier of the user owning the media library.</param>
/// <param name="Title">The title of the media library.</param>
/// <param name="LibraryType">The type of the media library.</param>
/// <param name="ContentLocations">The file system paths of the directories where the media library elements are located.</param>
/// <param name="CoverImage">The path of the image file used as the cover for the library.</param>
/// <param name="IsEnabled">Whether the media library is enabled or not. A disabled media library is never shown or changed.</param>
/// <param name="IsLocked">Whether the media library is locked or not. A locked media library is displayed, but is never changed or updated.</param>
/// <param name="CanDownloadMetadataFromWeb">Whether the media library should update the metadata of its elements from the web, or not.</param>
/// <param name="ShouldSaveMetadataInMediaDirectories">Whether the media library should copy the downloaded metadata into the media library content locations, or not.</param>
/// <param name="ShouldSkipUnchangedDirectoriesDuringScan">Whether the media library should skip the directories whose contents have not changed since the last scan, during the scan.</param>
/// <param name="CreatedOnUtc">The date and time when the library was created.</param>
/// <param name="UpdatedOnUtc">The optional date and time when the library was updated.</param>
[DebuggerDisplay("Title: {Title}")]
public record LibraryResponse(
    Guid Id,
    Guid UserId,
    string Title,
    LibraryType LibraryType,
    List<string> ContentLocations,
    string? CoverImage,
    bool IsEnabled,
    bool IsLocked,
    bool CanDownloadMetadataFromWeb,
    bool ShouldSaveMetadataInMediaDirectories,
    bool ShouldSkipUnchangedDirectoriesDuringScan,
    DateTime CreatedOnUtc,
    DateTime? UpdatedOnUtc
);
