#region ========================================================================= USING =====================================================================================
using System;
using System.Collections.Generic;
#endregion

namespace Lumina.Presentation.Web.Common.Models.Plugins;

/// <summary>
/// Represents the settings of a plugin and their schema.
/// </summary>
public class PluginSettingsModel
{
    /// <summary>
    /// Gets or sets the unique identifier of the plugin.
    /// </summary>
    public Guid PluginId { get; set; }

    /// <summary>
    /// Gets or sets the schema of the plugin settings.
    /// </summary>
    public List<PluginSettingDescriptorModel> Schema { get; set; } = [];

    /// <summary>
    /// Gets or sets the current values of the plugin settings.
    /// </summary>
    public Dictionary<string, string>? Settings { get; set; }
}
