#region ========================================================================= USING =====================================================================================
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.Common.ValueObjects.Metadata;
using System.Collections.Generic;
using System.Diagnostics;
#endregion

namespace Lumina.Domain.Core.BoundedContexts.AudioLibraryBoundedContext.Common.ValueObjects;

/// <summary>
/// Value Object for the audio metadata of a media element.
/// </summary>
[DebuggerDisplay("{Title}")]
public class AudioMetadata : BaseMetadata
{
    /// <summary>
    /// Gets the duration of the audio in seconds.
    /// </summary>
    public int DurationInSeconds { get; }

    /// <summary>
    /// Gets the sample rate of the audio in Hz.
    /// </summary>
    public int SampleRate { get; }

    /// <summary>
    /// Gets the number of audio channels.
    /// </summary>
    public int Channels { get; }

    /// <summary>
    /// Gets the bit depth of the audio.
    /// </summary>
    public Optional<int> BitDepth { get; }

    /// <summary>
    /// Gets the audio codec used.
    /// </summary>
    public Optional<string> AudioCodec { get; }

    /// <summary>
    /// Gets the bitrate of the audio in kbps.
    /// </summary>
    public Optional<int> Bitrate { get; }

    /// <summary>
    /// Gets the AcoustID fingerprint identifier of the audio file, if applicable.
    /// </summary>
    public Optional<string> AcoustId { get; }

    /// <summary>
    /// Gets the ReplayGain track gain in decibels, if applicable.
    /// </summary>
    public Optional<decimal> ReplayGainTrackGain { get; }

    /// <summary>
    /// Gets the ReplayGain track peak, if applicable.
    /// </summary>
    public Optional<decimal> ReplayGainTrackPeak { get; }

    /// <summary>
    /// Gets the ReplayGain album gain in decibels, if applicable.
    /// </summary>
    public Optional<decimal> ReplayGainAlbumGain { get; }

    /// <summary>
    /// Gets the ReplayGain album peak, if applicable.
    /// </summary>
    public Optional<decimal> ReplayGainAlbumPeak { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="AudioMetadata"/> class.
    /// </summary>
    /// <param name="title">The title of the audio.</param>
    /// <param name="originalTitle">The optional original title of the audio.</param>
    /// <param name="durationInSeconds">The duration of the audio, in seconds.</param>
    /// <param name="sampleRate">The sample rate of the audio, in Hz.</param>
    /// <param name="channels">The number of audio channels.</param>
    /// <param name="releaseInfo">The release information of the audio.</param>
    /// <param name="description">The description of the audio.</param>
    /// <param name="language">The language of the audio.</param>
    /// <param name="originalLanguage">The optional original language of the audio.</param>
    /// <param name="bitDepth">The bit depth of the audio.</param>
    /// <param name="audioCodec">The audio codec used.</param>
    /// <param name="genres">The genres of the audio.</param>
    /// <param name="tags">The tags associated with the audio.</param>
    /// <param name="bitrate">The bitrate of the audio in kbps.</param>
    /// <param name="acoustId">The AcoustID fingerprint identifier of the audio file.</param>
    /// <param name="replayGainTrackGain">The ReplayGain track gain in decibels.</param>
    /// <param name="replayGainTrackPeak">The ReplayGain track peak.</param>
    /// <param name="replayGainAlbumGain">The ReplayGain album gain in decibels.</param>
    /// <param name="replayGainAlbumPeak">The ReplayGain album peak.</param>
    private AudioMetadata(
        string title,
        Optional<string> originalTitle,
        int durationInSeconds,
        int sampleRate,
        int channels,
        ReleaseInfo releaseInfo,
        Optional<string> description,
        List<Genre> genres,
        List<Tag> tags,
        Optional<LanguageInfo> language,
        Optional<LanguageInfo> originalLanguage,
        Optional<int> bitDepth,
        Optional<string> audioCodec,
        Optional<int> bitrate,
        Optional<string> acoustId,
        Optional<decimal> replayGainTrackGain,
        Optional<decimal> replayGainTrackPeak,
        Optional<decimal> replayGainAlbumGain,
        Optional<decimal> replayGainAlbumPeak)
        : base(title, originalTitle, description, releaseInfo, genres, tags, language, originalLanguage)
    {
        DurationInSeconds = durationInSeconds;
        SampleRate = sampleRate;
        Channels = channels;
        BitDepth = bitDepth;
        AudioCodec = audioCodec;
        Bitrate = bitrate;
        AcoustId = acoustId;
        ReplayGainTrackGain = replayGainTrackGain;
        ReplayGainTrackPeak = replayGainTrackPeak;
        ReplayGainAlbumGain = replayGainAlbumGain;
        ReplayGainAlbumPeak = replayGainAlbumPeak;
    }

    /// <summary>
    /// Creates a new instance of the <see cref="AudioMetadata"/> class.
    /// </summary>
    /// <param name="title">The title of the audio.</param>
    /// <param name="originalTitle">The optional original title of the audio.</param>
    /// <param name="durationInSeconds">The duration of the audio, in seconds.</param>
    /// <param name="sampleRate">The sample rate of the audio, in Hz.</param>
    /// <param name="channels">The number of audio channels.</param>
    /// <param name="releaseInfo">The release information of the audio.</param>
    /// <param name="description">The description of the audio.</param>
    /// <param name="language">The language of the audio.</param>
    /// <param name="originalLanguage">The optional original language of the audio.</param>
    /// <param name="bitDepth">The bit depth of the audio.</param>
    /// <param name="audioCodec">The audio codec used.</param>
    /// <param name="genres">The genres of the audio.</param>
    /// <param name="tags">The tags associated with the audio.</param>
    /// <param name="bitrate">The bitrate of the audio in kbps.</param>
    /// <param name="acoustId">The AcoustID fingerprint identifier of the audio file.</param>
    /// <param name="replayGainTrackGain">The ReplayGain track gain in decibels.</param>
    /// <param name="replayGainTrackPeak">The ReplayGain track peak.</param>
    /// <param name="replayGainAlbumGain">The ReplayGain album gain in decibels.</param>
    /// <param name="replayGainAlbumPeak">The ReplayGain album peak.</param>
    /// <returns>
    /// An <see cref="Result{TValue}"/> containing either a successfully created <see cref="AudioMetadata"/>, or an error message.
    /// </returns>
    public static Result<AudioMetadata> Create(
        string title,
        Optional<string> originalTitle,
        int durationInSeconds,
        int sampleRate,
        int channels,
        ReleaseInfo releaseInfo,
        Optional<string> description,
        List<Genre> genres,
        List<Tag> tags,
        Optional<LanguageInfo> language,
        Optional<LanguageInfo> originalLanguage,
        Optional<int> bitDepth,
        Optional<string> audioCodec,
        Optional<int> bitrate,
        Optional<string> acoustId,
        Optional<decimal> replayGainTrackGain,
        Optional<decimal> replayGainTrackPeak,
        Optional<decimal> replayGainAlbumGain,
        Optional<decimal> replayGainAlbumPeak)
    {
        return new AudioMetadata(
            title,
            originalTitle,
            durationInSeconds,
            sampleRate,
            channels,
            releaseInfo,
            description,
            genres,
            tags,
            language,
            originalLanguage,
            bitDepth,
            audioCodec,
            bitrate,
            acoustId,
            replayGainTrackGain,
            replayGainTrackPeak,
            replayGainAlbumGain,
            replayGainAlbumPeak);
    }

    /// <summary>
    /// Gets the list of items that define equality of the object.
    /// </summary>
    /// <returns>A list of items defining the equality.</returns>
    public override IEnumerable<object> GetEqualityComponents()
    {
        foreach (object component in base.GetEqualityComponents())
            yield return component;
        yield return DurationInSeconds;
        yield return SampleRate;
        yield return Channels;
        yield return BitDepth;
        yield return AudioCodec;
        yield return Bitrate;
        yield return AcoustId;
        yield return ReplayGainTrackGain;
        yield return ReplayGainTrackPeak;
        yield return ReplayGainAlbumGain;
        yield return ReplayGainAlbumPeak;
    }
}
