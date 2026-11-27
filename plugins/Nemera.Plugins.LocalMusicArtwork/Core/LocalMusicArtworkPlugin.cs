#region ========================================================================= USING =====================================================================================
using Lumina.Domain.SharedKernel.Common.Enums.Plugins;
using Lumina.Plugins.Contracts.Common.Models.DTO.Settings;
using Lumina.Plugins.Contracts.Core.Plugins;
using Lumina.Plugins.LocalMusicArtwork.Common.DependencyInjection;
using Lumina.Plugins.LocalMusicArtwork.Core.Settings;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
#endregion

namespace Lumina.Plugins.LocalMusicArtwork.Core;

/// <summary>
/// Plugin that provides the artwork of music artists and albums from the images stored in the folders of a local music library.
/// </summary>
public sealed class LocalMusicArtworkPlugin : IPlugin, IPluginServiceRegistrator
{
    /// <summary>
    /// The unique identifier of the plugin.
    /// </summary>
    public static readonly Guid s_pluginId = new("c9f1a7d3-2b64-4e18-8f05-7a3d9c1b6e42");

    /// <summary>
    /// Gets the unique identifier of the plugin.
    /// </summary>
    public Guid Id => s_pluginId;

    /// <summary>
    /// Gets the display name of the plugin.
    /// </summary>
    public string Name => "Local Music Artwork";

    /// <summary>
    /// Gets the author of the plugin.
    /// </summary>
    public string Author => "Lumina";

    /// <summary>
    /// Gets the version of the plugin.
    /// </summary>
    public Version Version => new(1, 0, 0);

    /// <summary>
    /// Gets the description of the plugin.
    /// </summary>
    public string Description => "Reads the artwork of the artists and the albums from the images stored in the folders of a local music library.";

    /// <summary>
    /// Gets the settings schema of the plugin.
    /// </summary>
    /// <returns>The settings descriptors of the plugin.</returns>
    public IReadOnlyList<PluginSettingDescriptorDto> GetSettingsSchema()
    {
        return
        [
            new PluginSettingDescriptorDto(
                Key: LocalMusicArtworkSettingsKeys.SHOULD_EXTRACT_EMBEDDED_COVER,
                Label: "Extract embedded album cover image from audio files when it is not already present on disk",
                Type: PluginSettingType.Boolean,
                DefaultValue: "true")
        ];
    }

    /// <summary>
    /// Registers the services required by the plugin.
    /// </summary>
    /// <param name="services">The service collection to register the services into.</param>
    public void RegisterServices(IServiceCollection services)
    {
        services.AddLocalMusicArtworkProviders(pluginId: Id);
    }
}
