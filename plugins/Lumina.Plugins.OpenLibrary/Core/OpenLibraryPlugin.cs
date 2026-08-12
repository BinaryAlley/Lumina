#region ========================================================================= USING =====================================================================================
using Lumina.Domain.SharedKernel.Common.Enums.Plugins;
using Lumina.Plugins.Contracts.Common.Models.DTO.Settings;
using Lumina.Plugins.Contracts.Core.Plugins;
using Lumina.Plugins.OpenLibrary.Common.DependencyInjection;
using Lumina.Plugins.OpenLibrary.Core.Settings;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
#endregion

namespace Lumina.Plugins.OpenLibrary.Core;

/// <summary>
/// Plugin that provides book metadata retrieval from Open Library.
/// </summary>
public sealed class OpenLibraryPlugin : IPlugin, IPluginServiceRegistrator
{
    /// <summary>
    /// The unique identifier of the plugin.
    /// </summary>
    public static readonly Guid s_pluginId = new("08b17802-7f9c-4c4b-9d7f-b507bbed3e58");

    /// <summary>
    /// Gets the unique identifier of the plugin.
    /// </summary>
    public Guid Id => s_pluginId;

    /// <summary>
    /// Gets the display name of the plugin.
    /// </summary>
    public string Name => "Open Library Metadata";

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
    public string Description => "Retrieves book and edition metadata from Open Library.";

    /// <summary>
    /// Gets the settings schema of the plugin.
    /// </summary>
    /// <returns>The settings descriptors of the plugin.</returns>
    public IReadOnlyList<PluginSettingDescriptorDto> GetSettingsSchema()
    {
        return
        [
            new PluginSettingDescriptorDto(
                Key: OpenLibrarySettingsKeys.CONTACT_EMAIL,
                Label: "Contact Email",
                Type: PluginSettingType.Text),
            new PluginSettingDescriptorDto(
                Key: OpenLibrarySettingsKeys.SEARCH_RESULT_LIMIT,
                Label: "Search Result Limit",
                Type: PluginSettingType.Number,
                DefaultValue: "10"),
            new PluginSettingDescriptorDto(
                Key: OpenLibrarySettingsKeys.WORK_EDITION_LIMIT,
                Label: "Work Edition Limit",
                Type: PluginSettingType.Number,
                DefaultValue: "50"),
            new PluginSettingDescriptorDto(
                Key: OpenLibrarySettingsKeys.MINIMUM_REQUEST_INTERVAL_SECONDS,
                Label: "Minimum Request Interval (seconds)",
                Type: PluginSettingType.Number,
                DefaultValue: "1.1")
        ];
    }

    /// <summary>
    /// Registers the services required by the plugin.
    /// </summary>
    /// <param name="services">The service collection to register the services into.</param>
    public void RegisterServices(IServiceCollection services)
    {
        services.AddOpenLibraryBookMetadataProvider(pluginId: Id);
    }
}
