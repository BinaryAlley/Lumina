#region ========================================================================= USING =====================================================================================
using Lumina.Domain.SharedKernel.Common.Enums.Playback;
using System;
using System.Diagnostics;
#endregion

namespace Lumina.Contracts.Responses.MediaLibrary.AudioLibrary.MusicLibrary.Playback;

/// <summary>
/// Represents a response for the playback manifest of a track, containing the format of its stored audio, the delivery the server decided on for the
/// requesting player, and the path of the track stream, so that a player is told what to fetch and how before it fetches any audio.
/// </summary>
/// <param name="TrackId">The Id of the track the manifest describes.</param>
/// <param name="DurationInSeconds">The duration of the audio of the track, in seconds, as stored by the probe. A normalized production can differ slightly; its exact produced duration is reported by the segment responses.</param>
/// <param name="SourceContainer">The container of the audio file of the track, as stored by the probe, for example MP3, FLAC, OGG, AAC, MP4 or WAV. Optional.</param>
/// <param name="SourceCodec">The codec of the audio of the track, as stored by the probe. Optional.</param>
/// <param name="SampleRate">The sample rate of the audio of the track, in Hz.</param>
/// <param name="Channels">The number of audio channels of the track.</param>
/// <param name="BitDepth">The bit depth of the audio of the track. Optional.</param>
/// <param name="ReplayGainTrackGain">The ReplayGain track gain of the track, in decibels. Optional.</param>
/// <param name="ReplayGainTrackPeak">The ReplayGain track peak of the track. Optional.</param>
/// <param name="ReplayGainAlbumGain">The ReplayGain album gain of the track, in decibels. Optional.</param>
/// <param name="ReplayGainAlbumPeak">The ReplayGain album peak of the track. Optional.</param>
/// <param name="IsDirectPlay">Whether the player can play the stored audio as it is, without the server changing its container or its codec.</param>
/// <param name="DeliveryContainer">The container of the audio the server delivers, when the delivery is not a direct play. <see langword="null"/> for a direct play.</param>
/// <param name="DeliveryCodec">The codec of the audio the server delivers, when the delivery is not a direct play. <see langword="null"/> for a direct play.</param>
/// <param name="DeliveryShape">The shape the delivered audio is addressed and seeked by, when the delivery is not a direct play. <see langword="null"/> for a direct play.</param>
/// <param name="SegmentDurationSeconds">The duration of each segment of the delivered audio, in seconds, when the delivery is addressed as time addressed segments. <see langword="null"/> otherwise.</param>
/// <param name="IsSeekable">Whether the delivered audio can be seeked by the player.</param>
/// <param name="StreamPath">The path of the track stream, relative to the API, without a host or a scheme, so that a client never reaches the API directly.</param>
/// <param name="DeliveryId">
/// The opaque handle of the normalized delivery, which the segment route accepts in place of a capability list, so the delivery the
/// manifest declared cannot be recomputed from inputs that change while the track plays. <see langword="null"/> for a direct play.
/// </param>
/// <param name="SegmentStreamPathTemplate">
/// The path of the segment route of the normalized delivery, relative to the API and without a host or a scheme, with a <c>{deliveryId}</c> and a
/// <c>{segmentIndex}</c> placeholder. <see langword="null"/> for a direct play, whose delivery is addressed by byte range.
/// </param>
[DebuggerDisplay("TrackId: {TrackId} IsDirectPlay: {IsDirectPlay}")]
public sealed record PlaybackManifestResponse(
    Guid TrackId,
    int DurationInSeconds,
    string? SourceContainer,
    string? SourceCodec,
    int SampleRate,
    int Channels,
    int? BitDepth,
    decimal? ReplayGainTrackGain,
    decimal? ReplayGainTrackPeak,
    decimal? ReplayGainAlbumGain,
    decimal? ReplayGainAlbumPeak,
    bool IsDirectPlay,
    string? DeliveryContainer,
    string? DeliveryCodec,
    PlaybackDeliveryShape? DeliveryShape,
    int? SegmentDurationSeconds,
    bool IsSeekable,
    string StreamPath,
    string? DeliveryId,
    string? SegmentStreamPathTemplate
);
