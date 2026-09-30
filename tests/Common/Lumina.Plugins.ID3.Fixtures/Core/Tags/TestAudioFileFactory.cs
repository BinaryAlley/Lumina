#region ========================================================================= USING =====================================================================================
using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Text;
#endregion

namespace Lumina.Plugins.ID3.Fixtures.Core.Tags;

/// <summary>
/// Builds minimal, structurally valid WAV files used as test input for the ID3 tag reading tests.
/// The files are produced into a temporary directory at test runtime, because a WAV is a binary container whose
/// embedded tags can only be written by the tagging library itself.
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
        File.WriteAllBytes(path, BuildWav(durationMs));
    }

    /// <summary>
    /// Creates a PCM WAV file whose embedded tags are configured by the provided callback.
    /// </summary>
    /// <param name="path">The path where the WAV is written.</param>
    /// <param name="configureTags">The callback that configures the tags of the file.</param>
    /// <param name="durationMs">The duration of the audio of the file, in milliseconds.</param>
    public static void CreateTaggedWav(string path, Action<TagLib.File> configureTags, int durationMs = 200)
    {
        File.WriteAllBytes(path, BuildWav(durationMs));
        using (TagLib.File tagFile = TagLib.File.Create(path))
        {
            configureTags(tagFile);
            tagFile.Save();
        }
    }

    /// <summary>
    /// Adds a user defined text information frame (TXXX) to the ID3v2 tag of the file.
    /// </summary>
    /// <param name="tagFile">The file whose ID3v2 tag is extended.</param>
    /// <param name="description">The description that identifies the frame.</param>
    /// <param name="values">The values of the frame.</param>
    public static void AddUserTextFrame(TagLib.File tagFile, string description, params string[] values)
    {
        GetId3v2Tag(tagFile).AddFrame(new TagLib.Id3v2.UserTextInformationFrame(description) { Text = values });
    }

    /// <summary>
    /// Adds a text information frame to the ID3v2 tag of the file.
    /// </summary>
    /// <param name="tagFile">The file whose ID3v2 tag is extended.</param>
    /// <param name="frameId">The four character identifier of the frame.</param>
    /// <param name="values">The values of the frame.</param>
    public static void AddTextFrame(TagLib.File tagFile, string frameId, params string[] values)
    {
        GetId3v2Tag(tagFile).AddFrame(new TagLib.Id3v2.TextInformationFrame(frameId) { Text = values });
    }

    /// <summary>
    /// Adds a URL link frame (WXXX/WOAR) to the ID3v2 tag of the file.
    /// </summary>
    /// <param name="tagFile">The file whose ID3v2 tag is extended.</param>
    /// <param name="frameId">The four character identifier of the frame.</param>
    /// <param name="values">The URL values of the frame.</param>
    public static void AddUrlFrame(TagLib.File tagFile, string frameId, params string[] values)
    {
        GetId3v2Tag(tagFile).AddFrame(new TagLib.Id3v2.UrlLinkFrame(frameId) { Text = values });
    }

    /// <summary>
    /// Gets the ID3v2 tag of the file, creating it when the file does not carry one yet.
    /// </summary>
    /// <param name="tagFile">The file whose ID3v2 tag is read.</param>
    /// <returns>The ID3v2 tag of the file.</returns>
    private static TagLib.Id3v2.Tag GetId3v2Tag(TagLib.File tagFile)
    {
        return tagFile.GetTag(TagLib.TagTypes.Id3v2, true) as TagLib.Id3v2.Tag
            ?? throw new InvalidOperationException("The test audio file does not expose an ID3v2 tag.");
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
