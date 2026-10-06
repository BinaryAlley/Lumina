#region ========================================================================= USING =====================================================================================
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.Common.ValueObjects.Metadata;
using Lumina.Domain.Core.BoundedContexts.AudioLibraryBoundedContext.Common.ValueObjects;
using Lumina.Domain.Fixtures.Common.ValueObjects.Metadata;
using Lumina.Domain.Fixtures.Core.BoundedContexts.AudioLibraryBoundedContext.Common.ValueObjects;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Domain.UnitTests.Core.BoundedContexts.AudioLibraryBoundedContext.Common.ValueObjects;

/// <summary>
/// Contains unit tests for the <see cref="AudioMetadata"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class AudioMetadataTests
{
    private readonly GenreFixture _genreFixture = new();
    private readonly TagFixture _tagFixture = new();
    private readonly ReleaseInfoFixture _releaseInfoFixture = new();
    private readonly LanguageInfoFixture _languageInfoFixture = new();
    private readonly AudioMetadataFixture _audioMetadataFixture = new();

    [Fact]
    public void Create_WhenCalledWithValidValues_ShouldCreateMetadataWithAllPropertiesSet()
    {
        // Act
        Result<AudioMetadata> result = AudioMetadata.Create(
            "Bohemian Rhapsody",
            Optional<string>.None(),
            durationInSeconds: 2826,
            sampleRate: 44100,
            channels: 2,
            _releaseInfoFixture.Create(),
            Optional<string>.Some("A song by the British rock band Queen."),
            [_genreFixture.Create(name: "Rock")],
            [_tagFixture.Create(name: "classic")],
            Optional<LanguageInfo>.None(),
            Optional<LanguageInfo>.None(),
            Optional<int>.Some(16),
            Optional<string>.Some("PCM"),
            Optional<int>.Some(1411),
            Optional<string>.Some("f0e9c1a2-0000-0000-0000-000000000000"),
            Optional<decimal>.Some(-6.53m),
            Optional<decimal>.Some(0.988m),
            Optional<decimal>.Some(-7.01m),
            Optional<decimal>.Some(1.001m));

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal("Bohemian Rhapsody", result.Value.Title);
        Assert.Equal(2826, result.Value.DurationInSeconds);
        Assert.Equal(44100, result.Value.SampleRate);
        Assert.Equal(2, result.Value.Channels);
        Assert.Equal("A song by the British rock band Queen.", result.Value.Description.Value);
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
            "Bohemian Rhapsody",
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
            Optional<int>.None(),
            Optional<string>.None(),
            Optional<decimal>.None(),
            Optional<decimal>.None(),
            Optional<decimal>.None(),
            Optional<decimal>.None());

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
            title: "Bohemian Rhapsody",
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
            title: "Bohemian Rhapsody",
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
            title: "Bohemian Rhapsody",
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
            title: "Bohemian Rhapsody",
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

    [Fact]
    public void Create_WhenOptionalBaseValuesArePresent_ShouldSetThem()
    {
        // Arrange
        LanguageInfo language = _languageInfoFixture.Create(languageCode: "en", languageName: "English");
        LanguageInfo originalLanguage = _languageInfoFixture.Create(languageCode: "fr", languageName: "French");

        // Act
        Result<AudioMetadata> result = AudioMetadata.Create(
            "Bohemian Rhapsody",
            Optional<string>.Some("Bohemian Rhapsody (original)"),
            durationInSeconds: 355,
            sampleRate: 44100,
            channels: 2,
            _releaseInfoFixture.Create(),
            Optional<string>.Some("A song by the British rock band Queen."),
            [_genreFixture.Create(name: "Rock")],
            [_tagFixture.Create(name: "classic")],
            Optional<LanguageInfo>.Some(language),
            Optional<LanguageInfo>.Some(originalLanguage),
            Optional<int>.Some(16),
            Optional<string>.Some("PCM"),
            Optional<int>.Some(1411),
            Optional<string>.Some("f0e9c1a2-0000-0000-0000-000000000000"),
            Optional<decimal>.Some(-6.53m),
            Optional<decimal>.Some(0.988m),
            Optional<decimal>.Some(-7.01m),
            Optional<decimal>.Some(1.001m));

        // Assert
        Assert.False(result.IsFailure);
        AudioMetadata metadata = result.Value;
        Assert.Equal("Bohemian Rhapsody", metadata.Title);
        Assert.Equal(Optional<string>.Some("Bohemian Rhapsody (original)"), metadata.OriginalTitle);
        Assert.Equal(Optional<string>.Some("A song by the British rock band Queen."), metadata.Description);
        Assert.Equal(Optional<LanguageInfo>.Some(language), metadata.Language);
        Assert.Equal(Optional<LanguageInfo>.Some(originalLanguage), metadata.OriginalLanguage);
        Assert.Single(metadata.Genres);
        Assert.Single(metadata.Tags);
    }

    [Fact]
    public void Create_WhenCollectionsAreEmpty_ShouldCreateMetadataWithEmptyCollections()
    {
        // Act
        Result<AudioMetadata> result = AudioMetadata.Create(
            "Bohemian Rhapsody",
            Optional<string>.None(),
            durationInSeconds: 355,
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
            Optional<int>.None(),
            Optional<string>.None(),
            Optional<decimal>.None(),
            Optional<decimal>.None(),
            Optional<decimal>.None(),
            Optional<decimal>.None());

        // Assert
        Assert.False(result.IsFailure);
        Assert.Empty(result.Value.Genres);
        Assert.Empty(result.Value.Tags);
    }

    [Fact]
    public void Equals_WithDifferentTitle_ShouldReturnFalse()
    {
        // Arrange
        ReleaseInfo releaseInfo = _releaseInfoFixture.Create();

        // Act
        AudioMetadata firstResult = _audioMetadataFixture.Create(
            title: "Bohemian Rhapsody",
            originalTitle: Optional<string>.None(),
            durationInSeconds: 355,
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
            title: "Love of My Life",
            originalTitle: Optional<string>.None(),
            durationInSeconds: 355,
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
        Assert.NotEqual(firstResult, secondResult);
    }

    [Fact]
    public void Equals_WithDifferentAudioCodec_ShouldReturnFalse()
    {
        // Arrange
        ReleaseInfo releaseInfo = _releaseInfoFixture.Create();

        // Act
        AudioMetadata firstResult = _audioMetadataFixture.Create(
            title: "Bohemian Rhapsody",
            originalTitle: Optional<string>.None(),
            durationInSeconds: 355,
            sampleRate: 44100,
            channels: 2,
            releaseInfo: releaseInfo,
            description: Optional<string>.None(),
            genres: [],
            tags: [],
            language: Optional<LanguageInfo>.None(),
            originalLanguage: Optional<LanguageInfo>.None(),
            bitDepth: Optional<int>.None(),
            audioCodec: Optional<string>.Some("PCM"),
            bitrate: Optional<int>.None());
        AudioMetadata secondResult = _audioMetadataFixture.Create(
            title: "Bohemian Rhapsody",
            originalTitle: Optional<string>.None(),
            durationInSeconds: 355,
            sampleRate: 44100,
            channels: 2,
            releaseInfo: releaseInfo,
            description: Optional<string>.None(),
            genres: [],
            tags: [],
            language: Optional<LanguageInfo>.None(),
            originalLanguage: Optional<LanguageInfo>.None(),
            bitDepth: Optional<int>.None(),
            audioCodec: Optional<string>.Some("FLAC"),
            bitrate: Optional<int>.None());

        // Assert
        Assert.NotEqual(firstResult, secondResult);
    }

    [Fact]
    public void Equals_WithDifferentGenres_ShouldReturnFalse()
    {
        // Arrange
        ReleaseInfo releaseInfo = _releaseInfoFixture.Create();

        // Act
        AudioMetadata firstResult = _audioMetadataFixture.Create(
            title: "Bohemian Rhapsody",
            originalTitle: Optional<string>.None(),
            durationInSeconds: 355,
            sampleRate: 44100,
            channels: 2,
            releaseInfo: releaseInfo,
            description: Optional<string>.None(),
            genres: [_genreFixture.Create(name: "Rock")],
            tags: [],
            language: Optional<LanguageInfo>.None(),
            originalLanguage: Optional<LanguageInfo>.None(),
            bitDepth: Optional<int>.None(),
            audioCodec: Optional<string>.None(),
            bitrate: Optional<int>.None());
        AudioMetadata secondResult = _audioMetadataFixture.Create(
            title: "Bohemian Rhapsody",
            originalTitle: Optional<string>.None(),
            durationInSeconds: 355,
            sampleRate: 44100,
            channels: 2,
            releaseInfo: releaseInfo,
            description: Optional<string>.None(),
            genres: [_genreFixture.Create(name: "Jazz")],
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
