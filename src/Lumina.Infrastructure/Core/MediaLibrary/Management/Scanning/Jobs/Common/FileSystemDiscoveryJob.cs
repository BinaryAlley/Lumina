#region ========================================================================= USING =====================================================================================
using ErrorOr;
using Lumina.Application.Common.DataAccess.Entities.MediaLibrary.Management;
using Lumina.Application.Common.DataAccess.Repositories.MediaLibrary;
using Lumina.Application.Common.DataAccess.UoW;
using Lumina.Application.Common.Mapping.MediaLibrary.Management;
using Lumina.Domain.Common.Enums.MediaLibrary;
using Lumina.Domain.Core.BoundedContexts.FileSystemManagementBoundedContext.FileSystemManagementAggregate.ValueObjects;
using Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryAggregate;
using Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryScanAggregate.Events;
using Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryScanAggregate.Services.Jobs;
using Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryScanAggregate.ValueObjects;
using Lumina.Infrastructure.Common.Errors;
using Mediator;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Infrastructure.Core.MediaLibrary.Management.Scanning.Jobs.Common;

/// <summary>
/// Media library scan job for discovering file system items.
/// </summary>
internal sealed class FileSystemDiscoveryJob : MediaLibraryScanJob, IFileSystemDiscoveryJob
{
    private readonly IServiceScopeFactory _serviceScopeFactory;
    
    /// <summary>
    /// Initializes a new instance of the <see cref="FileSystemDiscoveryJob"/> class.
    /// </summary>
    /// <param name="serviceScopeFactory">
    /// Injected factory for creating scopes in which services are requested. 
    /// See docs/technical/achitecture/architecture-knowledge-management/architecture-decision-log/architecture-decission-record-0001.md for details.
    /// </param>
    public FileSystemDiscoveryJob(IServiceScopeFactory serviceScopeFactory)
    {
        _serviceScopeFactory = serviceScopeFactory;
    }

    /// <inheritdoc/>
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
                // file system processing that takes time, and would block the processing of scan jobs in the in-memory queue 
                await Task.Run(async () =>
                {
                    Status = LibraryScanJobStatus.Running;
                    Stopwatch sw = Stopwatch.StartNew();
                    Console.WriteLine("Started file system discovery scan job...");
                    // see docs/technical/achitecture/architecture-knowledge-management/architecture-decision-log/architecture-decission-record-0001.md for details:
                    await using AsyncServiceScope asyncServiceScope = _serviceScopeFactory.CreateAsyncScope();
                    IUnitOfWork unitOfWork = asyncServiceScope.ServiceProvider.GetService<IUnitOfWork>()!;
                    IPublisher publisher = asyncServiceScope.ServiceProvider.GetService<IPublisher>()!;
                    ILibraryRepository libraryRepository = unitOfWork.GetRepository<ILibraryRepository>()!;
                    MediaLibraryScanCompositeId compositeKey = MediaLibraryScanCompositeId.Create(ScanId, UserId);

                    // get the library from the repository
                    ErrorOr<LibraryEntity?> getLibraryResult = await libraryRepository.GetByIdAsync(LibraryId.Value, cancellationToken).ConfigureAwait(false);
                    if (getLibraryResult.IsError || getLibraryResult.Value is null)
                    {
                        await publisher.Publish(new LibraryScanFailedDomainEvent(Guid.NewGuid(), LibraryId, compositeKey, DateTime.UtcNow), cancellationToken).ConfigureAwait(false);
                        return;
                    }

                    // convert it to a domain object
                    ErrorOr<Library> domainLibraryResult = getLibraryResult.Value.ToDomainEntity();
                    if (domainLibraryResult.IsError)
                    {
                        await publisher.Publish(new LibraryScanFailedDomainEvent(Guid.NewGuid(), LibraryId, compositeKey, DateTime.UtcNow), cancellationToken).ConfigureAwait(false);
                        return;
                    }

                    // set the initial progress of the scan job
                    ErrorOr<Success> publishJobProgressResult = await PublishJobProgress(publisher, compositeKey, 0, domainLibraryResult.Value.ContentLocations.Count, cancellationToken).ConfigureAwait(false);
                    if (publishJobProgressResult.IsError) 
                        return;

                    TimeSpan heartbeatInterval = TimeSpan.FromSeconds(1);
                    DateTime lastHeartbeat = DateTime.UtcNow;
                    long processedFiles = 0L;
                    int processedContentLocations = 0;

                    // get the files for each of the media library content locations
                    List<FileInfo> discoveredFiles = [];
                    foreach (FileSystemPathId contentLocation in domainLibraryResult.Value.ContentLocations)
                    {
                        cancellationToken.ThrowIfCancellationRequested();

                        foreach (FileInfo file in GetFiles(contentLocation.Path, true, cancellationToken))
                        {
                            discoveredFiles.Add(file);
                            processedFiles++;

                            // increment the number of processed elements progress in a high-frequency counter with low overhead check
                            if (processedFiles % 1000 == 0)
                            {
                                DateTime now = DateTime.UtcNow;
                                if (now - lastHeartbeat >= heartbeatInterval)
                                {
                                    publishJobProgressResult = await PublishJobProgress(
                                        publisher, compositeKey, processedContentLocations, domainLibraryResult.Value.ContentLocations.Count, cancellationToken).ConfigureAwait(false);
                                    if (publishJobProgressResult.IsError)
                                        return;
                                    lastHeartbeat = now;
                                }
                            }
                        }
                        processedContentLocations++;
                    }
                    // this job finished, increment the number of processed jobs progress
                    await publisher.Publish(new LibraryScanProgressChangedDomainEvent(
                        Guid.NewGuid(), LibraryId, compositeKey, DateTime.UtcNow), cancellationToken).ConfigureAwait(false);
                    sw.Stop();
                    Console.WriteLine($"Ended file system discovery scan job in {sw.ElapsedMilliseconds}ms, discovered {discoveredFiles.Count} files.");
                    
                    Status = LibraryScanJobStatus.Completed;

                    // call each linked child with the obtained payload
                    foreach (IMediaLibraryScanJob child in Children)
                        await child.ExecuteAsync(id, discoveredFiles, cancellationToken).ConfigureAwait(false);
                }, cancellationToken).ConfigureAwait(false);
            }
        }
        catch (OperationCanceledException)
        {
            Status = LibraryScanJobStatus.Canceled;
            throw;
        }
    }

    /// <summary>
    /// Gets all files under the specified directory.
    /// </summary>
    /// <param name="rootDirectoryPath">The path of the root directory from which to start collecting files.</param>
    /// <param name="includeHiddenElements">Specifies whether hidden files and directories should be included.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <remarks>
    /// See docs/technical/achitecture/architecture-knowledge-management/architecture-decision-log/architecture-decission-record-0003.md for details.
    /// </remarks>
    /// <returns>The discovered collection of files.</returns>
    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
    private static IEnumerable<FileInfo> GetFiles(string rootDirectoryPath, bool includeHiddenElements, CancellationToken cancellationToken)
    {
        Queue<DirectoryInfo> directoryStack = new();
        directoryStack.Enqueue(new DirectoryInfo(rootDirectoryPath));

        EnumerationOptions enumerationOptions = new()
        {
            AttributesToSkip = includeHiddenElements ? FileAttributes.None : FileAttributes.Hidden,
            IgnoreInaccessible = true,
            RecurseSubdirectories = false
        };
        // use a breadth-first traversal iterative approach, instead of recursion, which is heavier on call frames and memory
        // and could result in stack overflow on deeply nested directories
        while (directoryStack.Count > 0)
        {
            cancellationToken.ThrowIfCancellationRequested();
            DirectoryInfo currentDirectory = directoryStack.Dequeue();

            // process files first
            IEnumerable<FileInfo>? files = null;
            try
            {
                files = currentDirectory.EnumerateFiles("*", enumerationOptions);
            }
            catch (IOException ex)
            {
                Console.WriteLine(ex.Message);
            }

            if (files is not null)
            {
                foreach (FileInfo file in files)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    yield return file;
                }
            }

            // process subdirectories - each new subdirectory is added to the same stack queue, for later processing
            IEnumerable<DirectoryInfo>? subdirs = null;
            try
            {
                subdirs = currentDirectory.EnumerateDirectories("*", enumerationOptions);
            }
            catch (IOException ex)
            {
                Console.WriteLine(ex.Message);
            }

            if (subdirs is not null)
            {
                foreach (DirectoryInfo subdir in subdirs)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    directoryStack.Enqueue(subdir);
                }
            }
        }
    }

    /// <summary>
    /// Publishes a job progress update.
    /// </summary>
    /// <param name="publisher">The service used to publish the progress update.</param>
    /// <param name="compositeKey">The composite unique identifier of a media library scan.</param>
    /// <param name="currentProgress">The current job progress.</param>
    /// <param name="totalProgress">The total job progress.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="ErrorOr{TValue}"/> representing either a successful operation, or an error.</returns>
    private async Task<ErrorOr<Success>> PublishJobProgress(IPublisher publisher, MediaLibraryScanCompositeId compositeKey, int currentProgress, int totalProgress, CancellationToken cancellationToken)
    {
        ErrorOr<MediaLibraryScanJobProgress> scanJobProgressResult = MediaLibraryScanJobProgress.Create(currentProgress, totalProgress, "DiscoveringFiles");
        if (scanJobProgressResult.IsError)
        {
            await publisher.Publish(new LibraryScanFailedDomainEvent(Guid.NewGuid(), LibraryId, compositeKey, DateTime.UtcNow), cancellationToken).ConfigureAwait(false);
            return Errors.LibraryScanning.FailedToCreateScanJobProgress;
        }

        await publisher.Publish(new LibraryScanJobProgressChangedDomainEvent(
            Guid.NewGuid(), LibraryId, compositeKey, scanJobProgressResult.Value, DateTime.UtcNow), cancellationToken).ConfigureAwait(false);

        return Result.Success;
    }
}
