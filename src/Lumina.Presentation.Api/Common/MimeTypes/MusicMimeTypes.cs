#region ========================================================================= USING =====================================================================================
using Lumina.Domain.SharedKernel.Common.Enums.AudioLibrary;
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

    /// <summary>
    /// Gets the MIME content type of <paramref name="format"/>.
    /// </summary>
    /// <param name="format">The audio file format for which to get the MIME content type.</param>
    /// <returns>The MIME content type of <paramref name="format"/>, or <c>application/octet-stream</c> when the format is unknown.</returns>
    public static string GetMimeType(AudioFileFormat format)
    {
        return s_audioFileFormatToMimeType.TryGetValue(format, out string? mimeType) ? mimeType : "application/octet-stream";
    }
}
