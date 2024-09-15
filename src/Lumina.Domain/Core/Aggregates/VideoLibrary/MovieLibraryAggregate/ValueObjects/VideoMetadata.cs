#region ========================================================================= USING =====================================================================================
using ErrorOr;
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.Common.ValueObjects.Metadata;
using System;
using System.Collections.Generic;
using System.Diagnostics;
#endregion

namespace Lumina.Domain.Core.Aggregates.VideoLibrary.MovieLibraryAggregate.ValueObjects;

/// <summary>
/// Value Object for the video metadata of a media element.
/// </summary>
[DebuggerDisplay("{Title}")]
public class VideoMetadata : BaseMetadata
{
    #region ==================================================================== PROPERTIES =================================================================================
    /// <summary>
    /// Gets the duration of the video in seconds.
    /// </summary>
    public int DurationInSeconds { get; }

    /// <summary>
    /// Gets the resolution of the video.
    /// </summary>
    public string Resolution { get; } // TODO: should be implemented as a value object with width, height, aspet ratio, etc.

    /// <summary>
    /// Gets the frame rate of the video.
    /// </summary>
    public Optional<float> FrameRate { get; }

    /// <summary>
    /// Gets the video codec used.
    /// </summary>
    public Optional<string> VideoCodec { get; }

    /// <summary>
    /// Gets the audio codec used.
    /// </summary>
    public Optional<string> AudioCodec { get; }
    #endregion

    #region ====================================================================== CTOR =====================================================================================
    /// <summary>
    /// Initializes a new instance of the <see cref="VideoMetadata"/> class.
    /// </summary>
    /// <param name="title">The title of the video.</param>
    /// <param name="originalTitle">The optional original title of the video.</param>
    /// <param name="durationInSeconds">The duration of the video, in seconds.</param>
    /// <param name="resolution">The resolution of the video.</param>
    /// <param name="releaseInfo">The release information of the video.</param>
    /// <param name="description">The optional description of the video.</param>
    /// <param name="language">The language of the video.</param>
    /// <param name="originalLanguage">The optional original language of the video.</param>
    /// <param name="frameRate">The optional frame rate of the video.</param>
    /// <param name="videoCodec">The optional video codec used.</param>
    /// <param name="audioCodec">The optional audio codec used.</param>
    /// <param name="genres">The genres of the video.</param>
    /// <param name="tags">The tags associated with the video.</param>
    private VideoMetadata(
        string title, 
        Optional<string> originalTitle, 
        int durationInSeconds, 
        string resolution, 
        Optional<string> description,
        ReleaseInfo releaseInfo, 
        Optional<LanguageInfo> language, 
        Optional<LanguageInfo> originalLanguage, 
        Optional<float> frameRate, 
        Optional<string> videoCodec, 
        Optional<string> audioCodec,
        List<Genre> genres, 
        List<Tag> tags)
        : base(title, originalTitle, description, releaseInfo, genres, tags, language, originalLanguage)
    {
        DurationInSeconds = durationInSeconds;
        Resolution = resolution ?? throw new ArgumentNullException(nameof(resolution));
        FrameRate = frameRate;
        VideoCodec = videoCodec;
        AudioCodec = audioCodec;
    }
    #endregion

    #region ===================================================================== METHODS ===================================================================================
    /// <summary>
    /// Creates a new instance of the <see cref="VideoMetadata"/> class.
    /// </summary>
    /// <param name="title">The title of the video.</param>
    /// <param name="originalTitle">The optional original title of the video.</param>
    /// <param name="durationInSeconds">The duration of the video in seconds.</param>
    /// <param name="resolution">The resolution of the video.</param>
    /// <param name="description">The optional description of the video.</param>
    /// <param name="releaseInfo">The release information of the video.</param>
    /// <param name="language">The language of the video.</param>
    /// <param name="originalLanguage">The optional original language of the video.</param>
    /// <param name="frameRate">The optional frame rate of the video.</param>
    /// <param name="videoCodec">The optional video codec used.</param>
    /// <param name="audioCodec">The optional audio codec used.</param>
    /// <param name="genres">The genres of the video.</param>
    /// <param name="tags">The tags associated with the video.</param>
    /// <returns>
    /// An <see cref="ErrorOr{TValue}"/> containing either a successfully created <see cref="VideoMetadata"/>, or an error message.
    /// </returns>
    public static ErrorOr<VideoMetadata> Create(
        string title, 
        Optional<string> originalTitle, 
        int durationInSeconds, 
        string resolution, 
        Optional<string> description,
        ReleaseInfo releaseInfo, 
        Optional<LanguageInfo> language, 
        Optional<LanguageInfo> originalLanguage, 
        Optional<float> frameRate, 
        Optional<string> videoCodec, 
        Optional<string> audioCodec,
        List<Genre> genres, 
        List<Tag> tags)
    {
        return new VideoMetadata(title, originalTitle, durationInSeconds, resolution, description, releaseInfo,
            language, originalLanguage, frameRate, videoCodec, audioCodec, genres, tags);
    }

    /// <inheritdoc/>
    public override IEnumerable<object> GetEqualityComponents()
    {
        foreach (var component in base.GetEqualityComponents())
            yield return component;
        yield return DurationInSeconds;
        yield return Resolution;
        yield return FrameRate;
        yield return VideoCodec;
        yield return AudioCodec;
    }
    #endregion
}