#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.Infrastructure.Plugins;
using Lumina.Plugins.Contracts.Core.Plugins;
using System;
using System.Collections.Generic;
using System.Linq;
#endregion

namespace Lumina.Infrastructure.Core.Plugins;

/// <summary>
/// Manager of the plugins loaded by the host application.
/// </summary>
internal sealed class PluginManager : IPluginManager
{
    private readonly IReadOnlyDictionary<Guid, IPlugin> _pluginsById;
    // The load contexts of the loaded plugins are held for the lifetime of the host, because a collectible load context is only kept alive by a direct reference to the
    // context object itself, and the garbage collector would otherwise unload the plugin assemblies mid-request, when a lazily resolved dependency is first used.
    private readonly IReadOnlyList<PluginLoadContext> _loadContexts;

    /// <summary>
    /// Initializes a new instance of the <see cref="PluginManager"/> class.
    /// </summary>
    /// <param name="plugins">The plugins that were loaded successfully.</param>
    /// <param name="loadContexts">The load contexts of the loaded plugins, kept referenced for the lifetime of the host.</param>
    public PluginManager(IEnumerable<IPlugin> plugins, IReadOnlyList<PluginLoadContext> loadContexts)
    {
        _pluginsById = plugins.ToDictionary(plugin => plugin.Id);
        _loadContexts = loadContexts;
    }

    /// <summary>
    /// Gets the plugins that were loaded successfully.
    /// </summary>
    /// <returns>The collection of loaded plugins.</returns>
    public IReadOnlyList<IPlugin> GetPlugins()
    {
        return [.. _pluginsById.Values];
    }

    /// <summary>
    /// Gets the plugin identified by <paramref name="pluginId"/>.
    /// </summary>
    /// <param name="pluginId">The unique identifier of the plugin.</param>
    /// <returns>The plugin, or <see langword="null"/> when no plugin with the provided Id was loaded.</returns>
    public IPlugin? GetPlugin(Guid pluginId)
    {
        return _pluginsById.GetValueOrDefault(pluginId);
    }
}
