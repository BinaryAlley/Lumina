namespace Lumina.Domain.SharedKernel.Common.Enums.Playback;

/// <summary>
/// Enumeration for the quality cap applied to the audio the server normalizes (remuxes or transcodes) for a user.
/// </summary>
/// <remarks>
/// The caps apply only to audio the server has to change; a track that is played directly is always delivered as it was stored.
/// </remarks>
public enum PlaybackStreamingQuality
{
    /// <summary>
    /// The server chooses the quality, with no explicit cap.
    /// </summary>
    Auto,

    /// <summary>
    /// Normalized audio is capped at 96 kbps.
    /// </summary>
    Low,

    /// <summary>
    /// Normalized audio is capped at 160 kbps.
    /// </summary>
    Medium,

    /// <summary>
    /// Normalized audio is capped at 320 kbps.
    /// </summary>
    High,
}
