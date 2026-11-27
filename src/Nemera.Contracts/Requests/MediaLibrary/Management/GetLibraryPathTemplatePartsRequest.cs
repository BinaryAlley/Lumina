#region ========================================================================= USING =====================================================================================
using System.Diagnostics;
#endregion

namespace Lumina.Contracts.Requests.MediaLibrary.Management;

/// <summary>
/// Represents a request to get the path parts available for a media library type.
/// </summary>
/// <param name="LibraryType">The media library type whose available path parts are retrieved. Required.</param>
[DebuggerDisplay("LibraryType: {LibraryType}")]
public record GetLibraryPathTemplatePartsRequest(
    string? LibraryType
);
