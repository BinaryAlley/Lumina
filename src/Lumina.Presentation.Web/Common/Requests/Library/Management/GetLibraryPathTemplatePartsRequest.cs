#region ========================================================================= USING =====================================================================================
using System.Diagnostics;
#endregion

namespace Lumina.Presentation.Web.Common.Requests.Library.Management;

/// <summary>
/// Represents the request for retrieving the path parts available for a media library type.
/// </summary>
/// <param name="LibraryType">The media library type whose available path parts are retrieved. Required.</param>
[DebuggerDisplay("LibraryType: {LibraryType}")]
public record GetLibraryPathTemplatePartsRequest(
    string LibraryType
);
