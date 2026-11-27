#region ========================================================================= USING =====================================================================================
using System.Diagnostics;
#endregion

namespace Lumina.Application.Common.DTO.MediaLibrary.AudioLibrary.MusicLibrary.Tracks;

/// <summary>
/// Data transfer object for the stored probe result of a track, containing only the fields a playback manifest is built from.
/// </summary>
/// <param name="Path">The file system path of the stored audio file of the track, used to bind a normalized delivery to the file version it was declared for.</param>
/// <param name="Container">The container of the audio file of the track, for example MP3, FLAC, OGG, AAC, MP4 or WAV, if applicable.</param>
/// <param name="AudioCodec">The audio codec used by the audio file of the track, if applicable.</param>
/// <param name="DurationInSeconds">The duration of the audio of the track in seconds.</param>
/// <param name="SampleRate">The sample rate of the audio of the track in Hz.</param>
/// <param name="Channels">The number of audio channels of the track.</param>
/// <param name="BitDepth">The bit depth of the audio of the track, if applicable.</param>
/// <param name="Bitrate">The bitrate of the audio of the track, in kbps, if applicable.</param>
/// <param name="ReplayGainTrackGain">The ReplayGain track gain of the track, in decibels, if applicable.</param>
/// <param name="ReplayGainTrackPeak">The ReplayGain track peak of the track, if applicable.</param>
/// <param name="ReplayGainAlbumGain">The ReplayGain album gain of the track, in decibels, if applicable.</param>
/// <param name="ReplayGainAlbumPeak">The ReplayGain album peak of the track, if applicable.</param>
[DebuggerDisplay("Container: {Container} Codec: {AudioCodec}")]
public sealed record TrackPlaybackManifestDto(
    string Path,
    string? Container,
    string? AudioCodec,
    int DurationInSeconds,
    int SampleRate,
    int Channels,
    int? BitDepth,
    int? Bitrate,
    decimal? ReplayGainTrackGain,
    decimal? ReplayGainTrackPeak,
    decimal? ReplayGainAlbumGain,
    decimal? ReplayGainAlbumPeak
);
