#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.Plugins;
using Lumina.Contracts.Responses.Plugins;
using System.Collections.Generic;
using System.Text.Json;
#endregion

namespace Lumina.Application.Common.Mapping.Plugins;

/// <summary>
/// Extension methods for converting <see cref="PluginEntity"/>.
/// </summary>
public static class PluginEntityMapping
{
    /// <summary>
    /// Converts <paramref name="pluginEntity"/> to <see cref="PluginResponse"/>.
    /// </summary>
    /// <param name="pluginEntity">The repository entity to be converted.</param>
    /// <returns>The converted response.</returns>
    public static PluginResponse ToResponse(this PluginEntity pluginEntity)
    {
        return new PluginResponse(
            pluginEntity.Id,
            pluginEntity.Name,
            pluginEntity.Author,
            pluginEntity.Version,
            pluginEntity.Description,
            pluginEntity.LoadStatus,
            pluginEntity.LoadError,
            pluginEntity.ToSettings()
        );
    }

    /// <summary>
    /// Deserializes the settings of <paramref name="pluginEntity"/>.
    /// </summary>
    /// <param name="pluginEntity">The repository entity whose settings are deserialized.</param>
    /// <returns>The deserialized settings, or <see langword="null"/> when no settings are stored.</returns>
    public static IReadOnlyDictionary<string, string>? ToSettings(this PluginEntity pluginEntity)
    {
        if (pluginEntity.SettingsJson is null)
            return null;
        return JsonSerializer.Deserialize<Dictionary<string, string>>(pluginEntity.SettingsJson);
    }
}
