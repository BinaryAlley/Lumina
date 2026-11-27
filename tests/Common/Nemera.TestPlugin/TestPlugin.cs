#region ========================================================================= USING =====================================================================================
using Lumina.Plugins.Contracts.Common.Models.DTO.Settings;
using Lumina.Plugins.Contracts.Core.Plugins;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.TestPlugin;

/// <summary>
/// Test plugin used to verify the plugin loading pipeline of the host application.
/// </summary>
[ExcludeFromCodeCoverage]
public sealed class TestPlugin : IPlugin, IPluginServiceRegistrator
{
    public const string REGISTERED_MARKER = "plugin-services-registered";

    /// <summary>
    /// Gets the stable unique identifier of the plugin.
    /// </summary>
    public static Guid PluginId { get; } = Guid.Parse("7B5A3E5D-9B7F-4A1E-8D9C-2F3A4B5C6D7E");

    /// <summary>
    /// Gets the display name of the plugin.
    /// </summary>
    public static string PluginName { get; } = "Test Plugin";

    /// <summary>
    /// Gets the stable unique identifier of the plugin, used to persist its state.
    /// </summary>
    public Guid Id => PluginId;

    /// <summary>
    /// Gets the display name of the plugin.
    /// </summary>
    public string Name => PluginName;

    /// <summary>
    /// Gets the author of the plugin.
    /// </summary>
    public string Author => "Test Author";

    /// <summary>
    /// Gets the version of the plugin.
    /// </summary>
    public Version Version => new(1, 0, 0);

    /// <summary>
    /// Gets the description of the plugin.
    /// </summary>
    public string Description => "A test plugin used to verify the plugin loading pipeline.";

    /// <summary>
    /// Gets the settings schema of the plugin, used by the host to render the plugin settings page.
    /// </summary>
    /// <returns>The collection of setting descriptors declared by the plugin.</returns>
    public IReadOnlyList<PluginSettingDescriptorDto> GetSettingsSchema()
    {
        return [];
    }

    /// <summary>
    /// Registers the services of the plugin into the host dependency injection container.
    /// </summary>
    /// <param name="services">The service collection to which the plugin services are added.</param>
    public void RegisterServices(IServiceCollection services)
    {
        services.AddSingleton(REGISTERED_MARKER);
    }
}
