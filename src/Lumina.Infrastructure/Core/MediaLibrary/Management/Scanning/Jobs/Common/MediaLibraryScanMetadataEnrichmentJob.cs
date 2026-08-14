#region ========================================================================= USING =====================================================================================
using Lumina.Domain.Common.Primitives;
using Lumina.Application.Common.DataAccess.Entities.MediaLibrary.WrittenContentLibrary.BookLibrary;
using Lumina.Application.Common.DataAccess.Entities.Plugins;
using Lumina.Application.Common.DataAccess.Repositories.Books;
using Lumina.Application.Common.DataAccess.Repositories.Plugins;
using Lumina.Application.Common.DataAccess.UoW;
using Lumina.Application.Common.Mapping.MediaLibrary.WrittenContentLibrary.BookLibrary.Books;
using Lumina.Contracts.DTO.Common;
using Lumina.Contracts.DTO.MediaLibrary.WrittenContentLibrary.BookLibrary;
using Lumina.Domain.Common.Events;
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
                    ILibraryMetadataProviderConfigurationRepository configurationRepository = unitOfWork.GetRepository<ILibraryMetadataProviderConfigurationRepository>();
                    IBookRepository bookRepository = unitOfWork.GetRepository<IBookRepository>();

                    MediaLibraryScanCompositeId compositeKey = MediaLibraryScanCompositeId.Create(ScanId, UserId);

                    // get the metadata providers configured for the media library, in their configured order, that support the media library type
                    Result<IReadOnlyList<LibraryMetadataProviderConfigurationEntity>> getConfigurationsResult = await configurationRepository.GetByLibraryIdAsync(LibraryId.Value, cancellationToken).ConfigureAwait(false);
                    if (getConfigurationsResult.IsFailure)
                        throw new InvalidOperationException(getConfigurationsResult.FirstError.Description);
                    List<IRemoteMetadataProvider> providers = [];
                    foreach (LibraryMetadataProviderConfigurationEntity configuration in getConfigurationsResult.Value.Where(configuration => configuration.IsEnabled).OrderBy(configuration => configuration.Rank))
                    {
                        List<IRemoteMetadataProvider> configuredProviders = [.. asyncServiceScope.ServiceProvider
                            .GetKeyedServices<IRemoteMetadataProvider>(configuration.PluginId)
                            .Where(provider => provider.SupportedLibraryType == LibraryType.Book)];
                        if (configuredProviders.Count == 0)
                            _logger.LogWarning("No metadata provider was found for the configured plugin with Id '{PluginId}' and the {LibraryType} library type.", configuration.PluginId, LibraryType.Book);
                        providers.AddRange(configuredProviders);
                    }

                    // count the books that need their metadata enriched, for progress reporting purposes
                    Result<int> getBooksToEnrichCountResult = await bookRepository.GetBooksNeedingMetadataCountAsync(LibraryId.Value, cancellationToken).ConfigureAwait(false);
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
                    string? lastPath = null;

                    // process the books that need their metadata enriched in pages, keeping the peak memory bounded regardless of the library size
                    while (true)
                    {
                        cancellationToken.ThrowIfCancellationRequested();

                        Result<IReadOnlyList<BookEntity>> getBooksPageResult = await bookRepository.GetBooksNeedingMetadataAsync(LibraryId.Value, lastPath, ENRICHMENT_PAGE_SIZE, cancellationToken).ConfigureAwait(false);
                        if (getBooksPageResult.IsFailure)
                            throw new InvalidOperationException(getBooksPageResult.FirstError.Description);
                        IReadOnlyList<BookEntity> booksPage = getBooksPageResult.Value;
                        if (booksPage.Count == 0)
                            break;

                        foreach (BookEntity bookEntity in booksPage)
                        {
                            cancellationToken.ThrowIfCancellationRequested();

                            await EnrichBookAsync(bookEntity, providers, bookRepository, cancellationToken).ConfigureAwait(false);

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

                        lastPath = booksPage[^1].Path;
                    }

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
    /// Enriches the metadata of the provided <paramref name="bookEntity"/>, using the provided <paramref name="providers"/> in their configured order.
    /// </summary>
    /// <param name="bookEntity">The book whose metadata is enriched.</param>
    /// <param name="providers">The metadata providers, in their configured order.</param>
    /// <param name="bookRepository">The repository used to persist the enriched metadata.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    private static async Task EnrichBookAsync(BookEntity bookEntity, IReadOnlyList<IRemoteMetadataProvider> providers, IBookRepository bookRepository, CancellationToken cancellationToken)
    {
        // convert the book to a domain object
        Result<Book> getBookResult = bookEntity.ToDomainEntity();
        if (getBookResult.IsFailure)
            return;
        Book book = getBookResult.Value;

        BookMetadataLookupDto lookup = new(
            LibraryId: bookEntity.LibraryId,
            Path: bookEntity.Path,
            Isbn: bookEntity.ISBNs.Count > 0 ? bookEntity.ISBNs.Where(isbn => isbn.Value is not null).Select(isbn => isbn.Value!).First() : null, // TODO: this should not take just the first ISBN, it should ask the metadata plugin to retry with the next ISBN, if the first one returned no results
            OpenLibraryId: bookEntity.OpenLibraryId,
            Title: bookEntity.Title,
            Author: null,
            LanguageCode: bookEntity.LanguageCode
        );

        // try the metadata providers in order, until one returns usable metadata
        foreach (IRemoteMetadataProvider provider in providers)
        {
            try
            {
                MetadataDto? metadataResult = await provider.GetMetadataAsync(lookup, cancellationToken).ConfigureAwait(false);
                if (metadataResult is not BookMetadataDto bookMetadata || !IsUsableMetadata(bookMetadata))
                    continue;

                Result<Success> applyMetadataResult = book.ApplyMetadata(bookMetadata, provider.Name, DateTime.UtcNow);
                if (applyMetadataResult.IsFailure)
                    continue;

                Result<Updated> updateBookResult = await bookRepository.UpdateAsync(book.ToRepositoryEntity(), cancellationToken).ConfigureAwait(false);
                if (updateBookResult.IsFailure)
                    continue;

                return;
            }
            catch (Exception)
            {
                // a failing metadata provider must not prevent the other providers from being tried
            }
        }

        // no metadata provider returned usable metadata, mark the book as failed to enrich
        book.MarkMetadataAsFailed();
        await bookRepository.UpdateAsync(book.ToRepositoryEntity(), cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Determines whether the provided <paramref name="metadata"/> is usable, meaning it has a title and at least one primary identifier.
    /// </summary>
    /// <param name="metadata">The metadata to validate.</param>
    /// <returns><see langword="true"/> when the metadata is usable, otherwise <see langword="false"/>.</returns>
    private static bool IsUsableMetadata(BookMetadataDto metadata)
    {
        if (string.IsNullOrWhiteSpace(metadata.Title))
            return false;
        return !string.IsNullOrWhiteSpace(metadata.GoodreadsId)
            || !string.IsNullOrWhiteSpace(metadata.OpenLibraryId)
            || !string.IsNullOrWhiteSpace(metadata.GoogleBooksId)
            || !string.IsNullOrWhiteSpace(metadata.ASIN)
            || (metadata.Isbns is not null && metadata.Isbns.Count > 0);
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
