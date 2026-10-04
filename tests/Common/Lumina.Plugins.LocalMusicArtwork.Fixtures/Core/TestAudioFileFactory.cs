#region ========================================================================= USING =====================================================================================
using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Text;
#endregion

namespace Lumina.Plugins.LocalMusicArtwork.Fixtures.Core;

/// <summary>
/// Builds minimal, structurally valid WAV files used as test input for the artwork scanning and embedded cover extraction tests.
/// The files are produced into a temporary directory at test runtime, because a WAV is a binary container whose embedded pictures can only be written by the tagging library itself.
/// </summary>
[ExcludeFromCodeCoverage]
public static class TestAudioFileFactory
{
    /// <summary>
    /// Creates a minimal, untagged PCM WAV file.
    /// </summary>
    /// <param name="path">The path where the WAV is written.</param>
    /// <param name="durationMs">The duration of the audio of the file, in milliseconds.</param>
    public static void CreateWav(string path, int durationMs = 200)
    {
        WriteFile(path, BuildWav(durationMs));
    }

    /// <summary>
    /// Creates a PCM WAV file that carries the provided image as its embedded front cover.
    /// </summary>
    /// <param name="path">The path where the WAV is written.</param>
    /// <param name="imageBytes">The bytes of the embedded cover image.</param>
    /// <param name="mimeType">The MIME type of the embedded cover image, defaulting to JPEG.</param>
    /// <param name="durationMs">The duration of the audio of the file, in milliseconds.</param>
    public static void CreateWavWithEmbeddedCover(string path, byte[] imageBytes, string? mimeType = "image/jpeg", int durationMs = 200)
    {
        CreateTaggedWav(path, tagFile =>
        {
            tagFile.Tag.Pictures =
            [
                new TagLib.Picture
                {
                    MimeType = mimeType!,
                    Type = TagLib.PictureType.FrontCover,
                    Data = [.. imageBytes]
                }
            ];
        }, durationMs);
    }

    /// <summary>
    /// Creates a PCM WAV file whose embedded tags are configured by the provided callback.
    /// </summary>
    /// <param name="path">The path where the WAV is written.</param>
    /// <param name="configureTags">The callback that configures the tags of the file.</param>
    /// <param name="durationMs">The duration of the audio of the file, in milliseconds.</param>
    public static void CreateTaggedWav(string path, Action<TagLib.File> configureTags, int durationMs = 200)
    {
        WriteFile(path, BuildWav(durationMs));
        using (TagLib.File tagFile = TagLib.File.Create(path))
        {
            configureTags(tagFile);
            tagFile.Save();
        }
    }

    /// <summary>
    /// Creates a file whose content is not a recognizable audio container, so that the resilience of the embedded cover extraction can be exercised.
    /// </summary>
    /// <param name="path">The path where the file is written.</param>
    public static void CreateUnsupportedFile(string path)
    {
        WriteFile(path, Encoding.UTF8.GetBytes("This is not an audio file."));
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

    /// <summary>
    /// Builds the bytes of a minimal, valid PCM WAV file with a single silent audio channel.
    /// </summary>
    /// <param name="durationMs">The duration of the audio of the file, in milliseconds.</param>
    /// <returns>The bytes of the WAV file.</returns>
    private static byte[] BuildWav(int durationMs)
    {
        const int SAMPLE_RATE = 44100;
        const short CHANNELS = 1;
        const short BITS_PER_SAMPLE = 16;

        int bytesPerSample = BITS_PER_SAMPLE / 8;
        int dataSize = SAMPLE_RATE * CHANNELS * bytesPerSample * durationMs / 1000;

        using (MemoryStream memoryStream = new())
        {
            using (BinaryWriter writer = new(memoryStream))
            {
                writer.Write(Encoding.ASCII.GetBytes("RIFF"));
                writer.Write(36 + dataSize);
                writer.Write(Encoding.ASCII.GetBytes("WAVE"));
                writer.Write(Encoding.ASCII.GetBytes("fmt "));
                writer.Write(16);
                writer.Write((short)1);
                writer.Write(CHANNELS);
                writer.Write(SAMPLE_RATE);
                writer.Write(SAMPLE_RATE * CHANNELS * bytesPerSample);
                writer.Write((short)(CHANNELS * bytesPerSample));
                writer.Write(BITS_PER_SAMPLE);
                writer.Write(Encoding.ASCII.GetBytes("data"));
                writer.Write(dataSize);
                writer.Write(new byte[dataSize]);
            }
            return memoryStream.ToArray();
        }
    }
}
