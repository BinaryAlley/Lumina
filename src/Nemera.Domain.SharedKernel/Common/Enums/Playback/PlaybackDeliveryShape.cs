namespace Lumina.Domain.SharedKernel.Common.Enums.Playback;

/// <summary>
/// Enumeration for the shapes in which delivered audio is addressed and seeked.
/// </summary>
public enum PlaybackDeliveryShape
{
    /// <summary>
    /// The delivered media is the original file itself, addressed and seeked by byte range.
    /// </summary>
    ByteRangeSeekable,

    /// <summary>
    /// The delivered media is a sequence of fixed duration segments, addressed and seeked by time, so that finding a time never
    /// requires mapping a byte range to it.
    /// </summary>
    TimeAddressedSegments,
}
