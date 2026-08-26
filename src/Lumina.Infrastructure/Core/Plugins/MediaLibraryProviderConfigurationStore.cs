#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.Plugins;
using Lumina.Application.Common.DataAccess.UoW;
using Lumina.Application.Common.Infrastructure.Plugins;
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.SharedKernel.Common.Enums.MediaLibrary;
using Lumina.Plugins.Contracts.Core.Metadata;
using Lumina.Plugins.Contracts.Core.Plugins;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Infrastructure.Core.Plugins;

/// <summary>
/// Store of the metadata and artwork provider configurations of the media libraries.
/// </summary>
internal sealed class MediaLibraryProviderConfigurationStore : IMediaLibraryProviderConfigurationStore
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPluginManager _pluginManager;
    private readonly IServiceProvider _serviceProvider;

    /// <summary>
    /// Initializes a new instance of the <see cref="MediaLibraryProviderConfigurationStore"/> class.
    /// </summary>
    /// <param name="unitOfWork">Injected unit of work for interacting with the data access layer repositories.</param>
    /// <param name="pluginManager">Injected manager of the plugins loaded by the host application.</param>
    /// <param name="serviceProvider">Injected provider used to resolve the providers registered by the plugins.</param>
    public MediaLibraryProviderConfigurationStore(IUnitOfWork unitOfWork, IPluginManager pluginManager, IServiceProvider serviceProvider)
    {
        _unitOfWork = unitOfWork;
        _pluginManager = pluginManager;
        _serviceProvider = serviceProvider;
    }

    /// <inheritdoc/>
    public async Task<Result<IReadOnlyList<LibraryMetadataProviderConfigurationEntity>>> GetConfigurationsAsync(Guid libraryId, CancellationToken cancellationToken)
    {
        return await _unitOfWork.LibraryMetadataProviderConfigurationRepository.GetByLibraryIdAsync(libraryId, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async Task<Result<Success>> EnsureProviderConfigurationsAsync(Guid libraryId, LibraryType libraryType, CancellationToken cancellationToken)
    {
        (IReadOnlyList<Guid> metadataProviderPluginIds, IReadOnlyList<Guid> artworkProviderPluginIds) = GetSupportedProviderPluginIds(libraryType);
        Result<Success> ensureMetadataResult = await EnsureMetadataConfigurationsAsync(libraryId, metadataProviderPluginIds, cancellationToken).ConfigureAwait(false);
        if (ensureMetadataResult.IsFailure)
            return ensureMetadataResult;
        return await EnsureArtworkConfigurationsAsync(libraryId, artworkProviderPluginIds, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async Task<Result<Success>> ReconcileProviderConfigurationsAsync(Guid libraryId, LibraryType libraryType, CancellationToken cancellationToken)
    {
        (IReadOnlyList<Guid> metadataProviderPluginIds, IReadOnlyList<Guid> artworkProviderPluginIds) = GetSupportedProviderPluginIds(libraryType);
        Result<Success> reconcileMetadataResult = await ReconcileMetadataConfigurationsAsync(libraryId, metadataProviderPluginIds, cancellationToken).ConfigureAwait(false);
        if (reconcileMetadataResult.IsFailure)
            return reconcileMetadataResult;
        return await ReconcileArtworkConfigurationsAsync(libraryId, artworkProviderPluginIds, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async Task<Result<Deleted>> RemoveProviderConfigurationsForLibraryAsync(Guid libraryId, CancellationToken cancellationToken)
    {
        Result<Deleted> removeMetadataResult = await _unitOfWork.LibraryMetadataProviderConfigurationRepository.DeleteByLibraryIdAsync(libraryId, cancellationToken).ConfigureAwait(false);
        if (removeMetadataResult.IsFailure)
            return removeMetadataResult;
        return await _unitOfWork.ArtworkProviderConfigurationRepository.DeleteByLibraryIdAsync(libraryId, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async Task<Result<Deleted>> RemoveProviderConfigurationsAsync(Guid pluginId, CancellationToken cancellationToken)
    {
        Result<Deleted> removeMetadataResult = await _unitOfWork.LibraryMetadataProviderConfigurationRepository.DeleteByPluginIdAsync(pluginId, cancellationToken).ConfigureAwait(false);
        if (removeMetadataResult.IsFailure)
            return removeMetadataResult;
        return await _unitOfWork.ArtworkProviderConfigurationRepository.DeleteByPluginIdAsync(pluginId, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Gets the Ids of the loaded plugins that provide metadata or artwork for the provided library type,
    /// ordered alphabetically by plugin name so that the assigned ranks are stable.
    /// </summary>
    /// <param name="libraryType">The type of the media library the providers must support.</param>
    /// <returns>The Ids of the plugins providing metadata, and the Ids of the plugins providing artwork.</returns>
    private (IReadOnlyList<Guid> metadataProviderPluginIds, IReadOnlyList<Guid> artworkProviderPluginIds) GetSupportedProviderPluginIds(LibraryType libraryType)
    {
        List<Guid> metadataProviderPluginIds = [];
        List<Guid> artworkProviderPluginIds = [];
        foreach (IPlugin plugin in _pluginManager.GetPlugins().OrderBy(plugin => plugin.Name))
        {
            if (_serviceProvider.GetKeyedServices<IMetadataProvider>(plugin.Id).Any(provider => provider.SupportedLibraryTypes.Contains(libraryType)))
                metadataProviderPluginIds.Add(plugin.Id);
            if (_serviceProvider.GetKeyedServices<IArtworkProvider>(plugin.Id).Any(provider => provider.SupportedLibraryTypes.Contains(libraryType)))
                artworkProviderPluginIds.Add(plugin.Id);
        }
        return (metadataProviderPluginIds, artworkProviderPluginIds);
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
        HashSet<Guid> supportedPluginIds = metadataProviderPluginIds.ToHashSet();
        List<Guid> stalePluginIds = getConfigurationsResult.Value
            .Select(configuration => configuration.PluginId)
            .Where(pluginId => !supportedPluginIds.Contains(pluginId))
            .ToList();
        if (stalePluginIds.Count > 0)
        {
            Result<Deleted> removeResult = await _unitOfWork.LibraryMetadataProviderConfigurationRepository.DeleteByLibraryIdAndPluginIdsAsync(libraryId, stalePluginIds, cancellationToken).ConfigureAwait(false);
            if (removeResult.IsFailure)
                return Result<Success>.Failure(removeResult.Errors);
        }
        HashSet<Guid> configuredPluginIds = getConfigurationsResult.Value.Select(configuration => configuration.PluginId).ToHashSet();
        int nextRank = getConfigurationsResult.Value.Count == 0 ? 1 : getConfigurationsResult.Value.Max(configuration => configuration.Rank) + 1;
        foreach (Guid pluginId in metadataProviderPluginIds)
        {
            if (configuredPluginIds.Contains(pluginId))
                continue;
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
        HashSet<Guid> supportedPluginIds = artworkProviderPluginIds.ToHashSet();
        List<Guid> stalePluginIds = getConfigurationsResult.Value
            .Select(configuration => configuration.PluginId)
            .Where(pluginId => !supportedPluginIds.Contains(pluginId))
            .ToList();
        if (stalePluginIds.Count > 0)
        {
            Result<Deleted> removeResult = await _unitOfWork.ArtworkProviderConfigurationRepository.DeleteByLibraryIdAndPluginIdsAsync(libraryId, stalePluginIds, cancellationToken).ConfigureAwait(false);
            if (removeResult.IsFailure)
                return Result<Success>.Failure(removeResult.Errors);
        }
        HashSet<Guid> configuredPluginIds = getConfigurationsResult.Value.Select(configuration => configuration.PluginId).ToHashSet();
        int nextRank = getConfigurationsResult.Value.Count == 0 ? 1 : getConfigurationsResult.Value.Max(configuration => configuration.Rank) + 1;
        foreach (Guid pluginId in artworkProviderPluginIds)
        {
            if (configuredPluginIds.Contains(pluginId))
                continue;
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
        HashSet<Guid> configuredPluginIds = getConfigurationsResult.Value.Select(configuration => configuration.PluginId).ToHashSet();
        int nextRank = getConfigurationsResult.Value.Count == 0 ? 1 : getConfigurationsResult.Value.Max(configuration => configuration.Rank) + 1;
        foreach (Guid pluginId in metadataProviderPluginIds)
        {
            if (configuredPluginIds.Contains(pluginId))
                continue;
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
        HashSet<Guid> configuredPluginIds = getConfigurationsResult.Value.Select(configuration => configuration.PluginId).ToHashSet();
        int nextRank = getConfigurationsResult.Value.Count == 0 ? 1 : getConfigurationsResult.Value.Max(configuration => configuration.Rank) + 1;
        foreach (Guid pluginId in artworkProviderPluginIds)
        {
            if (configuredPluginIds.Contains(pluginId))
                continue;
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
