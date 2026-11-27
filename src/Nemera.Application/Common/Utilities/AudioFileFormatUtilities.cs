#region ========================================================================= USING =====================================================================================
using Lumina.Domain.SharedKernel.Common.Enums.AudioLibrary;
using System;
using System.IO;
#endregion

namespace Lumina.Application.Common.Utilities;

/// <summary>
/// Provides helpers for the <see cref="AudioFileFormat"/> enumeration.
/// </summary>
public static class AudioFileFormatUtilities
{
    /// <summary>
    /// Gets the audio file format of the file stored at the provided <paramref name="filePath"/>, derived from its file extension.
    /// </summary>
    /// <param name="filePath">The path of the audio file whose file format is resolved.</param>
    /// <returns>The audio file format of the file, or <see cref="AudioFileFormat.None"/> when the extension is not a recognized audio extension.</returns>
    public static AudioFileFormat ToAudioFileFormat(this string filePath)
    {
        return Path.GetExtension(filePath).ToLowerInvariant() switch
        {
            ".mp3" => AudioFileFormat.MP3,
            ".flac" => AudioFileFormat.FLAC,
            ".m4a" => AudioFileFormat.M4A,
            ".aac" => AudioFileFormat.AAC,
            ".ogg" => AudioFileFormat.OGG,
            ".opus" => AudioFileFormat.OPUS,
            ".wav" => AudioFileFormat.WAV,
            ".wma" => AudioFileFormat.WMA,
            ".aiff" or ".aif" => AudioFileFormat.AIFF,
            ".ape" => AudioFileFormat.APE,
            ".wv" => AudioFileFormat.WAVPACK,
            ".dsf" => AudioFileFormat.DSF,
            ".dff" => AudioFileFormat.DFF,
            ".mka" => AudioFileFormat.MATROSKA,
            _ => AudioFileFormat.None
        };
    }
}
