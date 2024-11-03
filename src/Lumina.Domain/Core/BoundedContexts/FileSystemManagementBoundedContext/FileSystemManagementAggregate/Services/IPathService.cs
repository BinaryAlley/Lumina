#region ========================================================================= USING =====================================================================================
using ErrorOr;
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
    /// <param name="includeHiddenElements">Whether to include hidden file system elements or not.</param>
    /// <returns><see langword="true"/> if <paramref name="path"/> exists, <see langword="false"/> otherwise.</returns>
    bool Exists(string path, bool includeHiddenElements = true);

    /// <summary>
    /// Tries to combine <paramref name="path"/> with <paramref name="name"/>.
    /// </summary>
    /// <param name="path">The path to be combined.</param>
    /// <param name="path">The name to be combined with the path.</param>
    /// <returns>An <see cref="ErrorOr{TValue}"/> containing the combined path, or an error.</returns>
    ErrorOr<string> CombinePath(string path, string name);

    /// <summary>
    /// Parses <paramref name="path"/> into path segments.
    /// </summary>
    /// <param name="path">The path to be parsed.</param>
    /// <returns>An <see cref="ErrorOr{TValue}"/> containing the path segments, or an error.</returns>
    ErrorOr<IEnumerable<PathSegment>> ParsePath(string path);

    /// <summary>
    /// Goes up one level from <paramref name="path"/>, and returns the path segments.
    /// </summary>
    /// <param name="path">The path from which to navigate up one level.</param>
    /// <returns>An <see cref="ErrorOr{TValue}"/> containing the path segments of the path up one level from <paramref name="path"/>, or an error.</returns>
    ErrorOr<IEnumerable<PathSegment>> GoUpOneLevel(string path);

    /// <summary>
    /// Returns a collection of characters that are invalid for paths.
    /// </summary>
    /// <returns>A collection of characters that are invalid in the context of paths.</returns>
    char[] GetInvalidPathCharsForPlatform();

    /// <summary>
    /// Returns the root portion of the given path.
    /// </summary>
    /// <param name="path">The path for which to get the root.</param>
    /// <returns>An <see cref="ErrorOr{TValue}"/> containing the root of <paramref name="path"/>, or an error.</returns>
    ErrorOr<PathSegment> GetPathRoot(string path);
}
