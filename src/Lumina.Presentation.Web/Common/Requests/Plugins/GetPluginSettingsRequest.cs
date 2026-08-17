#region ========================================================================= USING =====================================================================================
using System;
using System.Diagnostics;
#endregion

namespace Lumina.Presentation.Web.Common.Requests.Plugins;

/// <summary>
/// Represents a request to get the settings of a plugin.
/// </summary>
/// <param name="PluginId">The unique identifier of the plugin. Required.</param>
[DebuggerDisplay("PluginId: {PluginId}")]
public record GetPluginSettingsRequest(
    Guid PluginId
);
