#region ========================================================================= USING =====================================================================================
using Lumina.Domain.Common.Primitives;
using Lumina.Application.Common.DataAccess.Entities.MediaLibrary.WrittenContentLibrary.BookLibrary;
using Lumina.Application.Common.DataAccess.Repositories.Books;
using Lumina.Application.Common.DataAccess.Repositories.MediaLibrary;
using Lumina.Application.Common.DataAccess.UoW;
using Lumina.Domain.SharedKernel.Common.Enums.BookLibrary;
using Lumina.Domain.SharedKernel.Common.Enums.MediaLibrary;
using Lumina.Domain.Common.Events;
using Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryScanAggregate.Events;
using Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryScanAggregate.Services.Jobs;
using Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryScanAggregate.ValueObjects;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Infrastructure.Core.MediaLibrary.Management.Scanning.Jobs.Common;

/// <summary>
/// Media library scan job for persisting the results of the current scan. This should always be the last job in the directed acyclic job graph.
/// </summary>
internal sealed class MediaLibraryScanResultsSaveJob : MediaLibraryScanJob, IMediaLibraryScanResultsSaveJob
{
    private readonly IServiceScopeFactory _serviceScopeFactory;

    /// <summary>
    /// Initializes a new instance of the <see cref="MediaLibraryScanResultsSaveJob"/> class.
    /// </summary>
    /// <param name="serviceScopeFactory">
    /// Injected factory for creating scopes in which services are requested.
    /// See docs/technical/architecture/architecture-knowledge-management/architecture-decision-log/architecture-decision-record-0001.md for details.
    /// </param>
    public MediaLibraryScanResultsSaveJob(IServiceScopeFactory serviceScopeFactory)
    {
        _serviceScopeFactory = serviceScopeFactory;
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

                    // set the initial progress of the scan job, it's a 1 step job - applying the scan results to the storage medium
                    Result<Success> publishJobProgressResult = await PublishJobProgressAsync(domainEventPublisher, compositeKey, 0, 1, cancellationToken).ConfigureAwait(false);
                    if (publishJobProgressResult.IsFailure)
                        throw new InvalidOperationException(publishJobProgressResult.FirstError.Description);

                    // get the paths of the media library scan snapshot items that are no longer present in the current scan, so that deletion events can be raised for them
                    Result<IReadOnlyList<string>> getDeletedPathsResult = await unitOfWork.LibraryScanSnapshotRepository.GetDeletedPathsAsync(LibraryId.Value, ScanId.Value, cancellationToken).ConfigureAwait(false);
                    if (getDeletedPathsResult.IsFailure)
                        throw new InvalidOperationException(getDeletedPathsResult.FirstError.Description);

                    // apply the results of the current scan to the storage medium, atomically replacing the media library scan snapshot of the previous scan
                    Result<Updated> applySnapshotSwapResult = await unitOfWork.LibraryScanSnapshotRepository.ApplySnapshotSwapAsync(LibraryId.Value, ScanId.Value, UserId.Value, cancellationToken).ConfigureAwait(false);
                    if (applySnapshotSwapResult.IsFailure)
                        throw new InvalidOperationException(applySnapshotSwapResult.FirstError.Description);

                    // raise a deletion event for every media library scan snapshot item that is no longer present on disk
                    foreach (string deletedPath in getDeletedPathsResult.Value)
                    {
                        cancellationToken.ThrowIfCancellationRequested();
                        await domainEventPublisher.PublishAsync(new LibraryMediaItemDeletedDomainEvent(Guid.NewGuid(), LibraryId, compositeKey, deletedPath, DateTime.UtcNow), cancellationToken).ConfigureAwait(false);
                    }

                    // materialize the books of the media library from the scan snapshot, so that they are browsable even without web metadata
                    Result<IReadOnlyList<string>> getPathsResult = await unitOfWork.LibraryScanSnapshotRepository.GetPathsAsync(LibraryId.Value, cancellationToken).ConfigureAwait(false);
                    if (getPathsResult.IsFailure)
                        throw new InvalidOperationException(getPathsResult.FirstError.Description);

                    foreach (string path in getPathsResult.Value)
                    {
                        cancellationToken.ThrowIfCancellationRequested();

                        Result<BookEntity?> getExistingBookResult = await unitOfWork.BookRepository.GetByPathAsync(LibraryId.Value, path, cancellationToken).ConfigureAwait(false);
                        if (getExistingBookResult.IsFailure)
                            throw new InvalidOperationException(getExistingBookResult.FirstError.Description);
                        if (getExistingBookResult.Value is not null)
                            continue;

                        Result<Created> insertBookResult = await unitOfWork.BookRepository.InsertAsync(CreateShellBookEntity(LibraryId.Value, path), cancellationToken).ConfigureAwait(false);
                        if (insertBookResult.IsFailure)
                            throw new InvalidOperationException(insertBookResult.FirstError.Description);
                    }
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
        Result<MediaLibraryScanJobProgress> scanJobProgressResult = MediaLibraryScanJobProgress.Create(currentProgress, totalProgress, "SavingScanData");
        if (scanJobProgressResult.IsFailure)
            return scanJobProgressResult.Errors;

        await domainEventPublisher.PublishAsync(new LibraryScanJobProgressChangedDomainEvent(Guid.NewGuid(), LibraryId, compositeKey, scanJobProgressResult.Value, DateTime.UtcNow), cancellationToken).ConfigureAwait(false);

        return Result.Success;
    }

    /// <summary>
    /// Creates a shell book entity for the file stored at <paramref name="path"/> in the media library identified by <paramref name="libraryId"/>,
    /// with a title derived from the file name, and no web metadata yet.
    /// </summary>
    /// <param name="libraryId">The Id of the media library the book belongs to.</param>
    /// <param name="path">The file system path of the book.</param>
    /// <returns>The created shell book entity.</returns>
    private static BookEntity CreateShellBookEntity(Guid libraryId, string path)
    {
        return new BookEntity
        {
            Id = Guid.NewGuid(),
            LibraryId = libraryId,
            Path = path,
            Title = GetTitleFromPath(path),
            MetadataStatus = MetadataStatus.Pending,
            CreatedOnUtc = DateTime.UtcNow,
            CreatedBy = Guid.Empty,
            UpdatedBy = null
        };
    }

    /// <summary>
    /// Derives a book title from the file name of the provided <paramref name="path"/>, by removing the extension and replacing separators with spaces.
    /// </summary>
    /// <param name="path">The file system path of the book.</param>
    /// <returns>The derived title.</returns>
    private static string GetTitleFromPath(string path)
    {
        string fileName = Path.GetFileNameWithoutExtension(path);
        string title = Regex.Replace(fileName, @"[_\-\.]+", " ").Trim();
        return title.Length > 0 ? title : fileName;
    }
}
