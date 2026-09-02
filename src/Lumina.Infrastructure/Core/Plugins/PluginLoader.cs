#region ========================================================================= USING =====================================================================================
using Lumina.Infrastructure.Common.Models.DTO.Plugins;
using Lumina.Plugins.Contracts.Core.Plugins;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
#endregion

namespace Lumina.Infrastructure.Core.Plugins;

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
    public static PluginLoadResultDto LoadPlugins(string pluginsDirectory, IServiceCollection services)
    {
        List<IPlugin> plugins = [];
        List<PluginLoadContext> loadContexts = [];
        List<PluginLoadErrorDto> errors = [];

        if (!Directory.Exists(pluginsDirectory))
            return new PluginLoadResultDto(plugins, loadContexts, errors);

        foreach (string assemblyPath in Directory.EnumerateFiles(pluginsDirectory, "*.dll"))
        {
            // A failing plugin must not prevent the other plugins from loading, so each assembly is isolated in its own load context.
            PluginLoadContext loadContext = new(pluginsDirectory, Path.GetFileNameWithoutExtension(assemblyPath));
            try
            {
                Assembly assembly = loadContext.LoadFromAssemblyPath(assemblyPath);
                // The plugins directory also holds the managed dependencies of the plugins (like PdfPig.dll or SkiaSharp.dll next to the PDF plugin), which are not
                // plugin assemblies and must not be reflected over here. Only an assembly that references the plugin contracts can implement IPlugin, so the
                // dependency assemblies are filtered out before their types are ever loaded, which would otherwise surface spurious failures (for example a
                // ReflectionTypeLoadException on platform type forwarders) that the catch blocks below would report as broken plugins.
                if (assembly.GetReferencedAssemblies().Any(assemblyReference => assemblyReference.Name == "Lumina.Plugins.Contracts"))
                {
                    foreach (Type type in assembly.GetTypes())
                    {
                        if (type.IsClass && !type.IsAbstract && typeof(IPlugin).IsAssignableFrom(type))
                        {
                            IPlugin plugin = (IPlugin)Activator.CreateInstance(type)!;
                            if (plugin is IPluginServiceRegistrator serviceRegistrator)
                                serviceRegistrator.RegisterServices(services);
                            plugins.Add(plugin);
                            // A collectible load context is only kept alive by a direct reference to the context object itself, so the context of a loaded plugin is held for the lifetime
                            // of the host, keeping the plugin assemblies (and any lazily resolved dependency, like a book reader library) from being unloaded by the garbage collector.
                            loadContexts.Add(loadContext);
                            break; // Only one plugin descriptor per assembly.
                        }
                    }
                }
            }
            catch (BadImageFormatException)
            {
                // A native library is a valid PE image, but it cannot be loaded as a managed assembly, so it is thrown out by LoadFromAssemblyPath before it can ever reach the
                // plugin contracts reference check, and it is skipped silently. A file that is not a valid PE image at all is a genuinely broken plugin, and is recorded.
                // The managed dependencies of the plugins are valid managed assemblies, so they are not rejected here: they are instead filtered out by the plugin contracts
                // reference check above, before any reflection happens, and therefore never surface an error for being a non-plugin file.
                if (!IsPeImage(assemblyPath))
                    errors.Add(new PluginLoadErrorDto(Path.GetFileNameWithoutExtension(assemblyPath), $"Failed to load plugin assembly '{Path.GetFileName(assemblyPath)}': The file is not a valid .NET assembly."));
            }
            catch (Exception exception)
            {
                errors.Add(new PluginLoadErrorDto(Path.GetFileNameWithoutExtension(assemblyPath), $"Failed to load plugin assembly '{Path.GetFileName(assemblyPath)}': {exception.Message}"));
            }
        }

        return new PluginLoadResultDto(plugins, loadContexts, errors);
    }

    /// <summary>
    /// Determines whether the file at <paramref name="path"/> is a PE image (either a managed assembly or a native library),
    /// by checking its DOS signature.
    /// </summary>
    /// <param name="path">The path of the file to check.</param>
    /// <returns><see langword="true"/> when the file starts with the DOS signature of a PE image, <see langword="false"/> otherwise.</returns>
    private static bool IsPeImage(string path)
    {
        try
        {
            using FileStream fileStream = new(path, FileMode.Open, FileAccess.Read, FileShare.Read);
            Span<byte> signature = stackalloc byte[2];
            if (fileStream.Read(signature) < signature.Length)
                return false;
            // The DOS signature "MZ" is the header every PE image starts with, whether it is a managed assembly or a native library.
            return signature[0] == (byte)'M' && signature[1] == (byte)'Z';
        }
        catch (IOException)
        {
            return false;
        }
        catch (UnauthorizedAccessException)
        {
            return false;
        }
    }
}
