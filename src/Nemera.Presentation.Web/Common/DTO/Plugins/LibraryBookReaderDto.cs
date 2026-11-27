#region ========================================================================= USING =====================================================================================
using System;
using System.Collections.Generic;
using System.Diagnostics;
#endregion

namespace Lumina.Presentation.Web.Common.DTO.Plugins;

/// <summary>
/// Data transfer object for a book reader configured for a media library.
/// </summary>
[DebuggerDisplay("Name: {Name}")]
public class LibraryBookReaderDto
{
    /// <summary>
    /// Gets or sets the unique identifier of the plugin providing the book reader.
    /// </summary>
    public Guid PluginId { get; set; }

    /// <summary>
    /// Gets or sets the display name of the book reader.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the file extensions supported by the book reader.
    /// </summary>
    public List<string> SupportedExtensions { get; set; } = [];

    /// <summary>
    /// Gets or sets whether the book reader is enabled for the media library.
    /// </summary>
    public bool IsEnabled { get; set; }
}
