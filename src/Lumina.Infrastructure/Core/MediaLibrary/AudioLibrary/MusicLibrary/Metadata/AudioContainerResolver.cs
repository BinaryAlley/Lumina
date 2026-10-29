#region ========================================================================= USING =====================================================================================
using System;
#endregion

namespace Lumina.Infrastructure.Core.MediaLibrary.AudioLibrary.MusicLibrary.Metadata;

/// <summary>
/// Resolves the canonical name of the container of an audio file from the concrete type the tagging library parsed it as.
/// </summary>
internal static class AudioContainerResolver
{
    /// <summary>
    /// Resolves the container of the audio file parsed by <paramref name="tagFile"/>.
    /// </summary>
    /// <param name="tagFile">The parsed audio file whose container is resolved.</param>
    /// <returns>The canonical name of the container, for example MP3, FLAC, OGG, AAC, MP4 or WAV, or <see langword="null"/> when the container cannot be recognized.</returns>
    public static string? Resolve(TagLib.File tagFile)
    {
        // The concrete type of the parsed file is the most reliable description of its container, because it is chosen by the parser from the
        // actual bytes of the file, while the mime type only describes the extension. Containers that share a parser (for example the Opus and
        // Vorbis streams parsed by the Ogg parser) are reported under the name of their container, and the codec column carries the difference.
        return tagFile switch
        {
            TagLib.Mpeg.File => "MP3",
            TagLib.Flac.File => "FLAC",
            TagLib.Mpeg4.File => "MP4",
            TagLib.Aac.File => "AAC",
            TagLib.Ogg.File => "OGG",
            TagLib.Riff.File => "WAV",
            TagLib.Aiff.File => "AIFF",
            TagLib.Ape.File => "APE",
            TagLib.WavPack.File => "WAVPACK",
            TagLib.Asf.File => "WMA",
            TagLib.Dsf.File => "DSF",
            TagLib.Matroska.File => "MATROSKA",
            TagLib.MusePack.File => "MPC",
            TagLib.Audible.File => "AA",
            _ => ResolveFromMimeType(tagFile.MimeType)
        };
    }

    /// <summary>
    /// Resolves the container from the mime type reported by the tagging library, for the containers that have no dedicated parser.
    /// </summary>
    /// <param name="mimeType">The mime type reported by the tagging library.</param>
    /// <returns>The name of the container, or <see langword="null"/> when it cannot be derived.</returns>
    public static string? ResolveFromMimeType(string? mimeType)
    {
        if (string.IsNullOrWhiteSpace(mimeType))
            return null;

        // The library reports its mime types with the "taglib/" prefix, so only the last segment names the container.
        string[] segments = mimeType.Split('/', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        string? container = segments.Length > 0 ? segments[^1] : null;
        return string.IsNullOrWhiteSpace(container) ? null : container.ToUpperInvariant();
    }
}
