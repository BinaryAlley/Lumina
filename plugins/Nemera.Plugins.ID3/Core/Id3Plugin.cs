#region ========================================================================= USING =====================================================================================
using Lumina.Plugins.Contracts.Common.Models.DTO.Settings;
using Lumina.Plugins.Contracts.Core.Plugins;
using Lumina.Plugins.ID3.Common.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
#endregion

namespace Lumina.Plugins.ID3.Core;

/// <summary>
/// Plugin that populates music metadata exclusively from the embedded tags of the audio files, without accessing the web.
/// </summary>
public sealed class Id3Plugin : IPlugin, IPluginServiceRegistrator
{
    /// <summary>
    /// The unique identifier of the plugin.
    /// </summary>
    public static readonly Guid s_pluginId = new("a3f1c8d2-6b47-4e9a-8c15-2d7f9e4b6a03");

    /// <summary>
    /// Gets the unique identifier of the plugin.
    /// </summary>
    public Guid Id => s_pluginId;

    /// <summary>
    /// Gets the display name of the plugin.
    /// </summary>
    public string Name => "ID3 Metadata";

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
    public string Description => "Populates artist, album and track metadata from the embedded tags of the audio files, without accessing the web.";

    /// <summary>
    /// Gets the settings schema of the plugin.
    /// </summary>
    /// <returns>The settings descriptors of the plugin.</returns>
    public IReadOnlyList<PluginSettingDescriptorDto> GetSettingsSchema()
    {
        return [];
    }

    /// <summary>
    /// Registers the services required by the plugin.
    /// </summary>
    /// <param name="services">The service collection to register the services into.</param>
    public void RegisterServices(IServiceCollection services)
    {
        services.AddId3MetadataProviders(pluginId: Id);
    }
}
