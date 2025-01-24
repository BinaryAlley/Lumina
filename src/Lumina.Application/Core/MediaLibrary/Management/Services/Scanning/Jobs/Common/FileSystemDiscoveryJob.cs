#region ========================================================================= USING =====================================================================================
using ErrorOr;
using Lumina.Application.Core.MediaLibrary.Management.Services.Scanning.Jobs.Payloads;
using Lumina.Domain.Common.Enums.FileSystem;
using Lumina.Domain.Common.Enums.MediaLibrary;
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.Core.BoundedContexts.FileSystemManagementBoundedContext.FileSystemManagementAggregate.Entities;
using Lumina.Domain.Core.BoundedContexts.FileSystemManagementBoundedContext.FileSystemManagementAggregate.Services;
using Lumina.Domain.Core.BoundedContexts.FileSystemManagementBoundedContext.FileSystemManagementAggregate.ValueObjects;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Application.Core.MediaLibrary.Management.Services.Scanning.Jobs.Common;

/// <summary>
/// Media library scan job for discovering file system items.
/// </summary>
internal class FileSystemDiscoveryJob : MediaScanJob
{
    private readonly IDirectoryService _directoryService;
    private readonly IFileService _fileService;

    /// <summary>
    /// Initializes a new instance of the <see cref="FileSystemDiscoveryJob"/> class.
    /// </summary>
    /// <param name="directoryService">Injected service for handling directories.</param>
    /// <param name="fileService">Injected service for handling files.</param>
    public FileSystemDiscoveryJob(IDirectoryService directoryService, IFileService fileService)
    {
        _directoryService = directoryService;
        _fileService = fileService;
    }

    /// <inheritdoc/>
    public override async Task ExecuteAsync<TInput>(Guid id, TInput input, CancellationToken cancellationToken)
    {
        parentsPayloadsExecuted++; // increment the number of parents that finished their execution and called this job
        // only execute this job's payload when it has no parents, or when all the parents finished their execution
        if (Parents.Count == 0 || parentsPayloadsExecuted == Parents.Count)
        {
            Status = LibraryScanJobStatus.Running;
            // recursively get the file system tree for each of the media library content locations
            List<FileSystemTreeNode> contentLocationTrees = [];
            foreach (FileSystemPathId contentLocation in Library.ContentLocations)
            {
                ErrorOr<FileSystemTreeNode> treeResult = BuildTree(contentLocation, includeHiddenElements: true);
                if (treeResult.IsError)
                {
                    // handle errors, maybe:
                    // Status = LibraryScanJobStatus.Failed;
                }
                else
                    contentLocationTrees.Add(treeResult.Value);
            }
            Status = LibraryScanJobStatus.Completed;
            // call each linked child with the obtained payload
            foreach (MediaScanJob children in Children)
                await children.ExecuteAsync(id, contentLocationTrees, cancellationToken).ConfigureAwait(false);
        }
    }

    /// <summary>
    /// Builds a hierarchical tree structure representing the file system starting from the specified path.
    /// </summary>
    /// <param name="path">The identifier of the root path from which to build the tree.</param>
    /// <param name="includeHiddenElements">Specifies whether hidden files and directories should be included in the tree.</param>
    /// <returns>An <see cref="ErrorOr{TValue}"/> containing either a <see cref="FileSystemTreeNode"/>, or an error.</returns>
    public ErrorOr<FileSystemTreeNode> BuildTree(FileSystemPathId path, bool includeHiddenElements = false)
    {
        // validate the path and get the root directory node
        ErrorOr<Directory> rootDirectoryResult = Directory.Create(path, path.Path, Optional<DateTime>.None(), Optional<DateTime>.None());
        if (rootDirectoryResult.IsError)
            return rootDirectoryResult.Errors;

        // build the tree starting from the root directory
        return BuildNodeRecursive(rootDirectoryResult.Value, includeHiddenElements);
    }

    /// <summary>
    /// Recursively builds a tree node for a given directory, including its subdirectories and files.
    /// </summary>
    /// <param name="directory">The directory for which to build a tree node.</param>
    /// <param name="includeHiddenElements">Specifies whether hidden files and directories should be included in the node's children.</param>
    /// <returns>An <see cref="ErrorOr{TValue}"/> containing either a <see cref="FileSystemTreeNode"/>, or an error.</returns>
    private ErrorOr<FileSystemTreeNode> BuildNodeRecursive(Directory directory, bool includeHiddenElements)
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
            ErrorOr<FileSystemTreeNode> subNodeResult = BuildNodeRecursive(subdirectory, includeHiddenElements);
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
