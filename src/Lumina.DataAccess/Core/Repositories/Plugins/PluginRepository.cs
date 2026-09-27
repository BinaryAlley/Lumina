#region ========================================================================= USING =====================================================================================
using Lumina.Domain.Common.Primitives;
using Lumina.Application.Common.DataAccess.Entities.Plugins;
using Lumina.Application.Common.DataAccess.Repositories.Plugins;
using Lumina.Application.Common.DTO.Filtering;
using Lumina.Application.Common.DTO.Pagination;
using Lumina.DataAccess.Core.UoW;
using Lumina.Domain.Common.Errors;
using Lumina.Domain.SharedKernel.Common.Enums.Common;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
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
    public async Task<Result<PluginEntity?>> GetByIdAsync(Guid id, bool shouldIncludeNavigationProperties = true, bool shouldTrackEntities = true, CancellationToken cancellationToken = default)
    {
        IQueryable<PluginEntity> query = _luminaDbContext.Plugins;
        if (!shouldTrackEntities)
            query = query.AsNoTracking();
        return await query.FirstOrDefaultAsync(plugin => plugin.Id == id, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Gets all the detected plugins, or a page of them when the pagination data is provided.
    /// </summary>
    /// <typeparam name="TFilter">The type of the filter carrying the criteria used to filter the results.</typeparam>
    /// <param name="paginationData">The pagination data that includes the current page and the number of items per page to retrieve. If <see langword="null"/>, all the matching plugins are returned.</param>
    /// <param name="sortBy">The name of the field by which to sort the results.</param>
    /// <param name="sortOrder">The direction in which to sort the results.</param>
    /// <param name="filterModel">The model containing the parameters used to filter the results.</param>
    /// <param name="shouldIncludeNavigationProperties">Whether the navigation properties of the entities should be loaded together with the entities themselves. Pass <see langword="false"/> to retrieve only the data stored directly on the entity rows.</param>
    /// <param name="shouldTrackEntities">Whether the retrieved entities should be tracked by the persistence medium, so that changes to them can be saved. Pass <see langword="false"/> for read-only scenarios, to avoid the tracking overhead.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="Result{TValue}"/> containing either a paginated result of <see cref="PluginEntity"/>, or an error.</returns>
    public async Task<Result<PaginatedResultDto<PluginEntity>>> GetAllAsync<TFilter>(PaginationDataDto? paginationData = null, string? sortBy = null, SortOrder? sortOrder = null, TFilter? filterModel = null, bool shouldIncludeNavigationProperties = true, bool shouldTrackEntities = true, CancellationToken cancellationToken = default) where TFilter : BaseFilterDto
    {
        IQueryable<PluginEntity> query = _luminaDbContext.Plugins;
        if (!shouldTrackEntities)
            query = query.AsNoTracking();

        // If no pagination was requested, return all the plugins.
        if (paginationData is null)
        {
            IReadOnlyList<PluginEntity> allPlugins = await query.ToListAsync(cancellationToken).ConfigureAwait(false);
            return new PaginatedResultDto<PluginEntity>
            {
                Data = allPlugins,
                CurrentPage = 1,
                PerPage = allPlugins.Count,
                Count = allPlugins.Count,
                NumberOfPages = 1
            };
        }

        int count = await query.Select(plugin => plugin.Id).CountAsync(cancellationToken).ConfigureAwait(false);
        int numberOfPages = (int)Math.Ceiling((double)count / paginationData.PerPage);
        int currentPage = Math.Min(paginationData.CurrentPage, Math.Max(1, numberOfPages)); // Make sure current page doesn't exceed maximum number of pages.

        IReadOnlyList<PluginEntity> paginatedResult = await query
            .Skip((currentPage - 1) * paginationData.PerPage)
            .Take(paginationData.PerPage)
            .ToListAsync(cancellationToken).ConfigureAwait(false);

        return new PaginatedResultDto<PluginEntity>
        {
            Data = paginatedResult,
            CurrentPage = currentPage,
            PerPage = paginationData.PerPage,
            Count = count,
            NumberOfPages = numberOfPages
        };
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
            // Update only the detection fields, preserving the stored settings and creation date.
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
