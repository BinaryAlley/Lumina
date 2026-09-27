#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.Mapping.MediaLibrary.AudioLibrary.MusicLibrary.Albums;
using Lumina.Contracts.DTO.Common;
using Lumina.Contracts.DTO.MediaLibrary.AudioLibrary.MusicLibrary;
using Lumina.Contracts.Fixtures.Core.DTO.Common;
using Lumina.Contracts.Fixtures.Core.DTO.MediaLibrary.AudioLibrary.MusicLibrary;
using Lumina.Domain.Common.Errors;
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.Core.BoundedContexts.AudioLibraryBoundedContext.MusicLibraryAggregate.ValueObjects;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Application.UnitTests.Common.Mapping.MediaLibrary.AudioLibrary.MusicLibrary.Albums;

/// <summary>
/// Contains unit tests for the <see cref="AlbumMetadataDtoMapping"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class AlbumMetadataDtoMappingTests
{
    private readonly AlbumMetadataDtoFixture _albumMetadataDtoFixture = new();
    private readonly GenreDtoFixture _genreDtoFixture = new();
    private readonly TagDtoFixture _tagDtoFixture = new();
    private readonly ReleaseInfoDtoFixture _releaseInfoDtoFixture = new();
    private readonly LanguageInfoDtoFixture _languageInfoDtoFixture = new();

    [Fact]
    public void ToDomainEntity_WhenMappingCompleteDto_ShouldMapAllPropertiesCorrectly()
    {
        // Arrange
        AlbumMetadataDto dto = _albumMetadataDtoFixture.Create();

        // Act
        Result<AlbumMetadata> result = dto.ToDomainEntity();

        // Assert
        Assert.False(result.IsFailure);
        AlbumMetadata metadata = result.Value;
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
        Assert.Equal(dto.Genres!.Select(genre => genre.Name), metadata.Genres.Select(genre => genre.Name));
        Assert.Equal(dto.Tags!.Select(tag => tag.Name), metadata.Tags.Select(tag => tag.Name));
        Assert.True(metadata.ReleaseType.HasValue);
        Assert.Equal(dto.ReleaseType, metadata.ReleaseType.Value);
        Assert.True(metadata.ReleaseStatus.HasValue);
        Assert.Equal(dto.ReleaseStatus, metadata.ReleaseStatus.Value);
        Assert.True(metadata.TotalDiscs.HasValue);
        Assert.Equal(dto.TotalDiscs, metadata.TotalDiscs.Value);
        Assert.Equal(dto.TotalTracks, metadata.TotalTracks);
    }

    [Fact]
    public void ToDomainEntity_WhenOptionalPropertiesAreMissing_ShouldMapTheRequiredValuesAndDefaults()
    {
        // Arrange
        AlbumMetadataDto dto = _albumMetadataDtoFixture.Create(
            includeOriginalTitle: false,
            includeDescription: false,
            includeReleaseInfo: false,
            includeLanguage: false,
            includeOriginalLanguage: false,
            includeTags: false,
            includeGenres: false,
            includeReleaseType: false,
            includeReleaseStatus: false,
            includeTotalDiscs: false,
            includeTotalTracks: false);

        // Act
        Result<AlbumMetadata> result = dto.ToDomainEntity();

        // Assert
        Assert.False(result.IsFailure);
        AlbumMetadata metadata = result.Value;
        Assert.Equal(dto.Title, metadata.Title);
        Assert.False(metadata.OriginalTitle.HasValue);
        Assert.False(metadata.Description.HasValue);
        Assert.False(metadata.ReleaseInfo.OriginalReleaseDate.HasValue);
        Assert.False(metadata.ReleaseInfo.OriginalReleaseYear.HasValue);
        Assert.False(metadata.ReleaseInfo.ReReleaseDate.HasValue);
        Assert.False(metadata.ReleaseInfo.ReReleaseYear.HasValue);
        Assert.False(metadata.ReleaseInfo.ReleaseCountry.HasValue);
        Assert.False(metadata.ReleaseInfo.ReleaseVersion.HasValue);
        Assert.False(metadata.Language.HasValue);
        Assert.False(metadata.OriginalLanguage.HasValue);
        Assert.Empty(metadata.Genres);
        Assert.Empty(metadata.Tags);
        Assert.False(metadata.ReleaseType.HasValue);
        Assert.False(metadata.ReleaseStatus.HasValue);
        Assert.False(metadata.TotalDiscs.HasValue);
        Assert.Equal(0, metadata.TotalTracks);
    }

    [Fact]
    public void ToDomainEntity_WhenLanguageIsMissingRequiredSubproperties_ShouldMapLanguageToNone()
    {
        // Arrange
        LanguageInfoDto incompleteLanguage = _languageInfoDtoFixture.Create(includeLanguageCode: false);
        AlbumMetadataDto dto = _albumMetadataDtoFixture.Create(
            language: incompleteLanguage,
            originalLanguage: _languageInfoDtoFixture.Create(includeLanguageName: false));

        // Act
        Result<AlbumMetadata> result = dto.ToDomainEntity();

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
        AlbumMetadataDto dto = _albumMetadataDtoFixture.Create(releaseInfo: releaseInfo);

        // Act
        Result<AlbumMetadata> result = dto.ToDomainEntity();

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.Metadata.OriginalReleaseDateAndYearMustMatch, result.FirstError);
    }

    [Fact]
    public void ToDomainEntity_WhenGenreIsInvalid_ShouldReturnError()
    {
        // Arrange
        AlbumMetadataDto dto = _albumMetadataDtoFixture.Create(genres: [_genreDtoFixture.Create(name: string.Empty)]);

        // Act
        Result<AlbumMetadata> result = dto.ToDomainEntity();

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.Metadata.GenreNameCannotBeEmpty, result.FirstError);
    }

    [Fact]
    public void ToDomainEntity_WhenTagIsInvalid_ShouldReturnError()
    {
        // Arrange
        AlbumMetadataDto dto = _albumMetadataDtoFixture.Create(tags: [_tagDtoFixture.Create(name: "   ")]);

        // Act
        Result<AlbumMetadata> result = dto.ToDomainEntity();

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.Metadata.TagNameCannotBeEmpty, result.FirstError);
    }
}
