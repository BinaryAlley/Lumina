#region ========================================================================= USING =====================================================================================
using System;
using System.Diagnostics;
#endregion

namespace Lumina.Contracts.Requests.Plugins;

/// <summary>
/// Represents a request to enable or disable a book reader for a media library.
/// </summary>
/// <param name="LibraryId">The Id of the media library whose book reader is enabled or disabled. Required.</param>
/// <param name="PluginId">The unique identifier of the plugin providing the book reader. Required.</param>
/// <param name="IsEnabled">Whether the book reader should be enabled for the media library. Required.</param>
[DebuggerDisplay("LibraryId: {LibraryId}, PluginId: {PluginId}")]
public sealed record SetLibraryBookReaderEnabledRequest(
    Guid LibraryId,
    Guid PluginId,
    bool IsEnabled
);
