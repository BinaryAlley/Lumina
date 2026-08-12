#region ========================================================================= USING =====================================================================================
using ErrorOr;
using Lumina.Application.Common.DataAccess.Entities.Plugins;
using Lumina.Application.Common.DataAccess.Repositories.Common.Actions;
using Lumina.Application.Common.DataAccess.Repositories.Common.Base;
using System;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Application.Common.DataAccess.Repositories.Plugins;

/// <summary>
/// Interface for the repository for plugins.
/// </summary>
public interface IPluginRepository : IRepository<PluginEntity>,
                                     IGetByIdRepositoryAction<PluginEntity, Guid>,
                                     IGetAllRepositoryAction<PluginEntity>
{
    /// <summary>
    /// Inserts the provided <paramref name="plugin"/> into the storage medium, or updates it when it already exists.
    /// </summary>
    /// <param name="plugin">The plugin to insert or update.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="ErrorOr{TValue}"/> representing either a successful operation, or an error.</returns>
    Task<ErrorOr<Updated>> UpsertAsync(PluginEntity plugin, CancellationToken cancellationToken);

    /// <summary>
    /// Updates the settings of the plugin identified by <paramref name="pluginId"/>.
    /// </summary>
    /// <param name="pluginId">The Id of the plugin whose settings are updated.</param>
    /// <param name="settingsJson">The serialized settings of the plugin.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="ErrorOr{TValue}"/> representing either a successful operation, or an error.</returns>
    Task<ErrorOr<Updated>> UpdateSettingsAsync(Guid pluginId, string? settingsJson, CancellationToken cancellationToken);
}
