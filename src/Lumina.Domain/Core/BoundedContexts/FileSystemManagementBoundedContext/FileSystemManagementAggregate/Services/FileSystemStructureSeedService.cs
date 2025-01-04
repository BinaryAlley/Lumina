#region ========================================================================= USING =====================================================================================
using ErrorOr;
using Lumina.Domain.Common.Errors;
using Lumina.Domain.Core.BoundedContexts.FileSystemManagementBoundedContext.FileSystemManagementAggregate.Strategies.Environment;
using Lumina.Domain.Core.BoundedContexts.FileSystemManagementBoundedContext.FileSystemManagementAggregate.ValueObjects;
#endregion

namespace Lumina.Domain.Core.BoundedContexts.FileSystemManagementBoundedContext.FileSystemManagementAggregate.Services;

/// <summary>
/// Service for seeding initial file system structure.
/// </summary>
internal class FileSystemStructureSeedService : IFileSystemStructureSeedService
{
    private readonly IEnvironmentContext _environmentContext;
    private readonly IPathService _pathService;

    /// <summary>
    /// Initializes a new instance of the <see cref="FileSystemStructureSeedService"/> class.
    /// </summary>
    /// <param name="environmentContext">Injected facade service for environment contextual services.</param>
    /// <param name="pathService">Injected service for handling file system paths.</param>
    public FileSystemStructureSeedService(IEnvironmentContext environmentContext, IPathService pathService)
    {
        _environmentContext = environmentContext;
        _pathService = pathService;
    }

    /// <summary>
    /// Sets up the default file system directories needed by the application.
    /// </summary>
    /// <param name="rootPath">The base path where the directories will be created relative to.</param>
    /// <returns>An <see cref="ErrorOr{TValue}"/> representing either a successfull operation, or an error.</returns>
    public ErrorOr<Created> SetDefaultDirectories(string rootPath)
    {
        // make sure the root path exists
        ErrorOr<FileSystemPathId> rootPathIdResult = FileSystemPathId.Create(rootPath);
        if (rootPathIdResult.IsError)
            return rootPathIdResult.Errors;

        // create the libraries directory
        ErrorOr<string> librariesPathResult = _pathService.CombinePath(rootPath, "libraries"); 
        if (librariesPathResult.IsError)
            return librariesPathResult.Errors;

        ErrorOr<FileSystemPathId> libraryPathIdResult = FileSystemPathId.Create(librariesPathResult.Value);
        if (libraryPathIdResult.IsError)
            return libraryPathIdResult.Errors;

        ErrorOr<bool> librariesDirectoryExistsResult = _environmentContext.DirectoryProviderService.DirectoryExists(libraryPathIdResult.Value);
        if (librariesDirectoryExistsResult.IsError)
            return librariesDirectoryExistsResult.Errors;

        // only create it if it doesn't already exist
        if (!librariesDirectoryExistsResult.Value)
        {
            ErrorOr<FileSystemPathId> createDirectoryResult = _environmentContext.DirectoryProviderService.CreateDirectory(rootPathIdResult.Value, "libraries");
            if (createDirectoryResult.IsError)
                return createDirectoryResult.Errors;
        }
        return Result.Created;
    }
}
