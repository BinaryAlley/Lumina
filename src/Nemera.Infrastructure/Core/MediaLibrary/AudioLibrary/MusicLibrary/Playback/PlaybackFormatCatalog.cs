#region ========================================================================= USING =====================================================================================
using Lumina.Infrastructure.Common.Models.DTO.MediaLibrary.AudioLibrary.MusicLibrary.Playback;
using System;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Linq;
#endregion

namespace Lumina.Infrastructure.Core.MediaLibrary.AudioLibrary.MusicLibrary.Playback;

/// <summary>
/// The single catalog of the audio formats the server can produce, shared by the playback delivery decision and the audio normalization pipeline.
/// </summary>
/// <remarks>
/// The order of <see cref="Formats"/> is significant: the first appearance of a codec is its transcode preference, and the order of the containers of a
/// codec is the order in which the server prefers to produce it. Both the delivery decision and the normalization pipeline derive their tables from this
/// catalog, so a format can never be promised in a manifest without also being implementable by the pipeline.
/// </remarks>
internal static class PlaybackFormatCatalog
{
    /// <summary>
    /// Gets the formats the server can produce, in the order of the transcode codec preference and, within a codec, of the container preference.
    /// </summary>
    public static IReadOnlyList<NormalizedAudioFormatDto> Formats { get; } =
    [
        new("MP4", "AAC", "mp4", ".m4a", "aac", false),
        new("AAC", "AAC", "adts", ".aac", "aac", false),
        new("OGG", "OPUS", "ogg", ".ogg", "libopus", false),
        new("MP3", "MP3", "mp3", ".mp3", "libmp3lame", false),
        new("OGG", "VORBIS", "ogg", ".ogg", "libvorbis", false),
        new("FLAC", "FLAC", "flac", ".flac", "flac", true),
        new("WAV", "PCM", "wav", ".wav", "pcm_s16le", true)
    ];

    /// <summary>
    /// Gets the formats the server can produce, keyed by their canonical container and codec pair.
    /// </summary>
    public static FrozenDictionary<string, NormalizedAudioFormatDto> FormatsByPair { get; } = BuildFormatsByPair();

    /// <summary>
    /// Gets the containers the server can produce each codec in, in order of preference.
    /// </summary>
    public static FrozenDictionary<string, IReadOnlyList<string>> ProducibleContainersByCodec { get; } = BuildProducibleContainersByCodec();

    /// <summary>
    /// Gets the codecs the server can produce, in order of preference.
    /// </summary>
    public static IReadOnlyList<string> TranscodeCodecPreference { get; } = BuildTranscodeCodecPreference();

    /// <summary>
    /// Builds the key of a canonical container and codec pair.
    /// </summary>
    /// <param name="container">The container of the audio.</param>
    /// <param name="codec">The codec of the audio.</param>
    /// <returns>The canonical key of the pair.</returns>
    public static string BuildPairKey(string container, string codec)
    {
        return (container ?? string.Empty).Trim().ToUpperInvariant() + "/" + (codec ?? string.Empty).Trim().ToUpperInvariant();
    }

    /// <summary>
    /// Builds the map of the producible formats, keyed by their canonical container and codec pair.
    /// </summary>
    /// <returns>The producible formats, keyed by their canonical pair.</returns>
    private static FrozenDictionary<string, NormalizedAudioFormatDto> BuildFormatsByPair()
    {
        return Formats.ToFrozenDictionary(format => BuildPairKey(format.Container, format.Codec), StringComparer.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Builds the map of the containers the server can produce each codec in, preserving the order of the catalog.
    /// </summary>
    /// <returns>The containers of each producible codec, in order of preference.</returns>
    private static FrozenDictionary<string, IReadOnlyList<string>> BuildProducibleContainersByCodec()
    {
        Dictionary<string, List<string>> containersByCodec = new(StringComparer.OrdinalIgnoreCase);
        foreach (NormalizedAudioFormatDto format in Formats)
        {
            if (!containersByCodec.TryGetValue(format.Codec, out List<string>? containers))
            {
                containers = [];
                containersByCodec[format.Codec] = containers;
            }
            if (!containers.Contains(format.Container, StringComparer.OrdinalIgnoreCase))
                containers.Add(format.Container);
        }
        return containersByCodec.ToFrozenDictionary(entry => entry.Key, entry => (IReadOnlyList<string>)entry.Value, StringComparer.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Builds the codecs the server can produce, preserving the order in which they first appear in the catalog.
    /// </summary>
    /// <returns>The producible codecs, in order of preference.</returns>
    private static IReadOnlyList<string> BuildTranscodeCodecPreference()
    {
        List<string> codecs = [];
        foreach (NormalizedAudioFormatDto format in Formats)
            if (!codecs.Contains(format.Codec, StringComparer.OrdinalIgnoreCase))
                codecs.Add(format.Codec);
        return codecs;
    }
}
