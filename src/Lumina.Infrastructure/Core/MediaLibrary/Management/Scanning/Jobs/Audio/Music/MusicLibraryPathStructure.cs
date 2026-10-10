#region ========================================================================= USING =====================================================================================
using Lumina.Domain.Core.BoundedContexts.FileSystemManagementBoundedContext.FileSystemManagementAggregate.Services;
using Lumina.Domain.SharedKernel.Common.Enums.AudioLibrary;
using System;
using System.Text.RegularExpressions;
#endregion

namespace Lumina.Infrastructure.Core.MediaLibrary.Management.Scanning.Jobs.Audio.Music;

/// <summary>
/// Derives the structure of a music library on disk from the file system path of a music file.
/// </summary>
internal sealed class MusicLibraryPathStructure
{
    /// <summary>
    /// Matches the name of a directory that holds one disc of a multi-disc album, like "Disc 1" or "CD2".
    /// </summary>
    private static readonly Regex s_discDirectoryPattern = new(@"^(?:disk|disc|cd)\s*\d+$", RegexOptions.IgnoreCase | RegexOptions.Compiled);

    private readonly IPathService _pathService;

    /// <summary>
    /// Initializes a new instance of the <see cref="MusicLibraryPathStructure"/> class.
    /// </summary>
    /// <param name="pathService">Injected service used to determine the path separator of the current platform.</param>
    public MusicLibraryPathStructure(IPathService pathService)
    {
        _pathService = pathService;
    }

    /// <summary>
    /// Gets the on-disk directory of the album the track stored at <paramref name="trackPath"/> belongs to.
    /// A multi-disc album nests its tracks in disc subdirectories, so the directory of a disc is skipped in favor of its parent.
    /// </summary>
    /// <param name="trackPath">The file system path of the track whose album directory is retrieved.</param>
    /// <returns>The file system path of the album directory, or the directory of the track when it cannot be resolved.</returns>
    public string GetAlbumDirectory(string trackPath)
    {
        if (string.IsNullOrEmpty(trackPath))
            return trackPath;

        string? trackDirectory = GetParentDirectoryPath(trackPath);
        if (string.IsNullOrEmpty(trackDirectory))
            return trackPath;

        if (!s_discDirectoryPattern.IsMatch(GetLastSegment(trackDirectory)))
            return trackDirectory;

        string? albumDirectory = GetParentDirectoryPath(trackDirectory);
        return string.IsNullOrEmpty(albumDirectory) ? trackDirectory : albumDirectory;
    }

    /// <summary>
    /// Gets the name of the on-disk release type directory of the track stored at <paramref name="trackPath"/>, when the structure of the library
    /// uses one. The directory is the parent of the album directory, and it is only used when its name is a known release type, so that the artist
    /// directory of a flat library is not mistaken for a release type.
    /// </summary>
    /// <param name="trackPath">The file system path of the track whose release type directory is retrieved.</param>
    /// <returns>The name of the on-disk release type directory, or <see langword="null"/> when the library does not use one.</returns>
    public string? GetReleaseTypeDirectoryName(string trackPath)
    {
        string albumDirectory = GetAlbumDirectory(trackPath);
        string? releaseTypeDirectory = GetParentDirectoryPath(albumDirectory);
        if (string.IsNullOrEmpty(releaseTypeDirectory))
            return null;

        string releaseTypeName = GetLastSegment(releaseTypeDirectory);
        return Enum.TryParse(releaseTypeName, ignoreCase: true, out MusicReleaseType _) ? releaseTypeName : null;
    }

    /// <summary>
    /// Gets the parent directory of the file or directory stored at <paramref name="path"/>, using the path separator of the current platform.
    /// </summary>
    /// <param name="path">The path whose parent directory is retrieved.</param>
    /// <returns>The parent directory of the path, or <see langword="null"/> when the path has no parent directory.</returns>
    private string? GetParentDirectoryPath(string path)
    {
        string trimmedPath = path.TrimEnd(_pathService.PathSeparator);
        int lastSeparatorIndex = trimmedPath.LastIndexOf(_pathService.PathSeparator);
        if (lastSeparatorIndex <= 0)
            return null;
        return trimmedPath[..lastSeparatorIndex];
    }

    /// <summary>
    /// Gets the last segment, the file or directory name, of <paramref name="path"/>, using the path separator of the current platform.
    /// </summary>
    /// <param name="path">The path whose last segment is retrieved.</param>
    /// <returns>The last segment of the path.</returns>
    private string GetLastSegment(string path)
    {
        string trimmedPath = path.TrimEnd(_pathService.PathSeparator);
        int lastSeparatorIndex = trimmedPath.LastIndexOf(_pathService.PathSeparator);
        return lastSeparatorIndex < 0 ? trimmedPath : trimmedPath[(lastSeparatorIndex + 1)..];
    }
}
