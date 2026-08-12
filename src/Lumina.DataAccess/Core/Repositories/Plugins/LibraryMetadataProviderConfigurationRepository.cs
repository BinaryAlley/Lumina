#region ========================================================================= USING =====================================================================================
using ErrorOr;
using Lumina.Application.Common.DataAccess.Entities.Plugins;
using Lumina.Application.Common.DataAccess.Repositories.Plugins;
using Lumina.DataAccess.Core.UoW;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.DataAccess.Core.Repositories.Plugins;

/// <summary>
/// Repository for the metadata provider configurations of media libraries.
/// </summary>
internal sealed class LibraryMetadataProviderConfigurationRepository : ILibraryMetadataProviderConfigurationRepository
{
    private readonly LuminaDbContext _luminaDbContext;

    /// <summary>
    /// Initializes a new instance of the <see cref="LibraryMetadataProviderConfigurationRepository"/> class.
    /// </summary>
    /// <param name="luminaDbContext">Injected Entity Framework DbContext.</param>
    public LibraryMetadataProviderConfigurationRepository(LuminaDbContext luminaDbContext)
    {
        _luminaDbContext = luminaDbContext;
    }

    /// <summary>
    /// Gets all the metadata provider configurations of the media library identified by <paramref name="libraryId"/>.
    /// </summary>
    /// <param name="libraryId">The Id of the media library whose metadata provider configurations are retrieved.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="ErrorOr{TValue}"/> containing either a collection of <see cref="LibraryMetadataProviderConfigurationEntity"/>, or an error.</returns>
    public async Task<ErrorOr<IReadOnlyList<LibraryMetadataProviderConfigurationEntity>>> GetByLibraryIdAsync(Guid libraryId, CancellationToken cancellationToken)
    {
        return await _luminaDbContext.LibraryMetadataProviderConfigurations
            .Where(configuration => configuration.LibraryId == libraryId)
            .ToListAsync(cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Gets the metadata provider configuration of the media library identified by <paramref name="libraryId"/> for the plugin identified by <paramref name="pluginId"/>.
    /// </summary>
    /// <param name="libraryId">The Id of the media library whose metadata provider configuration is retrieved.</param>
    /// <param name="pluginId">The Id of the plugin whose metadata provider configuration is retrieved.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="ErrorOr{TValue}"/> containing either the <see cref="LibraryMetadataProviderConfigurationEntity"/>, or an error.</returns>
    public async Task<ErrorOr<LibraryMetadataProviderConfigurationEntity?>> GetByLibraryAndPluginIdAsync(Guid libraryId, Guid pluginId, CancellationToken cancellationToken)
    {
        return await _luminaDbContext.LibraryMetadataProviderConfigurations
            .FirstOrDefaultAsync(configuration => configuration.LibraryId == libraryId && configuration.PluginId == pluginId, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Inserts a metadata provider configuration into the storage medium, or updates it when it already exists.
    /// </summary>
    /// <param name="configuration">The metadata provider configuration to insert or update.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="ErrorOr{TValue}"/> representing either a successful operation, or an error.</returns>
    public async Task<ErrorOr<Updated>> UpsertAsync(LibraryMetadataProviderConfigurationEntity configuration, CancellationToken cancellationToken)
    {
        LibraryMetadataProviderConfigurationEntity? existingConfiguration = await _luminaDbContext.LibraryMetadataProviderConfigurations
            .FirstOrDefaultAsync(repositoryConfiguration => repositoryConfiguration.LibraryId == configuration.LibraryId && repositoryConfiguration.PluginId == configuration.PluginId, cancellationToken).ConfigureAwait(false);
        if (existingConfiguration is null)
        {
            _luminaDbContext.LibraryMetadataProviderConfigurations.Add(configuration);
        }
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
