#region ========================================================================= USING =====================================================================================
using Lumina.Plugins.Contracts.Common.Models.DTO.Settings;
using System;
using System.Collections.Generic;
#endregion

namespace Lumina.Plugins.Contracts.Core.Plugins;

/// <summary>
/// Contract for a Lumina plugin.
/// </summary>
public interface IPlugin
{
    /// <summary>
    /// Gets the stable unique identifier of the plugin, used to persist its state.
    /// </summary>
    Guid Id { get; }

    /// <summary>
    /// Gets the display name of the plugin.
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Gets the author of the plugin.
    /// </summary>
    string Author { get; }

    /// <summary>
    /// Gets the version of the plugin.
    /// </summary>
    Version Version { get; }

    /// <summary>
    /// Gets the description of the plugin.
    /// </summary>
    string Description { get; }

    /// <summary>
    /// Gets the settings schema of the plugin, used by the host to render the plugin settings page.
    /// </summary>
    /// <returns>The collection of setting descriptors declared by the plugin.</returns>
    IReadOnlyList<PluginSettingDescriptorDto> GetSettingsSchema();
}
