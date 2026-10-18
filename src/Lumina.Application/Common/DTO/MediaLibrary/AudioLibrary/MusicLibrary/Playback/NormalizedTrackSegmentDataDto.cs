#region ========================================================================= USING =====================================================================================
using System.Diagnostics;
using System.IO;
#endregion

namespace Lumina.Application.Common.DTO.MediaLibrary.AudioLibrary.MusicLibrary.Playback;

/// <summary>
/// Data transfer object for a single produced segment of the normalized audio of a track, ready to be written to the response.
/// </summary>
/// <remarks>
/// The segment owns a live stream over a leased cache entry, so it is an internal delivery object of the API rather than a serializable
/// contract. The caller must dispose the stream once the response is written, which releases the cache reader lease.
/// </remarks>
/// <param name="Content">The read stream over the segment, which the caller owns and must release once the response is written.</param>
/// <param name="Length">The length of the segment, in bytes, taken from the index of the normalized audio.</param>
/// <param name="StartSeconds">The exact start of the segment on the track timeline, in seconds, taken from the index of the normalized audio.</param>
/// <param name="Container">The container of the delivered audio, used to resolve the media type of the segment.</param>
/// <param name="SegmentCount">The number of segments the production holds, so the caller can tell the end of the audio from a missing segment.</param>
/// <param name="DurationInSeconds">The duration of the produced normalized audio, in seconds, which can differ from the probed duration.</param>
[DebuggerDisplay("Length: {Length} StartSeconds: {StartSeconds} Container: {Container} SegmentCount: {SegmentCount}")]
public sealed record NormalizedTrackSegmentDataDto(
    Stream Content,
    long Length,
    decimal StartSeconds,
    string Container,
    int SegmentCount,
    int DurationInSeconds
);
