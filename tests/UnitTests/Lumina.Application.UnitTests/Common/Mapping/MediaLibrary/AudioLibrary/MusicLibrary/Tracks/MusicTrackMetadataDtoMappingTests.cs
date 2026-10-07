#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.Mapping.MediaLibrary.AudioLibrary.MusicLibrary.Tracks;
using Lumina.Contracts.DTO.Common;
using Lumina.Contracts.DTO.MediaLibrary.AudioLibrary.MusicLibrary;
using Lumina.Contracts.Fixtures.Core.DTO.Common;
using Lumina.Contracts.Fixtures.Core.DTO.MediaLibrary.AudioLibrary.MusicLibrary;
using Lumina.Domain.Common.Errors;
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.Core.BoundedContexts.AudioLibraryBoundedContext.Common.ValueObjects;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Application.UnitTests.Common.Mapping.MediaLibrary.AudioLibrary.MusicLibrary.Tracks;

/// <summary>
/// Contains unit tests for the <see cref="MusicTrackMetadataDtoMapping"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class MusicTrackMetadataDtoMappingTests
{
    private readonly MusicTrackMetadataDtoFixture _audioMetadataDtoFixture = new();
    private readonly GenreDtoFixture _genreDtoFixture = new();
    private readonly TagDtoFixture _tagDtoFixture = new();
    private readonly ReleaseInfoDtoFixture _releaseInfoDtoFixture = new();
    private readonly LanguageInfoDtoFixture _languageInfoDtoFixture = new();

    [Fact]
    public void ToDomainEntity_WhenMappingCompleteDto_ShouldMapAllPropertiesCorrectly()
    {
        // Arrange
        MusicTrackMetadataDto dto = _audioMetadataDtoFixture.Create();

        // Act
        Result<AudioMetadata> result = dto.ToDomainValueObject();

        // Assert
        Assert.False(result.IsFailure);
        AudioMetadata metadata = result.Value;
        Assert.Equal(dto.Title, metadata.Title);
        Assert.True(metadata.OriginalTitle.HasValue);
        Assert.Equal(dto.OriginalTitle, metadata.OriginalTitle.Value);
        Assert.True(metadata.Description.HasValue);
        Assert.Equal(dto.Description, metadata.Description.Value);
        Assert.True(metadata.ReleaseInfo.OriginalReleaseDate.HasValue);
        Assert.Equal(dto.ReleaseInfo!.OriginalReleaseDate, metadata.ReleaseInfo.OriginalReleaseDate.Value);
        Assert.True(metadata.ReleaseInfo.OriginalReleaseYear.HasValue);
        Assert.Equal(dto.ReleaseInfo.OriginalReleaseYear, metadata.ReleaseInfo.OriginalReleaseYear.Value);
        Assert.True(metadata.ReleaseInfo.ReReleaseDate.HasValue);
        Assert.Equal(dto.ReleaseInfo.ReReleaseDate, metadata.ReleaseInfo.ReReleaseDate.Value);
        Assert.True(metadata.ReleaseInfo.ReReleaseYear.HasValue);
        Assert.Equal(dto.ReleaseInfo.ReReleaseYear, metadata.ReleaseInfo.ReReleaseYear.Value);
        Assert.True(metadata.ReleaseInfo.ReleaseCountry.HasValue);
        Assert.Equal(dto.ReleaseInfo.ReleaseCountry, metadata.ReleaseInfo.ReleaseCountry.Value);
        Assert.True(metadata.ReleaseInfo.ReleaseVersion.HasValue);
        Assert.Equal(dto.ReleaseInfo.ReleaseVersion, metadata.ReleaseInfo.ReleaseVersion.Value);
        Assert.True(metadata.Language.HasValue);
        Assert.Equal(dto.Language!.LanguageCode!.ToLowerInvariant(), metadata.Language.Value.LanguageCode);
        Assert.Equal(dto.Language.LanguageName, metadata.Language.Value.LanguageName);
        Assert.Equal(dto.Language.NativeName, metadata.Language.Value.NativeName.Value);
        Assert.True(metadata.OriginalLanguage.HasValue);
        Assert.Equal(dto.OriginalLanguage!.LanguageCode!.ToLowerInvariant(), metadata.OriginalLanguage.Value.LanguageCode);
        Assert.Equal(dto.OriginalLanguage.LanguageName, metadata.OriginalLanguage.Value.LanguageName);
        Assert.Equal(dto.OriginalLanguage.NativeName, metadata.OriginalLanguage.Value.NativeName.Value);
        Assert.Equal(dto.DurationInSeconds, metadata.DurationInSeconds);
        Assert.Equal(dto.SampleRate, metadata.SampleRate);
        Assert.Equal(dto.Channels, metadata.Channels);
        Assert.True(metadata.BitDepth.HasValue);
        Assert.Equal(dto.BitDepth, metadata.BitDepth.Value);
        Assert.True(metadata.AudioCodec.HasValue);
        Assert.Equal(dto.AudioCodec, metadata.AudioCodec.Value);
        Assert.True(metadata.Bitrate.HasValue);
        Assert.Equal(dto.Bitrate, metadata.Bitrate.Value);
        Assert.Equal(dto.Genres!.Select(genre => genre.Name), metadata.Genres.Select(genre => genre.Name));
        Assert.Equal(dto.Tags!.Select(tag => tag.Name), metadata.Tags.Select(tag => tag.Name));
    }

    [Fact]
    public void ToDomainEntity_WhenOptionalPropertiesAreMissing_ShouldMapTheRequiredValuesAndDefaults()
    {
        // Arrange
        MusicTrackMetadataDto dto = _audioMetadataDtoFixture.Create(
            includeOriginalTitle: false,
            includeDescription: false,
            includeReleaseInfo: false,
            includeLanguage: false,
            includeOriginalLanguage: false,
            includeTags: false,
            includeGenres: false,
            includeDurationInSeconds: false,
            includeSampleRate: false,
            includeChannels: false,
            includeBitDepth: false,
            includeAudioCodec: false,
            includeBitrate: false);

        // Act
        Result<AudioMetadata> result = dto.ToDomainValueObject();

        // Assert
        Assert.False(result.IsFailure);
        AudioMetadata metadata = result.Value;
        Assert.Equal(dto.Title, metadata.Title);
        Assert.False(metadata.OriginalTitle.HasValue);
        Assert.False(metadata.Description.HasValue);
        Assert.False(metadata.ReleaseInfo.OriginalReleaseDate.HasValue);
        Assert.False(metadata.ReleaseInfo.OriginalReleaseYear.HasValue);
        Assert.False(metadata.ReleaseInfo.ReReleaseDate.HasValue);
        Assert.False(metadata.ReleaseInfo.ReReleaseYear.HasValue);
        Assert.False(metadata.ReleaseInfo.ReleaseCountry.HasValue);
        Assert.False(metadata.ReleaseInfo.ReleaseVersion.HasValue);
        Assert.Equal(0, metadata.DurationInSeconds);
        Assert.Equal(0, metadata.SampleRate);
        Assert.Equal(0, metadata.Channels);
        Assert.False(metadata.BitDepth.HasValue);
        Assert.False(metadata.AudioCodec.HasValue);
        Assert.False(metadata.Bitrate.HasValue);
        Assert.False(metadata.Language.HasValue);
        Assert.False(metadata.OriginalLanguage.HasValue);
        Assert.Empty(metadata.Genres);
        Assert.Empty(metadata.Tags);
    }

    [Fact]
    public void ToDomainEntity_WhenLanguageIsMissingRequiredSubproperties_ShouldMapLanguageToNone()
    {
        // Arrange
        LanguageInfoDto incompleteLanguage = _languageInfoDtoFixture.Create(includeLanguageCode: false);
        MusicTrackMetadataDto dto = _audioMetadataDtoFixture.Create(
            language: incompleteLanguage,
            originalLanguage: _languageInfoDtoFixture.Create(includeLanguageName: false));

        // Act
        Result<AudioMetadata> result = dto.ToDomainValueObject();

        // Assert
        Assert.False(result.IsFailure);
        Assert.False(result.Value.Language.HasValue);
        Assert.False(result.Value.OriginalLanguage.HasValue);
    }

    [Fact]
    public void ToDomainEntity_WhenReleaseInfoIsInvalid_ShouldReturnError()
    {
        // Arrange
        ReleaseInfoDto releaseInfo = _releaseInfoDtoFixture.Create(
            originalReleaseDate: new DateOnly(2000, 1, 1),
            originalReleaseYear: 1999);
        MusicTrackMetadataDto dto = _audioMetadataDtoFixture.Create(releaseInfo: releaseInfo);

        // Act
        Result<AudioMetadata> result = dto.ToDomainValueObject();

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.Metadata.OriginalReleaseDateAndYearMustMatch, result.FirstError);
    }

    [Fact]
    public void ToDomainEntity_WhenGenreIsInvalid_ShouldReturnError()
    {
        // Arrange
        MusicTrackMetadataDto dto = _audioMetadataDtoFixture.Create(genres: [_genreDtoFixture.Create(name: string.Empty)]);

        // Act
        Result<AudioMetadata> result = dto.ToDomainValueObject();

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.Metadata.GenreNameCannotBeEmpty, result.FirstError);
    }

    [Fact]
    public void ToDomainEntity_WhenTagIsInvalid_ShouldReturnError()
    {
        // Arrange
        MusicTrackMetadataDto dto = _audioMetadataDtoFixture.Create(tags: [_tagDtoFixture.Create(name: "   ")]);

        // Act
        Result<AudioMetadata> result = dto.ToDomainValueObject();

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.Metadata.TagNameCannotBeEmpty, result.FirstError);
    }
}
