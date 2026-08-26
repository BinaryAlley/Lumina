#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.Plugins;
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.SharedKernel.Common.Enums.MediaLibrary;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Application.Common.Infrastructure.Plugins;

/// <summary>
/// Store of the metadata and artwork provider configurations of the media libraries.
/// </summary>
public interface IMediaLibraryProviderConfigurationStore
{
    /// <summary>
    /// Gets the metadata provider configurations of the media library identified by <paramref name="libraryId"/>.
    /// </summary>
    /// <param name="libraryId">The Id of the media library whose metadata provider configurations are retrieved.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="Result{TValue}"/> containing either the configurations, or an error.</returns>
    Task<Result<IReadOnlyList<LibraryMetadataProviderConfigurationEntity>>> GetConfigurationsAsync(Guid libraryId, CancellationToken cancellationToken);

    /// <summary>
    /// Adds a disabled provider configuration for every loaded plugin that supports the provided library type and has no
    /// configuration yet for the media library identified by <paramref name="libraryId"/>.
    /// </summary>
    /// <param name="libraryId">The Id of the media library whose provider configurations are ensured.</param>
    /// <param name="libraryType">The type of the media library, used to determine the plugins whose providers apply.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="Result{TValue}"/> representing either a successful operation, or an error.</returns>
    Task<Result<Success>> EnsureProviderConfigurationsAsync(Guid libraryId, LibraryType libraryType, CancellationToken cancellationToken);

    /// <summary>
    /// Replaces the provider configurations of the media library identified by <paramref name="libraryId"/> so that they
    /// match the plugins supporting the provided library type.
    /// </summary>
    /// <param name="libraryId">The Id of the media library whose provider configurations are reconciled.</param>
    /// <param name="libraryType">The type of the media library, used to determine the plugins whose providers apply.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="Result{TValue}"/> representing either a successful operation, or an error.</returns>
    Task<Result<Success>> ReconcileProviderConfigurationsAsync(Guid libraryId, LibraryType libraryType, CancellationToken cancellationToken);

    /// <summary>
    /// Deletes all the provider configurations of the media library identified by <paramref name="libraryId"/>.
    /// </summary>
    /// <param name="libraryId">The Id of the media library whose provider configurations are deleted.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="Result{TValue}"/> representing either a successful operation, or an error.</returns>
    Task<Result<Deleted>> RemoveProviderConfigurationsForLibraryAsync(Guid libraryId, CancellationToken cancellationToken);

    /// <summary>
    /// Deletes all the provider configurations referencing the plugin identified by <paramref name="pluginId"/>.
    /// </summary>
    /// <param name="pluginId">The Id of the plugin whose provider configurations are deleted.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="Result{TValue}"/> representing either a successful operation, or an error.</returns>
    Task<Result<Deleted>> RemoveProviderConfigurationsAsync(Guid pluginId, CancellationToken cancellationToken);
}
