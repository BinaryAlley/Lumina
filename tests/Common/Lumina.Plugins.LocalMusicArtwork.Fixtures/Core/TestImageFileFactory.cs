#region ========================================================================= USING =====================================================================================
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Text;
#endregion

namespace Lumina.Plugins.LocalMusicArtwork.Fixtures.Core;

/// <summary>
/// Creates image files of the formats a local music library stores next to its tracks, so that artwork scanning can be exercised against real files.
/// Each file carries the magic bytes of its format, mirroring the images a music library keeps in the folders of its artists and albums.
/// </summary>
[ExcludeFromCodeCoverage]
public static class TestImageFileFactory
{
    /// <summary>
    /// Creates a JPEG image file at the provided path.
    /// </summary>
    /// <param name="path">The path where the image is written.</param>
    public static void CreateJpeg(string path)
    {
        WriteFile(path, [0xFF, 0xD8, 0xFF, 0xE0, 0x00, 0x10, 0x4A, 0x46, 0x49, 0x46, 0x00, 0x01, 0x01, 0x00, 0x00, 0x01, 0x00, 0x01, 0x00, 0x00, 0xFF, 0xD9]);
    }

    /// <summary>
    /// Creates a PNG image file at the provided path.
    /// </summary>
    /// <param name="path">The path where the image is written.</param>
    public static void CreatePng(string path)
    {
        WriteFile(path, [0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A, 0x00, 0x00, 0x00, 0x0D, 0x49, 0x48, 0x44, 0x52]);
    }

    /// <summary>
    /// Creates a WebP image file at the provided path.
    /// </summary>
    /// <param name="path">The path where the image is written.</param>
    public static void CreateWebp(string path)
    {
        WriteFile(path, [.. Encoding.ASCII.GetBytes("RIFF"), 0x00, 0x00, 0x00, 0x00, .. Encoding.ASCII.GetBytes("WEBP")]);
    }

    /// <summary>
    /// Creates a BMP image file at the provided path.
    /// </summary>
    /// <param name="path">The path where the image is written.</param>
    public static void CreateBmp(string path)
    {
        WriteFile(path, [0x42, 0x4D, 0x00, 0x00, 0x00, 0x00]);
    }

    /// <summary>
    /// Creates a GIF image file at the provided path.
    /// </summary>
    /// <param name="path">The path where the image is written.</param>
    public static void CreateGif(string path)
    {
        WriteFile(path, [.. Encoding.ASCII.GetBytes("GIF89a")]);
    }

    /// <summary>
    /// Creates a TIFF image file at the provided path.
    /// </summary>
    /// <param name="path">The path where the image is written.</param>
    public static void CreateTiff(string path)
    {
        WriteFile(path, [0x49, 0x49, 0x2A, 0x00]);
    }

    /// <summary>
    /// Creates a file whose content is not an image, so that the exclusion of non image files can be exercised.
    /// </summary>
    /// <param name="path">The path where the file is written.</param>
    public static void CreateTextFile(string path)
    {
        WriteFile(path, Encoding.UTF8.GetBytes("This is not an image."));
    }

    /// <summary>
    /// Writes the provided bytes at <paramref name="path"/>, creating the containing directory when it does not exist yet.
    /// </summary>
    /// <param name="path">The path where the bytes are written.</param>
    /// <param name="contents">The bytes of the file.</param>
    private static void WriteFile(string path, byte[] contents)
    {
        string? directory = Path.GetDirectoryName(path);
        if (!string.IsNullOrEmpty(directory))
            Directory.CreateDirectory(directory);
        File.WriteAllBytes(path, contents);
    }
}
