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
/// Repository for the artwork provider configurations of media libraries.
/// </summary>
internal sealed class ArtworkProviderConfigurationRepository : IArtworkProviderConfigurationRepository
{
    private readonly LuminaDbContext _luminaDbContext;

    /// <summary>
    /// Initializes a new instance of the <see cref="ArtworkProviderConfigurationRepository"/> class.
    /// </summary>
    /// <param name="luminaDbContext">Injected Entity Framework DbContext.</param>
    public ArtworkProviderConfigurationRepository(LuminaDbContext luminaDbContext)
    {
        _luminaDbContext = luminaDbContext;
    }

    /// <summary>
    /// Gets all the artwork provider configurations of the media library identified by <paramref name="libraryId"/>.
    /// </summary>
    /// <param name="libraryId">The Id of the media library whose artwork provider configurations are retrieved.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="Result{TValue}"/> containing either a collection of <see cref="LibraryArtworkProviderConfigurationEntity"/>, or an error.</returns>
    public async Task<Result<IReadOnlyList<LibraryArtworkProviderConfigurationEntity>>> GetByLibraryIdAsync(Guid libraryId, CancellationToken cancellationToken)
    {
        return await _luminaDbContext.LibraryArtworkProviderConfigurations
            .Where(configuration => configuration.LibraryId == libraryId)
            .ToListAsync(cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Gets the artwork provider configuration of the media library identified by <paramref name="libraryId"/> for the plugin identified by <paramref name="pluginId"/>.
    /// </summary>
    /// <param name="libraryId">The Id of the media library whose artwork provider configuration is retrieved.</param>
    /// <param name="pluginId">The Id of the plugin whose artwork provider configuration is retrieved.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="Result{TValue}"/> containing either the <see cref="LibraryArtworkProviderConfigurationEntity"/>, or an error.</returns>
    public async Task<Result<LibraryArtworkProviderConfigurationEntity?>> GetByLibraryAndPluginIdAsync(Guid libraryId, Guid pluginId, CancellationToken cancellationToken)
    {
        return await _luminaDbContext.LibraryArtworkProviderConfigurations
            .FirstOrDefaultAsync(configuration => configuration.LibraryId == libraryId && configuration.PluginId == pluginId, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Inserts an artwork provider configuration into the storage medium, or updates it when it already exists.
    /// </summary>
    /// <param name="configuration">The artwork provider configuration to insert or update.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="Result{TValue}"/> representing either a successful operation, or an error.</returns>
    public async Task<Result<Updated>> UpsertAsync(LibraryArtworkProviderConfigurationEntity configuration, CancellationToken cancellationToken)
    {
        LibraryArtworkProviderConfigurationEntity? existingConfiguration = await _luminaDbContext.LibraryArtworkProviderConfigurations
            .FirstOrDefaultAsync(repositoryConfiguration => repositoryConfiguration.LibraryId == configuration.LibraryId && repositoryConfiguration.PluginId == configuration.PluginId, cancellationToken).ConfigureAwait(false);
        if (existingConfiguration is null)
            _luminaDbContext.LibraryArtworkProviderConfigurations.Add(configuration);
        else
        {
            // update only the mutable fields, preserving the Id of the existing configuration
            existingConfiguration.LibraryId = configuration.LibraryId;
            existingConfiguration.PluginId = configuration.PluginId;
            existingConfiguration.IsEnabled = configuration.IsEnabled;
            existingConfiguration.Rank = configuration.Rank;
        }
        return Result.Updated;
    }
}
