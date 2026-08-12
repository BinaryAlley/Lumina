#region ========================================================================= USING =====================================================================================
using System.IO;
using System.Reflection;
using System.Runtime.Loader;
#endregion

namespace Lumina.Infrastructure.Core.Plugins;

/// <summary>
/// Assembly load context of a single plugin, resolving the shared host assemblies from the default context and the plugin local dependencies from the plugin directory.
/// </summary>
internal sealed class PluginLoadContext : AssemblyLoadContext
{
    private readonly string _pluginDirectory;

    /// <summary>
    /// Initializes a new instance of the <see cref="PluginLoadContext"/> class.
    /// </summary>
    /// <param name="pluginDirectory">The directory where the plugin assemblies and their dependencies are located.</param>
    /// <param name="name">The name of the load context.</param>
    public PluginLoadContext(string pluginDirectory, string name) : base(name, isCollectible: true)
    {
        _pluginDirectory = pluginDirectory;
    }

    /// <inheritdoc/>
    protected override Assembly? Load(AssemblyName assemblyName)
    {
        if (assemblyName.Name is null)
            return null;

        // try to resolve the assembly from the default context first, so that the plugin shares the host and framework assemblies
        try
        {
            return AssemblyLoadContext.Default.LoadFromAssemblyName(assemblyName);
        }
        catch (FileNotFoundException)
        {
        }

        // fall back to resolving the assembly from the plugin directory
        string dependencyPath = Path.Combine(_pluginDirectory, assemblyName.Name + ".dll");
        if (File.Exists(dependencyPath))
            return LoadFromAssemblyPath(dependencyPath);

        return null;
    }
}
