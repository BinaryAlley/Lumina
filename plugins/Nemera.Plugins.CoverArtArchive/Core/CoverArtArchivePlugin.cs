#region ========================================================================= USING =====================================================================================
using Lumina.Domain.SharedKernel.Common.Enums.Plugins;
using Lumina.Plugins.Contracts.Common.Models.DTO.Settings;
using Lumina.Plugins.Contracts.Core.Plugins;
using Lumina.Plugins.CoverArtArchive.Common.DependencyInjection;
using Lumina.Plugins.CoverArtArchive.Core.Settings;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
#endregion

namespace Lumina.Plugins.CoverArtArchive.Core;

/// <summary>
/// Plugin that provides music album artwork retrieval from the Cover Art Archive.
/// </summary>
public sealed class CoverArtArchivePlugin : IPlugin, IPluginServiceRegistrator
{
    /// <summary>
    /// The unique identifier of the plugin.
    /// </summary>
    public static readonly Guid s_pluginId = new("b7e4c2a9-1f38-4d65-9a02-3e6b8f5c7d14");

    /// <summary>
    /// Gets the unique identifier of the plugin.
    /// </summary>
    public Guid Id => s_pluginId;

    /// <summary>
    /// Gets the display name of the plugin.
    /// </summary>
    public string Name => "Cover Art Archive";

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
    public string Description => "Retrieves the cover, back, booklet, medium and other artwork of music albums from the Cover Art Archive.";

    /// <summary>
    /// Gets the settings schema of the plugin.
    /// </summary>
    /// <returns>The settings descriptors of the plugin.</returns>
    public IReadOnlyList<PluginSettingDescriptorDto> GetSettingsSchema()
    {
        return
        [
            new PluginSettingDescriptorDto(
                Key: CoverArtArchiveSettingsKeys.CONTACT_EMAIL,
                Label: "Contact Email",
                Type: PluginSettingType.Text),
            new PluginSettingDescriptorDto(
                Key: CoverArtArchiveSettingsKeys.MINIMUM_REQUEST_INTERVAL_SECONDS,
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
        services.AddCoverArtArchiveArtworkProvider(pluginId: Id);
    }
}
