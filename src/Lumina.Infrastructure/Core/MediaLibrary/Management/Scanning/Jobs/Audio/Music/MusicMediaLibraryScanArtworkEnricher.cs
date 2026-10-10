#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.MediaLibrary.AudioLibrary.MusicLibrary;
using Lumina.Application.Common.DataAccess.Entities.MediaLibrary.Management;
using Lumina.Application.Common.DataAccess.Entities.Plugins;
using Lumina.Application.Common.DataAccess.Entities.UsersManagement;
using Lumina.Application.Common.DataAccess.UoW;
using Lumina.Application.Common.Infrastructure.Security;
using Lumina.Application.Core.MediaLibrary.AudioLibrary.MusicLibrary.Artwork;
using Lumina.Contracts.DTO.Common;
using Lumina.Contracts.DTO.MediaLibrary.AudioLibrary.MusicLibrary;
using Lumina.Domain.Common.Events;
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryAggregate.ValueObjects;
using Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryScanAggregate.Events;
using Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryScanAggregate.ValueObjects;
using Lumina.Domain.Core.BoundedContexts.UserManagementBoundedContext.UserAggregate.ValueObjects;
using Lumina.Domain.SharedKernel.Common.Enums.AudioLibrary;
using Lumina.Domain.SharedKernel.Common.Enums.MediaLibrary;
using Lumina.Infrastructure.Core.MediaLibrary.Management.Scanning.Jobs.Common;
using Lumina.Plugins.Contracts.Core.Metadata;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Infrastructure.Core.MediaLibrary.Management.Scanning.Jobs.Audio.Music;

/// <summary>
/// Artwork enricher for the media library items of the music media library type, using the artwork providers configured for the media library.
/// </summary>
/// <remarks>
/// The artwork of an artist and of an album is tracked per type and ordinal, so that a change of one piece of artwork does not require the others to be re-fetched.
/// </remarks>
internal sealed class MusicMediaLibraryScanArtworkEnricher : IMediaLibraryScanArtworkEnricher
{
    private const int ENRICHMENT_PAGE_SIZE = 200; // The number of artists or albums that are enriched in a single batch, keeping the peak memory bounded regardless of the library size.
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly ILogger<MusicMediaLibraryScanArtworkEnricher> _logger;
    private readonly MusicLibraryPathStructure _musicLibraryPathStructure;

    /// <summary>
    /// The media library type that this artwork enricher supports.
    /// </summary>
    public LibraryType SupportedLibraryType => LibraryType.Music;

    /// <summary>
    /// Initializes a new instance of the <see cref="MusicMediaLibraryScanArtworkEnricher"/> class.
    /// </summary>
    /// <param name="serviceScopeFactory">Injected factory for creating scopes in which services are requested.</param>
    /// <param name="logger">Injected logger used to report the issues encountered while resolving the artwork.</param>
    /// <param name="musicLibraryPathStructure">Injected service used to derive the release type directory of the tracks from their file system paths.</param>
    public MusicMediaLibraryScanArtworkEnricher(IServiceScopeFactory serviceScopeFactory, ILogger<MusicMediaLibraryScanArtworkEnricher> logger, MusicLibraryPathStructure musicLibraryPathStructure)
    {
        _serviceScopeFactory = serviceScopeFactory;
        _logger = logger;
        _musicLibraryPathStructure = musicLibraryPathStructure;
    }

    /// <summary>
    /// Resolves the artwork of the artists and the albums of the provided media library, using the artwork providers configured for it.
    /// </summary>
    /// <param name="libraryId">The unique identifier of the media library whose artwork is resolved.</param>
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

        // Load the media library, whose name is used to build the directories of the artwork, and whose setting determines whether the providers that require access to the web are used during the enrichment.
        string libraryName = string.Empty;
        bool canDownloadMetadataFromWeb = false;
        if (unitOfWork.LibraryRepository is not null)
        {
            Result<LibraryEntity?> getLibraryResult = await unitOfWork.LibraryRepository.GetByIdAsync(libraryId.Value, cancellationToken: cancellationToken).ConfigureAwait(false);
            if (getLibraryResult.IsFailure || getLibraryResult.Value is null)
                _logger.LogWarning("Failed to read the media library, the artwork will not be stored and the providers requiring the web will not be used.");
            else
            {
                libraryName = getLibraryResult.Value.Title;
                canDownloadMetadataFromWeb = getLibraryResult.Value.CanDownloadMetadataFromWeb;
            }
        }

        // Read whether the artwork of the items of the user is aggregated from multiple providers, when it is missing.
        bool shouldAggregateArtworkWhenMissing = false;
        if (unitOfWork.UserSettingsRepository is not null)
        {
            Result<UserSettingsEntity?> getUserSettingsResult = await unitOfWork.UserSettingsRepository.GetByUserIdAsync(userId.Value, cancellationToken).ConfigureAwait(false);
            if (getUserSettingsResult.IsFailure)
                _logger.LogWarning("Failed to read the user settings, the artwork will not be aggregated across providers.");
            else
                shouldAggregateArtworkWhenMissing = getUserSettingsResult.Value?.ShouldAggregateArtworkWhenMissing ?? false;
        }

        // Get the artwork providers configured for the media library, in their configured order, that support the media library type.
        // The artwork resolution is best-effort, so a failure to read the artwork configurations must not prevent the enrichment from proceeding.
        List<IArtworkProvider> artworkProviders = [];
        if (unitOfWork.ArtworkProviderConfigurationRepository is not null)
        {
            Result<IReadOnlyList<LibraryArtworkProviderConfigurationEntity>> getArtworkConfigurationsResult = await unitOfWork.ArtworkProviderConfigurationRepository.GetByLibraryIdAsync(libraryId.Value, cancellationToken).ConfigureAwait(false);
            if (getArtworkConfigurationsResult.IsFailure)
                _logger.LogWarning("Failed to read the artwork provider configurations.");
            else
            {
                foreach (LibraryArtworkProviderConfigurationEntity configuration in getArtworkConfigurationsResult.Value.Where(configuration => configuration.IsEnabled).OrderBy(configuration => configuration.Rank))
                {
                    List<IArtworkProvider> configuredProviders = [.. asyncServiceScope.ServiceProvider
                        .GetKeyedServices<IArtworkProvider>(configuration.PluginId)
                        .Where(provider => provider.SupportedLibraryTypes.Contains(LibraryType.Music)
                            && (canDownloadMetadataFromWeb || !provider.RequiresWebAccess))];
                    if (configuredProviders.Count == 0)
                        _logger.LogWarning("No artwork provider was found for the configured plugin with Id '{PluginId}' and the {LibraryType} library type.", configuration.PluginId, LibraryType.Music);
                    artworkProviders.AddRange(configuredProviders);
                }
            }
        }

        // When no artwork provider is available, the items must not be marked as failed to resolve, so the enrichment is skipped entirely.
        if (artworkProviders.Count == 0)
        {
            _logger.LogWarning("No artwork provider is configured for the media library with Id '{LibraryId}', the artwork enrichment will be skipped.", libraryId.Value);
            return;
        }

        // The providers are split by the lookup they accept, keeping the configured rank order in which they were gathered.
        IReadOnlyList<IArtworkProvider> artistProviders = [.. artworkProviders.Where(provider => provider is IArtworkProvider<ArtistMetadataLookupDto>)];
        IReadOnlyList<IArtworkProvider> albumProviders = [.. artworkProviders.Where(provider => provider is IArtworkProvider<AlbumMetadataLookupDto>)];

        IMusicArtworkService? musicArtworkService = asyncServiceScope.ServiceProvider.GetService<IMusicArtworkService>();
        IFileHashService fileHashService = asyncServiceScope.ServiceProvider.GetService<IFileHashService>()!;

        int totalArtistsToEnrich = 0;
        if (artistProviders.Count > 0)
        {
            Result<int> getArtistsToEnrichCountResult = await unitOfWork.ArtistRepository.GetArtistsNeedingArtworkCountAsync(libraryId.Value, cancellationToken).ConfigureAwait(false);
            if (getArtistsToEnrichCountResult.IsFailure)
                throw new InvalidOperationException(getArtistsToEnrichCountResult.FirstError.Description);
            totalArtistsToEnrich = getArtistsToEnrichCountResult.Value;
        }

        int totalAlbumsToEnrich = 0;
        if (albumProviders.Count > 0)
        {
            Result<int> getAlbumsToEnrichCountResult = await unitOfWork.AlbumRepository.GetAlbumsNeedingArtworkCountAsync(libraryId.Value, cancellationToken).ConfigureAwait(false);
            if (getAlbumsToEnrichCountResult.IsFailure)
                throw new InvalidOperationException(getAlbumsToEnrichCountResult.FirstError.Description);
            totalAlbumsToEnrich = getAlbumsToEnrichCountResult.Value;
        }

        int totalItemsToEnrich = totalArtistsToEnrich + totalAlbumsToEnrich;

        // Set the initial progress of the scan job.
        Result<Success> publishJobProgressResult = await PublishJobProgressAsync(domainEventPublisher, libraryId, compositeKey, 0, totalItemsToEnrich, cancellationToken).ConfigureAwait(false);
        if (publishJobProgressResult.IsFailure)
            throw new InvalidOperationException(publishJobProgressResult.FirstError.Description);

        DateTime lastUpdateTime = DateTime.UtcNow;
        int minUpdateIntervalMs = 100;
        int processedItemsCount = 0;

        // Reports that a single artist or album was processed, publishing the progress at most once per interval.
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

        if (artistProviders.Count > 0)
            await EnrichArtistsAsync(libraryId, artistProviders, musicArtworkService, fileHashService, libraryName, shouldAggregateArtworkWhenMissing, unitOfWork, ReportItemProcessedAsync, cancellationToken).ConfigureAwait(false);

        if (albumProviders.Count > 0)
            await EnrichAlbumsAsync(libraryId, albumProviders, musicArtworkService, fileHashService, libraryName, shouldAggregateArtworkWhenMissing, unitOfWork, ReportItemProcessedAsync, cancellationToken).ConfigureAwait(false);

        // The enrichment finished, force the progress to reach its total, so that a throttled update cannot leave it incomplete.
        Result<Success> publishFinalProgressResult = await PublishJobProgressAsync(domainEventPublisher, libraryId, compositeKey, totalItemsToEnrich, totalItemsToEnrich, cancellationToken).ConfigureAwait(false);
        if (publishFinalProgressResult.IsFailure)
            throw new InvalidOperationException(publishFinalProgressResult.FirstError.Description);
    }

    /// <summary>
    /// Resolves the artwork of the artists of the provided media library whose artwork has not been resolved yet, page by page.
    /// </summary>
    /// <param name="libraryId">The unique identifier of the media library whose artists are enriched.</param>
    /// <param name="artworkProviders">The artists artwork providers, in the order they are consulted.</param>
    /// <param name="musicArtworkService">The service used to store the artwork of the artists.</param>
    /// <param name="fileHashService">The service used to hash the artwork, to detect whether the resolved artwork differs from the stored one.</param>
    /// <param name="libraryName">The name of the media library the artists belong to.</param>
    /// <param name="shouldAggregateArtworkWhenMissing">Whether the artwork is aggregated from multiple providers, when it is missing, or only the first provider that supplies it is used.</param>
    /// <param name="unitOfWork">The unit of work used to persist the artwork of the artists.</param>
    /// <param name="reportItemProcessedAsync">The callback invoked after each artist is processed, to advance the progress.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    private async Task EnrichArtistsAsync(LibraryId libraryId, IReadOnlyList<IArtworkProvider> artworkProviders, IMusicArtworkService? musicArtworkService, IFileHashService fileHashService, string libraryName, bool shouldAggregateArtworkWhenMissing, IUnitOfWork unitOfWork, Func<Task> reportItemProcessedAsync, CancellationToken cancellationToken)
    {
        string? lastName = null;
        while (true)
        {
            cancellationToken.ThrowIfCancellationRequested();

            Result<IReadOnlyList<ArtistEntity>> getArtistsPageResult = await unitOfWork.ArtistRepository.GetArtistsNeedingArtworkAsync(libraryId.Value, lastName, ENRICHMENT_PAGE_SIZE, cancellationToken).ConfigureAwait(false);
            if (getArtistsPageResult.IsFailure)
                throw new InvalidOperationException(getArtistsPageResult.FirstError.Description);
            IReadOnlyList<ArtistEntity> artistsPage = getArtistsPageResult.Value;
            if (artistsPage.Count == 0)
                break;

            foreach (ArtistEntity artistEntity in artistsPage)
            {
                cancellationToken.ThrowIfCancellationRequested();

                // The first track of the artist, in a deterministic order, describes the artist in the artwork lookup and locates its directory on the file system.
                string firstTrackPath = artistEntity.Albums.SelectMany(album => album.Tracks).Select(track => track.Path).OrderBy(path => path).FirstOrDefault() ?? string.Empty;
                await EnrichArtistArtworkAsync(artistEntity, firstTrackPath, artworkProviders, musicArtworkService, fileHashService, libraryName, shouldAggregateArtworkWhenMissing, unitOfWork, cancellationToken).ConfigureAwait(false);
                await reportItemProcessedAsync().ConfigureAwait(false);
            }

            // Persist the resolved artwork of this page, then detach it from the change tracker, keeping the peak memory bounded regardless of the library size.
            Result<Success> saveChangesResult = await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            if (saveChangesResult.IsFailure)
                throw new InvalidOperationException(saveChangesResult.FirstError.Description);
            unitOfWork.ClearTrackedEntities();

            lastName = artistsPage[^1].Name;
        }
    }

    /// <summary>
    /// Resolves the artwork of the albums of the provided media library whose artwork has not been resolved yet, page by page.
    /// </summary>
    /// <param name="libraryId">The unique identifier of the media library whose albums are enriched.</param>
    /// <param name="artworkProviders">The album artwork providers, in the order they are consulted.</param>
    /// <param name="musicArtworkService">The service used to store the artwork of the albums.</param>
    /// <param name="fileHashService">The service used to hash the artwork, to detect whether the resolved artwork differs from the stored one.</param>
    /// <param name="libraryName">The name of the media library the albums belong to.</param>
    /// <param name="shouldAggregateArtworkWhenMissing">Whether the artwork is aggregated from multiple providers, when it is missing, or only the first provider that supplies it is used.</param>
    /// <param name="unitOfWork">The unit of work used to persist the artwork of the albums.</param>
    /// <param name="reportItemProcessedAsync">The callback invoked after each album is processed, to advance the progress.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    private async Task EnrichAlbumsAsync(LibraryId libraryId, IReadOnlyList<IArtworkProvider> artworkProviders, IMusicArtworkService? musicArtworkService, IFileHashService fileHashService, string libraryName, bool shouldAggregateArtworkWhenMissing, IUnitOfWork unitOfWork, Func<Task> reportItemProcessedAsync, CancellationToken cancellationToken)
    {
        // A provider that does not require the web, like the one that reads the files of the user, can resolve the artwork without any MusicBrainz identifier.
        bool canResolveWithoutMusicBrainzIds = artworkProviders.Any(provider => !provider.RequiresWebAccess);

        // The albums that were attempted but remain unresolved are tracked and excluded from the following pages, so that a page that made no album reach a terminal state cannot make the pagination loop forever.
        List<Guid> albumsRemainingUnresolved = [];

        while (true)
        {
            cancellationToken.ThrowIfCancellationRequested();

            Result<IReadOnlyList<AlbumEntity>> getAlbumsPageResult = await unitOfWork.AlbumRepository.GetAlbumsNeedingArtworkAsync(libraryId.Value, albumsRemainingUnresolved, ENRICHMENT_PAGE_SIZE, cancellationToken).ConfigureAwait(false);
            if (getAlbumsPageResult.IsFailure)
                throw new InvalidOperationException(getAlbumsPageResult.FirstError.Description);
            IReadOnlyList<AlbumEntity> albumsPage = getAlbumsPageResult.Value;
            if (albumsPage.Count == 0)
                break;

            // Load the path of one track of each album of this page, in one query, since it is used to locate the album directory on the file system.
            Result<IReadOnlyDictionary<Guid, string>> getTrackPathsResult = await unitOfWork.AlbumRepository.GetFirstTrackPathsByAlbumIdsAsync([.. albumsPage.Select(album => album.Id)], cancellationToken).ConfigureAwait(false);
            if (getTrackPathsResult.IsFailure)
                throw new InvalidOperationException(getTrackPathsResult.FirstError.Description);

            foreach (AlbumEntity albumEntity in albumsPage)
            {
                cancellationToken.ThrowIfCancellationRequested();

                string firstTrackPath = getTrackPathsResult.Value.TryGetValue(albumEntity.Id, out string? trackPath) ? trackPath : string.Empty;
                bool isTerminal = await EnrichAlbumArtworkAsync(albumEntity, firstTrackPath, artworkProviders, musicArtworkService, fileHashService, libraryName, canResolveWithoutMusicBrainzIds, shouldAggregateArtworkWhenMissing, unitOfWork, cancellationToken).ConfigureAwait(false);
                if (!isTerminal)
                    albumsRemainingUnresolved.Add(albumEntity.Id);
                await reportItemProcessedAsync().ConfigureAwait(false);
            }

            // Persist the enriched albums of this page, then detach them from the change tracker, keeping the peak memory bounded regardless of the library size.
            Result<Success> saveChangesResult = await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            if (saveChangesResult.IsFailure)
                throw new InvalidOperationException(saveChangesResult.FirstError.Description);
            unitOfWork.ClearTrackedEntities();
        }
    }

    /// <summary>
    /// Resolves the artwork of the provided <paramref name="artistEntity"/> from the <paramref name="artworkProviders"/>, storing the artwork resolved from the providers, in their order.
    /// </summary>
    /// <param name="artistEntity">The artist whose artwork is resolved.</param>
    /// <param name="firstTrackPath">The file system path of one track of the artist, used to locate its directory.</param>
    /// <param name="artworkProviders">The artist artwork providers, in the order they are consulted.</param>
    /// <param name="musicArtworkService">The service used to store the artwork of the artist.</param>
    /// <param name="fileHashService">The service used to hash the artwork, to detect whether the resolved artwork differs from the stored one.</param>
    /// <param name="libraryName">The name of the media library the artist belongs to.</param>
    /// <param name="shouldAggregateArtworkWhenMissing">Whether the artwork is aggregated from multiple providers, when it is missing, or only the first provider that supplies it is used.</param>
    /// <param name="unitOfWork">The unit of work used to persist the artwork of the artist.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns><see langword="true"/> when the artist reached a terminal artwork state, otherwise <see langword="false"/>.</returns>
    private async Task<bool> EnrichArtistArtworkAsync(ArtistEntity artistEntity, string firstTrackPath, IReadOnlyList<IArtworkProvider> artworkProviders, IMusicArtworkService? musicArtworkService, IFileHashService fileHashService, string libraryName, bool shouldAggregateArtworkWhenMissing, IUnitOfWork unitOfWork, CancellationToken cancellationToken)
    {
        ArtistMetadataLookupDto artworkLookup = new(
            LibraryId: artistEntity.LibraryId,
            Path: firstTrackPath,
            MusicBrainzArtistId: artistEntity.MusicBrainzArtistId,
            Name: artistEntity.Name);

        string artistName = artistEntity.Name;

        return await EnrichArtworkAsync(
            artworkLookup,
            MusicArtworkOwnerType.Artist,
            artistEntity.Id,
            artistName,
            artworkProviders,
            musicArtworkService,
            fileHashService,
            () => musicArtworkService!.DeleteArtistArtwork(artistEntity.LibraryId, libraryName, artistName),
            (artwork, token) => musicArtworkService!.SaveArtistArtworkAsync(artistEntity.LibraryId, libraryName, artistName, artwork, token),
            shouldAggregateArtworkWhenMissing,
            unitOfWork,
            cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Resolves the artwork of the provided <paramref name="albumEntity"/> from the <paramref name="artworkProviders"/>, storing the artwork resolved from the providers, in their order.
    /// </summary>
    /// <param name="albumEntity">The album whose artwork is resolved.</param>
    /// <param name="firstTrackPath">The file system path of one track of the album, used to locate its directory.</param>
    /// <param name="artworkProviders">The album artwork providers, in the order they are consulted.</param>
    /// <param name="musicArtworkService">The service used to store the artwork of the album.</param>
    /// <param name="fileHashService">The service used to hash the artwork, to detect whether the resolved artwork differs from the stored one.</param>
    /// <param name="libraryName">The name of the media library the album belongs to.</param>
    /// <param name="canResolveWithoutMusicBrainzIds">Whether a provider that does not require the web can resolve the artwork without any MusicBrainz identifier.</param>
    /// <param name="shouldAggregateArtworkWhenMissing">Whether the artwork is aggregated from multiple providers, when it is missing, or only the first provider that supplies it is used.</param>
    /// <param name="unitOfWork">The unit of work used to persist the artwork of the album.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns><see langword="true"/> when the album reached a terminal artwork state, otherwise <see langword="false"/>.</returns>
    private async Task<bool> EnrichAlbumArtworkAsync(AlbumEntity albumEntity, string firstTrackPath, IReadOnlyList<IArtworkProvider> artworkProviders, IMusicArtworkService? musicArtworkService, IFileHashService fileHashService, string libraryName, bool canResolveWithoutMusicBrainzIds, bool shouldAggregateArtworkWhenMissing, IUnitOfWork unitOfWork, CancellationToken cancellationToken)
    {
        // Without any MusicBrainz identifier, only a provider that reads the files of the user can resolve the artwork, so the album is left for a following scan otherwise.
        if (albumEntity.MusicBrainzReleaseId is null && albumEntity.MusicBrainzReleaseGroupId is null && !canResolveWithoutMusicBrainzIds)
            return false;

        AlbumMetadataLookupDto artworkLookup = new(
            LibraryId: albumEntity.LibraryId,
            Path: firstTrackPath,
            MusicBrainzReleaseGroupId: albumEntity.MusicBrainzReleaseGroupId,
            MusicBrainzReleaseId: albumEntity.MusicBrainzReleaseId,
            Title: albumEntity.Title,
            ArtistName: albumEntity.Artist?.Name,
            ReleaseYear: albumEntity.OriginalReleaseYear,
            TrackCount: albumEntity.TotalTracks);

        string artistName = albumEntity.Artist?.Name ?? string.Empty;

        // The release type directory groups the artwork of the releases of the artist that share a title, so the album, the single and the live release of the same songs do not collide.
        // It is read from the file system, because the structure on disk is the source of truth of the release type of an album.
        string? releaseTypeName = _musicLibraryPathStructure.GetReleaseTypeDirectoryName(firstTrackPath);

        return await EnrichArtworkAsync(
            artworkLookup,
            MusicArtworkOwnerType.Album,
            albumEntity.Id,
            albumEntity.Title,
            artworkProviders,
            musicArtworkService,
            fileHashService,
            () => musicArtworkService!.DeleteAlbumArtwork(albumEntity.LibraryId, albumEntity.Id, libraryName, artistName, albumEntity.Title, releaseTypeName),
            (artwork, token) => musicArtworkService!.SaveAlbumArtworkAsync(albumEntity.LibraryId, albumEntity.Id, libraryName, artistName, albumEntity.Title, artwork, token, releaseTypeName),
            shouldAggregateArtworkWhenMissing,
            unitOfWork,
            cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Resolves the artwork of the media item described by <paramref name="artworkLookup"/> from the <paramref name="artworkProviders"/>, in their order, replacing the stored artwork of the item.
    /// </summary>
    /// <param name="artworkLookup">The lookup describing the media item whose artwork is resolved.</param>
    /// <param name="ownerType">The type of the music library item that owns the artwork.</param>
    /// <param name="ownerId">The Id of the music library item that owns the artwork.</param>
    /// <param name="ownerDescription">The description of the owner, used to report the issues encountered.</param>
    /// <param name="artworkProviders">The artwork providers, in the order they are consulted.</param>
    /// <param name="musicArtworkService">The service used to store the artwork of the owner.</param>
    /// <param name="fileHashService">The service used to hash the artwork, to detect whether the resolved artwork differs from the stored one.</param>
    /// <param name="deleteStoredArtwork">The action that deletes the artwork of the owner from the internal media directory.</param>
    /// <param name="saveArtworkAsync">The action that stores a piece of artwork of the owner into the internal media directory.</param>
    /// <param name="shouldAggregateArtworkWhenMissing">Whether the artwork is aggregated from multiple providers, when it is missing, or only the first provider that supplies it is used.</param>
    /// <param name="unitOfWork">The unit of work used to persist the artwork of the owner.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns><see langword="true"/> when the owner reached a terminal artwork state, otherwise <see langword="false"/>.</returns>
    private async Task<bool> EnrichArtworkAsync(MetadataLookupDto artworkLookup, MusicArtworkOwnerType ownerType, Guid ownerId, string ownerDescription, IReadOnlyList<IArtworkProvider> artworkProviders, IMusicArtworkService? musicArtworkService, IFileHashService fileHashService, Func<Result<Deleted>> deleteStoredArtwork, Func<ArtworkDto, CancellationToken, Task<Result<string>>> saveArtworkAsync, bool shouldAggregateArtworkWhenMissing, IUnitOfWork unitOfWork, CancellationToken cancellationToken)
    {
        if (musicArtworkService is null || unitOfWork.MusicArtworkRepository is null)
            return false;

        ArtworkResolution resolution = await ResolveArtworkAsync(artworkLookup, artworkProviders, ownerDescription, shouldAggregateArtworkWhenMissing, cancellationToken).ConfigureAwait(false);

        // Replace the stored artwork of the owner as a whole, so that a piece of artwork that no longer exists at the source does not linger.
        Result<Deleted> deleteStoredArtworkResult = deleteStoredArtwork();
        if (deleteStoredArtworkResult.IsFailure)
        {
            _logger.LogWarning("Failed to clear the stored artwork files of the {OwnerType} '{OwnerDescription}'.", ownerType, ownerDescription);
            DeleteTemporaryArtworks(resolution.Artworks.Select(resolvedArtwork => resolvedArtwork.Artwork));
            return false;
        }
        Result<Deleted> deleteEntitiesResult = await unitOfWork.MusicArtworkRepository.DeleteByOwnerAsync(ownerType, ownerId, cancellationToken).ConfigureAwait(false);
        if (deleteEntitiesResult.IsFailure)
        {
            _logger.LogWarning("Failed to clear the stored artwork records of the {OwnerType} '{OwnerDescription}'.", ownerType, ownerDescription);
            DeleteTemporaryArtworks(resolution.Artworks.Select(resolvedArtwork => resolvedArtwork.Artwork));
            return false;
        }

        List<MusicArtworkEntity> artworkEntities = [];

        if (resolution.Artworks.Count > 0)
        {
            foreach (ResolvedArtwork resolvedArtwork in resolution.Artworks)
            {
                ArtworkDto artwork = resolvedArtwork.Artwork;
                Result<string> saveArtworkResult = await saveArtworkAsync(artwork, cancellationToken).ConfigureAwait(false);
                if (saveArtworkResult.IsFailure)
                {
                    _logger.LogWarning("Failed to store the {ArtworkType} artwork of the {OwnerType} '{OwnerDescription}'.", artwork.Type, ownerType, ownerDescription);
                    continue;
                }

                string storedArtworkPath = Path.Combine(AppContext.BaseDirectory, saveArtworkResult.Value.TrimStart('/', '\\'));
                ulong contentHash = fileHashService.ComputeFileHash(storedArtworkPath);
                artworkEntities.Add(CreateArtworkEntity(ownerId, ownerType, artwork.Type, artwork.Ordinal, saveArtworkResult.Value, contentHash, ArtworkStatus.Enriched, resolvedArtwork.ProviderName));
            }

            // When every resolved piece of artwork failed to be stored, the owner is treated as failed to resolve, so that it is retried.
            if (artworkEntities.Count == 0)
            {
                artworkEntities.Add(CreateArtworkEntity(ownerId, ownerType, ArtworkType.Cover, 0, null, 0, ArtworkStatus.Failed, null));
                await PersistArtworkAsync(unitOfWork, artworkEntities, cancellationToken).ConfigureAwait(false);
                return false;
            }
        }
        else
        {
            // No artwork exists for the owner when a provider answered and found none, which is not a failure; it only failed when no provider could answer.
            ArtworkStatus status = resolution.HasResolved ? ArtworkStatus.NotAvailable : ArtworkStatus.Failed;
            artworkEntities.Add(CreateArtworkEntity(ownerId, ownerType, ArtworkType.Cover, 0, null, 0, status, null));
            await PersistArtworkAsync(unitOfWork, artworkEntities, cancellationToken).ConfigureAwait(false);
            return status == ArtworkStatus.NotAvailable;
        }

        await PersistArtworkAsync(unitOfWork, artworkEntities, cancellationToken).ConfigureAwait(false);
        return true;
    }

    /// <summary>
    /// Persists the provided pieces of <paramref name="artworkEntities"/> of a music library item.
    /// </summary>
    /// <param name="unitOfWork">The unit of work used to persist the artwork.</param>
    /// <param name="artworkEntities">The pieces of artwork to persist.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    private async Task PersistArtworkAsync(IUnitOfWork unitOfWork, List<MusicArtworkEntity> artworkEntities, CancellationToken cancellationToken)
    {
        Result<Created> insertArtworkResult = await unitOfWork.MusicArtworkRepository.InsertRangeAsync(artworkEntities, cancellationToken).ConfigureAwait(false);
        if (insertArtworkResult.IsFailure)
            _logger.LogWarning("Failed to persist the resolved artwork of a music library item.");
    }

    /// <summary>
    /// Deletes the temporary files of the provided <paramref name="artworks"/>, used when the resolved artwork is not stored, so the temporary files do not linger.
    /// </summary>
    /// <param name="artworks">The resolved artworks whose temporary files are deleted.</param>
    private static void DeleteTemporaryArtworks(IEnumerable<ArtworkDto> artworks)
    {
        foreach (ArtworkDto artwork in artworks)
        {
            if (!artwork.IsTemporary || string.IsNullOrWhiteSpace(artwork.LocalPath))
                continue;
            try
            {
                if (File.Exists(artwork.LocalPath))
                    File.Delete(artwork.LocalPath);
            }
            catch (IOException)
            {
                // A failed cleanup of the temporary file must not mask the result of the operation.
            }
            catch (UnauthorizedAccessException)
            {
                // A failed cleanup of the temporary file must not mask the result of the operation.
            }
        }
    }

    /// <summary>
    /// Resolves the artwork of the media item described by <paramref name="lookup"/> from the <paramref name="artworkProviders"/>,
    /// either aggregating the providers when <paramref name="shouldAggregateArtworkWhenMissing"/> is set, or using the first provider that supplies artwork otherwise.
    /// </summary>
    /// <param name="lookup">The lookup describing the media item whose artwork is resolved.</param>
    /// <param name="artworkProviders">The artwork providers, in the order they are consulted.</param>
    /// <param name="ownerDescription">The description of the owner, used to report the issues encountered.</param>
    /// <param name="shouldAggregateArtworkWhenMissing">Whether the artwork of every provider is combined, or only the first provider that supplies artwork is used.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>The resolved artwork, each with the provider that supplied it, and whether a provider answered at all, distinguishing a confirmed absence of artwork from a resolution failure.</returns>
    private async Task<ArtworkResolution> ResolveArtworkAsync(MetadataLookupDto lookup, IReadOnlyList<IArtworkProvider> artworkProviders, string ownerDescription, bool shouldAggregateArtworkWhenMissing, CancellationToken cancellationToken)
    {
        if (shouldAggregateArtworkWhenMissing)
            return await ResolveAggregatedArtworkAsync(lookup, artworkProviders, ownerDescription, cancellationToken).ConfigureAwait(false);
        return await ResolveFirstArtworkAsync(lookup, artworkProviders, ownerDescription, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Resolves the artwork of the media item described by <paramref name="lookup"/> from the first <paramref name="artworkProviders"/> provider that supplies some, in their order.
    /// </summary>
    /// <param name="lookup">The lookup describing the media item whose artwork is resolved.</param>
    /// <param name="artworkProviders">The artwork providers, in the order they are consulted.</param>
    /// <param name="ownerDescription">The description of the owner, used to report the issues encountered.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>The resolved artwork, each with the provider that supplied it, and whether a provider answered at all.</returns>
    private async Task<ArtworkResolution> ResolveFirstArtworkAsync(MetadataLookupDto lookup, IReadOnlyList<IArtworkProvider> artworkProviders, string ownerDescription, CancellationToken cancellationToken)
    {
        bool hasResolved = false;
        bool hasFailed = false;
        foreach (IArtworkProvider provider in artworkProviders)
        {
            IReadOnlyList<ArtworkDto> providerArtworks;
            try
            {
                providerArtworks = await provider.GetArtworkAsync(lookup, cancellationToken).ConfigureAwait(false);
            }
            catch (Exception exception)
            {
                // A failing artwork provider must not prevent the other providers from being tried, but the failure must be surfaced for diagnostics.
                hasFailed = true;
                _logger.LogWarning(exception, "The artwork provider '{ProviderName}' failed while resolving the artwork of '{OwnerDescription}'.", provider.Name, ownerDescription);
                continue;
            }

            hasResolved = true;
            // A provider that returns remote artwork must declare that it requires web access, otherwise downloading it would contradict the provider's contract.
            List<ResolvedArtwork> usableArtworks = [.. providerArtworks
                .Where(artwork => string.IsNullOrWhiteSpace(artwork.RemoteUrl) || provider.RequiresWebAccess)
                .Select(artwork => new ResolvedArtwork(artwork, provider.Name))];
            if (usableArtworks.Count > 0)
                return new ArtworkResolution(usableArtworks, HasResolved: true);
        }

        // An empty result is only treated as a confirmed absence of artwork when at least one provider answered; when every provider failed, it is a failure.
        return new ArtworkResolution([], HasResolved: hasResolved || !hasFailed);
    }

    /// <summary>
    /// Resolves the artwork of the media item described by <paramref name="lookup"/> from every <paramref name="artworkProviders"/> provider, in their order,
    /// combining them so that a provider only fills the artwork the providers before it did not supply.
    /// </summary>
    /// <param name="lookup">The lookup describing the media item whose artwork is resolved.</param>
    /// <param name="artworkProviders">The artwork providers, in the order they are consulted.</param>
    /// <param name="ownerDescription">The description of the owner, used to report the issues encountered.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>The resolved artwork, each with the provider that supplied it, and whether a provider answered at all.</returns>
    private async Task<ArtworkResolution> ResolveAggregatedArtworkAsync(MetadataLookupDto lookup, IReadOnlyList<IArtworkProvider> artworkProviders, string ownerDescription, CancellationToken cancellationToken)
    {
        bool hasResolved = false;
        bool hasFailed = false;
        List<ResolvedArtwork> resolvedArtworks = [];
        HashSet<(ArtworkType Type, int Ordinal)> resolvedKeys = [];
        foreach (IArtworkProvider provider in artworkProviders)
        {
            IReadOnlyList<ArtworkDto> providerArtworks;
            try
            {
                providerArtworks = await provider.GetArtworkAsync(lookup, cancellationToken).ConfigureAwait(false);
            }
            catch (Exception exception)
            {
                // A failing artwork provider must not prevent the other providers from being tried, but the failure must be surfaced for diagnostics.
                hasFailed = true;
                _logger.LogWarning(exception, "The artwork provider '{ProviderName}' failed while resolving the artwork of '{OwnerDescription}'.", provider.Name, ownerDescription);
                continue;
            }

            hasResolved = true;
            foreach (ArtworkDto artwork in providerArtworks)
            {
                // A provider that returns remote artwork must declare that it requires web access, otherwise downloading it would contradict the provider's contract.
                if (!string.IsNullOrWhiteSpace(artwork.RemoteUrl) && !provider.RequiresWebAccess)
                    continue;
                // The providers are consulted in their configured order, so a provider only fills the artwork the higher ranked ones did not supply, and a provider
                // that holds just a part of the artwork of the item, like the local files of the user, is complemented by the providers that follow it.
                if (resolvedKeys.Add((artwork.Type, artwork.Ordinal)))
                    resolvedArtworks.Add(new ResolvedArtwork(artwork, provider.Name));
            }
        }

        // An empty result is only treated as a confirmed absence of artwork when at least one provider answered; when every provider failed, it is a failure.
        return new ArtworkResolution(resolvedArtworks, HasResolved: hasResolved || !hasFailed);
    }

    /// <summary>
    /// Creates a piece of artwork of the music library item of the provided <paramref name="ownerType"/> identified by <paramref name="ownerId"/>.
    /// </summary>
    /// <param name="ownerId">The Id of the music library item the artwork belongs to.</param>
    /// <param name="ownerType">The type of the music library item the artwork belongs to.</param>
    /// <param name="artworkType">The type of the artwork.</param>
    /// <param name="ordinal">The ordinal of the artwork within its type.</param>
    /// <param name="fileName">The relative file name of the stored artwork, if the artwork has been resolved.</param>
    /// <param name="contentHash">The content hash of the stored artwork.</param>
    /// <param name="status">The status of the artwork enrichment.</param>
    /// <param name="provider">The name of the plugin that resolved the artwork, if applicable.</param>
    /// <returns>The created artwork entity.</returns>
    private static MusicArtworkEntity CreateArtworkEntity(Guid ownerId, MusicArtworkOwnerType ownerType, ArtworkType artworkType, int ordinal, string? fileName, ulong contentHash, ArtworkStatus status, string? provider)
    {
        return new MusicArtworkEntity
        {
            Id = Guid.NewGuid(),
            OwnerType = ownerType,
            OwnerId = ownerId,
            ArtworkType = artworkType,
            Ordinal = ordinal,
            FileName = fileName,
            ContentHash = contentHash,
            Status = status,
            Provider = provider,
            LastUpdateUtc = DateTime.UtcNow,
            CreatedOnUtc = DateTime.UtcNow,
            CreatedBy = Guid.Empty,
            UpdatedBy = null
        };
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
        Result<MediaLibraryScanJobProgress> scanJobProgressResult = MediaLibraryScanJobProgress.Create(currentProgress, totalProgress, "ResolvingArtwork");
        if (scanJobProgressResult.IsFailure)
            return scanJobProgressResult.Errors;

        await domainEventPublisher.PublishAsync(new LibraryScanJobProgressChangedDomainEvent(
            Guid.NewGuid(), libraryId, compositeKey, scanJobProgressResult.Value, DateTime.UtcNow), cancellationToken).ConfigureAwait(false);

        return Result.Success;
    }

    /// <summary>
    /// The artwork resolved for a music library item, each with the provider that supplied it, and whether a provider answered at all.
    /// </summary>
    /// <param name="Artworks">The resolved artworks of the item, each with the provider that supplied it.</param>
    /// <param name="HasResolved">Whether at least one provider answered, confirming that any absence of artwork is real rather than a failure.</param>
    private sealed record ArtworkResolution(IReadOnlyList<ResolvedArtwork> Artworks, bool HasResolved);

    /// <summary>
    /// A piece of artwork of a music library item, together with the provider that supplied it.
    /// </summary>
    /// <param name="Artwork">The resolved artwork.</param>
    /// <param name="ProviderName">The name of the provider that supplied the artwork.</param>
    private sealed record ResolvedArtwork(ArtworkDto Artwork, string ProviderName);
}
