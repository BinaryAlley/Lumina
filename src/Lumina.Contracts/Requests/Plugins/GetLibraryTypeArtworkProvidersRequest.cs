#region ========================================================================= USING =====================================================================================
using System.Diagnostics;
#endregion

namespace Lumina.Contracts.Requests.Plugins;

/// <summary>
/// Represents a request to get the artwork providers available for a media library type.
/// </summary>
/// <param name="LibraryType">The media library type whose available artwork providers are retrieved. Required.</param>
[DebuggerDisplay("LibraryType: {LibraryType}")]
public sealed record GetLibraryTypeArtworkProvidersRequest(
    string? LibraryType
);
