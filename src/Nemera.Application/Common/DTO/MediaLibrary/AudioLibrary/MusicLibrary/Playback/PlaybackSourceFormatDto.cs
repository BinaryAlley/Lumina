#region ========================================================================= USING =====================================================================================
using System.Diagnostics;
#endregion

namespace Lumina.Application.Common.DTO.MediaLibrary.AudioLibrary.MusicLibrary.Playback;

/// <summary>
/// Data transfer object for the stored format of a track's audio, that a playback delivery decision is made on.
/// </summary>
/// <param name="Container">The container of the audio file of the track, for example MP3, FLAC, OGG, MP4 or WAV.</param>
/// <param name="Codec">The codec of the audio of the track, as reported by the probe, for example AAC, FLAC, Opus or PCM.</param>
[DebuggerDisplay("Container: {Container} Codec: {Codec}")]
public sealed record PlaybackSourceFormatDto(
    string? Container,
    string? Codec
);
