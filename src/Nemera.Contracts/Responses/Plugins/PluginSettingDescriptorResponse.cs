#region ========================================================================= USING =====================================================================================
using Lumina.Domain.SharedKernel.Common.Enums.Plugins;
using System.Collections.Generic;
#endregion

namespace Lumina.Contracts.Responses.Plugins;

/// <summary>
/// Describes a single setting of a plugin.
/// </summary>
/// <param name="Key">The unique key of the setting.</param>
/// <param name="Label">The display label of the setting.</param>
/// <param name="Type">The type of the setting.</param>
/// <param name="DefaultValue">The optional default value of the setting.</param>
/// <param name="AllowedValues">The list of allowed values, when the setting is a selection.</param>
public sealed record PluginSettingDescriptorResponse(
    string Key,
    string Label,
    PluginSettingType Type,
    string? DefaultValue,
    IReadOnlyList<string>? AllowedValues
);
