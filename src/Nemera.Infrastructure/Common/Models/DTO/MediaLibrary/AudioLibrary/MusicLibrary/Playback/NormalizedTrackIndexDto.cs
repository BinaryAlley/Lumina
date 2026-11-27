#region ========================================================================= USING =====================================================================================
using Lumina.Domain.SharedKernel.Common.Enums.Playback;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text.Json.Serialization;
#endregion

namespace Lumina.Infrastructure.Common.Models.DTO.MediaLibrary.AudioLibrary.MusicLibrary.Playback;

/// <summary>
/// Represents the persisted description of a cached normalized track, which lets the cache be read without parsing the ffmpeg output again.
/// </summary>
/// <param name="Version">The version of the index layout, so an index written by an older version is treated as a cache miss.</param>
/// <param name="TrackId">The Id of the track the cached audio belongs to, so a cache handle can be authorized against the track of the route.</param>
/// <param name="Mode">The way the audio is delivered, which is a remux or a transcode.</param>
/// <param name="Container">The container of the delivered audio.</param>
/// <param name="Codec">The codec of the delivered audio.</param>
/// <param name="SegmentDurationSeconds">The duration of each segment of the delivered audio, in seconds.</param>
/// <param name="DurationInSeconds">
/// The duration of the produced and cached audio, in seconds, rounded up to the next second. It is the produced end rather than the probed duration, so
/// it always covers every produced segment.
/// </param>
/// <param name="Segments">The segments that make up the delivered audio, in playback order.</param>
[DebuggerDisplay("TrackId: {TrackId} Container: {Container} Codec: {Codec} Segments: {Segments.Count}")]
// The record is internal, so its primary constructor is not public. Marking it tells System.Text.Json to use it for the on disk index anyway.
[method: JsonConstructor]
internal sealed record NormalizedTrackIndexDto(
    int Version,
    Guid TrackId,
    PlaybackDeliveryMode Mode,
    string Container,
    string Codec,
    int SegmentDurationSeconds,
    int DurationInSeconds,
    IReadOnlyList<NormalizedTrackSegmentIndexDto> Segments
);
