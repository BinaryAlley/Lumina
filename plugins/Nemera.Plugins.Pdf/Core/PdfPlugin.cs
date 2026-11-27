#region ========================================================================= USING =====================================================================================
using Lumina.Plugins.Contracts.Common.Models.DTO.Settings;
using Lumina.Plugins.Contracts.Core.Plugins;
using Lumina.Plugins.Pdf.Common.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
#endregion

namespace Lumina.Plugins.Pdf.Core;

/// <summary>
/// Plugin that decodes PDF books into a normalized reading document.
/// </summary>
public sealed class PdfPlugin : IPlugin, IPluginServiceRegistrator
{
    /// <summary>
    /// The unique identifier of the plugin.
    /// </summary>
    public static readonly Guid s_pluginId = new("c2f9b8f0-5a4b-4e3d-8b2f-7e4a9c1d3e5f");

    /// <summary>
    /// Gets the unique identifier of the plugin.
    /// </summary>
    public Guid Id => s_pluginId;

    /// <summary>
    /// Gets the display name of the plugin.
    /// </summary>
    public string Name => "PDF Reader";

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
    public string Description => "Allows reading of PDF format books.";

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
        services.AddPdfReader(pluginId: Id);
    }
}
