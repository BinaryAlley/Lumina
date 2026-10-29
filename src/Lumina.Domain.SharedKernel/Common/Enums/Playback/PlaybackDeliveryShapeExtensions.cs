namespace Lumina.Domain.SharedKernel.Common.Enums.Playback;

/// <summary>
/// Extension methods for <see cref="PlaybackDeliveryShape"/>.
/// </summary>
public static class PlaybackDeliveryShapeExtensions
{
    /// <summary>
    /// Gets whether the audio delivered in the provided <paramref name="shape"/> can be seeked by a player.
    /// </summary>
    /// <param name="shape">The shape the delivered audio is addressed and seeked by.</param>
    /// <returns>
    /// <see langword="true"/> when the shape can be seeked, otherwise <see langword="false"/>. Audio delivered as the original file is
    /// seeked by byte range, and audio delivered as time addressed segments is seeked by segment, so both of the shapes the server
    /// produces are seekable; a future shape that cannot be seeked reports <see langword="false"/>.
    /// </returns>
    public static bool IsSeekable(this PlaybackDeliveryShape shape)
    {
        return shape is PlaybackDeliveryShape.ByteRangeSeekable or PlaybackDeliveryShape.TimeAddressedSegments;
    }
}
