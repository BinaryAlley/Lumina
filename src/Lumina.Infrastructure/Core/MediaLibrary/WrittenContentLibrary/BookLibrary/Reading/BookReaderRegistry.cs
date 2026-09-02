#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.Infrastructure.Plugins;
using Lumina.Application.Common.Infrastructure.Reading;
using Lumina.Plugins.Contracts.Core.Plugins;
using Lumina.Plugins.Contracts.Core.Reading;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
#endregion

namespace Lumina.Infrastructure.Core.MediaLibrary.WrittenContentLibrary.BookLibrary.Reading;

/// <summary>
/// Registry of the book readers provided by the loaded plugins.
/// </summary>
internal sealed class BookReaderRegistry : IBookReaderRegistry
{
    private readonly IPluginManager _pluginManager;
    private readonly IServiceProvider _serviceProvider;

    /// <summary>
    /// Initializes a new instance of the <see cref="BookReaderRegistry"/> class.
    /// </summary>
    /// <param name="pluginManager">Injected manager of the plugins loaded by the host application.</param>
    /// <param name="serviceProvider">Injected provider used to resolve the book readers registered by the plugins.</param>
    public BookReaderRegistry(IPluginManager pluginManager, IServiceProvider serviceProvider)
    {
        _pluginManager = pluginManager;
        _serviceProvider = serviceProvider;
    }

    /// <summary>
    /// Gets the file extensions supported by the book readers of each loaded plugin, keyed by the Id of the plugin.
    /// </summary>
    /// <returns>The supported extensions of the book readers, keyed by the Id of the plugin providing them.</returns>
    public IReadOnlyDictionary<Guid, IReadOnlyList<string>> GetSupportedExtensionsByPluginId()
    {
        Dictionary<Guid, IReadOnlyList<string>> extensionsByPluginId = [];
        // The extensions are read live from the loaded book readers, rather than persisted, so that a plugin update that
        // changes its supported formats is reflected immediately and the stored configurations never go stale.
        foreach (IPlugin plugin in _pluginManager.GetPlugins())
        {
            List<string> extensions = [.. _serviceProvider.GetKeyedServices<IBookReader>(plugin.Id)
                .SelectMany(reader => reader.SupportedExtensions)
                .Where(extension => !string.IsNullOrWhiteSpace(extension))
                .OrderBy(extension => extension, StringComparer.OrdinalIgnoreCase)
                .Distinct(StringComparer.OrdinalIgnoreCase)];
            if (extensions.Count > 0)
                extensionsByPluginId[plugin.Id] = extensions;
        }
        return extensionsByPluginId;
    }
}
