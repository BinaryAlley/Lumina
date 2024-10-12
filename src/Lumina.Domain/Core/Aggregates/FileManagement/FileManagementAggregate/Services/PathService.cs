#region ========================================================================= USING =====================================================================================
using ErrorOr;
using Lumina.Domain.Common.Errors;
using Lumina.Domain.Core.Aggregates.FileManagement.FileManagementAggregate.Strategies.Platform;
using Lumina.Domain.Core.Aggregates.FileManagement.FileManagementAggregate.ValueObjects;
using System.Collections.Generic;
#endregion

namespace Lumina.Domain.Core.Aggregates.FileManagement.FileManagementAggregate.Services;

/// <summary>
/// Service defining methods for handling path-related operations.
/// </summary>
public class PathService : IPathService
{
    private readonly IPlatformContext _platformContext;

    /// <summary>
    /// Gets the character used to separate path segments.
    /// </summary>
    public char PathSeparator => _platformContext.PathStrategy.PathSeparator;

    /// <summary>
    /// Initializes a new instance of the <see cref="PathService"/> class.
    /// </summary>
    /// <param name="platformContextManager">Injected facade service for platform contextual services.</param>
    public PathService(IPlatformContextManager platformContextManager)
    {
        _platformContext = platformContextManager.GetCurrentContext();
    }

    /// <summary>
    /// Checks if <paramref name="path"/> is a valid path.
    /// </summary>
    /// <param name="path">The path to be checked.</param>
    /// <returns><see langword="true"/> if <paramref name="path"/> is a valid path, <see langword="false"/> otherwise.</returns>
    public bool IsValidPath(string path)
    {
        ErrorOr<FileSystemPathId> newPathResult = FileSystemPathId.Create(path);
        if (newPathResult.IsError)
            return false;
        return _platformContext.PathStrategy.IsValidPath(newPathResult.Value);
    }

    /// <summary>
    /// Checks if <paramref name="path"/> exists.
    /// </summary>
    /// <param name="path">The path to be checked.</param>
    /// <param name="includeHiddenElements">Whether to include hidden file system elements or not.</param>
    /// <returns><see langword="true"/> if <paramref name="path"/> exists, <see langword="false"/> otherwise.</returns>
    public bool Exists(string path, bool includeHiddenElements = true)
    {
        ErrorOr<FileSystemPathId> newPathResult = FileSystemPathId.Create(path);
        if (newPathResult.IsError)
            return false;
        return _platformContext.PathStrategy.Exists(newPathResult.Value, includeHiddenElements);
    }

    /// <summary>
    /// Tries to combine <paramref name="path"/> with <paramref name="name"/>.
    /// </summary>
    /// <param name="path">The path to be combined.</param>
    /// <param name="path">The name to be combined with the path.</param>
    /// <returns>An <see cref="ErrorOr{TValue}"/> containing the combined path, or an error.</returns>
    public ErrorOr<string> CombinePath(string path, string name)
    {
        if (string.IsNullOrWhiteSpace(path) || string.IsNullOrWhiteSpace(name))
            return Errors.FileManagement.InvalidPath;
        ErrorOr<FileSystemPathId> newPathResult = FileSystemPathId.Create(path);
        if (newPathResult.IsError)
            return newPathResult.Errors;
        ErrorOr<FileSystemPathId> combinedPathResult = _platformContext.PathStrategy.CombinePath(newPathResult.Value, name);
        if (combinedPathResult.IsError)
            return combinedPathResult.Errors;
        return combinedPathResult.Value.Path;
    }

    /// <summary>
    /// Parses <paramref name="path"/> into path segments.
    /// </summary>
    /// <param name="path">The path to be parsed.</param>
    /// <returns>An <see cref="ErrorOr{TValue}"/> containing the path segments, or an error.</returns>
    public ErrorOr<IEnumerable<PathSegment>> ParsePath(string path)
    {
        ErrorOr<FileSystemPathId> newPathResult = FileSystemPathId.Create(path);
        if (newPathResult.IsError)
            return newPathResult.Errors;
        return _platformContext.PathStrategy.ParsePath(newPathResult.Value);
    }

    /// <summary>
    /// Goes up one level from <paramref name="path"/>, and returns the path segments.
    /// </summary>
    /// <param name="path">The path from which to navigate up one level.</param>
    /// <returns>An <see cref="ErrorOr{TValue}"/> containing the path segments of the path up one level from <paramref name="path"/>, or an error.</returns>
    public ErrorOr<IEnumerable<PathSegment>> GoUpOneLevel(string path)
    {
        ErrorOr<FileSystemPathId> newPathResult = FileSystemPathId.Create(path);
        if (newPathResult.IsError)
            return newPathResult.Errors;
        return _platformContext.PathStrategy.GoUpOneLevel(newPathResult.Value);
    }

    /// <summary>
    /// Returns a collection of characters that are invalid for paths.
    /// </summary>
    /// <returns>A collection of characters that are invalid in the context of paths.</returns>
    public char[] GetInvalidPathCharsForPlatform()
    {
        return _platformContext.PathStrategy.GetInvalidPathCharsForPlatform();
    }

    /// <summary>
    /// Returns the root portion of the given path.
    /// </summary>
    /// <param name="path">The path for which to get the root.</param>
    /// <returns>An <see cref="ErrorOr{TValue}"/> containing the root of <paramref name="path"/>, or an error.</returns>
    public ErrorOr<PathSegment> GetPathRoot(string path)
    {
        ErrorOr<FileSystemPathId> newPathResult = FileSystemPathId.Create(path);
        if (newPathResult.IsError)
            return newPathResult.Errors;
        return _platformContext.PathStrategy.GetPathRoot(newPathResult.Value);
    }
}
