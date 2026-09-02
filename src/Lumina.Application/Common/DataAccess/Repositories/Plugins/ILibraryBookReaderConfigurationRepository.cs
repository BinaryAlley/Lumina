#region ========================================================================= USING =====================================================================================
using Lumina.Domain.Common.Primitives;
using Lumina.Application.Common.DataAccess.Entities.Plugins;
using Lumina.Application.Common.DataAccess.Repositories.Common.Base;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Application.Common.DataAccess.Repositories.Plugins;

/// <summary>
/// Interface for the repository for the book reader configurations of media libraries.
/// </summary>
public interface ILibraryBookReaderConfigurationRepository : IRepository<LibraryBookReaderConfigurationEntity>
{
    /// <summary>
    /// Gets all the book reader configurations of the media library identified by <paramref name="libraryId"/>.
    /// </summary>
    /// <param name="libraryId">The Id of the media library whose book reader configurations are retrieved.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="Result{TValue}"/> containing either a collection of <see cref="LibraryBookReaderConfigurationEntity"/>, or an error.</returns>
    Task<Result<IReadOnlyList<LibraryBookReaderConfigurationEntity>>> GetByLibraryIdAsync(Guid libraryId, CancellationToken cancellationToken);

    /// <summary>
    /// Gets the book reader configuration of the media library identified by <paramref name="libraryId"/> for the plugin identified by <paramref name="pluginId"/>.
    /// </summary>
    /// <param name="libraryId">The Id of the media library whose book reader configuration is retrieved.</param>
    /// <param name="pluginId">The Id of the plugin whose book reader configuration is retrieved.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="Result{TValue}"/> containing either the <see cref="LibraryBookReaderConfigurationEntity"/>, or an error.</returns>
    Task<Result<LibraryBookReaderConfigurationEntity?>> GetByLibraryAndPluginIdAsync(Guid libraryId, Guid pluginId, CancellationToken cancellationToken);

    /// <summary>
    /// Inserts the provided <paramref name="configuration"/> into the storage medium, or updates it when it already exists.
    /// </summary>
    /// <param name="configuration">The book reader configuration to insert or update.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="Result{TValue}"/> representing either a successful operation, or an error.</returns>
    Task<Result<Updated>> UpsertAsync(LibraryBookReaderConfigurationEntity configuration, CancellationToken cancellationToken);

    /// <summary>
    /// Deletes all the book reader configurations of the media library identified by <paramref name="libraryId"/>.
    /// </summary>
    /// <param name="libraryId">The Id of the media library whose book reader configurations are deleted.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="Result{TValue}"/> representing either a successful operation, or an error.</returns>
    Task<Result<Deleted>> DeleteByLibraryIdAsync(Guid libraryId, CancellationToken cancellationToken);

    /// <summary>
    /// Deletes all the book reader configurations referencing the plugin identified by <paramref name="pluginId"/>.
    /// </summary>
    /// <param name="pluginId">The Id of the plugin whose book reader configurations are deleted.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="Result{TValue}"/> representing either a successful operation, or an error.</returns>
    Task<Result<Deleted>> DeleteByPluginIdAsync(Guid pluginId, CancellationToken cancellationToken);

    /// <summary>
    /// Deletes the book reader configurations of the media library identified by <paramref name="libraryId"/> that reference
    /// one of the plugins identified by <paramref name="pluginIds"/>.
    /// </summary>
    /// <param name="libraryId">The Id of the media library whose book reader configurations are deleted.</param>
    /// <param name="pluginIds">The Ids of the plugins whose configurations of the media library are deleted.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="Result{TValue}"/> representing either a successful operation, or an error.</returns>
    Task<Result<Deleted>> DeleteByLibraryIdAndPluginIdsAsync(Guid libraryId, IEnumerable<Guid> pluginIds, CancellationToken cancellationToken);
}
