#region ========================================================================= USING =====================================================================================
using System;
using System.Diagnostics;
#endregion

namespace Lumina.Contracts.Requests.Plugins;

/// <summary>
/// Represents a request to get the settings of a plugin.
/// </summary>
/// <param name="PluginId">The unique identifier of the plugin. Required.</param>
[DebuggerDisplay("PluginId: {PluginId}")]
public sealed record GetPluginSettingsRequest(
    Guid PluginId
);
