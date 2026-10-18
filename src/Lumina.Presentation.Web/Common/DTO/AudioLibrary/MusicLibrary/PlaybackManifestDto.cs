#region ========================================================================= USING =====================================================================================
using Lumina.Presentation.Web.Common.Enums.AudioLibrary;
using System;
using System.Diagnostics;
#endregion

namespace Lumina.Presentation.Web.Common.DTO.AudioLibrary.MusicLibrary;

/// <summary>
/// Data transfer object for the playback manifest of a track, describing the format of its stored audio, the delivery the server decided on for the requesting player, and the path of its stream.
/// </summary>
[DebuggerDisplay("TrackId: {TrackId} IsDirectPlay: {IsDirectPlay}")]
public class PlaybackManifestDto
{
    /// <summary>
    /// Gets or sets the Id of the track the manifest describes.
    /// </summary>
    public Guid TrackId { get; set; }

    /// <summary>
    /// Gets or sets the duration of the audio of the track, in seconds.
    /// </summary>
    public int DurationInSeconds { get; set; }

    /// <summary>
    /// Gets or sets the container of the audio file of the track, as stored by the probe.
    /// </summary>
    public string? SourceContainer { get; set; }

    /// <summary>
    /// Gets or sets the codec of the audio of the track, as stored by the probe.
    /// </summary>
    public string? SourceCodec { get; set; }

    /// <summary>
    /// Gets or sets the sample rate of the audio of the track, in Hz.
    /// </summary>
    public int SampleRate { get; set; }

    /// <summary>
    /// Gets or sets the number of audio channels of the track.
    /// </summary>
    public int Channels { get; set; }

    /// <summary>
    /// Gets or sets the bit depth of the audio of the track.
    /// </summary>
    public int? BitDepth { get; set; }

    /// <summary>
    /// Gets or sets the ReplayGain track gain of the track, in decibels.
    /// </summary>
    public decimal? ReplayGainTrackGain { get; set; }

    /// <summary>
    /// Gets or sets the ReplayGain track peak of the track.
    /// </summary>
    public decimal? ReplayGainTrackPeak { get; set; }

    /// <summary>
    /// Gets or sets the ReplayGain album gain of the track, in decibels.
    /// </summary>
    public decimal? ReplayGainAlbumGain { get; set; }

    /// <summary>
    /// Gets or sets the ReplayGain album peak of the track.
    /// </summary>
    public decimal? ReplayGainAlbumPeak { get; set; }

    /// <summary>
    /// Gets or sets whether the player can play the stored audio as it is, without the server changing its container or its codec.
    /// </summary>
    public bool IsDirectPlay { get; set; }

    /// <summary>
    /// Gets or sets the container of the audio the server delivers, when the delivery is not a direct play.
    /// </summary>
    public string? DeliveryContainer { get; set; }

    /// <summary>
    /// Gets or sets the codec of the audio the server delivers, when the delivery is not a direct play.
    /// </summary>
    public string? DeliveryCodec { get; set; }

    /// <summary>
    /// Gets or sets the shape the delivered audio is addressed and seeked by, when the delivery is not a direct play.
    /// </summary>
    public PlaybackDeliveryShape? DeliveryShape { get; set; }

    /// <summary>
    /// Gets or sets the duration of each segment of the delivered audio, in seconds, when the delivery is addressed as time addressed segments.
    /// </summary>
    public int? SegmentDurationSeconds { get; set; }

    /// <summary>
    /// Gets or sets whether the delivered audio can be seeked by the player.
    /// </summary>
    public bool IsSeekable { get; set; }

    /// <summary>
    /// Gets or sets the path of the track stream. The API returns it relative to itself; this web application replaces it with
    /// the URL of its own streaming proxy, so that the browser never reaches the API server directly.
    /// </summary>
    public string StreamPath { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the opaque handle of the normalized delivery. The API returns it so the segment route can address exactly the delivery
    /// the manifest declared. It is <see langword="null"/> for a direct play.
    /// </summary>
    public string? DeliveryId { get; set; }

    /// <summary>
    /// Gets or sets the path template of the segment route of the normalized delivery. The API returns it relative to itself with a
    /// <c>{deliveryId}</c> and a <c>{segmentIndex}</c> placeholder; this web application replaces it with the URL of its own segment proxy, so
    /// that the browser never reaches the API server directly. It is <see langword="null"/> for a direct play.
    /// </summary>
    public string? SegmentStreamPathTemplate { get; set; }
}
