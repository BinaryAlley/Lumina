#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Application.Core.MediaLibrary.AudioLibrary.MusicLibrary.Tracks.Commands.AddTrack;
using Lumina.Application.Fixtures.Core.MediaLibrary.AudioLibrary.MusicLibrary.Tracks.Commands.AddTrack;
using Lumina.Application.UnitTests.Common.Setup;
using Lumina.Contracts.DTO.Common;
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

namespace Lumina.Application.UnitTests.Core.MediaLibrary.AudioLibrary.MusicLibrary.Tracks.Commands.AddTrack;

/// <summary>
/// Contains unit tests for the <see cref="AddTrackCommandValidator"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class AddTrackCommandValidatorTests
{
    private readonly AddTrackCommandFixture _addTrackCommandFixture = new();
    private readonly AddTrackCommandValidator _validator = new();
    private readonly MusicTrackMetadataDtoFixture _musicTrackMetadataDtoFixture = new();
    private readonly MusicWorkDtoFixture _musicWorkDtoFixture = new();
    private readonly ReleaseInfoDtoFixture _releaseInfoDtoFixture = new();
    private readonly GenreDtoFixture _genreDtoFixture = new();
    private readonly TagDtoFixture _tagDtoFixture = new();
    private readonly LanguageInfoDtoFixture _languageInfoDtoFixture = new();
    private readonly MoodDtoFixture _moodDtoFixture = new();
    private readonly IsrcDtoFixture _isrcDtoFixture = new();
    private readonly AudioRatingDtoFixture _audioRatingDtoFixture = new();
    private readonly MediaContributorReferenceDtoFixture _mediaContributorReferenceDtoFixture = new();

    [Fact]
    public void Validate_WhenLibraryIdIsNull_ShouldHaveValidationError()
    {
        // Arrange
        AddTrackCommand command = _addTrackCommandFixture.Create(includeLibraryId: false);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Library.LibraryIdCannotBeEmpty);
    }

    [Fact]
    public void Validate_WhenLibraryIdIsEmpty_ShouldHaveValidationError()
    {
        // Arrange
        AddTrackCommand command = _addTrackCommandFixture.Create(libraryId: string.Empty);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Library.LibraryIdCannotBeEmpty);
    }

    [Fact]
    public void Validate_WhenLibraryIdIsWhitespace_ShouldHaveValidationError()
    {
        // Arrange
        AddTrackCommand command = _addTrackCommandFixture.Create(libraryId: "   ");

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Library.LibraryIdCannotBeEmpty);
    }

    [Fact]
    public void Validate_WhenLibraryIdIsNotAGuid_ShouldHaveValidationError()
    {
        // Arrange
        AddTrackCommand command = _addTrackCommandFixture.Create(libraryId: "not-a-library-guid");

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Library.LibraryIdCannotBeEmpty);
    }

    [Fact]
    public void Validate_WhenLibraryIdIsValid_ShouldNotHaveValidationError()
    {
        // Arrange
        AddTrackCommand command = _addTrackCommandFixture.Create(libraryId: Guid.NewGuid().ToString());

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationError(Errors.Library.LibraryIdCannotBeEmpty);
    }

    [Fact]
    public void Validate_WhenArtistIdIsNull_ShouldHaveValidationError()
    {
        // Arrange
        AddTrackCommand command = _addTrackCommandFixture.Create(includeArtistId: false);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Music.ArtistIdCannotBeEmpty);
    }

    [Fact]
    public void Validate_WhenArtistIdIsEmpty_ShouldHaveValidationError()
    {
        // Arrange
        AddTrackCommand command = _addTrackCommandFixture.Create(artistId: string.Empty);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Music.ArtistIdCannotBeEmpty);
    }

    [Fact]
    public void Validate_WhenArtistIdIsWhitespace_ShouldHaveValidationError()
    {
        // Arrange
        AddTrackCommand command = _addTrackCommandFixture.Create(artistId: "   ");

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Music.ArtistIdCannotBeEmpty);
    }

    [Fact]
    public void Validate_WhenArtistIdIsNotAGuid_ShouldHaveValidationError()
    {
        // Arrange
        AddTrackCommand command = _addTrackCommandFixture.Create(artistId: "not-an-artist-guid");

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Music.ArtistIdCannotBeEmpty);
    }

    [Fact]
    public void Validate_WhenArtistIdIsValid_ShouldNotHaveValidationError()
    {
        // Arrange
        AddTrackCommand command = _addTrackCommandFixture.Create(artistId: Guid.NewGuid().ToString());

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationError(Errors.Music.ArtistIdCannotBeEmpty);
    }

    [Fact]
    public void Validate_WhenAlbumIdIsNull_ShouldHaveValidationError()
    {
        // Arrange
        AddTrackCommand command = _addTrackCommandFixture.Create(includeAlbumId: false);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Music.AlbumIdCannotBeEmpty);
    }

    [Fact]
    public void Validate_WhenAlbumIdIsEmpty_ShouldHaveValidationError()
    {
        // Arrange
        AddTrackCommand command = _addTrackCommandFixture.Create(albumId: string.Empty);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Music.AlbumIdCannotBeEmpty);
    }

    [Fact]
    public void Validate_WhenAlbumIdIsWhitespace_ShouldHaveValidationError()
    {
        // Arrange
        AddTrackCommand command = _addTrackCommandFixture.Create(albumId: "   ");

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Music.AlbumIdCannotBeEmpty);
    }

    [Fact]
    public void Validate_WhenAlbumIdIsNotAGuid_ShouldHaveValidationError()
    {
        // Arrange
        AddTrackCommand command = _addTrackCommandFixture.Create(albumId: "not-an-album-guid");

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Music.AlbumIdCannotBeEmpty);
    }

    [Fact]
    public void Validate_WhenAlbumIdIsValid_ShouldNotHaveValidationError()
    {
        // Arrange
        AddTrackCommand command = _addTrackCommandFixture.Create(albumId: Guid.NewGuid().ToString());

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationError(Errors.Music.AlbumIdCannotBeEmpty);
    }

    [Fact]
    public void Validate_WhenPathIsNull_ShouldHaveValidationError()
    {
        // Arrange
        AddTrackCommand command = _addTrackCommandFixture.Create(includePath: false);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Music.TrackPathCannotBeEmpty);
    }

    [Fact]
    public void Validate_WhenPathIsLongerThan2048Characters_ShouldHaveValidationError()
    {
        // Arrange
        AddTrackCommand command = _addTrackCommandFixture.Create(path: "/" + new Faker().Random.String2(2048) + ".flac");

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Music.TrackPathMustBeMaximum2048CharactersLong);
    }

    [Fact]
    public void Validate_WhenPathIsAtMost2048Characters_ShouldNotHaveValidationError()
    {
        // Arrange
        AddTrackCommand command = _addTrackCommandFixture.Create(path: "/" + new Faker().Random.String2(2000) + ".flac");

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationError(Errors.Music.TrackPathMustBeMaximum2048CharactersLong);
    }

    [Fact]
    public void Validate_WhenMetadataIsNull_ShouldHaveValidationError()
    {
        // Arrange
        AddTrackCommand command = _addTrackCommandFixture.Create(includeMetadata: false);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Metadata.MetadataCannotBeNull);
    }

    [Fact]
    public void Validate_WhenTitleIsNull_ShouldHaveValidationError()
    {
        // Arrange
        AddTrackCommand command = _addTrackCommandFixture.Create(metadata: _musicTrackMetadataDtoFixture.Create(includeTitle: false));

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Metadata.TitleCannotBeEmpty);
    }

    [Fact]
    public void Validate_WhenTitleExceeds255Characters_ShouldHaveValidationError()
    {
        // Arrange
        AddTrackCommand command = _addTrackCommandFixture.Create(metadata: _musicTrackMetadataDtoFixture.Create(title: new Faker().Random.String2(300)));

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Metadata.TitleMustBeMaximum255CharactersLong);
    }

    [Fact]
    public void Validate_WhenTitleIsValid_ShouldNotHaveValidationError()
    {
        // Arrange
        AddTrackCommand command = _addTrackCommandFixture.Create(metadata: _musicTrackMetadataDtoFixture.Create(title: new Faker().Random.String2(200)));

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationError(Errors.Metadata.TitleCannotBeEmpty);
        result.ShouldNotHaveValidationError(Errors.Metadata.TitleMustBeMaximum255CharactersLong);
    }

    [Fact]
    public void Validate_WhenOriginalTitleExceeds255Characters_ShouldHaveValidationError()
    {
        // Arrange
        AddTrackCommand command = _addTrackCommandFixture.Create(metadata: _musicTrackMetadataDtoFixture.Create(originalTitle: new Faker().Random.String2(300)));

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Metadata.OriginalTitleMustBeMaximum255CharactersLong);
    }

    [Fact]
    public void Validate_WhenOriginalTitleIsNull_ShouldNotHaveValidationError()
    {
        // Arrange
        AddTrackCommand command = _addTrackCommandFixture.Create(metadata: _musicTrackMetadataDtoFixture.Create(includeOriginalTitle: false));

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationError(Errors.Metadata.OriginalTitleMustBeMaximum255CharactersLong);
    }

    [Fact]
    public void Validate_WhenOriginalTitleIsValid_ShouldNotHaveValidationError()
    {
        // Arrange
        AddTrackCommand command = _addTrackCommandFixture.Create(metadata: _musicTrackMetadataDtoFixture.Create(originalTitle: new Faker().Random.String2(200)));

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationError(Errors.Metadata.OriginalTitleMustBeMaximum255CharactersLong);
    }

    [Fact]
    public void Validate_WhenDescriptionExceeds2000Characters_ShouldHaveValidationError()
    {
        // Arrange
        AddTrackCommand command = _addTrackCommandFixture.Create(metadata: _musicTrackMetadataDtoFixture.Create(description: new Faker().Random.String2(2001)));

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Metadata.DescriptionMustBeMaximum2000CharactersLong);
    }

    [Fact]
    public void Validate_WhenDescriptionIsNull_ShouldNotHaveValidationError()
    {
        // Arrange
        AddTrackCommand command = _addTrackCommandFixture.Create(metadata: _musicTrackMetadataDtoFixture.Create(includeDescription: false));

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationError(Errors.Metadata.DescriptionMustBeMaximum2000CharactersLong);
    }

    [Fact]
    public void Validate_WhenDescriptionIsValid_ShouldNotHaveValidationError()
    {
        // Arrange
        AddTrackCommand command = _addTrackCommandFixture.Create(metadata: _musicTrackMetadataDtoFixture.Create(description: new Faker().Random.String2(1500)));

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationError(Errors.Metadata.DescriptionMustBeMaximum2000CharactersLong);
    }

    [Fact]
    public void Validate_WhenReleaseInfoIsNull_ShouldHaveValidationError()
    {
        // Arrange
        AddTrackCommand command = _addTrackCommandFixture.Create(metadata: _musicTrackMetadataDtoFixture.Create(includeReleaseInfo: false));

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Metadata.ReleaseInfoCannotBeNull);
    }

    [Fact]
    public void Validate_WhenOriginalReleaseYearIsLessThan1_ShouldHaveValidationError()
    {
        // Arrange
        AddTrackCommand command = _addTrackCommandFixture.Create(metadata: _musicTrackMetadataDtoFixture.Create(releaseInfo: _releaseInfoDtoFixture.Create(originalReleaseYear: 0)));

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Metadata.OriginalReleaseYearMustBeBetween1And9999);
    }

    [Fact]
    public void Validate_WhenOriginalReleaseYearIsGreaterThan9999_ShouldHaveValidationError()
    {
        // Arrange
        AddTrackCommand command = _addTrackCommandFixture.Create(metadata: _musicTrackMetadataDtoFixture.Create(releaseInfo: _releaseInfoDtoFixture.Create(originalReleaseYear: 10000)));

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Metadata.OriginalReleaseYearMustBeBetween1And9999);
    }

    [Fact]
    public void Validate_WhenOriginalReleaseYearIsValid_ShouldNotHaveValidationError()
    {
        // Arrange
        AddTrackCommand command = _addTrackCommandFixture.Create(metadata: _musicTrackMetadataDtoFixture.Create(releaseInfo: _releaseInfoDtoFixture.Create(includeOriginalReleaseDate: false, originalReleaseYear: new Faker().Random.Int(2000, 2005), reReleaseYear: new Faker().Random.Int(2005, 2010), includeReReleaseDate: false)));

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationError(Errors.Metadata.OriginalReleaseYearMustBeBetween1And9999);
    }

    [Fact]
    public void Validate_WhenReReleaseYearIsLessThan1_ShouldHaveValidationError()
    {
        // Arrange
        AddTrackCommand command = _addTrackCommandFixture.Create(metadata: _musicTrackMetadataDtoFixture.Create(releaseInfo: _releaseInfoDtoFixture.Create(reReleaseYear: 0)));

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Metadata.ReReleaseYearMustBeBetween1And9999);
    }

    [Fact]
    public void Validate_WhenReReleaseYearIsGreaterThan9999_ShouldHaveValidationError()
    {
        // Arrange
        AddTrackCommand command = _addTrackCommandFixture.Create(metadata: _musicTrackMetadataDtoFixture.Create(releaseInfo: _releaseInfoDtoFixture.Create(reReleaseYear: 10000)));

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Metadata.ReReleaseYearMustBeBetween1And9999);
    }

    [Fact]
    public void Validate_WhenReReleaseYearIsValid_ShouldNotHaveValidationError()
    {
        // Arrange
        AddTrackCommand command = _addTrackCommandFixture.Create(metadata: _musicTrackMetadataDtoFixture.Create(releaseInfo: _releaseInfoDtoFixture.Create(originalReleaseYear: new Faker().Random.Int(2000, 2005), reReleaseYear: new Faker().Random.Int(2005, 2010), includeReReleaseDate: false)));

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationError(Errors.Metadata.ReReleaseYearMustBeBetween1And9999);
    }

    [Fact]
    public void Validate_WhenReleaseVersionExceeds50Characters_ShouldHaveValidationError()
    {
        // Arrange
        AddTrackCommand command = _addTrackCommandFixture.Create(metadata: _musicTrackMetadataDtoFixture.Create(releaseInfo: _releaseInfoDtoFixture.Create(releaseVersion: new Faker().Random.String2(51))));

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Metadata.ReleaseVersionMustBeMaximum50CharactersLong);
    }

    [Fact]
    public void Validate_WhenReleaseVersionIsValid_ShouldNotHaveValidationError()
    {
        // Arrange
        AddTrackCommand command = _addTrackCommandFixture.Create(metadata: _musicTrackMetadataDtoFixture.Create(releaseInfo: _releaseInfoDtoFixture.Create(releaseVersion: new Faker().Random.String2(50))));

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationError(Errors.Metadata.ReleaseVersionMustBeMaximum50CharactersLong);
    }

    [Fact]
    public void Validate_WhenOriginalReleaseDateAndYearDoNotMatch_ShouldHaveValidationError()
    {
        // Arrange
        AddTrackCommand command = _addTrackCommandFixture.Create(metadata: _musicTrackMetadataDtoFixture.Create(releaseInfo: _releaseInfoDtoFixture.Create(originalReleaseDate: new DateOnly(2020, 1, 1), originalReleaseYear: 2019, includeReReleaseDate: false)));

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Metadata.OriginalReleaseDateAndYearMustMatch);
    }

    [Fact]
    public void Validate_WhenOriginalReleaseDateAndYearMatch_ShouldNotHaveValidationError()
    {
        // Arrange
        AddTrackCommand command = _addTrackCommandFixture.Create(metadata: _musicTrackMetadataDtoFixture.Create(releaseInfo: _releaseInfoDtoFixture.Create(originalReleaseDate: new DateOnly(2020, 1, 1), originalReleaseYear: 2020, includeReReleaseDate: false)));

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationError(Errors.Metadata.OriginalReleaseDateAndYearMustMatch);
    }

    [Fact]
    public void Validate_WhenReReleaseDateAndYearDoNotMatch_ShouldHaveValidationError()
    {
        // Arrange
        AddTrackCommand command = _addTrackCommandFixture.Create(metadata: _musicTrackMetadataDtoFixture.Create(releaseInfo: _releaseInfoDtoFixture.Create(reReleaseDate: new DateOnly(2021, 1, 1), reReleaseYear: 2020)));

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Metadata.ReReleaseDateAndYearMustMatch);
    }

    [Fact]
    public void Validate_WhenReReleaseDateAndYearMatch_ShouldNotHaveValidationError()
    {
        // Arrange
        AddTrackCommand command = _addTrackCommandFixture.Create(metadata: _musicTrackMetadataDtoFixture.Create(releaseInfo: _releaseInfoDtoFixture.Create(reReleaseDate: new DateOnly(2021, 1, 1), reReleaseYear: 2021)));

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationError(Errors.Metadata.ReReleaseDateAndYearMustMatch);
    }

    [Fact]
    public void Validate_WhenReReleaseYearIsBeforeOriginalReleaseYear_ShouldHaveValidationError()
    {
        // Arrange
        AddTrackCommand command = _addTrackCommandFixture.Create(metadata: _musicTrackMetadataDtoFixture.Create(releaseInfo: _releaseInfoDtoFixture.Create(originalReleaseYear: 2001, reReleaseYear: 2000, includeReReleaseDate: false)));

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Metadata.ReReleaseYearCannotBeEarlierThanOriginalReleaseYear);
    }

    [Fact]
    public void Validate_WhenReReleaseYearIsAfterOriginalReleaseYear_ShouldNotHaveValidationError()
    {
        // Arrange
        AddTrackCommand command = _addTrackCommandFixture.Create(metadata: _musicTrackMetadataDtoFixture.Create(releaseInfo: _releaseInfoDtoFixture.Create(originalReleaseYear: 2000, reReleaseYear: 2001, includeReReleaseDate: false)));

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationError(Errors.Metadata.ReReleaseYearCannotBeEarlierThanOriginalReleaseYear);
    }

    [Fact]
    public void Validate_WhenReReleaseDateIsBeforeOriginalReleaseDate_ShouldHaveValidationError()
    {
        // Arrange
        AddTrackCommand command = _addTrackCommandFixture.Create(metadata: _musicTrackMetadataDtoFixture.Create(releaseInfo: _releaseInfoDtoFixture.Create(originalReleaseDate: new DateOnly(2001, 1, 1), reReleaseDate: new DateOnly(2000, 1, 1))));

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Metadata.ReReleaseDateCannotBeEarlierThanOriginalReleaseDate);
    }

    [Fact]
    public void Validate_WhenReReleaseDateIsAfterOriginalReleaseDate_ShouldNotHaveValidationError()
    {
        // Arrange
        AddTrackCommand command = _addTrackCommandFixture.Create(metadata: _musicTrackMetadataDtoFixture.Create(releaseInfo: _releaseInfoDtoFixture.Create(originalReleaseDate: new DateOnly(2000, 1, 1), reReleaseDate: new DateOnly(2001, 1, 1))));

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationError(Errors.Metadata.ReReleaseDateCannotBeEarlierThanOriginalReleaseDate);
    }

    [Fact]
    public void Validate_WhenGenresIsNull_ShouldHaveValidationError()
    {
        // Arrange
        AddTrackCommand command = _addTrackCommandFixture.Create(metadata: _musicTrackMetadataDtoFixture.Create(includeGenres: false));

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Metadata.GenresListCannotBeNull);
    }

    [Fact]
    public void Validate_WhenGenreNameIsEmpty_ShouldHaveValidationError()
    {
        // Arrange
        AddTrackCommand command = _addTrackCommandFixture.Create(metadata: _musicTrackMetadataDtoFixture.Create(genres: [_genreDtoFixture.Create(name: string.Empty)]));

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Metadata.GenreNameCannotBeEmpty);
    }

    [Fact]
    public void Validate_WhenGenreNameExceeds50Characters_ShouldHaveValidationError()
    {
        // Arrange
        AddTrackCommand command = _addTrackCommandFixture.Create(metadata: _musicTrackMetadataDtoFixture.Create(genres: [_genreDtoFixture.Create(name: new Faker().Random.String2(51))]));

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Metadata.GenreNameMustBeMaximum50CharactersLong);
    }

    [Fact]
    public void Validate_WhenGenresAreValid_ShouldNotHaveValidationError()
    {
        // Arrange
        AddTrackCommand command = _addTrackCommandFixture.Create(metadata: _musicTrackMetadataDtoFixture.Create(genres: [_genreDtoFixture.Create(name: new Faker().Random.String2(50))]));

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
        AddTrackCommand command = _addTrackCommandFixture.Create(metadata: _musicTrackMetadataDtoFixture.Create(includeTags: false));

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Metadata.TagsListCannotBeNull);
    }

    [Fact]
    public void Validate_WhenTagNameIsEmpty_ShouldHaveValidationError()
    {
        // Arrange
        AddTrackCommand command = _addTrackCommandFixture.Create(metadata: _musicTrackMetadataDtoFixture.Create(tags: [_tagDtoFixture.Create(name: string.Empty)]));

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Metadata.TagNameCannotBeEmpty);
    }

    [Fact]
    public void Validate_WhenTagNameExceeds50Characters_ShouldHaveValidationError()
    {
        // Arrange
        AddTrackCommand command = _addTrackCommandFixture.Create(metadata: _musicTrackMetadataDtoFixture.Create(tags: [_tagDtoFixture.Create(name: new Faker().Random.String2(51))]));

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Metadata.TagNameMustBeMaximum50CharactersLong);
    }

    [Fact]
    public void Validate_WhenTagsAreValid_ShouldNotHaveValidationError()
    {
        // Arrange
        AddTrackCommand command = _addTrackCommandFixture.Create(metadata: _musicTrackMetadataDtoFixture.Create(tags: [_tagDtoFixture.Create(name: new Faker().Random.String2(50))]));

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
        AddTrackCommand command = _addTrackCommandFixture.Create(metadata: _musicTrackMetadataDtoFixture.Create(includeLanguage: false));

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationError(Errors.Metadata.LanguageCodeCannotBeEmpty);
    }

    [Fact]
    public void Validate_WhenLanguageCodeIsEmpty_ShouldHaveValidationError()
    {
        // Arrange
        AddTrackCommand command = _addTrackCommandFixture.Create(metadata: _musicTrackMetadataDtoFixture.Create(language: _languageInfoDtoFixture.Create(languageCode: string.Empty)));

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Metadata.LanguageCodeCannotBeEmpty);
    }

    [Fact]
    public void Validate_WhenLanguageCodeIsNot2CharactersLong_ShouldHaveValidationError()
    {
        // Arrange
        AddTrackCommand command = _addTrackCommandFixture.Create(metadata: _musicTrackMetadataDtoFixture.Create(language: _languageInfoDtoFixture.Create(languageCode: new Faker().Random.String2(3))));

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Metadata.LanguageCodeMustBe2CharactersLong);
    }

    [Fact]
    public void Validate_WhenLanguageCodeIsValid_ShouldNotHaveValidationError()
    {
        // Arrange
        AddTrackCommand command = _addTrackCommandFixture.Create(metadata: _musicTrackMetadataDtoFixture.Create(language: _languageInfoDtoFixture.Create(languageCode: new Faker().Random.String2(2))));

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationError(Errors.Metadata.LanguageCodeMustBe2CharactersLong);
    }

    [Fact]
    public void Validate_WhenLanguageNameIsEmpty_ShouldHaveValidationError()
    {
        // Arrange
        AddTrackCommand command = _addTrackCommandFixture.Create(metadata: _musicTrackMetadataDtoFixture.Create(language: _languageInfoDtoFixture.Create(languageName: string.Empty)));

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Metadata.LanguageNameCannotBeEmpty);
    }

    [Fact]
    public void Validate_WhenLanguageNameExceeds50Characters_ShouldHaveValidationError()
    {
        // Arrange
        AddTrackCommand command = _addTrackCommandFixture.Create(metadata: _musicTrackMetadataDtoFixture.Create(language: _languageInfoDtoFixture.Create(languageName: new Faker().Random.String2(51))));

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Metadata.LanguageNameMustBeMaximum50CharactersLong);
    }

    [Fact]
    public void Validate_WhenLanguageNativeNameIsNull_ShouldNotHaveValidationError()
    {
        // Arrange
        AddTrackCommand command = _addTrackCommandFixture.Create(metadata: _musicTrackMetadataDtoFixture.Create(language: _languageInfoDtoFixture.Create(includeNativeName: false)));

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationError(Errors.Metadata.LanguageNativeNameMustBeMaximum50CharactersLong);
    }

    [Fact]
    public void Validate_WhenLanguageNativeNameExceeds50Characters_ShouldHaveValidationError()
    {
        // Arrange
        AddTrackCommand command = _addTrackCommandFixture.Create(metadata: _musicTrackMetadataDtoFixture.Create(language: _languageInfoDtoFixture.Create(nativeName: new Faker().Random.String2(51))));

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Metadata.LanguageNativeNameMustBeMaximum50CharactersLong);
    }

    [Fact]
    public void Validate_WhenOriginalLanguageIsNull_ShouldNotHaveValidationError()
    {
        // Arrange
        AddTrackCommand command = _addTrackCommandFixture.Create(metadata: _musicTrackMetadataDtoFixture.Create(includeOriginalLanguage: false));

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationError(Errors.Metadata.LanguageCodeCannotBeEmpty);
    }

    [Fact]
    public void Validate_WhenOriginalLanguageCodeIsEmpty_ShouldHaveValidationError()
    {
        // Arrange
        AddTrackCommand command = _addTrackCommandFixture.Create(metadata: _musicTrackMetadataDtoFixture.Create(originalLanguage: _languageInfoDtoFixture.Create(languageCode: string.Empty)));

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Metadata.LanguageCodeCannotBeEmpty);
    }

    [Fact]
    public void Validate_WhenOriginalLanguageCodeIsNot2CharactersLong_ShouldHaveValidationError()
    {
        // Arrange
        AddTrackCommand command = _addTrackCommandFixture.Create(metadata: _musicTrackMetadataDtoFixture.Create(originalLanguage: _languageInfoDtoFixture.Create(languageCode: new Faker().Random.String2(3))));

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Metadata.LanguageCodeMustBe2CharactersLong);
    }

    [Fact]
    public void Validate_WhenOriginalLanguageNameIsEmpty_ShouldHaveValidationError()
    {
        // Arrange
        AddTrackCommand command = _addTrackCommandFixture.Create(metadata: _musicTrackMetadataDtoFixture.Create(originalLanguage: _languageInfoDtoFixture.Create(languageName: string.Empty)));

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Metadata.LanguageNameCannotBeEmpty);
    }

    [Fact]
    public void Validate_WhenOriginalLanguageNameExceeds50Characters_ShouldHaveValidationError()
    {
        // Arrange
        AddTrackCommand command = _addTrackCommandFixture.Create(metadata: _musicTrackMetadataDtoFixture.Create(originalLanguage: _languageInfoDtoFixture.Create(languageName: new Faker().Random.String2(51))));

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Metadata.LanguageNameMustBeMaximum50CharactersLong);
    }

    [Fact]
    public void Validate_WhenOriginalLanguageNativeNameIsNull_ShouldNotHaveValidationError()
    {
        // Arrange
        AddTrackCommand command = _addTrackCommandFixture.Create(metadata: _musicTrackMetadataDtoFixture.Create(originalLanguage: _languageInfoDtoFixture.Create(includeNativeName: false)));

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationError(Errors.Metadata.LanguageNativeNameMustBeMaximum50CharactersLong);
    }

    [Fact]
    public void Validate_WhenOriginalLanguageNativeNameExceeds50Characters_ShouldHaveValidationError()
    {
        // Arrange
        AddTrackCommand command = _addTrackCommandFixture.Create(metadata: _musicTrackMetadataDtoFixture.Create(originalLanguage: _languageInfoDtoFixture.Create(nativeName: new Faker().Random.String2(51))));

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Metadata.LanguageNativeNameMustBeMaximum50CharactersLong);
    }

    [Fact]
    public void Validate_WhenMusicBrainzRecordingIdIsEmptyGuid_ShouldHaveValidationError()
    {
        // Arrange
        AddTrackCommand command = _addTrackCommandFixture.Create(musicBrainzRecordingId: Guid.Empty);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Music.MusicBrainzIdInvalidFormat);
    }

    [Fact]
    public void Validate_WhenMusicBrainzRecordingIdIsNull_ShouldNotHaveValidationError()
    {
        // Arrange
        AddTrackCommand command = _addTrackCommandFixture.Create(includeDiscNumber: false, includeScript: false, includeKey: false, includeBpm: false, includeWork: false, includeMusicBrainzRecordingId: false, includeMusicBrainzTrackId: false, includeMoods: false, includeIsrcs: false);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationError(Errors.Music.MusicBrainzIdInvalidFormat);
    }

    [Fact]
    public void Validate_WhenMusicBrainzTrackIdIsEmptyGuid_ShouldHaveValidationError()
    {
        // Arrange
        AddTrackCommand command = _addTrackCommandFixture.Create(musicBrainzTrackId: Guid.Empty);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Music.MusicBrainzIdInvalidFormat);
    }

    [Fact]
    public void Validate_WhenMusicBrainzWorkIdIsEmptyGuid_ShouldHaveValidationError()
    {
        // Arrange
        AddTrackCommand command = _addTrackCommandFixture.Create(work: _musicWorkDtoFixture.Create(musicBrainzWorkId: Guid.Empty));

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Music.MusicBrainzIdInvalidFormat);
    }

    [Fact]
    public void Validate_WhenMusicBrainzIdsAreValid_ShouldNotHaveValidationError()
    {
        // Arrange
        AddTrackCommand command = _addTrackCommandFixture.Create(
            musicBrainzRecordingId: Guid.NewGuid(),
            musicBrainzTrackId: Guid.NewGuid(),
            work: _musicWorkDtoFixture.Create(musicBrainzWorkId: Guid.NewGuid()));

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationError(Errors.Music.MusicBrainzIdInvalidFormat);
    }

    [Fact]
    public void Validate_WhenTrackNumberIsNull_ShouldHaveValidationError()
    {
        // Arrange
        AddTrackCommand command = _addTrackCommandFixture.Create(includeTrackNumber: false);

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
        AddTrackCommand command = _addTrackCommandFixture.Create(trackNumber: trackNumber);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Music.TrackNumberMustBeGreaterThanZero);
    }

    [Fact]
    public void Validate_WhenTrackNumberIsPositive_ShouldNotHaveValidationError()
    {
        // Arrange
        AddTrackCommand command = _addTrackCommandFixture.Create(trackNumber: 7);

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
        AddTrackCommand command = _addTrackCommandFixture.Create(discNumber: discNumber);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Music.DiscNumberMustBeGreaterThanZero);
    }

    [Fact]
    public void Validate_WhenDiscNumberIsNull_ShouldNotHaveValidationError()
    {
        // Arrange
        AddTrackCommand command = _addTrackCommandFixture.Create(includeDiscNumber: false, includeScript: false, includeKey: false, includeBpm: false, includeWork: false, includeMusicBrainzRecordingId: false, includeMusicBrainzTrackId: false, includeMoods: false, includeIsrcs: false);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationError(Errors.Music.DiscNumberMustBeGreaterThanZero);
    }

    [Fact]
    public void Validate_WhenDiscNumberIsPositive_ShouldNotHaveValidationError()
    {
        // Arrange
        AddTrackCommand command = _addTrackCommandFixture.Create(discNumber: 1);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationError(Errors.Music.DiscNumberMustBeGreaterThanZero);
    }

    [Fact]
    public void Validate_WhenScriptExceeds50Characters_ShouldHaveValidationError()
    {
        // Arrange
        AddTrackCommand command = _addTrackCommandFixture.Create(script: new Faker().Random.String2(51));

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Music.ScriptMustBeMaximum50CharactersLong);
    }

    [Fact]
    public void Validate_WhenScriptIsNull_ShouldNotHaveValidationError()
    {
        // Arrange
        AddTrackCommand command = _addTrackCommandFixture.Create(includeDiscNumber: false, includeScript: false, includeKey: false, includeBpm: false, includeWork: false, includeMusicBrainzRecordingId: false, includeMusicBrainzTrackId: false, includeMoods: false, includeIsrcs: false);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationError(Errors.Music.ScriptMustBeMaximum50CharactersLong);
    }

    [Fact]
    public void Validate_WhenScriptIsValid_ShouldNotHaveValidationError()
    {
        // Arrange
        AddTrackCommand command = _addTrackCommandFixture.Create(script: new Faker().Random.String2(50));

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationError(Errors.Music.ScriptMustBeMaximum50CharactersLong);
    }

    [Fact]
    public void Validate_WhenKeyIsNotDefined_ShouldHaveValidationError()
    {
        // Arrange
        AddTrackCommand command = _addTrackCommandFixture.Create(key: (MusicKey)999);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Music.UnknownMusicKey);
    }

    [Fact]
    public void Validate_WhenKeyIsNull_ShouldNotHaveValidationError()
    {
        // Arrange
        AddTrackCommand command = _addTrackCommandFixture.Create(includeDiscNumber: false, includeScript: false, includeKey: false, includeBpm: false, includeWork: false, includeMusicBrainzRecordingId: false, includeMusicBrainzTrackId: false, includeMoods: false, includeIsrcs: false);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationError(Errors.Music.UnknownMusicKey);
    }

    [Fact]
    public void Validate_WhenKeyIsDefined_ShouldNotHaveValidationError()
    {
        // Arrange
        AddTrackCommand command = _addTrackCommandFixture.Create(key: MusicKey.CMajor);

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
        AddTrackCommand command = _addTrackCommandFixture.Create(bpm: bpm);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Music.BpmMustBeGreaterThanZero);
    }

    [Fact]
    public void Validate_WhenBpmIsNull_ShouldNotHaveValidationError()
    {
        // Arrange
        AddTrackCommand command = _addTrackCommandFixture.Create(includeDiscNumber: false, includeScript: false, includeKey: false, includeBpm: false, includeWork: false, includeMusicBrainzRecordingId: false, includeMusicBrainzTrackId: false, includeMoods: false, includeIsrcs: false);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationError(Errors.Music.BpmMustBeGreaterThanZero);
    }

    [Fact]
    public void Validate_WhenBpmIsPositive_ShouldNotHaveValidationError()
    {
        // Arrange
        AddTrackCommand command = _addTrackCommandFixture.Create(bpm: 120);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationError(Errors.Music.BpmMustBeGreaterThanZero);
    }

    [Fact]
    public void Validate_WhenWorkExceeds255Characters_ShouldHaveValidationError()
    {
        // Arrange
        AddTrackCommand command = _addTrackCommandFixture.Create(work: _musicWorkDtoFixture.Create(title: new Faker().Random.String2(256)));

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Music.WorkMustBeMaximum255CharactersLong);
    }

    [Fact]
    public void Validate_WhenWorkIsNull_ShouldNotHaveValidationError()
    {
        // Arrange
        AddTrackCommand command = _addTrackCommandFixture.Create(includeDiscNumber: false, includeScript: false, includeKey: false, includeBpm: false, includeWork: false, includeMusicBrainzRecordingId: false, includeMusicBrainzTrackId: false, includeMoods: false, includeIsrcs: false);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationError(Errors.Music.WorkMustBeMaximum255CharactersLong);
    }

    [Fact]
    public void Validate_WhenWorkIsValid_ShouldNotHaveValidationError()
    {
        // Arrange
        AddTrackCommand command = _addTrackCommandFixture.Create(work: _musicWorkDtoFixture.Create(title: new Faker().Random.String2(255)));

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationError(Errors.Music.WorkMustBeMaximum255CharactersLong);
    }

    [Fact]
    public void Validate_WhenMoodNameIsEmpty_ShouldHaveValidationError()
    {
        // Arrange
        AddTrackCommand command = _addTrackCommandFixture.Create(moods: [_moodDtoFixture.Create(includeName: false)]);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Metadata.MoodNameCannotBeEmpty);
    }

    [Fact]
    public void Validate_WhenMoodNameIsValid_ShouldNotHaveValidationError()
    {
        // Arrange
        AddTrackCommand command = _addTrackCommandFixture.Create(moods: [_moodDtoFixture.Create(name: "dramatic")]);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationError(Errors.Metadata.MoodNameCannotBeEmpty);
    }

    [Fact]
    public void Validate_WhenIsrcValueIsEmpty_ShouldHaveValidationError()
    {
        // Arrange
        AddTrackCommand command = _addTrackCommandFixture.Create(isrcs: [_isrcDtoFixture.Create(includeValue: false)]);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Music.IsrcValueCannotBeEmpty);
    }

    [Fact]
    public void Validate_WhenIsrcValueIsValid_ShouldNotHaveValidationError()
    {
        // Arrange
        AddTrackCommand command = _addTrackCommandFixture.Create(isrcs: [_isrcDtoFixture.Create(value: "GBUM71029604")]);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationError(Errors.Music.IsrcValueCannotBeEmpty);
    }

    [Fact]
    public void Validate_WhenContributorsIsNull_ShouldHaveValidationError()
    {
        // Arrange
        AddTrackCommand command = _addTrackCommandFixture.Create(includeContributors: false);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.MediaContributor.ContributorsListCannotBeNull);
    }

    [Fact]
    public void Validate_WhenContributorIdIsEmpty_ShouldHaveValidationError()
    {
        // Arrange
        AddTrackCommand command = _addTrackCommandFixture.Create(contributors: [_mediaContributorReferenceDtoFixture.Create(contributorId: Guid.Empty, role: MediaContributorRole.Vocals)]);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.MediaContributor.MediaContributorIdCannotBeEmpty);
    }

    [Fact]
    public void Validate_WhenContributorRoleIsNotDefined_ShouldHaveValidationError()
    {
        // Arrange
        AddTrackCommand command = _addTrackCommandFixture.Create(contributors: [_mediaContributorReferenceDtoFixture.Create(contributorId: Guid.NewGuid(), role: (MediaContributorRole)999)]);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.MediaContributor.UnknownMediaContributorRole);
    }

    [Fact]
    public void Validate_WhenContributorsAreValid_ShouldNotHaveValidationError()
    {
        // Arrange
        AddTrackCommand command = _addTrackCommandFixture.Create(contributors: [_mediaContributorReferenceDtoFixture.Create(contributorId: Guid.NewGuid(), role: MediaContributorRole.Vocals)]);

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
        AddTrackCommand command = _addTrackCommandFixture.Create(includeRatings: false);

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
        AddTrackCommand command = _addTrackCommandFixture.Create(ratings: [_audioRatingDtoFixture.Create(value: value, maxValue: 5)]);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Metadata.RatingValueMustBePositive);
    }

    [Fact]
    public void Validate_WhenRatingValueIsGreaterThanMaxValue_ShouldHaveValidationError()
    {
        // Arrange
        AddTrackCommand command = _addTrackCommandFixture.Create(ratings: [_audioRatingDtoFixture.Create(value: 6, maxValue: 5)]);

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
        AddTrackCommand command = _addTrackCommandFixture.Create(ratings: [_audioRatingDtoFixture.Create(value: 1, maxValue: maxValue)]);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Metadata.RatingMaxValueMustBePositive);
    }

    [Fact]
    public void Validate_WhenRatingVoteCountIsNegative_ShouldHaveValidationError()
    {
        // Arrange
        AddTrackCommand command = _addTrackCommandFixture.Create(ratings: [_audioRatingDtoFixture.Create(value: 4, maxValue: 5, voteCount: -1)]);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Metadata.RatingVoteCountMustBePositive);
    }

    [Fact]
    public void Validate_WhenRatingVoteCountIsNull_ShouldNotHaveValidationError()
    {
        // Arrange
        AddTrackCommand command = _addTrackCommandFixture.Create(ratings: [_audioRatingDtoFixture.Create(value: 4, maxValue: 5, includeVoteCount: false)]);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationError(Errors.Metadata.RatingVoteCountMustBePositive);
    }

    [Fact]
    public void Validate_WhenRatingsAreValid_ShouldNotHaveValidationError()
    {
        // Arrange
        AddTrackCommand command = _addTrackCommandFixture.Create(ratings: [_audioRatingDtoFixture.Create(value: 4, maxValue: 5, voteCount: 100)]);

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
    public void Validate_WhenCommandIsValid_ShouldNotHaveAnyValidationErrors()
    {
        // Arrange
        AddTrackCommand command = _addTrackCommandFixture.Create();

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }
}
