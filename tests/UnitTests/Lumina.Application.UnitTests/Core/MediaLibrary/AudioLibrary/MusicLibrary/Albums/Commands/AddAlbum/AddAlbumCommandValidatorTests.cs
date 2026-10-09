#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Application.Core.MediaLibrary.AudioLibrary.MusicLibrary.Albums.Commands.AddAlbum;
using Lumina.Application.Core.MediaLibrary.AudioLibrary.MusicLibrary.Tracks.Commands.AddTrack;
using Lumina.Application.Fixtures.Core.MediaLibrary.AudioLibrary.MusicLibrary.Albums.Commands.AddAlbum;
using Lumina.Application.Fixtures.Core.MediaLibrary.AudioLibrary.MusicLibrary.Tracks.Commands.AddTrack;
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

namespace Lumina.Application.UnitTests.Core.MediaLibrary.AudioLibrary.MusicLibrary.Albums.Commands.AddAlbum;

/// <summary>
/// Contains unit tests for the <see cref="AddAlbumCommandValidator"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class AddAlbumCommandValidatorTests
{
    private readonly AddAlbumCommandFixture _addAlbumCommandFixture = new();
    private readonly AddAlbumCommandValidator _validator = new();
    private readonly MusicAlbumMetadataDtoFixture _albumMetadataDtoFixture = new();
    private readonly MusicTrackMetadataDtoFixture _audioMetadataDtoFixture = new();
    private readonly MusicWorkDtoFixture _musicWorkDtoFixture = new();
    private readonly ReleaseInfoDtoFixture _releaseInfoDtoFixture = new();
    private readonly GenreDtoFixture _genreDtoFixture = new();
    private readonly TagDtoFixture _tagDtoFixture = new();
    private readonly LanguageInfoDtoFixture _languageInfoDtoFixture = new();
    private readonly MediaContributorReferenceDtoFixture _mediaContributorReferenceDtoFixture = new();
    private readonly AudioRatingDtoFixture _audioRatingDtoFixture = new();
    private readonly AddTrackCommandFixture _addTrackCommandFixture = new();
    private readonly MoodDtoFixture _moodDtoFixture = new();
    private readonly IsrcDtoFixture _isrcDtoFixture = new();

    [Theory]
    [InlineData(null)] // missing route value
    [InlineData("")] // empty route value
    [InlineData("   ")] // whitespace route value, which both the not empty rule and the format rule reject
    [InlineData("not-a-library-guid")] // non-Guid route value
    [InlineData("00000000-0000-0000-0000-000000000000")] // empty Guid route value
    public void Validate_WhenLibraryIdIsEmptyOrInvalid_ShouldHaveValidationError(string? libraryId)
    {
        // Arrange
        AddAlbumCommand command = _addAlbumCommandFixture.Create(libraryId: libraryId, includeLibraryId: libraryId is not null);

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
        AddAlbumCommand command = _addAlbumCommandFixture.Create(artistId: artistId, includeArtistId: artistId is not null);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Music.ArtistIdCannotBeEmpty);
    }

    [Fact]
    public void Validate_WhenMetadataIsNull_ShouldHaveValidationError()
    {
        // Arrange
        AddAlbumCommand command = _addAlbumCommandFixture.Create(includeMetadata: false);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Metadata.MetadataCannotBeNull);
    }

    [Fact]
    public void Validate_WhenTitleIsNull_ShouldHaveValidationError()
    {
        // Arrange
        AddAlbumCommand command = _addAlbumCommandFixture.Create(metadata: _albumMetadataDtoFixture.Create(includeTitle: false));

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
        AddAlbumCommand command = _addAlbumCommandFixture.Create(metadata: _albumMetadataDtoFixture.Create(title: title));

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Music.AlbumTitleCannotBeEmpty);
    }

    [Fact]
    public void Validate_WhenTitleExceeds255Characters_ShouldHaveValidationError()
    {
        // Arrange
        AddAlbumCommand command = _addAlbumCommandFixture.Create(metadata: _albumMetadataDtoFixture.Create(title: new Faker().Random.String2(300)));

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Music.AlbumTitleMustBeMaximum255CharactersLong);
    }

    [Fact]
    public void Validate_WhenTitleIsValid_ShouldNotHaveValidationError()
    {
        // Arrange
        AddAlbumCommand command = _addAlbumCommandFixture.Create(metadata: _albumMetadataDtoFixture.Create(title: new Faker().Random.String2(200)));

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
        AddAlbumCommand command = _addAlbumCommandFixture.Create(metadata: _albumMetadataDtoFixture.Create(originalTitle: new Faker().Random.String2(300)));

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Metadata.OriginalTitleMustBeMaximum255CharactersLong);
    }

    [Fact]
    public void Validate_WhenOriginalTitleIsNull_ShouldNotHaveValidationError()
    {
        // Arrange
        AddAlbumCommand command = _addAlbumCommandFixture.Create(metadata: _albumMetadataDtoFixture.Create(includeOriginalTitle: false));

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationError(Errors.Metadata.OriginalTitleMustBeMaximum255CharactersLong);
    }

    [Fact]
    public void Validate_WhenOriginalTitleIsValid_ShouldNotHaveValidationError()
    {
        // Arrange
        AddAlbumCommand command = _addAlbumCommandFixture.Create(metadata: _albumMetadataDtoFixture.Create(originalTitle: new Faker().Random.String2(200)));

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationError(Errors.Metadata.OriginalTitleMustBeMaximum255CharactersLong);
    }

    [Fact]
    public void Validate_WhenDescriptionExceeds2000Characters_ShouldHaveValidationError()
    {
        // Arrange
        AddAlbumCommand command = _addAlbumCommandFixture.Create(metadata: _albumMetadataDtoFixture.Create(description: new Faker().Random.String2(2001)));

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Metadata.DescriptionMustBeMaximum2000CharactersLong);
    }

    [Fact]
    public void Validate_WhenDescriptionIsNull_ShouldNotHaveValidationError()
    {
        // Arrange
        AddAlbumCommand command = _addAlbumCommandFixture.Create(metadata: _albumMetadataDtoFixture.Create(includeDescription: false));

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationError(Errors.Metadata.DescriptionMustBeMaximum2000CharactersLong);
    }

    [Fact]
    public void Validate_WhenDescriptionIsValid_ShouldNotHaveValidationError()
    {
        // Arrange
        AddAlbumCommand command = _addAlbumCommandFixture.Create(metadata: _albumMetadataDtoFixture.Create(description: new Faker().Random.String2(1500)));

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationError(Errors.Metadata.DescriptionMustBeMaximum2000CharactersLong);
    }

    [Fact]
    public void Validate_WhenReleaseTypeIsNotDefined_ShouldHaveValidationError()
    {
        // Arrange
        AddAlbumCommand command = _addAlbumCommandFixture.Create(metadata: _albumMetadataDtoFixture.Create(releaseTypes: [(MusicReleaseType)999]));

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Music.UnknownMusicReleaseType);
    }

    [Fact]
    public void Validate_WhenReleaseTypeIsNull_ShouldNotHaveValidationError()
    {
        // Arrange
        AddAlbumCommand command = _addAlbumCommandFixture.Create(metadata: _albumMetadataDtoFixture.Create(includeReleaseTypes: false));

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationError(Errors.Music.UnknownMusicReleaseType);
    }

    [Fact]
    public void Validate_WhenReleaseTypeIsDefined_ShouldNotHaveValidationError()
    {
        // Arrange
        AddAlbumCommand command = _addAlbumCommandFixture.Create(metadata: _albumMetadataDtoFixture.Create(releaseTypes: [MusicReleaseType.Album]));

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationError(Errors.Music.UnknownMusicReleaseType);
    }

    [Fact]
    public void Validate_WhenReleaseStatusIsNotDefined_ShouldHaveValidationError()
    {
        // Arrange
        AddAlbumCommand command = _addAlbumCommandFixture.Create(metadata: _albumMetadataDtoFixture.Create(releaseStatus: (MusicReleaseStatus)999));

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Music.UnknownMusicReleaseStatus);
    }

    [Fact]
    public void Validate_WhenReleaseStatusIsNull_ShouldNotHaveValidationError()
    {
        // Arrange
        AddAlbumCommand command = _addAlbumCommandFixture.Create(metadata: _albumMetadataDtoFixture.Create(includeReleaseStatus: false));

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationError(Errors.Music.UnknownMusicReleaseStatus);
    }

    [Fact]
    public void Validate_WhenReleaseStatusIsDefined_ShouldNotHaveValidationError()
    {
        // Arrange
        AddAlbumCommand command = _addAlbumCommandFixture.Create(metadata: _albumMetadataDtoFixture.Create(releaseStatus: MusicReleaseStatus.Official));

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
        AddAlbumCommand command = _addAlbumCommandFixture.Create(metadata: _albumMetadataDtoFixture.Create(totalDiscs: totalDiscs));

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Music.TotalDiscsMustBeGreaterThanZero);
    }

    [Fact]
    public void Validate_WhenTotalDiscsIsNull_ShouldNotHaveValidationError()
    {
        // Arrange
        AddAlbumCommand command = _addAlbumCommandFixture.Create(metadata: _albumMetadataDtoFixture.Create(includeTotalDiscs: false));

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationError(Errors.Music.TotalDiscsMustBeGreaterThanZero);
    }

    [Fact]
    public void Validate_WhenTotalDiscsIsPositive_ShouldNotHaveValidationError()
    {
        // Arrange
        AddAlbumCommand command = _addAlbumCommandFixture.Create(metadata: _albumMetadataDtoFixture.Create(totalDiscs: 2));

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
        AddAlbumCommand command = _addAlbumCommandFixture.Create(metadata: _albumMetadataDtoFixture.Create(totalTracks: totalTracks));

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Music.TotalTracksMustBeGreaterThanZero);
    }

    [Fact]
    public void Validate_WhenTotalTracksIsNull_ShouldNotHaveValidationError()
    {
        // Arrange
        AddAlbumCommand command = _addAlbumCommandFixture.Create(metadata: _albumMetadataDtoFixture.Create(includeTotalTracks: false));

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationError(Errors.Music.TotalTracksMustBeGreaterThanZero);
    }

    [Fact]
    public void Validate_WhenTotalTracksIsPositive_ShouldNotHaveValidationError()
    {
        // Arrange
        AddAlbumCommand command = _addAlbumCommandFixture.Create(metadata: _albumMetadataDtoFixture.Create(totalTracks: 10));

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationError(Errors.Music.TotalTracksMustBeGreaterThanZero);
    }

    [Fact]
    public void Validate_WhenReleaseInfoIsNull_ShouldHaveValidationError()
    {
        // Arrange
        AddAlbumCommand command = _addAlbumCommandFixture.Create(metadata: _albumMetadataDtoFixture.Create(includeReleaseInfo: false));

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Metadata.ReleaseInfoCannotBeNull);
    }

    [Fact]
    public void Validate_WhenOriginalReleaseYearIsLessThan1_ShouldHaveValidationError()
    {
        // Arrange
        AddAlbumCommand command = _addAlbumCommandFixture.Create(metadata: _albumMetadataDtoFixture.Create(releaseInfo: _releaseInfoDtoFixture.Create(originalReleaseYear: 0)));

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Metadata.OriginalReleaseYearMustBeBetween1And9999);
    }

    [Fact]
    public void Validate_WhenOriginalReleaseYearIsGreaterThan9999_ShouldHaveValidationError()
    {
        // Arrange
        AddAlbumCommand command = _addAlbumCommandFixture.Create(metadata: _albumMetadataDtoFixture.Create(releaseInfo: _releaseInfoDtoFixture.Create(originalReleaseYear: 10000)));

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Metadata.OriginalReleaseYearMustBeBetween1And9999);
    }

    [Fact]
    public void Validate_WhenOriginalReleaseYearIsValid_ShouldNotHaveValidationError()
    {
        // Arrange
        AddAlbumCommand command = _addAlbumCommandFixture.Create(metadata: _albumMetadataDtoFixture.Create(releaseInfo: _releaseInfoDtoFixture.Create(includeOriginalReleaseDate: false, originalReleaseYear: new Faker().Random.Int(2000, 2005), reReleaseYear: new Faker().Random.Int(2005, 2010), includeReReleaseDate: false)));

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationError(Errors.Metadata.OriginalReleaseYearMustBeBetween1And9999);
    }

    [Fact]
    public void Validate_WhenReReleaseYearIsLessThan1_ShouldHaveValidationError()
    {
        // Arrange
        AddAlbumCommand command = _addAlbumCommandFixture.Create(metadata: _albumMetadataDtoFixture.Create(releaseInfo: _releaseInfoDtoFixture.Create(reReleaseYear: 0)));

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Metadata.ReReleaseYearMustBeBetween1And9999);
    }

    [Fact]
    public void Validate_WhenReReleaseYearIsGreaterThan9999_ShouldHaveValidationError()
    {
        // Arrange
        AddAlbumCommand command = _addAlbumCommandFixture.Create(metadata: _albumMetadataDtoFixture.Create(releaseInfo: _releaseInfoDtoFixture.Create(reReleaseYear: 10000)));

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Metadata.ReReleaseYearMustBeBetween1And9999);
    }

    [Fact]
    public void Validate_WhenReReleaseYearIsValid_ShouldNotHaveValidationError()
    {
        // Arrange
        AddAlbumCommand command = _addAlbumCommandFixture.Create(metadata: _albumMetadataDtoFixture.Create(releaseInfo: _releaseInfoDtoFixture.Create(originalReleaseYear: new Faker().Random.Int(2000, 2005), reReleaseYear: new Faker().Random.Int(2005, 2010), includeReReleaseDate: false)));

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationError(Errors.Metadata.ReReleaseYearMustBeBetween1And9999);
    }

    [Fact]
    public void Validate_WhenReleaseVersionExceeds50Characters_ShouldHaveValidationError()
    {
        // Arrange
        AddAlbumCommand command = _addAlbumCommandFixture.Create(metadata: _albumMetadataDtoFixture.Create(releaseInfo: _releaseInfoDtoFixture.Create(releaseVersion: new Faker().Random.String2(51))));

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Metadata.ReleaseVersionMustBeMaximum50CharactersLong);
    }

    [Fact]
    public void Validate_WhenReleaseVersionIsValid_ShouldNotHaveValidationError()
    {
        // Arrange
        AddAlbumCommand command = _addAlbumCommandFixture.Create(metadata: _albumMetadataDtoFixture.Create(releaseInfo: _releaseInfoDtoFixture.Create(releaseVersion: new Faker().Random.String2(50))));

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationError(Errors.Metadata.ReleaseVersionMustBeMaximum50CharactersLong);
    }

    [Fact]
    public void Validate_WhenOriginalReleaseDateAndYearDoNotMatch_ShouldHaveValidationError()
    {
        // Arrange
        AddAlbumCommand command = _addAlbumCommandFixture.Create(metadata: _albumMetadataDtoFixture.Create(releaseInfo: _releaseInfoDtoFixture.Create(originalReleaseDate: new DateOnly(2020, 1, 1), originalReleaseYear: 2019, includeReReleaseDate: false)));

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Metadata.OriginalReleaseDateAndYearMustMatch);
    }

    [Fact]
    public void Validate_WhenOriginalReleaseDateAndYearMatch_ShouldNotHaveValidationError()
    {
        // Arrange
        AddAlbumCommand command = _addAlbumCommandFixture.Create(metadata: _albumMetadataDtoFixture.Create(releaseInfo: _releaseInfoDtoFixture.Create(originalReleaseDate: new DateOnly(2020, 1, 1), originalReleaseYear: 2020, includeReReleaseDate: false)));

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationError(Errors.Metadata.OriginalReleaseDateAndYearMustMatch);
    }

    [Fact]
    public void Validate_WhenReReleaseDateAndYearDoNotMatch_ShouldHaveValidationError()
    {
        // Arrange
        AddAlbumCommand command = _addAlbumCommandFixture.Create(metadata: _albumMetadataDtoFixture.Create(releaseInfo: _releaseInfoDtoFixture.Create(reReleaseDate: new DateOnly(2021, 1, 1), reReleaseYear: 2020)));

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Metadata.ReReleaseDateAndYearMustMatch);
    }

    [Fact]
    public void Validate_WhenReReleaseDateAndYearMatch_ShouldNotHaveValidationError()
    {
        // Arrange
        AddAlbumCommand command = _addAlbumCommandFixture.Create(metadata: _albumMetadataDtoFixture.Create(releaseInfo: _releaseInfoDtoFixture.Create(reReleaseDate: new DateOnly(2021, 1, 1), reReleaseYear: 2021)));

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationError(Errors.Metadata.ReReleaseDateAndYearMustMatch);
    }

    [Fact]
    public void Validate_WhenReReleaseYearIsBeforeOriginalReleaseYear_ShouldHaveValidationError()
    {
        // Arrange
        AddAlbumCommand command = _addAlbumCommandFixture.Create(metadata: _albumMetadataDtoFixture.Create(releaseInfo: _releaseInfoDtoFixture.Create(originalReleaseYear: 2001, reReleaseYear: 2000, includeReReleaseDate: false)));

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Metadata.ReReleaseYearCannotBeEarlierThanOriginalReleaseYear);
    }

    [Fact]
    public void Validate_WhenReReleaseYearIsAfterOriginalReleaseYear_ShouldNotHaveValidationError()
    {
        // Arrange
        AddAlbumCommand command = _addAlbumCommandFixture.Create(metadata: _albumMetadataDtoFixture.Create(releaseInfo: _releaseInfoDtoFixture.Create(originalReleaseYear: 2000, reReleaseYear: 2001, includeReReleaseDate: false)));

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationError(Errors.Metadata.ReReleaseYearCannotBeEarlierThanOriginalReleaseYear);
    }

    [Fact]
    public void Validate_WhenReReleaseDateIsBeforeOriginalReleaseDate_ShouldHaveValidationError()
    {
        // Arrange
        AddAlbumCommand command = _addAlbumCommandFixture.Create(metadata: _albumMetadataDtoFixture.Create(releaseInfo: _releaseInfoDtoFixture.Create(originalReleaseDate: new DateOnly(2001, 1, 1), reReleaseDate: new DateOnly(2000, 1, 1))));

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Metadata.ReReleaseDateCannotBeEarlierThanOriginalReleaseDate);
    }

    [Fact]
    public void Validate_WhenReReleaseDateIsAfterOriginalReleaseDate_ShouldNotHaveValidationError()
    {
        // Arrange
        AddAlbumCommand command = _addAlbumCommandFixture.Create(metadata: _albumMetadataDtoFixture.Create(releaseInfo: _releaseInfoDtoFixture.Create(originalReleaseDate: new DateOnly(2000, 1, 1), reReleaseDate: new DateOnly(2001, 1, 1))));

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationError(Errors.Metadata.ReReleaseDateCannotBeEarlierThanOriginalReleaseDate);
    }

    [Fact]
    public void Validate_WhenGenresIsNull_ShouldHaveValidationError()
    {
        // Arrange
        AddAlbumCommand command = _addAlbumCommandFixture.Create(metadata: _albumMetadataDtoFixture.Create(includeGenres: false));

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Metadata.GenresListCannotBeNull);
    }

    [Fact]
    public void Validate_WhenGenreNameIsEmpty_ShouldHaveValidationError()
    {
        // Arrange
        AddAlbumCommand command = _addAlbumCommandFixture.Create(metadata: _albumMetadataDtoFixture.Create(genres: [_genreDtoFixture.Create(name: string.Empty)]));

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Metadata.GenreNameCannotBeEmpty);
    }

    [Fact]
    public void Validate_WhenGenreNameExceeds50Characters_ShouldHaveValidationError()
    {
        // Arrange
        AddAlbumCommand command = _addAlbumCommandFixture.Create(metadata: _albumMetadataDtoFixture.Create(genres: [_genreDtoFixture.Create(name: new Faker().Random.String2(51))]));

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Metadata.GenreNameMustBeMaximum50CharactersLong);
    }

    [Fact]
    public void Validate_WhenGenresAreValid_ShouldNotHaveValidationError()
    {
        // Arrange
        AddAlbumCommand command = _addAlbumCommandFixture.Create(metadata: _albumMetadataDtoFixture.Create(genres: [_genreDtoFixture.Create(name: new Faker().Random.String2(50))]));

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
        AddAlbumCommand command = _addAlbumCommandFixture.Create(metadata: _albumMetadataDtoFixture.Create(includeTags: false));

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Metadata.TagsListCannotBeNull);
    }

    [Fact]
    public void Validate_WhenTagNameIsEmpty_ShouldHaveValidationError()
    {
        // Arrange
        AddAlbumCommand command = _addAlbumCommandFixture.Create(metadata: _albumMetadataDtoFixture.Create(tags: [_tagDtoFixture.Create(name: string.Empty)]));

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Metadata.TagNameCannotBeEmpty);
    }

    [Fact]
    public void Validate_WhenTagNameExceeds50Characters_ShouldHaveValidationError()
    {
        // Arrange
        AddAlbumCommand command = _addAlbumCommandFixture.Create(metadata: _albumMetadataDtoFixture.Create(tags: [_tagDtoFixture.Create(name: new Faker().Random.String2(51))]));

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Metadata.TagNameMustBeMaximum50CharactersLong);
    }

    [Fact]
    public void Validate_WhenTagsAreValid_ShouldNotHaveValidationError()
    {
        // Arrange
        AddAlbumCommand command = _addAlbumCommandFixture.Create(metadata: _albumMetadataDtoFixture.Create(tags: [_tagDtoFixture.Create(name: new Faker().Random.String2(50))]));

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
        AddAlbumCommand command = _addAlbumCommandFixture.Create(metadata: _albumMetadataDtoFixture.Create(includeLanguage: false));

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationError(Errors.Metadata.LanguageCodeCannotBeEmpty);
    }

    [Fact]
    public void Validate_WhenLanguageCodeIsEmpty_ShouldHaveValidationError()
    {
        // Arrange
        AddAlbumCommand command = _addAlbumCommandFixture.Create(metadata: _albumMetadataDtoFixture.Create(language: _languageInfoDtoFixture.Create(languageCode: string.Empty)));

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Metadata.LanguageCodeCannotBeEmpty);
    }

    [Fact]
    public void Validate_WhenLanguageCodeIsNot2CharactersLong_ShouldHaveValidationError()
    {
        // Arrange
        AddAlbumCommand command = _addAlbumCommandFixture.Create(metadata: _albumMetadataDtoFixture.Create(language: _languageInfoDtoFixture.Create(languageCode: new Faker().Random.String2(3))));

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Metadata.LanguageCodeMustBe2CharactersLong);
    }

    [Fact]
    public void Validate_WhenLanguageCodeIsValid_ShouldNotHaveValidationError()
    {
        // Arrange
        AddAlbumCommand command = _addAlbumCommandFixture.Create(metadata: _albumMetadataDtoFixture.Create(language: _languageInfoDtoFixture.Create(languageCode: new Faker().Random.String2(2))));

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationError(Errors.Metadata.LanguageCodeMustBe2CharactersLong);
    }

    [Fact]
    public void Validate_WhenLanguageNameIsEmpty_ShouldHaveValidationError()
    {
        // Arrange
        AddAlbumCommand command = _addAlbumCommandFixture.Create(metadata: _albumMetadataDtoFixture.Create(language: _languageInfoDtoFixture.Create(languageName: string.Empty)));

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Metadata.LanguageNameCannotBeEmpty);
    }

    [Fact]
    public void Validate_WhenLanguageNameExceeds50Characters_ShouldHaveValidationError()
    {
        // Arrange
        AddAlbumCommand command = _addAlbumCommandFixture.Create(metadata: _albumMetadataDtoFixture.Create(language: _languageInfoDtoFixture.Create(languageName: new Faker().Random.String2(51))));

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Metadata.LanguageNameMustBeMaximum50CharactersLong);
    }

    [Fact]
    public void Validate_WhenLanguageNativeNameIsNull_ShouldNotHaveValidationError()
    {
        // Arrange
        AddAlbumCommand command = _addAlbumCommandFixture.Create(metadata: _albumMetadataDtoFixture.Create(language: _languageInfoDtoFixture.Create(includeNativeName: false)));

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationError(Errors.Metadata.LanguageNativeNameMustBeMaximum50CharactersLong);
    }

    [Fact]
    public void Validate_WhenLanguageNativeNameExceeds50Characters_ShouldHaveValidationError()
    {
        // Arrange
        AddAlbumCommand command = _addAlbumCommandFixture.Create(metadata: _albumMetadataDtoFixture.Create(language: _languageInfoDtoFixture.Create(nativeName: new Faker().Random.String2(51))));

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Metadata.LanguageNativeNameMustBeMaximum50CharactersLong);
    }

    [Fact]
    public void Validate_WhenOriginalLanguageIsNull_ShouldNotHaveValidationError()
    {
        // Arrange
        AddAlbumCommand command = _addAlbumCommandFixture.Create(metadata: _albumMetadataDtoFixture.Create(includeOriginalLanguage: false));

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationError(Errors.Metadata.LanguageCodeCannotBeEmpty);
    }

    [Fact]
    public void Validate_WhenOriginalLanguageCodeIsEmpty_ShouldHaveValidationError()
    {
        // Arrange
        AddAlbumCommand command = _addAlbumCommandFixture.Create(metadata: _albumMetadataDtoFixture.Create(originalLanguage: _languageInfoDtoFixture.Create(languageCode: string.Empty)));

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Metadata.LanguageCodeCannotBeEmpty);
    }

    [Fact]
    public void Validate_WhenOriginalLanguageCodeIsNot2CharactersLong_ShouldHaveValidationError()
    {
        // Arrange
        AddAlbumCommand command = _addAlbumCommandFixture.Create(metadata: _albumMetadataDtoFixture.Create(originalLanguage: _languageInfoDtoFixture.Create(languageCode: new Faker().Random.String2(3))));

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Metadata.LanguageCodeMustBe2CharactersLong);
    }

    [Fact]
    public void Validate_WhenOriginalLanguageNameIsEmpty_ShouldHaveValidationError()
    {
        // Arrange
        AddAlbumCommand command = _addAlbumCommandFixture.Create(metadata: _albumMetadataDtoFixture.Create(originalLanguage: _languageInfoDtoFixture.Create(languageName: string.Empty)));

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Metadata.LanguageNameCannotBeEmpty);
    }

    [Fact]
    public void Validate_WhenOriginalLanguageNameExceeds50Characters_ShouldHaveValidationError()
    {
        // Arrange
        AddAlbumCommand command = _addAlbumCommandFixture.Create(metadata: _albumMetadataDtoFixture.Create(originalLanguage: _languageInfoDtoFixture.Create(languageName: new Faker().Random.String2(51))));

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Metadata.LanguageNameMustBeMaximum50CharactersLong);
    }

    [Fact]
    public void Validate_WhenOriginalLanguageNativeNameIsNull_ShouldNotHaveValidationError()
    {
        // Arrange
        AddAlbumCommand command = _addAlbumCommandFixture.Create(metadata: _albumMetadataDtoFixture.Create(originalLanguage: _languageInfoDtoFixture.Create(includeNativeName: false)));

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationError(Errors.Metadata.LanguageNativeNameMustBeMaximum50CharactersLong);
    }

    [Fact]
    public void Validate_WhenOriginalLanguageNativeNameExceeds50Characters_ShouldHaveValidationError()
    {
        // Arrange
        AddAlbumCommand command = _addAlbumCommandFixture.Create(metadata: _albumMetadataDtoFixture.Create(originalLanguage: _languageInfoDtoFixture.Create(nativeName: new Faker().Random.String2(51))));

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Metadata.LanguageNativeNameMustBeMaximum50CharactersLong);
    }

    [Fact]
    public void Validate_WhenMediaFormatIsNotDefined_ShouldHaveValidationError()
    {
        // Arrange
        AddAlbumCommand command = _addAlbumCommandFixture.Create(mediaFormat: (MusicMediaFormat)999);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Music.UnknownMusicMediaFormat);
    }

    [Fact]
    public void Validate_WhenMediaFormatIsNull_ShouldNotHaveValidationError()
    {
        // Arrange
        AddAlbumCommand command = _addAlbumCommandFixture.Create(includeMediaFormat: false, includeBarcode: false, includeCatalogNumbers: false, includeMusicBrainzReleaseId: false, includeMusicBrainzReleaseGroupId: false, includeMusicBrainzReleaseArtistId: false);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationError(Errors.Music.UnknownMusicMediaFormat);
    }

    [Fact]
    public void Validate_WhenMediaFormatIsDefined_ShouldNotHaveValidationError()
    {
        // Arrange
        AddAlbumCommand command = _addAlbumCommandFixture.Create(mediaFormat: MusicMediaFormat.CD);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationError(Errors.Music.UnknownMusicMediaFormat);
    }

    [Fact]
    public void Validate_WhenCatalogNumberExceeds50Characters_ShouldHaveValidationError()
    {
        // Arrange
        AddAlbumCommand command = _addAlbumCommandFixture.Create(catalogNumbers: [new Faker().Random.String2(51)]);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Music.CatalogNumberMustBeMaximum50CharactersLong);
    }

    [Fact]
    public void Validate_WhenCatalogNumberIsNull_ShouldNotHaveValidationError()
    {
        // Arrange
        AddAlbumCommand command = _addAlbumCommandFixture.Create(includeMediaFormat: false, includeBarcode: false, includeCatalogNumbers: false, includeMusicBrainzReleaseId: false, includeMusicBrainzReleaseGroupId: false, includeMusicBrainzReleaseArtistId: false);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationError(Errors.Music.CatalogNumberMustBeMaximum50CharactersLong);
    }

    [Fact]
    public void Validate_WhenCatalogNumberIsValid_ShouldNotHaveValidationError()
    {
        // Arrange
        AddAlbumCommand command = _addAlbumCommandFixture.Create(catalogNumbers: [new Faker().Random.String2(50)]);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationError(Errors.Music.CatalogNumberMustBeMaximum50CharactersLong);
    }

    [Fact]
    public void Validate_WhenBarcodeIsEmpty_ShouldHaveValidationError()
    {
        // Arrange
        AddAlbumCommand command = _addAlbumCommandFixture.Create(barcode: string.Empty);

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
        AddAlbumCommand command = _addAlbumCommandFixture.Create(barcode: barcode);

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
        AddAlbumCommand command = _addAlbumCommandFixture.Create(barcode: barcode);

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
        AddAlbumCommand command = _addAlbumCommandFixture.Create(includeMediaFormat: false, includeBarcode: false, includeCatalogNumbers: false, includeMusicBrainzReleaseId: false, includeMusicBrainzReleaseGroupId: false, includeMusicBrainzReleaseArtistId: false);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationError(Errors.Music.BarcodeValueCannotBeEmpty);
    }

    [Fact]
    public void Validate_WhenMusicBrainzReleaseIdIsEmptyGuid_ShouldHaveValidationError()
    {
        // Arrange
        AddAlbumCommand command = _addAlbumCommandFixture.Create(musicBrainzReleaseId: Guid.Empty);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Music.MusicBrainzIdInvalidFormat);
    }

    [Fact]
    public void Validate_WhenMusicBrainzReleaseGroupIdIsEmptyGuid_ShouldHaveValidationError()
    {
        // Arrange
        AddAlbumCommand command = _addAlbumCommandFixture.Create(musicBrainzReleaseGroupId: Guid.Empty);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Music.MusicBrainzIdInvalidFormat);
    }

    [Fact]
    public void Validate_WhenMusicBrainzReleaseArtistIdIsEmptyGuid_ShouldHaveValidationError()
    {
        // Arrange
        AddAlbumCommand command = _addAlbumCommandFixture.Create(musicBrainzReleaseArtistId: Guid.Empty);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Music.MusicBrainzIdInvalidFormat);
    }

    [Fact]
    public void Validate_WhenMusicBrainzIdsAreNull_ShouldNotHaveValidationError()
    {
        // Arrange
        AddAlbumCommand command = _addAlbumCommandFixture.Create(includeMediaFormat: false, includeBarcode: false, includeCatalogNumbers: false, includeMusicBrainzReleaseId: false, includeMusicBrainzReleaseGroupId: false, includeMusicBrainzReleaseArtistId: false);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationError(Errors.Music.MusicBrainzIdInvalidFormat);
    }

    [Fact]
    public void Validate_WhenMusicBrainzIdsAreValid_ShouldNotHaveValidationError()
    {
        // Arrange
        AddAlbumCommand command = _addAlbumCommandFixture.Create(
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
        AddAlbumCommand command = _addAlbumCommandFixture.Create(includeContributors: false);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.MediaContributor.ContributorsListCannotBeNull);
    }

    [Fact]
    public void Validate_WhenContributorIdIsEmpty_ShouldHaveValidationError()
    {
        // Arrange
        AddAlbumCommand command = _addAlbumCommandFixture.Create(contributors: [_mediaContributorReferenceDtoFixture.Create(contributorId: Guid.Empty, role: MediaContributorRole.Vocals)]);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.MediaContributor.MediaContributorIdCannotBeEmpty);
    }

    [Fact]
    public void Validate_WhenContributorRoleIsNotDefined_ShouldHaveValidationError()
    {
        // Arrange
        AddAlbumCommand command = _addAlbumCommandFixture.Create(contributors: [_mediaContributorReferenceDtoFixture.Create(contributorId: Guid.NewGuid(), role: (MediaContributorRole)999)]);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.MediaContributor.UnknownMediaContributorRole);
    }

    [Fact]
    public void Validate_WhenContributorsAreValid_ShouldNotHaveValidationError()
    {
        // Arrange
        AddAlbumCommand command = _addAlbumCommandFixture.Create(contributors: [_mediaContributorReferenceDtoFixture.Create(contributorId: Guid.NewGuid(), role: MediaContributorRole.Vocals)]);

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
        AddAlbumCommand command = _addAlbumCommandFixture.Create(includeRatings: false);

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
        AddAlbumCommand command = _addAlbumCommandFixture.Create(ratings: [_audioRatingDtoFixture.Create(value: value, maxValue: 5)]);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Metadata.RatingValueMustBePositive);
    }

    [Fact]
    public void Validate_WhenRatingValueIsNull_ShouldNotHaveValidationError()
    {
        // Arrange
        AddAlbumCommand command = _addAlbumCommandFixture.Create(ratings: [_audioRatingDtoFixture.Create(includeValue: false, maxValue: 5)]);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationError(Errors.Metadata.RatingValueMustBePositive);
    }

    [Fact]
    public void Validate_WhenRatingValueIsGreaterThanMaxValue_ShouldHaveValidationError()
    {
        // Arrange
        AddAlbumCommand command = _addAlbumCommandFixture.Create(ratings: [_audioRatingDtoFixture.Create(value: 6, maxValue: 5)]);

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
        AddAlbumCommand command = _addAlbumCommandFixture.Create(ratings: [_audioRatingDtoFixture.Create(value: 1, maxValue: maxValue)]);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Metadata.RatingMaxValueMustBePositive);
    }

    [Fact]
    public void Validate_WhenRatingMaxValueIsNull_ShouldNotHaveValidationError()
    {
        // Arrange
        AddAlbumCommand command = _addAlbumCommandFixture.Create(ratings: [_audioRatingDtoFixture.Create(value: 1, includeMaxValue: false)]);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationError(Errors.Metadata.RatingMaxValueMustBePositive);
    }

    [Fact]
    public void Validate_WhenRatingVoteCountIsNegative_ShouldHaveValidationError()
    {
        // Arrange
        AddAlbumCommand command = _addAlbumCommandFixture.Create(ratings: [_audioRatingDtoFixture.Create(value: 4, maxValue: 5, voteCount: -1)]);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Metadata.RatingVoteCountMustBePositive);
    }

    [Fact]
    public void Validate_WhenRatingVoteCountIsNull_ShouldNotHaveValidationError()
    {
        // Arrange
        AddAlbumCommand command = _addAlbumCommandFixture.Create(ratings: [_audioRatingDtoFixture.Create(value: 4, maxValue: 5, includeVoteCount: false)]);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationError(Errors.Metadata.RatingVoteCountMustBePositive);
    }

    [Fact]
    public void Validate_WhenRatingsAreValid_ShouldNotHaveValidationError()
    {
        // Arrange
        AddAlbumCommand command = _addAlbumCommandFixture.Create(ratings: [_audioRatingDtoFixture.Create(value: 4, maxValue: 5, voteCount: 100)]);

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
    public void Validate_WhenTracksIsNull_ShouldHaveValidationError()
    {
        // Arrange
        AddAlbumCommand command = _addAlbumCommandFixture.Create(includeTracks: false);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Music.TracksListCannotBeNull);
    }

    [Fact]
    public void Validate_WhenTrackPathIsNull_ShouldHaveValidationError()
    {
        // Arrange
        AddAlbumCommand command = _addAlbumCommandFixture.Create(tracks: [_addTrackCommandFixture.Create(includePath: false)]);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Music.TrackPathCannotBeEmpty);
    }

    [Theory]
    [InlineData("")] // empty path
    [InlineData("   ")] // whitespace path, which the not empty rule rejects
    public void Validate_WhenTrackPathIsEmpty_ShouldHaveValidationError(string path)
    {
        // Arrange
        AddAlbumCommand command = _addAlbumCommandFixture.Create(tracks: [_addTrackCommandFixture.Create(path: path)]);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Music.TrackPathCannotBeEmpty);
    }

    [Fact]
    public void Validate_WhenTrackPathExceeds2048Characters_ShouldHaveValidationError()
    {
        // Arrange
        AddAlbumCommand command = _addAlbumCommandFixture.Create(tracks: [_addTrackCommandFixture.Create(path: "/" + new Faker().Random.String2(2048) + ".flac")]);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Music.TrackPathMustBeMaximum2048CharactersLong);
    }

    [Fact]
    public void Validate_WhenTrackPathIsValid_ShouldNotHaveValidationError()
    {
        // Arrange
        AddAlbumCommand command = _addAlbumCommandFixture.Create(tracks: [_addTrackCommandFixture.Create(path: "/" + new Faker().Random.String2(2000) + ".flac")]);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationError(Errors.Music.TrackPathCannotBeEmpty);
        result.ShouldNotHaveValidationError(Errors.Music.TrackPathMustBeMaximum2048CharactersLong);
    }

    [Fact]
    public void Validate_WhenTrackMetadataIsNull_ShouldHaveValidationError()
    {
        // Arrange
        AddAlbumCommand command = _addAlbumCommandFixture.Create(tracks: [_addTrackCommandFixture.Create(includeMetadata: false)]);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Metadata.MetadataCannotBeNull);
    }

    [Fact]
    public void Validate_WhenTrackTitleIsNull_ShouldHaveValidationError()
    {
        // Arrange
        AddAlbumCommand command = _addAlbumCommandFixture.Create(tracks: [_addTrackCommandFixture.Create(metadata: _audioMetadataDtoFixture.Create(includeTitle: false))]);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Metadata.TitleCannotBeEmpty);
    }

    [Fact]
    public void Validate_WhenTrackTitleExceeds255Characters_ShouldHaveValidationError()
    {
        // Arrange
        AddAlbumCommand command = _addAlbumCommandFixture.Create(tracks: [_addTrackCommandFixture.Create(metadata: _audioMetadataDtoFixture.Create(title: new Faker().Random.String2(300)))]);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Metadata.TitleMustBeMaximum255CharactersLong);
    }

    [Fact]
    public void Validate_WhenTrackTitleIsValid_ShouldNotHaveValidationError()
    {
        // Arrange
        AddAlbumCommand command = _addAlbumCommandFixture.Create(tracks: [_addTrackCommandFixture.Create(metadata: _audioMetadataDtoFixture.Create(title: new Faker().Random.String2(200)))]);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationError(Errors.Metadata.TitleCannotBeEmpty);
        result.ShouldNotHaveValidationError(Errors.Metadata.TitleMustBeMaximum255CharactersLong);
    }

    [Fact]
    public void Validate_WhenTrackOriginalTitleExceeds255Characters_ShouldHaveValidationError()
    {
        // Arrange
        AddAlbumCommand command = _addAlbumCommandFixture.Create(tracks: [_addTrackCommandFixture.Create(metadata: _audioMetadataDtoFixture.Create(originalTitle: new Faker().Random.String2(300)))]);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Metadata.OriginalTitleMustBeMaximum255CharactersLong);
    }

    [Fact]
    public void Validate_WhenTrackDescriptionExceeds2000Characters_ShouldHaveValidationError()
    {
        // Arrange
        AddAlbumCommand command = _addAlbumCommandFixture.Create(tracks: [_addTrackCommandFixture.Create(metadata: _audioMetadataDtoFixture.Create(description: new Faker().Random.String2(2001)))]);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Metadata.DescriptionMustBeMaximum2000CharactersLong);
    }

    [Fact]
    public void Validate_WhenTrackReleaseInfoIsNull_ShouldHaveValidationError()
    {
        // Arrange
        AddAlbumCommand command = _addAlbumCommandFixture.Create(tracks: [_addTrackCommandFixture.Create(metadata: _audioMetadataDtoFixture.Create(includeReleaseInfo: false))]);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Metadata.ReleaseInfoCannotBeNull);
    }

    [Fact]
    public void Validate_WhenTrackOriginalReleaseYearIsLessThan1_ShouldHaveValidationError()
    {
        // Arrange
        AddAlbumCommand command = _addAlbumCommandFixture.Create(tracks: [_addTrackCommandFixture.Create(metadata: _audioMetadataDtoFixture.Create(releaseInfo: _releaseInfoDtoFixture.Create(originalReleaseYear: 0)))]);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Metadata.OriginalReleaseYearMustBeBetween1And9999);
    }

    [Fact]
    public void Validate_WhenTrackOriginalReleaseYearIsGreaterThan9999_ShouldHaveValidationError()
    {
        // Arrange
        AddAlbumCommand command = _addAlbumCommandFixture.Create(tracks: [_addTrackCommandFixture.Create(metadata: _audioMetadataDtoFixture.Create(releaseInfo: _releaseInfoDtoFixture.Create(originalReleaseYear: 10000)))]);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Metadata.OriginalReleaseYearMustBeBetween1And9999);
    }

    [Fact]
    public void Validate_WhenTrackReReleaseYearIsLessThan1_ShouldHaveValidationError()
    {
        // Arrange
        AddAlbumCommand command = _addAlbumCommandFixture.Create(tracks: [_addTrackCommandFixture.Create(metadata: _audioMetadataDtoFixture.Create(releaseInfo: _releaseInfoDtoFixture.Create(reReleaseYear: 0)))]);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Metadata.ReReleaseYearMustBeBetween1And9999);
    }

    [Fact]
    public void Validate_WhenTrackReReleaseYearIsGreaterThan9999_ShouldHaveValidationError()
    {
        // Arrange
        AddAlbumCommand command = _addAlbumCommandFixture.Create(tracks: [_addTrackCommandFixture.Create(metadata: _audioMetadataDtoFixture.Create(releaseInfo: _releaseInfoDtoFixture.Create(reReleaseYear: 10000)))]);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Metadata.ReReleaseYearMustBeBetween1And9999);
    }

    [Fact]
    public void Validate_WhenTrackReleaseVersionExceeds50Characters_ShouldHaveValidationError()
    {
        // Arrange
        AddAlbumCommand command = _addAlbumCommandFixture.Create(tracks: [_addTrackCommandFixture.Create(metadata: _audioMetadataDtoFixture.Create(releaseInfo: _releaseInfoDtoFixture.Create(releaseVersion: new Faker().Random.String2(51))))]);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Metadata.ReleaseVersionMustBeMaximum50CharactersLong);
    }

    [Fact]
    public void Validate_WhenTrackOriginalReleaseDateAndYearDoNotMatch_ShouldHaveValidationError()
    {
        // Arrange
        AddAlbumCommand command = _addAlbumCommandFixture.Create(tracks: [_addTrackCommandFixture.Create(metadata: _audioMetadataDtoFixture.Create(releaseInfo: _releaseInfoDtoFixture.Create(originalReleaseDate: new DateOnly(2020, 1, 1), originalReleaseYear: 2019, includeReReleaseDate: false)))]);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Metadata.OriginalReleaseDateAndYearMustMatch);
    }

    [Fact]
    public void Validate_WhenTrackReReleaseDateAndYearDoNotMatch_ShouldHaveValidationError()
    {
        // Arrange
        AddAlbumCommand command = _addAlbumCommandFixture.Create(tracks: [_addTrackCommandFixture.Create(metadata: _audioMetadataDtoFixture.Create(releaseInfo: _releaseInfoDtoFixture.Create(reReleaseDate: new DateOnly(2021, 1, 1), reReleaseYear: 2020)))]);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Metadata.ReReleaseDateAndYearMustMatch);
    }

    [Fact]
    public void Validate_WhenTrackReReleaseYearIsBeforeOriginalReleaseYear_ShouldHaveValidationError()
    {
        // Arrange
        AddAlbumCommand command = _addAlbumCommandFixture.Create(tracks: [_addTrackCommandFixture.Create(metadata: _audioMetadataDtoFixture.Create(releaseInfo: _releaseInfoDtoFixture.Create(originalReleaseYear: 2001, reReleaseYear: 2000, includeReReleaseDate: false)))]);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Metadata.ReReleaseYearCannotBeEarlierThanOriginalReleaseYear);
    }

    [Fact]
    public void Validate_WhenTrackReReleaseDateIsBeforeOriginalReleaseDate_ShouldHaveValidationError()
    {
        // Arrange
        AddAlbumCommand command = _addAlbumCommandFixture.Create(tracks: [_addTrackCommandFixture.Create(metadata: _audioMetadataDtoFixture.Create(releaseInfo: _releaseInfoDtoFixture.Create(originalReleaseDate: new DateOnly(2001, 1, 1), reReleaseDate: new DateOnly(2000, 1, 1))))]);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Metadata.ReReleaseDateCannotBeEarlierThanOriginalReleaseDate);
    }

    [Fact]
    public void Validate_WhenTrackGenresIsNull_ShouldHaveValidationError()
    {
        // Arrange
        AddAlbumCommand command = _addAlbumCommandFixture.Create(tracks: [_addTrackCommandFixture.Create(metadata: _audioMetadataDtoFixture.Create(includeGenres: false))]);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Metadata.GenresListCannotBeNull);
    }

    [Fact]
    public void Validate_WhenTrackGenreNameIsEmpty_ShouldHaveValidationError()
    {
        // Arrange
        AddAlbumCommand command = _addAlbumCommandFixture.Create(tracks: [_addTrackCommandFixture.Create(metadata: _audioMetadataDtoFixture.Create(genres: [_genreDtoFixture.Create(name: string.Empty)]))]);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Metadata.GenreNameCannotBeEmpty);
    }

    [Fact]
    public void Validate_WhenTrackGenreNameExceeds50Characters_ShouldHaveValidationError()
    {
        // Arrange
        AddAlbumCommand command = _addAlbumCommandFixture.Create(tracks: [_addTrackCommandFixture.Create(metadata: _audioMetadataDtoFixture.Create(genres: [_genreDtoFixture.Create(name: new Faker().Random.String2(51))]))]);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Metadata.GenreNameMustBeMaximum50CharactersLong);
    }

    [Fact]
    public void Validate_WhenTrackTagsIsNull_ShouldHaveValidationError()
    {
        // Arrange
        AddAlbumCommand command = _addAlbumCommandFixture.Create(tracks: [_addTrackCommandFixture.Create(metadata: _audioMetadataDtoFixture.Create(includeTags: false))]);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Metadata.TagsListCannotBeNull);
    }

    [Fact]
    public void Validate_WhenTrackTagNameIsEmpty_ShouldHaveValidationError()
    {
        // Arrange
        AddAlbumCommand command = _addAlbumCommandFixture.Create(tracks: [_addTrackCommandFixture.Create(metadata: _audioMetadataDtoFixture.Create(tags: [_tagDtoFixture.Create(name: string.Empty)]))]);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Metadata.TagNameCannotBeEmpty);
    }

    [Fact]
    public void Validate_WhenTrackTagNameExceeds50Characters_ShouldHaveValidationError()
    {
        // Arrange
        AddAlbumCommand command = _addAlbumCommandFixture.Create(tracks: [_addTrackCommandFixture.Create(metadata: _audioMetadataDtoFixture.Create(tags: [_tagDtoFixture.Create(name: new Faker().Random.String2(51))]))]);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Metadata.TagNameMustBeMaximum50CharactersLong);
    }

    [Fact]
    public void Validate_WhenTrackLanguageCodeIsEmpty_ShouldHaveValidationError()
    {
        // Arrange
        AddAlbumCommand command = _addAlbumCommandFixture.Create(tracks: [_addTrackCommandFixture.Create(metadata: _audioMetadataDtoFixture.Create(language: _languageInfoDtoFixture.Create(languageCode: string.Empty)))]);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Metadata.LanguageCodeCannotBeEmpty);
    }

    [Fact]
    public void Validate_WhenTrackLanguageCodeIsNot2CharactersLong_ShouldHaveValidationError()
    {
        // Arrange
        AddAlbumCommand command = _addAlbumCommandFixture.Create(tracks: [_addTrackCommandFixture.Create(metadata: _audioMetadataDtoFixture.Create(language: _languageInfoDtoFixture.Create(languageCode: new Faker().Random.String2(3))))]);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Metadata.LanguageCodeMustBe2CharactersLong);
    }

    [Fact]
    public void Validate_WhenTrackLanguageNameIsEmpty_ShouldHaveValidationError()
    {
        // Arrange
        AddAlbumCommand command = _addAlbumCommandFixture.Create(tracks: [_addTrackCommandFixture.Create(metadata: _audioMetadataDtoFixture.Create(language: _languageInfoDtoFixture.Create(languageName: string.Empty)))]);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Metadata.LanguageNameCannotBeEmpty);
    }

    [Fact]
    public void Validate_WhenTrackLanguageNameExceeds50Characters_ShouldHaveValidationError()
    {
        // Arrange
        AddAlbumCommand command = _addAlbumCommandFixture.Create(tracks: [_addTrackCommandFixture.Create(metadata: _audioMetadataDtoFixture.Create(language: _languageInfoDtoFixture.Create(languageName: new Faker().Random.String2(51))))]);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Metadata.LanguageNameMustBeMaximum50CharactersLong);
    }

    [Fact]
    public void Validate_WhenTrackLanguageNativeNameExceeds50Characters_ShouldHaveValidationError()
    {
        // Arrange
        AddAlbumCommand command = _addAlbumCommandFixture.Create(tracks: [_addTrackCommandFixture.Create(metadata: _audioMetadataDtoFixture.Create(language: _languageInfoDtoFixture.Create(nativeName: new Faker().Random.String2(51))))]);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Metadata.LanguageNativeNameMustBeMaximum50CharactersLong);
    }

    [Fact]
    public void Validate_WhenTrackOriginalLanguageCodeIsEmpty_ShouldHaveValidationError()
    {
        // Arrange
        AddAlbumCommand command = _addAlbumCommandFixture.Create(tracks: [_addTrackCommandFixture.Create(metadata: _audioMetadataDtoFixture.Create(originalLanguage: _languageInfoDtoFixture.Create(languageCode: string.Empty)))]);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Metadata.LanguageCodeCannotBeEmpty);
    }

    [Fact]
    public void Validate_WhenTrackOriginalLanguageNameIsEmpty_ShouldHaveValidationError()
    {
        // Arrange
        AddAlbumCommand command = _addAlbumCommandFixture.Create(tracks: [_addTrackCommandFixture.Create(metadata: _audioMetadataDtoFixture.Create(originalLanguage: _languageInfoDtoFixture.Create(languageName: string.Empty)))]);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Metadata.LanguageNameCannotBeEmpty);
    }

    [Fact]
    public void Validate_WhenTrackOriginalLanguageNativeNameExceeds50Characters_ShouldHaveValidationError()
    {
        // Arrange
        AddAlbumCommand command = _addAlbumCommandFixture.Create(tracks: [_addTrackCommandFixture.Create(metadata: _audioMetadataDtoFixture.Create(originalLanguage: _languageInfoDtoFixture.Create(nativeName: new Faker().Random.String2(51))))]);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Metadata.LanguageNativeNameMustBeMaximum50CharactersLong);
    }

    [Fact]
    public void Validate_WhenTrackMusicBrainzRecordingIdIsEmptyGuid_ShouldHaveValidationError()
    {
        // Arrange
        AddAlbumCommand command = _addAlbumCommandFixture.Create(tracks: [_addTrackCommandFixture.Create(musicBrainzRecordingId: Guid.Empty)]);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Music.MusicBrainzIdInvalidFormat);
    }

    [Fact]
    public void Validate_WhenTrackMusicBrainzTrackIdIsEmptyGuid_ShouldHaveValidationError()
    {
        // Arrange
        AddAlbumCommand command = _addAlbumCommandFixture.Create(tracks: [_addTrackCommandFixture.Create(musicBrainzTrackId: Guid.Empty)]);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Music.MusicBrainzIdInvalidFormat);
    }

    [Fact]
    public void Validate_WhenTrackMusicBrainzWorkIdIsEmptyGuid_ShouldHaveValidationError()
    {
        // Arrange
        AddAlbumCommand command = _addAlbumCommandFixture.Create(tracks: [_addTrackCommandFixture.Create(work: _musicWorkDtoFixture.Create(musicBrainzWorkId: Guid.Empty))]);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Music.MusicBrainzIdInvalidFormat);
    }

    [Fact]
    public void Validate_WhenTrackMusicBrainzIdsAreNull_ShouldNotHaveValidationError()
    {
        // Arrange
        AddAlbumCommand command = _addAlbumCommandFixture.Create(tracks: [_addTrackCommandFixture.Create(includeDiscNumber: false, includeScript: false, includeKey: false, includeBpm: false, includeWork: false, includeMusicBrainzRecordingId: false, includeMusicBrainzTrackId: false, includeMoods: false, includeIsrcs: false)]);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationError(Errors.Music.MusicBrainzIdInvalidFormat);
    }

    [Fact]
    public void Validate_WhenTrackNumberIsNull_ShouldHaveValidationError()
    {
        // Arrange
        AddAlbumCommand command = _addAlbumCommandFixture.Create(tracks: [_addTrackCommandFixture.Create(includeTrackNumber: false)]);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Music.TrackNumberMustBeGreaterThanZero);
    }

    [Theory]
    [InlineData(0)] // zero is not a valid track number
    [InlineData(-1)] // a negative track number is not valid
    public void Validate_WhenTrackNumberIsNotPositive_ShouldHaveValidationError(int trackNumber)
    {
        // Arrange
        AddAlbumCommand command = _addAlbumCommandFixture.Create(tracks: [_addTrackCommandFixture.Create(trackNumber: trackNumber)]);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Music.TrackNumberMustBeGreaterThanZero);
    }

    [Fact]
    public void Validate_WhenTrackNumberIsPositive_ShouldNotHaveValidationError()
    {
        // Arrange
        AddAlbumCommand command = _addAlbumCommandFixture.Create(tracks: [_addTrackCommandFixture.Create(trackNumber: 7)]);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationError(Errors.Music.TrackNumberMustBeGreaterThanZero);
    }

    [Theory]
    [InlineData(0)] // zero is not a valid disc number
    [InlineData(-1)] // a negative disc number is not valid
    public void Validate_WhenDiscNumberIsNotPositive_ShouldHaveValidationError(int discNumber)
    {
        // Arrange
        AddAlbumCommand command = _addAlbumCommandFixture.Create(tracks: [_addTrackCommandFixture.Create(discNumber: discNumber)]);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Music.DiscNumberMustBeGreaterThanZero);
    }

    [Fact]
    public void Validate_WhenDiscNumberIsNull_ShouldNotHaveValidationError()
    {
        // Arrange
        AddAlbumCommand command = _addAlbumCommandFixture.Create(tracks: [_addTrackCommandFixture.Create(includeDiscNumber: false, includeScript: false, includeKey: false, includeBpm: false, includeWork: false, includeMusicBrainzRecordingId: false, includeMusicBrainzTrackId: false, includeMoods: false, includeIsrcs: false)]);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationError(Errors.Music.DiscNumberMustBeGreaterThanZero);
    }

    [Fact]
    public void Validate_WhenDiscNumberIsPositive_ShouldNotHaveValidationError()
    {
        // Arrange
        AddAlbumCommand command = _addAlbumCommandFixture.Create(tracks: [_addTrackCommandFixture.Create(discNumber: 1)]);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationError(Errors.Music.DiscNumberMustBeGreaterThanZero);
    }

    [Fact]
    public void Validate_WhenScriptExceeds50Characters_ShouldHaveValidationError()
    {
        // Arrange
        AddAlbumCommand command = _addAlbumCommandFixture.Create(tracks: [_addTrackCommandFixture.Create(script: new Faker().Random.String2(51))]);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Music.ScriptMustBeMaximum50CharactersLong);
    }

    [Fact]
    public void Validate_WhenScriptIsNull_ShouldNotHaveValidationError()
    {
        // Arrange
        AddAlbumCommand command = _addAlbumCommandFixture.Create(tracks: [_addTrackCommandFixture.Create(includeDiscNumber: false, includeScript: false, includeKey: false, includeBpm: false, includeWork: false, includeMusicBrainzRecordingId: false, includeMusicBrainzTrackId: false, includeMoods: false, includeIsrcs: false)]);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationError(Errors.Music.ScriptMustBeMaximum50CharactersLong);
    }

    [Fact]
    public void Validate_WhenScriptIsValid_ShouldNotHaveValidationError()
    {
        // Arrange
        AddAlbumCommand command = _addAlbumCommandFixture.Create(tracks: [_addTrackCommandFixture.Create(script: new Faker().Random.String2(50))]);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationError(Errors.Music.ScriptMustBeMaximum50CharactersLong);
    }

    [Fact]
    public void Validate_WhenKeyIsNotDefined_ShouldHaveValidationError()
    {
        // Arrange
        AddAlbumCommand command = _addAlbumCommandFixture.Create(tracks: [_addTrackCommandFixture.Create(key: (MusicKey)999)]);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Music.UnknownMusicKey);
    }

    [Fact]
    public void Validate_WhenKeyIsNull_ShouldNotHaveValidationError()
    {
        // Arrange
        AddAlbumCommand command = _addAlbumCommandFixture.Create(tracks: [_addTrackCommandFixture.Create(includeDiscNumber: false, includeScript: false, includeKey: false, includeBpm: false, includeWork: false, includeMusicBrainzRecordingId: false, includeMusicBrainzTrackId: false, includeMoods: false, includeIsrcs: false)]);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationError(Errors.Music.UnknownMusicKey);
    }

    [Fact]
    public void Validate_WhenKeyIsDefined_ShouldNotHaveValidationError()
    {
        // Arrange
        AddAlbumCommand command = _addAlbumCommandFixture.Create(tracks: [_addTrackCommandFixture.Create(key: MusicKey.CMajor)]);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationError(Errors.Music.UnknownMusicKey);
    }

    [Theory]
    [InlineData(0)] // zero is not a valid tempo
    [InlineData(-1)] // a negative tempo is not valid
    public void Validate_WhenBpmIsNotPositive_ShouldHaveValidationError(int bpm)
    {
        // Arrange
        AddAlbumCommand command = _addAlbumCommandFixture.Create(tracks: [_addTrackCommandFixture.Create(bpm: bpm)]);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Music.BpmMustBeGreaterThanZero);
    }

    [Fact]
    public void Validate_WhenBpmIsNull_ShouldNotHaveValidationError()
    {
        // Arrange
        AddAlbumCommand command = _addAlbumCommandFixture.Create(tracks: [_addTrackCommandFixture.Create(includeDiscNumber: false, includeScript: false, includeKey: false, includeBpm: false, includeWork: false, includeMusicBrainzRecordingId: false, includeMusicBrainzTrackId: false, includeMoods: false, includeIsrcs: false)]);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationError(Errors.Music.BpmMustBeGreaterThanZero);
    }

    [Fact]
    public void Validate_WhenBpmIsPositive_ShouldNotHaveValidationError()
    {
        // Arrange
        AddAlbumCommand command = _addAlbumCommandFixture.Create(tracks: [_addTrackCommandFixture.Create(bpm: 120)]);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationError(Errors.Music.BpmMustBeGreaterThanZero);
    }

    [Fact]
    public void Validate_WhenWorkExceeds255Characters_ShouldHaveValidationError()
    {
        // Arrange
        AddAlbumCommand command = _addAlbumCommandFixture.Create(tracks: [_addTrackCommandFixture.Create(work: _musicWorkDtoFixture.Create(title: new Faker().Random.String2(256)))]);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Music.WorkMustBeMaximum255CharactersLong);
    }

    [Fact]
    public void Validate_WhenWorkIsNull_ShouldNotHaveValidationError()
    {
        // Arrange
        AddAlbumCommand command = _addAlbumCommandFixture.Create(tracks: [_addTrackCommandFixture.Create(includeDiscNumber: false, includeScript: false, includeKey: false, includeBpm: false, includeWork: false, includeMusicBrainzRecordingId: false, includeMusicBrainzTrackId: false, includeMoods: false, includeIsrcs: false)]);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationError(Errors.Music.WorkMustBeMaximum255CharactersLong);
    }

    [Fact]
    public void Validate_WhenWorkIsValid_ShouldNotHaveValidationError()
    {
        // Arrange
        AddAlbumCommand command = _addAlbumCommandFixture.Create(tracks: [_addTrackCommandFixture.Create(work: _musicWorkDtoFixture.Create(title: new Faker().Random.String2(255)))]);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationError(Errors.Music.WorkMustBeMaximum255CharactersLong);
    }

    [Fact]
    public void Validate_WhenMoodNameIsEmpty_ShouldHaveValidationError()
    {
        // Arrange
        AddAlbumCommand command = _addAlbumCommandFixture.Create(tracks: [_addTrackCommandFixture.Create(moods: [_moodDtoFixture.Create(includeName: false)])]);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Metadata.MoodNameCannotBeEmpty);
    }

    [Fact]
    public void Validate_WhenMoodNameIsValid_ShouldNotHaveValidationError()
    {
        // Arrange
        AddAlbumCommand command = _addAlbumCommandFixture.Create(tracks: [_addTrackCommandFixture.Create(moods: [_moodDtoFixture.Create(name: "dramatic")])]);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationError(Errors.Metadata.MoodNameCannotBeEmpty);
    }

    [Fact]
    public void Validate_WhenIsrcValueIsEmpty_ShouldHaveValidationError()
    {
        // Arrange
        AddAlbumCommand command = _addAlbumCommandFixture.Create(tracks: [_addTrackCommandFixture.Create(isrcs: [_isrcDtoFixture.Create(includeValue: false)])]);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Music.IsrcValueCannotBeEmpty);
    }

    [Fact]
    public void Validate_WhenIsrcValueIsValid_ShouldNotHaveValidationError()
    {
        // Arrange
        AddAlbumCommand command = _addAlbumCommandFixture.Create(tracks: [_addTrackCommandFixture.Create(isrcs: [_isrcDtoFixture.Create(value: "GBUM71029604")])]);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationError(Errors.Music.IsrcValueCannotBeEmpty);
    }

    [Fact]
    public void Validate_WhenTrackContributorsIsNull_ShouldHaveValidationError()
    {
        // Arrange
        AddAlbumCommand command = _addAlbumCommandFixture.Create(tracks: [_addTrackCommandFixture.Create(includeContributors: false)]);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.MediaContributor.ContributorsListCannotBeNull);
    }

    [Fact]
    public void Validate_WhenTrackContributorIdIsEmpty_ShouldHaveValidationError()
    {
        // Arrange
        AddAlbumCommand command = _addAlbumCommandFixture.Create(tracks: [_addTrackCommandFixture.Create(contributors: [_mediaContributorReferenceDtoFixture.Create(contributorId: Guid.Empty, role: MediaContributorRole.Vocals)])]);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.MediaContributor.MediaContributorIdCannotBeEmpty);
    }

    [Fact]
    public void Validate_WhenTrackContributorRoleIsNotDefined_ShouldHaveValidationError()
    {
        // Arrange
        AddAlbumCommand command = _addAlbumCommandFixture.Create(tracks: [_addTrackCommandFixture.Create(contributors: [_mediaContributorReferenceDtoFixture.Create(contributorId: Guid.NewGuid(), role: (MediaContributorRole)999)])]);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.MediaContributor.UnknownMediaContributorRole);
    }

    [Fact]
    public void Validate_WhenTrackRatingsIsNull_ShouldHaveValidationError()
    {
        // Arrange
        AddAlbumCommand command = _addAlbumCommandFixture.Create(tracks: [_addTrackCommandFixture.Create(includeRatings: false)]);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Metadata.RatingsListCannotBeNull);
    }

    [Theory]
    [InlineData(0)] // zero is not a valid rating value
    [InlineData(-1)] // a negative rating value is not valid
    public void Validate_WhenTrackRatingValueIsNotPositive_ShouldHaveValidationError(decimal value)
    {
        // Arrange
        AddAlbumCommand command = _addAlbumCommandFixture.Create(tracks: [_addTrackCommandFixture.Create(ratings: [_audioRatingDtoFixture.Create(value: value, maxValue: 5)])]);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Metadata.RatingValueMustBePositive);
    }

    [Fact]
    public void Validate_WhenTrackRatingValueIsGreaterThanMaxValue_ShouldHaveValidationError()
    {
        // Arrange
        AddAlbumCommand command = _addAlbumCommandFixture.Create(tracks: [_addTrackCommandFixture.Create(ratings: [_audioRatingDtoFixture.Create(value: 6, maxValue: 5)])]);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Metadata.RatingValueCannotBeGreaterThanMaxValue);
    }

    [Theory]
    [InlineData(0)] // zero is not a valid maximum rating value
    [InlineData(-1)] // a negative maximum rating value is not valid
    public void Validate_WhenTrackRatingMaxValueIsNotPositive_ShouldHaveValidationError(decimal maxValue)
    {
        // Arrange
        AddAlbumCommand command = _addAlbumCommandFixture.Create(tracks: [_addTrackCommandFixture.Create(ratings: [_audioRatingDtoFixture.Create(value: 1, maxValue: maxValue)])]);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Metadata.RatingMaxValueMustBePositive);
    }

    [Fact]
    public void Validate_WhenTrackRatingVoteCountIsNegative_ShouldHaveValidationError()
    {
        // Arrange
        AddAlbumCommand command = _addAlbumCommandFixture.Create(tracks: [_addTrackCommandFixture.Create(ratings: [_audioRatingDtoFixture.Create(value: 4, maxValue: 5, voteCount: -1)])]);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Metadata.RatingVoteCountMustBePositive);
    }

    [Fact]
    public void Validate_WhenTrackRatingVoteCountIsNull_ShouldNotHaveValidationError()
    {
        // Arrange
        AddAlbumCommand command = _addAlbumCommandFixture.Create(tracks: [_addTrackCommandFixture.Create(ratings: [_audioRatingDtoFixture.Create(value: 4, maxValue: 5, includeVoteCount: false)])]);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationError(Errors.Metadata.RatingVoteCountMustBePositive);
    }

    [Fact]
    public void Validate_WhenTrackRatingsAreValid_ShouldNotHaveValidationError()
    {
        // Arrange
        AddAlbumCommand command = _addAlbumCommandFixture.Create(tracks: [_addTrackCommandFixture.Create(ratings: [_audioRatingDtoFixture.Create(value: 4, maxValue: 5, voteCount: 100)])]);

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
    public void Validate_WhenTrackContributorsAreValid_ShouldNotHaveValidationError()
    {
        // Arrange
        AddAlbumCommand command = _addAlbumCommandFixture.Create(tracks: [_addTrackCommandFixture.Create(contributors: [_mediaContributorReferenceDtoFixture.Create(contributorId: Guid.NewGuid(), role: MediaContributorRole.Vocals)])]);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationError(Errors.MediaContributor.ContributorsListCannotBeNull);
        result.ShouldNotHaveValidationError(Errors.MediaContributor.MediaContributorIdCannotBeEmpty);
        result.ShouldNotHaveValidationError(Errors.MediaContributor.UnknownMediaContributorRole);
    }

    [Fact]
    public void Validate_WhenTrackOriginalTitleIsNull_ShouldNotHaveValidationError()
    {
        // Arrange
        AddAlbumCommand command = _addAlbumCommandFixture.Create(tracks: [_addTrackCommandFixture.Create(metadata: _audioMetadataDtoFixture.Create(includeOriginalTitle: false))]);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationError(Errors.Metadata.OriginalTitleMustBeMaximum255CharactersLong);
    }

    [Fact]
    public void Validate_WhenTrackDescriptionIsNull_ShouldNotHaveValidationError()
    {
        // Arrange
        AddAlbumCommand command = _addAlbumCommandFixture.Create(tracks: [_addTrackCommandFixture.Create(metadata: _audioMetadataDtoFixture.Create(includeDescription: false))]);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationError(Errors.Metadata.DescriptionMustBeMaximum2000CharactersLong);
    }

    [Fact]
    public void Validate_WhenTrackReleaseInfoIsValid_ShouldNotHaveValidationError()
    {
        // Arrange
        AddAlbumCommand command = _addAlbumCommandFixture.Create(tracks: [_addTrackCommandFixture.Create(metadata: _audioMetadataDtoFixture.Create(releaseInfo: _releaseInfoDtoFixture.Create(originalReleaseYear: 2000, reReleaseYear: 2001, includeReReleaseDate: false)))]);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationError(Errors.Metadata.OriginalReleaseYearMustBeBetween1And9999);
        result.ShouldNotHaveValidationError(Errors.Metadata.ReReleaseYearMustBeBetween1And9999);
        result.ShouldNotHaveValidationError(Errors.Metadata.ReReleaseYearCannotBeEarlierThanOriginalReleaseYear);
    }

    [Fact]
    public void Validate_WhenTrackGenresAreValid_ShouldNotHaveValidationError()
    {
        // Arrange
        AddAlbumCommand command = _addAlbumCommandFixture.Create(tracks: [_addTrackCommandFixture.Create(metadata: _audioMetadataDtoFixture.Create(genres: [_genreDtoFixture.Create(name: new Faker().Random.String2(50))]))]);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationError(Errors.Metadata.GenresListCannotBeNull);
        result.ShouldNotHaveValidationError(Errors.Metadata.GenreNameCannotBeEmpty);
        result.ShouldNotHaveValidationError(Errors.Metadata.GenreNameMustBeMaximum50CharactersLong);
    }

    [Fact]
    public void Validate_WhenTrackTagsAreValid_ShouldNotHaveValidationError()
    {
        // Arrange
        AddAlbumCommand command = _addAlbumCommandFixture.Create(tracks: [_addTrackCommandFixture.Create(metadata: _audioMetadataDtoFixture.Create(tags: [_tagDtoFixture.Create(name: new Faker().Random.String2(50))]))]);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationError(Errors.Metadata.TagsListCannotBeNull);
        result.ShouldNotHaveValidationError(Errors.Metadata.TagNameCannotBeEmpty);
        result.ShouldNotHaveValidationError(Errors.Metadata.TagNameMustBeMaximum50CharactersLong);
    }

    [Fact]
    public void Validate_WhenTrackLanguageIsNull_ShouldNotHaveValidationError()
    {
        // Arrange
        AddAlbumCommand command = _addAlbumCommandFixture.Create(tracks: [_addTrackCommandFixture.Create(metadata: _audioMetadataDtoFixture.Create(includeLanguage: false))]);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationError(Errors.Metadata.LanguageCodeCannotBeEmpty);
    }

    [Fact]
    public void Validate_WhenTrackLanguageCodeIsValid_ShouldNotHaveValidationError()
    {
        // Arrange
        AddAlbumCommand command = _addAlbumCommandFixture.Create(tracks: [_addTrackCommandFixture.Create(metadata: _audioMetadataDtoFixture.Create(language: _languageInfoDtoFixture.Create(languageCode: new Faker().Random.String2(2))))]);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationError(Errors.Metadata.LanguageCodeMustBe2CharactersLong);
    }

    [Fact]
    public void Validate_WhenTrackLanguageNativeNameIsNull_ShouldNotHaveValidationError()
    {
        // Arrange
        AddAlbumCommand command = _addAlbumCommandFixture.Create(tracks: [_addTrackCommandFixture.Create(metadata: _audioMetadataDtoFixture.Create(language: _languageInfoDtoFixture.Create(includeNativeName: false)))]);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationError(Errors.Metadata.LanguageNativeNameMustBeMaximum50CharactersLong);
    }

    [Fact]
    public void Validate_WhenTrackOriginalLanguageIsNull_ShouldNotHaveValidationError()
    {
        // Arrange
        AddAlbumCommand command = _addAlbumCommandFixture.Create(tracks: [_addTrackCommandFixture.Create(metadata: _audioMetadataDtoFixture.Create(includeOriginalLanguage: false))]);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationError(Errors.Metadata.LanguageCodeCannotBeEmpty);
    }

    [Fact]
    public void Validate_WhenTrackOriginalLanguageCodeIsNot2CharactersLong_ShouldHaveValidationError()
    {
        // Arrange
        AddAlbumCommand command = _addAlbumCommandFixture.Create(tracks: [_addTrackCommandFixture.Create(metadata: _audioMetadataDtoFixture.Create(originalLanguage: _languageInfoDtoFixture.Create(languageCode: new Faker().Random.String2(3))))]);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Metadata.LanguageCodeMustBe2CharactersLong);
    }

    [Fact]
    public void Validate_WhenTrackOriginalLanguageNameExceeds50Characters_ShouldHaveValidationError()
    {
        // Arrange
        AddAlbumCommand command = _addAlbumCommandFixture.Create(tracks: [_addTrackCommandFixture.Create(metadata: _audioMetadataDtoFixture.Create(originalLanguage: _languageInfoDtoFixture.Create(languageName: new Faker().Random.String2(51))))]);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Metadata.LanguageNameMustBeMaximum50CharactersLong);
    }

    [Fact]
    public void Validate_WhenTrackOriginalLanguageNativeNameIsNull_ShouldNotHaveValidationError()
    {
        // Arrange
        AddAlbumCommand command = _addAlbumCommandFixture.Create(tracks: [_addTrackCommandFixture.Create(metadata: _audioMetadataDtoFixture.Create(originalLanguage: _languageInfoDtoFixture.Create(includeNativeName: false)))]);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationError(Errors.Metadata.LanguageNativeNameMustBeMaximum50CharactersLong);
    }

    [Fact]
    public void Validate_WhenTrackMusicBrainzIdsAreValid_ShouldNotHaveValidationError()
    {
        // Arrange
        AddAlbumCommand command = _addAlbumCommandFixture.Create(tracks: [_addTrackCommandFixture.Create(musicBrainzRecordingId: Guid.NewGuid(), musicBrainzTrackId: Guid.NewGuid(), work: _musicWorkDtoFixture.Create(musicBrainzWorkId: Guid.NewGuid()))]);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationError(Errors.Music.MusicBrainzIdInvalidFormat);
    }

    [Fact]
    public void Validate_WhenLanguageNameIsValid_ShouldNotHaveValidationError()
    {
        // Arrange
        AddAlbumCommand command = _addAlbumCommandFixture.Create(metadata: _albumMetadataDtoFixture.Create(language: _languageInfoDtoFixture.Create(languageName: new Faker().Random.String2(20))));

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
        AddAlbumCommand command = _addAlbumCommandFixture.Create(metadata: _albumMetadataDtoFixture.Create(language: _languageInfoDtoFixture.Create(nativeName: new Faker().Random.String2(20))));

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationError(Errors.Metadata.LanguageNativeNameMustBeMaximum50CharactersLong);
    }

    [Fact]
    public void Validate_WhenOriginalLanguageCodeIsValid_ShouldNotHaveValidationError()
    {
        // Arrange
        AddAlbumCommand command = _addAlbumCommandFixture.Create(metadata: _albumMetadataDtoFixture.Create(originalLanguage: _languageInfoDtoFixture.Create(languageCode: new Faker().Random.String2(2))));

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationError(Errors.Metadata.LanguageCodeMustBe2CharactersLong);
    }

    [Fact]
    public void Validate_WhenOriginalLanguageNameIsValid_ShouldNotHaveValidationError()
    {
        // Arrange
        AddAlbumCommand command = _addAlbumCommandFixture.Create(metadata: _albumMetadataDtoFixture.Create(originalLanguage: _languageInfoDtoFixture.Create(languageName: new Faker().Random.String2(20))));

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
        AddAlbumCommand command = _addAlbumCommandFixture.Create(metadata: _albumMetadataDtoFixture.Create(originalLanguage: _languageInfoDtoFixture.Create(nativeName: new Faker().Random.String2(20))));

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationError(Errors.Metadata.LanguageNativeNameMustBeMaximum50CharactersLong);
    }

    [Fact]
    public void Validate_WhenReleaseVersionIsNull_ShouldNotHaveValidationError()
    {
        // Arrange
        AddAlbumCommand command = _addAlbumCommandFixture.Create(metadata: _albumMetadataDtoFixture.Create(releaseInfo: _releaseInfoDtoFixture.Create(includeReleaseVersion: false)));

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationError(Errors.Metadata.ReleaseVersionMustBeMaximum50CharactersLong);
    }

    [Fact]
    public void Validate_WhenTrackOriginalTitleIsValid_ShouldNotHaveValidationError()
    {
        // Arrange
        AddAlbumCommand command = _addAlbumCommandFixture.Create(tracks: [_addTrackCommandFixture.Create(metadata: _audioMetadataDtoFixture.Create(originalTitle: new Faker().Random.String2(200)))]);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationError(Errors.Metadata.OriginalTitleMustBeMaximum255CharactersLong);
    }

    [Fact]
    public void Validate_WhenTrackDescriptionIsValid_ShouldNotHaveValidationError()
    {
        // Arrange
        AddAlbumCommand command = _addAlbumCommandFixture.Create(tracks: [_addTrackCommandFixture.Create(metadata: _audioMetadataDtoFixture.Create(description: new Faker().Random.String2(1500)))]);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationError(Errors.Metadata.DescriptionMustBeMaximum2000CharactersLong);
    }

    [Fact]
    public void Validate_WhenTrackOriginalReleaseYearIsValid_ShouldNotHaveValidationError()
    {
        // Arrange
        AddAlbumCommand command = _addAlbumCommandFixture.Create(tracks: [_addTrackCommandFixture.Create(metadata: _audioMetadataDtoFixture.Create(releaseInfo: _releaseInfoDtoFixture.Create(includeOriginalReleaseDate: false, originalReleaseYear: 2000, reReleaseYear: 2001, includeReReleaseDate: false)))]);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationError(Errors.Metadata.OriginalReleaseYearMustBeBetween1And9999);
    }

    [Fact]
    public void Validate_WhenTrackReReleaseYearIsValid_ShouldNotHaveValidationError()
    {
        // Arrange
        AddAlbumCommand command = _addAlbumCommandFixture.Create(tracks: [_addTrackCommandFixture.Create(metadata: _audioMetadataDtoFixture.Create(releaseInfo: _releaseInfoDtoFixture.Create(originalReleaseYear: 2000, reReleaseYear: 2001, includeReReleaseDate: false)))]);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationError(Errors.Metadata.ReReleaseYearMustBeBetween1And9999);
    }

    [Fact]
    public void Validate_WhenTrackReleaseVersionIsValid_ShouldNotHaveValidationError()
    {
        // Arrange
        AddAlbumCommand command = _addAlbumCommandFixture.Create(tracks: [_addTrackCommandFixture.Create(metadata: _audioMetadataDtoFixture.Create(releaseInfo: _releaseInfoDtoFixture.Create(releaseVersion: new Faker().Random.String2(50))))]);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationError(Errors.Metadata.ReleaseVersionMustBeMaximum50CharactersLong);
    }

    [Fact]
    public void Validate_WhenTrackOriginalReleaseDateAndYearMatch_ShouldNotHaveValidationError()
    {
        // Arrange
        AddAlbumCommand command = _addAlbumCommandFixture.Create(tracks: [_addTrackCommandFixture.Create(metadata: _audioMetadataDtoFixture.Create(releaseInfo: _releaseInfoDtoFixture.Create(originalReleaseDate: new DateOnly(2020, 1, 1), originalReleaseYear: 2020, includeReReleaseDate: false)))]);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationError(Errors.Metadata.OriginalReleaseDateAndYearMustMatch);
    }

    [Fact]
    public void Validate_WhenTrackReReleaseDateAndYearMatch_ShouldNotHaveValidationError()
    {
        // Arrange
        AddAlbumCommand command = _addAlbumCommandFixture.Create(tracks: [_addTrackCommandFixture.Create(metadata: _audioMetadataDtoFixture.Create(releaseInfo: _releaseInfoDtoFixture.Create(reReleaseDate: new DateOnly(2021, 1, 1), reReleaseYear: 2021)))]);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationError(Errors.Metadata.ReReleaseDateAndYearMustMatch);
    }

    [Fact]
    public void Validate_WhenTrackReReleaseYearIsAfterOriginalReleaseYear_ShouldNotHaveValidationError()
    {
        // Arrange
        AddAlbumCommand command = _addAlbumCommandFixture.Create(tracks: [_addTrackCommandFixture.Create(metadata: _audioMetadataDtoFixture.Create(releaseInfo: _releaseInfoDtoFixture.Create(originalReleaseYear: 2000, reReleaseYear: 2001, includeReReleaseDate: false)))]);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationError(Errors.Metadata.ReReleaseYearCannotBeEarlierThanOriginalReleaseYear);
    }

    [Fact]
    public void Validate_WhenTrackReReleaseDateIsAfterOriginalReleaseDate_ShouldNotHaveValidationError()
    {
        // Arrange
        AddAlbumCommand command = _addAlbumCommandFixture.Create(tracks: [_addTrackCommandFixture.Create(metadata: _audioMetadataDtoFixture.Create(releaseInfo: _releaseInfoDtoFixture.Create(originalReleaseDate: new DateOnly(2000, 1, 1), reReleaseDate: new DateOnly(2001, 1, 1))))]);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationError(Errors.Metadata.ReReleaseDateCannotBeEarlierThanOriginalReleaseDate);
    }

    [Fact]
    public void Validate_WhenTrackLanguageNameIsValid_ShouldNotHaveValidationError()
    {
        // Arrange
        AddAlbumCommand command = _addAlbumCommandFixture.Create(tracks: [_addTrackCommandFixture.Create(metadata: _audioMetadataDtoFixture.Create(language: _languageInfoDtoFixture.Create(languageName: new Faker().Random.String2(20))))]);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationError(Errors.Metadata.LanguageNameCannotBeEmpty);
        result.ShouldNotHaveValidationError(Errors.Metadata.LanguageNameMustBeMaximum50CharactersLong);
    }

    [Fact]
    public void Validate_WhenTrackLanguageNativeNameIsValid_ShouldNotHaveValidationError()
    {
        // Arrange
        AddAlbumCommand command = _addAlbumCommandFixture.Create(tracks: [_addTrackCommandFixture.Create(metadata: _audioMetadataDtoFixture.Create(language: _languageInfoDtoFixture.Create(nativeName: new Faker().Random.String2(20))))]);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationError(Errors.Metadata.LanguageNativeNameMustBeMaximum50CharactersLong);
    }

    [Fact]
    public void Validate_WhenTrackOriginalLanguageCodeIsValid_ShouldNotHaveValidationError()
    {
        // Arrange
        AddAlbumCommand command = _addAlbumCommandFixture.Create(tracks: [_addTrackCommandFixture.Create(metadata: _audioMetadataDtoFixture.Create(originalLanguage: _languageInfoDtoFixture.Create(languageCode: new Faker().Random.String2(2))))]);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationError(Errors.Metadata.LanguageCodeMustBe2CharactersLong);
    }

    [Fact]
    public void Validate_WhenTrackOriginalLanguageNameIsValid_ShouldNotHaveValidationError()
    {
        // Arrange
        AddAlbumCommand command = _addAlbumCommandFixture.Create(tracks: [_addTrackCommandFixture.Create(metadata: _audioMetadataDtoFixture.Create(originalLanguage: _languageInfoDtoFixture.Create(languageName: new Faker().Random.String2(20))))]);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationError(Errors.Metadata.LanguageNameCannotBeEmpty);
        result.ShouldNotHaveValidationError(Errors.Metadata.LanguageNameMustBeMaximum50CharactersLong);
    }

    [Fact]
    public void Validate_WhenTrackOriginalLanguageNativeNameIsValid_ShouldNotHaveValidationError()
    {
        // Arrange
        AddAlbumCommand command = _addAlbumCommandFixture.Create(tracks: [_addTrackCommandFixture.Create(metadata: _audioMetadataDtoFixture.Create(originalLanguage: _languageInfoDtoFixture.Create(nativeName: new Faker().Random.String2(20))))]);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationError(Errors.Metadata.LanguageNativeNameMustBeMaximum50CharactersLong);
    }

    [Fact]
    public void Validate_WhenMoodsIsNull_ShouldNotHaveValidationError()
    {
        // Arrange
        AddAlbumCommand command = _addAlbumCommandFixture.Create(tracks: [_addTrackCommandFixture.Create(includeDiscNumber: false, includeScript: false, includeKey: false, includeBpm: false, includeWork: false, includeMusicBrainzRecordingId: false, includeMusicBrainzTrackId: false, includeMoods: false, includeIsrcs: false)]);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationError(Errors.Metadata.MoodNameCannotBeEmpty);
    }

    [Fact]
    public void Validate_WhenIsrcsIsNull_ShouldNotHaveValidationError()
    {
        // Arrange
        AddAlbumCommand command = _addAlbumCommandFixture.Create(tracks: [_addTrackCommandFixture.Create(includeDiscNumber: false, includeScript: false, includeKey: false, includeBpm: false, includeWork: false, includeMusicBrainzRecordingId: false, includeMusicBrainzTrackId: false, includeMoods: false, includeIsrcs: false)]);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationError(Errors.Music.IsrcValueCannotBeEmpty);
    }

    [Fact]
    public void Validate_WhenCommandIsValid_ShouldNotHaveAnyValidationErrors()
    {
        // Arrange
        AddAlbumCommand command = _addAlbumCommandFixture.Create();

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }
}
