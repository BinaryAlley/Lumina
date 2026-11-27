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
/// capped, and the input is scanned entry by entry over its own characters, so a request never materializes an array of all its
/// entries and never yields more than a fixed number of capabilities, regardless of its length. When the input is absent or lists no
/// valid pair, the <see cref="Default"/> capabilities of the bundled player apply.
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
        // The input is scanned entry by entry over its own characters, so the entries are never all materialized at once: only the
        // container and the codec of an accepted pair are allocated, and the number of accepted pairs is capped.
        ReadOnlySpan<char> remainingEntries = capabilitiesInput.AsSpan();
        while (!remainingEntries.IsEmpty)
        {
            // The number of accepted pairs is capped, so a request never yields more than a fixed number of capabilities.
            if (capabilities.Count >= MAX_CAPABILITY_COUNT)
                break;

            int entrySeparatorIndex = remainingEntries.IndexOf(CAPABILITY_SEPARATOR);
            ReadOnlySpan<char> entry = entrySeparatorIndex < 0 ? remainingEntries : remainingEntries[..entrySeparatorIndex];
            remainingEntries = entrySeparatorIndex < 0 ? ReadOnlySpan<char>.Empty : remainingEntries[(entrySeparatorIndex + 1)..];

            // A pair names exactly one container and one codec, so exactly one separator must be present, and both parts must be
            // named: a container or a codec that is empty (for example "a//b" or "/wav") does not name a pair.
            int containerCodecSeparatorIndex = entry.IndexOf(CONTAINER_CODEC_SEPARATOR);
            if (containerCodecSeparatorIndex < 0)
                continue;
            ReadOnlySpan<char> container = entry[..containerCodecSeparatorIndex].Trim();
            ReadOnlySpan<char> codec = entry[(containerCodecSeparatorIndex + 1)..].Trim();
            if (container.IsEmpty || codec.IsEmpty || codec.Contains(CONTAINER_CODEC_SEPARATOR))
                continue;

            capabilities.Add(new PlaybackCapabilityDto(container.ToString(), codec.ToString()));
        }

        return capabilities.Count == 0 ? Default : capabilities;
    }
}
