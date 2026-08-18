#region ========================================================================= USING =====================================================================================
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.Common.ValueObjects.Metadata;
using Lumina.Domain.Core.BoundedContexts.AudioLibraryBoundedContext.AudioLibraryAggregate.ValueObjects;
using Lumina.Domain.Fixtures.Common.ValueObjects.Metadata;
using Lumina.Domain.Fixtures.Core.BoundedContexts.AudioLibraryBoundedContext.AudioLibraryAggregate.ValueObjects;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Domain.UnitTests.Core.BoundedContexts.AudioLibraryBoundedContext.AudioLibraryAggregate.ValueObjects;

/// <summary>
/// Contains unit tests for the <see cref="AudioMetadata"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class AudioMetadataTests
{
    private readonly GenreFixture _genreFixture = new();
    private readonly TagFixture _tagFixture = new();
    private readonly ReleaseInfoFixture _releaseInfoFixture = new();
    private readonly AudioMetadataFixture _audioMetadataFixture = new();

    [Fact]
    public void Create_WhenCalledWithValidValues_ShouldCreateMetadataWithAllPropertiesSet()
    {
        // Act
        Result<AudioMetadata> result = AudioMetadata.Create(
            "Abbey Road",
            Optional<string>.None(),
            durationInSeconds: 2826,
            sampleRate: 44100,
            channels: 2,
            _releaseInfoFixture.Create(),
            Optional<string>.Some("The last recorded album by the Beatles."),
            [_genreFixture.Create(name: "Rock")],
            [_tagFixture.Create(name: "classic")],
            Optional<LanguageInfo>.None(),
            Optional<LanguageInfo>.None(),
            Optional<int>.Some(16),
            Optional<string>.Some("PCM"),
            Optional<int>.Some(1411));

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal("Abbey Road", result.Value.Title);
        Assert.Equal(2826, result.Value.DurationInSeconds);
        Assert.Equal(44100, result.Value.SampleRate);
        Assert.Equal(2, result.Value.Channels);
        Assert.Equal("The last recorded album by the Beatles.", result.Value.Description.Value);
        Assert.Equal(16, result.Value.BitDepth.Value);
        Assert.Equal("PCM", result.Value.AudioCodec.Value);
        Assert.Equal(1411, result.Value.Bitrate.Value);
        Assert.Single(result.Value.Genres);
        Assert.Single(result.Value.Tags);
    }

    [Fact]
    public void Create_WhenCalledWithOptionalValuesAbsent_ShouldCreateMetadataWithoutThem()
    {
        // Act
        Result<AudioMetadata> result = AudioMetadata.Create(
            "Abbey Road",
            Optional<string>.None(),
            durationInSeconds: 2826,
            sampleRate: 44100,
            channels: 2,
            _releaseInfoFixture.Create(),
            Optional<string>.None(),
            [],
            [],
            Optional<LanguageInfo>.None(),
            Optional<LanguageInfo>.None(),
            Optional<int>.None(),
            Optional<string>.None(),
            Optional<int>.None());

        // Assert
        Assert.False(result.IsFailure);
        Assert.False(result.Value.BitDepth.HasValue);
        Assert.False(result.Value.AudioCodec.HasValue);
        Assert.False(result.Value.Bitrate.HasValue);
    }

    [Fact]
    public void Equals_WithSameValues_ShouldReturnTrue()
    {
        // Arrange
        ReleaseInfo releaseInfo = _releaseInfoFixture.Create();

        // Act
        AudioMetadata firstResult = _audioMetadataFixture.Create(
            title: "Abbey Road",
            originalTitle: Optional<string>.None(),
            durationInSeconds: 2826,
            sampleRate: 44100,
            channels: 2,
            releaseInfo: releaseInfo,
            description: Optional<string>.None(),
            genres: [],
            tags: [],
            language: Optional<LanguageInfo>.None(),
            originalLanguage: Optional<LanguageInfo>.None(),
            bitDepth: Optional<int>.None(),
            audioCodec: Optional<string>.None(),
            bitrate: Optional<int>.None());
        AudioMetadata secondResult = _audioMetadataFixture.Create(
            title: "Abbey Road",
            originalTitle: Optional<string>.None(),
            durationInSeconds: 2826,
            sampleRate: 44100,
            channels: 2,
            releaseInfo: releaseInfo,
            description: Optional<string>.None(),
            genres: [],
            tags: [],
            language: Optional<LanguageInfo>.None(),
            originalLanguage: Optional<LanguageInfo>.None(),
            bitDepth: Optional<int>.None(),
            audioCodec: Optional<string>.None(),
            bitrate: Optional<int>.None());

        // Assert
        Assert.Equal(firstResult, secondResult);
    }

    [Fact]
    public void Equals_WithDifferentSampleRate_ShouldReturnFalse()
    {
        // Arrange
        ReleaseInfo releaseInfo = _releaseInfoFixture.Create();

        // Act
        AudioMetadata firstResult = _audioMetadataFixture.Create(
            title: "Abbey Road",
            originalTitle: Optional<string>.None(),
            durationInSeconds: 2826,
            sampleRate: 44100,
            channels: 2,
            releaseInfo: releaseInfo,
            description: Optional<string>.None(),
            genres: [],
            tags: [],
            language: Optional<LanguageInfo>.None(),
            originalLanguage: Optional<LanguageInfo>.None(),
            bitDepth: Optional<int>.None(),
            audioCodec: Optional<string>.None(),
            bitrate: Optional<int>.None());
        AudioMetadata secondResult = _audioMetadataFixture.Create(
            title: "Abbey Road",
            originalTitle: Optional<string>.None(),
            durationInSeconds: 2826,
            sampleRate: 48000,
            channels: 2,
            releaseInfo: releaseInfo,
            description: Optional<string>.None(),
            genres: [],
            tags: [],
            language: Optional<LanguageInfo>.None(),
            originalLanguage: Optional<LanguageInfo>.None(),
            bitDepth: Optional<int>.None(),
            audioCodec: Optional<string>.None(),
            bitrate: Optional<int>.None());

        // Assert
        Assert.NotEqual(firstResult, secondResult);
    }
}
