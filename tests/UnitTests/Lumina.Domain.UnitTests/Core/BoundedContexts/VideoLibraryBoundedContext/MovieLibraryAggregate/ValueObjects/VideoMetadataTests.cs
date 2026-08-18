#region ========================================================================= USING =====================================================================================
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.Common.ValueObjects.Metadata;
using Lumina.Domain.Core.BoundedContexts.VideoLibraryBoundedContext.MovieLibraryAggregate.ValueObjects;
using Lumina.Domain.Fixtures.Common.ValueObjects.Metadata;
using Lumina.Domain.Fixtures.Core.BoundedContexts.VideoLibraryBoundedContext.MovieLibraryAggregate.ValueObjects;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Domain.UnitTests.Core.BoundedContexts.VideoLibraryBoundedContext.MovieLibraryAggregate.ValueObjects;

/// <summary>
/// Contains unit tests for the <see cref="VideoMetadata"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class VideoMetadataTests
{
    private readonly GenreFixture _genreFixture = new();
    private readonly TagFixture _tagFixture = new();
    private readonly ReleaseInfoFixture _releaseInfoFixture = new();
    private readonly LanguageInfoFixture _languageInfoFixture = new();
    private readonly VideoMetadataFixture _videoMetadataFixture = new();

    [Fact]
    public void Create_WhenCalledWithValidValues_ShouldCreateMetadataWithAllPropertiesSet()
    {
        // Act
        Result<VideoMetadata> result = VideoMetadata.Create(
            "Inception",
            Optional<string>.Some("Inception (Original)"),
            durationInSeconds: 8880,
            "1920x1080",
            Optional<string>.Some("A thief who steals corporate secrets."),
            _releaseInfoFixture.Create(),
            Optional<LanguageInfo>.Some(_languageInfoFixture.Create()),
            Optional<LanguageInfo>.None(),
            Optional<float>.Some(24),
            Optional<string>.Some("H.264"),
            Optional<string>.Some("AAC"),
            [_genreFixture.Create(name: "Action")],
            [_tagFixture.Create(name: "heist")]);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal("Inception", result.Value.Title);
        Assert.Equal("Inception (Original)", result.Value.OriginalTitle.Value);
        Assert.Equal(8880, result.Value.DurationInSeconds);
        Assert.Equal("1920x1080", result.Value.Resolution);
        Assert.Equal("A thief who steals corporate secrets.", result.Value.Description.Value);
        Assert.Equal(24, result.Value.FrameRate.Value);
        Assert.Equal("H.264", result.Value.VideoCodec.Value);
        Assert.Equal("AAC", result.Value.AudioCodec.Value);
        Assert.Single(result.Value.Genres);
        Assert.Single(result.Value.Tags);
        Assert.True(result.Value.Language.HasValue);
        Assert.False(result.Value.OriginalLanguage.HasValue);
    }

    [Fact]
    public void Create_WhenResolutionIsNull_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => VideoMetadata.Create(
            "Inception",
            Optional<string>.None(),
            durationInSeconds: 8880,
            null!,
            Optional<string>.None(),
            _releaseInfoFixture.Create(),
            Optional<LanguageInfo>.None(),
            Optional<LanguageInfo>.None(),
            Optional<float>.None(),
            Optional<string>.None(),
            Optional<string>.None(),
            [],
            []));
    }

    [Fact]
    public void Equals_WithSameValues_ShouldReturnTrue()
    {
        // Arrange
        ReleaseInfo releaseInfo = _releaseInfoFixture.Create();
        VideoMetadata first = _videoMetadataFixture.Create(
            title: "Inception",
            originalTitle: Optional<string>.None(),
            durationInSeconds: 8880,
            resolution: "1920x1080",
            description: Optional<string>.None(),
            releaseInfo: releaseInfo,
            language: Optional<LanguageInfo>.None(),
            originalLanguage: Optional<LanguageInfo>.None(),
            frameRate: Optional<float>.None(),
            videoCodec: Optional<string>.None(),
            audioCodec: Optional<string>.None(),
            genres: [],
            tags: []);
        VideoMetadata second = _videoMetadataFixture.Create(
            title: "Inception",
            originalTitle: Optional<string>.None(),
            durationInSeconds: 8880,
            resolution: "1920x1080",
            description: Optional<string>.None(),
            releaseInfo: releaseInfo,
            language: Optional<LanguageInfo>.None(),
            originalLanguage: Optional<LanguageInfo>.None(),
            frameRate: Optional<float>.None(),
            videoCodec: Optional<string>.None(),
            audioCodec: Optional<string>.None(),
            genres: [],
            tags: []);

        // Act
        bool result = first.Equals(second);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void Equals_WithDifferentDuration_ShouldReturnFalse()
    {
        // Arrange
        ReleaseInfo releaseInfo = _releaseInfoFixture.Create();
        VideoMetadata first = _videoMetadataFixture.Create(
            title: "Inception",
            originalTitle: Optional<string>.None(),
            durationInSeconds: 8880,
            resolution: "1920x1080",
            description: Optional<string>.None(),
            releaseInfo: releaseInfo,
            language: Optional<LanguageInfo>.None(),
            originalLanguage: Optional<LanguageInfo>.None(),
            frameRate: Optional<float>.None(),
            videoCodec: Optional<string>.None(),
            audioCodec: Optional<string>.None(),
            genres: [],
            tags: []);
        VideoMetadata second = _videoMetadataFixture.Create(
            title: "Inception",
            originalTitle: Optional<string>.None(),
            durationInSeconds: 8881,
            resolution: "1920x1080",
            description: Optional<string>.None(),
            releaseInfo: releaseInfo,
            language: Optional<LanguageInfo>.None(),
            originalLanguage: Optional<LanguageInfo>.None(),
            frameRate: Optional<float>.None(),
            videoCodec: Optional<string>.None(),
            audioCodec: Optional<string>.None(),
            genres: [],
            tags: []);

        // Act
        bool result = first.Equals(second);

        // Assert
        Assert.False(result);
    }
}
