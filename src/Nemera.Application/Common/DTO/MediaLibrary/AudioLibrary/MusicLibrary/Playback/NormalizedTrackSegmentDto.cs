#region ========================================================================= USING =====================================================================================
using System.Diagnostics;
#endregion

namespace Lumina.Application.Common.DTO.MediaLibrary.AudioLibrary.MusicLibrary.Playback;

/// <summary>
/// Data transfer object for a single fixed duration segment of the normalized audio of a track.
/// </summary>
/// <param name="Index">The zero based position of the segment in the delivered audio.</param>
/// <param name="StartSeconds">The time, in seconds from the start of the track, at which the segment starts.</param>
/// <param name="DurationSeconds">The duration of the segment, in seconds.</param>
/// <param name="Length">The length of the segment, in bytes.</param>
[DebuggerDisplay("Index: {Index} StartSeconds: {StartSeconds} DurationSeconds: {DurationSeconds}")]
public sealed record NormalizedTrackSegmentDto(
    int Index,
    decimal StartSeconds,
    decimal DurationSeconds,
    long Length
);
