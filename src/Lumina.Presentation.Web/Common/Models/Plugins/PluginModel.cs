#region ========================================================================= USING =====================================================================================
using Lumina.Presentation.Web.Common.Enums.Plugins;
using System;
using System.Collections.Generic;
using System.Diagnostics;
#endregion

namespace Lumina.Presentation.Web.Common.Models.Plugins;

/// <summary>
/// Represents a detected plugin.
/// </summary>
[DebuggerDisplay("Name: {Name}")]
public class PluginModel
{
    /// <summary>
    /// Gets or sets the unique identifier of the plugin.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the display name of the plugin.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the author of the plugin.
    /// </summary>
    public string Author { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the version of the plugin.
    /// </summary>
    public string Version { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the description of the plugin.
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the load status of the plugin.
    /// </summary>
    public PluginLoadStatus LoadStatus { get; set; }

    /// <summary>
    /// Gets or sets the error message when the plugin failed to load.
    /// </summary>
    public string? LoadError { get; set; }

    /// <summary>
    /// Gets or sets the settings of the plugin.
    /// </summary>
    public Dictionary<string, string>? Settings { get; set; }
}
