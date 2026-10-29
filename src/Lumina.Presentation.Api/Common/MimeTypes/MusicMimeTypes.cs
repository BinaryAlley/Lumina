#region ========================================================================= USING =====================================================================================
using Lumina.Domain.SharedKernel.Common.Enums.AudioLibrary;
using System;
using System.Collections.Frozen;
using System.Collections.Generic;
#endregion

namespace Lumina.Presentation.Api.Common.MimeTypes;

/// <summary>
/// Resolves the MIME content type of audio file formats.
/// </summary>
public static class MusicMimeTypes
{
    private static readonly FrozenDictionary<AudioFileFormat, string> s_audioFileFormatToMimeType = new Dictionary<AudioFileFormat, string>
    {
        { AudioFileFormat.MP3, "audio/mpeg" },
        { AudioFileFormat.FLAC, "audio/flac" },
        { AudioFileFormat.M4A, "audio/mp4" },
        { AudioFileFormat.AAC, "audio/aac" },
        { AudioFileFormat.OGG, "audio/ogg" },
        { AudioFileFormat.OPUS, "audio/ogg" },
        { AudioFileFormat.WAV, "audio/wav" },
        { AudioFileFormat.WMA, "audio/x-ms-wma" },
        { AudioFileFormat.AIFF, "audio/aiff" },
        { AudioFileFormat.APE, "audio/x-ape" },
        { AudioFileFormat.WAVPACK, "audio/x-wavpack" },
        { AudioFileFormat.DSF, "audio/x-dsf" },
        { AudioFileFormat.DFF, "audio/x-dff" },
        { AudioFileFormat.MATROSKA, "audio/x-matroska" }
    }.ToFrozenDictionary();

    // Container names are mapped to the file format enumeration instead of to their own media type strings, so the media type table above stays the
    // single source of truth and a new format only needs one entry.
    private static readonly FrozenDictionary<string, AudioFileFormat> s_containerToAudioFileFormat = new Dictionary<string, AudioFileFormat>(StringComparer.OrdinalIgnoreCase)
    {
        { "MP3", AudioFileFormat.MP3 },
        { "FLAC", AudioFileFormat.FLAC },
        { "MP4", AudioFileFormat.M4A },
        { "M4A", AudioFileFormat.M4A },
        { "AAC", AudioFileFormat.AAC },
        { "OGG", AudioFileFormat.OGG },
        { "OPUS", AudioFileFormat.OPUS },
        { "WAV", AudioFileFormat.WAV }
    }.ToFrozenDictionary(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Gets the MIME content type of <paramref name="format"/>.
    /// </summary>
    /// <param name="format">The audio file format for which to get the MIME content type.</param>
    /// <returns>The MIME content type of <paramref name="format"/>, or <c>application/octet-stream</c> when the format is unknown.</returns>
    public static string GetMimeType(AudioFileFormat format)
    {
        return s_audioFileFormatToMimeType.TryGetValue(format, out string? mimeType) ? mimeType : "application/octet-stream";
    }

    /// <summary>
    /// Gets the MIME content type of an audio container, as named by the delivery decision of a playback manifest.
    /// </summary>
    /// <param name="container">The canonical container of the delivered audio, for example MP4, AAC, OGG, MP3, FLAC or WAV.</param>
    /// <returns>The MIME content type of <paramref name="container"/>, or <c>application/octet-stream</c> when the container is unknown.</returns>
    public static string GetContainerMimeType(string? container)
    {
        return !string.IsNullOrWhiteSpace(container) && s_containerToAudioFileFormat.TryGetValue(container.Trim(), out AudioFileFormat format)
            ? GetMimeType(format)
            : "application/octet-stream";
    }
}
