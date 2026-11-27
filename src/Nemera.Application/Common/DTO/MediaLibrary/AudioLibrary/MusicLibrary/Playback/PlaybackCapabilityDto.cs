#region ========================================================================= USING =====================================================================================
using System.Diagnostics;
#endregion

namespace Lumina.Application.Common.DTO.MediaLibrary.AudioLibrary.MusicLibrary.Playback;

/// <summary>
/// Data transfer object for a single container and codec combination that a player advertises it can demux and decode directly.
/// </summary>
/// <param name="Container">The container the player can demux, for example MP3, FLAC, OGG, WAV or MP4.</param>
/// <param name="Codec">The codec the player can decode, for example MP3, FLAC, Opus, Vorbis, PCM or AAC.</param>
[DebuggerDisplay("Container: {Container} Codec: {Codec}")]
public sealed record PlaybackCapabilityDto(
    string Container,
    string Codec
);
