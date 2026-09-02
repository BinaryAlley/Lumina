#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.Plugins;
using Lumina.Application.Common.DataAccess.UoW;
using Lumina.Application.Common.Infrastructure.Plugins;
using Lumina.Application.Common.Infrastructure.Reading;
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
/// plugins can change between restarts. This class is the single place that maintains them, and the only one that knows the
/// difference between "ensure" (only add what is missing) and "reconcile" (make the persisted configurations match the loaded
/// plugins exactly).
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
    /// Adds a disabled provider configuration for every loaded plugin that supports the provided library type and has no configuration yet for the media library identified by <paramref name="libraryId"/>.
    /// </summary>
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
    /// Replaces the provider configurations of the media library identified by <paramref name="libraryId"/> so that they match the plugins supporting the provided library type.
    /// </summary>
    /// <param name="libraryId">The Id of the media library whose provider configurations are reconciled.</param>
    /// <param name="libraryType">The type of the media library, used to determine the plugins whose providers apply.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="Result{TValue}"/> representing either a successful operation, or an error.</returns>
    public async Task<Result<Success>> ReconcileProviderConfigurationsAsync(Guid libraryId, LibraryType libraryType, CancellationToken cancellationToken)
    {
        // "Reconcile" makes the persisted configurations match the loaded plugins exactly: configurations of plugins that no longer support
        // the library type are removed, configurations for newly supported plugins are added, and the rest are kept. It is called when a
        // plugin is uninstalled or when the type of a library changes.
        (IReadOnlyList<Guid> metadataProviderPluginIds, IReadOnlyList<Guid> artworkProviderPluginIds) = GetSupportedProviderPluginIds(libraryType);
        Result<Success> reconcileMetadataResult = await ReconcileMetadataConfigurationsAsync(libraryId, metadataProviderPluginIds, cancellationToken).ConfigureAwait(false);
        if (reconcileMetadataResult.IsFailure)
            return reconcileMetadataResult;
        Result<Success> reconcileArtworkResult = await ReconcileArtworkConfigurationsAsync(libraryId, artworkProviderPluginIds, cancellationToken).ConfigureAwait(false);
        if (reconcileArtworkResult.IsFailure)
            return reconcileArtworkResult;
        return await ReconcileBookReaderConfigurationsAsync(libraryId, libraryType, cancellationToken).ConfigureAwait(false);
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
    /// Deletes all the book reader configurations of the media library identified by <paramref name="libraryId"/>.
    /// </summary>
    /// <param name="libraryId">The Id of the media library whose book reader configurations are deleted.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="Result{TValue}"/> representing either a successful operation, or an error.</returns>
    public async Task<Result<Deleted>> RemoveBookReaderConfigurationsForLibraryAsync(Guid libraryId, CancellationToken cancellationToken)
    {
        Result<Deleted> removeResult = await _unitOfWork.LibraryBookReaderConfigurationRepository.DeleteByLibraryIdAsync(libraryId, cancellationToken).ConfigureAwait(false);
        if (removeResult.IsFailure)
            return removeResult;
        // The reader enablements of the deleted library are no longer valid, so the reading service must not serve its books from the cache.
        _enablementCache.InvalidateLibrary(libraryId);
        return removeResult;
    }

    /// <summary>
    /// Deletes all the book reader configurations referencing the plugin identified by <paramref name="pluginId"/>.
    /// </summary>
    /// <param name="pluginId">The Id of the plugin whose book reader configurations are deleted.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="Result{TValue}"/> representing either a successful operation, or an error.</returns>
    public async Task<Result<Deleted>> RemoveBookReaderConfigurationsAsync(Guid pluginId, CancellationToken cancellationToken)
    {
        Result<Deleted> removeResult = await _unitOfWork.LibraryBookReaderConfigurationRepository.DeleteByPluginIdAsync(pluginId, cancellationToken).ConfigureAwait(false);
        if (removeResult.IsFailure)
            return removeResult;
        // The reader enablements of the removed plugin are no longer valid, so the reading service must not serve its books from the cache.
        _enablementCache.InvalidatePlugin(pluginId);
        return removeResult;
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
    /// Gets the Ids of the loaded plugins that provide a book reader for the provided library type, ordered alphabetically by plugin name so that the assigned order is stable.
    /// </summary>
    /// <param name="libraryType">The type of the media library the book readers must support.</param>
    /// <returns>The Ids of the plugins providing a book reader for the library type.</returns>
    private IReadOnlyList<Guid> GetSupportedBookReaderPluginIds(LibraryType libraryType)
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
    /// <param name="libraryId">The Id of the media library whose book reader configurations are reconciled.</param>
    /// <param name="libraryType">The type of the media library, used to determine the plugins whose book readers apply.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="Result{TValue}"/> representing either a successful operation, or an error.</returns>
    public async Task<Result<Success>> ReconcileBookReaderConfigurationsAsync(Guid libraryId, LibraryType libraryType, CancellationToken cancellationToken)
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
        return Result.Success;
    }

    /// <summary>
    /// Adds a disabled book reader configuration for every loaded plugin that supports the provided library type and has no configuration yet, for the media library identified by <paramref name="libraryId"/>.
    /// </summary>
    /// <param name="libraryId">The Id of the media library whose book reader configurations are ensured.</param>
    /// <param name="libraryType">The type of the media library, used to determine the plugins whose book readers apply.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="Result{TValue}"/> representing either a successful operation, or an error.</returns>
    public async Task<Result<Success>> EnsureBookReaderConfigurationsAsync(Guid libraryId, LibraryType libraryType, CancellationToken cancellationToken)
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
}
