namespace Lumina.Application.Core.MediaLibrary.AudioLibrary.MusicLibrary.Playback;

/// <summary>
/// Defines the format of the opaque delivery handle of a playback manifest.
/// </summary>
/// <remarks>
/// A handle is a <see cref="Guid"/> rendered with the <c>N</c> format: exactly 32 lowercase hexadecimal characters, with no separator. Every producer
/// and consumer of a handle uses this rule, so a handle is never silently normalized into another value.
/// </remarks>
public static class PlaybackDeliveryId
{
    /// <summary>
    /// The number of characters of a delivery handle.
    /// </summary>
    public const int CHARACTER_COUNT = 32;

    /// <summary>
    /// Checks whether a value has the format of a delivery handle.
    /// </summary>
    /// <param name="value">The value to check.</param>
    /// <returns><see langword="true"/> when the value is exactly 32 lowercase hexadecimal characters, otherwise <see langword="false"/>.</returns>
    public static bool IsValid(string? value)
    {
        if (value is null || value.Length != CHARACTER_COUNT)
            return false;
        foreach (char character in value)
            if (!((character >= '0' && character <= '9') || (character >= 'a' && character <= 'f')))
                return false;
        return true;
    }
}
