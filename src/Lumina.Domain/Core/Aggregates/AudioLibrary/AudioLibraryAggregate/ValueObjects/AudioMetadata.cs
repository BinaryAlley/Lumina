#region ========================================================================= USING =====================================================================================
using ErrorOr;
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.Common.ValueObjects.Metadata;
using System.Collections.Generic;
using System.Diagnostics;
#endregion

namespace Lumina.Domain.Core.Aggregates.AudioLibrary.AudioLibraryAggregate.ValueObjects;

/// <summary>
/// Value Object for the audio metadata of a media element.
/// </summary>
[DebuggerDisplay("{Title}")]
public class AudioMetadata : BaseMetadata
{
    #region ==================================================================== PROPERTIES =================================================================================
    /// <summary>
    /// Gets the duration of the video in seconds.
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
    #endregion

    #region ====================================================================== CTOR =====================================================================================
    /// <summary>
    /// Initializes a new instance of the <see cref="AudioMetadata"/> class.
    /// </summary>
    /// <param name="title">The title of the audio.</param>
    /// <param name="originalTitle">The optional original title of the video.</param>
    /// <param name="durationInSeconds">The duration of the audio, in seconds.</param>
    /// <param name="sampleRate">The sample rate of the audio, in Hz.</param>
    /// <param name="channels">The number of audio channels.</param>
    /// <param name="releaseInfo">The release information of the audio.</param>
    /// <param name="description">The description of the audio.</param>
    /// <param name="language">The language of the audio.</param>
    /// <param name="originalLanguage">The optional original language of the video.</param>
    /// <param name="bitDepth">The bit depth of the audio.</param>
    /// <param name="audioCodec">The audio codec used.</param>
    /// <param name="genres">The genres of the audio.</param>
    /// <param name="tags">The tags associated with the audio.</param>
    public AudioMetadata(
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
        Optional<int> bitrate)
        : base(title, originalTitle, description, releaseInfo, genres, tags, language, originalLanguage)
    {
        DurationInSeconds = durationInSeconds;
        SampleRate = sampleRate;
        Channels = channels;
        BitDepth = bitDepth;
        AudioCodec = audioCodec;
        Bitrate = bitrate;
    }
    #endregion

    #region ===================================================================== METHODS ===================================================================================
    /// <summary>
    /// Creates a new instance of the <see cref="AudioMetadata"/> class.
    /// </summary>
    /// <param name="title">The title of the audio.</param>
    /// <param name="originalTitle">The optional original title of the video.</param>
    /// <param name="durationInSeconds">The duration of the audio, in seconds.</param>
    /// <param name="sampleRate">The sample rate of the audio, in Hz.</param>
    /// <param name="channels">The number of audio channels.</param>
    /// <param name="release">The release information of the audio.</param>
    /// <param name="description">The description of the audio.</param>
    /// <param name="language">The language of the audio.</param>
    /// <param name="originalLanguage">The optional original language of the video.</param>
    /// <param name="bitDepth">The bit depth of the audio.</param>
    /// <param name="audioCodec">The audio codec used.</param>
    /// <param name="genres">The genres of the audio.</param>
    /// <param name="tags">The tags associated with the audio.</param>
    /// <returns>
    /// An <see cref="ErrorOr{TValue}"/> containing either a successfully created <see cref="AudioMetadata"/>, or an error message.
    /// </returns>
    public static ErrorOr<AudioMetadata> Create(
        string title, 
        Optional<string> originalTitle, 
        int durationInSeconds, 
        int sampleRate, 
        int channels, 
        ReleaseInfo release, 
        Optional<string> 
        description,
        List<Genre> genres, 
        List<Tag> tags, 
        Optional<LanguageInfo> language, 
        Optional<LanguageInfo> originalLanguage, 
        Optional<int> bitDepth, 
        Optional<string> audioCodec, 
        Optional<int> bitrate)
    {
        return new AudioMetadata(
            title, 
            originalTitle, 
            durationInSeconds, 
            sampleRate, 
            channels, 
            release, 
            description, 
            genres, 
            tags,
            language, 
            originalLanguage, 
            bitDepth, 
            audioCodec, 
            bitrate);
    }

    /// <inheritdoc/>
    public override IEnumerable<object> GetEqualityComponents()
    {
        foreach (var component in base.GetEqualityComponents())
            yield return component;
        yield return DurationInSeconds;
        yield return SampleRate;
        yield return Channels;
        yield return BitDepth;
        yield return AudioCodec;
        yield return Bitrate;
    }
    #endregion
}