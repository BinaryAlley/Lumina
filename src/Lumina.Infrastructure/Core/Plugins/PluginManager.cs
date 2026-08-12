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

    /// <summary>
    /// Initializes a new instance of the <see cref="PluginManager"/> class.
    /// </summary>
    /// <param name="plugins">The plugins that were loaded successfully.</param>
    public PluginManager(IEnumerable<IPlugin> plugins)
    {
        _pluginsById = plugins.ToDictionary(plugin => plugin.Id);
    }

    /// <inheritdoc/>
    public IReadOnlyList<IPlugin> GetPlugins()
    {
        return [.. _pluginsById.Values];
    }

    /// <inheritdoc/>
    public IPlugin? GetPlugin(Guid pluginId)
    {
        return _pluginsById.GetValueOrDefault(pluginId);
    }
}
