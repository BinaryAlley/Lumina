#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.MediaLibrary.Management;
using Lumina.Application.Common.DataAccess.Entities.MediaLibrary.WrittenContentLibrary.BookLibrary;
using Lumina.Application.Common.DataAccess.Entities.Plugins;
using Lumina.Application.Common.DataAccess.UoW;
using Lumina.Application.Common.Infrastructure.Security;
using Lumina.Application.Core.MediaLibrary.WrittenContentLibrary.BooksLibrary.Artwork;
using Lumina.Contracts.DTO.Common;
using Lumina.Contracts.DTO.MediaLibrary.WrittenContentLibrary.BookLibrary;
using Lumina.Domain.Common.Events;
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryScanAggregate.Events;
using Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryScanAggregate.Services.Jobs;
using Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryScanAggregate.ValueObjects;
using Lumina.Domain.SharedKernel.Common.Enums.BookLibrary;
using Lumina.Domain.SharedKernel.Common.Enums.MediaLibrary;
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

namespace Lumina.Infrastructure.Core.MediaLibrary.Management.Scanning.Jobs.Common;

/// <summary>
/// Media library scan job for resolving the artwork of the media library items, using the artwork providers configured for the media library.
/// The artwork is tracked per book, per type and ordinal, so that a change of one artwork does not require the others to be re-fetched.
/// </summary>
internal sealed class MediaLibraryScanArtworkEnrichmentJob : MediaLibraryScanJob, IMediaLibraryScanArtworkEnrichmentJob
{
    private const int ENRICHMENT_PAGE_SIZE = 1000; // the number of books that are enriched in a single batch, keeping the peak memory bounded regardless of the library size
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly ILogger<MediaLibraryScanArtworkEnrichmentJob> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="MediaLibraryScanArtworkEnrichmentJob"/> class.
    /// </summary>
    /// <param name="serviceScopeFactory">
    /// Injected factory for creating scopes in which services are requested.
    /// See docs/technical/architecture/architecture-knowledge-management/architecture-decision-log/architecture-decision-record-0001.md for details.
    /// </param>
    /// <param name="logger">Injected logger used to report the issues encountered while resolving the artwork.</param>
    public MediaLibraryScanArtworkEnrichmentJob(IServiceScopeFactory serviceScopeFactory, ILogger<MediaLibraryScanArtworkEnrichmentJob> logger)
    {
        _serviceScopeFactory = serviceScopeFactory;
        _logger = logger;
    }

    /// <summary>
    /// Executes the payload of the media library scan job.
    /// </summary>
    /// <typeparam name="TInput">The type of the input parameter representing the data to be processed by this payload.</typeparam>
    /// <param name="id">The unique identifier of the media library scan job.</param>
    /// <param name="input">The input data to be processed by this payload.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    public override async Task ExecuteAsync<TInput>(Guid id, TInput input, CancellationToken cancellationToken)
    {
        try
        {
            // increment the number of parents that finished their execution and called this job (beware race conditions, jobs run in parallel)
            int parentsCompleted = Interlocked.Increment(ref parentsPayloadsExecuted);
            // only execute this job's payload when it has no parents, or when all the parents finished their execution
            if (Parents.Count == 0 || parentsCompleted == Parents.Count)
            {
                // this needs to be wrapped in a task because even though this job is processed in a "fire and forget" async manner, it still does synchronous
                // processing that takes time, and would block the processing of scan jobs in the in-memory queue
                await Task.Run(async () =>
                {
                    Status = LibraryScanJobStatus.Running;
                    // see docs/technical/architecture/architecture-knowledge-management/architecture-decision-log/architecture-decision-record-0001.md for details:
                    await using AsyncServiceScope asyncServiceScope = _serviceScopeFactory.CreateAsyncScope();
                    IUnitOfWork unitOfWork = asyncServiceScope.ServiceProvider.GetService<IUnitOfWork>()!;
                    IDomainEventPublisher domainEventPublisher = asyncServiceScope.ServiceProvider.GetService<IDomainEventPublisher>()!;

                    MediaLibraryScanCompositeId compositeKey = MediaLibraryScanCompositeId.Create(ScanId, UserId);

                    // load the media library, whose name is used to build the directory of the book artwork, and whose setting determines
                    // whether the providers that require access to the web are used during the enrichment
                    string libraryName = string.Empty;
                    bool canDownloadMetadataFromWeb = false;
                    if (unitOfWork.LibraryRepository is not null)
                    {
                        Result<LibraryEntity?> getLibraryResult = await unitOfWork.LibraryRepository.GetByIdAsync(LibraryId.Value, cancellationToken).ConfigureAwait(false);
                        if (getLibraryResult.IsFailure || getLibraryResult.Value is null)
                            _logger.LogWarning("Failed to read the media library, the book artwork will not be stored and the providers requiring the web will not be used.");
                        else
                        {
                            libraryName = getLibraryResult.Value.Title;
                            canDownloadMetadataFromWeb = getLibraryResult.Value.CanDownloadMetadataFromWeb;
                        }
                    }

                    // get the artwork providers configured for the media library, in their configured order, that support the media library type.
                    // the artwork resolution is best-effort, so a failure to read the artwork configurations must not prevent the enrichment from proceeding
                    List<IArtworkProvider> artworkProviders = [];
                    if (unitOfWork.ArtworkProviderConfigurationRepository is not null)
                    {
                        Result<IReadOnlyList<LibraryArtworkProviderConfigurationEntity>> getArtworkConfigurationsResult = await unitOfWork.ArtworkProviderConfigurationRepository.GetByLibraryIdAsync(LibraryId.Value, cancellationToken).ConfigureAwait(false);
                        if (getArtworkConfigurationsResult.IsFailure)
                            _logger.LogWarning("Failed to read the artwork provider configurations.");
                        else
                        {
                            foreach (LibraryArtworkProviderConfigurationEntity configuration in getArtworkConfigurationsResult.Value.Where(configuration => configuration.IsEnabled).OrderBy(configuration => configuration.Rank))
                            {
                                List<IArtworkProvider> configuredProviders = [.. asyncServiceScope.ServiceProvider
                                    .GetKeyedServices<IArtworkProvider>(configuration.PluginId)
                                    .Where(provider => provider.SupportedLibraryTypes.Contains(LibraryType.Book)
                                        && (canDownloadMetadataFromWeb || !provider.RequiresWebAccess))];
                                if (configuredProviders.Count == 0)
                                    _logger.LogWarning("No artwork provider was found for the configured plugin with Id '{PluginId}' and the {LibraryType} library type.", configuration.PluginId, LibraryType.Book);
                                artworkProviders.AddRange(configuredProviders);
                            }
                        }
                    }

                    // when no artwork provider is available, the books must not be marked as failed to resolve, so the enrichment is skipped entirely
                    if (artworkProviders.Count > 0)
                    {
                        IBookArtworkService? bookArtworkService = asyncServiceScope.ServiceProvider.GetService<IBookArtworkService>();
                        IFileHashService fileHashService = asyncServiceScope.ServiceProvider.GetService<IFileHashService>()!;

                        Result<int> getBooksToEnrichCountResult = await unitOfWork.BookRepository.GetBooksNeedingArtworkCountAsync(LibraryId.Value, cancellationToken).ConfigureAwait(false);
                        if (getBooksToEnrichCountResult.IsFailure)
                            throw new InvalidOperationException(getBooksToEnrichCountResult.FirstError.Description);
                        int totalBooksToEnrich = getBooksToEnrichCountResult.Value;

                        // set the initial progress of the scan job
                        Result<Success> publishJobProgressResult = await PublishJobProgressAsync(domainEventPublisher, compositeKey, 0, totalBooksToEnrich, cancellationToken).ConfigureAwait(false);
                        if (publishJobProgressResult.IsFailure)
                            throw new InvalidOperationException(publishJobProgressResult.FirstError.Description);

                        DateTime lastUpdateTime = DateTime.UtcNow;
                        int minUpdateIntervalMs = 100;
                        int processedBooksCount = 0;

                        // process the books that need their artwork resolved in pages, keeping the peak memory bounded regardless of the library size
                        string? lastPath = null;
                        while (true)
                        {
                            cancellationToken.ThrowIfCancellationRequested();

                            Result<IReadOnlyList<BookEntity>> getBooksPageResult = await unitOfWork.BookRepository.GetBooksNeedingArtworkAsync(LibraryId.Value, lastPath, ENRICHMENT_PAGE_SIZE, cancellationToken).ConfigureAwait(false);
                            if (getBooksPageResult.IsFailure)
                                throw new InvalidOperationException(getBooksPageResult.FirstError.Description);
                            IReadOnlyList<BookEntity> booksPage = getBooksPageResult.Value;
                            if (booksPage.Count == 0)
                                break;

                            // load the display names of the authors of the books of this page, in one query, since they are used to build the artwork directory
                            Result<IReadOnlyDictionary<Guid, string?>> getAuthorsResult = await unitOfWork.BookRepository.GetAuthorsDisplayNamesByBookIdsAsync([.. booksPage.Select(bookEntity => bookEntity.Id)], cancellationToken).ConfigureAwait(false);
                            if (getAuthorsResult.IsFailure)
                                throw new InvalidOperationException(getAuthorsResult.FirstError.Description);

                            foreach (BookEntity bookEntity in booksPage)
                            {
                                cancellationToken.ThrowIfCancellationRequested();

                                await EnrichBookArtworkAsync(bookEntity, artworkProviders, bookArtworkService, fileHashService, libraryName, getAuthorsResult.Value, cancellationToken).ConfigureAwait(false);

                                // check if enough time has passed since last update
                                DateTime now = DateTime.UtcNow;
                                if ((now - lastUpdateTime).TotalMilliseconds >= minUpdateIntervalMs)
                                {
                                    // increment the number of processed elements progress
                                    publishJobProgressResult = await PublishJobProgressAsync(domainEventPublisher, compositeKey, Interlocked.Increment(ref processedBooksCount), totalBooksToEnrich, cancellationToken).ConfigureAwait(false);
                                    if (publishJobProgressResult.IsFailure)
                                        throw new InvalidOperationException(publishJobProgressResult.FirstError.Description);
                                    lastUpdateTime = now;
                                }
                            }

                            // persist the enriched books of this page, then detach them from the change tracker, keeping the peak memory bounded regardless of the library size
                            await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
                            unitOfWork.ClearTrackedEntities();

                            lastPath = booksPage[^1].Path;
                        }
                    }
                    else
                    {
                        _logger.LogWarning("No artwork provider is configured for the media library with Id '{LibraryId}', the artwork enrichment will be skipped.", LibraryId.Value);
                    }

                    Status = LibraryScanJobStatus.Completed;
                    // when this job has no linked children, it's the last job in the directed acyclic job graph, and the scan is completed
                    if (Children.Count == 0)
                        await domainEventPublisher.PublishAsync(new LibraryScanFinishedDomainEvent(Guid.NewGuid(), compositeKey, DateTime.UtcNow), cancellationToken).ConfigureAwait(false);

                    // call each linked child with the obtained payload
                    foreach (IMediaLibraryScanJob child in Children)
                        await child.ExecuteAsync(id, input, cancellationToken).ConfigureAwait(false);
                }, cancellationToken).ConfigureAwait(false);
            }
        }
        catch (OperationCanceledException)
        {
            Status = LibraryScanJobStatus.Canceled;
            throw;
        }
        catch (Exception exception)
        {
            Status = LibraryScanJobStatus.Failed;
            await ScanFailurePublisher.PublishAsync(_serviceScopeFactory, LibraryId, MediaLibraryScanCompositeId.Create(ScanId, UserId), exception, cancellationToken).ConfigureAwait(false);
        }
    }

    /// <summary>
    /// Resolves the cover artwork of the provided <paramref name="bookEntity"/> from the <paramref name="artworkProviders"/>, in their configured order,
    /// storing the artwork of the first provider that returns it, and tracking the enrichment state of the artwork on the book.
    /// </summary>
    /// <param name="bookEntity">The book whose artwork is resolved.</param>
    /// <param name="artworkProviders">The artwork providers, in their configured order.</param>
    /// <param name="bookArtworkService">The service used to store the artwork of the book.</param>
    /// <param name="fileHashService">The service used to hash the artwork, to detect whether the resolved artwork differs from the stored one.</param>
    /// <param name="libraryName">The name of the media library the book belongs to.</param>
    /// <param name="authorsDisplayNamesByBookId">The display names of the authors of the books of the current page, keyed by the Id of the book.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    private async Task EnrichBookArtworkAsync(BookEntity bookEntity, IReadOnlyList<IArtworkProvider> artworkProviders, IBookArtworkService? bookArtworkService, IFileHashService fileHashService, string libraryName, IReadOnlyDictionary<Guid, string?> authorsDisplayNamesByBookId, CancellationToken cancellationToken)
    {
        // when no artwork provider is available, the book must not be marked as failed to resolve, so the enrichment is skipped
        if (bookArtworkService is null || artworkProviders.Count == 0)
            return;

        ResolvedArtwork? resolvedArtwork = await ResolveArtworkAsync(bookEntity, artworkProviders, cancellationToken).ConfigureAwait(false);
        if (resolvedArtwork is null)
        {
            // no artwork provider returned usable artwork, mark the cover artwork of the book as failed to resolve
            BookArtworkEntity? coverArtwork = bookEntity.BookArtwork.FirstOrDefault(artwork => artwork.ArtworkType == ArtworkType.Cover && artwork.Ordinal == 0);
            if (coverArtwork is null)
            {
                bookEntity.BookArtwork.Add(CreateCoverArtworkEntity(bookEntity.Id, ArtworkStatus.Failed, null, 0, null, null));
            }
            else
            {
                coverArtwork.Status = ArtworkStatus.Failed;
                coverArtwork.UpdatedOnUtc = DateTime.UtcNow;
                coverArtwork.UpdatedBy = Guid.NewGuid();
            }
            return;
        }

        BookArtworkEntity? existingCoverArtwork = bookEntity.BookArtwork.FirstOrDefault(artwork => artwork.ArtworkType == ArtworkType.Cover && artwork.Ordinal == 0);

        // when the artwork comes from a local file, its content hash can be computed before storing it, so that an artwork identical to the stored one
        // is not copied again. For remote artwork, the content hash of the stored artwork is computed after storing it.
        ulong contentHash = 0;
        bool shouldStoreArtwork = true;
        if (!string.IsNullOrWhiteSpace(resolvedArtwork.Artwork.LocalPath))
        {
            contentHash = fileHashService.ComputeFileHash(resolvedArtwork.Artwork.LocalPath);
            shouldStoreArtwork = existingCoverArtwork is null || contentHash != existingCoverArtwork.ContentHash;
        }

        if (shouldStoreArtwork)
        {
            string authorName = authorsDisplayNamesByBookId.TryGetValue(bookEntity.Id, out string? authorDisplayName) && authorDisplayName is not null ? authorDisplayName : string.Empty;
            Result<string> saveArtworkResult = await bookArtworkService.SaveBookArtworkAsync(bookEntity.LibraryId, bookEntity.Id, libraryName, authorName, bookEntity.Title, resolvedArtwork.Artwork, cancellationToken).ConfigureAwait(false);
            if (saveArtworkResult.IsFailure)
            {
                // a failing artwork storage must not prevent the book from being tracked as failed to resolve
                if (existingCoverArtwork is null)
                {
                    bookEntity.BookArtwork.Add(CreateCoverArtworkEntity(bookEntity.Id, ArtworkStatus.Failed, null, 0, null, null));
                }
                else
                {
                    existingCoverArtwork.Status = ArtworkStatus.Failed;
                    existingCoverArtwork.UpdatedOnUtc = DateTime.UtcNow;
                    existingCoverArtwork.UpdatedBy = Guid.NewGuid();
                }
                return;
            }

            // for remote artwork, compute the content hash of the stored artwork, which is a copy of the resolved one
            if (contentHash == 0)
            {
                string storedArtworkPath = Path.Combine(AppContext.BaseDirectory, saveArtworkResult.Value.TrimStart('/', '\\'));
                contentHash = fileHashService.ComputeFileHash(storedArtworkPath);
            }

            if (existingCoverArtwork is null)
            {
                bookEntity.BookArtwork.Add(CreateCoverArtworkEntity(bookEntity.Id, ArtworkStatus.Enriched, saveArtworkResult.Value, contentHash, resolvedArtwork.ProviderName, DateTime.UtcNow));
            }
            else
            {
                existingCoverArtwork.FileName = saveArtworkResult.Value;
                existingCoverArtwork.ContentHash = contentHash;
                existingCoverArtwork.Status = ArtworkStatus.Enriched;
                existingCoverArtwork.Provider = resolvedArtwork.ProviderName;
                existingCoverArtwork.LastUpdateUtc = DateTime.UtcNow;
                existingCoverArtwork.UpdatedOnUtc = DateTime.UtcNow;
                existingCoverArtwork.UpdatedBy = Guid.NewGuid();
            }
        }
        else
        {
            // the resolved artwork is identical to the stored one, so the stored artwork is kept and the book is marked as enriched
            existingCoverArtwork!.Status = ArtworkStatus.Enriched;
            existingCoverArtwork.Provider = resolvedArtwork.ProviderName;
            existingCoverArtwork.LastUpdateUtc = DateTime.UtcNow;
            existingCoverArtwork.UpdatedOnUtc = DateTime.UtcNow;
            existingCoverArtwork.UpdatedBy = Guid.NewGuid();
        }
    }

    /// <summary>
    /// Resolves the artwork of the <paramref name="bookEntity"/> from the <paramref name="artworkProviders"/>, in their configured order,
    /// returning the artwork of the first provider that returns it.
    /// </summary>
    /// <param name="bookEntity">The book whose artwork is resolved.</param>
    /// <param name="artworkProviders">The artwork providers, in their configured order.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>The resolved artwork and the provider that supplied it, or <see langword="null"/> when no provider returned usable artwork.</returns>
    private static async Task<ResolvedArtwork?> ResolveArtworkAsync(BookEntity bookEntity, IReadOnlyList<IArtworkProvider> artworkProviders, CancellationToken cancellationToken)
    {
        BookMetadataLookupDto artworkLookup = new(
            LibraryId: bookEntity.LibraryId,
            Path: bookEntity.Path,
            Isbn: bookEntity.ISBNs.Count > 0 ? bookEntity.ISBNs.Where(isbn => isbn.Value is not null).Select(isbn => isbn.Value!).First() : null,
            OpenLibraryId: bookEntity.OpenLibraryId,
            Title: bookEntity.Title,
            Author: null,
            LanguageCode: bookEntity.LanguageCode
        );

        foreach (IArtworkProvider provider in artworkProviders)
        {
            ArtworkDto? artwork = null;
            try
            {
                artwork = await provider.GetArtworkAsync(artworkLookup, cancellationToken).ConfigureAwait(false);
            }
            catch (Exception)
            {
                // a failing artwork provider must not prevent the other providers from being tried
            }

            if (artwork is null)
                continue;

            // a provider that returns remote artwork must declare that it requires web access, otherwise downloading it would contradict the provider's contract
            if (!string.IsNullOrWhiteSpace(artwork.RemoteUrl) && !provider.RequiresWebAccess)
                continue;

            return new ResolvedArtwork(artwork, provider.Name);
        }
        return null;
    }

    /// <summary>
    /// Creates a cover artwork entity for the book identified by <paramref name="bookId"/>.
    /// </summary>
    /// <param name="bookId">The Id of the book the artwork belongs to.</param>
    /// <param name="status">The status of the artwork enrichment.</param>
    /// <param name="fileName">The relative file name of the stored artwork, if the artwork has been resolved.</param>
    /// <param name="contentHash">The content hash of the stored artwork.</param>
    /// <param name="provider">The name of the plugin that resolved the artwork, if applicable.</param>
    /// <param name="lastUpdateUtc">The date and time when the artwork was resolved, if applicable.</param>
    /// <returns>The created cover artwork entity.</returns>
    private static BookArtworkEntity CreateCoverArtworkEntity(Guid bookId, ArtworkStatus status, string? fileName, ulong contentHash, string? provider, DateTime? lastUpdateUtc)
    {
        return new BookArtworkEntity
        {
            Id = Guid.NewGuid(),
            BookId = bookId,
            ArtworkType = ArtworkType.Cover,
            Ordinal = 0,
            FileName = fileName,
            ContentHash = contentHash,
            Status = status,
            Provider = provider,
            LastUpdateUtc = lastUpdateUtc,
            CreatedOnUtc = DateTime.UtcNow,
            CreatedBy = Guid.Empty,
            UpdatedBy = null
        };
    }

    /// <summary>
    /// Publishes a job progress update.
    /// </summary>
    /// <param name="domainEventPublisher">The service used to publish the progress update.</param>
    /// <param name="compositeKey">The composite unique identifier of a media library scan.</param>
    /// <param name="currentProgress">The current job progress.</param>
    /// <param name="totalProgress">The total job progress.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="Result{TValue}"/> representing either a successful operation, or an error.</returns>
    private async Task<Result<Success>> PublishJobProgressAsync(IDomainEventPublisher domainEventPublisher, MediaLibraryScanCompositeId compositeKey, int currentProgress, int totalProgress, CancellationToken cancellationToken)
    {
        Result<MediaLibraryScanJobProgress> scanJobProgressResult = MediaLibraryScanJobProgress.Create(currentProgress, totalProgress, "ResolvingArtwork");
        if (scanJobProgressResult.IsFailure)
            return scanJobProgressResult.Errors;

        await domainEventPublisher.PublishAsync(new LibraryScanJobProgressChangedDomainEvent(
            Guid.NewGuid(), LibraryId, compositeKey, scanJobProgressResult.Value, DateTime.UtcNow), cancellationToken).ConfigureAwait(false);

        return Result.Success;
    }

    /// <summary>
    /// The artwork resolved for a book, along with the provider that supplied it.
    /// </summary>
    /// <param name="Artwork">The resolved artwork of the book.</param>
    /// <param name="ProviderName">The name of the provider that supplied the artwork.</param>
    private sealed record ResolvedArtwork(ArtworkDto Artwork, string ProviderName);
}
