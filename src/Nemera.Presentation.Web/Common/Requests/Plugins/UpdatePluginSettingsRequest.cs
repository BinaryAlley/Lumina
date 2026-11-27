#region ========================================================================= USING =====================================================================================
using System;
using System.Collections.Generic;
using System.Diagnostics;
#endregion

namespace Lumina.Presentation.Web.Common.Requests.Plugins;

/// <summary>
/// Represents a request to update the settings of a plugin.
/// </summary>
/// <param name="PluginId">The unique identifier of the plugin. Required.</param>
/// <param name="Settings">The settings of the plugin. Optional.</param>
[DebuggerDisplay("PluginId: {PluginId}")]
public record UpdatePluginSettingsRequest(
    Guid PluginId,
    Dictionary<string, string>? Settings
);
