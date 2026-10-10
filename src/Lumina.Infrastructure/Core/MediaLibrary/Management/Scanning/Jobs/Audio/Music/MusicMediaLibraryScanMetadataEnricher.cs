#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.MediaContributors;
using Lumina.Application.Common.DataAccess.Entities.MediaLibrary.AudioLibrary.MusicLibrary;
using Lumina.Application.Common.DataAccess.Entities.MediaLibrary.Management;
using Lumina.Application.Common.DataAccess.Entities.Plugins;
using Lumina.Application.Common.DataAccess.Entities.UsersManagement;
using Lumina.Application.Common.DataAccess.Repositories.MediaContributors;
using Lumina.Application.Common.DataAccess.UoW;
using Lumina.Application.Common.Errors;
using Lumina.Application.Common.Mapping.MediaLibrary.AudioLibrary.MusicLibrary.Albums;
using Lumina.Application.Common.Mapping.MediaLibrary.AudioLibrary.MusicLibrary.Artists;
using Lumina.Application.Common.Mapping.MediaLibrary.AudioLibrary.MusicLibrary.Tracks;
using Lumina.Contracts.DTO.Common;
using Lumina.Contracts.DTO.MediaContributors;
using Lumina.Contracts.DTO.MediaLibrary.AudioLibrary.MusicLibrary;
using Lumina.Domain.Common.Events;
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.Core.BoundedContexts.AudioLibraryBoundedContext.ExternalIdentifiers.MediaContributorBoundedContext.MediaContributorAggregate;
using Lumina.Domain.Core.BoundedContexts.AudioLibraryBoundedContext.MusicLibraryAggregate;
using Lumina.Domain.Core.BoundedContexts.AudioLibraryBoundedContext.MusicLibraryAggregate.Entities;
using Lumina.Domain.Core.BoundedContexts.AudioLibraryBoundedContext.MusicLibraryAggregate.ValueObjects;
using Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryAggregate.ValueObjects;
using Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryScanAggregate.Events;
using Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryScanAggregate.ValueObjects;
using Lumina.Domain.Core.BoundedContexts.UserManagementBoundedContext.UserAggregate.ValueObjects;
using Lumina.Domain.SharedKernel.Common.Enums.MediaContributors;
using Lumina.Domain.SharedKernel.Common.Enums.MediaLibrary;
using Lumina.Infrastructure.Core.MediaLibrary.AudioLibrary.MusicLibrary.Metadata;
using Lumina.Infrastructure.Core.MediaLibrary.Management.Scanning.Jobs.Common;
using Lumina.Plugins.Contracts.Core.Metadata;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Infrastructure.Core.MediaLibrary.Management.Scanning.Jobs.Audio.Music;

/// <summary>
/// Metadata enricher for the media library items of the music media library type, using the metadata providers configured for the media library.
/// The enriched metadata is applied to the artists, their albums and their tracks, and the media contributors discovered while enriching are linked to them.
/// </summary>
internal sealed class MusicMediaLibraryScanMetadataEnricher : IMediaLibraryScanMetadataEnricher
{
    private const int ENRICHMENT_PAGE_SIZE = 200; // The number of artists that are enriched in a single batch, keeping the peak memory bounded regardless of the library size.
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly ILogger<MusicMediaLibraryScanMetadataEnricher> _logger;

    /// <summary>
    /// The media library type that this metadata enricher supports.
    /// </summary>
    public LibraryType SupportedLibraryType => LibraryType.Music;

    /// <summary>
    /// Initializes a new instance of the <see cref="MusicMediaLibraryScanMetadataEnricher"/> class.
    /// </summary>
    /// <param name="serviceScopeFactory">Injected factory for creating scopes in which services are requested.</param>
    /// <param name="logger">Injected logger used to report the issues encountered while enriching the metadata.</param>
    public MusicMediaLibraryScanMetadataEnricher(IServiceScopeFactory serviceScopeFactory, ILogger<MusicMediaLibraryScanMetadataEnricher> logger)
    {
        _serviceScopeFactory = serviceScopeFactory;
        _logger = logger;
    }

    /// <summary>
    /// Enriches the metadata of the artists, the albums and the tracks of the provided media library, using the metadata providers configured for it.
    /// </summary>
    /// <param name="libraryId">The unique identifier of the media library whose items are enriched.</param>
    /// <param name="scanId">The unique identifier of the media library scan.</param>
    /// <param name="userId">The unique identifier of the user that initiated the media library scan.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public async Task EnrichAsync(LibraryId libraryId, ScanId scanId, UserId userId, CancellationToken cancellationToken)
    {
        await using AsyncServiceScope asyncServiceScope = _serviceScopeFactory.CreateAsyncScope();
        IUnitOfWork unitOfWork = asyncServiceScope.ServiceProvider.GetService<IUnitOfWork>()!;
        IDomainEventPublisher domainEventPublisher = asyncServiceScope.ServiceProvider.GetService<IDomainEventPublisher>()!;

        MediaLibraryScanCompositeId compositeKey = MediaLibraryScanCompositeId.Create(scanId, userId);

        // Load the media library, whose setting determines whether the providers that require access to the web are used during the enrichment.
        bool canDownloadMetadataFromWeb = false;
        if (unitOfWork.LibraryRepository is not null)
        {
            Result<LibraryEntity?> getLibraryResult = await unitOfWork.LibraryRepository.GetByIdAsync(libraryId.Value, cancellationToken: cancellationToken).ConfigureAwait(false);
            if (getLibraryResult.IsFailure || getLibraryResult.Value is null)
                _logger.LogWarning("Failed to read the media library, the providers requiring the web will not be used.");
            else
                canDownloadMetadataFromWeb = getLibraryResult.Value.CanDownloadMetadataFromWeb;
        }

        // Get the metadata providers configured for the media library, in their configured order, that support the media library type,
        // skipping the providers that require access to the web when the media library does not permit downloading data from the web.
        Result<IReadOnlyList<LibraryMetadataProviderConfigurationEntity>> getConfigurationsResult = await unitOfWork.LibraryMetadataProviderConfigurationRepository.GetByLibraryIdAsync(libraryId.Value, cancellationToken).ConfigureAwait(false);
        if (getConfigurationsResult.IsFailure)
            throw new InvalidOperationException(getConfigurationsResult.FirstError.Description);
        List<IMetadataProvider> providers = [];
        foreach (LibraryMetadataProviderConfigurationEntity configuration in getConfigurationsResult.Value.Where(configuration => configuration.IsEnabled).OrderBy(configuration => configuration.Rank))
        {
            List<IMetadataProvider> configuredProviders = [.. asyncServiceScope.ServiceProvider
                .GetKeyedServices<IMetadataProvider>(configuration.PluginId)
                .Where(provider => provider.SupportedLibraryTypes.Contains(LibraryType.Music)
                    && (canDownloadMetadataFromWeb || !provider.RequiresWebAccess))];
            if (configuredProviders.Count == 0)
                _logger.LogWarning("No metadata provider was found for the configured plugin with Id '{PluginId}' and the {LibraryType} library type.", configuration.PluginId, LibraryType.Music);
            providers.AddRange(configuredProviders);
        }

        // Read whether the metadata of the items of the user is aggregated from multiple providers, when fields are missing.
        bool shouldAggregateMetadataWhenMissing = false;
        if (unitOfWork.UserSettingsRepository is not null)
        {
            Result<UserSettingsEntity?> getUserSettingsResult = await unitOfWork.UserSettingsRepository.GetByUserIdAsync(userId.Value, cancellationToken).ConfigureAwait(false);
            if (getUserSettingsResult.IsFailure)
                _logger.LogWarning("Failed to read the user settings, the metadata will not be aggregated across providers.");
            else
                shouldAggregateMetadataWhenMissing = getUserSettingsResult.Value?.ShouldAggregateMetadataWhenMissing ?? false;
        }

        // When no metadata provider is available, the items must not be marked as failed to enrich, so the enrichment is skipped entirely.
        if (providers.Count > 0)
        {
            Result<int> getArtistsToEnrichCountResult = await unitOfWork.ArtistRepository.GetArtistsNeedingMetadataCountAsync(libraryId.Value, cancellationToken).ConfigureAwait(false);
            if (getArtistsToEnrichCountResult.IsFailure)
                throw new InvalidOperationException(getArtistsToEnrichCountResult.FirstError.Description);

            Result<int> getAlbumsToEnrichCountResult = await unitOfWork.AlbumRepository.GetAlbumsNeedingMetadataCountAsync(libraryId.Value, cancellationToken).ConfigureAwait(false);
            if (getAlbumsToEnrichCountResult.IsFailure)
                throw new InvalidOperationException(getAlbumsToEnrichCountResult.FirstError.Description);

            Result<int> getTracksToEnrichCountResult = await unitOfWork.TrackRepository.GetTracksNeedingMetadataCountAsync(libraryId.Value, cancellationToken).ConfigureAwait(false);
            if (getTracksToEnrichCountResult.IsFailure)
                throw new InvalidOperationException(getTracksToEnrichCountResult.FirstError.Description);

            // The progress is measured in items, not artists: an artist is counted together with its albums and its tracks, because enriching a single
            // artist can require a large number of metadata provider calls, one for each of its tracks, and would otherwise barely advance the progress.
            int totalItemsToEnrich = getArtistsToEnrichCountResult.Value + getAlbumsToEnrichCountResult.Value + getTracksToEnrichCountResult.Value;

            // Set the initial progress of the scan job.
            Result<Success> publishJobProgressResult = await PublishJobProgressAsync(domainEventPublisher, libraryId, compositeKey, 0, totalItemsToEnrich, cancellationToken).ConfigureAwait(false);
            if (publishJobProgressResult.IsFailure)
                throw new InvalidOperationException(publishJobProgressResult.FirstError.Description);

            DateTime lastUpdateTime = DateTime.UtcNow;
            int minUpdateIntervalMs = 100;
            int processedItemsCount = 0;

            // Reports that a single item (an artist, an album or a track) was processed, publishing the progress at most once per interval.
            async Task ReportItemProcessedAsync()
            {
                int processedItems = Interlocked.Increment(ref processedItemsCount);
                DateTime now = DateTime.UtcNow;
                if ((now - lastUpdateTime).TotalMilliseconds >= minUpdateIntervalMs)
                {
                    Result<Success> publishItemProgressResult = await PublishJobProgressAsync(domainEventPublisher, libraryId, compositeKey, processedItems, totalItemsToEnrich, cancellationToken).ConfigureAwait(false);
                    if (publishItemProgressResult.IsFailure)
                        throw new InvalidOperationException(publishItemProgressResult.FirstError.Description);
                    lastUpdateTime = now;
                }
            }

            // The media contributors discovered while enriching a page are cached by normalized display name, so that a contributor is only
            // created once even when the same person is discovered for many items of the same page.
            Dictionary<string, MediaContributorEntity> contributorsByNormalizedName = [];

            // Process the artists that need their metadata enriched in pages, keeping the peak memory bounded regardless of the library size.
            string? lastName = null;
            while (true)
            {
                cancellationToken.ThrowIfCancellationRequested();

                Result<IReadOnlyList<ArtistEntity>> getArtistsPageResult = await unitOfWork.ArtistRepository.GetArtistsNeedingMetadataAsync(libraryId.Value, lastName, ENRICHMENT_PAGE_SIZE, cancellationToken).ConfigureAwait(false);
                if (getArtistsPageResult.IsFailure)
                    throw new InvalidOperationException(getArtistsPageResult.FirstError.Description);
                IReadOnlyList<ArtistEntity> artistsPage = getArtistsPageResult.Value;
                if (artistsPage.Count == 0)
                    break;

                foreach (ArtistEntity artistEntity in artistsPage)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    await EnrichArtistAsync(artistEntity, providers, shouldAggregateMetadataWhenMissing, unitOfWork, contributorsByNormalizedName, ReportItemProcessedAsync, cancellationToken).ConfigureAwait(false);

                    // Persist each enriched artist as soon as it is done, then detach it, so that an interrupted scan keeps everything it already
                    // enriched, and the peak memory stays bounded regardless of the library size.
                    Result<Success> saveChangesResult = await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
                    if (saveChangesResult.IsFailure)
                    {
                        // A unique constraint conflict on a shared lookup such as a media contributor can happen when two scans enrich concurrently;
                        // it must not abort the whole scan, so the artist is left unenriched and retried on the next scan, while the rest proceeds.
                        if (saveChangesResult.FirstError == Errors.Persistence.UniqueConstraintViolation)
                            _logger.LogWarning("The enriched metadata of the artist '{ArtistName}' could not be persisted because of a unique constraint conflict; it will be retried on the next scan.", artistEntity.Name);
                        else
                            throw new InvalidOperationException(saveChangesResult.FirstError.Description);
                    }
                    unitOfWork.ClearTrackedEntities();
                    contributorsByNormalizedName.Clear();
                }

                lastName = artistsPage[^1].Name;
            }

            // The enrichment finished, force the progress to reach its total, so that an item skipped during an early return cannot leave it incomplete.
            Result<Success> publishFinalProgressResult = await PublishJobProgressAsync(domainEventPublisher, libraryId, compositeKey, totalItemsToEnrich, totalItemsToEnrich, cancellationToken).ConfigureAwait(false);
            if (publishFinalProgressResult.IsFailure)
                throw new InvalidOperationException(publishFinalProgressResult.FirstError.Description);
        }
        else
            _logger.LogWarning("No metadata provider is configured for the media library with Id '{LibraryId}', the metadata enrichment will be skipped.", libraryId.Value);
    }

    /// <summary>
    /// Enriches the metadata of the provided <paramref name="artistEntity"/>, its albums and its tracks, using the provided metadata providers in their configured order.
    /// </summary>
    /// <param name="artistEntity">The artist whose metadata, and the metadata of its albums and tracks, is enriched.</param>
    /// <param name="metadataProviders">The metadata providers, in their configured order.</param>
    /// <param name="shouldAggregateMetadataWhenMissing">Whether the metadata is aggregated from multiple providers, when fields are missing, or not.</param>
    /// <param name="unitOfWork">The unit of work used to persist the enriched artist.</param>
    /// <param name="contributorsByNormalizedName">The cache of the media contributors discovered in the current page, keyed by their normalized display name.</param>
    /// <param name="onItemProcessed">The callback invoked after each artist, album or track is processed.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    private async Task EnrichArtistAsync(ArtistEntity artistEntity, IReadOnlyList<IMetadataProvider> metadataProviders, bool shouldAggregateMetadataWhenMissing, IUnitOfWork unitOfWork, Dictionary<string, MediaContributorEntity> contributorsByNormalizedName, Func<Task> onItemProcessed, CancellationToken cancellationToken)
    {
        // Convert the artist aggregate, together with its albums and tracks, to a domain object.
        Result<Artist> getArtistResult = artistEntity.ToDomainEntity();
        if (getArtistResult.IsFailure)
            return;
        Artist artist = getArtistResult.Value;

        string fallbackPath = artistEntity.Albums.SelectMany(album => album.Tracks).Select(track => track.Path).FirstOrDefault() ?? string.Empty;

        if (artistEntity.MetadataStatus != MetadataStatus.Enriched)
        {
            ArtistMetadataLookupDto artistLookup = new(
                LibraryId: artistEntity.LibraryId,
                Path: fallbackPath,
                MusicBrainzArtistId: artistEntity.MusicBrainzArtistId,
                Name: artistEntity.Name);
            ResolvedMetadata<ArtistMetadataDto>? resolvedArtist = await ResolveAndApplyMetadataAsync<ArtistMetadataLookupDto, ArtistMetadataDto>(
                artistLookup, metadataProviders, shouldAggregateMetadataWhenMissing, IsUsableArtistMetadata,
                (first, second) => MusicMetadataAggregator.Merge(first, second),
                async metadata =>
                {
                    List<MusicMediaContributor> contributors = await ResolveContributorsAsync(metadata.Contributors, unitOfWork.MediaContributorRepository, contributorsByNormalizedName, cancellationToken).ConfigureAwait(false);
                    return metadata.ApplyTo(artist, contributors);
                }, _logger, cancellationToken).ConfigureAwait(false);
            if (resolvedArtist is null)
                artistEntity.MetadataStatus = MetadataStatus.Failed;
            else
            {
                artistEntity.MetadataStatus = MetadataStatus.Enriched;
                artistEntity.MetadataProvider = resolvedArtist.ProviderName;
                artistEntity.LastMetadataUpdateUtc = DateTime.UtcNow;
            }
        }

        // An artist is reported as processed even when its own metadata was already enriched, because it is only visited when one of its descendants still needs it.
        await onItemProcessed().ConfigureAwait(false);

        foreach (AlbumEntity albumEntity in artistEntity.Albums)
        {
            Album? album = artist.Albums.FirstOrDefault(candidate => candidate.Id.Value == albumEntity.Id);
            if (album is null)
                continue;

            // The release the tracks of the album are enriched from: it comes from the tags, or from the release the album was matched to, and it is
            // only ever a release whose tracklist corresponds to the local files, so the tracks are never resolved from a different edition.
            Guid? albumReleaseId = albumEntity.MusicBrainzReleaseId;

            if (albumEntity.MetadataStatus != MetadataStatus.Enriched)
            {
                string albumPath = albumEntity.Tracks.Select(track => track.Path).FirstOrDefault() ?? fallbackPath;
                AlbumMetadataLookupDto albumLookup = new(
                    LibraryId: artistEntity.LibraryId,
                    Path: albumPath,
                    MusicBrainzReleaseGroupId: albumEntity.MusicBrainzReleaseGroupId,
                    MusicBrainzReleaseId: albumEntity.MusicBrainzReleaseId,
                    Title: albumEntity.Title,
                    ArtistName: artistEntity.Name,
                    ReleaseYear: albumEntity.OriginalReleaseYear,
                    TrackCount: albumEntity.Tracks.Count);
                ResolvedMetadata<AlbumMetadataDto>? resolvedAlbum = await ResolveAndApplyMetadataAsync<AlbumMetadataLookupDto, AlbumMetadataDto>(
                    albumLookup, metadataProviders, shouldAggregateMetadataWhenMissing, IsUsableAlbumMetadata,
                    (first, second) => MusicMetadataAggregator.Merge(first, second),
                    async metadata =>
                    {
                        List<MusicMediaContributor> contributors = await ResolveContributorsAsync(metadata.Contributors, unitOfWork.MediaContributorRepository, contributorsByNormalizedName, cancellationToken).ConfigureAwait(false);
                        return metadata.ApplyTo(artist, album, contributors);
                    }, _logger, cancellationToken).ConfigureAwait(false);
                if (resolvedAlbum is null)
                    albumEntity.MetadataStatus = MetadataStatus.Failed;
                else
                {
                    albumReleaseId = resolvedAlbum.Metadata.MusicBrainzReleaseId ?? albumReleaseId;
                    albumEntity.MetadataStatus = MetadataStatus.Enriched;
                    albumEntity.MetadataProvider = resolvedAlbum.ProviderName;
                    albumEntity.LastMetadataUpdateUtc = DateTime.UtcNow;
                }
                await onItemProcessed().ConfigureAwait(false);
            }

            foreach (TrackEntity trackEntity in albumEntity.Tracks)
            {
                if (trackEntity.MetadataStatus == MetadataStatus.Enriched)
                    continue;
                Track? track = album.Tracks.FirstOrDefault(candidate => candidate.Id.Value == trackEntity.Id);
                if (track is null)
                    continue;

                TrackMetadataLookupDto trackLookup = new(
                    LibraryId: artistEntity.LibraryId,
                    Path: trackEntity.Path,
                    MusicBrainzRecordingId: trackEntity.MusicBrainzRecordingId,
                    Isrc: trackEntity.Isrcs.Select(isrc => isrc.Value).FirstOrDefault(),
                    Title: trackEntity.Title,
                    ArtistName: artistEntity.Name,
                    ReleaseName: albumEntity.Title,
                    TrackNumber: trackEntity.TrackNumber,
                    DurationInSeconds: trackEntity.DurationInSeconds > 0 ? trackEntity.DurationInSeconds : null,
                    MusicBrainzReleaseId: albumReleaseId,
                    DiscNumber: trackEntity.DiscNumber,
                    MusicBrainzWorkId: trackEntity.MusicBrainzWorkId,
                    WorkTitle: trackEntity.WorkTitle);
                ResolvedMetadata<AudioMetadataDto>? resolvedTrack = await ResolveAndApplyMetadataAsync<TrackMetadataLookupDto, AudioMetadataDto>(
                    trackLookup, metadataProviders, shouldAggregateMetadataWhenMissing, IsUsableAudioMetadata,
                    (first, second) => MusicMetadataAggregator.Merge(first, second),
                    async metadata =>
                    {
                        List<MusicMediaContributor> contributors = await ResolveContributorsAsync(metadata.Contributors, unitOfWork.MediaContributorRepository, contributorsByNormalizedName, cancellationToken).ConfigureAwait(false);
                        return metadata.ApplyTo(artist, album, track, contributors);
                    }, _logger, cancellationToken).ConfigureAwait(false);
                if (resolvedTrack is null)
                    trackEntity.MetadataStatus = MetadataStatus.Failed;
                else
                {
                    trackEntity.MetadataStatus = MetadataStatus.Enriched;
                    trackEntity.MetadataProvider = resolvedTrack.ProviderName;
                    trackEntity.LastMetadataUpdateUtc = DateTime.UtcNow;
                }
                await onItemProcessed().ConfigureAwait(false);
            }
        }

        // Rebuild the repository entity graph from the enriched domain aggregate, carrying over the enrichment tracking columns of the tracked entities.
        ArtistEntity enrichedEntity = artist.ToRepositoryEntity();
        CopyEnrichmentTracking(enrichedEntity, artistEntity);

        Result<Updated> updateResult = await unitOfWork.ArtistRepository.UpdateAsync(enrichedEntity, cancellationToken).ConfigureAwait(false);
        if (updateResult.IsFailure)
            throw new InvalidOperationException(updateResult.FirstError.Description);
    }

    /// <summary>
    /// Resolves the metadata of the provided lookup and applies it to the media item, either from the first provider whose metadata is usable and applies successfully,
    /// or, when aggregation is enabled, from the merged metadata of every usable provider.
    /// </summary>
    /// <typeparam name="TLookup">The type of the lookup the metadata providers must accept.</typeparam>
    /// <typeparam name="TMetadata">The type of the metadata returned by the metadata providers.</typeparam>
    /// <param name="lookup">The lookup describing the media item.</param>
    /// <param name="metadataProviders">The metadata providers, in their configured order.</param>
    /// <param name="shouldAggregate">Whether the metadata of every usable provider is merged, or only the first one is used.</param>
    /// <param name="isUsable">The predicate that determines whether a metadata result is usable.</param>
    /// <param name="merge">The function that merges two metadata results.</param>
    /// <param name="applyAsync">The function that applies a metadata result to the media item.</param>
    /// <param name="logger">The logger used to report the metadata providers that fail.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>The applied metadata and the name of the provider, or the providers, that supplied it, or <see langword="null"/> when no provider returned applicable metadata.</returns>
    private static async Task<ResolvedMetadata<TMetadata>?> ResolveAndApplyMetadataAsync<TLookup, TMetadata>(MetadataLookupDto lookup, IReadOnlyList<IMetadataProvider> metadataProviders, bool shouldAggregate, Func<TMetadata, bool> isUsable, Func<TMetadata, TMetadata, TMetadata> merge, Func<TMetadata, Task<Result<Updated>>> applyAsync, ILogger logger, CancellationToken cancellationToken)
        where TLookup : MetadataLookupDto
        where TMetadata : MetadataDto
    {
        if (shouldAggregate)
            return await ResolveAggregatedAndApplyAsync<TLookup, TMetadata>(lookup, metadataProviders, isUsable, merge, applyAsync, logger, cancellationToken).ConfigureAwait(false);
        return await ResolveFirstAndApplyAsync<TLookup, TMetadata>(lookup, metadataProviders, isUsable, applyAsync, logger, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Tries the providers in configured order, applying the metadata of the first one that is usable and applies successfully, without ever querying the providers that come after it.
    /// </summary>
    /// <typeparam name="TLookup">The type of the lookup the metadata providers must accept.</typeparam>
    /// <typeparam name="TMetadata">The type of the metadata returned by the metadata providers.</typeparam>
    /// <param name="lookup">The lookup describing the media item.</param>
    /// <param name="metadataProviders">The metadata providers, in their configured order.</param>
    /// <param name="isUsable">The predicate that determines whether a metadata result is usable.</param>
    /// <param name="applyAsync">The function that applies a metadata result to the media item.</param>
    /// <param name="logger">The logger used to report the metadata providers that fail.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>The applied metadata and the name of the provider that supplied it, or <see langword="null"/> when no provider returned applicable metadata.</returns>
    private static async Task<ResolvedMetadata<TMetadata>?> ResolveFirstAndApplyAsync<TLookup, TMetadata>(MetadataLookupDto lookup, IReadOnlyList<IMetadataProvider> metadataProviders, Func<TMetadata, bool> isUsable, Func<TMetadata, Task<Result<Updated>>> applyAsync, ILogger logger, CancellationToken cancellationToken)
        where TLookup : MetadataLookupDto
        where TMetadata : MetadataDto
    {
        foreach (IMetadataProvider metadataProvider in metadataProviders)
        {
            if (metadataProvider.LookupType != typeof(TLookup))
                continue;

            try
            {
                MetadataDto? metadataResult = await metadataProvider.GetMetadataAsync(lookup, cancellationToken).ConfigureAwait(false);
                if (metadataResult is not TMetadata metadata || !isUsable(metadata))
                    continue;

                // The metadata is applied while the provider is being tried, so that a provider whose metadata cannot be applied does not stop the enrichment and the next one is tried instead.
                Result<Updated> applyResult = await applyAsync(metadata).ConfigureAwait(false);
                if (applyResult.IsFailure)
                    continue;
                return new ResolvedMetadata<TMetadata>(metadata, metadataProvider.Name);
            }
            catch (Exception exception)
            {
                // A failing metadata provider must not prevent the other providers from being tried, but the failure must be surfaced for diagnostics.
                logger.LogWarning(exception, "The metadata provider '{ProviderName}' failed while resolving a {LookupType} lookup for '{Lookup}'.", metadataProvider.Name, typeof(TLookup).Name, lookup);
            }
        }
        return null;
    }

    /// <summary>
    /// Queries every provider, merges the metadata of the usable ones, and applies the merged metadata once.
    /// </summary>
    /// <typeparam name="TLookup">The type of the lookup the metadata providers must accept.</typeparam>
    /// <typeparam name="TMetadata">The type of the metadata returned by the metadata providers.</typeparam>
    /// <param name="lookup">The lookup describing the media item.</param>
    /// <param name="metadataProviders">The metadata providers, in their configured order.</param>
    /// <param name="isUsable">The predicate that determines whether a metadata result is usable.</param>
    /// <param name="merge">The function that merges two metadata results.</param>
    /// <param name="applyAsync">The function that applies a metadata result to the media item.</param>
    /// <param name="logger">The logger used to report the metadata providers that fail.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>The applied merged metadata and the names of the providers that supplied it, or <see langword="null"/> when no provider returned applicable metadata.</returns>
    private static async Task<ResolvedMetadata<TMetadata>?> ResolveAggregatedAndApplyAsync<TLookup, TMetadata>(MetadataLookupDto lookup, IReadOnlyList<IMetadataProvider> metadataProviders, Func<TMetadata, bool> isUsable, Func<TMetadata, TMetadata, TMetadata> merge, Func<TMetadata, Task<Result<Updated>>> applyAsync, ILogger logger, CancellationToken cancellationToken)
        where TLookup : MetadataLookupDto
        where TMetadata : MetadataDto
    {
        TMetadata? mergedMetadata = default;
        List<string> providerNames = [];
        foreach (IMetadataProvider metadataProvider in metadataProviders)
        {
            if (metadataProvider.LookupType != typeof(TLookup))
                continue;

            try
            {
                MetadataDto? metadataResult = await metadataProvider.GetMetadataAsync(lookup, cancellationToken).ConfigureAwait(false);
                if (metadataResult is not TMetadata metadata || !isUsable(metadata))
                    continue;

                mergedMetadata = mergedMetadata is null ? metadata : merge(mergedMetadata, metadata);
                providerNames.Add(metadataProvider.Name);
            }
            catch (Exception exception)
            {
                // A failing metadata provider must not prevent the other providers from being tried, but the failure must be surfaced for diagnostics.
                logger.LogWarning(exception, "The metadata provider '{ProviderName}' failed while resolving a {LookupType} lookup for '{Lookup}'.", metadataProvider.Name, typeof(TLookup).Name, lookup);
            }
        }

        if (mergedMetadata is null)
            return null;

        Result<Updated> applyResult = await applyAsync(mergedMetadata).ConfigureAwait(false);
        if (applyResult.IsFailure)
            return null;
        return new ResolvedMetadata<TMetadata>(mergedMetadata, string.Join(", ", providerNames.Distinct(StringComparer.Ordinal)));
    }

    /// <summary>
    /// Finds the media contributor with the name of the provided <paramref name="contributor"/>, or creates it, caching it by its normalized display name
    /// so that a contributor is only created once even when the same person is discovered for many items of the same page.
    /// </summary>
    /// <param name="contributors">The metadata contributors to resolve.</param>
    /// <param name="mediaContributorRepository">The repository used to persist the media contributors discovered while enriching.</param>
    /// <param name="contributorsByNormalizedName">The cache of the media contributors discovered in the current page, keyed by their normalized display name.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>The resolved domain media contributors, already carrying their persisted identifiers.</returns>
    private static async Task<List<MusicMediaContributor>> ResolveContributorsAsync(IReadOnlyCollection<MediaContributorDto>? contributors, IMediaContributorRepository mediaContributorRepository, Dictionary<string, MediaContributorEntity> contributorsByNormalizedName, CancellationToken cancellationToken)
    {
        List<MusicMediaContributor> resolvedContributors = [];
        foreach (MediaContributorDto contributor in contributors ?? [])
        {
            if (contributor.Name?.DisplayName is null)
                continue;

            string displayName = contributor.Name.DisplayName;
            string normalizedDisplayName = displayName.ToLowerInvariant();
            if (!contributorsByNormalizedName.TryGetValue(normalizedDisplayName, out MediaContributorEntity? contributorEntity))
            {
                Result<MediaContributorEntity> getContributorResult = await mediaContributorRepository.FindOrCreateByDisplayNameAsync(displayName, contributor.Name.LegalName, cancellationToken).ConfigureAwait(false);
                if (getContributorResult.IsFailure)
                    throw new InvalidOperationException(getContributorResult.FirstError.Description);
                contributorEntity = getContributorResult.Value;
                contributorsByNormalizedName[normalizedDisplayName] = contributorEntity;
            }

            MediaContributorRole role = contributor.Role ?? MediaContributorRole.Performer;
            if (resolvedContributors.Any(resolved => resolved.ContributorId.Value == contributorEntity.Id && resolved.Role == role))
                continue;

            Result<MusicMediaContributor> domainContributorResult = MusicMediaContributor.Create(MediaContributorId.Create(contributorEntity.Id), role);
            if (domainContributorResult.IsFailure)
                continue;
            resolvedContributors.Add(domainContributorResult.Value);
        }
        return resolvedContributors;
    }

    /// <summary>
    /// Copies the metadata enrichment tracking columns of the tracked <paramref name="source"/> artist, albums and tracks onto the rebuilt repository entity graph.
    /// </summary>
    /// <param name="target">The rebuilt repository entity graph that receives the tracking columns.</param>
    /// <param name="source">The tracked repository entity graph that carries the tracking columns.</param>
    private static void CopyEnrichmentTracking(ArtistEntity target, ArtistEntity source)
    {
        target.MetadataStatus = source.MetadataStatus;
        target.MetadataProvider = source.MetadataProvider;
        target.LastMetadataUpdateUtc = source.LastMetadataUpdateUtc;
        foreach (AlbumEntity targetAlbum in target.Albums)
        {
            AlbumEntity? sourceAlbum = source.Albums.FirstOrDefault(album => album.Id == targetAlbum.Id);
            if (sourceAlbum is null)
                continue;
            targetAlbum.MetadataStatus = sourceAlbum.MetadataStatus;
            targetAlbum.MetadataProvider = sourceAlbum.MetadataProvider;
            targetAlbum.LastMetadataUpdateUtc = sourceAlbum.LastMetadataUpdateUtc;
            foreach (TrackEntity targetTrack in targetAlbum.Tracks)
            {
                TrackEntity? sourceTrack = sourceAlbum.Tracks.FirstOrDefault(track => track.Id == targetTrack.Id);
                if (sourceTrack is null)
                    continue;
                targetTrack.MetadataStatus = sourceTrack.MetadataStatus;
                targetTrack.MetadataProvider = sourceTrack.MetadataProvider;
                targetTrack.LastMetadataUpdateUtc = sourceTrack.LastMetadataUpdateUtc;
            }
        }
    }

    /// <summary>
    /// Determines whether the provided artist metadata is usable, meaning it has a name.
    /// </summary>
    /// <param name="metadata">The metadata to validate.</param>
    /// <returns><see langword="true"/> when the metadata is usable, otherwise <see langword="false"/>.</returns>
    private static bool IsUsableArtistMetadata(ArtistMetadataDto metadata)
    {
        return !string.IsNullOrWhiteSpace(metadata.Name);
    }

    /// <summary>
    /// Determines whether the provided album metadata is usable, meaning it has a title.
    /// </summary>
    /// <param name="metadata">The metadata to validate.</param>
    /// <returns><see langword="true"/> when the metadata is usable, otherwise <see langword="false"/>.</returns>
    private static bool IsUsableAlbumMetadata(AlbumMetadataDto metadata)
    {
        return !string.IsNullOrWhiteSpace(metadata.Title);
    }

    /// <summary>
    /// Determines whether the provided audio metadata is usable, meaning it has a title.
    /// </summary>
    /// <param name="metadata">The metadata to validate.</param>
    /// <returns><see langword="true"/> when the metadata is usable, otherwise <see langword="false"/>.</returns>
    private static bool IsUsableAudioMetadata(AudioMetadataDto metadata)
    {
        return !string.IsNullOrWhiteSpace(metadata.Title);
    }

    /// <summary>
    /// Publishes a job progress update.
    /// </summary>
    /// <param name="domainEventPublisher">The service used to publish the progress update.</param>
    /// <param name="libraryId">The unique identifier of the media library being scanned.</param>
    /// <param name="compositeKey">The composite unique identifier of a media library scan.</param>
    /// <param name="currentProgress">The current job progress.</param>
    /// <param name="totalProgress">The total job progress.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="Result{TValue}"/> representing either a successful operation, or an error.</returns>
    private async Task<Result<Success>> PublishJobProgressAsync(IDomainEventPublisher domainEventPublisher, LibraryId libraryId, MediaLibraryScanCompositeId compositeKey, int currentProgress, int totalProgress, CancellationToken cancellationToken)
    {
        Result<MediaLibraryScanJobProgress> scanJobProgressResult = MediaLibraryScanJobProgress.Create(currentProgress, totalProgress, "EnrichingMetadata");
        if (scanJobProgressResult.IsFailure)
            return scanJobProgressResult.Errors;

        await domainEventPublisher.PublishAsync(new LibraryScanJobProgressChangedDomainEvent(
            Guid.NewGuid(), libraryId, compositeKey, scanJobProgressResult.Value, DateTime.UtcNow), cancellationToken).ConfigureAwait(false);

        return Result.Success;
    }

    /// <summary>
    /// The metadata resolved for a media library item, along with the provider, or providers, that supplied it.
    /// </summary>
    /// <typeparam name="TMetadata">The type of the resolved metadata.</typeparam>
    /// <param name="Metadata">The resolved metadata of the item.</param>
    /// <param name="ProviderName">The name of the provider, or the providers, that supplied the metadata.</param>
    private sealed record ResolvedMetadata<TMetadata>(TMetadata Metadata, string ProviderName) where TMetadata : MetadataDto;
}
