#region ========================================================================= USING =====================================================================================
using Lumina.Plugins.Contracts.Core.Plugins;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
#endregion

namespace Lumina.Infrastructure.Core.Plugins;

/// <summary>
/// Result of loading the plugins from the plugins directory.
/// </summary>
/// <param name="Plugins">The plugins that were loaded successfully.</param>
/// <param name="Errors">The errors that occurred while loading the plugins.</param>
internal sealed record PluginLoadResult(IReadOnlyList<IPlugin> Plugins, IReadOnlyList<string> Errors);

/// <summary>
/// Loads the plugins from the plugins directory, discovers their descriptors and registers their services.
/// </summary>
internal static class PluginLoader
{
    /// <summary>
    /// Loads the plugins from the <paramref name="pluginsDirectory"/>, discovers their descriptors and registers their services.
    /// </summary>
    /// <param name="pluginsDirectory">The directory where the plugin assemblies are located.</param>
    /// <param name="services">The service collection to which the plugin services are added.</param>
    /// <returns>The result of loading the plugins.</returns>
    public static PluginLoadResult LoadPlugins(string pluginsDirectory, IServiceCollection services)
    {
        List<IPlugin> plugins = [];
        List<string> errors = [];

        if (!Directory.Exists(pluginsDirectory))
            return new PluginLoadResult(plugins, errors);

        foreach (string assemblyPath in Directory.EnumerateFiles(pluginsDirectory, "*.dll"))
        {
            // a failing plugin must not prevent the other plugins from loading, so each assembly is isolated in its own load context
            PluginLoadContext loadContext = new(pluginsDirectory, Path.GetFileNameWithoutExtension(assemblyPath));
            try
            {
                Assembly assembly = loadContext.LoadFromAssemblyPath(assemblyPath);
                foreach (Type type in assembly.GetTypes())
                {
                    if (type.IsClass && !type.IsAbstract && typeof(IPlugin).IsAssignableFrom(type))
                    {
                        IPlugin plugin = (IPlugin)Activator.CreateInstance(type)!;
                        if (plugin is IPluginServiceRegistrator serviceRegistrator)
                            serviceRegistrator.RegisterServices(services);
                        plugins.Add(plugin);
                        break; // only one plugin descriptor per assembly
                    }
                }
            }
            catch (Exception exception)
            {
                errors.Add($"Failed to load plugin assembly '{Path.GetFileName(assemblyPath)}': {exception.Message}");
            }
        }

        return new PluginLoadResult(plugins, errors);
    }
}
