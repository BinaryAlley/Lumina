#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.Mapping.MediaLibrary.AudioLibrary.MusicLibrary.Tracks;
using Lumina.Contracts.DTO.Common;
using Lumina.Contracts.DTO.MediaLibrary.AudioLibrary.MusicLibrary;
using Lumina.Contracts.Fixtures.Core.DTO.Common;
using Lumina.Contracts.Fixtures.Core.DTO.MediaLibrary.AudioLibrary.MusicLibrary;
using Lumina.Domain.Common.Errors;
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.Common.ValueObjects.Metadata;
using Lumina.Domain.Core.BoundedContexts.AudioLibraryBoundedContext.MusicLibraryAggregate.ValueObjects;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Application.UnitTests.Common.Mapping.MediaLibrary.AudioLibrary.MusicLibrary.Tracks;

/// <summary>
/// Contains unit tests for the <see cref="MusicWorkDtoMapping"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class MusicWorkDtoMappingTests
{
    private readonly MusicWorkDtoFixture _musicWorkDtoFixture = new();
    private readonly LanguageInfoDtoFixture _languageInfoDtoFixture = new();

    [Fact]
    public void ToDomainEntity_WhenMappingCompleteDto_ShouldMapAllPropertiesCorrectly()
    {
        // Arrange
        MusicWorkDto dto = _musicWorkDtoFixture.Create();

        // Act
        Result<MusicWork> result = dto.ToDomainValueObject();

        // Assert
        Assert.False(result.IsFailure);
        MusicWork work = result.Value;
        Assert.Equal(dto.MusicBrainzWorkId!.Value, work.MusicBrainzWorkId.Value);
        Assert.Equal(dto.Title, work.Title);
        Assert.True(work.Type.HasValue);
        Assert.Equal(dto.Type, work.Type.Value);
        Assert.Equal(dto.Languages!.Count, work.Languages.Count);
        Assert.Equal(dto.Iswcs, work.Iswcs);
    }

    [Fact]
    public void ToDomainEntity_WhenLanguageIsMapped_ShouldLowercaseTheLanguageCode()
    {
        // Arrange
        LanguageInfoDto language = _languageInfoDtoFixture.Create(languageCode: "EN", languageName: "English", nativeName: "English");
        MusicWorkDto dto = _musicWorkDtoFixture.Create(languages: [language]);

        // Act
        Result<MusicWork> result = dto.ToDomainValueObject();

        // Assert
        Assert.False(result.IsFailure);
        LanguageInfo mappedLanguage = Assert.Single(result.Value.Languages);
        Assert.Equal("en", mappedLanguage.LanguageCode);
        Assert.Equal("English", mappedLanguage.LanguageName);
        Assert.Equal("English", mappedLanguage.NativeName.Value);
    }

    [Fact]
    public void ToDomainEntity_WhenMusicBrainzWorkIdIsMissing_ShouldReturnError()
    {
        // Arrange
        MusicWorkDto dto = _musicWorkDtoFixture.Create(includeMusicBrainzWorkId: false);

        // Act
        Result<MusicWork> result = dto.ToDomainValueObject();

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.Music.MusicBrainzIdInvalidFormat, result.FirstError);
    }

    [Fact]
    public void ToDomainEntity_WhenTitleIsMissing_ShouldReturnError()
    {
        // Arrange
        MusicWorkDto dto = _musicWorkDtoFixture.Create(includeTitle: false);

        // Act
        Result<MusicWork> result = dto.ToDomainValueObject();

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.Music.WorkTitleCannotBeEmpty, result.FirstError);
    }

    [Theory]
    [InlineData("")] // empty work title
    [InlineData("   ")] // whitespace work title
    public void ToDomainEntity_WhenTitleIsEmptyOrWhitespace_ShouldReturnError(string title)
    {
        // Arrange
        MusicWorkDto dto = _musicWorkDtoFixture.Create(title: title);

        // Act
        Result<MusicWork> result = dto.ToDomainValueObject();

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.Music.WorkTitleCannotBeEmpty, result.FirstError);
    }

    [Fact]
    public void ToDomainEntity_WhenOptionalPropertiesAreMissing_ShouldMapTheRequiredValuesAndDefaults()
    {
        // Arrange
        MusicWorkDto dto = _musicWorkDtoFixture.Create(
            includeType: false,
            includeLanguages: false,
            includeIswcs: false);

        // Act
        Result<MusicWork> result = dto.ToDomainValueObject();

        // Assert
        Assert.False(result.IsFailure);
        MusicWork work = result.Value;
        Assert.Equal(dto.Title, work.Title);
        Assert.False(work.Type.HasValue);
        Assert.Empty(work.Languages);
        Assert.Empty(work.Iswcs);
    }

    [Fact]
    public void ToDomainEntity_WhenLanguageIsMissingRequiredSubproperties_ShouldSkipIt()
    {
        // Arrange
        LanguageInfoDto languageWithoutCode = _languageInfoDtoFixture.Create(languageName: "English", includeLanguageCode: false);
        LanguageInfoDto languageWithoutName = _languageInfoDtoFixture.Create(languageCode: "fr", includeLanguageName: false);
        MusicWorkDto dto = _musicWorkDtoFixture.Create(languages: [languageWithoutCode, languageWithoutName]);

        // Act
        Result<MusicWork> result = dto.ToDomainValueObject();

        // Assert
        Assert.False(result.IsFailure);
        Assert.Empty(result.Value.Languages);
    }

    [Fact]
    public void ToDomainEntity_WhenIswcsContainBlankValues_ShouldTrimThemAndSkipTheBlankOnes()
    {
        // Arrange
        MusicWorkDto dto = _musicWorkDtoFixture.Create(iswcs: ["  T1234567890  ", "   ", ""]);

        // Act
        Result<MusicWork> result = dto.ToDomainValueObject();

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(["T1234567890"], result.Value.Iswcs);
    }
}
