#region ========================================================================= USING =====================================================================================
using System.Diagnostics;
#endregion

namespace Lumina.Application.Common.DTO.MediaLibrary.AudioLibrary.MusicLibrary.Tracks;

/// <summary>
/// Data transfer object for the stored probe result of a track, containing only the fields a playback manifest is built from.
/// </summary>
[DebuggerDisplay("Container: {Container} Codec: {AudioCodec}")]
public sealed record TrackPlaybackManifestDto
{
    /// <summary>
    /// Gets the container of the audio file of the track, for example MP3, FLAC, OGG, AAC, MP4 or WAV, if applicable.
    /// </summary>
    public string? Container { get; init; }

    /// <summary>
    /// Gets the audio codec used by the audio file of the track, if applicable.
    /// </summary>
    public string? AudioCodec { get; init; }

    /// <summary>
    /// Gets the duration of the audio of the track in seconds.
    /// </summary>
    public int DurationInSeconds { get; init; }

    /// <summary>
    /// Gets the sample rate of the audio of the track in Hz.
    /// </summary>
    public int SampleRate { get; init; }

    /// <summary>
    /// Gets the number of audio channels of the track.
    /// </summary>
    public int Channels { get; init; }

    /// <summary>
    /// Gets the bit depth of the audio of the track, if applicable.
    /// </summary>
    public int? BitDepth { get; init; }

    /// <summary>
    /// Gets the ReplayGain track gain of the track, in decibels, if applicable.
    /// </summary>
    public decimal? ReplayGainTrackGain { get; init; }

    /// <summary>
    /// Gets the ReplayGain track peak of the track, if applicable.
    /// </summary>
    public decimal? ReplayGainTrackPeak { get; init; }

    /// <summary>
    /// Gets the ReplayGain album gain of the track, in decibels, if applicable.
    /// </summary>
    public decimal? ReplayGainAlbumGain { get; init; }

    /// <summary>
    /// Gets the ReplayGain album peak of the track, if applicable.
    /// </summary>
    public decimal? ReplayGainAlbumPeak { get; init; }
}
