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
/// Service defining methods for handling path-related operations on Windows platforms.
/// </summary>
public class WindowsPathStrategy : IWindowsPathStrategy
{
    private readonly IFileSystem _fileSystem;

    public char PathSeparator => '\\';

    /// <summary>
    /// Initializes a new instance of the <see cref="WindowsPathStrategy"/> class.
    /// </summary>
    /// <param name="fileSystem">Injected service used to interact with the local filesystem.</param>
    public WindowsPathStrategy(IFileSystem fileSystem)
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
        // Regular expression to match valid absolute paths.
        // This allows drive letters (e.g., C:\) and UNC paths (e.g., \\server\share).
        const string PATH_PATTERN = @"^(?:[a-zA-Z]:\\|\\\\[a-zA-Z0-9\s()._&+,\[\]-]+\\[a-zA-Z0-9\s()._&+,\[\]-]+)(?:[a-zA-Z0-9\s()._&+,\[\]-]+\\)*[a-zA-Z0-9\s()._&+,\[\]-]*\\?$";
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
        // Combine the two parts with the Windows directory separator character.
        return FileSystemPathId.Create(subpath + PathSeparator + name + PathSeparator);
    }

    /// <summary>
    /// Parses <paramref name="path"/> into path segments.
    /// </summary>
    /// <param name="path">The path to be parsed.</param>
    /// <returns>An <see cref="Result{TValue}"/> containing the path segments, or an error.</returns>
    public Result<IEnumerable<PathSegment>> ParsePath(FileSystemPathId path)
    {
        // Windows paths usually start with a drive letter and colon (e.g., "C:") or with "\\" (UNC paths).
        if (!(path.Path.StartsWith(@"\\") || (path.Path.Length >= 3 && char.IsLetter(path.Path[0]) && path.Path[1] == ':' && path.Path[2] == PathSeparator)))
            return Errors.FileSystemManagement.InvalidPath;
        IEnumerable<Result<PathSegment>> getPathSegmentsResults = GetPathSegments();
        foreach (Result<PathSegment> getPathSegmentsResult in getPathSegmentsResults)
            if (getPathSegmentsResult.IsFailure)
                return getPathSegmentsResult.Errors;
        return Result.From(getPathSegmentsResults.Select(getPathSegmentsResult => getPathSegmentsResult.Value));
        IEnumerable<Result<PathSegment>> GetPathSegments()
        {
            if (path.Path.StartsWith(@"\\"))
            {
                // Handle a UNC path.
                yield return PathSegment.Create(@"\\", isDirectory: false, isDrive: true);

                string[] segments = path.Path[2..].Split([PathSeparator], StringSplitOptions.RemoveEmptyEntries);
                for (int i = 0; i < segments.Length; i++)
                    yield return CreatePathSegment(segments[i], i, segments.Length);
            }
            else // Handle a regular Windows path.
            {
                // The drive segment.
                yield return PathSegment.Create(path.Path[..2], isDirectory: false, isDrive: true);
                // Extract the other segments.
                string[] segments = path.Path[3..].Split([PathSeparator], StringSplitOptions.RemoveEmptyEntries);
                for (int i = 0; i < segments.Length; i++)
                {
                    string segment = segments[i];
                    bool isDirectory;
                    if (segment.Contains('.'))
                        isDirectory = i != segments.Length - 1 || path.Path.EndsWith(PathSeparator); // Check if it is the last segment, or if the next segment also contains a path delimiter.
                    else
                        isDirectory = true;
                    yield return PathSegment.Create(segment, isDirectory, isDrive: false);
                }
            }
        }
        Result<PathSegment> CreatePathSegment(string segment, int index, int totalSegments)
        {
            bool isDirectory = segment.Contains('.')
                ? index != totalSegments - 1 || path.Path.EndsWith(PathSeparator)
                : true;
            return PathSegment.Create(segment, isDirectory, isDrive: false);
        }
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
        // Trim the trailing backslash for consistent processing.
        string tempPath = path.Path.TrimEnd(PathSeparator);
        // Check for a UNC path.
        if (tempPath.StartsWith(@"\\"))
        {
            string[] parts = tempPath.Split(PathSeparator, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length <= 2)
                return Errors.FileSystemManagement.CannotNavigateUp;
            // If it is a single-level UNC path, return the UNC root.
            if (parts.Length == 3)
                return ParsePath(FileSystemPathId.Create($@"\\{parts[0]}\{parts[1]}").Value);
        } // Check for the drive root, both with and without a trailing backslash.
        else if ((tempPath.Length == 2 && tempPath[1] == ':') || (tempPath.Length == 3 && tempPath[1] == ':' && tempPath[2] == PathSeparator))
            return Errors.FileSystemManagement.CannotNavigateUp;
        // Find the last occurrence of a backslash.
        int lastIndex = tempPath.LastIndexOf(PathSeparator);
        // If no backslash is found or it is at the root level, return the root.
        if (lastIndex <= 2)
            return ParsePath(FileSystemPathId.Create(tempPath[..3]).Value);
        // Return the path up to the last backslash.
        Result<FileSystemPathId> newPathResult = FileSystemPathId.Create(tempPath[..lastIndex]);
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
        return ['<', '>', '"', '/', '|', '?', '*'];
    }

    /// <summary>
    /// Returns a collection of characters that are invalid for a single path segment.
    /// </summary>
    /// <returns>A collection of characters that are invalid in the context of a single path segment.</returns>
    public char[] GetInvalidPathSegmentCharsForPlatform()
    {
        return ['<', '>', '"', '/', '|', '?', '*', ':'];
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
        // Handle UNC paths (e.g., \\server\share\folder).
        if (path.Path.StartsWith(@"\\"))
        {
            // Find the position of the second backslash (after \\server).
            int secondBackslash = path.Path.IndexOf('\\', 2);
            if (secondBackslash == -1)
                return Errors.FileSystemManagement.InvalidPath; // Invalid UNC path, missing server name.
            // Find the position of the third backslash (after \\server\share).
            int thirdBackslash = path.Path.IndexOf('\\', secondBackslash + 1);
            if (thirdBackslash == -1)
                thirdBackslash = path.Path.Length; // No third backslash, use the entire path.
            // Return the UNC root (\\server\share\).
            return PathSegment.Create(path.Path[..thirdBackslash] + "\\", isDirectory: true, isDrive: false);
        }
        else // Handle drive letter paths (e.g., C:\folder).
             // Check if the path starts with a drive letter followed by a colon (e.g., C:).
             // The path must be at least 2 characters long.
             // The first character must be a letter.
             // The second character must be a colon.
            if (path.Path.Length >= 2 && char.IsLetter(path.Path[0]) && path.Path[1] == ':')
            return PathSegment.Create(path.Path[..2] + "\\", isDirectory: true, isDrive: true); // Return the drive root (e.g., C:\).
        // If we reach here, the path is neither a valid UNC path nor a valid drive path.
        return Errors.FileSystemManagement.InvalidPath;
    }

    /// <summary>
    /// Checks whether <paramref name="path"/> is located inside <paramref name="parentPath"/>. The comparison resolves the "." and ".."
    /// segments first, so that a path can never escape its parent through relative segments, and it is case-insensitive, because
    /// Windows paths are case-insensitive.
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
            if (!string.Equals(pathSegments[index], parentPathSegments[index], StringComparison.OrdinalIgnoreCase))
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
