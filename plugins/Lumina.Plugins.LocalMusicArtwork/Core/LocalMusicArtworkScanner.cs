#region ========================================================================= USING =====================================================================================
using Lumina.Contracts.DTO.Common;
using Lumina.Domain.SharedKernel.Common.Enums.MediaLibrary;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
#endregion

namespace Lumina.Plugins.LocalMusicArtwork.Core;

/// <summary>
/// Scans the folders of a local music library and classifies the images they contain into artwork DTOs, following the naming
/// conventions that music media servers use for the images of an artist and of an album.
/// The file names are matched case sensitively, so that a library that stores them with a different casing is left alone.
/// </summary>
internal static class LocalMusicArtworkScanner
{
    private const int MAX_ALBUM_ANCESTOR_LEVELS = 2; // The number of directories, the one of a track and its parent, that are inspected when locating the folder of an album.
    private const int MAX_ARTIST_ANCESTOR_LEVELS = 5; // The number of parent directories that are inspected when locating the folder of an artist.

    private static readonly HashSet<string> s_imageExtensions = new([".jpg", ".jpeg", ".png", ".webp", ".bmp", ".gif", ".tif", ".tiff"], StringComparer.Ordinal);

    /// <summary>
    /// Scans the directory of the provided track, and its parent directory, for the artwork of an album.
    /// </summary>
    /// <param name="trackPath">The file system path of a track of the album.</param>
    /// <returns>The artwork of the album, or an empty collection when no artwork was found.</returns>
    public static IReadOnlyList<ArtworkDto> ScanAlbumArtwork(string? trackPath)
    {
        string? trackDirectory = GetDirectoryOrNull(trackPath);
        if (trackDirectory is null)
            return [];

        // A multi disc album nests its tracks in disc subdirectories, so both the track directory and its parent are inspected, the nearest one taking precedence.
        List<ArtworkDto> artworks = [];
        HashSet<(ArtworkType Type, int Ordinal)> seenArtworks = [];
        DirectoryInfo? currentDirectory = new(trackDirectory);
        int level = 0;
        while (currentDirectory is not null && level < MAX_ALBUM_ANCESTOR_LEVELS)
        {
            foreach (ArtworkDto artwork in ClassifyImages(currentDirectory, isArtist: false))
                if (seenArtworks.Add((artwork.Type, artwork.Ordinal)))
                    artworks.Add(artwork);
            currentDirectory = currentDirectory.Parent;
            level++;
        }
        return artworks;
    }

    /// <summary>
    /// Scans the ancestor directories of the provided track for the artwork of an artist.
    /// </summary>
    /// <param name="trackPath">The file system path of a track of the artist.</param>
    /// <returns>The artwork of the artist, or an empty collection when no artwork was found.</returns>
    public static IReadOnlyList<ArtworkDto> ScanArtistArtwork(string? trackPath)
    {
        string? trackDirectory = GetDirectoryOrNull(trackPath);
        if (trackDirectory is null)
            return [];

        // The artist directory is the nearest ancestor that carries an artist level image, like a folder, a backdrop or a logo.
        DirectoryInfo? currentDirectory = new(trackDirectory);
        int level = 0;
        while (currentDirectory is not null && level < MAX_ARTIST_ANCESTOR_LEVELS)
        {
            IReadOnlyList<ArtworkDto> artworks = ClassifyImages(currentDirectory, isArtist: true);
            if (artworks.Count > 0)
                return artworks;
            currentDirectory = currentDirectory.Parent;
            level++;
        }
        return [];
    }

    /// <summary>
    /// Classifies the images directly contained in <paramref name="directory"/>.
    /// </summary>
    /// <param name="directory">The directory whose images are classified.</param>
    /// <param name="isArtist">Whether the images belong to an artist, as opposed to an album.</param>
    /// <returns>The classified artwork, one for every recognized file, ordered by type and ordinal.</returns>
    private static IReadOnlyList<ArtworkDto> ClassifyImages(DirectoryInfo directory, bool isArtist)
    {
        FileInfo[] files;
        try
        {
            files = [.. directory.EnumerateFiles()];
        }
        catch (IOException)
        {
            return [];
        }
        catch (UnauthorizedAccessException)
        {
            return [];
        }

        List<ClassifiedArtwork> candidates = [];
        foreach (FileInfo file in files)
        {
            if (!s_imageExtensions.Contains(file.Extension))
                continue;

            (string baseName, int ordinal) = SplitStem(Path.GetFileNameWithoutExtension(file.Name));
            bool isRecognized = isArtist
                ? TryMapArtistName(baseName, out ArtworkType artworkType, out int priority)
                : TryMapAlbumName(baseName, out artworkType, out priority);
            if (!isRecognized)
                continue;
            candidates.Add(new ClassifiedArtwork(artworkType, ordinal, priority, file.Name, file.FullName));
        }

        // Multiple file names can describe the same piece of artwork, like a cover and a front image, so only the preferred one is kept per type and ordinal.
        List<ArtworkDto> artworks = [];
        HashSet<(ArtworkType Type, int Ordinal)> seenArtworks = [];
        foreach (ClassifiedArtwork candidate in candidates
            .OrderBy(candidate => candidate.Type)
            .ThenBy(candidate => candidate.Ordinal)
            .ThenBy(candidate => candidate.Priority)
            .ThenBy(candidate => candidate.FileName, StringComparer.Ordinal))
        {
            if (!seenArtworks.Add((candidate.Type, candidate.Ordinal)))
                continue;
            artworks.Add(new ArtworkDto(candidate.Type, candidate.Ordinal, LocalPath: candidate.FilePath, RemoteUrl: null));
        }
        return artworks;
    }

    /// <summary>
    /// Splits the stem of an image file into the base name that identifies the artwork and the ordinal that orders it within its type.
    /// </summary>
    /// <param name="stem">The stem of the image file, without its extension.</param>
    /// <returns>The base name of the artwork and the ordinal it carries, which is zero when the stem carries no number.</returns>
    private static (string BaseName, int Ordinal) SplitStem(string stem)
    {
        int index = stem.Length;
        while (index > 0 && char.IsAsciiDigit(stem[index - 1]))
            index--;
        string ordinalPart = stem[index..];
        string baseName = stem[..index].TrimEnd(' ', '-', '_');
        int ordinal = ordinalPart.Length > 0 && int.TryParse(ordinalPart, out int parsedOrdinal) ? parsedOrdinal : 0;
        return (baseName, ordinal);
    }

    /// <summary>
    /// Maps the base name of an artist image onto an artwork type.
    /// </summary>
    /// <param name="baseName">The base name of the image file.</param>
    /// <param name="artworkType">The mapped artwork type, when the base name is recognized.</param>
    /// <param name="priority">The priority of the base name among the file names of the same artwork type and ordinal, lower being preferred.</param>
    /// <returns><see langword="true"/> when the base name was recognized, otherwise <see langword="false"/>.</returns>
    private static bool TryMapArtistName(string baseName, out ArtworkType artworkType, out int priority)
    {
        priority = 0;
        switch (baseName)
        {
            case "folder":
                artworkType = ArtworkType.Cover;
                return true;
            case "backdrop":
                artworkType = ArtworkType.Backdrop;
                return true;
            case "banner":
                artworkType = ArtworkType.Banner;
                return true;
            case "logo":
                artworkType = ArtworkType.Logo;
                return true;
            case "thumb":
                artworkType = ArtworkType.Thumb;
                return true;
            default:
                artworkType = ArtworkType.Other;
                priority = int.MaxValue;
                return false;
        }
    }

    /// <summary>
    /// Maps the base name of an album image onto an artwork type.
    /// </summary>
    /// <param name="baseName">The base name of the image file.</param>
    /// <param name="artworkType">The mapped artwork type, when the base name is recognized.</param>
    /// <param name="priority">The priority of the base name among the file names of the same artwork type and ordinal, lower being preferred.</param>
    /// <returns><see langword="true"/> when the base name was recognized, otherwise <see langword="false"/>.</returns>
    private static bool TryMapAlbumName(string baseName, out ArtworkType artworkType, out int priority)
    {
        switch (baseName)
        {
            case "cover":
                artworkType = ArtworkType.Cover;
                priority = 0;
                return true;
            case "front":
                artworkType = ArtworkType.Front;
                priority = 0;
                return true;
            case "back":
                artworkType = ArtworkType.Back;
                priority = 0;
                return true;
            case "booklet":
                artworkType = ArtworkType.Booklet;
                priority = 0;
                return true;
            case "disk":
            case "disc":
                artworkType = ArtworkType.Medium;
                priority = 0;
                return true;
            case "liner":
                artworkType = ArtworkType.Liner;
                priority = 0;
                return true;
            case "tray":
                artworkType = ArtworkType.Tray;
                priority = 0;
                return true;
            case "spine":
                artworkType = ArtworkType.Spine;
                priority = 0;
                return true;
            case "obi":
                artworkType = ArtworkType.Obi;
                priority = 0;
                return true;
            case "sticker":
                artworkType = ArtworkType.Sticker;
                priority = 0;
                return true;
            case "poster":
                artworkType = ArtworkType.Poster;
                priority = 0;
                return true;
            default:
                artworkType = ArtworkType.Other;
                priority = int.MaxValue;
                return false;
        }
    }

    /// <summary>
    /// Gets the directory of the provided <paramref name="path"/>.
    /// </summary>
    /// <param name="path">The file system path whose directory is retrieved.</param>
    /// <returns>The directory of the path, or <see langword="null"/> when the path is empty or has no directory part.</returns>
    private static string? GetDirectoryOrNull(string? path)
    {
        if (string.IsNullOrWhiteSpace(path))
            return null;
        string? directory = Path.GetDirectoryName(path);
        return string.IsNullOrWhiteSpace(directory) ? null : directory;
    }

    /// <summary>
    /// A recognized image of an artist or an album, before it is ordered and de-duplicated.
    /// </summary>
    /// <param name="Type">The type of the artwork.</param>
    /// <param name="Ordinal">The ordinal of the artwork within its type.</param>
    /// <param name="Priority">The priority of the file name among the files of the same artwork type and ordinal, lower being preferred.</param>
    /// <param name="FileName">The name of the image file.</param>
    /// <param name="FilePath">The file system path of the image file.</param>
    private sealed record ClassifiedArtwork(
        ArtworkType Type,
        int Ordinal,
        int Priority,
        string FileName,
        string FilePath
    );
}
