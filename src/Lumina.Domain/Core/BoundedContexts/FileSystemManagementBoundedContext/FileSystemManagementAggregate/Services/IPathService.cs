#region ========================================================================= USING =====================================================================================
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.Core.BoundedContexts.FileSystemManagementBoundedContext.FileSystemManagementAggregate.ValueObjects;
using System.Collections.Generic;
#endregion

namespace Lumina.Domain.Core.BoundedContexts.FileSystemManagementBoundedContext.FileSystemManagementAggregate.Services;

/// <summary>
/// Interface for the service for handling file system paths.
/// </summary>
public interface IPathService
{
    /// <summary>
    /// Gets the character used to separate path segments.
    /// </summary>
    char PathSeparator { get; }

    /// <summary>
    /// Checks if <paramref name="path"/> is a valid path.
    /// </summary>
    /// <param name="path">The path to be checked.</param>
    /// <returns><see langword="true"/> if <paramref name="path"/> is a valid path, <see langword="false"/> otherwise.</returns>
    bool IsValidPath(string path);

    /// <summary>
    /// Checks if <paramref name="path"/> exists.
    /// </summary>
    /// <param name="path">The path to be checked.</param>
    /// <param name="shouldIncludeHiddenElements">Whether to include hidden file system elements or not.</param>
    /// <returns><see langword="true"/> if <paramref name="path"/> exists, <see langword="false"/> otherwise.</returns>
    bool Exists(string path, bool shouldIncludeHiddenElements = true);

    /// <summary>
    /// Tries to combine <paramref name="path"/> with <paramref name="name"/>.
    /// </summary>
    /// <param name="path">The path to be combined.</param>
    /// <param name="path">The name to be combined with the path.</param>
    /// <returns>An <see cref="Result{TValue}"/> containing the combined path, or an error.</returns>
    Result<string> CombinePath(string path, string name);

    /// <summary>
    /// Parses <paramref name="path"/> into path segments.
    /// </summary>
    /// <param name="path">The path to be parsed.</param>
    /// <returns>An <see cref="Result{TValue}"/> containing the path segments, or an error.</returns>
    Result<IEnumerable<PathSegment>> ParsePath(string path);

    /// <summary>
    /// Goes up one level from <paramref name="path"/>, and returns the path segments.
    /// </summary>
    /// <param name="path">The path from which to navigate up one level.</param>
    /// <returns>An <see cref="Result{TValue}"/> containing the path segments of the path up one level from <paramref name="path"/>, or an error.</returns>
    Result<IEnumerable<PathSegment>> GoUpOneLevel(string path);

    /// <summary>
    /// Gets the name of the file or directory stored at <paramref name="path"/>, using the path separator of the current platform.
    /// </summary>
    /// <param name="path">The path whose name is retrieved.</param>
    /// <returns>The name of the file or directory stored at <paramref name="path"/>.</returns>
    string GetFileName(string path);

    /// <summary>
    /// Gets the name of the file stored at <paramref name="path"/> without its extension, using the path separator of the current platform.
    /// </summary>
    /// <param name="path">The path whose file name is retrieved.</param>
    /// <returns>The name of the file stored at <paramref name="path"/> without its extension.</returns>
    string GetFileNameWithoutExtension(string path);

    /// <summary>
    /// Returns a collection of characters that are invalid for paths.
    /// </summary>
    /// <returns>A collection of characters that are invalid in the context of paths.</returns>
    char[] GetInvalidPathCharsForPlatform();

    /// <summary>
    /// Sanitizes a human-readable <paramref name="name"/> into a safe path segment.
    /// </summary>
    /// <param name="name">The human-readable name to sanitize.</param>
    /// <returns>An <see cref="Result{TValue}"/> containing the sanitized path segment, or an error.</returns>
    Result<PathSegment> SanitizeSegment(string name);

    /// <summary>
    /// Returns the root portion of the given path.
    /// </summary>
    /// <param name="path">The path for which to get the root.</param>
    /// <returns>An <see cref="Result{TValue}"/> containing the root of <paramref name="path"/>, or an error.</returns>
    Result<PathSegment> GetPathRoot(string path);

    /// <summary>
    /// Checks whether <paramref name="path"/> is located inside <paramref name="parentPath"/>.
    /// </summary>
    /// <param name="path">The path to be checked.</param>
    /// <param name="parentPath">The path that must contain the checked path.</param>
    /// <returns><see langword="true"/> if the path is inside the parent path, <see langword="false"/> otherwise.</returns>
    bool IsPathWithin(string path, string parentPath);
}
