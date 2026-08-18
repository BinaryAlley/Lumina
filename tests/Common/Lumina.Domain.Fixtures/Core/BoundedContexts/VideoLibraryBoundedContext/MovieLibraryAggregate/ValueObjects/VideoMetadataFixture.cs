#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.Common.ValueObjects.Metadata;
using Lumina.Domain.Core.BoundedContexts.VideoLibraryBoundedContext.MovieLibraryAggregate.ValueObjects;
using Lumina.Domain.Fixtures.Common.ValueObjects.Metadata;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Domain.Fixtures.Core.BoundedContexts.VideoLibraryBoundedContext.MovieLibraryAggregate.ValueObjects;

/// <summary>
/// Fixture class for the <see cref="VideoMetadata"/> domain value object.
/// </summary>
[ExcludeFromCodeCoverage]
public class VideoMetadataFixture
{
    private readonly Faker _faker = new();
    private readonly GenreFixture _genreFixture = new();
    private readonly TagFixture _tagFixture = new();
    private readonly ReleaseInfoFixture _releaseInfoFixture = new();
    private readonly LanguageInfoFixture _languageInfoFixture = new();

    /// <summary>
    /// Creates a random valid <see cref="VideoMetadata"/>.
    /// </summary>
    /// <param name="title">Optional. The title of the video.</param>
    /// <param name="originalTitle">Optional. The original title of the video.</param>
    /// <param name="durationInSeconds">Optional. The duration of the video in seconds.</param>
    /// <param name="resolution">Optional. The resolution of the video.</param>
    /// <param name="description">Optional. The description of the video.</param>
    /// <param name="releaseInfo">Optional. The release information of the video.</param>
    /// <param name="language">Optional. The language of the video.</param>
    /// <param name="originalLanguage">Optional. The original language of the video.</param>
    /// <param name="frameRate">Optional. The frame rate of the video.</param>
    /// <param name="videoCodec">Optional. The video codec used.</param>
    /// <param name="audioCodec">Optional. The audio codec used.</param>
    /// <param name="genres">Optional. The genres of the video.</param>
    /// <param name="tags">Optional. The tags associated with the video.</param>
    /// <returns>The created <see cref="VideoMetadata"/>.</returns>
    public VideoMetadata Create(
        string? title = null,
        Optional<string>? originalTitle = null,
        int? durationInSeconds = null,
        string? resolution = null,
        Optional<string>? description = null,
        ReleaseInfo? releaseInfo = null,
        Optional<LanguageInfo>? language = null,
        Optional<LanguageInfo>? originalLanguage = null,
        Optional<float>? frameRate = null,
        Optional<string>? videoCodec = null,
        Optional<string>? audioCodec = null,
        List<Genre>? genres = null,
        List<Tag>? tags = null)
    {
        return VideoMetadata.Create(
            title ?? _faker.Commerce.ProductName(),
            originalTitle ?? Optional<string>.None(),
            durationInSeconds ?? _faker.Random.Int(60, 18000),
            resolution ?? "1920x1080",
            description ?? Optional<string>.Some(_faker.Lorem.Sentence()),
            releaseInfo ?? _releaseInfoFixture.Create(),
            language ?? Optional<LanguageInfo>.Some(_languageInfoFixture.Create()),
            originalLanguage ?? Optional<LanguageInfo>.None(),
            frameRate ?? Optional<float>.Some(24),
            videoCodec ?? Optional<string>.Some("H.264"),
            audioCodec ?? Optional<string>.Some("AAC"),
            genres ?? [_genreFixture.Create(), _genreFixture.Create()],
            tags ?? [_tagFixture.Create(), _tagFixture.Create()]).Value;
    }

    /// <summary>
    /// Creates multiple <see cref="VideoMetadata"/> instances with randomized test data.
    /// </summary>
    /// <param name="count">Number of instances to create.</param>
    /// <returns>List of configured <see cref="VideoMetadata"/> instances.</returns>
    public List<VideoMetadata> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
