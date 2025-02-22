#region ========================================================================= USING =====================================================================================
using ErrorOr;
using Lumina.Application.Common.DataAccess.Entities.MediaLibrary.Management;
using Lumina.Application.Common.DataAccess.Repositories.MediaLibrary;
using Lumina.Application.Common.DataAccess.UoW;
using Lumina.Application.Common.Mapping.MediaLibrary.Management;
using Lumina.Domain.Common.Enums.FileSystem;
using Lumina.Domain.Common.Enums.MediaLibrary;
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.Core.BoundedContexts.FileSystemManagementBoundedContext.FileSystemManagementAggregate.Entities;
using Lumina.Domain.Core.BoundedContexts.FileSystemManagementBoundedContext.FileSystemManagementAggregate.Services;
using Lumina.Domain.Core.BoundedContexts.FileSystemManagementBoundedContext.FileSystemManagementAggregate.ValueObjects;
using Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryAggregate;
using Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryScanAggregate.Events;
using Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryScanAggregate.Services.Jobs;
using Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryScanAggregate.ValueObjects;
using Lumina.Infrastructure.Core.MediaLibrary.Management.Scanning.Jobs.Payloads;
using Mediator;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Infrastructure.Core.MediaLibrary.Management.Scanning.Jobs.Common;

/// <summary>
/// Media library scan job for discovering file system items.
/// </summary>
internal sealed class FileSystemDiscoveryJob : MediaLibraryScanJob, IFileSystemDiscoveryJob
{
    private readonly IDirectoryService _directoryService;
    private readonly IFileService _fileService;
    private readonly IServiceScopeFactory _serviceScopeFactory;

    /// <summary>
    /// Initializes a new instance of the <see cref="FileSystemDiscoveryJob"/> class.
    /// </summary>
    /// <param name="directoryService">Injected service for handling directories.</param>
    /// <param name="fileService">Injected service for handling files.</param>
    /// <param name="serviceScopeFactory">
    /// Injected factory for creating scopes in which services are requested. 
    /// See docs/technical/achitecture/architecture-knowledge-management/architecture-decision-log/architecture-decission-record-0001.md for details.
    /// </param>
    public FileSystemDiscoveryJob(IDirectoryService directoryService, IFileService fileService, IServiceScopeFactory serviceScopeFactory)
    {
        _directoryService = directoryService;
        _fileService = fileService;
        _serviceScopeFactory = serviceScopeFactory;
    }

    /// <inheritdoc/>
    public override async Task ExecuteAsync<TInput>(Guid id, TInput input, CancellationToken cancellationToken)
    {
        try
        {
            parentsPayloadsExecuted++; // increment the number of parents that finished their execution and called this job
            // only execute this job's payload when it has no parents, or when all the parents finished their execution
            if (Parents.Count == 0 || parentsPayloadsExecuted == Parents.Count)
            {
                // this needs to be wrapped in a task because even though this job is processed in a "fire and forget" async manner, it still does synchronous
                // file system processing that takes time, and would block the processing of scan jobs in the in-memory queue 
                await Task.Run(async () =>
                {
                    Status = LibraryScanJobStatus.Running;
                    Console.WriteLine("started file discovering");
                    // see docs/technical/achitecture/architecture-knowledge-management/architecture-decision-log/architecture-decission-record-0001.md for details:
                    await using AsyncServiceScope asyncServiceScope = _serviceScopeFactory.CreateAsyncScope();
                    IUnitOfWork? unitOfWork = asyncServiceScope.ServiceProvider.GetService<IUnitOfWork>();
                    IPublisher? publisher = asyncServiceScope.ServiceProvider.GetService<IPublisher>();
                    ILibraryRepository libraryRepository = unitOfWork!.GetRepository<ILibraryRepository>();
                    MediaLibraryScanCompositeId compositeKey = MediaLibraryScanCompositeId.Create(ScanId, UserId);

                    // get the library from the repository
                    ErrorOr<LibraryEntity?> getLibraryResult = await libraryRepository.GetByIdAsync(LibraryId.Value, cancellationToken).ConfigureAwait(false);
                    if (getLibraryResult.IsError || getLibraryResult.Value is null)
                    {
                        await publisher!.Publish(new LibraryScanFailedDomainEvent(Guid.NewGuid(), LibraryId, compositeKey, DateTime.UtcNow), cancellationToken).ConfigureAwait(false);
                        return;
                    }

                    // convert it to a domain object
                    ErrorOr<Library> domainLibraryResult = getLibraryResult.Value.ToDomainEntity();
                    if (domainLibraryResult.IsError)
                    {
                        await publisher!.Publish(new LibraryScanFailedDomainEvent(Guid.NewGuid(), LibraryId, compositeKey, DateTime.UtcNow), cancellationToken).ConfigureAwait(false);
                        return;
                    }

                    // set the initial progress of the scan job
                    ErrorOr<MediaLibraryScanJobProgress> scanJobProgressResult = MediaLibraryScanJobProgress.Create(0, domainLibraryResult.Value.ContentLocations.Count, "DiscoveringFiles");
                    if (scanJobProgressResult.IsError)
                    {
                        await publisher!.Publish(new LibraryScanFailedDomainEvent(Guid.NewGuid(), LibraryId, compositeKey, DateTime.UtcNow), cancellationToken).ConfigureAwait(false);
                        return;
                    }

                    await publisher!.Publish(new LibraryScanJobProgressChangedDomainEvent(
                        Guid.NewGuid(), LibraryId, compositeKey, scanJobProgressResult.Value, DateTime.UtcNow), cancellationToken).ConfigureAwait(false);

                    // recursively get the file system tree for each of the media library content locations
                    List<FileSystemTreeNode> contentLocationTrees = [];
                    foreach (FileSystemPathId contentLocation in domainLibraryResult.Value.ContentLocations)
                    {
                        await Task.Delay(1000);
                        cancellationToken.ThrowIfCancellationRequested();
                        ErrorOr<FileSystemTreeNode> treeResult = BuildTree(contentLocation, includeHiddenElements: true, cancellationToken);
                        if (treeResult.IsError)
                        {
                            // handle errors, maybe:
                            // Status = LibraryScanJobStatus.Failed;
                        }
                        else
                            contentLocationTrees.Add(treeResult.Value);

                        // increment the number of processed elements progress
                        scanJobProgressResult = MediaLibraryScanJobProgress.Create(
                            scanJobProgressResult.Value.CompletedItems + 1, domainLibraryResult.Value.ContentLocations.Count, "DiscoveringFiles");
                        if (scanJobProgressResult.IsError)
                        {
                            await publisher!.Publish(new LibraryScanFailedDomainEvent(
                                Guid.NewGuid(), LibraryId, compositeKey, DateTime.UtcNow), cancellationToken).ConfigureAwait(false);
                            return;
                        }
                        await publisher!.Publish(new LibraryScanJobProgressChangedDomainEvent(
                            Guid.NewGuid(), LibraryId, compositeKey, scanJobProgressResult.Value, DateTime.UtcNow), cancellationToken).ConfigureAwait(false);
                    }
                    // this job finished, increment the number of processed jobs progress
                    await publisher!.Publish(new LibraryScanProgressChangedDomainEvent(
                        Guid.NewGuid(), LibraryId, compositeKey, DateTime.UtcNow), cancellationToken).ConfigureAwait(false);
                    Console.WriteLine("ended file discovering");
                    Status = LibraryScanJobStatus.Completed;

                    // call each linked child with the obtained payload
                    foreach (IMediaLibraryScanJob children in Children)
                        await children.ExecuteAsync(id, contentLocationTrees, cancellationToken).ConfigureAwait(false);
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
    /// Builds a hierarchical tree structure representing the file system starting from the specified path.
    /// </summary>
    /// <param name="path">The identifier of the root path from which to build the tree.</param>
    /// <param name="includeHiddenElements">Specifies whether hidden files and directories should be included in the tree.</param>
    /// <returns>An <see cref="ErrorOr{TValue}"/> containing either a <see cref="FileSystemTreeNode"/>, or an error.</returns>
    private ErrorOr<FileSystemTreeNode> BuildTree(FileSystemPathId path, bool includeHiddenElements = false, CancellationToken cancellationToken = default)
    {
        // validate the path and get the root directory node
        ErrorOr<Directory> rootDirectoryResult = Directory.Create(path, path.Path, Optional<DateTime>.None(), Optional<DateTime>.None());
        if (rootDirectoryResult.IsError)
            return rootDirectoryResult.Errors;

        // build the tree starting from the root directory
        return BuildNodeRecursive(rootDirectoryResult.Value, includeHiddenElements, cancellationToken);
    }

    /// <summary>
    /// Recursively builds a tree node for a given directory, including its subdirectories and files.
    /// </summary>
    /// <param name="directory">The directory for which to build a tree node.</param>
    /// <param name="includeHiddenElements">Specifies whether hidden files and directories should be included in the node's children.</param>
    /// <returns>An <see cref="ErrorOr{TValue}"/> containing either a <see cref="FileSystemTreeNode"/>, or an error.</returns>
    private ErrorOr<FileSystemTreeNode> BuildNodeRecursive(Directory directory, bool includeHiddenElements, CancellationToken cancellationToken)
    {
        FileSystemTreeNode node = new()
        {
            Path = directory.Id.Path,
            Name = directory.Name,
            ItemType = FileSystemItemType.Directory,
            Children = []
        };

        // fetch and add subdirectories
        ErrorOr<IEnumerable<Directory>> subdirectoriesResult = _directoryService.GetSubdirectories(directory, includeHiddenElements);
        if (subdirectoriesResult.IsError)
            return subdirectoriesResult.Errors;

        foreach (Directory subdirectory in subdirectoriesResult.Value)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ErrorOr<FileSystemTreeNode> subNodeResult = BuildNodeRecursive(subdirectory, includeHiddenElements, cancellationToken);
            if (subNodeResult.IsError)
                return subNodeResult.Errors;

            node.Children.Add(subNodeResult.Value);
        }

        // fetch and add files
        ErrorOr<IEnumerable<File>> filesResult = _fileService.GetFiles(directory.Id, includeHiddenElements);
        if (filesResult.IsError)
            return filesResult.Errors;

        foreach (File file in filesResult.Value)
        {
            cancellationToken.ThrowIfCancellationRequested();
            node.Children.Add(new FileSystemTreeNode
            {
                Path = file.Id.Path,
                Name = file.Name,
                ItemType = FileSystemItemType.File,
                Children = [] // files have no children
            });
        }
        return node;
    }
}
