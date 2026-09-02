#region ========================================================================= USING =====================================================================================
using Lumina.Plugins.Contracts.Common.Models.DTO.Settings;
using Lumina.Plugins.Contracts.Core.Plugins;
using Lumina.Plugins.Epub.Common.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
#endregion

namespace Lumina.Plugins.Epub.Core;

/// <summary>
/// Plugin that decodes EPUB books into a normalized reading document.
/// </summary>
public sealed class EpubPlugin : IPlugin, IPluginServiceRegistrator
{
    /// <summary>
    /// The unique identifier of the plugin.
    /// </summary>
    public static readonly Guid s_pluginId = new("b1e8a8e0-4f3a-4d2c-9a1e-6d3f8a9b2c3d");

    /// <summary>
    /// Gets the unique identifier of the plugin.
    /// </summary>
    public Guid Id => s_pluginId;

    /// <summary>
    /// Gets the display name of the plugin.
    /// </summary>
    public string Name => "EPUB Reader";

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
    public string Description => "Allows reading of EPUB format books.";

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
        services.AddEpubReader(pluginId: Id);
    }
}
