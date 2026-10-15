#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DTO.MediaLibrary.AudioLibrary.MusicLibrary.Playback;
using System;
using System.Collections.Generic;
#endregion

namespace Lumina.Application.Core.MediaLibrary.AudioLibrary.MusicLibrary.Playback;

/// <summary>
/// Parses the capabilities input of a playback manifest request into the container and codec combinations a player advertises it
/// can demux and decode directly.
/// </summary>
/// <remarks>
/// The input is a comma separated list of <c>container/codec</c> pairs, for example
/// <c>mp3/mp3,flac/flac,ogg/opus,ogg/vorbis,wav/pcm,mp4/aac</c>. The container and the codec of a pair are matched case
/// insensitively, and a pair that does not name both, with a non-empty container and codec, is ignored. The number of pairs is
/// capped, so a request cannot force an unbounded amount of parsing work. When the input is absent or lists no valid pair, the
/// <see cref="Default"/> capabilities of the bundled player apply.
/// </remarks>
public static class PlaybackCapabilitiesParser
{
    /// <summary>
    /// The maximum number of capability pairs that are read from a single request, so that the parsing work stays bounded.
    /// </summary>
    public const int MAX_CAPABILITY_COUNT = 32;

    /// <summary>
    /// The character that separates the container from the codec of a single capability pair.
    /// </summary>
    public const char CONTAINER_CODEC_SEPARATOR = '/';

    /// <summary>
    /// The character that separates the capability pairs of a capability set.
    /// </summary>
    public const char CAPABILITY_SEPARATOR = ',';

    /// <summary>
    /// Gets the capability set the bundled player advertises, used when a request does not advertise any capability.
    /// </summary>
    public static IReadOnlyList<PlaybackCapabilityDto> Default { get; } =
    [
        new PlaybackCapabilityDto("MP3", "MP3"),
        new PlaybackCapabilityDto("FLAC", "FLAC"),
        new PlaybackCapabilityDto("OGG", "OPUS"),
        new PlaybackCapabilityDto("OGG", "VORBIS"),
        new PlaybackCapabilityDto("WAV", "PCM"),
        new PlaybackCapabilityDto("AAC", "AAC")
    ];

    /// <summary>
    /// Parses the capabilities input of a playback manifest request into a capability set.
    /// </summary>
    /// <param name="capabilitiesInput">The value of the capabilities input, a comma separated list of <c>container/codec</c> pairs.</param>
    /// <returns>The parsed capabilities, or <see cref="Default"/> when the input is absent, empty or lists no valid pair.</returns>
    public static IReadOnlyList<PlaybackCapabilityDto> Parse(string? capabilitiesInput)
    {
        if (string.IsNullOrWhiteSpace(capabilitiesInput))
            return Default;

        List<PlaybackCapabilityDto> capabilities = [];
        foreach (string entry in capabilitiesInput.Split(CAPABILITY_SEPARATOR, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            // The number of pairs is capped, so a single request cannot force an unbounded amount of parsing work.
            if (capabilities.Count >= MAX_CAPABILITY_COUNT)
                break;

            // Both parts must be named: a container or a codec that is empty (for example "a//b" or "/wav") does not name a pair.
            string[] parts = entry.Split(CONTAINER_CODEC_SEPARATOR, StringSplitOptions.TrimEntries);
            if (parts.Length != 2 || parts[0].Length == 0 || parts[1].Length == 0)
                continue;
            capabilities.Add(new PlaybackCapabilityDto(parts[0], parts[1]));
        }

        return capabilities.Count == 0 ? Default : capabilities;
    }
}
