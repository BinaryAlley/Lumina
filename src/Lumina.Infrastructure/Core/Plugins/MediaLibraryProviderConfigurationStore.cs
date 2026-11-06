#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.Plugins;
using Lumina.Application.Common.DataAccess.UoW;
using Lumina.Application.Common.DTO.MediaLibrary.Management;
using Lumina.Application.Common.Infrastructure.Plugins;
using Lumina.Application.Common.Infrastructure.Reading;
using Lumina.Domain.Common.Errors;
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.SharedKernel.Common.Enums.MediaLibrary;
using Lumina.Plugins.Contracts.Core.Metadata;
using Lumina.Plugins.Contracts.Core.Plugins;
using Lumina.Plugins.Contracts.Core.Reading;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Infrastructure.Core.Plugins;

/// <summary>
/// Store of the metadata, artwork and book reader configurations of the media libraries.
/// </summary>
/// <remarks>
/// The configurations decide, per media library, which loaded plugins may serve metadata or artwork, which book reader opens the
/// books of the library, and whether each of them is currently enabled. They are persisted as rows instead of being derived from
/// the loaded plugins on the fly, because the user can enable or disable a provider per library, and because the set of loaded
/// plugins can change between restarts. This class is the only one that writes them, while the queries, the scan enrichers and the
/// reading service read them through their repositories. It maintains them in three ways: "ensure" only adds what is missing,
/// "reconcile" makes the persisted configurations match the plugins supporting the library type, and "apply" reconciles them
/// and sets the enabled state and the order chosen when the library is added or updated.
/// </remarks>
internal sealed class MediaLibraryProviderConfigurationStore : IMediaLibraryProviderConfigurationStore
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPluginManager _pluginManager;
    private readonly IServiceProvider _serviceProvider;
    private readonly IBookReaderEnablementCache _enablementCache;

    /// <summary>
    /// Initializes a new instance of the <see cref="MediaLibraryProviderConfigurationStore"/> class.
    /// </summary>
    /// <param name="unitOfWork">Injected unit of work for interacting with the data access layer repositories.</param>
    /// <param name="pluginManager">Injected manager of the plugins loaded by the host application.</param>
    /// <param name="serviceProvider">Injected provider used to resolve the providers registered by the plugins.</param>
    /// <param name="enablementCache">Injected cache of whether the book reader of a plugin is enabled for a media library.</param>
    public MediaLibraryProviderConfigurationStore(IUnitOfWork unitOfWork, IPluginManager pluginManager, IServiceProvider serviceProvider, IBookReaderEnablementCache enablementCache)
    {
        _unitOfWork = unitOfWork;
        _pluginManager = pluginManager;
        _serviceProvider = serviceProvider;
        _enablementCache = enablementCache;
    }

    /// <summary>
    /// Gets the metadata provider configurations of the media library identified by <paramref name="libraryId"/>.
    /// </summary>
    /// <param name="libraryId">The Id of the media library whose metadata provider configurations are retrieved.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="Result{TValue}"/> containing either the configurations, or an error.</returns>
    public async Task<Result<IReadOnlyList<LibraryMetadataProviderConfigurationEntity>>> GetConfigurationsAsync(Guid libraryId, CancellationToken cancellationToken)
    {
        return await _unitOfWork.LibraryMetadataProviderConfigurationRepository.GetByLibraryIdAsync(libraryId, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Adds a disabled metadata provider, artwork provider and book reader configuration for every loaded plugin that supports the provided library type and has no configuration yet
    /// for the media library identified by <paramref name="libraryId"/>.
    /// </summary>
    /// <remarks>
    /// The added configurations are only staged until the unit of work is saved, and are not visible to a later ensure, so it must be called at most once per media library before the changes are saved.
    /// </remarks>
    /// <param name="libraryId">The Id of the media library whose provider configurations are ensured.</param>
    /// <param name="libraryType">The type of the media library, used to determine the plugins whose providers apply.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="Result{TValue}"/> representing either a successful operation, or an error.</returns>
    public async Task<Result<Success>> EnsureProviderConfigurationsAsync(Guid libraryId, LibraryType libraryType, CancellationToken cancellationToken)
    {
        // "Ensure" is idempotent: it adds a configuration for every plugin that supports the library type and has none yet, but never removes any.
        // It is called when a library is created or when plugins are (re)loaded, so every applicable provider is present and can be enabled from the UI.
        (IReadOnlyList<Guid> metadataProviderPluginIds, IReadOnlyList<Guid> artworkProviderPluginIds) = GetSupportedProviderPluginIds(libraryType);
        Result<Success> ensureMetadataResult = await EnsureMetadataConfigurationsAsync(libraryId, metadataProviderPluginIds, cancellationToken).ConfigureAwait(false);
        if (ensureMetadataResult.IsFailure)
            return ensureMetadataResult;
        Result<Success> ensureArtworkResult = await EnsureArtworkConfigurationsAsync(libraryId, artworkProviderPluginIds, cancellationToken).ConfigureAwait(false);
        if (ensureArtworkResult.IsFailure)
            return ensureArtworkResult;
        return await EnsureBookReaderConfigurationsAsync(libraryId, libraryType, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Deletes all the provider configurations of the media library identified by <paramref name="libraryId"/>.
    /// </summary>
    /// <param name="libraryId">The Id of the media library whose provider configurations are deleted.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="Result{TValue}"/> representing either a successful operation, or an error.</returns>
    public async Task<Result<Deleted>> RemoveProviderConfigurationsForLibraryAsync(Guid libraryId, CancellationToken cancellationToken)
    {
        // The configurations would otherwise keep referencing a library that no longer exists, so they are all deleted with it.
        Result<Deleted> removeMetadataResult = await _unitOfWork.LibraryMetadataProviderConfigurationRepository.DeleteByLibraryIdAsync(libraryId, cancellationToken).ConfigureAwait(false);
        if (removeMetadataResult.IsFailure)
            return removeMetadataResult;
        Result<Deleted> removeArtworkResult = await _unitOfWork.ArtworkProviderConfigurationRepository.DeleteByLibraryIdAsync(libraryId, cancellationToken).ConfigureAwait(false);
        if (removeArtworkResult.IsFailure)
            return removeArtworkResult;
        Result<Deleted> removeBookReadersResult = await _unitOfWork.LibraryBookReaderConfigurationRepository.DeleteByLibraryIdAsync(libraryId, cancellationToken).ConfigureAwait(false);
        if (removeBookReadersResult.IsFailure)
            return removeBookReadersResult;
        // The reader enablements of the deleted library are no longer valid, so the reading service must not serve its books from the cache.
        _enablementCache.InvalidateLibrary(libraryId);
        return removeBookReadersResult;
    }

    /// <summary>
    /// Deletes all the provider configurations referencing the plugin identified by <paramref name="pluginId"/>.
    /// </summary>
    /// <param name="pluginId">The Id of the plugin whose provider configurations are deleted.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="Result{TValue}"/> representing either a successful operation, or an error.</returns>
    public async Task<Result<Deleted>> RemoveProviderConfigurationsAsync(Guid pluginId, CancellationToken cancellationToken)
    {
        // An uninstalled plugin can no longer serve any provider, so the configurations referencing it are deleted.
        Result<Deleted> removeMetadataResult = await _unitOfWork.LibraryMetadataProviderConfigurationRepository.DeleteByPluginIdAsync(pluginId, cancellationToken).ConfigureAwait(false);
        if (removeMetadataResult.IsFailure)
            return removeMetadataResult;
        Result<Deleted> removeArtworkResult = await _unitOfWork.ArtworkProviderConfigurationRepository.DeleteByPluginIdAsync(pluginId, cancellationToken).ConfigureAwait(false);
        if (removeArtworkResult.IsFailure)
            return removeArtworkResult;
        Result<Deleted> removeBookReadersResult = await _unitOfWork.LibraryBookReaderConfigurationRepository.DeleteByPluginIdAsync(pluginId, cancellationToken).ConfigureAwait(false);
        if (removeBookReadersResult.IsFailure)
            return removeBookReadersResult;
        // The reader enablements of the removed plugin are no longer valid, so the reading service must not serve its books from the cache.
        _enablementCache.InvalidatePlugin(pluginId);
        return removeBookReadersResult;
    }

    /// <summary>
    /// Makes the provider and book reader configurations of the media library identified by <paramref name="libraryId"/> match the plugins supporting the provided library type,
    /// and applies the choices made for them.
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
    public async Task<Result<Success>> ConfigureProvidersAsync(
        Guid libraryId,
        LibraryType libraryType,
        IReadOnlyList<LibraryProviderSelectionDto>? metadataProviders,
        IReadOnlyList<LibraryProviderSelectionDto>? artworkProviders,
        IReadOnlyList<LibraryProviderSelectionDto>? bookReaders,
        bool shouldReconcileOmittedKinds,
        CancellationToken cancellationToken)
    {
        // The configurations are computed in a single pass from the stored state, instead of reconciling first and applying the choices afterwards,
        // because the configurations added by a reconcile are only staged until the unit of work is saved, and a second upsert of the same plugin would not find them and would add a duplicate.
        (IReadOnlyList<Guid> metadataProviderPluginIds, IReadOnlyList<Guid> artworkProviderPluginIds) = GetSupportedProviderPluginIds(libraryType);
        if (metadataProviders is not null || shouldReconcileOmittedKinds)
        {
            Result<Success> configureMetadataResult = metadataProviders is null
                ? await ReconcileMetadataConfigurationsAsync(libraryId, metadataProviderPluginIds, cancellationToken).ConfigureAwait(false)
                : await ApplyMetadataProviderSelectionsAsync(libraryId, metadataProviderPluginIds, metadataProviders, cancellationToken).ConfigureAwait(false);
            if (configureMetadataResult.IsFailure)
                return configureMetadataResult;
        }
        if (artworkProviders is not null || shouldReconcileOmittedKinds)
        {
            Result<Success> configureArtworkResult = artworkProviders is null
                ? await ReconcileArtworkConfigurationsAsync(libraryId, artworkProviderPluginIds, cancellationToken).ConfigureAwait(false)
                : await ApplyArtworkProviderSelectionsAsync(libraryId, artworkProviderPluginIds, artworkProviders, cancellationToken).ConfigureAwait(false);
            if (configureArtworkResult.IsFailure)
                return configureArtworkResult;
        }
        // Reconciling the book readers drops the cached enablements of the library by itself.
        if (bookReaders is null)
            return shouldReconcileOmittedKinds ? await ReconcileBookReaderConfigurationsAsync(libraryId, libraryType, cancellationToken).ConfigureAwait(false) : Result.Success;
        Result<Success> configureBookReadersResult = await ApplyBookReaderSelectionsAsync(libraryId, GetSupportedBookReaderPluginIds(libraryType), bookReaders, cancellationToken).ConfigureAwait(false);
        if (configureBookReadersResult.IsFailure)
            return configureBookReadersResult;
        // The reading service caches the enablement of the readers, so the cached state of the library is dropped, and a reader that was disabled stops serving its books immediately.
        _enablementCache.InvalidateLibrary(libraryId);
        return Result.Success;
    }

    /// <summary>
    /// Gets the Ids of the loaded plugins that provide metadata or artwork for the provided library type, ordered alphabetically by plugin name so that the assigned ranks are stable.
    /// </summary>
    /// <param name="libraryType">The type of the media library the providers must support.</param>
    /// <returns>The Ids of the plugins providing metadata, and the Ids of the plugins providing artwork.</returns>
    private (IReadOnlyList<Guid> metadataProviderPluginIds, IReadOnlyList<Guid> artworkProviderPluginIds) GetSupportedProviderPluginIds(LibraryType libraryType)
    {
        // The plugins are ordered alphabetically by name so the resulting ranks are stable across restarts, and each plugin is tracked in
        // the metadata or artwork list independently, since it can support either, both, or neither.
        List<Guid> metadataProviderPluginIds = [];
        List<Guid> artworkProviderPluginIds = [];
        foreach (IPlugin plugin in _pluginManager.GetPlugins().OrderBy(plugin => plugin.Name))
        {
            // The providers are resolved as keyed services under the Id of their plugin, and the plugin supports the library type when at least one of its providers declares that type.
            if (_serviceProvider.GetKeyedServices<IMetadataProvider>(plugin.Id).Any(provider => provider.SupportedLibraryTypes.Contains(libraryType)))
                metadataProviderPluginIds.Add(plugin.Id);
            if (_serviceProvider.GetKeyedServices<IArtworkProvider>(plugin.Id).Any(provider => provider.SupportedLibraryTypes.Contains(libraryType)))
                artworkProviderPluginIds.Add(plugin.Id);
        }
        return (metadataProviderPluginIds, artworkProviderPluginIds);
    }

    /// <summary>
    /// Gets the Ids of the loaded plugins that provide metadata for the provided library type, ordered alphabetically by plugin name.
    /// </summary>
    /// <param name="libraryType">The type of the media library the metadata providers must support.</param>
    /// <returns>The Ids of the plugins providing metadata for the library type.</returns>
    public IReadOnlyList<Guid> GetSupportedMetadataProviderPluginIds(LibraryType libraryType)
    {
        return GetSupportedProviderPluginIds(libraryType).metadataProviderPluginIds;
    }

    /// <summary>
    /// Gets the Ids of the loaded plugins that provide artwork for the provided library type, ordered alphabetically by plugin name.
    /// </summary>
    /// <param name="libraryType">The type of the media library the artwork providers must support.</param>
    /// <returns>The Ids of the plugins providing artwork for the library type.</returns>
    public IReadOnlyList<Guid> GetSupportedArtworkProviderPluginIds(LibraryType libraryType)
    {
        return GetSupportedProviderPluginIds(libraryType).artworkProviderPluginIds;
    }

    /// <summary>
    /// Gets the Ids of the loaded plugins that provide a book reader for the provided library type, ordered alphabetically by plugin name so that the assigned order is stable.
    /// </summary>
    /// <param name="libraryType">The type of the media library the book readers must support.</param>
    /// <returns>The Ids of the plugins providing a book reader for the library type.</returns>
    public IReadOnlyList<Guid> GetSupportedBookReaderPluginIds(LibraryType libraryType)
    {
        // Book readers have no rank: a book is opened by exactly one reader matched by its file extension, not by priority, so the alphabetical order is only used to keep the display order stable.
        List<Guid> bookReaderPluginIds = [];
        foreach (IPlugin plugin in _pluginManager.GetPlugins().OrderBy(plugin => plugin.Name))
            // A reader that declares no file extension can never be asked to open a file, so it is not worth configuring.
            if (_serviceProvider.GetKeyedServices<IBookReader>(plugin.Id).Any(reader => reader.SupportedLibraryTypes.Contains(libraryType) && reader.SupportedExtensions.Count > 0))
                bookReaderPluginIds.Add(plugin.Id);
        return bookReaderPluginIds;
    }

    /// <summary>
    /// Replaces the book reader configurations of the media library identified by <paramref name="libraryId"/> so that they match the plugins supporting the provided library type.
    /// </summary>
    /// <remarks>
    /// The cached reader enablements of the library are dropped, so a reader whose configuration was removed stops serving its books immediately.
    /// </remarks>
    /// <param name="libraryId">The Id of the media library whose book reader configurations are reconciled.</param>
    /// <param name="libraryType">The type of the media library, used to determine the plugins whose book readers apply.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="Result{TValue}"/> representing either a successful operation, or an error.</returns>
    private async Task<Result<Success>> ReconcileBookReaderConfigurationsAsync(Guid libraryId, LibraryType libraryType, CancellationToken cancellationToken)
    {
        // First, the configurations of readers that no longer apply are deleted, then a disabled configuration is added for every reader
        // that applies, but is not configured yet; the configurations of readers that still apply are kept untouched.
        IReadOnlyList<Guid> bookReaderPluginIds = GetSupportedBookReaderPluginIds(libraryType);
        Result<IReadOnlyList<LibraryBookReaderConfigurationEntity>> getConfigurationsResult = await _unitOfWork.LibraryBookReaderConfigurationRepository.GetByLibraryIdAsync(libraryId, cancellationToken).ConfigureAwait(false);
        if (getConfigurationsResult.IsFailure)
            return Result<Success>.Failure(getConfigurationsResult.Errors);
        HashSet<Guid> supportedPluginIds = [.. bookReaderPluginIds];
        List<Guid> stalePluginIds = [.. getConfigurationsResult.Value
            .Select(configuration => configuration.PluginId)
            .Where(pluginId => !supportedPluginIds.Contains(pluginId))];
        if (stalePluginIds.Count > 0)
        {
            Result<Deleted> removeResult = await _unitOfWork.LibraryBookReaderConfigurationRepository.DeleteByLibraryIdAndPluginIdsAsync(libraryId, stalePluginIds, cancellationToken).ConfigureAwait(false);
            if (removeResult.IsFailure)
                return Result<Success>.Failure(removeResult.Errors);
        }
        HashSet<Guid> configuredPluginIds = [.. getConfigurationsResult.Value.Select(configuration => configuration.PluginId)];
        foreach (Guid pluginId in bookReaderPluginIds)
        {
            if (configuredPluginIds.Contains(pluginId))
                continue;
            // A newly discovered reader starts disabled so it never silently takes over reading the books of the library, and the audit
            // columns are left at their defaults because the persistence layer stamps them when the row is saved.
            LibraryBookReaderConfigurationEntity configuration = new()
            {
                Id = Guid.NewGuid(),
                LibraryId = libraryId,
                PluginId = pluginId,
                IsEnabled = false,
                CreatedOnUtc = default,
                CreatedBy = default,
                UpdatedBy = default
            };
            Result<Updated> upsertResult = await _unitOfWork.LibraryBookReaderConfigurationRepository.UpsertAsync(configuration, cancellationToken).ConfigureAwait(false);
            if (upsertResult.IsFailure)
                return Result<Success>.Failure(upsertResult.Errors);
        }
        // The reading service caches the enablement of the readers, so the cached state of the library is dropped, and a reader whose configuration was removed stops serving its books immediately.
        _enablementCache.InvalidateLibrary(libraryId);
        return Result.Success;
    }

    /// <summary>
    /// Adds a disabled book reader configuration for every loaded plugin that supports the provided library type and has no configuration yet, for the media library identified by <paramref name="libraryId"/>.
    /// </summary>
    /// <param name="libraryId">The Id of the media library whose book reader configurations are ensured.</param>
    /// <param name="libraryType">The type of the media library, used to determine the plugins whose book readers apply.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="Result{TValue}"/> representing either a successful operation, or an error.</returns>
    private async Task<Result<Success>> EnsureBookReaderConfigurationsAsync(Guid libraryId, LibraryType libraryType, CancellationToken cancellationToken)
    {
        // Unlike reconcile, ensure never deletes: it only adds a disabled configuration for the readers that are not configured yet.
        IReadOnlyList<Guid> bookReaderPluginIds = GetSupportedBookReaderPluginIds(libraryType);
        if (bookReaderPluginIds.Count == 0)
            return Result.Success;
        Result<IReadOnlyList<LibraryBookReaderConfigurationEntity>> getConfigurationsResult = await _unitOfWork.LibraryBookReaderConfigurationRepository.GetByLibraryIdAsync(libraryId, cancellationToken).ConfigureAwait(false);
        if (getConfigurationsResult.IsFailure)
            return Result<Success>.Failure(getConfigurationsResult.Errors);
        HashSet<Guid> configuredPluginIds = [.. getConfigurationsResult.Value.Select(configuration => configuration.PluginId)];
        foreach (Guid pluginId in bookReaderPluginIds)
        {
            if (configuredPluginIds.Contains(pluginId))
                continue;
            // A newly discovered reader starts disabled so it never silently takes over reading the books of the library, and the audit
            // columns are left at their defaults because the persistence layer stamps them when the row is saved.
            LibraryBookReaderConfigurationEntity configuration = new()
            {
                Id = Guid.NewGuid(),
                LibraryId = libraryId,
                PluginId = pluginId,
                IsEnabled = false,
                CreatedOnUtc = default,
                CreatedBy = default,
                UpdatedBy = default
            };
            Result<Updated> upsertResult = await _unitOfWork.LibraryBookReaderConfigurationRepository.UpsertAsync(configuration, cancellationToken).ConfigureAwait(false);
            if (upsertResult.IsFailure)
                return Result<Success>.Failure(upsertResult.Errors);
        }
        return Result.Success;
    }

    /// <summary>
    /// Removes the metadata provider configurations of the plugins that no longer support the library type, and adds a disabled
    /// configuration for every plugin that supports it and has none yet, keeping the configurations of the plugins that still apply.
    /// </summary>
    /// <param name="libraryId">The Id of the media library whose metadata provider configurations are reconciled.</param>
    /// <param name="metadataProviderPluginIds">The Ids of the plugins providing metadata for the library type, in rank order.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="Result{TValue}"/> representing either a successful operation, or an error.</returns>
    private async Task<Result<Success>> ReconcileMetadataConfigurationsAsync(Guid libraryId, IReadOnlyList<Guid> metadataProviderPluginIds, CancellationToken cancellationToken)
    {
        Result<IReadOnlyList<LibraryMetadataProviderConfigurationEntity>> getConfigurationsResult = await _unitOfWork.LibraryMetadataProviderConfigurationRepository.GetByLibraryIdAsync(libraryId, cancellationToken).ConfigureAwait(false);
        if (getConfigurationsResult.IsFailure)
            return Result<Success>.Failure(getConfigurationsResult.Errors);
        HashSet<Guid> supportedPluginIds = [.. metadataProviderPluginIds];
        List<Guid> stalePluginIds = [.. getConfigurationsResult.Value
            .Select(configuration => configuration.PluginId)
            .Where(pluginId => !supportedPluginIds.Contains(pluginId))];
        if (stalePluginIds.Count > 0)
        {
            // The stale configurations are deleted first, so the next free rank is computed on the configurations that remain.
            Result<Deleted> removeResult = await _unitOfWork.LibraryMetadataProviderConfigurationRepository.DeleteByLibraryIdAndPluginIdsAsync(libraryId, stalePluginIds, cancellationToken).ConfigureAwait(false);
            if (removeResult.IsFailure)
                return Result<Success>.Failure(removeResult.Errors);
        }
        HashSet<Guid> configuredPluginIds = [.. getConfigurationsResult.Value.Select(configuration => configuration.PluginId)];
        // The metadata providers of a library are ordered by an explicit 1-based rank that decides which provider is consulted first when
        // enriching a book; new providers are appended after the current highest rank so the priority of the existing ones is preserved.
        int nextRank = getConfigurationsResult.Value.Count == 0 ? 1 : getConfigurationsResult.Value.Max(configuration => configuration.Rank) + 1;
        foreach (Guid pluginId in metadataProviderPluginIds)
        {
            if (configuredPluginIds.Contains(pluginId))
                continue;
            // A newly discovered provider starts disabled, so it never silently starts enriching the library until the user enables it, and
            // the audit columns are left at their defaults because the persistence layer stamps them when the row is saved.
            LibraryMetadataProviderConfigurationEntity configuration = new()
            {
                Id = Guid.NewGuid(),
                LibraryId = libraryId,
                PluginId = pluginId,
                IsEnabled = false,
                Rank = nextRank,
                CreatedOnUtc = default,
                CreatedBy = default,
                UpdatedBy = default
            };
            Result<Updated> upsertResult = await _unitOfWork.LibraryMetadataProviderConfigurationRepository.UpsertAsync(configuration, cancellationToken).ConfigureAwait(false);
            if (upsertResult.IsFailure)
                return Result<Success>.Failure(upsertResult.Errors);
            nextRank++;
        }
        return Result.Success;
    }

    /// <summary>
    /// Removes the artwork provider configurations of the plugins that no longer support the library type, and adds a disabled
    /// configuration for every plugin that supports it and has none yet, keeping the configurations of the plugins that still apply.
    /// </summary>
    /// <param name="libraryId">The Id of the media library whose artwork provider configurations are reconciled.</param>
    /// <param name="artworkProviderPluginIds">The Ids of the plugins providing artwork for the library type, in rank order.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="Result{TValue}"/> representing either a successful operation, or an error.</returns>
    private async Task<Result<Success>> ReconcileArtworkConfigurationsAsync(Guid libraryId, IReadOnlyList<Guid> artworkProviderPluginIds, CancellationToken cancellationToken)
    {
        Result<IReadOnlyList<LibraryArtworkProviderConfigurationEntity>> getConfigurationsResult = await _unitOfWork.ArtworkProviderConfigurationRepository.GetByLibraryIdAsync(libraryId, cancellationToken).ConfigureAwait(false);
        if (getConfigurationsResult.IsFailure)
            return Result<Success>.Failure(getConfigurationsResult.Errors);
        HashSet<Guid> supportedPluginIds = [.. artworkProviderPluginIds];
        List<Guid> stalePluginIds = [.. getConfigurationsResult.Value
            .Select(configuration => configuration.PluginId)
            .Where(pluginId => !supportedPluginIds.Contains(pluginId))];
        if (stalePluginIds.Count > 0)
        {
            // The stale configurations are deleted first, so the next free rank is computed on the configurations that remain.
            Result<Deleted> removeResult = await _unitOfWork.ArtworkProviderConfigurationRepository.DeleteByLibraryIdAndPluginIdsAsync(libraryId, stalePluginIds, cancellationToken).ConfigureAwait(false);
            if (removeResult.IsFailure)
                return Result<Success>.Failure(removeResult.Errors);
        }
        HashSet<Guid> configuredPluginIds = [.. getConfigurationsResult.Value.Select(configuration => configuration.PluginId)];
        // Same rank logic as the metadata providers: artwork providers are ordered by an explicit 1-based rank that decides which
        // provider is consulted first, and new providers are appended after the current highest rank.
        int nextRank = getConfigurationsResult.Value.Count == 0 ? 1 : getConfigurationsResult.Value.Max(configuration => configuration.Rank) + 1;
        foreach (Guid pluginId in artworkProviderPluginIds)
        {
            if (configuredPluginIds.Contains(pluginId))
                continue;
            // A newly discovered provider starts disabled, so it never silently starts fetching artwork until the user enables it, and the
            // audit columns are left at their defaults because the persistence layer stamps them when the row is saved.
            LibraryArtworkProviderConfigurationEntity configuration = new()
            {
                Id = Guid.NewGuid(),
                LibraryId = libraryId,
                PluginId = pluginId,
                IsEnabled = false,
                Rank = nextRank,
                CreatedOnUtc = default,
                CreatedBy = default,
                UpdatedBy = default
            };
            Result<Updated> upsertResult = await _unitOfWork.ArtworkProviderConfigurationRepository.UpsertAsync(configuration, cancellationToken).ConfigureAwait(false);
            if (upsertResult.IsFailure)
                return Result<Success>.Failure(upsertResult.Errors);
            nextRank++;
        }
        return Result.Success;
    }

    /// <summary>
    /// Adds a disabled metadata provider configuration for every provided plugin that has no configuration yet for the media library.
    /// </summary>
    /// <param name="libraryId">The Id of the media library whose metadata provider configurations are ensured.</param>
    /// <param name="metadataProviderPluginIds">The Ids of the plugins providing metadata for the library type, in rank order.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="Result{TValue}"/> representing either a successful operation, or an error.</returns>
    private async Task<Result<Success>> EnsureMetadataConfigurationsAsync(Guid libraryId, IReadOnlyList<Guid> metadataProviderPluginIds, CancellationToken cancellationToken)
    {
        if (metadataProviderPluginIds.Count == 0)
            return Result.Success;
        Result<IReadOnlyList<LibraryMetadataProviderConfigurationEntity>> getConfigurationsResult = await _unitOfWork.LibraryMetadataProviderConfigurationRepository.GetByLibraryIdAsync(libraryId, cancellationToken).ConfigureAwait(false);
        if (getConfigurationsResult.IsFailure)
            return Result<Success>.Failure(getConfigurationsResult.Errors);
        HashSet<Guid> configuredPluginIds = [.. getConfigurationsResult.Value.Select(configuration => configuration.PluginId)];
        // New providers are appended after the current highest rank, and only added when missing: unlike reconcile, ensure never deletes.
        int nextRank = getConfigurationsResult.Value.Count == 0 ? 1 : getConfigurationsResult.Value.Max(configuration => configuration.Rank) + 1;
        foreach (Guid pluginId in metadataProviderPluginIds)
        {
            if (configuredPluginIds.Contains(pluginId))
                continue;
            // A newly discovered provider starts disabled, so it never silently starts enriching the library until the user enables it, and
            // the audit columns are left at their defaults because the persistence layer stamps them when the row is saved.
            LibraryMetadataProviderConfigurationEntity configuration = new()
            {
                Id = Guid.NewGuid(),
                LibraryId = libraryId,
                PluginId = pluginId,
                IsEnabled = false,
                Rank = nextRank,
                CreatedOnUtc = default,
                CreatedBy = default,
                UpdatedBy = default
            };
            Result<Updated> upsertResult = await _unitOfWork.LibraryMetadataProviderConfigurationRepository.UpsertAsync(configuration, cancellationToken).ConfigureAwait(false);
            if (upsertResult.IsFailure)
                return Result<Success>.Failure(upsertResult.Errors);
            nextRank++;
        }
        return Result.Success;
    }

    /// <summary>
    /// Adds a disabled artwork provider configuration for every provided plugin that has no configuration yet for the media library.
    /// </summary>
    /// <param name="libraryId">The Id of the media library whose artwork provider configurations are ensured.</param>
    /// <param name="artworkProviderPluginIds">The Ids of the plugins providing artwork for the library type, in rank order.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="Result{TValue}"/> representing either a successful operation, or an error.</returns>
    private async Task<Result<Success>> EnsureArtworkConfigurationsAsync(Guid libraryId, IReadOnlyList<Guid> artworkProviderPluginIds, CancellationToken cancellationToken)
    {
        if (artworkProviderPluginIds.Count == 0)
            return Result.Success;
        Result<IReadOnlyList<LibraryArtworkProviderConfigurationEntity>> getConfigurationsResult = await _unitOfWork.ArtworkProviderConfigurationRepository.GetByLibraryIdAsync(libraryId, cancellationToken).ConfigureAwait(false);
        if (getConfigurationsResult.IsFailure)
            return Result<Success>.Failure(getConfigurationsResult.Errors);
        HashSet<Guid> configuredPluginIds = [.. getConfigurationsResult.Value.Select(configuration => configuration.PluginId)];
        // New providers are appended after the current highest rank, and only added when missing: unlike reconcile, ensure never deletes.
        int nextRank = getConfigurationsResult.Value.Count == 0 ? 1 : getConfigurationsResult.Value.Max(configuration => configuration.Rank) + 1;
        foreach (Guid pluginId in artworkProviderPluginIds)
        {
            if (configuredPluginIds.Contains(pluginId))
                continue;
            // A newly discovered provider starts disabled, so it never silently starts fetching artwork until the user enables it, and the
            // audit columns are left at their defaults because the persistence layer stamps them when the row is saved.
            LibraryArtworkProviderConfigurationEntity configuration = new()
            {
                Id = Guid.NewGuid(),
                LibraryId = libraryId,
                PluginId = pluginId,
                IsEnabled = false,
                Rank = nextRank,
                CreatedOnUtc = default,
                CreatedBy = default,
                UpdatedBy = default
            };
            Result<Updated> upsertResult = await _unitOfWork.ArtworkProviderConfigurationRepository.UpsertAsync(configuration, cancellationToken).ConfigureAwait(false);
            if (upsertResult.IsFailure)
                return Result<Success>.Failure(upsertResult.Errors);
            nextRank++;
        }
        return Result.Success;
    }

    /// <summary>
    /// Makes the metadata provider configurations of the media library match the plugins supporting its type, and applies the provided choices.
    /// </summary>
    /// <param name="libraryId">The Id of the media library whose metadata provider configurations are configured.</param>
    /// <param name="metadataProviderPluginIds">The Ids of the plugins providing metadata for the library type, in alphabetical order.</param>
    /// <param name="selections">The choices made for the metadata providers, in the order in which they are tried.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="Result{TValue}"/> representing either a successful operation, or an error.</returns>
    private async Task<Result<Success>> ApplyMetadataProviderSelectionsAsync(Guid libraryId, IReadOnlyList<Guid> metadataProviderPluginIds, IReadOnlyList<LibraryProviderSelectionDto> selections, CancellationToken cancellationToken)
    {
        // The command validation rejects duplicated choices already, and they are reported here too, so that a direct caller gets an error instead of an exception.
        if (selections.Select(selection => selection.PluginId).Distinct().Count() != selections.Count)
            return Errors.Plugins.PluginIdsMustBeUnique;
        if (selections.Any(selection => !metadataProviderPluginIds.Contains(selection.PluginId)))
            return Errors.Plugins.PluginNotSupportedForLibraryType;
        Result<IReadOnlyList<LibraryMetadataProviderConfigurationEntity>> getConfigurationsResult = await _unitOfWork.LibraryMetadataProviderConfigurationRepository.GetByLibraryIdAsync(libraryId, cancellationToken).ConfigureAwait(false);
        if (getConfigurationsResult.IsFailure)
            return Result<Success>.Failure(getConfigurationsResult.Errors);
        List<Guid> stalePluginIds = [.. getConfigurationsResult.Value
            .Select(configuration => configuration.PluginId)
            .Where(pluginId => !metadataProviderPluginIds.Contains(pluginId))];
        if (stalePluginIds.Count > 0)
        {
            Result<Deleted> removeResult = await _unitOfWork.LibraryMetadataProviderConfigurationRepository.DeleteByLibraryIdAndPluginIdsAsync(libraryId, stalePluginIds, cancellationToken).ConfigureAwait(false);
            if (removeResult.IsFailure)
                return Result<Success>.Failure(removeResult.Errors);
        }
        Dictionary<Guid, LibraryMetadataProviderConfigurationEntity> configurationsByPluginId = getConfigurationsResult.Value
            .Where(configuration => metadataProviderPluginIds.Contains(configuration.PluginId))
            .ToDictionary(configuration => configuration.PluginId);
        IReadOnlyList<Guid> orderedPluginIds = GetProviderOrder(selections, configurationsByPluginId.Values.OrderBy(configuration => configuration.Rank).Select(configuration => configuration.PluginId), metadataProviderPluginIds);
        Dictionary<Guid, bool?> enabledStatesByPluginId = selections.ToDictionary(selection => selection.PluginId, selection => selection.IsEnabled);
        for (int index = 0; index < orderedPluginIds.Count; index++)
        {
            Guid pluginId = orderedPluginIds[index];
            configurationsByPluginId.TryGetValue(pluginId, out LibraryMetadataProviderConfigurationEntity? storedConfiguration);
            // A provider that is not named, or is named without an enabled state, keeps its stored state, and a provider that has no configuration yet starts disabled, so it never silently starts enriching the library.
            bool isEnabled = enabledStatesByPluginId.GetValueOrDefault(pluginId) ?? storedConfiguration?.IsEnabled ?? false;
            LibraryMetadataProviderConfigurationEntity configuration = storedConfiguration ?? new LibraryMetadataProviderConfigurationEntity
            {
                Id = Guid.NewGuid(),
                LibraryId = libraryId,
                PluginId = pluginId,
                IsEnabled = isEnabled,
                Rank = index + 1,
                CreatedOnUtc = default,
                CreatedBy = default,
                UpdatedBy = default
            };
            configuration.IsEnabled = isEnabled;
            configuration.Rank = index + 1;
            Result<Updated> upsertResult = await _unitOfWork.LibraryMetadataProviderConfigurationRepository.UpsertAsync(configuration, cancellationToken).ConfigureAwait(false);
            if (upsertResult.IsFailure)
                return Result<Success>.Failure(upsertResult.Errors);
        }
        return Result.Success;
    }

    /// <summary>
    /// Makes the artwork provider configurations of the media library match the plugins supporting its type, and applies the provided choices.
    /// </summary>
    /// <param name="libraryId">The Id of the media library whose artwork provider configurations are configured.</param>
    /// <param name="artworkProviderPluginIds">The Ids of the plugins providing artwork for the library type, in alphabetical order.</param>
    /// <param name="selections">The choices made for the artwork providers, in the order in which they are tried.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="Result{TValue}"/> representing either a successful operation, or an error.</returns>
    private async Task<Result<Success>> ApplyArtworkProviderSelectionsAsync(Guid libraryId, IReadOnlyList<Guid> artworkProviderPluginIds, IReadOnlyList<LibraryProviderSelectionDto> selections, CancellationToken cancellationToken)
    {
        // The command validation rejects duplicated choices already, and they are reported here too, so that a direct caller gets an error instead of an exception.
        if (selections.Select(selection => selection.PluginId).Distinct().Count() != selections.Count)
            return Errors.Plugins.PluginIdsMustBeUnique;
        if (selections.Any(selection => !artworkProviderPluginIds.Contains(selection.PluginId)))
            return Errors.Plugins.PluginNotSupportedForLibraryType;
        Result<IReadOnlyList<LibraryArtworkProviderConfigurationEntity>> getConfigurationsResult = await _unitOfWork.ArtworkProviderConfigurationRepository.GetByLibraryIdAsync(libraryId, cancellationToken).ConfigureAwait(false);
        if (getConfigurationsResult.IsFailure)
            return Result<Success>.Failure(getConfigurationsResult.Errors);
        List<Guid> stalePluginIds = [.. getConfigurationsResult.Value
            .Select(configuration => configuration.PluginId)
            .Where(pluginId => !artworkProviderPluginIds.Contains(pluginId))];
        if (stalePluginIds.Count > 0)
        {
            Result<Deleted> removeResult = await _unitOfWork.ArtworkProviderConfigurationRepository.DeleteByLibraryIdAndPluginIdsAsync(libraryId, stalePluginIds, cancellationToken).ConfigureAwait(false);
            if (removeResult.IsFailure)
                return Result<Success>.Failure(removeResult.Errors);
        }
        Dictionary<Guid, LibraryArtworkProviderConfigurationEntity> configurationsByPluginId = getConfigurationsResult.Value
            .Where(configuration => artworkProviderPluginIds.Contains(configuration.PluginId))
            .ToDictionary(configuration => configuration.PluginId);
        IReadOnlyList<Guid> orderedPluginIds = GetProviderOrder(selections, configurationsByPluginId.Values.OrderBy(configuration => configuration.Rank).Select(configuration => configuration.PluginId), artworkProviderPluginIds);
        Dictionary<Guid, bool?> enabledStatesByPluginId = selections.ToDictionary(selection => selection.PluginId, selection => selection.IsEnabled);
        for (int index = 0; index < orderedPluginIds.Count; index++)
        {
            Guid pluginId = orderedPluginIds[index];
            configurationsByPluginId.TryGetValue(pluginId, out LibraryArtworkProviderConfigurationEntity? storedConfiguration);
            // A provider that is not named, or is named without an enabled state, keeps its stored state, and a provider that has no configuration yet starts disabled, so it never silently starts enriching the library.
            bool isEnabled = enabledStatesByPluginId.GetValueOrDefault(pluginId) ?? storedConfiguration?.IsEnabled ?? false;
            LibraryArtworkProviderConfigurationEntity configuration = storedConfiguration ?? new LibraryArtworkProviderConfigurationEntity
            {
                Id = Guid.NewGuid(),
                LibraryId = libraryId,
                PluginId = pluginId,
                IsEnabled = isEnabled,
                Rank = index + 1,
                CreatedOnUtc = default,
                CreatedBy = default,
                UpdatedBy = default
            };
            configuration.IsEnabled = isEnabled;
            configuration.Rank = index + 1;
            Result<Updated> upsertResult = await _unitOfWork.ArtworkProviderConfigurationRepository.UpsertAsync(configuration, cancellationToken).ConfigureAwait(false);
            if (upsertResult.IsFailure)
                return Result<Success>.Failure(upsertResult.Errors);
        }
        return Result.Success;
    }

    /// <summary>
    /// Makes the book reader configurations of the media library match the plugins supporting its type, and applies the provided choices.
    /// </summary>
    /// <param name="libraryId">The Id of the media library whose book reader configurations are configured.</param>
    /// <param name="bookReaderPluginIds">The Ids of the plugins providing a book reader for the library type.</param>
    /// <param name="selections">The choices made for the book readers.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="Result{TValue}"/> representing either a successful operation, or an error.</returns>
    private async Task<Result<Success>> ApplyBookReaderSelectionsAsync(Guid libraryId, IReadOnlyList<Guid> bookReaderPluginIds, IReadOnlyList<LibraryProviderSelectionDto> selections, CancellationToken cancellationToken)
    {
        // The command validation rejects duplicated choices already, and they are reported here too, so that a direct caller gets an error instead of an exception.
        if (selections.Select(selection => selection.PluginId).Distinct().Count() != selections.Count)
            return Errors.Plugins.PluginIdsMustBeUnique;
        if (selections.Any(selection => !bookReaderPluginIds.Contains(selection.PluginId)))
            return Errors.Plugins.PluginNotSupportedForLibraryType;
        Result<IReadOnlyList<LibraryBookReaderConfigurationEntity>> getConfigurationsResult = await _unitOfWork.LibraryBookReaderConfigurationRepository.GetByLibraryIdAsync(libraryId, cancellationToken).ConfigureAwait(false);
        if (getConfigurationsResult.IsFailure)
            return Result<Success>.Failure(getConfigurationsResult.Errors);
        List<Guid> stalePluginIds = [.. getConfigurationsResult.Value
            .Select(configuration => configuration.PluginId)
            .Where(pluginId => !bookReaderPluginIds.Contains(pluginId))];
        if (stalePluginIds.Count > 0)
        {
            Result<Deleted> removeResult = await _unitOfWork.LibraryBookReaderConfigurationRepository.DeleteByLibraryIdAndPluginIdsAsync(libraryId, stalePluginIds, cancellationToken).ConfigureAwait(false);
            if (removeResult.IsFailure)
                return Result<Success>.Failure(removeResult.Errors);
        }
        Dictionary<Guid, LibraryBookReaderConfigurationEntity> configurationsByPluginId = getConfigurationsResult.Value
            .Where(configuration => bookReaderPluginIds.Contains(configuration.PluginId))
            .ToDictionary(configuration => configuration.PluginId);
        Dictionary<Guid, bool?> enabledStatesByPluginId = selections.ToDictionary(selection => selection.PluginId, selection => selection.IsEnabled);
        // Book readers have no rank, since a book is opened by the reader matched by its file extension, so only their enabled state is applied.
        foreach (Guid pluginId in bookReaderPluginIds)
        {
            configurationsByPluginId.TryGetValue(pluginId, out LibraryBookReaderConfigurationEntity? storedConfiguration);
            // A reader that is not named, or is named without an enabled state, keeps its stored state, and a reader that has no configuration yet starts disabled, so it never silently takes over reading the books of the library.
            bool isEnabled = enabledStatesByPluginId.GetValueOrDefault(pluginId) ?? storedConfiguration?.IsEnabled ?? false;
            LibraryBookReaderConfigurationEntity configuration = storedConfiguration ?? new LibraryBookReaderConfigurationEntity
            {
                Id = Guid.NewGuid(),
                LibraryId = libraryId,
                PluginId = pluginId,
                IsEnabled = isEnabled,
                CreatedOnUtc = default,
                CreatedBy = default,
                UpdatedBy = default
            };
            configuration.IsEnabled = isEnabled;
            Result<Updated> upsertResult = await _unitOfWork.LibraryBookReaderConfigurationRepository.UpsertAsync(configuration, cancellationToken).ConfigureAwait(false);
            if (upsertResult.IsFailure)
                return Result<Success>.Failure(upsertResult.Errors);
        }
        return Result.Success;
    }

    /// <summary>
    /// Gets the order in which the supported providers of a media library are tried: the named providers in the order of the choices, then the providers that are already configured,
    /// in their stored order, then the providers that are not configured yet, in alphabetical order.
    /// </summary>
    /// <param name="selections">The choices made for the providers, in the order in which they are tried.</param>
    /// <param name="configuredPluginIds">The Ids of the plugins whose providers are already configured and still supported, in their stored order.</param>
    /// <param name="supportedPluginIds">The Ids of the plugins providing for the library type, in alphabetical order.</param>
    /// <returns>The Ids of the plugins, in the order in which their providers are tried.</returns>
    private static IReadOnlyList<Guid> GetProviderOrder(IReadOnlyList<LibraryProviderSelectionDto> selections, IEnumerable<Guid> configuredPluginIds, IReadOnlyList<Guid> supportedPluginIds)
    {
        List<Guid> orderedPluginIds = [.. selections.Select(selection => selection.PluginId)];
        HashSet<Guid> orderedPluginIdSet = [.. orderedPluginIds];
        foreach (Guid pluginId in configuredPluginIds.Concat(supportedPluginIds))
            if (orderedPluginIdSet.Add(pluginId))
                orderedPluginIds.Add(pluginId);
        return orderedPluginIds;
    }
}
