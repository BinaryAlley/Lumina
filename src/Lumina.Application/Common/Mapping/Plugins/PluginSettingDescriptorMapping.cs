#region ========================================================================= USING =====================================================================================
using Lumina.Contracts.Responses.Plugins;
using Lumina.Plugins.Contracts.Common.Models.DTO.Settings;
using ContractsPluginSettingType = Lumina.Contracts.Responses.Plugins.PluginSettingType;
#endregion

namespace Lumina.Application.Common.Mapping.Plugins;

/// <summary>
/// Extension methods for converting <see cref="PluginSettingDescriptorDto"/>.
/// </summary>
public static class PluginSettingDescriptorMapping
{
    /// <summary>
    /// Converts <paramref name="descriptor"/> to <see cref="PluginSettingDescriptorResponse"/>.
    /// </summary>
    /// <param name="descriptor">The plugin contract setting descriptor to be converted.</param>
    /// <returns>The converted response.</returns>
    public static PluginSettingDescriptorResponse ToResponse(this PluginSettingDescriptorDto descriptor)
    {
        return new PluginSettingDescriptorResponse(
            descriptor.Key,
            descriptor.Label,
            (ContractsPluginSettingType)(int)descriptor.Type,
            descriptor.DefaultValue,
            descriptor.AllowedValues
        );
    }
}
