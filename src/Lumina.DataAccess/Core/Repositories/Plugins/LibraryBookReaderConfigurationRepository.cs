#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.Plugins;
using Lumina.Application.Common.DataAccess.Repositories.Plugins;
using Lumina.DataAccess.Core.UoW;
using Lumina.Domain.Common.Primitives;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.DataAccess.Core.Repositories.Plugins;

/// <summary>
/// Repository for the book reader configurations of media libraries.
/// </summary>
internal sealed class LibraryBookReaderConfigurationRepository : ILibraryBookReaderConfigurationRepository
{
    private readonly LuminaDbContext _luminaDbContext;

    /// <summary>
    /// Initializes a new instance of the <see cref="LibraryBookReaderConfigurationRepository"/> class.
    /// </summary>
    /// <param name="luminaDbContext">Injected Entity Framework DbContext.</param>
    public LibraryBookReaderConfigurationRepository(LuminaDbContext luminaDbContext)
    {
        _luminaDbContext = luminaDbContext;
    }

    /// <summary>
    /// Gets all the book reader configurations of the media library identified by <paramref name="libraryId"/>.
    /// </summary>
    /// <param name="libraryId">The Id of the media library whose book reader configurations are retrieved.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="Result{TValue}"/> containing either a collection of <see cref="LibraryBookReaderConfigurationEntity"/>, or an error.</returns>
    public async Task<Result<IReadOnlyList<LibraryBookReaderConfigurationEntity>>> GetByLibraryIdAsync(Guid libraryId, CancellationToken cancellationToken)
    {
        return await _luminaDbContext.LibraryBookReaderConfigurations
            .Where(configuration => configuration.LibraryId == libraryId)
            .ToListAsync(cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Gets the book reader configuration of the media library identified by <paramref name="libraryId"/> for the plugin identified by <paramref name="pluginId"/>.
    /// </summary>
    /// <param name="libraryId">The Id of the media library whose book reader configuration is retrieved.</param>
    /// <param name="pluginId">The Id of the plugin whose book reader configuration is retrieved.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="Result{TValue}"/> containing either the <see cref="LibraryBookReaderConfigurationEntity"/>, or an error.</returns>
    public async Task<Result<LibraryBookReaderConfigurationEntity?>> GetByLibraryAndPluginIdAsync(Guid libraryId, Guid pluginId, CancellationToken cancellationToken)
    {
        return await _luminaDbContext.LibraryBookReaderConfigurations
            .FirstOrDefaultAsync(configuration => configuration.LibraryId == libraryId && configuration.PluginId == pluginId, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Inserts a book reader configuration into the storage medium, or updates it when it already exists.
    /// </summary>
    /// <param name="configuration">The book reader configuration to insert or update.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="Result{TValue}"/> representing either a successful operation, or an error.</returns>
    public async Task<Result<Updated>> UpsertAsync(LibraryBookReaderConfigurationEntity configuration, CancellationToken cancellationToken)
    {
        LibraryBookReaderConfigurationEntity? existingConfiguration = await _luminaDbContext.LibraryBookReaderConfigurations
            .FirstOrDefaultAsync(repositoryConfiguration => repositoryConfiguration.LibraryId == configuration.LibraryId && repositoryConfiguration.PluginId == configuration.PluginId, cancellationToken).ConfigureAwait(false);
        if (existingConfiguration is null)
            _luminaDbContext.LibraryBookReaderConfigurations.Add(configuration);
        else
        {
            // Update only the mutable fields, preserving the Id of the existing configuration.
            existingConfiguration.LibraryId = configuration.LibraryId;
            existingConfiguration.PluginId = configuration.PluginId;
            existingConfiguration.IsEnabled = configuration.IsEnabled;
        }
        return Result.Updated;
    }

    /// <summary>
    /// Deletes all the book reader configurations of the media library identified by <paramref name="libraryId"/>.
    /// </summary>
    /// <param name="libraryId">The Id of the media library whose book reader configurations are deleted.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="Result{TValue}"/> representing either a successful operation, or an error.</returns>
    public async Task<Result<Deleted>> DeleteByLibraryIdAsync(Guid libraryId, CancellationToken cancellationToken)
    {
        List<LibraryBookReaderConfigurationEntity> configurations = await _luminaDbContext.LibraryBookReaderConfigurations
            .Where(configuration => configuration.LibraryId == libraryId)
            .ToListAsync(cancellationToken).ConfigureAwait(false);
        if (configurations.Count > 0)
            _luminaDbContext.LibraryBookReaderConfigurations.RemoveRange(configurations);
        return Result.Deleted;
    }

    /// <summary>
    /// Deletes all the book reader configurations referencing the plugin identified by <paramref name="pluginId"/>.
    /// </summary>
    /// <param name="pluginId">The Id of the plugin whose book reader configurations are deleted.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="Result{TValue}"/> representing either a successful operation, or an error.</returns>
    public async Task<Result<Deleted>> DeleteByPluginIdAsync(Guid pluginId, CancellationToken cancellationToken)
    {
        List<LibraryBookReaderConfigurationEntity> configurations = await _luminaDbContext.LibraryBookReaderConfigurations
            .Where(configuration => configuration.PluginId == pluginId)
            .ToListAsync(cancellationToken).ConfigureAwait(false);
        if (configurations.Count > 0)
            _luminaDbContext.LibraryBookReaderConfigurations.RemoveRange(configurations);
        return Result.Deleted;
    }

    /// <summary>
    /// Deletes the book reader configurations of the media library identified by <paramref name="libraryId"/> that reference
    /// one of the plugins identified by <paramref name="pluginIds"/>.
    /// </summary>
    /// <param name="libraryId">The Id of the media library whose book reader configurations are deleted.</param>
    /// <param name="pluginIds">The Ids of the plugins whose configurations of the media library are deleted.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="Result{TValue}"/> representing either a successful operation, or an error.</returns>
    public async Task<Result<Deleted>> DeleteByLibraryIdAndPluginIdsAsync(Guid libraryId, IEnumerable<Guid> pluginIds, CancellationToken cancellationToken)
    {
        List<Guid> pluginIdList = [.. pluginIds];
        List<LibraryBookReaderConfigurationEntity> configurations = await _luminaDbContext.LibraryBookReaderConfigurations
            .Where(configuration => configuration.LibraryId == libraryId && pluginIdList.Contains(configuration.PluginId))
            .ToListAsync(cancellationToken).ConfigureAwait(false);
        if (configurations.Count > 0)
            _luminaDbContext.LibraryBookReaderConfigurations.RemoveRange(configurations);
        return Result.Deleted;
    }
}
