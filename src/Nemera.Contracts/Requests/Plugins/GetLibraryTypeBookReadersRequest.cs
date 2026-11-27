#region ========================================================================= USING =====================================================================================
using System.Diagnostics;
#endregion

namespace Nemera.Contracts.Requests.Plugins;

/// <summary>
/// Represents a request to get the book readers available for a media library type.
/// </summary>
/// <param name="LibraryType">The media library type whose available book readers are retrieved. Required.</param>
[DebuggerDisplay("LibraryType: {LibraryType}")]
public sealed record GetLibraryTypeBookReadersRequest(
    string? LibraryType
);
