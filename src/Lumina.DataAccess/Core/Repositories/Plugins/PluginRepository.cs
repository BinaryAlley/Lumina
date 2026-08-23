#region ========================================================================= USING =====================================================================================
using Lumina.Domain.Common.Primitives;
using Lumina.Application.Common.DataAccess.Entities.Plugins;
using Lumina.Application.Common.DataAccess.Repositories.Plugins;
using Lumina.DataAccess.Core.UoW;
using Lumina.Domain.Common.Errors;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.DataAccess.Core.Repositories.Plugins;

/// <summary>
/// Repository for plugins.
/// </summary>
internal sealed class PluginRepository : IPluginRepository
{
    private readonly LuminaDbContext _luminaDbContext;

    /// <summary>
    /// Initializes a new instance of the <see cref="PluginRepository"/> class.
    /// </summary>
    /// <param name="luminaDbContext">Injected Entity Framework DbContext.</param>
    public PluginRepository(LuminaDbContext luminaDbContext)
    {
        _luminaDbContext = luminaDbContext;
    }

    /// <summary>
    /// Gets a plugin by its Id.
    /// </summary>
    /// <param name="id">The Id of the plugin to get.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="Result{TValue}"/> containing either a <see cref="PluginEntity"/>, or an error.</returns>
    public async Task<Result<PluginEntity?>> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return await _luminaDbContext.Plugins
            .FirstOrDefaultAsync(plugin => plugin.Id == id, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Gets all the detected plugins.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="Result{TValue}"/> containing either a collection of <see cref="PluginEntity"/>, or an error.</returns>
    public async Task<Result<IEnumerable<PluginEntity>>> GetAllAsync(CancellationToken cancellationToken)
    {
        return await _luminaDbContext.Plugins
            .ToListAsync(cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Inserts a plugin into the storage medium, or updates it when it already exists.
    /// </summary>
    /// <param name="plugin">The plugin to insert or update.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="Result{TValue}"/> representing either a successful operation, or an error.</returns>
    public async Task<Result<Updated>> UpsertAsync(PluginEntity plugin, CancellationToken cancellationToken)
    {
        PluginEntity? existingPlugin = await _luminaDbContext.Plugins
            .FirstOrDefaultAsync(repositoryPlugin => repositoryPlugin.Id == plugin.Id, cancellationToken).ConfigureAwait(false);
        if (existingPlugin is null)
            _luminaDbContext.Plugins.Add(plugin);
        else
        {
            // update only the detection fields, preserving the stored settings and creation date
            existingPlugin.Name = plugin.Name;
            existingPlugin.Author = plugin.Author;
            existingPlugin.Version = plugin.Version;
            existingPlugin.Description = plugin.Description;
            existingPlugin.LoadStatus = plugin.LoadStatus;
            existingPlugin.LoadError = plugin.LoadError;
        }
        return Result.Updated;
    }

    /// <summary>
    /// Deletes a plugin identified by <paramref name="id"/> from the storage medium.
    /// </summary>
    /// <param name="id">The id of the plugin to be deleted.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="Result{TValue}"/> representing either a successful operation, or an error.</returns>
    public async Task<Result<Deleted>> DeleteByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        PluginEntity? plugin = await _luminaDbContext.Plugins
            .FirstOrDefaultAsync(repositoryPlugin => repositoryPlugin.Id == id, cancellationToken).ConfigureAwait(false);
        if (plugin is null)
            return Errors.Plugins.PluginNotFound;
        _luminaDbContext.Plugins.Remove(plugin);
        return Result.Deleted;
    }

    /// <summary>
    /// Updates the settings of the plugin identified by <paramref name="pluginId"/>.
    /// </summary>
    /// <param name="pluginId">The Id of the plugin whose settings are updated.</param>
    /// <param name="settingsJson">The serialized settings of the plugin.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="Result{TValue}"/> representing either a successful operation, or an error.</returns>
    public async Task<Result<Updated>> UpdateSettingsAsync(Guid pluginId, string? settingsJson, CancellationToken cancellationToken)
    {
        PluginEntity? existingPlugin = await _luminaDbContext.Plugins
            .FirstOrDefaultAsync(repositoryPlugin => repositoryPlugin.Id == pluginId, cancellationToken).ConfigureAwait(false);
        if (existingPlugin is null)
            return Errors.Plugins.PluginNotFound;
        existingPlugin.SettingsJson = settingsJson;
        return Result.Updated;
    }
}
