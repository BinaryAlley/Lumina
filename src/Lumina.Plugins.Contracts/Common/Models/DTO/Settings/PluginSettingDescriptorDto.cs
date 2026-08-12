#region ========================================================================= USING =====================================================================================
using Lumina.Domain.SharedKernel.Common.Enums.Plugins;
using System.Collections.Generic;
using System.Diagnostics;
#endregion

namespace Lumina.Plugins.Contracts.Common.Models.DTO.Settings;

/// <summary>
/// Data transfer object describing a single setting of a plugin, used by the host to render the plugin settings page.
/// </summary>
/// <param name="Key">The unique key of the setting, used to persist and retrieve its value.</param>
/// <param name="Label">The display label of the setting.</param>
/// <param name="Type">The type of the setting.</param>
/// <param name="DefaultValue">The optional default value of the setting, serialized as a string.</param>
/// <param name="AllowedValues">The list of allowed values, when the setting is a selection.</param>
[DebuggerDisplay("Key: {Key}")]
public sealed record PluginSettingDescriptorDto(
    string Key,
    string Label,
    PluginSettingType Type,
    string? DefaultValue = null,
    IReadOnlyList<string>? AllowedValues = null
);
