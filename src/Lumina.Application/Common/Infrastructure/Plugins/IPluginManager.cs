#region ========================================================================= USING =====================================================================================
using Lumina.Plugins.Contracts.Core.Plugins;
using System;
using System.Collections.Generic;
#endregion

namespace Lumina.Application.Common.Infrastructure.Plugins;

/// <summary>
/// Manager of the plugins loaded by the host application.
/// </summary>
public interface IPluginManager
{
    /// <summary>
    /// Gets the plugins that were loaded successfully.
    /// </summary>
    /// <returns>The collection of loaded plugins.</returns>
    IReadOnlyList<IPlugin> GetPlugins();

    /// <summary>
    /// Gets the plugin identified by <paramref name="pluginId"/>.
    /// </summary>
    /// <param name="pluginId">The unique identifier of the plugin.</param>
    /// <returns>The plugin, or <see langword="null"/> when no plugin with the provided Id was loaded.</returns>
    IPlugin? GetPlugin(Guid pluginId);
}
