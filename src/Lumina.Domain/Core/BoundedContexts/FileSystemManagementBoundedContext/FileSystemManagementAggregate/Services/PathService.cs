#region ========================================================================= USING =====================================================================================
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.Common.Errors;
using Lumina.Domain.Core.BoundedContexts.FileSystemManagementBoundedContext.FileSystemManagementAggregate.Strategies.Platform;
using Lumina.Domain.Core.BoundedContexts.FileSystemManagementBoundedContext.FileSystemManagementAggregate.ValueObjects;
using System;
using System.Collections.Generic;
using System.Linq;
#endregion

namespace Lumina.Domain.Core.BoundedContexts.FileSystemManagementBoundedContext.FileSystemManagementAggregate.Services;

/// <summary>
/// Service defining methods for handling path-related operations.
/// </summary>
public class PathService : IPathService
{
    private const int MAX_DIRECTORY_SEGMENT_LENGTH = 100;

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
        Result<FileSystemPathId> newPathResult = FileSystemPathId.Create(path);
        if (newPathResult.IsFailure)
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
        Result<FileSystemPathId> newPathResult = FileSystemPathId.Create(path);
        if (newPathResult.IsFailure)
            return false;
        return _platformContext.PathStrategy.Exists(newPathResult.Value, includeHiddenElements);
    }

    /// <summary>
    /// Tries to combine <paramref name="path"/> with <paramref name="name"/>.
    /// </summary>
    /// <param name="path">The path to be combined.</param>
    /// <param name="path">The name to be combined with the path.</param>
    /// <returns>An <see cref="Result{TValue}"/> containing the combined path, or an error.</returns>
    public Result<string> CombinePath(string path, string name)
    {
        if (string.IsNullOrWhiteSpace(path) || string.IsNullOrWhiteSpace(name))
            return Errors.FileSystemManagement.InvalidPath;
        Result<FileSystemPathId> newPathResult = FileSystemPathId.Create(path);
        if (newPathResult.IsFailure)
            return newPathResult.Errors;
        Result<FileSystemPathId> combinedPathResult = _platformContext.PathStrategy.CombinePath(newPathResult.Value, name);
        if (combinedPathResult.IsFailure)
            return combinedPathResult.Errors;
        return combinedPathResult.Value.Path;
    }

    /// <summary>
    /// Parses <paramref name="path"/> into path segments.
    /// </summary>
    /// <param name="path">The path to be parsed.</param>
    /// <returns>An <see cref="Result{TValue}"/> containing the path segments, or an error.</returns>
    public Result<IEnumerable<PathSegment>> ParsePath(string path)
    {
        Result<FileSystemPathId> newPathResult = FileSystemPathId.Create(path);
        if (newPathResult.IsFailure)
            return newPathResult.Errors;
        return _platformContext.PathStrategy.ParsePath(newPathResult.Value);
    }

    /// <summary>
    /// Goes up one level from <paramref name="path"/>, and returns the path segments.
    /// </summary>
    /// <param name="path">The path from which to navigate up one level.</param>
    /// <returns>An <see cref="Result{TValue}"/> containing the path segments of the path up one level from <paramref name="path"/>, or an error.</returns>
    public Result<IEnumerable<PathSegment>> GoUpOneLevel(string path)
    {
        Result<FileSystemPathId> newPathResult = FileSystemPathId.Create(path);
        if (newPathResult.IsFailure)
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
    /// Sanitizes a human-readable <paramref name="name"/> into a safe path segment, replacing the characters that are invalid for the current platform, and rejecting the segments that could escape the directory.
    /// </summary>
    /// <param name="name">The human-readable name to sanitize.</param>
    /// <returns>An <see cref="Result{TValue}"/> containing the sanitized path segment, or an error.</returns>
    public Result<PathSegment> SanitizeSegment(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return Errors.FileSystemManagement.NameCannotBeEmpty;

        char[] invalidChars = _platformContext.PathStrategy.GetInvalidPathSegmentCharsForPlatform();
        char[] sanitizedChars = [.. name.Trim().Select(character => Array.IndexOf(invalidChars, character) >= 0 ? ' ' : character)];

        // collapse the consecutive whitespace into a single space, so that the segment does not contain ragged spacing
        List<char> collapsedChars = [];
        bool previousWasWhitespace = false;
        foreach (char character in sanitizedChars)
        {
            if (char.IsWhiteSpace(character))
            {
                if (!previousWasWhitespace)
                    collapsedChars.Add(' ');
                previousWasWhitespace = true;
            }
            else
            {
                collapsedChars.Add(character);
                previousWasWhitespace = false;
            }
        }

        string sanitized = new string([.. collapsedChars]).Trim();
        if (string.Equals(sanitized, ".", StringComparison.Ordinal) || string.Equals(sanitized, "..", StringComparison.Ordinal))
            return Errors.FileSystemManagement.InvalidPath;
        // a segment must never contain a path separator, otherwise the segment could escape its directory or nest additional directories
        if (sanitized.Contains('/') || sanitized.Contains('\\'))
            return Errors.FileSystemManagement.InvalidPath;
        if (sanitized.Length > MAX_DIRECTORY_SEGMENT_LENGTH)
            sanitized = sanitized[..MAX_DIRECTORY_SEGMENT_LENGTH].TrimEnd();
        if (sanitized.Length == 0)
            return Errors.FileSystemManagement.InvalidPath;

        return PathSegment.Create(sanitized, isDirectory: true, isDrive: false);
    }

    /// <summary>
    /// Returns the root portion of the given path.
    /// </summary>
    /// <param name="path">The path for which to get the root.</param>
    /// <returns>An <see cref="Result{TValue}"/> containing the root of <paramref name="path"/>, or an error.</returns>
    public Result<PathSegment> GetPathRoot(string path)
    {
        Result<FileSystemPathId> newPathResult = FileSystemPathId.Create(path);
        if (newPathResult.IsFailure)
            return newPathResult.Errors;
        return _platformContext.PathStrategy.GetPathRoot(newPathResult.Value);
    }
}
