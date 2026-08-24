#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.MediaLibrary.Management;
using Lumina.Application.Common.DataAccess.Entities.MediaLibrary.WrittenContentLibrary.BookLibrary;
using Lumina.Application.Common.DataAccess.Entities.Plugins;
using Lumina.Application.Common.DataAccess.Entities.UsersManagement;
using Lumina.Application.Common.DataAccess.Repositories.Books;
using Lumina.Application.Common.DataAccess.UoW;
using Lumina.Application.Common.Mapping.MediaLibrary.WrittenContentLibrary.BookLibrary.Books;
using Lumina.Application.Core.MediaLibrary.WrittenContentLibrary.BooksLibrary.Artwork;
using Lumina.Contracts.DTO.Common;
using Lumina.Contracts.DTO.MediaLibrary.WrittenContentLibrary.BookLibrary;
using Lumina.Domain.Common.Events;
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryScanAggregate.Events;
using Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryScanAggregate.Services.Jobs;
using Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryScanAggregate.ValueObjects;
using Lumina.Domain.Core.BoundedContexts.WrittenContentLibraryBoundedContext.BookLibraryAggregate;
using Lumina.Domain.SharedKernel.Common.Enums.MediaLibrary;
using Lumina.Plugins.Contracts.Core.Metadata;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Infrastructure.Core.MediaLibrary.Management.Scanning.Jobs.Common;

/// <summary>
/// Media library scan job for enriching the metadata of the media library items, using the metadata providers configured for the media library.
/// This should always be the last job in the directed acyclic job graph.
/// </summary>
internal sealed class MediaLibraryScanMetadataEnrichmentJob : MediaLibraryScanJob, IMediaLibraryScanMetadataEnrichmentJob
{
    private const int ENRICHMENT_PAGE_SIZE = 1000; // the number of books that are enriched in a single batch, keeping the peak memory bounded regardless of the library size
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly ILogger<MediaLibraryScanMetadataEnrichmentJob> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="MediaLibraryScanMetadataEnrichmentJob"/> class.
    /// </summary>
    /// <param name="serviceScopeFactory">
    /// Injected factory for creating scopes in which services are requested.
    /// See docs/technical/architecture/architecture-knowledge-management/architecture-decision-log/architecture-decision-record-0001.md for details.
    /// </param>
    /// <param name="logger">Injected logger used to report the issues encountered while enriching the metadata.</param>
    public MediaLibraryScanMetadataEnrichmentJob(IServiceScopeFactory serviceScopeFactory, ILogger<MediaLibraryScanMetadataEnrichmentJob> logger)
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

                    // get the metadata providers configured for the media library, in their configured order, that support the media library type,
                    // skipping the providers that require access to the web when the media library does not permit downloading data from the web
                    Result<IReadOnlyList<LibraryMetadataProviderConfigurationEntity>> getConfigurationsResult = await unitOfWork.LibraryMetadataProviderConfigurationRepository.GetByLibraryIdAsync(LibraryId.Value, cancellationToken).ConfigureAwait(false);
                    if (getConfigurationsResult.IsFailure)
                        throw new InvalidOperationException(getConfigurationsResult.FirstError.Description);
                    List<IMetadataProvider> providers = [];
                    foreach (LibraryMetadataProviderConfigurationEntity configuration in getConfigurationsResult.Value.Where(configuration => configuration.IsEnabled).OrderBy(configuration => configuration.Rank))
                    {
                        List<IMetadataProvider> configuredProviders = [.. asyncServiceScope.ServiceProvider
                            .GetKeyedServices<IMetadataProvider>(configuration.PluginId)
                            .Where(provider => provider.SupportedLibraryTypes.Contains(LibraryType.Book)
                                && (canDownloadMetadataFromWeb || !provider.RequiresWebAccess))];
                        if (configuredProviders.Count == 0)
                            _logger.LogWarning("No metadata provider was found for the configured plugin with Id '{PluginId}' and the {LibraryType} library type.", configuration.PluginId, LibraryType.Book);
                        providers.AddRange(configuredProviders);
                    }

                    // read whether the metadata of the books of the user is aggregated from multiple providers, when fields are missing
                    bool shouldAggregateMetadataWhenMissing = false;
                    if (unitOfWork.UserSettingsRepository is not null)
                    {
                        Result<UserSettingsEntity?> getUserSettingsResult = await unitOfWork.UserSettingsRepository.GetByUserIdAsync(UserId.Value, cancellationToken).ConfigureAwait(false);
                        if (getUserSettingsResult.IsFailure)
                            _logger.LogWarning("Failed to read the user settings, the metadata will not be aggregated across providers.");
                        else
                            shouldAggregateMetadataWhenMissing = getUserSettingsResult.Value?.ShouldAggregateMetadataWhenMissing ?? false;
                    }

                    // get the artwork providers configured for the media library, in their configured order, that support the media library type.
                    // the artwork resolution is best-effort, so a failure to read the artwork configurations must not prevent the metadata enrichment from proceeding
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

                    IBookArtworkService? bookArtworkService = asyncServiceScope.ServiceProvider.GetService<IBookArtworkService>();

                    // count the books that need their metadata enriched, for progress reporting purposes
                    string? lastPath = null;

                    // when no metadata provider is available, the books must not be marked as failed to enrich, so the enrichment is skipped entirely
                    if (providers.Count > 0)
                    {
                        Result<int> getBooksToEnrichCountResult = await unitOfWork.BookRepository.GetBooksNeedingMetadataCountAsync(LibraryId.Value, cancellationToken).ConfigureAwait(false);
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

                        // process the books that need their metadata enriched in pages, keeping the peak memory bounded regardless of the library size
                        while (true)
                        {
                            cancellationToken.ThrowIfCancellationRequested();

                            Result<IReadOnlyList<BookEntity>> getBooksPageResult = await unitOfWork.BookRepository.GetBooksNeedingMetadataAsync(LibraryId.Value, lastPath, ENRICHMENT_PAGE_SIZE, cancellationToken).ConfigureAwait(false);
                            if (getBooksPageResult.IsFailure)
                                throw new InvalidOperationException(getBooksPageResult.FirstError.Description);
                            IReadOnlyList<BookEntity> booksPage = getBooksPageResult.Value;
                            if (booksPage.Count == 0)
                                break;

                            foreach (BookEntity bookEntity in booksPage)
                            {
                                cancellationToken.ThrowIfCancellationRequested();

                                await EnrichBookAsync(bookEntity, providers, shouldAggregateMetadataWhenMissing, artworkProviders, bookArtworkService, libraryName, unitOfWork.BookRepository, cancellationToken).ConfigureAwait(false);

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
                        _logger.LogWarning("No metadata provider is configured for the media library with Id '{LibraryId}', the metadata enrichment will be skipped.", LibraryId.Value);
                    }

                    // persist the changes when no page was processed, so that the scan still saves even when there are no books to enrich
                    if (lastPath is null)
                        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

                    // this job finished, and it's the last in the chain, the scan is completed
                    Status = LibraryScanJobStatus.Completed;
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
    /// Enriches the metadata and the artwork of the provided <paramref name="bookEntity"/>, using the provided metadata and artwork providers in their configured order.
    /// </summary>
    /// <param name="bookEntity">The book whose metadata and artwork are enriched.</param>
    /// <param name="metadataProviders">The metadata providers, in their configured order.</param>
    /// <param name="shouldAggregateMetadataWhenMissing">Whether the metadata of the book is aggregated from multiple providers, when fields are missing, or not.</param>
    /// <param name="artworkProviders">The artwork providers, in their configured order.</param>
    /// <param name="bookArtworkService">The service used to store the artwork of the book.</param>
    /// <param name="libraryName">The name of the media library the book belongs to.</param>
    /// <param name="bookRepository">The repository used to persist the enriched book.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    private async Task EnrichBookAsync(BookEntity bookEntity, IReadOnlyList<IMetadataProvider> metadataProviders, bool shouldAggregateMetadataWhenMissing, IReadOnlyList<IArtworkProvider> artworkProviders, IBookArtworkService? bookArtworkService, string libraryName, IBookRepository bookRepository, CancellationToken cancellationToken)
    {
        // when no metadata provider is available, the book must not be marked as failed to enrich, so the enrichment is skipped
        if (metadataProviders.Count == 0)
            return;

        // convert the book to a domain object
        Result<Book> getBookResult = bookEntity.ToDomainEntity();
        if (getBookResult.IsFailure)
            return;
        Book book = getBookResult.Value;

        BookMetadataLookupDto bookMetadataLookup = new(
            LibraryId: bookEntity.LibraryId,
            Path: bookEntity.Path,
            Isbn: bookEntity.ISBNs.Count > 0 ? bookEntity.ISBNs.Where(isbn => isbn.Value is not null).Select(isbn => isbn.Value!).First() : null, // TODO: this should not take just the first ISBN, it should ask the metadata plugin to retry with the next ISBN, if the first one returned no results
            OpenLibraryId: bookEntity.OpenLibraryId,
            Title: bookEntity.Title,
            Author: null,
            LanguageCode: bookEntity.LanguageCode
        );

        BookMetadataDto? appliedMetadata = shouldAggregateMetadataWhenMissing
            ? await AggregateMetadataAsync(book, bookMetadataLookup, metadataProviders, cancellationToken).ConfigureAwait(false)
            : await TryFirstProviderMetadataAsync(book, bookMetadataLookup, metadataProviders, cancellationToken).ConfigureAwait(false);

        if (appliedMetadata is null)
        {
            // no metadata provider returned usable metadata, mark the book as failed to enrich
            book.MarkMetadataAsFailed();
            Result<Updated> markFailedResult = await bookRepository.UpdateAsync(book.ToRepositoryEntity(), cancellationToken).ConfigureAwait(false);
            if (markFailedResult.IsFailure)
                _logger.LogWarning("Failed to persist the failed enrichment state of the book with Id '{BookId}' at path '{BookPath}'.", book.Id.Value, book.Path);
            return;
        }

        // resolve the artwork of the book from the artwork providers, in their configured order
        await EnrichBookArtworkAsync(book, appliedMetadata, artworkProviders, bookArtworkService, libraryName, cancellationToken).ConfigureAwait(false);

        Result<Updated> updateBookResult = await bookRepository.UpdateAsync(book.ToRepositoryEntity(), cancellationToken).ConfigureAwait(false);
        if (updateBookResult.IsFailure)
            _logger.LogWarning("Failed to persist the enriched book with Id '{BookId}' at path '{BookPath}'.", book.Id.Value, book.Path);
    }

    /// <summary>
    /// Tries the <paramref name="metadataProviders"/> in order, applying the metadata of the first one that returns usable metadata.
    /// </summary>
    /// <param name="book">The book onto which the metadata is applied.</param>
    /// <param name="bookMetadataLookup">The lookup describing the book.</param>
    /// <param name="metadataProviders">The metadata providers, in their configured order.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>The applied metadata, or <see langword="null"/> when no provider returned usable metadata.</returns>
    private static async Task<BookMetadataDto?> TryFirstProviderMetadataAsync(Book book, BookMetadataLookupDto bookMetadataLookup, IReadOnlyList<IMetadataProvider> metadataProviders, CancellationToken cancellationToken)
    {
        foreach (IMetadataProvider metadataProvider in metadataProviders)
        {
            try
            {
                MetadataDto? metadataResult = await metadataProvider.GetMetadataAsync(bookMetadataLookup, cancellationToken).ConfigureAwait(false);
                if (metadataResult is not BookMetadataDto bookMetadata || !IsUsableMetadata(bookMetadata))
                    continue;

                Result<Success> applyMetadataResult = book.ApplyMetadata(bookMetadata, metadataProvider.Name, DateTime.UtcNow);
                if (applyMetadataResult.IsFailure)
                    continue;
                return bookMetadata;
            }
            catch (Exception)
            {
                // a failing metadata provider must not prevent the other providers from being tried
            }
        }
        return null;
    }

    /// <summary>
    /// Aggregates the metadata of the <paramref name="book"/> from all the <paramref name="metadataProviders"/>, in their configured order, filling the fields that the earlier providers lack with the values of the later ones.
    /// </summary>
    /// <param name="book">The book onto which the aggregated metadata is applied.</param>
    /// <param name="bookMetadataLookup">The lookup describing the book.</param>
    /// <param name="metadataProviders">The metadata providers, in their configured order.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>The applied aggregated metadata, or <see langword="null"/> when no provider returned usable metadata.</returns>
    private static async Task<BookMetadataDto?> AggregateMetadataAsync(Book book, BookMetadataLookupDto bookMetadataLookup, IReadOnlyList<IMetadataProvider> metadataProviders, CancellationToken cancellationToken)
    {
        BookMetadataDto? mergedMetadata = null;
        List<string> contributingProviders = [];

        foreach (IMetadataProvider metadataProvider in metadataProviders)
        {
            try
            {
                MetadataDto? metadataResult = await metadataProvider.GetMetadataAsync(bookMetadataLookup, cancellationToken).ConfigureAwait(false);
                if (metadataResult is not BookMetadataDto bookMetadata || !IsUsableMetadata(bookMetadata))
                    continue;

                mergedMetadata = mergedMetadata is null ? bookMetadata : MetadataAggregator.Merge(mergedMetadata, bookMetadata);
                contributingProviders.Add(metadataProvider.Name);
                // the identifiers and the title discovered by the earlier providers are fed to the later ones, so that they can look up the book precisely
                bookMetadataLookup = EnrichLookup(bookMetadataLookup, mergedMetadata);
            }
            catch (Exception)
            {
                // a failing metadata provider must not prevent the other providers from being tried
            }
        }

        if (mergedMetadata is null)
            return null;

        Result<Success> applyMetadataResult = book.ApplyMetadata(mergedMetadata, string.Join(", ", contributingProviders.Distinct(StringComparer.Ordinal)), DateTime.UtcNow);
        return applyMetadataResult.IsFailure ? null : mergedMetadata;
    }

    /// <summary>
    /// Resolves the artwork of the <paramref name="book"/> from the <paramref name="artworkProviders"/>, in their configured order, storing the artwork of the first provider that returns it.
    /// </summary>
    /// <param name="book">The book whose artwork is enriched.</param>
    /// <param name="appliedMetadata">The metadata applied to the book, used to build the directory of the book artwork.</param>
    /// <param name="artworkProviders">The artwork providers, in their configured order.</param>
    /// <param name="bookArtworkService">The service used to store the artwork of the book.</param>
    /// <param name="libraryName">The name of the media library the book belongs to.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    private static async Task EnrichBookArtworkAsync(Book book, BookMetadataDto appliedMetadata, IReadOnlyList<IArtworkProvider> artworkProviders, IBookArtworkService? bookArtworkService, string libraryName, CancellationToken cancellationToken)
    {
        if (bookArtworkService is null || artworkProviders.Count == 0)
            return;

        foreach (IArtworkProvider provider in artworkProviders)
        {
            ArtworkDto? artwork = null;
            try
            {
                artwork = await provider.GetArtworkAsync(new BookMetadataLookupDto(LibraryId: book.LibraryId.Value, Path: book.Path), cancellationToken).ConfigureAwait(false);
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

            string authorName = appliedMetadata.Contributors?.FirstOrDefault(contributor => contributor.Name?.DisplayName is not null)?.Name?.DisplayName ?? string.Empty;
            string bookTitle = appliedMetadata.Title ?? book.Metadata.Title;
            Result<string> saveArtworkResult = await bookArtworkService.SaveBookArtworkAsync(book.LibraryId.Value, book.Id.Value, libraryName, authorName, bookTitle, artwork, cancellationToken).ConfigureAwait(false);
            if (saveArtworkResult.IsFailure)
                continue;

            book.SetBookCoverImagePath(saveArtworkResult.Value);
            return;
        }
    }

    /// <summary>
    /// Enriches the <paramref name="bookMetadataLookup"/> with the identifiers and the title discovered by the already queried metadata providers.
    /// </summary>
    /// <param name="bookMetadataLookup">The lookup to enrich.</param>
    /// <param name="bookMetadata">The metadata discovered by the already queried metadata providers.</param>
    /// <returns>The enriched lookup.</returns>
    private static BookMetadataLookupDto EnrichLookup(BookMetadataLookupDto bookMetadataLookup, BookMetadataDto bookMetadata)
    {
        string? isbn = bookMetadataLookup.Isbn ?? bookMetadata.Isbns?.FirstOrDefault()?.Value;
        string? openLibraryId = bookMetadataLookup.OpenLibraryId ?? bookMetadata.OpenLibraryId;
        string? title = bookMetadataLookup.Title ?? bookMetadata.Title;
        return bookMetadataLookup with
        {
            Isbn = isbn,
            OpenLibraryId = openLibraryId,
            Title = title
        };
    }

    /// <summary>
    /// Determines whether the provided <paramref name="bookMetadata"/> is usable, meaning it has a title.
    /// </summary>
    /// <param name="bookMetadata">The metadata to validate.</param>
    /// <returns><see langword="true"/> when the metadata is usable, otherwise <see langword="false"/>.</returns>
    private static bool IsUsableMetadata(BookMetadataDto bookMetadata)
    {
        return !string.IsNullOrWhiteSpace(bookMetadata.Title);
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
        Result<MediaLibraryScanJobProgress> scanJobProgressResult = MediaLibraryScanJobProgress.Create(currentProgress, totalProgress, "EnrichingMetadata");
        if (scanJobProgressResult.IsFailure)
            return scanJobProgressResult.Errors;

        await domainEventPublisher.PublishAsync(new LibraryScanJobProgressChangedDomainEvent(
            Guid.NewGuid(), LibraryId, compositeKey, scanJobProgressResult.Value, DateTime.UtcNow), cancellationToken).ConfigureAwait(false);

        return Result.Success;
    }
}
