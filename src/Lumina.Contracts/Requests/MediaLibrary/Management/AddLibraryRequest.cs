#region ========================================================================= USING =====================================================================================
using Lumina.Contracts.Enums.MediaLibrary;
using System;
using System.Diagnostics;
#endregion

namespace Lumina.Contracts.Requests.MediaLibrary.Management;

/// <summary>
/// Represents a request to add a media library.
/// </summary>
/// <param name="UserId">The Id of the user owning the media library.</param>
/// <param name="Title">The title of the media library.</param>
/// <param name="LibraryType">The type of the media library.</param>
/// <param name="ContentLocations">The file system paths of the directories where the media library elements are located.</param>
[DebuggerDisplay("Title: {Title}")]
public record AddLibraryRequest(
    Guid UserId,
    string? Title,
    LibraryType? LibraryType,
    string[]? ContentLocations
);
