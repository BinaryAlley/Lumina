#region ========================================================================= USING =====================================================================================
using Lumina.Plugins.Contracts.Core.Plugins;
using System.Collections.Generic;
#endregion

namespace Lumina.Infrastructure.Common.Models.DTO.Plugins;

/// <summary>
/// Data transfer object for the result of loading the plugins from the plugins directory.
/// </summary>
/// <param name="Plugins">The plugins that were loaded successfully.</param>
/// <param name="Errors">The errors that occurred while loading the plugins.</param>
internal sealed record PluginLoadResultDto(
    IReadOnlyList<IPlugin> Plugins, 
    IReadOnlyList<PluginLoadErrorDto> Errors
);
