#region ========================================================================= USING =====================================================================================
using Lumina.Domain.SharedKernel.Common.Enums.Playback;
using System;
using System.Collections.Generic;
using System.Diagnostics;
#endregion

namespace Lumina.Application.Common.DTO.MediaLibrary.AudioLibrary.MusicLibrary.Playback;

/// <summary>
/// Data transfer object for the request to produce the normalized audio of a track, as the delivery decision of a playback manifest describes it.
/// </summary>
/// <param name="TrackId">The Id of the track whose audio is normalized. It binds the produced cache entry to the track the caller authorized.</param>
/// <param name="SourcePath">The file system path of the stored audio file of the track.</param>
/// <param name="AllowedRoots">
/// The content locations the source path is allowed to be inside. The service validates the path against them and re-checks it after opening, so a
/// caller that forgets to enforce containment cannot hand an arbitrary path to the transcoder.
/// </param>
/// <param name="SourceContainer">The container of the stored audio file, as captured by the probe, used to pin the ffmpeg demuxer. Optional.</param>
/// <param name="Mode">The way the audio is delivered, which is a remux or a transcode.</param>
/// <param name="Container">The container of the delivered audio, as chosen by the delivery decision.</param>
/// <param name="Codec">The codec of the delivered audio, as chosen by the delivery decision.</param>
/// <param name="SegmentDurationSeconds">The duration of each segment of the delivered audio, in seconds.</param>
/// <param name="DurationInSeconds">The duration of the audio of the track, in seconds.</param>
/// <param name="Bitrate">The stored audio bitrate of the track, in kbps, used to estimate a remux of a lossy codec. Optional.</param>
/// <param name="SampleRate">The sample rate of the stored audio of the track, in Hz, used to estimate the size of a lossless delivery.</param>
/// <param name="Channels">The number of audio channels of the track, used to estimate the size of a lossless delivery.</param>
/// <param name="BitDepth">The bit depth of the stored audio of the track, if applicable, used to estimate the size of a lossless delivery.</param>
/// <param name="Quality">The quality cap applied to the delivered audio when it has to be re-encoded.</param>
[DebuggerDisplay("TrackId: {TrackId} Mode: {Mode} Container: {Container} Codec: {Codec}")]
public sealed record NormalizedTrackRequestDto(
    Guid TrackId,
    string SourcePath,
    IReadOnlyList<string> AllowedRoots,
    string? SourceContainer,
    PlaybackDeliveryMode Mode,
    string Container,
    string Codec,
    int SegmentDurationSeconds,
    int DurationInSeconds,
    int? Bitrate,
    int SampleRate,
    int Channels,
    int? BitDepth,
    PlaybackStreamingQuality Quality
);
