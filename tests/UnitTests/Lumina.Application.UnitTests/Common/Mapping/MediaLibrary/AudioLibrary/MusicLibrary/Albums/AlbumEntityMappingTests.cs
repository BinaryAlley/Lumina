#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.Common;
using Lumina.Application.Common.DataAccess.Entities.MediaLibrary.AudioLibrary.MusicLibrary;
using Lumina.Application.Common.Mapping.MediaLibrary.AudioLibrary.MusicLibrary.Albums;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.Common;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.MediaLibrary.AudioLibrary.MusicLibrary;
using Lumina.Contracts.DTO.MediaContributors;
using Lumina.Contracts.DTO.MediaLibrary.AudioLibrary;
using Lumina.Contracts.Responses.MediaLibrary.AudioLibrary.MusicLibrary.Albums;
using Lumina.Domain.Common.Errors;
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.Core.BoundedContexts.AudioLibraryBoundedContext.MusicLibraryAggregate.Entities;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Application.UnitTests.Common.Mapping.MediaLibrary.AudioLibrary.MusicLibrary.Albums;

/// <summary>
/// Contains unit tests for the <see cref="AlbumEntityMapping"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class AlbumEntityMappingTests
{
    private readonly AlbumEntityFixture _albumEntityFixture = new();
    private readonly TrackEntityFixture _trackEntityFixture = new();
    private readonly GenreEntityFixture _genreEntityFixture = new();
    private readonly TagEntityFixture _tagEntityFixture = new();
    private readonly AudioRatingEntityFixture _audioRatingEntityFixture = new();

    [Fact]
    public void ToDomainEntity_WhenMappingValidAlbumEntity_ShouldMapAllPropertiesCorrectly()
    {
        // Arrange
        AlbumEntity entity = _albumEntityFixture.Create();

        // Act
        Result<Album> result = entity.ToDomainEntity();

        // Assert
        Assert.False(result.IsFailure);
        Album album = result.Value;
        Assert.Equal(entity.Id, album.Id.Value);
        Assert.Equal(entity.Title, album.Metadata.Title);
        Assert.True(album.Metadata.OriginalTitle.HasValue);
        Assert.Equal(entity.OriginalTitle, album.Metadata.OriginalTitle.Value);
        Assert.True(album.Metadata.Description.HasValue);
        Assert.Equal(entity.Description, album.Metadata.Description.Value);
        Assert.Equal(entity.OriginalReleaseDate, album.Metadata.ReleaseInfo.OriginalReleaseDate.Value);
        Assert.Equal(entity.OriginalReleaseYear, album.Metadata.ReleaseInfo.OriginalReleaseYear.Value);
        Assert.Equal(entity.ReReleaseDate, album.Metadata.ReleaseInfo.ReReleaseDate.Value);
        Assert.Equal(entity.ReReleaseYear, album.Metadata.ReleaseInfo.ReReleaseYear.Value);
        Assert.Equal(entity.ReleaseCountry, album.Metadata.ReleaseInfo.ReleaseCountry.Value);
        Assert.Equal(entity.ReleaseVersion, album.Metadata.ReleaseInfo.ReleaseVersion.Value);
        Assert.Equal(entity.Genres.Select(genre => genre.Name).OrderBy(name => name), album.Metadata.Genres.Select(genre => genre.Name).OrderBy(name => name));
        Assert.Equal(entity.Tags.Select(tag => tag.Name).OrderBy(name => name), album.Metadata.Tags.Select(tag => tag.Name).OrderBy(name => name));
        Assert.True(album.Metadata.Language.HasValue);
        Assert.Equal(entity.LanguageCode!.ToLowerInvariant(), album.Metadata.Language.Value.LanguageCode);
        Assert.Equal(entity.LanguageName, album.Metadata.Language.Value.LanguageName);
        Assert.Equal(entity.LanguageNativeName, album.Metadata.Language.Value.NativeName.Value);
        Assert.True(album.Metadata.OriginalLanguage.HasValue);
        Assert.Equal(entity.OriginalLanguageCode!.ToLowerInvariant(), album.Metadata.OriginalLanguage.Value.LanguageCode);
        Assert.Equal(entity.OriginalLanguageName, album.Metadata.OriginalLanguage.Value.LanguageName);
        Assert.Equal(entity.OriginalLanguageNativeName, album.Metadata.OriginalLanguage.Value.NativeName.Value);
        Assert.Equal(entity.ReleaseType, album.Metadata.ReleaseType.Value);
        Assert.Equal(entity.ReleaseStatus, album.Metadata.ReleaseStatus.Value);
        Assert.Equal(entity.TotalDiscs, album.Metadata.TotalDiscs.Value);
        Assert.Equal(entity.TotalTracks, album.Metadata.TotalTracks);
        Assert.Equal(entity.MediaFormat, album.MediaFormat.Value);
        Assert.Equal(entity.Barcode, album.Barcode.Value.Value);
        Assert.Equal(entity.CatalogNumber, album.CatalogNumber.Value);
        Assert.Equal(entity.MusicBrainzReleaseId, album.MusicBrainzReleaseId.Value.Value);
        Assert.Equal(entity.MusicBrainzReleaseGroupId, album.MusicBrainzReleaseGroupId.Value.Value);
        Assert.Equal(entity.MusicBrainzReleaseArtistId, album.MusicBrainzReleaseArtistId.Value.Value);
        Assert.Equal(entity.Contributors.Select(contributor => contributor.MediaContributorId), album.Contributors.Select(contributor => contributor.ContributorId.Value));
        Assert.Equal(entity.Contributors.Select(contributor => contributor.Role), album.Contributors.Select(contributor => contributor.Role));
        Assert.Equal(entity.Ratings.Select(rating => rating.Value), album.Ratings.Select(rating => (decimal?)rating.Value));
        Assert.Equal(entity.Tracks.Select(track => track.Id), album.Tracks.Select(track => track.Id.Value));
        Assert.Equal(entity.CreatedOnUtc, album.CreatedOnUtc);
        Assert.Equal(entity.UpdatedOnUtc, album.UpdatedOnUtc.Value);
    }

    [Fact]
    public void ToDomainEntity_WhenOptionalPropertiesAreMissing_ShouldMapTheRequiredValuesAndNones()
    {
        // Arrange
        AlbumEntity entity = _albumEntityFixture.Create(
            includeMetadata: false,
            includeTracks: false,
            includeOriginalReleaseDate: false,
            includeOriginalReleaseYear: false,
            includeReReleaseDate: false,
            includeReReleaseYear: false,
            includeBarcode: false,
            includeOriginalTitle: false,
            includeDescription: false,
            includeLanguage: false,
            includeOriginalLanguage: false,
            includeReleaseType: false,
            includeReleaseStatus: false,
            includeTotalDiscs: false,
            includeMediaFormat: false,
            includeCatalogNumber: false,
            includeMusicBrainzReleaseId: false,
            includeMusicBrainzReleaseGroupId: false,
            includeMusicBrainzReleaseArtistId: false);

        // Act
        Result<Album> result = entity.ToDomainEntity();

        // Assert
        Assert.False(result.IsFailure);
        Album album = result.Value;
        Assert.False(album.Metadata.OriginalTitle.HasValue);
        Assert.False(album.Metadata.Description.HasValue);
        Assert.False(album.Metadata.ReleaseInfo.OriginalReleaseDate.HasValue);
        Assert.False(album.Metadata.ReleaseInfo.OriginalReleaseYear.HasValue);
        Assert.False(album.Metadata.ReleaseInfo.ReReleaseDate.HasValue);
        Assert.False(album.Metadata.ReleaseInfo.ReReleaseYear.HasValue);
        Assert.Empty(album.Metadata.Genres);
        Assert.Empty(album.Metadata.Tags);
        Assert.False(album.Metadata.Language.HasValue);
        Assert.False(album.Metadata.OriginalLanguage.HasValue);
        Assert.False(album.Metadata.ReleaseType.HasValue);
        Assert.False(album.Metadata.ReleaseStatus.HasValue);
        Assert.False(album.Metadata.TotalDiscs.HasValue);
        Assert.False(album.MediaFormat.HasValue);
        Assert.False(album.Barcode.HasValue);
        Assert.False(album.CatalogNumber.HasValue);
        Assert.False(album.MusicBrainzReleaseId.HasValue);
        Assert.False(album.MusicBrainzReleaseGroupId.HasValue);
        Assert.False(album.MusicBrainzReleaseArtistId.HasValue);
        Assert.Empty(album.Contributors);
        Assert.Empty(album.Ratings);
        Assert.Empty(album.Tracks);
    }

    [Fact]
    public void ToDomainEntity_WhenReleaseInfoIsInvalid_ShouldReturnError()
    {
        // Arrange
        AlbumEntity entity = _albumEntityFixture.Create(
            originalReleaseDate: new DateOnly(2000, 1, 1),
            originalReleaseYear: 1999);

        // Act
        Result<Album> result = entity.ToDomainEntity();

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.Metadata.OriginalReleaseDateAndYearMustMatch, result.FirstError);
    }

    [Fact]
    public void ToDomainEntity_WhenGenreIsInvalid_ShouldReturnError()
    {
        // Arrange
        AlbumEntity entity = _albumEntityFixture.Create();
        entity.Genres = [_genreEntityFixture.Create(name: string.Empty)];

        // Act
        Result<Album> result = entity.ToDomainEntity();

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.Metadata.GenreNameCannotBeEmpty, result.FirstError);
    }

    [Fact]
    public void ToDomainEntity_WhenTagIsInvalid_ShouldReturnError()
    {
        // Arrange
        AlbumEntity entity = _albumEntityFixture.Create();
        entity.Tags = [_tagEntityFixture.Create(name: "   ")];

        // Act
        Result<Album> result = entity.ToDomainEntity();

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.Metadata.TagNameCannotBeEmpty, result.FirstError);
    }

    [Fact]
    public void ToDomainEntity_WhenBarcodeCannotBeEmpty_ShouldReturnError()
    {
        // Arrange
        AlbumEntity entity = _albumEntityFixture.Create(barcode: string.Empty);

        // Act
        Result<Album> result = entity.ToDomainEntity();

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.Music.BarcodeValueCannotBeEmpty, result.FirstError);
    }

    [Fact]
    public void ToDomainEntity_WhenBarcodeHasInvalidFormat_ShouldReturnError()
    {
        // Arrange
        AlbumEntity entity = _albumEntityFixture.Create(barcode: "not-a-barcode");

        // Act
        Result<Album> result = entity.ToDomainEntity();

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.Music.InvalidFormatForBarcode, result.FirstError);
    }

    [Fact]
    public void ToDomainEntity_WhenRatingIsInvalid_ShouldReturnError()
    {
        // Arrange
        AlbumEntity entity = _albumEntityFixture.Create();
        entity.Ratings = [_audioRatingEntityFixture.Create(value: 6, maxValue: 5)];

        // Act
        Result<Album> result = entity.ToDomainEntity();

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.Metadata.RatingValueCannotBeGreaterThanMaxValue, result.FirstError);
    }

    [Fact]
    public void ToDomainEntity_WhenTrackIsInvalid_ShouldReturnError()
    {
        // Arrange
        TrackEntity trackEntity = _trackEntityFixture.Create();
        trackEntity.Isrcs = [new TrackIsrcEntity("not-an-isrc")];
        AlbumEntity entity = _albumEntityFixture.Create(tracks: [trackEntity]);

        // Act
        Result<Album> result = entity.ToDomainEntity();

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.Music.IsrcInvalidFormat, result.FirstError);
    }

    [Fact]
    public void ToResponse_WhenMappingAlbumEntity_ShouldMapAllPropertiesCorrectly()
    {
        // Arrange
        AlbumEntity entity = _albumEntityFixture.Create();

        // Act
        AlbumResponse result = entity.ToResponse();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(entity.Id, result.Id);
        Assert.Equal(entity.ArtistId, result.ArtistId);
        Assert.Equal(entity.LibraryId, result.LibraryId);
        Assert.Equal(entity.Title, result.Metadata.Title);
        Assert.Equal(entity.OriginalTitle, result.Metadata.OriginalTitle);
        Assert.Equal(entity.Description, result.Metadata.Description);
        Assert.Equal(entity.OriginalReleaseDate, result.Metadata.ReleaseInfo!.OriginalReleaseDate);
        Assert.Equal(entity.OriginalReleaseYear, result.Metadata.ReleaseInfo.OriginalReleaseYear);
        Assert.Equal(entity.ReReleaseDate, result.Metadata.ReleaseInfo.ReReleaseDate);
        Assert.Equal(entity.ReReleaseYear, result.Metadata.ReleaseInfo.ReReleaseYear);
        Assert.Equal(entity.ReleaseCountry, result.Metadata.ReleaseInfo.ReleaseCountry);
        Assert.Equal(entity.ReleaseVersion, result.Metadata.ReleaseInfo.ReleaseVersion);
        Assert.NotNull(result.Metadata.Language);
        Assert.Equal(entity.LanguageCode, result.Metadata.Language!.LanguageCode);
        Assert.Equal(entity.LanguageName, result.Metadata.Language.LanguageName);
        Assert.Equal(entity.LanguageNativeName, result.Metadata.Language.NativeName);
        Assert.NotNull(result.Metadata.OriginalLanguage);
        Assert.Equal(entity.OriginalLanguageCode, result.Metadata.OriginalLanguage!.LanguageCode);
        Assert.Equal(entity.OriginalLanguageName, result.Metadata.OriginalLanguage.LanguageName);
        Assert.Equal(entity.OriginalLanguageNativeName, result.Metadata.OriginalLanguage.NativeName);
        Assert.Equal(entity.Tags.Select(tag => tag.Name).OrderBy(name => name), result.Metadata.Tags!.Select(tag => tag.Name).OrderBy(name => name));
        Assert.Equal(entity.Genres.Select(genre => genre.Name).OrderBy(name => name), result.Metadata.Genres!.Select(genre => genre.Name).OrderBy(name => name));
        Assert.Equal(entity.ReleaseType, result.Metadata.ReleaseType);
        Assert.Equal(entity.ReleaseStatus, result.Metadata.ReleaseStatus);
        Assert.Equal(entity.TotalDiscs, result.Metadata.TotalDiscs);
        Assert.Equal(entity.TotalTracks, result.Metadata.TotalTracks);
        Assert.Equal(entity.MediaFormat, result.MediaFormat);
        Assert.Equal(entity.Barcode, result.Barcode);
        Assert.Equal(entity.CatalogNumber, result.CatalogNumber);
        Assert.Equal(entity.MusicBrainzReleaseId, result.MusicBrainzReleaseId);
        Assert.Equal(entity.MusicBrainzReleaseGroupId, result.MusicBrainzReleaseGroupId);
        Assert.Equal(entity.MusicBrainzReleaseArtistId, result.MusicBrainzReleaseArtistId);
        Assert.Equal(entity.CreatedOnUtc, result.CreatedOnUtc);
        Assert.Equal(entity.UpdatedOnUtc, result.UpdatedOnUtc);
        Assert.Equal(entity.Contributors.Select(contributor => new MediaContributorReferenceDto(contributor.MediaContributorId, contributor.Role)), result.Contributors);
        Assert.Equal(entity.Ratings.Select(rating => new AudioRatingDto(rating.Value, rating.MaxValue, rating.Source, rating.VoteCount)), result.Ratings);
        Assert.Equal(entity.Tracks.Select(track => track.Id), result.Tracks!.Select(track => track.Id));
    }

    [Fact]
    public void ToResponse_WhenLanguageSubpropertiesAreBlank_ShouldMapLanguagesToNull()
    {
        // Arrange
        AlbumEntity entity = _albumEntityFixture.Create(includeLanguage: false, includeOriginalLanguage: false);

        // Act
        AlbumResponse result = entity.ToResponse();

        // Assert
        Assert.Null(result.Metadata.Language);
        Assert.Null(result.Metadata.OriginalLanguage);
    }

    [Fact]
    public void ToResponse_WhenMetadataCollectionsAreEmpty_ShouldMapEmptyCollections()
    {
        // Arrange
        AlbumEntity entity = _albumEntityFixture.Create(includeMetadata: false, includeTracks: false);

        // Act
        AlbumResponse result = entity.ToResponse();

        // Assert
        Assert.Empty(result.Contributors!);
        Assert.Empty(result.Ratings!);
        Assert.Empty(result.Tracks!);
        Assert.Empty(result.Metadata.Tags!);
        Assert.Empty(result.Metadata.Genres!);
    }

    [Fact]
    public void ToResponses_WhenMappingMultipleAlbumEntities_ShouldMapAllCorrectly()
    {
        // Arrange
        List<AlbumEntity> entities = _albumEntityFixture.CreateMany(2);

        // Act
        IReadOnlyList<AlbumResponse> results = entities.ToResponses();

        // Assert
        Assert.NotNull(results);
        Assert.Equal(entities.Count, results.Count);
        for (int i = 0; i < entities.Count; i++)
        {
            Assert.Equal(entities[i].Id, results[i].Id);
            Assert.Equal(entities[i].Title, results[i].Metadata.Title);
        }
    }
}
