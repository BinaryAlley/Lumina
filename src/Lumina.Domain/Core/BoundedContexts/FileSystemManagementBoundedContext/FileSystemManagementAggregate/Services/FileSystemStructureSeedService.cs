#region ========================================================================= USING =====================================================================================
using Lumina.Domain.Common.Primitives;
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
    /// <returns>An <see cref="Result{TValue}"/> representing either a successful operation, or an error.</returns>
    public Result<Created> SetDefaultDirectories(string rootPath)
    {
        // make sure the root path exists
        Result<FileSystemPathId> rootPathIdResult = FileSystemPathId.Create(rootPath);
        if (rootPathIdResult.IsFailure)
            return rootPathIdResult.Errors;

        Result<Created> createLibrariesDirectoryResult = EnsureDirectory(rootPathIdResult.Value, "libraries");
        if (createLibrariesDirectoryResult.IsFailure)
            return createLibrariesDirectoryResult.Errors;

        Result<Created> createBooksDirectoryResult = EnsureDirectory(rootPathIdResult.Value, "books");
        if (createBooksDirectoryResult.IsFailure)
            return createBooksDirectoryResult.Errors;

        return Result.Created;
    }

    /// <summary>
    /// Creates the directory with the provided <paramref name="directoryName"/> under the <paramref name="rootPathId"/>, when it does not already exist.
    /// </summary>
    /// <param name="rootPathId">The id of the root path under which the directory is created.</param>
    /// <param name="directoryName">The name of the directory to create.</param>
    /// <returns>An <see cref="Result{TValue}"/> representing either a successful operation, or an error.</returns>
    private Result<Created> EnsureDirectory(FileSystemPathId rootPathId, string directoryName)
    {
        Result<string> directoryPathResult = _pathService.CombinePath(rootPathId.Path, directoryName);
        if (directoryPathResult.IsFailure)
            return directoryPathResult.Errors;

        Result<FileSystemPathId> directoryPathIdResult = FileSystemPathId.Create(directoryPathResult.Value);
        if (directoryPathIdResult.IsFailure)
            return directoryPathIdResult.Errors;

        Result<bool> directoryExistsResult = _environmentContext.DirectoryProviderService.DirectoryExists(directoryPathIdResult.Value);
        if (directoryExistsResult.IsFailure)
            return directoryExistsResult.Errors;

        // only create it if it doesn't already exist
        if (!directoryExistsResult.Value)
        {
            Result<FileSystemPathId> createDirectoryResult = _environmentContext.DirectoryProviderService.CreateDirectory(rootPathId, directoryName);
            if (createDirectoryResult.IsFailure)
                return createDirectoryResult.Errors;
        }
        return Result.Created;
    }
}
