#region ========================================================================= USING =====================================================================================
using Lumina.Domain.Common.Enums.MediaLibrary;
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
#endregion

namespace Lumina.Contracts.Requests.MediaLibrary.Management;

/// <summary>
/// Represents a request to update a media library.
/// </summary>
/// <param name="Id">The Id of the media library. Required.</param>
/// <param name="UserId">The Id of the user owning the media library. Required.</param>
/// <param name="Title">The title of the media library. Required.</param>
/// <param name="LibraryType">The type of the media library. Required.</param>
/// <param name="ContentLocations">The file system paths of the directories where the media library elements are located. Required.</param>
/// <param name="CoverImage">The path of the image file used as the cover for the library. Optional.</param>
[DebuggerDisplay("Title: {Title}")]
public record class UpdateLibraryRequest(
    Guid Id,
    Guid UserId,
    string? Title,
    string? LibraryType,
    string[]? ContentLocations,
    string? CoverImage
);
