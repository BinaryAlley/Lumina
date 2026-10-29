#region ========================================================================= USING =====================================================================================
using System.Diagnostics;
using System.Text.Json.Serialization;
#endregion

namespace Lumina.Infrastructure.Common.Models.DTO.MediaLibrary.AudioLibrary.MusicLibrary.Playback;

/// <summary>
/// Represents the persisted description of a single segment of a cached normalized track.
/// </summary>
/// <param name="Index">The zero based position of the segment in the delivered audio.</param>
/// <param name="StartSeconds">The time, in seconds from the start of the track, at which the segment starts.</param>
/// <param name="DurationSeconds">The duration of the segment, in seconds.</param>
/// <param name="FileName">The file name of the segment inside the cache entry.</param>
/// <param name="Length">The length of the segment, in bytes.</param>
[DebuggerDisplay("Index: {Index} StartSeconds: {StartSeconds} FileName: {FileName}")]
// The record is internal, so its primary constructor is not public. Marking it tells System.Text.Json to use it for the on disk index anyway.
[method: JsonConstructor]
internal sealed record NormalizedTrackSegmentIndexDto(
    int Index,
    decimal StartSeconds,
    decimal DurationSeconds,
    string FileName,
    long Length
);
