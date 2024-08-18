#region ========================================================================= USING =====================================================================================
using ErrorOr;
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.Common.ValueObjects.Media;
using System.Diagnostics;
#endregion

namespace Lumina.Domain.Common.ValueObjects.Metadata;

/// <summary>
/// Value Object for the rating of a media element.
/// </summary>
[DebuggerDisplay("{Value}/{MaxValue}")]
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
    public string Resolution { get; }

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

    /// <summary>
    /// Gets the list of actors starring in this video.
    /// </summary>
    public IReadOnlyList<MediaContributor> Contributors { get; }
    #endregion

    #region ====================================================================== CTOR =====================================================================================
    /// <summary>
    /// Overload C-tor.
    /// </summary>
    /// <param name="title">The title of the video.</param>
    /// <param name="originalTitle">The optional original title of the video.</param>
    /// <param name="durationInSeconds">The duration of the video in seconds.</param>
    /// <param name="resolution">The resolution of the video.</param>
    /// <param name="release">The release information of the video.</param>
    /// <param name="description">The optional description of the video.</param>
    /// <param name="language">The language of the video.</param>
    /// <param name="originalLanguage">The optional original language of the video.</param>
    /// <param name="frameRate">The optional frame rate of the video.</param>
    /// <param name="videoCodec">The optional video codec used.</param>
    /// <param name="audioCodec">The optional audio codec used.</param>
    /// <param name="genres">The genres of the video.</param>
    /// <param name="tags">The tags associated with the video.</param>
    /// <param name="contributors">The list of contributors for this video.</param>
    private VideoMetadata(string title, Optional<string> originalTitle, int durationInSeconds, string resolution, ReleaseInfo release, Optional<string> description,
        Optional<LanguageInfo> language, Optional<LanguageInfo> originalLanguage, Optional<float> frameRate, Optional<string> videoCodec, Optional<string> audioCodec,
        IEnumerable<Genre>? genres = null, IEnumerable<Tag>? tags = null, IEnumerable<MediaContributor>? contributors = null)
        : base(title, originalTitle, release, description, genres, tags, language, originalLanguage)
    {
        DurationInSeconds = durationInSeconds;
        Resolution = resolution ?? throw new ArgumentNullException(nameof(resolution));
        FrameRate = frameRate;
        VideoCodec = videoCodec;
        AudioCodec = audioCodec;
        Contributors = contributors?.ToList().AsReadOnly() ?? new List<MediaContributor>().AsReadOnly();
    }
    #endregion

    #region ===================================================================== METHODS ===================================================================================
    /// <summary>
    /// Creates a new instance of <see cref="VideoMetadata"/>.
    /// </summary>
    /// <param name="title">The title of the video.</param>
    /// <param name="originalTitle">The optional original title of the video.</param>
    /// <param name="durationInSeconds">The duration of the video in seconds.</param>
    /// <param name="resolution">The resolution of the video.</param>
    /// <param name="release">The release information of the video.</param>
    /// <param name="description">The optional description of the video.</param>
    /// <param name="language">The language of the video.</param>
    /// <param name="originalLanguage">The optional original language of the video.</param>
    /// <param name="frameRate">The optional frame rate of the video.</param>
    /// <param name="videoCodec">The optional video codec used.</param>
    /// <param name="audioCodec">The optional audio codec used.</param>
    /// <param name="genres">The genres of the video.</param>
    /// <param name="tags">The tags associated with the video.</param>
    /// <param name="contributors">The list of contributors for this video.</param>
    /// <returns>
    /// An <see cref="ErrorOr{T}"/> containing either a successfully created <see cref="VideoMetadata"/> or an error message.
    /// </returns>
    public static ErrorOr<VideoMetadata> Create(string title, Optional<string> originalTitle, int durationInSeconds, string resolution, ReleaseInfo release, Optional<string> description,
        Optional<LanguageInfo> language, Optional<LanguageInfo> originalLanguage, Optional<float> frameRate, Optional<string> videoCodec, Optional<string> audioCodec,
        IEnumerable<Genre>? genres = null, IEnumerable<Tag>? tags = null, IEnumerable<MediaContributor>? contributors = null)
    {
        return new VideoMetadata(title, originalTitle, durationInSeconds, resolution, release, description,
            language, originalLanguage, frameRate, videoCodec, audioCodec, genres, tags, contributors);
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
        foreach (var contributor in Contributors)
            yield return contributor;
    }
    #endregion
}