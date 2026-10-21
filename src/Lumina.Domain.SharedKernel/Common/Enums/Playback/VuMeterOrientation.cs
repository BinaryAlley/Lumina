namespace Lumina.Domain.SharedKernel.Common.Enums.Playback;

/// <summary>
/// Enumeration for the direction in which each channel of the VU meter of the audio player grows.
/// </summary>
public enum VuMeterOrientation
{
    /// <summary>
    /// The channels of the meter are columns and their levels grow from bottom to top.
    /// </summary>
    Vertical,

    /// <summary>
    /// The channels of the meter are rows and their levels grow from left to right.
    /// </summary>
    Horizontal,
}

