#region ========================================================================= USING =====================================================================================
using System;
using System.Collections.Generic;
#endregion

namespace Lumina.Contracts.Responses.Plugins;

/// <summary>
/// Represents the settings of a plugin and their schema.
/// </summary>
/// <param name="PluginId">The unique identifier of the plugin.</param>
/// <param name="Schema">The schema of the plugin settings.</param>
/// <param name="Settings">The current values of the plugin settings.</param>
public sealed record PluginSettingsResponse(
    Guid PluginId,
    IReadOnlyList<PluginSettingDescriptorResponse> Schema,
    IReadOnlyDictionary<string, string>? Settings
);
