#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DTO.MediaLibrary.AudioLibrary.MusicLibrary.Playback;
using Lumina.Application.Core.MediaLibrary.AudioLibrary.MusicLibrary.Playback;
using Lumina.Domain.Common.Errors;
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.SharedKernel.Common.Enums.Playback;
using System;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Linq;
#endregion

namespace Lumina.Infrastructure.Core.MediaLibrary.AudioLibrary.MusicLibrary.Playback;

/// <summary>
/// Service that decides whether the audio of a track can be played as stored by a player, or has to be repackaged into another container (remuxed) or re-encoded into another codec (transcoded).
/// </summary>
/// <remarks>
/// The decision uses the container and the codec of the stored probe result together with the container and codec pairs the player advertises it can demux and decode directly:
/// a pair the player advertises as stored is a direct play, a codec the player decodes but a container it cannot demux is a remux into a container it can demux, and a codec the player cannot decode
/// is a transcode to the most preferred codec the server can produce and the player advertises. Everything that is not a direct play is delivered as fixed duration, time addressed segments, so that seeking never requires mapping a byte range to a time.
/// </remarks>
internal sealed class PlaybackFormatService : IPlaybackFormatService
{
    // The duration of each segment of a normalized (remuxed or transcoded) delivery, in seconds: short enough to seek quickly, long enough to keep the request overhead low.
    private const int SEGMENT_DURATION_SECONDS = 6;

    // The order in which the codecs the server can produce are tried when a transcode is needed, most preferred first: the
    // efficient lossy codecs come before the lossless ones, so a transcode stays small unless the player only supports lossless.
    private static readonly IReadOnlyList<string> s_transcodeCodecPreference = ["AAC", "OPUS", "MP3", "VORBIS", "FLAC", "PCM"];

    // The containers the server can produce each codec in, in order of preference. The delivered container is always chosen from
    // this list, so a container the player named is only ever matched against it and never becomes the delivered container itself.
    private static readonly FrozenDictionary<string, IReadOnlyList<string>> s_producibleContainersByCodec = new Dictionary<string, IReadOnlyList<string>>(StringComparer.OrdinalIgnoreCase)
    {
        { "AAC", ["MP4", "AAC"] },
        { "OPUS", ["OGG"] },
        { "VORBIS", ["OGG"] },
        { "MP3", ["MP3"] },
        { "FLAC", ["FLAC"] },
        { "PCM", ["WAV"] }
    }.ToFrozenDictionary(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Decides how the audio of a track, described by <paramref name="sourceFormat"/>, is delivered to a player that advertises <paramref name="capabilities"/>.
    /// </summary>
    /// <param name="sourceFormat">The container and codec of the audio of the track, read from its stored probe result.</param>
    /// <param name="capabilities">The container and codec combinations the player advertises it can demux and decode directly.</param>
    /// <returns>
    /// An <see cref="Result{TValue}"/> containing either the delivery decision, or an error when the server cannot produce any format the player advertises it supports.
    /// </returns>
    public Result<PlaybackDeliveryDecisionDto> DecideDelivery(PlaybackSourceFormatDto sourceFormat, IReadOnlyList<PlaybackCapabilityDto> capabilities)
    {
        string container = CanonicalizeContainer(sourceFormat.Container);
        string codec = CanonicalizeCodec(sourceFormat.Codec);

        // Direct play: the player can demux the stored container and decode the stored codec, so the file is delivered unchanged, and it is addressed and seeked by byte range.
        // The delivered container and codec are the stored ones, not the player's.
        if (IsFormatAdvertised(capabilities, container, codec))
            return new PlaybackDeliveryDecisionDto(PlaybackDeliveryMode.DirectPlay, container, codec, PlaybackDeliveryShape.ByteRangeSeekable, 0);

        // Remux: the player can decode the stored codec but has no demuxer for the stored container, so the codec is repackaged
        // into a container the server can produce and the player can demux, without re-encoding it; the normalized output is delivered as time addressed segments.
        PlaybackCapabilityDto? remuxFormat = SelectFormatForCodec(capabilities, codec);
        if (remuxFormat is not null)
            return new PlaybackDeliveryDecisionDto(PlaybackDeliveryMode.Remux, remuxFormat.Container, remuxFormat.Codec, PlaybackDeliveryShape.TimeAddressedSegments, SEGMENT_DURATION_SECONDS);

        // Transcode: the player cannot decode the stored codec, so the audio is re-encoded into the most preferred codec the server can produce and the player advertises,
        // and the normalized output is delivered as time addressed segments.
        PlaybackCapabilityDto? transcodeFormat = SelectTranscodeFormat(capabilities);
        if (transcodeFormat is null)
            return Errors.Playback.NoSupportedPlaybackFormat;

        return new PlaybackDeliveryDecisionDto(PlaybackDeliveryMode.Transcode, transcodeFormat.Container, transcodeFormat.Codec, PlaybackDeliveryShape.TimeAddressedSegments, SEGMENT_DURATION_SECONDS);
    }

    /// <summary>
    /// Checks whether the player advertises the provided <paramref name="container"/> and <paramref name="codec"/> as a combination it can demux and decode directly.
    /// </summary>
    /// <param name="capabilities">The capabilities advertised by the player.</param>
    /// <param name="container">The canonical container that is looked for.</param>
    /// <param name="codec">The canonical codec that is looked for.</param>
    /// <returns><see langword="true"/> when the player advertises the combination, otherwise <see langword="false"/>.</returns>
    private static bool IsFormatAdvertised(IReadOnlyList<PlaybackCapabilityDto> capabilities, string container, string codec)
    {
        if (container.Length == 0 || codec.Length == 0)
            return false;

        return capabilities.Any(capability =>
            string.Equals(CanonicalizeContainer(capability.Container), container, StringComparison.OrdinalIgnoreCase)
            && string.Equals(CanonicalizeCodec(capability.Codec), codec, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Selects the container the server produces the provided <paramref name="codec"/> in, preferring the server's canonical one, among the containers the player advertises it can demux for that codec.
    /// </summary>
    /// <param name="capabilities">The capabilities advertised by the player.</param>
    /// <param name="codec">The canonical codec that is delivered.</param>
    /// <returns>
    /// The container and codec the server delivers in, or <see langword="null"/> when the server cannot produce the codec in any container the player advertises it can demux.
    /// </returns>
    private static PlaybackCapabilityDto? SelectFormatForCodec(IReadOnlyList<PlaybackCapabilityDto> capabilities, string codec)
    {
        if (codec.Length == 0 || !s_producibleContainersByCodec.TryGetValue(codec, out IReadOnlyList<string>? producibleContainers))
            return null;

        IReadOnlyList<string> advertisedContainers = [.. capabilities
            .Where(capability => string.Equals(CanonicalizeCodec(capability.Codec), codec, StringComparison.OrdinalIgnoreCase))
            .Select(capability => CanonicalizeContainer(capability.Container))];

        // The delivered container is one of the server's own containers, chosen as the first one the player advertises: the player
        // only selects among the containers the server knows, so a container the player names never reaches the transcoder itself.
        foreach (string producibleContainer in producibleContainers)
            if (advertisedContainers.Contains(producibleContainer, StringComparer.OrdinalIgnoreCase))
                return new PlaybackCapabilityDto(producibleContainer, codec);

        return null;
    }

    /// <summary>
    /// Selects the most preferred codec the server can produce that the player advertises it can decode.
    /// </summary>
    /// <param name="capabilities">The capabilities advertised by the player.</param>
    /// <returns>The preferred producible container and codec, or <see langword="null"/> when the player supports no producible codec.</returns>
    private static PlaybackCapabilityDto? SelectTranscodeFormat(IReadOnlyList<PlaybackCapabilityDto> capabilities)
    {
        foreach (string preferredCodec in s_transcodeCodecPreference)
        {
            PlaybackCapabilityDto? format = SelectFormatForCodec(capabilities, preferredCodec);
            if (format is not null)
                return format;
        }

        return null;
    }

    /// <summary>
    /// Canonicalizes the container of an audio file, so that containers compare regardless of casing.
    /// </summary>
    /// <param name="container">The container to canonicalize.</param>
    /// <returns>The canonical container, or an empty string when there is none.</returns>
    private static string CanonicalizeContainer(string? container)
    {
        return string.IsNullOrWhiteSpace(container) ? string.Empty : container.Trim().ToUpperInvariant();
    }

    /// <summary>
    /// Canonicalizes the codec of an audio file, mapping the free form descriptions reported by the probe to the canonical codec
    /// names the players advertise, and leaving a description it does not recognize as unplayable.
    /// </summary>
    /// <param name="codec">The codec to canonicalize.</param>
    /// <returns>The canonical codec, or an empty string when there is none.</returns>
    private static string CanonicalizeCodec(string? codec)
    {
        if (string.IsNullOrWhiteSpace(codec))
            return string.Empty;

        // The probe reports the codec as the description the tagging library parsed from the file, for example "MPEG-1 Layer 3" rather than "MP3",
        // so the known descriptions are recognized by their distinctive token, most specific ones first. The MPEG layer is matched by its layer number only,
        // so MPEG-1 Layer 1 and Layer 2 are not mistaken for MP3, and a description that names no recognizable codec is left as it is,
        // which keeps it from matching any advertised codec and forces a transcode.
        string normalized = codec.Trim().ToUpperInvariant();
        if (normalized.Contains("VORBIS", StringComparison.Ordinal))
            return "VORBIS";
        if (normalized.Contains("OPUS", StringComparison.Ordinal))
            return "OPUS";
        if (normalized.Contains("FLAC", StringComparison.Ordinal))
            return "FLAC";
        if (normalized.Contains("ALAC", StringComparison.Ordinal) || normalized.Contains("APPLE LOSSLESS", StringComparison.Ordinal))
            return "ALAC";
        if (normalized.Contains("AAC", StringComparison.Ordinal))
            return "AAC";
        if (normalized.Contains("MP3", StringComparison.Ordinal) || normalized.Contains("LAYER 3", StringComparison.Ordinal) || normalized.Contains("LAYER III", StringComparison.Ordinal))
            return "MP3";
        if (normalized.Contains("PCM", StringComparison.Ordinal) || normalized.Contains("UNCOMPRESSED", StringComparison.Ordinal) || normalized.Contains("LINEAR", StringComparison.Ordinal))
            return "PCM";
        if (normalized.Contains("WINDOWS MEDIA", StringComparison.Ordinal) || normalized.Contains("WMA", StringComparison.Ordinal))
            return "WMA";

        return normalized;
    }
}
