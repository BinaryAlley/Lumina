#region ========================================================================= USING =====================================================================================
using ErrorOr;
using Lumina.Application.Common.Mapping.FileSystemManagement.FileSystem;
using Lumina.Contracts.Enums.FileSystem;
using Lumina.Contracts.Responses.FileSystemManagement.Common;
using Lumina.Domain.Core.BoundedContexts.FileSystemManagementBoundedContext.FileSystemManagementAggregate;
using Lumina.Domain.Core.BoundedContexts.FileSystemManagementBoundedContext.FileSystemManagementAggregate.Entities;
using Lumina.Domain.Core.BoundedContexts.FileSystemManagementBoundedContext.FileSystemManagementAggregate.Services;
using Lumina.Domain.Core.BoundedContexts.FileSystemManagementBoundedContext.FileSystemManagementAggregate.ValueObjects;
using Mediator;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Application.Core.FileSystemManagement.Directories.Queries.GetDirectoryTree;

/// <summary>
/// Handler for the query to get a directory tree.
/// </summary>
public class GetDirectoryTreeQueryHandler : IRequestHandler<GetDirectoryTreeQuery, ErrorOr<IEnumerable<FileSystemTreeNodeResponse>>>
{
    private readonly IDirectoryService _directoryService;
    private readonly IFileService _fileService;
    private readonly IPathService _pathService;
    private readonly IDriveService _driveService;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetDirectoryTreeQueryHandler"/> class.
    /// </summary>
    /// <param name="driveService">Injected service for handling file system drives.</param>
    /// <param name="directoryService">Injected service for handling directories.</param>
    /// <param name="fileService">Injected service for handling files.</param>
    /// <param name="pathService">Injected service for handling file system paths.</param>
    public GetDirectoryTreeQueryHandler(IDriveService driveService, IDirectoryService directoryService, IFileService fileService, IPathService pathService)
    {
        _directoryService = directoryService;
        _fileService = fileService;
        _pathService = pathService;
        _driveService = driveService;
    }

    /// <summary>
    /// Gets the tree of expanded directories leading up to the requested path, with the additional list of drives, and children of the last child directory.
    /// </summary>
    /// <returns>
    /// An <see cref="ErrorOr{TValue}"/> containing either a collection of <see cref="FileSystemTreeNodeResponse"/>, or an error message.
    /// </returns>
    public async ValueTask<ErrorOr<IEnumerable<FileSystemTreeNodeResponse>>> Handle(GetDirectoryTreeQuery request, CancellationToken cancellationToken)
    {
        List<FileSystemTreeNodeResponse> result = [];
        // retrieve the list of drives
        ErrorOr<IEnumerable<FileSystemTreeNodeResponse>> getDrivesResult = GetDrives();
        if (getDrivesResult.IsError)
            return getDrivesResult.Errors;
        result.AddRange(getDrivesResult.Value);

        ErrorOr<IEnumerable<PathSegment>> pathPartsResults = _pathService.ParsePath(request.Path!);
        if (pathPartsResults.IsError)
            return pathPartsResults.Errors;

        List<PathSegment> pathParts = pathPartsResults.Value.ToList();

        ErrorOr<IEnumerable<FileSystemTreeNodeResponse>> buildDirectoryTreeResult = BuildDirectoryTree(result, pathParts, out FileSystemTreeNodeResponse? currentNode, request.IncludeHiddenElements);
        if (buildDirectoryTreeResult.IsError)
            return buildDirectoryTreeResult.Errors;

        // add subdirectories of the last directory node
        ErrorOr<Success> loadChildrenResult = LoadChildren(currentNode, request.IncludeFiles, request.IncludeHiddenElements);
        if (loadChildrenResult.IsError)
            return loadChildrenResult.Errors;
        return await ValueTask.FromResult(result);
    }

    /// <summary>
    /// Gets the root drives (on Windows), or an empty list on Unix-based systems.
    /// </summary>
    /// <returns>
    /// An <see cref="ErrorOr{TValue}"/> containing a list of <see cref="FileSystemTreeNodeResponse"/>, or an error message.
    /// </returns>
    private ErrorOr<IEnumerable<FileSystemTreeNodeResponse>> GetDrives()
    {
        ErrorOr<IEnumerable<FileSystemItem>> driveResult = _driveService.GetDrives();
        if (driveResult.IsError)
            return driveResult.Errors;
        return ErrorOrFactory.From(driveResult.Value.ToTreeNodeResponses());
    }

    /// <summary>
    /// Builds the tree of directories based on the parsed path parts.
    /// </summary>
    /// <param name="drives">The list of available drives.</param>
    /// <param name="pathParts">The list of parsed path segments (directories).</param>
    /// <param name="includeHiddenElements">Whether to include hidden file system elements or not.</param>
    /// <returns>
    /// An <see cref="ErrorOr{TValue}"/> containing a list of <see cref="FileSystemTreeNodeResponse"/> representing the directory tree, or an error message.
    /// </returns>
    private ErrorOr<IEnumerable<FileSystemTreeNodeResponse>> BuildDirectoryTree(List<FileSystemTreeNodeResponse> drives, List<PathSegment> pathParts, out FileSystemTreeNodeResponse currentNode, bool includeHiddenElements = true)
    {
        // find the root drive that matches the first part of the path
        FileSystemTreeNodeResponse drive = drives.First(drive => drive.Name.Contains(pathParts[0].Name));
        drive.IsExpanded = true;
        currentNode = drive;
        string currentPath = pathParts[0].Name;

        // traverse the remaining path parts to build the tree structure
        for (int i = 1; i < pathParts.Count; i++)
        {
            ErrorOr<string> combinedPathResult = _pathService.CombinePath(currentPath, pathParts[i].Name);
            if (combinedPathResult.IsError)
                return combinedPathResult.Errors;
            if (_pathService.Exists(combinedPathResult.Value, includeHiddenElements))
            {

                currentPath = combinedPathResult.Value;
                // create a new node for the directory and add it as a child of the current node
                FileSystemTreeNodeResponse newNode = new()
                {
                    Path = currentPath,
                    Name = pathParts[i].Name,
                    ItemType = FileSystemItemType.Directory,
                    IsExpanded = true
                };
                currentNode.Children.Add(newNode);
                currentNode = newNode; // move to the new node
            }
        }
        return new List<FileSystemTreeNodeResponse> { drive };
    }

    /// <summary>
    /// Loads the child directories and files of the specified directory node.
    /// </summary>
    /// <param name="node">The directory node to load children for.</param>
    /// <param name="includeFiles">Whether to include files in the directories or not.</param>
    /// <returns>A result indicating a successful operation, or an error message.</returns>
    private ErrorOr<Success> LoadChildren(FileSystemTreeNodeResponse node, bool includeFiles, bool includeHiddenElements)
    {
        // get subdirectories under the current node's path
        ErrorOr<IEnumerable<Directory>> subDirectoriesResult = _directoryService.GetSubdirectories(node.Path, includeHiddenElements);
        if (subDirectoriesResult.IsError)
            return subDirectoriesResult.Errors;
        // add each subdirectory as a child node of the current directory
        foreach (Directory subDirectory in subDirectoriesResult.Value ?? [])
        {
            FileSystemTreeNodeResponse subDirNode = new()
            {
                Name = subDirectory.Name,
                Path = subDirectory.Id.Path,
                ItemType = FileSystemItemType.Directory,
                IsExpanded = false,
                ChildrenLoaded = false
            };
            node.Children.Add(subDirNode);
        }
        if (includeFiles)
        {
            // get files under the current node's path
            ErrorOr<IEnumerable<File>> filesResult = _fileService.GetFiles(node.Path, includeHiddenElements);
            if (filesResult.IsError)
                return filesResult.Errors;
            // add each file as a child node of the current directory
            foreach (File file in filesResult.Value)
            {
                FileSystemTreeNodeResponse fileNode = new()
                {
                    Name = file.Name,
                    Path = file.Id.Path,
                    ItemType = FileSystemItemType.File,
                    IsExpanded = false,
                    ChildrenLoaded = true
                };
                node.Children.Add(fileNode);
            }
        }
        node.ChildrenLoaded = true;
        return Result.Success;
    }
}
