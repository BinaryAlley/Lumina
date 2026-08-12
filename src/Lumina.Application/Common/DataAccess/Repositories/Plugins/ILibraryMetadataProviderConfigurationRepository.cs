#region ========================================================================= USING =====================================================================================
using ErrorOr;
using Lumina.Application.Common.DataAccess.Entities.Plugins;
using Lumina.Application.Common.DataAccess.Repositories.Common.Base;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Application.Common.DataAccess.Repositories.Plugins;

/// <summary>
/// Interface for the repository for the metadata provider configurations of media libraries.
/// </summary>
public interface ILibraryMetadataProviderConfigurationRepository : IRepository<LibraryMetadataProviderConfigurationEntity>
{
    /// <summary>
    /// Gets all the metadata provider configurations of the media library identified by <paramref name="libraryId"/>.
    /// </summary>
    /// <param name="libraryId">The Id of the media library whose metadata provider configurations are retrieved.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="ErrorOr{TValue}"/> containing either a collection of <see cref="LibraryMetadataProviderConfigurationEntity"/>, or an error.</returns>
    Task<ErrorOr<IReadOnlyList<LibraryMetadataProviderConfigurationEntity>>> GetByLibraryIdAsync(Guid libraryId, CancellationToken cancellationToken);

    /// <summary>
    /// Gets the metadata provider configuration of the media library identified by <paramref name="libraryId"/> for the plugin identified by <paramref name="pluginId"/>.
    /// </summary>
    /// <param name="libraryId">The Id of the media library whose metadata provider configuration is retrieved.</param>
    /// <param name="pluginId">The Id of the plugin whose metadata provider configuration is retrieved.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="ErrorOr{TValue}"/> containing either the <see cref="LibraryMetadataProviderConfigurationEntity"/>, or an error.</returns>
    Task<ErrorOr<LibraryMetadataProviderConfigurationEntity?>> GetByLibraryAndPluginIdAsync(Guid libraryId, Guid pluginId, CancellationToken cancellationToken);

    /// <summary>
    /// Inserts the provided <paramref name="configuration"/> into the storage medium, or updates it when it already exists.
    /// </summary>
    /// <param name="configuration">The metadata provider configuration to insert or update.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="ErrorOr{TValue}"/> representing either a successful operation, or an error.</returns>
    Task<ErrorOr<Updated>> UpsertAsync(LibraryMetadataProviderConfigurationEntity configuration, CancellationToken cancellationToken);
}
