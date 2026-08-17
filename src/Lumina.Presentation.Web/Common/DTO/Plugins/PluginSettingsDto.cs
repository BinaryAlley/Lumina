#region ========================================================================= USING =====================================================================================
using System;
using System.Collections.Generic;
using System.Diagnostics;
#endregion

namespace Lumina.Presentation.Web.Common.DTO.Plugins;

/// <summary>
/// Data transfer object for the settings of a plugin and their schema.
/// </summary>
[DebuggerDisplay("PluginId: {PluginId}")]
public class PluginSettingsDto
{
    /// <summary>
    /// Gets or sets the unique identifier of the plugin.
    /// </summary>
    public Guid PluginId { get; set; }

    /// <summary>
    /// Gets or sets the schema of the plugin settings.
    /// </summary>
    public List<PluginSettingDescriptorDto> Schema { get; set; } = [];

    /// <summary>
    /// Gets or sets the current values of the plugin settings.
    /// </summary>
    public Dictionary<string, string>? Settings { get; set; }
}
