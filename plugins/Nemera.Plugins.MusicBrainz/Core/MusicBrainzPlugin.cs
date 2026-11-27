#region ========================================================================= USING =====================================================================================
using Lumina.Domain.SharedKernel.Common.Enums.Plugins;
using Lumina.Plugins.Contracts.Common.Models.DTO.Settings;
using Lumina.Plugins.Contracts.Core.Plugins;
using Lumina.Plugins.MusicBrainz.Common.DependencyInjection;
using Lumina.Plugins.MusicBrainz.Core.Settings;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
#endregion

namespace Lumina.Plugins.MusicBrainz.Core;

/// <summary>
/// Plugin that provides music metadata retrieval from MusicBrainz.
/// </summary>
public sealed class MusicBrainzPlugin : IPlugin, IPluginServiceRegistrator
{
    /// <summary>
    /// The unique identifier of the plugin.
    /// </summary>
    public static readonly Guid s_pluginId = new("f4c1a7de-3b52-4e6a-9d21-8c7e5b04a1f3");

    /// <summary>
    /// Gets the unique identifier of the plugin.
    /// </summary>
    public Guid Id => s_pluginId;

    /// <summary>
    /// Gets the display name of the plugin.
    /// </summary>
    public string Name => "MusicBrainz Metadata";

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
    public string Description => "Retrieves artist, album and track metadata from MusicBrainz.";

    /// <summary>
    /// Gets the settings schema of the plugin.
    /// </summary>
    /// <returns>The settings descriptors of the plugin.</returns>
    public IReadOnlyList<PluginSettingDescriptorDto> GetSettingsSchema()
    {
        return
        [
            new PluginSettingDescriptorDto(
                Key: MusicBrainzSettingsKeys.BASE_URL,
                Label: "Base URL",
                Type: PluginSettingType.Text,
                DefaultValue: "https://musicbrainz.org/ws/2/"),
            new PluginSettingDescriptorDto(
                Key: MusicBrainzSettingsKeys.DOES_ALLOW_PRIVATE_BASE_URL,
                Label: "Allow LAN/Private Base URL",
                Type: PluginSettingType.Boolean,
                DefaultValue: "false"),
            new PluginSettingDescriptorDto(
                Key: MusicBrainzSettingsKeys.CONTACT_EMAIL,
                Label: "Contact Email",
                Type: PluginSettingType.Text),
            new PluginSettingDescriptorDto(
                Key: MusicBrainzSettingsKeys.SEARCH_RESULT_LIMIT,
                Label: "Search Result Limit",
                Type: PluginSettingType.Number,
                DefaultValue: "10"),
            new PluginSettingDescriptorDto(
                Key: MusicBrainzSettingsKeys.RELEASE_LOOKUP_LIMIT,
                Label: "Release Lookup Limit",
                Type: PluginSettingType.Number,
                DefaultValue: "25"),
            new PluginSettingDescriptorDto(
                Key: MusicBrainzSettingsKeys.MINIMUM_REQUEST_INTERVAL_SECONDS,
                Label: "Minimum Request Interval (seconds)",
                Type: PluginSettingType.Number,
                DefaultValue: "1.0")
        ];
    }

    /// <summary>
    /// Registers the services required by the plugin.
    /// </summary>
    /// <param name="services">The service collection to register the services into.</param>
    public void RegisterServices(IServiceCollection services)
    {
        services.AddMusicBrainzMetadataProviders(pluginId: Id);
    }
}
