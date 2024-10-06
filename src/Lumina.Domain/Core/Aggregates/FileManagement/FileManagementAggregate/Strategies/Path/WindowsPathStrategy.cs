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
        const string PATH_PATTERN = @"^(?:[a-zA-Z]:\\|\\\\[a-zA-Z0-9\s()._\[\]-]+\\[a-zA-Z0-9\s()._\[\]-]+)(?:[a-zA-Z0-9\s()._\[\]-]+\\)*[a-zA-Z0-9\s()._\[\]-]*\\?$";
        return Regex.IsMatch(path.Path, PATH_PATTERN);
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
        // Windows paths usually start with a drive letter and colon, e.g., "C:", or "\\" (UNC paths)
        if (!(path.Path.StartsWith(@"\\") || (path.Path.Length >= 3 && char.IsLetter(path.Path[0]) && path.Path[1] == ':' && path.Path[2] == PathSeparator)))
            return Errors.FileManagement.InvalidPath;
        IEnumerable<ErrorOr<PathSegment>> getPathSegmentsResults = GetPathSegments();
        foreach (ErrorOr<PathSegment> getPathSegmentsResult in getPathSegmentsResults)
            if (getPathSegmentsResult.IsError)
                return getPathSegmentsResult.Errors;
        return ErrorOrFactory.From(getPathSegmentsResults.Select(getPathSegmentsResult => getPathSegmentsResult.Value));
        IEnumerable<ErrorOr<PathSegment>> GetPathSegments()
        {
            if (path.Path.StartsWith(@"\\"))
            {
                // handle UNC path
                yield return PathSegment.Create(@"\\", isDirectory: false, isDrive: true);

                string[] segments = path.Path[2..].Split(new[] { PathSeparator }, StringSplitOptions.RemoveEmptyEntries);
                for (int i = 0; i < segments.Length; i++)
                    yield return CreatePathSegment(segments[i], i, segments.Length);
            }
            else // handle regular Windows path
            {
                // the drive segment
                yield return PathSegment.Create(path.Path[..2], isDirectory: false, isDrive: true);
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
                    yield return PathSegment.Create(segment, isDirectory, isDrive: false);
                }
            }
        }
        ErrorOr<PathSegment> CreatePathSegment(string segment, int index, int totalSegments)
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
    /// <returns>An <see cref="ErrorOr{TValue}"/> containing the path segments of the path up one level from <paramref name="path"/>, or an error.</returns>
    public ErrorOr<IEnumerable<PathSegment>> GoUpOneLevel(FileSystemPathId path)
    {
        // validation: ensure the path is not null or empty
        if (!IsValidPath(path))
            return Errors.FileManagement.InvalidPath;
        // trim trailing backslash for consistent processing
        string tempPath = path.Path.TrimEnd(PathSeparator);
        // check for UNC path
        if (tempPath.StartsWith(@"\\"))
        {
            string[] parts = tempPath.Split(PathSeparator, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length <= 2)
                return Errors.FileManagement.CannotNavigateUp;
            // if it's a single-level UNC path, return the UNC root
            if (parts.Length == 3)
                return ParsePath(FileSystemPathId.Create($@"\\{parts[0]}\{parts[1]}").Value);
        } // check for drive root (both with and without trailing backslash)        
        else if ((tempPath.Length == 2 && tempPath[1] == ':') || (tempPath.Length == 3 && tempPath[1] == ':' && tempPath[2] == PathSeparator))
            return Errors.FileManagement.CannotNavigateUp;
        // find the last occurrence of a backslash
        int lastIndex = tempPath.LastIndexOf(PathSeparator);
        // if there's no backslash found or it's at the root level, return the root
        if (lastIndex <= 2)
            return ParsePath(FileSystemPathId.Create(tempPath[..3]).Value);
        // return the path up to the last backslash
        ErrorOr<FileSystemPathId> newPathResult = FileSystemPathId.Create(tempPath[..lastIndex]);
        if (newPathResult.IsError)
            return newPathResult.Errors;

        return ParsePath(newPathResult.Value);
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
        // handle UNC paths (e.g., \\server\share\folder)
        if (path.Path.StartsWith(@"\\"))
        {
            // find the position of the second backslash (after \\server)
            int secondBackslash = path.Path.IndexOf('\\', 2);
            if (secondBackslash == -1)
                return Errors.FileManagement.InvalidPath; // invalid UNC path, missing server name
            // find the position of the third backslash (after \\server\share)
            int thirdBackslash = path.Path.IndexOf('\\', secondBackslash + 1);
            if (thirdBackslash == -1)
                thirdBackslash = path.Path.Length; // no third backslash, use entire path
            // return the UNC root (\\server\share\)
            return PathSegment.Create(path.Path[..thirdBackslash] + "\\", isDirectory: true, isDrive: false);
        }
        else // handle drive letter paths (e.g., C:\folder)
        { 
            // check if the path starts with a drive letter followed by a colon (e.g., C:)
            // path.Path.Length >= 2: Ensure the path is at least 2 characters long
            // char.IsLetter(path.Path[0]): First character should be a letter
            // path.Path[1] == ':': Second character should be a colon
            if (path.Path.Length >= 2 && char.IsLetter(path.Path[0]) && path.Path[1] == ':')               
                return PathSegment.Create(path.Path[..2] + "\\", isDirectory: true, isDrive: true); // return the drive root (e.g., C:\)
        }
        // if we reach here, the path is neither a valid UNC path nor a valid drive path
        return Errors.FileManagement.InvalidPath;
    }
    #endregion
}
