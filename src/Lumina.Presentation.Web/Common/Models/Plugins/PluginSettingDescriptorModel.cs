#region ========================================================================= USING =====================================================================================
using Lumina.Presentation.Web.Common.Enums.Plugins;
using System.Collections.Generic;
#endregion

namespace Lumina.Presentation.Web.Common.Models.Plugins;

/// <summary>
/// Describes a single setting of a plugin.
/// </summary>
public class PluginSettingDescriptorModel
{
    /// <summary>
    /// Gets or sets the unique key of the setting.
    /// </summary>
    public string Key { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the display label of the setting.
    /// </summary>
    public string Label { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the type of the setting.
    /// </summary>
    public PluginSettingType Type { get; set; }

    /// <summary>
    /// Gets or sets the default value of the setting.
    /// </summary>
    public string? DefaultValue { get; set; }

    /// <summary>
    /// Gets or sets the list of allowed values, when the setting is a selection.
    /// </summary>
    public IReadOnlyList<string>? AllowedValues { get; set; }
}
