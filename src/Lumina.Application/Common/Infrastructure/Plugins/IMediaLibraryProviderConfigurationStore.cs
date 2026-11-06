#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.Plugins;
using Lumina.Application.Common.DTO.MediaLibrary.Management;
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.SharedKernel.Common.Enums.MediaLibrary;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Application.Common.Infrastructure.Plugins;

/// <summary>
/// Store of the metadata provider, artwork provider and book reader configurations of the media libraries.
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
    /// Adds a disabled metadata provider, artwork provider and book reader configuration for every loaded plugin that supports the provided library type and has no
    /// configuration yet for the media library identified by <paramref name="libraryId"/>.
    /// </summary>
    /// <remarks>
    /// The added configurations are only staged until the unit of work is saved, and are not visible to a later ensure, so it must be called at most once per media library before the changes are saved.
    /// </remarks>
    /// <param name="libraryId">The Id of the media library whose provider configurations are ensured.</param>
    /// <param name="libraryType">The type of the media library, used to determine the plugins whose providers apply.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="Result{TValue}"/> representing either a successful operation, or an error.</returns>
    Task<Result<Success>> EnsureProviderConfigurationsAsync(Guid libraryId, LibraryType libraryType, CancellationToken cancellationToken);

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

    /// <summary>
    /// Makes the provider and book reader configurations of the media library identified by <paramref name="libraryId"/> match the plugins supporting the provided library type, and applies the choices made for them.
    /// </summary>
    /// <remarks>
    /// A list that is provided reconciles that kind of configuration with the library type and applies the choices: the configurations of the plugins that do not support the library type
    /// are removed, even when the list does not name them, every supported plugin gets a configuration, and the list sets the enabled state of every provider or reader it names and, for the
    /// metadata and artwork providers, their order. A choice without an enabled state keeps the stored state, the supported plugins the list does not name keep their stored state and are
    /// ordered after the named ones, and a plugin without a stored configuration starts disabled. A list that is <see langword="null"/> leaves that kind of configuration untouched, unless
    /// <paramref name="shouldReconcileOmittedKinds"/> is <see langword="true"/>, in which case it is reconciled the same way, without choices. Whenever the book reader configurations are
    /// reconciled or chosen, the cached reader enablements of the library are dropped.
    /// </remarks>
    /// <param name="libraryId">The Id of the media library whose provider and book reader configurations are configured.</param>
    /// <param name="libraryType">The type of the media library, used to determine the plugins whose providers and book readers apply.</param>
    /// <param name="metadataProviders">The choices made for the metadata providers, in the order in which they are tried.</param>
    /// <param name="artworkProviders">The choices made for the artwork providers, in the order in which they are tried.</param>
    /// <param name="bookReaders">The choices made for the book readers.</param>
    /// <param name="shouldReconcileOmittedKinds">Whether the kinds of configuration whose list of choices is <see langword="null"/> are reconciled with the library type, which is needed when the
    /// library is new or its type changed.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="Result{TValue}"/> representing either a successful operation, or an error when a choice names a plugin that does not support the library type.</returns>
    Task<Result<Success>> ConfigureProvidersAsync(
        Guid libraryId,
        LibraryType libraryType,
        IReadOnlyList<LibraryProviderSelectionDto>? metadataProviders,
        IReadOnlyList<LibraryProviderSelectionDto>? artworkProviders,
        IReadOnlyList<LibraryProviderSelectionDto>? bookReaders,
        bool shouldReconcileOmittedKinds,
        CancellationToken cancellationToken);

    /// <summary>
    /// Gets the Ids of the loaded plugins that provide metadata for the provided library type, ordered alphabetically by plugin name.
    /// </summary>
    /// <param name="libraryType">The type of the media library the metadata providers must support.</param>
    /// <returns>The Ids of the plugins providing metadata for the library type.</returns>
    IReadOnlyList<Guid> GetSupportedMetadataProviderPluginIds(LibraryType libraryType);

    /// <summary>
    /// Gets the Ids of the loaded plugins that provide artwork for the provided library type, ordered alphabetically by plugin name.
    /// </summary>
    /// <param name="libraryType">The type of the media library the artwork providers must support.</param>
    /// <returns>The Ids of the plugins providing artwork for the library type.</returns>
    IReadOnlyList<Guid> GetSupportedArtworkProviderPluginIds(LibraryType libraryType);

    /// <summary>
    /// Gets the Ids of the loaded plugins that provide a book reader for the provided library type, ordered alphabetically by plugin name.
    /// </summary>
    /// <param name="libraryType">The type of the media library the book readers must support.</param>
    /// <returns>The Ids of the plugins providing a book reader for the library type.</returns>
    IReadOnlyList<Guid> GetSupportedBookReaderPluginIds(LibraryType libraryType);
}
