﻿#region ========================================================================= USING =====================================================================================
using ErrorOr;
using Lumina.Domain.Common.Errors;
using Lumina.Domain.Core.Aggregates.FileManagement.FileManagementAggregate.ValueObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
#endregion

namespace Lumina.Domain.Core.Aggregates.FileManagement.FileManagementAggregate.Strategies.Path;

/// <summary>
/// Service defining methods for handling path-related operations on UNIX platforms.
/// </summary>
public class UnixPathStrategy : IUnixPathStrategy
{
    #region ==================================================================== PROPERTIES =================================================================================
    public char PathSeparator
    {
        get { return '/'; }
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
        var invalidChars = GetInvalidPathCharsForPlatform();
        if (path.Path.IndexOfAny(invalidChars) >= 0)
            return false;
        // check for relative paths
        if (path.Path.StartsWith("./") || path.Path.StartsWith("../"))
            return false;
        var pathPattern = @"^\/([\w\-\.\~!$&'()*+,;=:@ ]+(\/[\w\-\.\~!$&'()*+,;=:@ ]+)*)?\/?$";
        return Regex.IsMatch(path.Path, pathPattern);
    }

    /// <summary>
    /// Tries to combine <paramref name="path"/> with <paramref name="name"/>.
    /// </summary>
    /// <param name="path">The path to be combined.</param>
    /// <param name="name">The name to be combined with the path.</param>
    /// <returns>An <see cref="ErrorOr{TValue}"/> containing the combined path, or an error.</returns>
    public ErrorOr<FileSystemPathId> CombinePath(FileSystemPathId path, string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return Errors.FileManagement.NameCannotBeEmpty;
        // trim any directory separator characters from the end of the path
        string subpath = path.Path.TrimEnd(PathSeparator);
        // if the name begins with a directory separator, remove it
        name = name.TrimStart(PathSeparator);
        // combine the two parts with the Unix directory separator character
        return FileSystemPathId.Create(subpath + PathSeparator + name);
    }

    /// <summary>
    /// Parses <paramref name="path"/> into path segments.
    /// </summary>
    /// <param name="path">The path to be parsed.</param>
    /// <returns>An <see cref="ErrorOr{TValue}"/> containing the path segments, or an error.</returns>
    public ErrorOr<IEnumerable<PathSegment>> ParsePath(FileSystemPathId path)
    {
        // if path starts with anything other than '/', it's considered relative and invalid for this parser
        if (!path.Path.StartsWith(PathSeparator))
            return Errors.FileManagement.InvalidPath;
        // get the path segments
        List<string> splitSegments = [.. path.Path.Split(new[] { PathSeparator }, StringSplitOptions.RemoveEmptyEntries)];
        IEnumerable<PathSegment> segments = splitSegments.Select((segment, index) =>
        {
            bool isDirectory;
            if (segment.Contains('.'))
                isDirectory = index != splitSegments.Count - 1 || path.Path.EndsWith(PathSeparator); // check if it's the last segment or if the path ends with a '/'
            else
                isDirectory = true;
            return new PathSegment(segment, isDirectory, isDrive: false);
        }).Prepend(new PathSegment(PathSeparator.ToString(), isDirectory: false, isDrive: true)); // UNIX paths have '/' as "root drive"
        return ErrorOrFactory.From(segments);
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
        // trim trailing slash for consistent processing
        string tempPath = path.Path;
        if (tempPath.EndsWith("/"))
            tempPath = tempPath.TrimEnd(PathSeparator);
        // find the last occurrence of a slash
        var lastIndex = tempPath.LastIndexOf(PathSeparator);
        // if there's no slash found (shouldn't happen due to previous steps), or if we are at the root level after trimming, return error
        if (lastIndex < 0)
            return Errors.FileManagement.CannotNavigateUp;
        // return the path up to the last slash, or, if there's only the root slash, return that one instead
        ErrorOr<FileSystemPathId> newPathResult = FileSystemPathId.Create(lastIndex > 0 ? tempPath[..lastIndex] : tempPath[..1]);
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
        return ['\0'];
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

        // On Unix-like systems, the root is always "/"
        if (path.Path.StartsWith(PathSeparator))
            return new PathSegment(PathSeparator.ToString(), isDirectory: false, isDrive: true);
        return Errors.FileManagement.InvalidPath;
    }
    #endregion
}