#region ========================================================================= USING =====================================================================================
using Lumina.Contracts.Enums.MediaLibrary;
using System;
using System.Collections.Generic;
using System.Diagnostics;
#endregion

namespace Lumina.Contracts.Responses.MediaLibrary.Management;

/// <summary>
/// Represents a media library response.
/// </summary>
/// <param name="Id">The id of the media library.</param>
/// <param name="UserId">The Id of the user owning the media library.</param>
/// <param name="Title">The title of the media library.</param>
/// <param name="LibraryType">The type of the media library.</param>
/// <param name="ContentLocations">The file system paths of the directories where the media library elements are located.</param>
/// <param name="Created">The date and time when the library was created.</param>
/// <param name="Updated">The optional date and time when the library was updated.</param>
[DebuggerDisplay("Title: {Title}")]
public record LibraryResponse(
    Guid Id,
    Guid UserId,
    string Title,
    LibraryType LibraryType,
    List<string> ContentLocations,
    DateTime Created,
    DateTime? Updated
);
