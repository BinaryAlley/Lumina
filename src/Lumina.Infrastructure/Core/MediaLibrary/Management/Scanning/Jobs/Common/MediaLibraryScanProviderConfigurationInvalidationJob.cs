#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.MediaLibrary.Management;
using Lumina.Application.Common.DataAccess.Entities.Plugins;
using Lumina.Application.Common.DataAccess.Entities.UsersManagement;
using Lumina.Application.Common.DataAccess.UoW;
using Lumina.Domain.Common.Events;
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryScanAggregate.Events;
using Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryScanAggregate.Services.Jobs;
using Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryScanAggregate.ValueObjects;
using Lumina.Domain.SharedKernel.Common.Enums.MediaLibrary;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Infrastructure.Core.MediaLibrary.Management.Scanning.Jobs.Common;

/// <summary>
/// Media library scan job for invalidating the enrichment state of the books whose metadata or artwork provider configuration
/// changed since the last scan, so that they are re-enriched by the enrichment jobs that follow.
/// </summary>
internal sealed class MediaLibraryScanProviderConfigurationInvalidationJob : MediaLibraryScanJob, IMediaLibraryScanProviderConfigurationInvalidationJob
{
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly ILogger<MediaLibraryScanProviderConfigurationInvalidationJob> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="MediaLibraryScanProviderConfigurationInvalidationJob"/> class.
    /// </summary>
    /// <param name="serviceScopeFactory">
    /// Injected factory for creating scopes in which services are requested.
    /// See docs/technical/architecture/architecture-knowledge-management/architecture-decision-log/architecture-decision-record-0001.md for details.
    /// </param>
    /// <param name="logger">Injected logger used to report the issues encountered while invalidating the enrichment state.</param>
    public MediaLibraryScanProviderConfigurationInvalidationJob(IServiceScopeFactory serviceScopeFactory, ILogger<MediaLibraryScanProviderConfigurationInvalidationJob> logger)
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

                    // set the initial progress of the scan job, it's a 1 step job - invalidating the stale enrichment state
                    Result<Success> publishJobProgressResult = await PublishJobProgressAsync(domainEventPublisher, compositeKey, 0, 1, cancellationToken).ConfigureAwait(false);
                    if (publishJobProgressResult.IsFailure)
                        throw new InvalidOperationException(publishJobProgressResult.FirstError.Description);

                    // load the media library, whose web access setting determines the effective provider set, and whose stored fingerprints
                    // are compared against the current provider configuration to detect whether it changed since the last scan
                    Result<LibraryEntity?> getLibraryResult = await unitOfWork.LibraryRepository.GetByIdAsync(LibraryId.Value, cancellationToken).ConfigureAwait(false);
                    if (getLibraryResult.IsFailure || getLibraryResult.Value is null)
                        throw new InvalidOperationException(getLibraryResult.IsFailure ? getLibraryResult.FirstError.Description : "The media library was not found.");
                    LibraryEntity library = getLibraryResult.Value;

                    // load the provider configurations of the media library, whose fingerprint is compared against the stored one
                    Result<IReadOnlyList<LibraryMetadataProviderConfigurationEntity>> getMetadataConfigurationsResult = await unitOfWork.LibraryMetadataProviderConfigurationRepository.GetByLibraryIdAsync(LibraryId.Value, cancellationToken).ConfigureAwait(false);
                    if (getMetadataConfigurationsResult.IsFailure)
                        throw new InvalidOperationException(getMetadataConfigurationsResult.FirstError.Description);

                    Result<IReadOnlyList<LibraryArtworkProviderConfigurationEntity>> getArtworkConfigurationsResult = await unitOfWork.ArtworkProviderConfigurationRepository.GetByLibraryIdAsync(LibraryId.Value, cancellationToken).ConfigureAwait(false);
                    if (getArtworkConfigurationsResult.IsFailure)
                        throw new InvalidOperationException(getArtworkConfigurationsResult.FirstError.Description);

                    // read whether the metadata of the books of the user is aggregated from multiple providers, when fields are missing
                    bool shouldAggregateMetadataWhenMissing = false;
                    if (unitOfWork.UserSettingsRepository is not null)
                    {
                        Result<UserSettingsEntity?> getUserSettingsResult = await unitOfWork.UserSettingsRepository.GetByUserIdAsync(UserId.Value, cancellationToken).ConfigureAwait(false);
                        if (getUserSettingsResult.IsFailure)
                            _logger.LogWarning("Failed to read the user settings, the metadata provider configuration fingerprint will not include the metadata aggregation setting.");
                        else
                            shouldAggregateMetadataWhenMissing = getUserSettingsResult.Value?.ShouldAggregateMetadataWhenMissing ?? false;
                    }

                    // compare the current fingerprints against the stored ones, resetting the enrichment state of the channel whose configuration
                    // changed, so that the enrichment jobs that follow re-enrich the books. A missing stored fingerprint means the configuration
                    // was never recorded yet, so the current state of the books is trusted, and only the new fingerprint is stored.
                    string metadataFingerprint = ProviderConfigurationFingerprint.ComputeMetadataFingerprint(getMetadataConfigurationsResult.Value, shouldAggregateMetadataWhenMissing, library.CanDownloadMetadataFromWeb);
                    string artworkFingerprint = ProviderConfigurationFingerprint.ComputeArtworkFingerprint(getArtworkConfigurationsResult.Value, library.CanDownloadMetadataFromWeb);

                    if (library.MetadataProvidersConfigurationFingerprint is not null && library.MetadataProvidersConfigurationFingerprint != metadataFingerprint)
                    {
                        Result<Updated> resetMetadataResult = await unitOfWork.BookRepository.ResetMetadataStatusForLibraryAsync(LibraryId.Value, cancellationToken).ConfigureAwait(false);
                        if (resetMetadataResult.IsFailure)
                            throw new InvalidOperationException(resetMetadataResult.FirstError.Description);
                    }

                    if (library.ArtworkProvidersConfigurationFingerprint is not null && library.ArtworkProvidersConfigurationFingerprint != artworkFingerprint)
                    {
                        Result<Updated> resetArtworkResult = await unitOfWork.BookRepository.ResetArtworkStatusForLibraryAsync(LibraryId.Value, cancellationToken).ConfigureAwait(false);
                        if (resetArtworkResult.IsFailure)
                            throw new InvalidOperationException(resetArtworkResult.FirstError.Description);
                    }

                    // store the current fingerprints, so that the next scan can detect whether the provider configuration changed again.
                    // the library is already tracked by the change tracker, so the modifications are persisted directly, without going through the repository
                    // update action, whose clearing and re-adding of the owned content locations would drop them when the same tracked instance is passed
                    library.MetadataProvidersConfigurationFingerprint = metadataFingerprint;
                    library.ArtworkProvidersConfigurationFingerprint = artworkFingerprint;
                    library.UpdatedOnUtc = DateTime.UtcNow;
                    library.UpdatedBy = Guid.NewGuid();

                    await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

                    // increment the number of processed elements progress
                    publishJobProgressResult = await PublishJobProgressAsync(domainEventPublisher, compositeKey, 1, 1, cancellationToken).ConfigureAwait(false);
                    if (publishJobProgressResult.IsFailure)
                        throw new InvalidOperationException(publishJobProgressResult.FirstError.Description);

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
        Result<MediaLibraryScanJobProgress> scanJobProgressResult = MediaLibraryScanJobProgress.Create(currentProgress, totalProgress, "InvalidatingStaleEnrichment");
        if (scanJobProgressResult.IsFailure)
            return scanJobProgressResult.Errors;

        await domainEventPublisher.PublishAsync(new LibraryScanJobProgressChangedDomainEvent(
            Guid.NewGuid(), LibraryId, compositeKey, scanJobProgressResult.Value, DateTime.UtcNow), cancellationToken).ConfigureAwait(false);

        return Result.Success;
    }
}
