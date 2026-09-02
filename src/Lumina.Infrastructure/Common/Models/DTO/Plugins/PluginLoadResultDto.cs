#region ========================================================================= USING =====================================================================================
using Lumina.Infrastructure.Core.Plugins;
using Lumina.Plugins.Contracts.Core.Plugins;
using System.Collections.Generic;
#endregion

namespace Lumina.Infrastructure.Common.Models.DTO.Plugins;

/// <summary>
/// Data transfer object for the result of loading the plugins from the plugins directory.
/// </summary>
/// <param name="Plugins">The plugins that were loaded successfully.</param>
/// <param name="LoadContexts">
/// The load contexts of the successfully loaded plugins. The contexts are kept referenced here for the lifetime of the host, because a collectible
/// load context is only kept alive by a direct reference to the context object itself, and the garbage collector would otherwise unload the plugin assemblies mid-request,
/// when a lazily resolved dependency is first used.
/// </param>
/// <param name="Errors">The errors that occurred while loading the plugins.</param>
internal sealed record PluginLoadResultDto(
    IReadOnlyList<IPlugin> Plugins,
    IReadOnlyList<PluginLoadContext> LoadContexts,
    IReadOnlyList<PluginLoadErrorDto> Errors
);
