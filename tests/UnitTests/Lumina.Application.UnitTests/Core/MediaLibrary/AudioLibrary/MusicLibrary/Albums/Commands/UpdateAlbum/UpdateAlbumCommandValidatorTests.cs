#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Application.Core.MediaLibrary.AudioLibrary.MusicLibrary.Albums.Commands.UpdateAlbum;
using Lumina.Application.Fixtures.Core.MediaLibrary.AudioLibrary.MusicLibrary.Albums.Commands.UpdateAlbum;
using Lumina.Application.UnitTests.Common.Setup;
using Lumina.Contracts.DTO.Common;
using Lumina.Contracts.DTO.MediaLibrary.AudioLibrary;
using Lumina.Contracts.Fixtures.Core.DTO.Common;
using Lumina.Contracts.Fixtures.Core.DTO.MediaContributors;
using Lumina.Contracts.Fixtures.Core.DTO.MediaLibrary.AudioLibrary;
using Lumina.Contracts.Fixtures.Core.DTO.MediaLibrary.AudioLibrary.MusicLibrary;
using Lumina.Domain.Common.Errors;
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.SharedKernel.Common.Enums.AudioLibrary;
using Lumina.Domain.SharedKernel.Common.Enums.MediaContributors;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Application.UnitTests.Core.MediaLibrary.AudioLibrary.MusicLibrary.Albums.Commands.UpdateAlbum;

/// <summary>
/// Contains unit tests for the <see cref="UpdateAlbumCommandValidator"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class UpdateAlbumCommandValidatorTests
{
    private readonly UpdateAlbumCommandFixture _updateAlbumCommandFixture = new();
    private readonly UpdateAlbumCommandValidator _validator = new();
    private readonly MusicAlbumMetadataDtoFixture _albumMetadataDtoFixture = new();
    private readonly ReleaseInfoDtoFixture _releaseInfoDtoFixture = new();
    private readonly GenreDtoFixture _genreDtoFixture = new();
    private readonly TagDtoFixture _tagDtoFixture = new();
    private readonly LanguageInfoDtoFixture _languageInfoDtoFixture = new();
    private readonly MediaContributorReferenceDtoFixture _mediaContributorReferenceDtoFixture = new();
    private readonly AudioRatingDtoFixture _audioRatingDtoFixture = new();

    [Theory]
    [InlineData(null)] // missing route value
    [InlineData("")] // empty route value
    [InlineData("   ")] // whitespace route value, which both the not empty rule and the format rule reject
    [InlineData("not-a-library-guid")] // non-Guid route value
    [InlineData("00000000-0000-0000-0000-000000000000")] // empty Guid route value
    public void Validate_WhenLibraryIdIsEmptyOrInvalid_ShouldHaveValidationError(string? libraryId)
    {
        // Arrange
        UpdateAlbumCommand command = _updateAlbumCommandFixture.Create(libraryId: libraryId, includeLibraryId: libraryId is not null);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Library.LibraryIdCannotBeEmpty);
    }

    [Theory]
    [InlineData(null)] // missing route value
    [InlineData("")] // empty route value
    [InlineData("   ")] // whitespace route value, which both the not empty rule and the format rule reject
    [InlineData("not-an-artist-guid")] // non-Guid route value
    [InlineData("00000000-0000-0000-0000-000000000000")] // empty Guid route value
    public void Validate_WhenArtistIdIsEmptyOrInvalid_ShouldHaveValidationError(string? artistId)
    {
        // Arrange
        UpdateAlbumCommand command = _updateAlbumCommandFixture.Create(artistId: artistId, includeArtistId: artistId is not null);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Music.ArtistIdCannotBeEmpty);
    }

    [Theory]
    [InlineData(null)] // missing route value
    [InlineData("")] // empty route value
    [InlineData("   ")] // whitespace route value, which both the not empty rule and the format rule reject
    [InlineData("not-an-album-guid")] // non-Guid route value
    [InlineData("00000000-0000-0000-0000-000000000000")] // empty Guid route value
    public void Validate_WhenAlbumIdIsEmptyOrInvalid_ShouldHaveValidationError(string? albumId)
    {
        // Arrange
        UpdateAlbumCommand command = _updateAlbumCommandFixture.Create(albumId: albumId, includeAlbumId: albumId is not null);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Music.AlbumIdCannotBeEmpty);
    }

    [Fact]
    public void Validate_WhenMetadataIsNull_ShouldHaveValidationError()
    {
        // Arrange
        UpdateAlbumCommand command = _updateAlbumCommandFixture.Create(includeMetadata: false);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Metadata.MetadataCannotBeNull);
    }

    [Fact]
    public void Validate_WhenTitleIsNull_ShouldHaveValidationError()
    {
        // Arrange
        UpdateAlbumCommand command = _updateAlbumCommandFixture.Create(metadata: _albumMetadataDtoFixture.Create(includeTitle: false));

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Music.AlbumTitleCannotBeEmpty);
    }

    [Theory]
    [InlineData("")] // empty title
    [InlineData("   ")] // whitespace title, which the not empty rule rejects
    public void Validate_WhenTitleIsEmpty_ShouldHaveValidationError(string title)
    {
        // Arrange
        UpdateAlbumCommand command = _updateAlbumCommandFixture.Create(metadata: _albumMetadataDtoFixture.Create(title: title));

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Music.AlbumTitleCannotBeEmpty);
    }

    [Fact]
    public void Validate_WhenTitleExceeds255Characters_ShouldHaveValidationError()
    {
        // Arrange
        UpdateAlbumCommand command = _updateAlbumCommandFixture.Create(metadata: _albumMetadataDtoFixture.Create(title: new Faker().Random.String2(300)));

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Music.AlbumTitleMustBeMaximum255CharactersLong);
    }

    [Fact]
    public void Validate_WhenTitleIsValid_ShouldNotHaveValidationError()
    {
        // Arrange
        UpdateAlbumCommand command = _updateAlbumCommandFixture.Create(metadata: _albumMetadataDtoFixture.Create(title: new Faker().Random.String2(200)));

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationError(Errors.Music.AlbumTitleCannotBeEmpty);
        result.ShouldNotHaveValidationError(Errors.Music.AlbumTitleMustBeMaximum255CharactersLong);
    }

    [Fact]
    public void Validate_WhenOriginalTitleExceeds255Characters_ShouldHaveValidationError()
    {
        // Arrange
        UpdateAlbumCommand command = _updateAlbumCommandFixture.Create(metadata: _albumMetadataDtoFixture.Create(originalTitle: new Faker().Random.String2(300)));

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Metadata.OriginalTitleMustBeMaximum255CharactersLong);
    }

    [Fact]
    public void Validate_WhenOriginalTitleIsNull_ShouldNotHaveValidationError()
    {
        // Arrange
        UpdateAlbumCommand command = _updateAlbumCommandFixture.Create(metadata: _albumMetadataDtoFixture.Create(includeOriginalTitle: false));

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationError(Errors.Metadata.OriginalTitleMustBeMaximum255CharactersLong);
    }

    [Fact]
    public void Validate_WhenOriginalTitleIsValid_ShouldNotHaveValidationError()
    {
        // Arrange
        UpdateAlbumCommand command = _updateAlbumCommandFixture.Create(metadata: _albumMetadataDtoFixture.Create(originalTitle: new Faker().Random.String2(200)));

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationError(Errors.Metadata.OriginalTitleMustBeMaximum255CharactersLong);
    }

    [Fact]
    public void Validate_WhenDescriptionExceeds2000Characters_ShouldHaveValidationError()
    {
        // Arrange
        UpdateAlbumCommand command = _updateAlbumCommandFixture.Create(metadata: _albumMetadataDtoFixture.Create(description: new Faker().Random.String2(2001)));

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Metadata.DescriptionMustBeMaximum2000CharactersLong);
    }

    [Fact]
    public void Validate_WhenDescriptionIsNull_ShouldNotHaveValidationError()
    {
        // Arrange
        UpdateAlbumCommand command = _updateAlbumCommandFixture.Create(metadata: _albumMetadataDtoFixture.Create(includeDescription: false));

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationError(Errors.Metadata.DescriptionMustBeMaximum2000CharactersLong);
    }

    [Fact]
    public void Validate_WhenDescriptionIsValid_ShouldNotHaveValidationError()
    {
        // Arrange
        UpdateAlbumCommand command = _updateAlbumCommandFixture.Create(metadata: _albumMetadataDtoFixture.Create(description: new Faker().Random.String2(1500)));

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationError(Errors.Metadata.DescriptionMustBeMaximum2000CharactersLong);
    }

    [Fact]
    public void Validate_WhenReleaseTypeIsNotDefined_ShouldHaveValidationError()
    {
        // Arrange
        UpdateAlbumCommand command = _updateAlbumCommandFixture.Create(metadata: _albumMetadataDtoFixture.Create(releaseTypes: [(MusicReleaseType)999]));

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Music.UnknownMusicReleaseType);
    }

    [Fact]
    public void Validate_WhenReleaseTypeIsNull_ShouldNotHaveValidationError()
    {
        // Arrange
        UpdateAlbumCommand command = _updateAlbumCommandFixture.Create(metadata: _albumMetadataDtoFixture.Create(includeReleaseTypes: false));

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationError(Errors.Music.UnknownMusicReleaseType);
    }

    [Fact]
    public void Validate_WhenReleaseTypeIsDefined_ShouldNotHaveValidationError()
    {
        // Arrange
        UpdateAlbumCommand command = _updateAlbumCommandFixture.Create(metadata: _albumMetadataDtoFixture.Create(releaseTypes: [MusicReleaseType.Album]));

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationError(Errors.Music.UnknownMusicReleaseType);
    }

    [Fact]
    public void Validate_WhenReleaseStatusIsNotDefined_ShouldHaveValidationError()
    {
        // Arrange
        UpdateAlbumCommand command = _updateAlbumCommandFixture.Create(metadata: _albumMetadataDtoFixture.Create(releaseStatus: (MusicReleaseStatus)999));

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Music.UnknownMusicReleaseStatus);
    }

    [Fact]
    public void Validate_WhenReleaseStatusIsNull_ShouldNotHaveValidationError()
    {
        // Arrange
        UpdateAlbumCommand command = _updateAlbumCommandFixture.Create(metadata: _albumMetadataDtoFixture.Create(includeReleaseStatus: false));

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationError(Errors.Music.UnknownMusicReleaseStatus);
    }

    [Fact]
    public void Validate_WhenReleaseStatusIsDefined_ShouldNotHaveValidationError()
    {
        // Arrange
        UpdateAlbumCommand command = _updateAlbumCommandFixture.Create(metadata: _albumMetadataDtoFixture.Create(releaseStatus: MusicReleaseStatus.Official));

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationError(Errors.Music.UnknownMusicReleaseStatus);
    }

    [Theory]
    [InlineData(0)] // zero is not a valid total discs value
    [InlineData(-1)] // a negative total discs value is not valid
    public void Validate_WhenTotalDiscsIsNotPositive_ShouldHaveValidationError(int totalDiscs)
    {
        // Arrange
        UpdateAlbumCommand command = _updateAlbumCommandFixture.Create(metadata: _albumMetadataDtoFixture.Create(totalDiscs: totalDiscs));

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Music.TotalDiscsMustBeGreaterThanZero);
    }

    [Fact]
    public void Validate_WhenTotalDiscsIsNull_ShouldNotHaveValidationError()
    {
        // Arrange
        UpdateAlbumCommand command = _updateAlbumCommandFixture.Create(metadata: _albumMetadataDtoFixture.Create(includeTotalDiscs: false));

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationError(Errors.Music.TotalDiscsMustBeGreaterThanZero);
    }

    [Fact]
    public void Validate_WhenTotalDiscsIsPositive_ShouldNotHaveValidationError()
    {
        // Arrange
        UpdateAlbumCommand command = _updateAlbumCommandFixture.Create(metadata: _albumMetadataDtoFixture.Create(totalDiscs: 2));

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationError(Errors.Music.TotalDiscsMustBeGreaterThanZero);
    }

    [Theory]
    [InlineData(0)] // zero is not a valid total tracks value
    [InlineData(-1)] // a negative total tracks value is not valid
    public void Validate_WhenTotalTracksIsNotPositive_ShouldHaveValidationError(int totalTracks)
    {
        // Arrange
        UpdateAlbumCommand command = _updateAlbumCommandFixture.Create(metadata: _albumMetadataDtoFixture.Create(totalTracks: totalTracks));

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Music.TotalTracksMustBeGreaterThanZero);
    }

    [Fact]
    public void Validate_WhenTotalTracksIsNull_ShouldNotHaveValidationError()
    {
        // Arrange
        UpdateAlbumCommand command = _updateAlbumCommandFixture.Create(metadata: _albumMetadataDtoFixture.Create(includeTotalTracks: false));

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationError(Errors.Music.TotalTracksMustBeGreaterThanZero);
    }

    [Fact]
    public void Validate_WhenTotalTracksIsPositive_ShouldNotHaveValidationError()
    {
        // Arrange
        UpdateAlbumCommand command = _updateAlbumCommandFixture.Create(metadata: _albumMetadataDtoFixture.Create(totalTracks: 10));

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationError(Errors.Music.TotalTracksMustBeGreaterThanZero);
    }

    [Fact]
    public void Validate_WhenReleaseInfoIsNull_ShouldHaveValidationError()
    {
        // Arrange
        UpdateAlbumCommand command = _updateAlbumCommandFixture.Create(metadata: _albumMetadataDtoFixture.Create(includeReleaseInfo: false));

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Metadata.ReleaseInfoCannotBeNull);
    }

    [Fact]
    public void Validate_WhenOriginalReleaseYearIsLessThan1_ShouldHaveValidationError()
    {
        // Arrange
        UpdateAlbumCommand command = _updateAlbumCommandFixture.Create(metadata: _albumMetadataDtoFixture.Create(releaseInfo: _releaseInfoDtoFixture.Create(originalReleaseYear: 0)));

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Metadata.OriginalReleaseYearMustBeBetween1And9999);
    }

    [Fact]
    public void Validate_WhenOriginalReleaseYearIsGreaterThan9999_ShouldHaveValidationError()
    {
        // Arrange
        UpdateAlbumCommand command = _updateAlbumCommandFixture.Create(metadata: _albumMetadataDtoFixture.Create(releaseInfo: _releaseInfoDtoFixture.Create(originalReleaseYear: 10000)));

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Metadata.OriginalReleaseYearMustBeBetween1And9999);
    }

    [Fact]
    public void Validate_WhenOriginalReleaseYearIsValid_ShouldNotHaveValidationError()
    {
        // Arrange
        UpdateAlbumCommand command = _updateAlbumCommandFixture.Create(metadata: _albumMetadataDtoFixture.Create(releaseInfo: _releaseInfoDtoFixture.Create(includeOriginalReleaseDate: false, originalReleaseYear: new Faker().Random.Int(2000, 2005), reReleaseYear: new Faker().Random.Int(2005, 2010), includeReReleaseDate: false)));

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationError(Errors.Metadata.OriginalReleaseYearMustBeBetween1And9999);
    }

    [Fact]
    public void Validate_WhenReReleaseYearIsLessThan1_ShouldHaveValidationError()
    {
        // Arrange
        UpdateAlbumCommand command = _updateAlbumCommandFixture.Create(metadata: _albumMetadataDtoFixture.Create(releaseInfo: _releaseInfoDtoFixture.Create(reReleaseYear: 0)));

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Metadata.ReReleaseYearMustBeBetween1And9999);
    }

    [Fact]
    public void Validate_WhenReReleaseYearIsGreaterThan9999_ShouldHaveValidationError()
    {
        // Arrange
        UpdateAlbumCommand command = _updateAlbumCommandFixture.Create(metadata: _albumMetadataDtoFixture.Create(releaseInfo: _releaseInfoDtoFixture.Create(reReleaseYear: 10000)));

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Metadata.ReReleaseYearMustBeBetween1And9999);
    }

    [Fact]
    public void Validate_WhenReReleaseYearIsValid_ShouldNotHaveValidationError()
    {
        // Arrange
        UpdateAlbumCommand command = _updateAlbumCommandFixture.Create(metadata: _albumMetadataDtoFixture.Create(releaseInfo: _releaseInfoDtoFixture.Create(originalReleaseYear: new Faker().Random.Int(2000, 2005), reReleaseYear: new Faker().Random.Int(2005, 2010), includeReReleaseDate: false)));

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationError(Errors.Metadata.ReReleaseYearMustBeBetween1And9999);
    }

    [Fact]
    public void Validate_WhenReleaseVersionExceeds50Characters_ShouldHaveValidationError()
    {
        // Arrange
        UpdateAlbumCommand command = _updateAlbumCommandFixture.Create(metadata: _albumMetadataDtoFixture.Create(releaseInfo: _releaseInfoDtoFixture.Create(releaseVersion: new Faker().Random.String2(51))));

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Metadata.ReleaseVersionMustBeMaximum50CharactersLong);
    }

    [Fact]
    public void Validate_WhenReleaseVersionIsValid_ShouldNotHaveValidationError()
    {
        // Arrange
        UpdateAlbumCommand command = _updateAlbumCommandFixture.Create(metadata: _albumMetadataDtoFixture.Create(releaseInfo: _releaseInfoDtoFixture.Create(releaseVersion: new Faker().Random.String2(50))));

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationError(Errors.Metadata.ReleaseVersionMustBeMaximum50CharactersLong);
    }

    [Fact]
    public void Validate_WhenOriginalReleaseDateAndYearDoNotMatch_ShouldHaveValidationError()
    {
        // Arrange
        UpdateAlbumCommand command = _updateAlbumCommandFixture.Create(metadata: _albumMetadataDtoFixture.Create(releaseInfo: _releaseInfoDtoFixture.Create(originalReleaseDate: new DateOnly(2020, 1, 1), originalReleaseYear: 2019, includeReReleaseDate: false)));

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Metadata.OriginalReleaseDateAndYearMustMatch);
    }

    [Fact]
    public void Validate_WhenOriginalReleaseDateAndYearMatch_ShouldNotHaveValidationError()
    {
        // Arrange
        UpdateAlbumCommand command = _updateAlbumCommandFixture.Create(metadata: _albumMetadataDtoFixture.Create(releaseInfo: _releaseInfoDtoFixture.Create(originalReleaseDate: new DateOnly(2020, 1, 1), originalReleaseYear: 2020, includeReReleaseDate: false)));

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationError(Errors.Metadata.OriginalReleaseDateAndYearMustMatch);
    }

    [Fact]
    public void Validate_WhenReReleaseDateAndYearDoNotMatch_ShouldHaveValidationError()
    {
        // Arrange
        UpdateAlbumCommand command = _updateAlbumCommandFixture.Create(metadata: _albumMetadataDtoFixture.Create(releaseInfo: _releaseInfoDtoFixture.Create(reReleaseDate: new DateOnly(2021, 1, 1), reReleaseYear: 2020)));

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Metadata.ReReleaseDateAndYearMustMatch);
    }

    [Fact]
    public void Validate_WhenReReleaseDateAndYearMatch_ShouldNotHaveValidationError()
    {
        // Arrange
        UpdateAlbumCommand command = _updateAlbumCommandFixture.Create(metadata: _albumMetadataDtoFixture.Create(releaseInfo: _releaseInfoDtoFixture.Create(reReleaseDate: new DateOnly(2021, 1, 1), reReleaseYear: 2021)));

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationError(Errors.Metadata.ReReleaseDateAndYearMustMatch);
    }

    [Fact]
    public void Validate_WhenReReleaseYearIsBeforeOriginalReleaseYear_ShouldHaveValidationError()
    {
        // Arrange
        UpdateAlbumCommand command = _updateAlbumCommandFixture.Create(metadata: _albumMetadataDtoFixture.Create(releaseInfo: _releaseInfoDtoFixture.Create(originalReleaseYear: 2001, reReleaseYear: 2000, includeReReleaseDate: false)));

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Metadata.ReReleaseYearCannotBeEarlierThanOriginalReleaseYear);
    }

    [Fact]
    public void Validate_WhenReReleaseYearIsAfterOriginalReleaseYear_ShouldNotHaveValidationError()
    {
        // Arrange
        UpdateAlbumCommand command = _updateAlbumCommandFixture.Create(metadata: _albumMetadataDtoFixture.Create(releaseInfo: _releaseInfoDtoFixture.Create(originalReleaseYear: 2000, reReleaseYear: 2001, includeReReleaseDate: false)));

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationError(Errors.Metadata.ReReleaseYearCannotBeEarlierThanOriginalReleaseYear);
    }

    [Fact]
    public void Validate_WhenReReleaseDateIsBeforeOriginalReleaseDate_ShouldHaveValidationError()
    {
        // Arrange
        UpdateAlbumCommand command = _updateAlbumCommandFixture.Create(metadata: _albumMetadataDtoFixture.Create(releaseInfo: _releaseInfoDtoFixture.Create(originalReleaseDate: new DateOnly(2001, 1, 1), reReleaseDate: new DateOnly(2000, 1, 1))));

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Metadata.ReReleaseDateCannotBeEarlierThanOriginalReleaseDate);
    }

    [Fact]
    public void Validate_WhenReReleaseDateIsAfterOriginalReleaseDate_ShouldNotHaveValidationError()
    {
        // Arrange
        UpdateAlbumCommand command = _updateAlbumCommandFixture.Create(metadata: _albumMetadataDtoFixture.Create(releaseInfo: _releaseInfoDtoFixture.Create(originalReleaseDate: new DateOnly(2000, 1, 1), reReleaseDate: new DateOnly(2001, 1, 1))));

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationError(Errors.Metadata.ReReleaseDateCannotBeEarlierThanOriginalReleaseDate);
    }

    [Fact]
    public void Validate_WhenGenresIsNull_ShouldHaveValidationError()
    {
        // Arrange
        UpdateAlbumCommand command = _updateAlbumCommandFixture.Create(metadata: _albumMetadataDtoFixture.Create(includeGenres: false));

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Metadata.GenresListCannotBeNull);
    }

    [Fact]
    public void Validate_WhenGenreNameIsEmpty_ShouldHaveValidationError()
    {
        // Arrange
        UpdateAlbumCommand command = _updateAlbumCommandFixture.Create(metadata: _albumMetadataDtoFixture.Create(genres: [_genreDtoFixture.Create(name: string.Empty)]));

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Metadata.GenreNameCannotBeEmpty);
    }

    [Fact]
    public void Validate_WhenGenreNameExceeds50Characters_ShouldHaveValidationError()
    {
        // Arrange
        UpdateAlbumCommand command = _updateAlbumCommandFixture.Create(metadata: _albumMetadataDtoFixture.Create(genres: [_genreDtoFixture.Create(name: new Faker().Random.String2(51))]));

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Metadata.GenreNameMustBeMaximum50CharactersLong);
    }

    [Fact]
    public void Validate_WhenGenresAreValid_ShouldNotHaveValidationError()
    {
        // Arrange
        UpdateAlbumCommand command = _updateAlbumCommandFixture.Create(metadata: _albumMetadataDtoFixture.Create(genres: [_genreDtoFixture.Create(name: new Faker().Random.String2(50))]));

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationError(Errors.Metadata.GenresListCannotBeNull);
        result.ShouldNotHaveValidationError(Errors.Metadata.GenreNameCannotBeEmpty);
        result.ShouldNotHaveValidationError(Errors.Metadata.GenreNameMustBeMaximum50CharactersLong);
    }

    [Fact]
    public void Validate_WhenTagsIsNull_ShouldHaveValidationError()
    {
        // Arrange
        UpdateAlbumCommand command = _updateAlbumCommandFixture.Create(metadata: _albumMetadataDtoFixture.Create(includeTags: false));

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Metadata.TagsListCannotBeNull);
    }

    [Fact]
    public void Validate_WhenTagNameIsEmpty_ShouldHaveValidationError()
    {
        // Arrange
        UpdateAlbumCommand command = _updateAlbumCommandFixture.Create(metadata: _albumMetadataDtoFixture.Create(tags: [_tagDtoFixture.Create(name: string.Empty)]));

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Metadata.TagNameCannotBeEmpty);
    }

    [Fact]
    public void Validate_WhenTagNameExceeds50Characters_ShouldHaveValidationError()
    {
        // Arrange
        UpdateAlbumCommand command = _updateAlbumCommandFixture.Create(metadata: _albumMetadataDtoFixture.Create(tags: [_tagDtoFixture.Create(name: new Faker().Random.String2(51))]));

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Metadata.TagNameMustBeMaximum50CharactersLong);
    }

    [Fact]
    public void Validate_WhenTagsAreValid_ShouldNotHaveValidationError()
    {
        // Arrange
        UpdateAlbumCommand command = _updateAlbumCommandFixture.Create(metadata: _albumMetadataDtoFixture.Create(tags: [_tagDtoFixture.Create(name: new Faker().Random.String2(50))]));

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationError(Errors.Metadata.TagsListCannotBeNull);
        result.ShouldNotHaveValidationError(Errors.Metadata.TagNameCannotBeEmpty);
        result.ShouldNotHaveValidationError(Errors.Metadata.TagNameMustBeMaximum50CharactersLong);
    }

    [Fact]
    public void Validate_WhenLanguageIsNull_ShouldNotHaveValidationError()
    {
        // Arrange
        UpdateAlbumCommand command = _updateAlbumCommandFixture.Create(metadata: _albumMetadataDtoFixture.Create(includeLanguage: false));

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationError(Errors.Metadata.LanguageCodeCannotBeEmpty);
    }

    [Fact]
    public void Validate_WhenLanguageCodeIsEmpty_ShouldHaveValidationError()
    {
        // Arrange
        UpdateAlbumCommand command = _updateAlbumCommandFixture.Create(metadata: _albumMetadataDtoFixture.Create(language: _languageInfoDtoFixture.Create(languageCode: string.Empty)));

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Metadata.LanguageCodeCannotBeEmpty);
    }

    [Fact]
    public void Validate_WhenLanguageCodeIsNot2CharactersLong_ShouldHaveValidationError()
    {
        // Arrange
        UpdateAlbumCommand command = _updateAlbumCommandFixture.Create(metadata: _albumMetadataDtoFixture.Create(language: _languageInfoDtoFixture.Create(languageCode: new Faker().Random.String2(3))));

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Metadata.LanguageCodeMustBe2CharactersLong);
    }

    [Fact]
    public void Validate_WhenLanguageCodeIsValid_ShouldNotHaveValidationError()
    {
        // Arrange
        UpdateAlbumCommand command = _updateAlbumCommandFixture.Create(metadata: _albumMetadataDtoFixture.Create(language: _languageInfoDtoFixture.Create(languageCode: new Faker().Random.String2(2))));

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationError(Errors.Metadata.LanguageCodeMustBe2CharactersLong);
    }

    [Fact]
    public void Validate_WhenLanguageNameIsEmpty_ShouldHaveValidationError()
    {
        // Arrange
        UpdateAlbumCommand command = _updateAlbumCommandFixture.Create(metadata: _albumMetadataDtoFixture.Create(language: _languageInfoDtoFixture.Create(languageName: string.Empty)));

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Metadata.LanguageNameCannotBeEmpty);
    }

    [Fact]
    public void Validate_WhenLanguageNameExceeds50Characters_ShouldHaveValidationError()
    {
        // Arrange
        UpdateAlbumCommand command = _updateAlbumCommandFixture.Create(metadata: _albumMetadataDtoFixture.Create(language: _languageInfoDtoFixture.Create(languageName: new Faker().Random.String2(51))));

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Metadata.LanguageNameMustBeMaximum50CharactersLong);
    }

    [Fact]
    public void Validate_WhenLanguageNativeNameIsNull_ShouldNotHaveValidationError()
    {
        // Arrange
        UpdateAlbumCommand command = _updateAlbumCommandFixture.Create(metadata: _albumMetadataDtoFixture.Create(language: _languageInfoDtoFixture.Create(includeNativeName: false)));

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationError(Errors.Metadata.LanguageNativeNameMustBeMaximum50CharactersLong);
    }

    [Fact]
    public void Validate_WhenLanguageNativeNameExceeds50Characters_ShouldHaveValidationError()
    {
        // Arrange
        UpdateAlbumCommand command = _updateAlbumCommandFixture.Create(metadata: _albumMetadataDtoFixture.Create(language: _languageInfoDtoFixture.Create(nativeName: new Faker().Random.String2(51))));

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Metadata.LanguageNativeNameMustBeMaximum50CharactersLong);
    }

    [Fact]
    public void Validate_WhenOriginalLanguageIsNull_ShouldNotHaveValidationError()
    {
        // Arrange
        UpdateAlbumCommand command = _updateAlbumCommandFixture.Create(metadata: _albumMetadataDtoFixture.Create(includeOriginalLanguage: false));

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationError(Errors.Metadata.LanguageCodeCannotBeEmpty);
    }

    [Fact]
    public void Validate_WhenOriginalLanguageCodeIsEmpty_ShouldHaveValidationError()
    {
        // Arrange
        UpdateAlbumCommand command = _updateAlbumCommandFixture.Create(metadata: _albumMetadataDtoFixture.Create(originalLanguage: _languageInfoDtoFixture.Create(languageCode: string.Empty)));

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Metadata.LanguageCodeCannotBeEmpty);
    }

    [Fact]
    public void Validate_WhenOriginalLanguageCodeIsNot2CharactersLong_ShouldHaveValidationError()
    {
        // Arrange
        UpdateAlbumCommand command = _updateAlbumCommandFixture.Create(metadata: _albumMetadataDtoFixture.Create(originalLanguage: _languageInfoDtoFixture.Create(languageCode: new Faker().Random.String2(3))));

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Metadata.LanguageCodeMustBe2CharactersLong);
    }

    [Fact]
    public void Validate_WhenOriginalLanguageNameIsEmpty_ShouldHaveValidationError()
    {
        // Arrange
        UpdateAlbumCommand command = _updateAlbumCommandFixture.Create(metadata: _albumMetadataDtoFixture.Create(originalLanguage: _languageInfoDtoFixture.Create(languageName: string.Empty)));

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Metadata.LanguageNameCannotBeEmpty);
    }

    [Fact]
    public void Validate_WhenOriginalLanguageNameExceeds50Characters_ShouldHaveValidationError()
    {
        // Arrange
        UpdateAlbumCommand command = _updateAlbumCommandFixture.Create(metadata: _albumMetadataDtoFixture.Create(originalLanguage: _languageInfoDtoFixture.Create(languageName: new Faker().Random.String2(51))));

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Metadata.LanguageNameMustBeMaximum50CharactersLong);
    }

    [Fact]
    public void Validate_WhenOriginalLanguageNativeNameIsNull_ShouldNotHaveValidationError()
    {
        // Arrange
        UpdateAlbumCommand command = _updateAlbumCommandFixture.Create(metadata: _albumMetadataDtoFixture.Create(originalLanguage: _languageInfoDtoFixture.Create(includeNativeName: false)));

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationError(Errors.Metadata.LanguageNativeNameMustBeMaximum50CharactersLong);
    }

    [Fact]
    public void Validate_WhenOriginalLanguageNativeNameExceeds50Characters_ShouldHaveValidationError()
    {
        // Arrange
        UpdateAlbumCommand command = _updateAlbumCommandFixture.Create(metadata: _albumMetadataDtoFixture.Create(originalLanguage: _languageInfoDtoFixture.Create(nativeName: new Faker().Random.String2(51))));

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Metadata.LanguageNativeNameMustBeMaximum50CharactersLong);
    }

    [Fact]
    public void Validate_WhenMediaFormatIsNotDefined_ShouldHaveValidationError()
    {
        // Arrange
        UpdateAlbumCommand command = _updateAlbumCommandFixture.Create(mediaFormat: (MusicMediaFormat)999);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Music.UnknownMusicMediaFormat);
    }

    [Fact]
    public void Validate_WhenMediaFormatIsNull_ShouldNotHaveValidationError()
    {
        // Arrange
        UpdateAlbumCommand command = _updateAlbumCommandFixture.Create(includeMediaFormat: false, includeBarcode: false, includeCatalogNumbers: false, includeMusicBrainzReleaseId: false, includeMusicBrainzReleaseGroupId: false, includeMusicBrainzReleaseArtistId: false);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationError(Errors.Music.UnknownMusicMediaFormat);
    }

    [Fact]
    public void Validate_WhenMediaFormatIsDefined_ShouldNotHaveValidationError()
    {
        // Arrange
        UpdateAlbumCommand command = _updateAlbumCommandFixture.Create(mediaFormat: MusicMediaFormat.CD);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationError(Errors.Music.UnknownMusicMediaFormat);
    }

    [Fact]
    public void Validate_WhenCatalogNumberExceeds50Characters_ShouldHaveValidationError()
    {
        // Arrange
        UpdateAlbumCommand command = _updateAlbumCommandFixture.Create(catalogNumbers: [new Faker().Random.String2(51)]);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Music.CatalogNumberMustBeMaximum50CharactersLong);
    }

    [Fact]
    public void Validate_WhenCatalogNumberIsNull_ShouldNotHaveValidationError()
    {
        // Arrange
        UpdateAlbumCommand command = _updateAlbumCommandFixture.Create(includeMediaFormat: false, includeBarcode: false, includeCatalogNumbers: false, includeMusicBrainzReleaseId: false, includeMusicBrainzReleaseGroupId: false, includeMusicBrainzReleaseArtistId: false);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationError(Errors.Music.CatalogNumberMustBeMaximum50CharactersLong);
    }

    [Fact]
    public void Validate_WhenCatalogNumberIsValid_ShouldNotHaveValidationError()
    {
        // Arrange
        UpdateAlbumCommand command = _updateAlbumCommandFixture.Create(catalogNumbers: [new Faker().Random.String2(50)]);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationError(Errors.Music.CatalogNumberMustBeMaximum50CharactersLong);
    }

    [Fact]
    public void Validate_WhenBarcodeIsEmpty_ShouldHaveValidationError()
    {
        // Arrange
        UpdateAlbumCommand command = _updateAlbumCommandFixture.Create(barcode: string.Empty);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Music.BarcodeValueCannotBeEmpty);
    }

    [Theory]
    [InlineData("123")] // too short to be a barcode
    [InlineData("abcdefghijkl")] // not numeric
    [InlineData("12345678901234")] // too long to be a barcode
    public void Validate_WhenBarcodeHasInvalidFormat_ShouldHaveValidationError(string barcode)
    {
        // Arrange
        UpdateAlbumCommand command = _updateAlbumCommandFixture.Create(barcode: barcode);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Music.InvalidFormatForBarcode);
    }

    [Theory]
    [InlineData("123456789012")] // 12 digit barcode
    [InlineData("1234567890123")] // 13 digit barcode
    public void Validate_WhenBarcodeIsValid_ShouldNotHaveValidationError(string barcode)
    {
        // Arrange
        UpdateAlbumCommand command = _updateAlbumCommandFixture.Create(barcode: barcode);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationError(Errors.Music.BarcodeValueCannotBeEmpty);
        result.ShouldNotHaveValidationError(Errors.Music.InvalidFormatForBarcode);
    }

    [Fact]
    public void Validate_WhenBarcodeIsNull_ShouldNotHaveValidationError()
    {
        // Arrange
        UpdateAlbumCommand command = _updateAlbumCommandFixture.Create(includeMediaFormat: false, includeBarcode: false, includeCatalogNumbers: false, includeMusicBrainzReleaseId: false, includeMusicBrainzReleaseGroupId: false, includeMusicBrainzReleaseArtistId: false);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationError(Errors.Music.BarcodeValueCannotBeEmpty);
    }

    [Fact]
    public void Validate_WhenMusicBrainzReleaseIdIsEmptyGuid_ShouldHaveValidationError()
    {
        // Arrange
        UpdateAlbumCommand command = _updateAlbumCommandFixture.Create(musicBrainzReleaseId: Guid.Empty);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Music.MusicBrainzIdInvalidFormat);
    }

    [Fact]
    public void Validate_WhenMusicBrainzReleaseGroupIdIsEmptyGuid_ShouldHaveValidationError()
    {
        // Arrange
        UpdateAlbumCommand command = _updateAlbumCommandFixture.Create(musicBrainzReleaseGroupId: Guid.Empty);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Music.MusicBrainzIdInvalidFormat);
    }

    [Fact]
    public void Validate_WhenMusicBrainzReleaseArtistIdIsEmptyGuid_ShouldHaveValidationError()
    {
        // Arrange
        UpdateAlbumCommand command = _updateAlbumCommandFixture.Create(musicBrainzReleaseArtistId: Guid.Empty);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Music.MusicBrainzIdInvalidFormat);
    }

    [Fact]
    public void Validate_WhenMusicBrainzIdsAreNull_ShouldNotHaveValidationError()
    {
        // Arrange
        UpdateAlbumCommand command = _updateAlbumCommandFixture.Create(includeMediaFormat: false, includeBarcode: false, includeCatalogNumbers: false, includeMusicBrainzReleaseId: false, includeMusicBrainzReleaseGroupId: false, includeMusicBrainzReleaseArtistId: false);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationError(Errors.Music.MusicBrainzIdInvalidFormat);
    }

    [Fact]
    public void Validate_WhenMusicBrainzIdsAreValid_ShouldNotHaveValidationError()
    {
        // Arrange
        UpdateAlbumCommand command = _updateAlbumCommandFixture.Create(
            musicBrainzReleaseId: Guid.NewGuid(),
            musicBrainzReleaseGroupId: Guid.NewGuid(),
            musicBrainzReleaseArtistId: Guid.NewGuid());

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationError(Errors.Music.MusicBrainzIdInvalidFormat);
    }

    [Fact]
    public void Validate_WhenContributorsIsNull_ShouldHaveValidationError()
    {
        // Arrange
        UpdateAlbumCommand command = _updateAlbumCommandFixture.Create(includeContributors: false);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.MediaContributor.ContributorsListCannotBeNull);
    }

    [Fact]
    public void Validate_WhenContributorIdIsEmpty_ShouldHaveValidationError()
    {
        // Arrange
        UpdateAlbumCommand command = _updateAlbumCommandFixture.Create(contributors: [_mediaContributorReferenceDtoFixture.Create(contributorId: Guid.Empty, role: MediaContributorRole.Vocals)]);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.MediaContributor.MediaContributorIdCannotBeEmpty);
    }

    [Fact]
    public void Validate_WhenContributorRoleIsNotDefined_ShouldHaveValidationError()
    {
        // Arrange
        UpdateAlbumCommand command = _updateAlbumCommandFixture.Create(contributors: [_mediaContributorReferenceDtoFixture.Create(contributorId: Guid.NewGuid(), role: (MediaContributorRole)999)]);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.MediaContributor.UnknownMediaContributorRole);
    }

    [Fact]
    public void Validate_WhenContributorsAreValid_ShouldNotHaveValidationError()
    {
        // Arrange
        UpdateAlbumCommand command = _updateAlbumCommandFixture.Create(contributors: [_mediaContributorReferenceDtoFixture.Create(contributorId: Guid.NewGuid(), role: MediaContributorRole.Vocals)]);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationError(Errors.MediaContributor.ContributorsListCannotBeNull);
        result.ShouldNotHaveValidationError(Errors.MediaContributor.MediaContributorIdCannotBeEmpty);
        result.ShouldNotHaveValidationError(Errors.MediaContributor.UnknownMediaContributorRole);
    }

    [Fact]
    public void Validate_WhenRatingsIsNull_ShouldHaveValidationError()
    {
        // Arrange
        UpdateAlbumCommand command = _updateAlbumCommandFixture.Create(includeRatings: false);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Metadata.RatingsListCannotBeNull);
    }

    [Theory]
    [InlineData(0)] // zero is not a valid rating value
    [InlineData(-1)] // a negative rating value is not valid
    public void Validate_WhenRatingValueIsNotPositive_ShouldHaveValidationError(decimal value)
    {
        // Arrange
        UpdateAlbumCommand command = _updateAlbumCommandFixture.Create(ratings: [_audioRatingDtoFixture.Create(value: value, maxValue: 5)]);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Metadata.RatingValueMustBePositive);
    }

    [Fact]
    public void Validate_WhenRatingValueIsNull_ShouldNotHaveValidationError()
    {
        // Arrange
        UpdateAlbumCommand command = _updateAlbumCommandFixture.Create(ratings: [_audioRatingDtoFixture.Create(includeValue: false, maxValue: 5)]);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationError(Errors.Metadata.RatingValueMustBePositive);
    }

    [Fact]
    public void Validate_WhenRatingValueIsGreaterThanMaxValue_ShouldHaveValidationError()
    {
        // Arrange
        UpdateAlbumCommand command = _updateAlbumCommandFixture.Create(ratings: [_audioRatingDtoFixture.Create(value: 6, maxValue: 5)]);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Metadata.RatingValueCannotBeGreaterThanMaxValue);
    }

    [Theory]
    [InlineData(0)] // zero is not a valid maximum rating value
    [InlineData(-1)] // a negative maximum rating value is not valid
    public void Validate_WhenRatingMaxValueIsNotPositive_ShouldHaveValidationError(decimal maxValue)
    {
        // Arrange
        UpdateAlbumCommand command = _updateAlbumCommandFixture.Create(ratings: [_audioRatingDtoFixture.Create(value: 1, maxValue: maxValue)]);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Metadata.RatingMaxValueMustBePositive);
    }

    [Fact]
    public void Validate_WhenRatingMaxValueIsNull_ShouldNotHaveValidationError()
    {
        // Arrange
        UpdateAlbumCommand command = _updateAlbumCommandFixture.Create(ratings: [_audioRatingDtoFixture.Create(value: 1, includeMaxValue: false)]);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationError(Errors.Metadata.RatingMaxValueMustBePositive);
    }

    [Fact]
    public void Validate_WhenRatingVoteCountIsNegative_ShouldHaveValidationError()
    {
        // Arrange
        UpdateAlbumCommand command = _updateAlbumCommandFixture.Create(ratings: [_audioRatingDtoFixture.Create(value: 4, maxValue: 5, voteCount: -1)]);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Metadata.RatingVoteCountMustBePositive);
    }

    [Fact]
    public void Validate_WhenRatingVoteCountIsNull_ShouldNotHaveValidationError()
    {
        // Arrange
        UpdateAlbumCommand command = _updateAlbumCommandFixture.Create(ratings: [_audioRatingDtoFixture.Create(value: 4, maxValue: 5, includeVoteCount: false)]);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationError(Errors.Metadata.RatingVoteCountMustBePositive);
    }

    [Fact]
    public void Validate_WhenRatingsAreValid_ShouldNotHaveValidationError()
    {
        // Arrange
        UpdateAlbumCommand command = _updateAlbumCommandFixture.Create(ratings: [_audioRatingDtoFixture.Create(value: 4, maxValue: 5, voteCount: 100)]);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationError(Errors.Metadata.RatingsListCannotBeNull);
        result.ShouldNotHaveValidationError(Errors.Metadata.RatingValueMustBePositive);
        result.ShouldNotHaveValidationError(Errors.Metadata.RatingValueCannotBeGreaterThanMaxValue);
        result.ShouldNotHaveValidationError(Errors.Metadata.RatingMaxValueMustBePositive);
        result.ShouldNotHaveValidationError(Errors.Metadata.RatingVoteCountMustBePositive);
    }

    [Fact]
    public void Validate_WhenLanguageNameIsValid_ShouldNotHaveValidationError()
    {
        // Arrange
        UpdateAlbumCommand command = _updateAlbumCommandFixture.Create(metadata: _albumMetadataDtoFixture.Create(language: _languageInfoDtoFixture.Create(languageName: new Faker().Random.String2(20))));

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationError(Errors.Metadata.LanguageNameCannotBeEmpty);
        result.ShouldNotHaveValidationError(Errors.Metadata.LanguageNameMustBeMaximum50CharactersLong);
    }

    [Fact]
    public void Validate_WhenLanguageNativeNameIsValid_ShouldNotHaveValidationError()
    {
        // Arrange
        UpdateAlbumCommand command = _updateAlbumCommandFixture.Create(metadata: _albumMetadataDtoFixture.Create(language: _languageInfoDtoFixture.Create(nativeName: new Faker().Random.String2(20))));

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationError(Errors.Metadata.LanguageNativeNameMustBeMaximum50CharactersLong);
    }

    [Fact]
    public void Validate_WhenOriginalLanguageCodeIsValid_ShouldNotHaveValidationError()
    {
        // Arrange
        UpdateAlbumCommand command = _updateAlbumCommandFixture.Create(metadata: _albumMetadataDtoFixture.Create(originalLanguage: _languageInfoDtoFixture.Create(languageCode: new Faker().Random.String2(2))));

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationError(Errors.Metadata.LanguageCodeMustBe2CharactersLong);
    }

    [Fact]
    public void Validate_WhenOriginalLanguageNameIsValid_ShouldNotHaveValidationError()
    {
        // Arrange
        UpdateAlbumCommand command = _updateAlbumCommandFixture.Create(metadata: _albumMetadataDtoFixture.Create(originalLanguage: _languageInfoDtoFixture.Create(languageName: new Faker().Random.String2(20))));

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationError(Errors.Metadata.LanguageNameCannotBeEmpty);
        result.ShouldNotHaveValidationError(Errors.Metadata.LanguageNameMustBeMaximum50CharactersLong);
    }

    [Fact]
    public void Validate_WhenOriginalLanguageNativeNameIsValid_ShouldNotHaveValidationError()
    {
        // Arrange
        UpdateAlbumCommand command = _updateAlbumCommandFixture.Create(metadata: _albumMetadataDtoFixture.Create(originalLanguage: _languageInfoDtoFixture.Create(nativeName: new Faker().Random.String2(20))));

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationError(Errors.Metadata.LanguageNativeNameMustBeMaximum50CharactersLong);
    }

    [Fact]
    public void Validate_WhenReleaseVersionIsNull_ShouldNotHaveValidationError()
    {
        // Arrange
        UpdateAlbumCommand command = _updateAlbumCommandFixture.Create(metadata: _albumMetadataDtoFixture.Create(releaseInfo: _releaseInfoDtoFixture.Create(includeReleaseVersion: false)));

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationError(Errors.Metadata.ReleaseVersionMustBeMaximum50CharactersLong);
    }

    [Fact]
    public void Validate_WhenCommandIsValid_ShouldNotHaveAnyValidationErrors()
    {
        // Arrange
        UpdateAlbumCommand command = _updateAlbumCommandFixture.Create();

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }
}
