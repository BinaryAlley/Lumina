#region ========================================================================= USING =====================================================================================
using System.Diagnostics;
#endregion

namespace Lumina.Contracts.Requests.Plugins;

/// <summary>
/// Represents a request to get the metadata providers available for a media library type.
/// </summary>
/// <param name="LibraryType">The media library type whose available metadata providers are retrieved. Required.</param>
[DebuggerDisplay("LibraryType: {LibraryType}")]
public sealed record GetLibraryTypeMetadataProvidersRequest(
    string? LibraryType
);
