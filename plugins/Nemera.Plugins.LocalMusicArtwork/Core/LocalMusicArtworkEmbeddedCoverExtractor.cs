#region ========================================================================= USING =====================================================================================
using Lumina.Contracts.DTO.Common;
using Lumina.Domain.SharedKernel.Common.Enums.MediaLibrary;
using System;
using System.IO;
using System.Linq;
#endregion

namespace Lumina.Plugins.LocalMusicArtwork.Core;

/// <summary>
/// Extracts the cover image embedded in the metadata of an audio file into a temporary file, so that it can be stored as the artwork of an album.
/// </summary>
internal static class LocalMusicArtworkEmbeddedCoverExtractor
{
    private const int MAX_EMBEDDED_COVER_SIZE_BYTES = 10 * 1024 * 1024; // The maximum size of an embedded cover, matching the size limit the artwork storage enforces.

    /// <summary>
    /// Extracts the first embedded cover image of the audio file stored at <paramref name="trackPath"/> into a temporary file.
    /// </summary>
    /// <param name="trackPath">The file system path of the audio file whose embedded cover is extracted.</param>
    /// <returns>The extracted cover artwork, which points to a temporary file, or <see langword="null"/> when the file carries no usable embedded cover.</returns>
    public static ArtworkDto? Extract(string? trackPath)
    {
        if (string.IsNullOrWhiteSpace(trackPath) || !File.Exists(trackPath))
            return null;

        try
        {
            // A link must not cause the metadata of a file outside the library to be read.
            if (File.GetAttributes(trackPath).HasFlag(FileAttributes.ReparsePoint))
                return null;

            using (TagLib.File tagFile = TagLib.File.Create(trackPath))
            {
                TagLib.IPicture? cover = tagFile.Tag.Pictures?.FirstOrDefault(picture => picture is not null && picture.Data is not null);
                if (cover?.Data is null)
                    return null;

                byte[] imageBytes = cover.Data.Data;
                if (imageBytes is null || imageBytes.Length == 0 || imageBytes.Length > MAX_EMBEDDED_COVER_SIZE_BYTES)
                    return null;

                string temporaryPath = Path.Combine(Path.GetTempPath(), $"lumina-local-music-artwork-{Guid.NewGuid():N}{ResolveImageExtension(cover.MimeType)}");
                File.WriteAllBytes(temporaryPath, imageBytes);
                return new ArtworkDto(ArtworkType.Cover, 0, LocalPath: temporaryPath, RemoteUrl: null, IsTemporary: true);
            }
        }
        catch (Exception exception) when (exception is TagLib.CorruptFileException or TagLib.UnsupportedFormatException or IOException or UnauthorizedAccessException)
        {
            // A file whose embedded cover cannot be read contributes no cover, rather than failing the artwork resolution of the whole album.
            return null;
        }
    }

    /// <summary>
    /// Resolves the file extension of an embedded cover image from its MIME type.
    /// </summary>
    /// <param name="mimeType">The MIME type of the embedded cover image.</param>
    /// <returns>The file extension matching the MIME type, falling back to the JPEG extension.</returns>
    private static string ResolveImageExtension(string? mimeType)
    {
        return mimeType?.Trim().ToLowerInvariant() switch
        {
            "image/png" => ".png",
            "image/webp" => ".webp",
            "image/bmp" or "image/x-ms-bmp" => ".bmp",
            "image/gif" => ".gif",
            "image/tiff" => ".tiff",
            _ => ".jpg"
        };
    }
}
