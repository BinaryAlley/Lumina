namespace Lumina.Domain.SharedKernel.Common.Enums.Playback;

/// <summary>
/// Enumeration for the ways in which a track's audio is delivered to a player.
/// </summary>
public enum PlaybackDeliveryMode
{
    /// <summary>
    /// The original audio file is delivered unchanged, because the player can demux its container and decode its codec as stored.
    /// </summary>
    DirectPlay,

    /// <summary>
    /// The audio is repackaged into a container the player can demux, without re-encoding its codec.
    /// </summary>
    Remux,

    /// <summary>
    /// The audio is re-encoded into a codec the player can decode, because the stored codec is not one the player supports.
    /// </summary>
    Transcode,
}
