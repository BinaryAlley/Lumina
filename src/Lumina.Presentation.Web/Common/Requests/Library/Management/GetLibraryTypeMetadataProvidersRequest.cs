#region ========================================================================= USING =====================================================================================
using System.Diagnostics;
#endregion

namespace Lumina.Presentation.Web.Common.Requests.Library.Management;

/// <summary>
/// Represents a request for retrieving the metadata providers available for a media library type.
/// </summary>
/// <param name="LibraryType">The media library type whose available metadata providers are retrieved. Required.</param>
[DebuggerDisplay("LibraryType: {LibraryType}")]
public record GetLibraryTypeMetadataProvidersRequest(
    string LibraryType
);
