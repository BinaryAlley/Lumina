#region ========================================================================= USING =====================================================================================
using ErrorOr;
using Lumina.Contracts.Responses.MediaLibrary.Management;
using Mediator;
using System;
using System.Diagnostics;
#endregion

namespace Lumina.Application.Core.MediaLibrary.Management.Commands.UpdateLibrary;

/// <summary>
/// Command for updating a media library.
/// </summary>
/// <param name="Id">The unique identifier of the media library.</param>
/// <param name="OwnerId">The unique identifier of the user owning the media library.</param>
/// <param name="Title">The title of the media library.</param>
/// <param name="LibraryType">The type of the media library.</param>
/// <param name="ContentLocations">The file system paths of the directories where the media library elements are located.</param>
/// <param name="CoverImage">The path of the image file used as the cover for the library.</param>
/// <param name="IsEnabled">Whether this media library is enabled or not. A disabled media library is never shown or changed.</param>
/// <param name="IsLocked">Whether this media library is locked or not. A locked media library is displayed, but is never changed or updated.</param>
/// <param name="DownloadMedatadaFromWeb">Whether this media library should update the metadata of its elements from the web, or not.</param>
/// <param name="SaveMetadataInMediaDirectories">Whether this media library should copy the downloaded metadata into the media library content locations, or not.</param>
[DebuggerDisplay("Title: {Title}")]
public record UpdateLibraryCommand(
    Guid Id,
    Guid OwnerId,
    string? Title,
    string? LibraryType,
    string[]? ContentLocations,
    string? CoverImage,
    bool IsEnabled,
    bool IsLocked,
    bool DownloadMedatadaFromWeb,
    bool SaveMetadataInMediaDirectories
) : IRequest<ErrorOr<LibraryResponse>>;
