#region ========================================================================= USING =====================================================================================
using DomainErrors = Lumina.Domain.Common.Errors.Errors;
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.Common.ValueObjects.Metadata;
using Lumina.Domain.Core.BoundedContexts.AudioLibraryBoundedContext.MusicLibraryAggregate.ValueObjects;
using Lumina.Domain.Fixtures.Common.ValueObjects.Metadata;
using Lumina.Domain.Fixtures.Core.BoundedContexts.AudioLibraryBoundedContext.MusicLibraryAggregate.ValueObjects;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Domain.UnitTests.Core.BoundedContexts.AudioLibraryBoundedContext.MusicLibraryAggregate.ValueObjects;

/// <summary>
/// Contains unit tests for the <see cref="MusicWork"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class MusicWorkTests
{
    private readonly MusicWorkFixture _musicWorkFixture = new();
    private readonly MusicBrainzIdFixture _musicBrainzIdFixture = new();
    private readonly LanguageInfoFixture _languageInfoFixture = new();

    [Fact]
    public void Create_WhenCalledWithValidValues_ShouldCreateWorkWithAllPropertiesSet()
    {
        // Arrange
        MusicBrainzId workId = _musicBrainzIdFixture.Create(value: Guid.Parse("f0e9c1a2-0000-0000-0000-000000000000"));
        LanguageInfo language = _languageInfoFixture.Create(languageCode: "en", languageName: "English");

        // Act
        Result<MusicWork> result = MusicWork.Create(
            workId,
            "Bohemian Rhapsody",
            Optional<string>.Some("Song"),
            [language],
            ["T1234567890"]);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(workId, result.Value.MusicBrainzWorkId);
        Assert.Equal("Bohemian Rhapsody", result.Value.Title);
        Assert.Equal(Optional<string>.Some("Song"), result.Value.Type);
        Assert.Equal([language], result.Value.Languages);
        Assert.Equal(["T1234567890"], result.Value.Iswcs);
    }

    [Fact]
    public void Create_WhenTitleHasSurroundingWhitespace_ShouldTrimIt()
    {
        // Act
        Result<MusicWork> result = MusicWork.Create(
            _musicBrainzIdFixture.Create(),
            "  Bohemian Rhapsody  ",
            Optional<string>.None(),
            [],
            []);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal("Bohemian Rhapsody", result.Value.Title);
    }

    [Theory]
    [InlineData(null)] // null work title
    [InlineData("")] // empty work title
    [InlineData("   ")] // whitespace work title
    public void Create_WhenTitleIsNullOrWhitespace_ShouldReturnError(string? title)
    {
        // Act
        Result<MusicWork> result = MusicWork.Create(
            _musicBrainzIdFixture.Create(),
            title!,
            Optional<string>.None(),
            [],
            []);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(DomainErrors.Music.WorkTitleCannotBeEmpty, result.FirstError);
    }

    [Fact]
    public void Create_WhenOptionalAndCollectionValuesAreAbsent_ShouldCreateWorkWithoutThem()
    {
        // Act
        Result<MusicWork> result = MusicWork.Create(
            _musicBrainzIdFixture.Create(),
            "Bohemian Rhapsody",
            Optional<string>.None(),
            [],
            []);

        // Assert
        Assert.False(result.IsFailure);
        Assert.False(result.Value.Type.HasValue);
        Assert.Empty(result.Value.Languages);
        Assert.Empty(result.Value.Iswcs);
    }

    [Fact]
    public void Equals_WithSameValues_ShouldReturnTrue()
    {
        // Arrange
        MusicWork firstWork = _musicWorkFixture.Create();

        // Act
        MusicWork secondWork = _musicWorkFixture.Create(
            musicBrainzWorkId: firstWork.MusicBrainzWorkId,
            title: firstWork.Title,
            type: firstWork.Type,
            languages: [.. firstWork.Languages],
            iswcs: [.. firstWork.Iswcs]);

        // Assert
        Assert.Equal(firstWork, secondWork);
    }

    [Fact]
    public void Equals_WithDifferentMusicBrainzWorkId_ShouldReturnFalse()
    {
        // Arrange
        MusicWork firstWork = _musicWorkFixture.Create(musicBrainzWorkId: _musicBrainzIdFixture.Create(value: Guid.Parse("11111111-1111-1111-1111-111111111111")));

        // Act
        MusicWork secondWork = _musicWorkFixture.Create(musicBrainzWorkId: _musicBrainzIdFixture.Create(value: Guid.Parse("22222222-2222-2222-2222-222222222222")));

        // Assert
        Assert.NotEqual(firstWork, secondWork);
    }

    [Fact]
    public void Equals_WithDifferentTitle_ShouldReturnFalse()
    {
        // Arrange
        MusicWork firstWork = _musicWorkFixture.Create(title: "Bohemian Rhapsody");

        // Act
        MusicWork secondWork = _musicWorkFixture.Create(title: "Love of My Life");

        // Assert
        Assert.NotEqual(firstWork, secondWork);
    }

    [Fact]
    public void Equals_WithDifferentType_ShouldReturnFalse()
    {
        // Arrange
        MusicWork firstWork = _musicWorkFixture.Create(type: Optional<string>.Some("Song"));

        // Act
        MusicWork secondWork = _musicWorkFixture.Create(type: Optional<string>.Some("Instrumental"));

        // Assert
        Assert.NotEqual(firstWork, secondWork);
    }

    [Fact]
    public void Equals_WithDifferentLanguages_ShouldReturnFalse()
    {
        // Arrange
        LanguageInfo english = _languageInfoFixture.Create(languageCode: "en", languageName: "English");
        LanguageInfo french = _languageInfoFixture.Create(languageCode: "fr", languageName: "French");
        MusicWork firstWork = _musicWorkFixture.Create(languages: [english]);

        // Act
        MusicWork secondWork = _musicWorkFixture.Create(languages: [french]);

        // Assert
        Assert.NotEqual(firstWork, secondWork);
    }

    [Fact]
    public void Equals_WithDifferentIswcs_ShouldReturnFalse()
    {
        // Arrange
        MusicWork firstWork = _musicWorkFixture.Create(iswcs: ["T1234567890"]);

        // Act
        MusicWork secondWork = _musicWorkFixture.Create(iswcs: ["T0987654321"]);

        // Assert
        Assert.NotEqual(firstWork, secondWork);
    }

    [Fact]
    public void Equals_WithDifferentLanguageOrder_ShouldReturnFalse()
    {
        // Arrange
        LanguageInfo english = _languageInfoFixture.Create(languageCode: "en", languageName: "English");
        LanguageInfo french = _languageInfoFixture.Create(languageCode: "fr", languageName: "French");
        MusicWork firstWork = _musicWorkFixture.Create(languages: [english, french]);

        // Act
        MusicWork secondWork = _musicWorkFixture.Create(languages: [french, english]);

        // Assert
        Assert.NotEqual(firstWork, secondWork);
    }
}
