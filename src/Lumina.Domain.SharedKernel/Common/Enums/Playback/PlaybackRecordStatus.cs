namespace Lumina.Domain.SharedKernel.Common.Enums.Playback;

/// <summary>
/// Enumeration for the states of a recorded playback.
/// </summary>
public enum PlaybackRecordStatus
{
    /// <summary>
    /// The playback was recorded as started and has not ended yet.
    /// </summary>
    InProgress,

    /// <summary>
    /// The playback reached the end of the media item.
    /// </summary>
    Completed,

    /// <summary>
    /// The playback ended before the media item completed.
    /// </summary>
    Stopped,
}
