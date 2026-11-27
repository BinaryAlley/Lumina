#region ========================================================================= USING =====================================================================================
using System.Diagnostics;
#endregion

namespace Lumina.Infrastructure.Common.Models.DTO.MediaLibrary.AudioLibrary.MusicLibrary.Playback;

/// <summary>
/// Describes how a single container and codec combination of the audio normalization pipeline is muxed and, when it has to be re-encoded, encoded.
/// </summary>
/// <param name="Container">The canonical container of the delivered audio.</param>
/// <param name="Codec">The canonical codec of the delivered audio.</param>
/// <param name="MuxerName">The name of the ffmpeg muxer that writes the container.</param>
/// <param name="FileExtension">The file extension of a single segment, including the leading dot.</param>
/// <param name="EncoderName">The name of the ffmpeg encoder used when the audio is transcoded.</param>
/// <param name="IsLossless">Whether the codec is lossless, in which case an encoder bitrate cap does not apply.</param>
[DebuggerDisplay("Container: {Container} Codec: {Codec} Muxer: {MuxerName}")]
internal sealed record NormalizedAudioFormatDto(
    string Container,
    string Codec,
    string MuxerName,
    string FileExtension,
    string EncoderName,
    bool IsLossless
);
