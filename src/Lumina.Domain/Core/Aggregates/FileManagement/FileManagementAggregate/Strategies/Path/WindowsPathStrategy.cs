#region ========================================================================= USING =====================================================================================
using ErrorOr;
using Lumina.Domain.Common.Errors;
using Lumina.Domain.Core.Aggregates.FileManagement.FileManagementAggregate.ValueObjects;
using System;
using System.Collections.Generic;
using System.IO.Abstractions;
using System.Linq;
using System.Text.RegularExpressions;
#endregion

namespace Lumina.Domain.Core.Aggregates.FileManagement.FileManagementAggregate.Strategies.Path;

/// <summary>
/// Service defining methods for handling path-related operations on Windows platforms.
/// </summary>
public class WindowsPathStrategy : IWindowsPathStrategy
{
    #region ================================================================== FIELD MEMBERS ================================================================================
    private readonly IFileSystem _fileSystem;
    #endregion

    #region ==================================================================== PROPERTIES =================================================================================
    public char PathSeparator => '\\';
    #endregion

    #region ====================================================================== CTOR =====================================================================================
    /// <summary>
    /// Initializes a new instance of the <see cref="WindowsPathStrategy"/> class.
    /// </summary>
    /// <param name="fileSystem">Injected service used to interact with the local filesystem.</param>
    public WindowsPathStrategy(IFileSystem fileSystem)
    {
        _fileSystem = fileSystem;
    }
    #endregion

    #region ===================================================================== METHODS ===================================================================================
    /// <summary>
    /// Checks if <paramref name="path"/> is a valid path.
    /// </summary>
    /// <param name="path">The path to be checked.</param>
    /// <returns><see langword="true"/> if <paramref name="path"/> is a valid path, <see langword="false"/> otherwise.</returns>
    public bool IsValidPath(FileSystemPathId path)
    {
        // check for invalid path characters
        char[] invalidChars = GetInvalidPathCharsForPlatform();
        if (path.Path.IndexOfAny(invalidChars) >= 0)
            return false;
        // regular expression to match valid absolute paths
        // this allows drive letters (e.g., C:\) and UNC paths (e.g., \\server\share)
        string pathPattern = @"^[a-zA-Z]:\\(?:[a-zA-Z0-9\s()._-]+\\)*[a-zA-Z0-9\s()._-]*\\?$";
        return Regex.IsMatch(path.Path, pathPattern);
    }

    /// <summary>
    /// Checks if <paramref name="path"/> exists.
    /// </summary>
    /// <param name="path">The path to be checked.</param>
    /// <returns><see langword="true"/> if <paramref name="path"/> exists, <see langword="false"/> otherwise.</returns>
    public bool Exists(FileSystemPathId path)
    {
        return _fileSystem.Path.Exists(path.Path);
    }

    /// <summary>
    /// Tries to combine <paramref name="path"/> with <paramref name="name"/>.
    /// </summary>
    /// <param name="path">The path to be combined.</param>
    /// <param name="name">The name to be combined with the path.</param>
    /// <returns>An <see cref="ErrorOr{TValue}"/> containing the combined path, or an error.</returns>
    public ErrorOr<FileSystemPathId> CombinePath(FileSystemPathId path, string name)
    {
        if (name == null)
            return Errors.FileManagement.NameCannotBeEmpty;
        // trim any directory separator characters from the end of the path
        string subpath = path.Path.TrimEnd(PathSeparator);
        // if the name begins with a directory separator, remove it
        name = name.TrimStart(PathSeparator);
        // combine the two parts with the Windows directory separator character
        return FileSystemPathId.Create(subpath + PathSeparator + name + PathSeparator);
    }

    /// <summary>
    /// Parses <paramref name="path"/> into path segments.
    /// </summary>
    /// <param name="path">The path to be parsed.</param>
    /// <returns>An <see cref="ErrorOr{TValue}"/> containing the path segments, or an error.</returns>
    public ErrorOr<IEnumerable<PathSegment>> ParsePath(FileSystemPathId path)
    {
        // Windows paths usually start with a drive letter and colon, e.g., "C:"
        return !path.Path.Contains(':') || !path.Path.Contains(PathSeparator) || !char.IsLetter(path.Path[0]) || path.Path[1] != ':' || path.Path[2] != PathSeparator
            ? (ErrorOr<IEnumerable<PathSegment>>)Errors.FileManagement.InvalidPath
            : ErrorOrFactory.From(GetPathSegments());
        IEnumerable<PathSegment> GetPathSegments()
        {
            // the drive segment
            yield return new PathSegment(path.Path[..2], isDirectory: false, isDrive: true);
            // extract the other segments
            string[] segments = path.Path[3..].Split(new[] { PathSeparator }, StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < segments.Length; i++)
            {
                string segment = segments[i];
                bool isDirectory;
                if (segment.Contains('.'))
                    isDirectory = i != segments.Length - 1 || path.Path.EndsWith(PathSeparator); // check if it's the last segment or if the next segment also contains a path delimiter
                else
                    isDirectory = true;
                yield return new PathSegment(segment, isDirectory, isDrive: false);
            }
        }
    }

    /// <summary>
    /// Goes up one level from <paramref name="path"/>, and returns the path segments.
    /// </summary>
    /// <param name="path">The path from which to navigate up one level.</param>
    /// <returns>An <see cref="ErrorOr{TValue}"/> containing the path segments of the path up one level from <paramref name="path"/>, or an error.</returns>
    public ErrorOr<IEnumerable<PathSegment>> GoUpOneLevel(FileSystemPathId path)
    {
        // validation: ensure the path is not null or empty
        if (!IsValidPath(path))
            return Errors.FileManagement.InvalidPath;
        // if path is just a drive letter followed by ":\", return null
        if (Regex.IsMatch(path.Path, @"^[a-zA-Z]:\\?$"))
            return Errors.FileManagement.CannotNavigateUp;
        // trim trailing slash for consistent processing
        string tempPath = path.Path;
        if (tempPath.EndsWith('\\'))
            tempPath = tempPath.TrimEnd(PathSeparator);
        // find the last occurrence of a slash
        int lastIndex = tempPath.LastIndexOf(PathSeparator);
        // if there's no slash found (shouldn't happen due to previous steps), or if we are at the root level after trimming, return error
        if (lastIndex < 0)
            return Errors.FileManagement.CannotNavigateUp;
        // if we are at the drive root level after trimming, return drive root, otherwise, return the path up to the last slash
        ErrorOr<FileSystemPathId> newPathResult = FileSystemPathId.Create(lastIndex == 2 && tempPath[1] == ':' ? tempPath[..3] : tempPath[..lastIndex]);
        return newPathResult.IsError ? (ErrorOr<IEnumerable<PathSegment>>)newPathResult.Errors : ParsePath(newPathResult.Value);
    }

    /// <summary>
    /// Returns a collection of characters that are invalid for paths.
    /// </summary>
    /// <returns>A collection of characters that are invalid in the context of paths</returns>
    public char[] GetInvalidPathCharsForPlatform()
    {
        return ['<', '>', '"', '/', '|', '?', '*'];
    }

    /// <summary>
    /// Returns the root portion of the given path.
    /// </summary>
    /// <param name="path">The path for which to get the root.</param>
    /// <returns>An <see cref="ErrorOr{TValue}"/> containing the root of <paramref name="path"/>, or an error.</returns>
    public ErrorOr<PathSegment> GetPathRoot(FileSystemPathId path)
    {
        if (!IsValidPath(path))
            return Errors.FileManagement.InvalidPath;
        // check if the path starts with a drive letter (e.g., "C:")
        if (path.Path.Length >= 2 && char.IsLetter(path.Path[0]) && path.Path[1] == ':')
        {
            string root = path.Path[..2];
            if (path.Path.Length > 2 && path.Path[2] == PathSeparator)
                root += PathSeparator;
            return new PathSegment(root, isDirectory: true, isDrive: true);
        }
        // check for UNC paths (e.g., "\\server\share")
        if (path.Path.StartsWith('\\'))
        {
            ErrorOr<IEnumerable<PathSegment>> parseResult = ParsePath(path);
            if (parseResult.IsError)
                return parseResult.Errors;

            List<PathSegment> segments = parseResult.Value.ToList();
            if (segments.Count >= 2)
            {
                string root = $@"\\{segments[0].Name}\{segments[1].Name}\";
                return new PathSegment(root, isDirectory: true, isDrive: false);
            }
        }
        return Errors.FileManagement.InvalidPath;
    }
    #endregion
}