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
[DebuggerDisplay("Title: {Title}")]
public record AddLibraryRequest(
    string? Title,
    string? LibraryType,
    string[]? ContentLocations
);
