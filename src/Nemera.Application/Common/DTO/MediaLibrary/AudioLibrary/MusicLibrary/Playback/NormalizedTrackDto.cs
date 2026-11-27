#region ========================================================================= USING =====================================================================================
using Lumina.Domain.SharedKernel.Common.Enums.Playback;
using System;
using System.Collections.Generic;
using System.Diagnostics;
#endregion

namespace Lumina.Application.Common.DTO.MediaLibrary.AudioLibrary.MusicLibrary.Playback;

/// <summary>
/// Data transfer object for the normalized audio of a track, produced and cached as a sequence of fixed duration segments.
/// </summary>
/// <param name="TrackId">The Id of the track whose audio the cached entry holds, so the caller can verify that the entry is the one it authorized.</param>
/// <param name="CacheId">The opaque identifier of the cached entry that holds the normalized audio.</param>
/// <param name="Mode">The way the audio is delivered, which is a remux or a transcode.</param>
/// <param name="Container">The container of the delivered audio.</param>
/// <param name="Codec">The codec of the delivered audio.</param>
/// <param name="SegmentDurationSeconds">The duration of each segment of the delivered audio, in seconds.</param>
/// <param name="DurationInSeconds">
/// The duration of the produced and cached audio, in seconds, rounded up to the next second. It is the produced end rather than the probed duration, so
/// it always covers every produced segment.
/// </param>
/// <param name="Segments">The segments that make up the delivered audio, in playback order.</param>
[DebuggerDisplay("TrackId: {TrackId} CacheId: {CacheId} Container: {Container} Codec: {Codec}")]
public sealed record NormalizedTrackDto(
    Guid TrackId,
    string CacheId,
    PlaybackDeliveryMode Mode,
    string Container,
    string Codec,
    int SegmentDurationSeconds,
    int DurationInSeconds,
    IReadOnlyList<NormalizedTrackSegmentDto> Segments
);
