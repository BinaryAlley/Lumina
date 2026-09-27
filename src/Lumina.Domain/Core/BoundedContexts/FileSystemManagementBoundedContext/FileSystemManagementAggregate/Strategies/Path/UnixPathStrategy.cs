#region ========================================================================= USING =====================================================================================
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.Common.Errors;
using Lumina.Domain.Core.BoundedContexts.FileSystemManagementBoundedContext.FileSystemManagementAggregate.ValueObjects;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using System.Text.RegularExpressions;
#endregion

namespace Lumina.Domain.Core.BoundedContexts.FileSystemManagementBoundedContext.FileSystemManagementAggregate.Strategies.Path;

/// <summary>
/// Service defining methods for handling path-related operations on UNIX platforms.
/// </summary>
public class UnixPathStrategy : IUnixPathStrategy
{
    private readonly IFileSystem _fileSystem;

    public char PathSeparator => '/';

    /// <summary>
    /// Initializes a new instance of the <see cref="UnixPathStrategy"/> class.
    /// </summary>
    /// <param name="fileSystem">Injected service used to interact with the local filesystem.</param>
    public UnixPathStrategy(IFileSystem fileSystem)
    {
        _fileSystem = fileSystem;
    }

    /// <summary>
    /// Checks if <paramref name="path"/> is a valid path.
    /// </summary>
    /// <param name="path">The path to be checked.</param>
    /// <returns><see langword="true"/> if <paramref name="path"/> is a valid path, <see langword="false"/> otherwise.</returns>
    public bool IsValidPath(FileSystemPathId path)
    {
        // Check for invalid path characters.
        char[] invalidChars = GetInvalidPathCharsForPlatform();
        if (path.Path.IndexOfAny(invalidChars) >= 0)
            return false;
        // Check for relative paths.
        if (path.Path.StartsWith("./") || path.Path.StartsWith("../"))
            return false;
        const string PATH_PATTERN = @"^\/([\w\-\.\~!$&'()*+,;=:@\[\] ]+(\/[\w\-\.\~!$&'()*+,;=:@\[\] ]+)*)?\/?$";
        return Regex.IsMatch(path.Path, PATH_PATTERN);
    }

    /// <summary>
    /// Checks if <paramref name="path"/> exists.
    /// </summary>
    /// <param name="path">The path to be checked.</param>
    /// <param name="shouldIncludeHiddenElements">Whether to include hidden file system elements or not.</param>
    /// <returns><see langword="true"/> if <paramref name="path"/> exists, <see langword="false"/> otherwise.</returns>
    public bool Exists(FileSystemPathId path, bool shouldIncludeHiddenElements = true)
    {
        if (!_fileSystem.Path.Exists(path.Path))
            return false;
        bool isHidden;
        if (_fileSystem.Directory.Exists(path.Path))
        {
            IDirectoryInfo dirInfo = _fileSystem.DirectoryInfo.New(path.Path);
            isHidden = (dirInfo.Attributes & FileAttributes.Hidden) == FileAttributes.Hidden;
        }
        else if (_fileSystem.File.Exists(path.Path))
        {
            IFileInfo fileInfo = _fileSystem.FileInfo.New(path.Path);
            isHidden = (fileInfo.Attributes & FileAttributes.Hidden) == FileAttributes.Hidden;
        }
        else // The path exists but is neither a file nor a directory (drive, etc.).
            return true;
        return shouldIncludeHiddenElements || !isHidden;
    }

    /// <summary>
    /// Tries to combine <paramref name="path"/> with <paramref name="name"/>.
    /// </summary>
    /// <param name="path">The path to be combined.</param>
    /// <param name="name">The name to be combined with the path.</param>
    /// <returns>An <see cref="Result{TValue}"/> containing the combined path, or an error.</returns>
    public Result<FileSystemPathId> CombinePath(FileSystemPathId path, string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return Errors.FileSystemManagement.NameCannotBeEmpty;
        // Trim any directory separator characters from the end of the path.
        string subpath = path.Path.TrimEnd(PathSeparator);
        // If the name begins with a directory separator, remove it.
        name = name.TrimStart(PathSeparator);
        // Combine the two parts with the Unix directory separator character.
        return FileSystemPathId.Create(subpath + PathSeparator + name + PathSeparator);
    }

    /// <summary>
    /// Parses <paramref name="path"/> into path segments.
    /// </summary>
    /// <param name="path">The path to be parsed.</param>
    /// <returns>An <see cref="Result{TValue}"/> containing the path segments, or an error.</returns>
    public Result<IEnumerable<PathSegment>> ParsePath(FileSystemPathId path)
    {
        // If the path starts with anything other than '/', it is considered relative and invalid for this parser.
        if (!path.Path.StartsWith(PathSeparator))
            return Errors.FileSystemManagement.InvalidPath;
        // Get the path segments.
        List<string> splitSegments = [.. path.Path.Split(new[] { PathSeparator }, StringSplitOptions.RemoveEmptyEntries)];
        IEnumerable<Result<PathSegment>> segmentsResults = splitSegments.Select((segment, index) =>
        {
            bool isDirectory;
            if (segment.Contains('.'))
                isDirectory = index != splitSegments.Count - 1 || path.Path.EndsWith(PathSeparator); // Check if it is the last segment or if the path ends with a '/'.
            else
                isDirectory = true;
            return PathSegment.Create(segment, isDirectory, isDrive: false);
        }).Prepend(PathSegment.Create(PathSeparator.ToString(), isDirectory: false, isDrive: true)); // UNIX paths have '/' as their "root drive".
        foreach (Result<PathSegment> segment in segmentsResults)
            if (segment.IsFailure)
                return segment.Errors;
        return Result.From(segmentsResults.Select(segmentResult => segmentResult.Value));
    }

    /// <summary>
    /// Goes up one level from <paramref name="path"/>, and returns the path segments.
    /// </summary>
    /// <param name="path">The path from which to navigate up one level.</param>
    /// <returns>An <see cref="Result{TValue}"/> containing the path segments of the path up one level from <paramref name="path"/>, or an error.</returns>
    public Result<IEnumerable<PathSegment>> GoUpOneLevel(FileSystemPathId path)
    {
        // Validation: ensure the path is not null or empty.
        if (!IsValidPath(path))
            return Errors.FileSystemManagement.InvalidPath;
        // Trim the trailing slash for consistent processing.
        string tempPath = path.Path;
        if (tempPath.EndsWith('/'))
            tempPath = tempPath.TrimEnd(PathSeparator);
        // Find the last occurrence of a slash.
        int lastIndex = tempPath.LastIndexOf(PathSeparator);
        // If no slash is found (which should not happen after the previous steps), or if we are at the root level after trimming, return an error.
        if (lastIndex < 0)
            return Errors.FileSystemManagement.CannotNavigateUp;
        // Return the path up to the last slash, or, if there is only the root slash, return that one instead.
        Result<FileSystemPathId> newPathResult = FileSystemPathId.Create(lastIndex > 0 ? tempPath[..lastIndex] : tempPath[..1]);
        if (newPathResult.IsFailure)
            return newPathResult.Errors;
        return ParsePath(newPathResult.Value);
    }

    /// <summary>
    /// Returns a collection of characters that are invalid for paths.
    /// </summary>
    /// <returns>A collection of characters that are invalid in the context of paths.</returns>
    public char[] GetInvalidPathCharsForPlatform()
    {
        return ['\0'];
    }

    /// <summary>
    /// Returns a collection of characters that are invalid for a single path segment.
    /// </summary>
    /// <returns>A collection of characters that are invalid in the context of a single path segment.</returns>
    public char[] GetInvalidPathSegmentCharsForPlatform()
    {
        return ['\0'];
    }

    /// <summary>
    /// Returns the root portion of the given path.
    /// </summary>
    /// <param name="path">The path for which to get the root.</param>
    /// <returns>An <see cref="Result{TValue}"/> containing the root of <paramref name="path"/>, or an error.</returns>
    public Result<PathSegment> GetPathRoot(FileSystemPathId path)
    {
        if (!IsValidPath(path))
            return Errors.FileSystemManagement.InvalidPath;

        // On Unix-like systems, the root is always "/".
        if (path.Path.StartsWith(PathSeparator))
            return PathSegment.Create(PathSeparator.ToString(), isDirectory: false, isDrive: true);
        return Errors.FileSystemManagement.InvalidPath;
    }

    /// <summary>
    /// Checks whether <paramref name="path"/> is located inside <paramref name="parentPath"/>. The comparison resolves the "." and ".."
    /// segments first, so that a path can never escape its parent through relative segments, and it is case-sensitive, because
    /// Unix paths are case-sensitive.
    /// </summary>
    /// <param name="path">The path to be checked.</param>
    /// <param name="parentPath">The path that must contain the checked path.</param>
    /// <returns><see langword="true"/> if the path is inside the parent path, <see langword="false"/> otherwise.</returns>
    public bool IsPathWithin(FileSystemPathId path, FileSystemPathId parentPath)
    {
        string[] pathSegments = NormalizePathSegments(path.Path);
        string[] parentPathSegments = NormalizePathSegments(parentPath.Path);
        if (parentPathSegments.Length == 0 || pathSegments.Length <= parentPathSegments.Length)
            return false;
        for (int index = 0; index < parentPathSegments.Length; index++)
            if (!string.Equals(pathSegments[index], parentPathSegments[index], StringComparison.Ordinal))
                return false;
        return true;
    }

    /// <summary>
    /// Splits <paramref name="path"/> into its segments, resolving the "." (current directory) and ".." (parent directory) segments.
    /// </summary>
    /// <param name="path">The path to be normalized.</param>
    /// <returns>The normalized path segments of the path.</returns>
    private string[] NormalizePathSegments(string path)
    {
        List<string> segments = [];
        foreach (string segment in path.Split([PathSeparator], StringSplitOptions.RemoveEmptyEntries))
        {
            if (segment == ".")
                continue;
            if (segment == "..")
            {
                if (segments.Count > 0)
                    segments.RemoveAt(segments.Count - 1);
                continue;
            }
            segments.Add(segment);
        }
        return [.. segments];
    }
}
