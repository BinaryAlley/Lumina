#region ========================================================================= USING =====================================================================================
using System;
using System.Collections.Generic;
using System.Diagnostics;
#endregion

namespace Lumina.Contracts.Responses.Plugins;

/// <summary>
/// Represents a book reader configured for a media library.
/// </summary>
/// <param name="PluginId">The unique identifier of the plugin providing the book reader.</param>
/// <param name="Name">The display name of the book reader.</param>
/// <param name="SupportedExtensions">The file extensions supported by the book reader.</param>
/// <param name="IsEnabled">Whether the book reader is enabled for the media library.</param>
[DebuggerDisplay("Name: {Name}")]
public sealed record LibraryBookReaderResponse(
    Guid PluginId,
    string Name,
    IReadOnlyList<string> SupportedExtensions,
    bool IsEnabled
);
