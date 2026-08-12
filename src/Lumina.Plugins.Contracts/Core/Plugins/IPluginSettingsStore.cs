#region ========================================================================= USING =====================================================================================
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Plugins.Contracts.Core.Plugins;

/// <summary>
/// Contract for reading the settings persisted by the host for a plugin.
/// </summary>
public interface IPluginSettingsStore
{
    /// <summary>
    /// Gets the settings persisted by the host for the plugin identified by <paramref name="pluginId"/>.
    /// </summary>
    /// <param name="pluginId">The unique identifier of the plugin whose settings are read.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to cancel the operation.</param>
    /// <returns>The persisted settings keyed by setting key, or <see langword="null"/> when no settings were persisted for the plugin.</returns>
    Task<IReadOnlyDictionary<string, string>?> GetSettingsAsync(Guid pluginId, CancellationToken cancellationToken);
}
