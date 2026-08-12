#region ========================================================================= USING =====================================================================================
using System;
using System.Collections.Generic;
using System.Diagnostics;
#endregion

namespace Lumina.Contracts.Requests.Plugins;

/// <summary>
/// Represents a request to update the settings of a plugin.
/// </summary>
/// <param name="PluginId">The unique identifier of the plugin. Required.</param>
/// <param name="Settings">The settings of the plugin. Required.</param>
[DebuggerDisplay("PluginId: {PluginId}")]
public sealed record UpdatePluginSettingsRequest(
    Guid PluginId,
    IReadOnlyDictionary<string, string>? Settings
);
