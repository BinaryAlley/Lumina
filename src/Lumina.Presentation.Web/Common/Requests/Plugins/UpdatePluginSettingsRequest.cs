#region ========================================================================= USING =====================================================================================
using System;
using System.Collections.Generic;
using System.Diagnostics;
#endregion

namespace Lumina.Presentation.Web.Common.Requests.Plugins;

/// <summary>
/// Represents a request to update the settings of a plugin.
/// </summary>
[DebuggerDisplay("PluginId: {PluginId}")]
public class UpdatePluginSettingsRequest
{
    /// <summary>
    /// Gets or sets the unique identifier of the plugin.
    /// </summary>
    public Guid PluginId { get; set; }

    /// <summary>
    /// Gets or sets the settings of the plugin.
    /// </summary>
    public Dictionary<string, string>? Settings { get; set; }
}
