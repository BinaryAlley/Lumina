#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Application.Core.MediaLibrary.AudioLibrary.MusicLibrary.Tracks.Commands.UpdateTrack;
using Lumina.Application.Fixtures.Core.MediaLibrary.AudioLibrary.MusicLibrary.Tracks.Commands.UpdateTrack;
using Lumina.Application.UnitTests.Common.Setup;
using Lumina.Contracts.DTO.Common;
using Lumina.Contracts.DTO.MediaContributors;
using Lumina.Contracts.DTO.MediaLibrary.AudioLibrary;
using Lumina.Contracts.DTO.MediaLibrary.AudioLibrary.MusicLibrary;
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

namespace Lumina.Application.UnitTests.Core.MediaLibrary.AudioLibrary.MusicLibrary.Tracks.Commands.UpdateTrack;

/// <summary>
/// Contains unit tests for the <see cref="UpdateTrackCommandValidator"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class UpdateTrackCommandValidatorTests
{
    private readonly UpdateTrackCommandFixture _updateTrackCommandFixture = new();
    private readonly UpdateTrackCommandValidator _validator = new();
    private readonly MusicTrackMetadataDtoFixture _musicTrackMetadataDtoFixture = new();
    private readonly MusicWorkDtoFixture _musicWorkDtoFixture = new();
    private readonly ReleaseInfoDtoFixture _releaseInfoDtoFixture = new();
    private readonly GenreDtoFixture _genreDtoFixture = new();
    private readonly TagDtoFixture _tagDtoFixture = new();
    private readonly LanguageInfoDtoFixture _languageInfoDtoFixture = new();
    private readonly MoodDtoFixture _moodDtoFixture = new();
    private readonly IsrcDtoFixture _isrcDtoFixture = new();
    private readonly MediaContributorReferenceDtoFixture _mediaContributorReferenceDtoFixture = new();
    private readonly AudioRatingDtoFixture _audioRatingDtoFixture = new();

    [Fact]
    public void Validate_WhenLibraryIdIsNull_ShouldHaveValidationError()
    {
        // Arrange
        UpdateTrackCommand command = _updateTrackCommandFixture.Create(includeLibraryId: false);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Library.LibraryIdCannotBeEmpty);
    }

    [Fact]
    public void Validate_WhenLibraryIdIsEmpty_ShouldHaveValidationError()
    {
        // Arrange
        UpdateTrackCommand command = _updateTrackCommandFixture.Create(libraryId: string.Empty);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Library.LibraryIdCannotBeEmpty);
    }

    [Fact]
    public void Validate_WhenLibraryIdIsWhitespace_ShouldHaveValidationError()
    {
        // Arrange
        UpdateTrackCommand command = _updateTrackCommandFixture.Create(libraryId: " ");

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Library.LibraryIdCannotBeEmpty);
    }

    [Fact]
    public void Validate_WhenLibraryIdIsNotAGuid_ShouldHaveValidationError()
    {
        // Arrange
        UpdateTrackCommand command = _updateTrackCommandFixture.Create(libraryId: "not-a-guid");

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Library.LibraryIdCannotBeEmpty);
    }

    [Fact]
    public void Validate_WhenLibraryIdIsAnEmptyGuid_ShouldHaveValidationError()
    {
        // Arrange
        UpdateTrackCommand command = _updateTrackCommandFixture.Create(libraryId: Guid.Empty.ToString());

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Library.LibraryIdCannotBeEmpty);
    }

    [Fact]
    public void Validate_WhenLibraryIdIsValid_ShouldNotHaveValidationError()
    {
        // Arrange
        UpdateTrackCommand command = _updateTrackCommandFixture.Create(libraryId: Guid.NewGuid().ToString());

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationError(Errors.Library.LibraryIdCannotBeEmpty);
    }

    [Fact]
    public void Validate_WhenArtistIdIsNull_ShouldHaveValidationError()
    {
        // Arrange
        UpdateTrackCommand command = _updateTrackCommandFixture.Create(includeArtistId: false);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Music.ArtistIdCannotBeEmpty);
    }

    [Fact]
    public void Validate_WhenArtistIdIsEmpty_ShouldHaveValidationError()
    {
        // Arrange
        UpdateTrackCommand command = _updateTrackCommandFixture.Create(artistId: string.Empty);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Music.ArtistIdCannotBeEmpty);
    }

    [Fact]
    public void Validate_WhenArtistIdIsWhitespace_ShouldHaveValidationError()
    {
        // Arrange
        UpdateTrackCommand command = _updateTrackCommandFixture.Create(artistId: " ");

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Music.ArtistIdCannotBeEmpty);
    }

    [Fact]
    public void Validate_WhenArtistIdIsNotAGuid_ShouldHaveValidationError()
    {
        // Arrange
        UpdateTrackCommand command = _updateTrackCommandFixture.Create(artistId: "not-a-guid");

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Music.ArtistIdCannotBeEmpty);
    }

    [Fact]
    public void Validate_WhenArtistIdIsAnEmptyGuid_ShouldHaveValidationError()
    {
        // Arrange
        UpdateTrackCommand command = _updateTrackCommandFixture.Create(artistId: Guid.Empty.ToString());

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Music.ArtistIdCannotBeEmpty);
    }

    [Fact]
    public void Validate_WhenArtistIdIsValid_ShouldNotHaveValidationError()
    {
        // Arrange
        UpdateTrackCommand command = _updateTrackCommandFixture.Create(artistId: Guid.NewGuid().ToString());

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationError(Errors.Music.ArtistIdCannotBeEmpty);
    }

    [Fact]
    public void Validate_WhenAlbumIdIsNull_ShouldHaveValidationError()
    {
        // Arrange
        UpdateTrackCommand command = _updateTrackCommandFixture.Create(includeAlbumId: false);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Music.AlbumIdCannotBeEmpty);
    }

    [Fact]
    public void Validate_WhenAlbumIdIsEmpty_ShouldHaveValidationError()
    {
        // Arrange
        UpdateTrackCommand command = _updateTrackCommandFixture.Create(albumId: string.Empty);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Music.AlbumIdCannotBeEmpty);
    }

    [Fact]
    public void Validate_WhenAlbumIdIsWhitespace_ShouldHaveValidationError()
    {
        // Arrange
        UpdateTrackCommand command = _updateTrackCommandFixture.Create(albumId: " ");

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Music.AlbumIdCannotBeEmpty);
    }

    [Fact]
    public void Validate_WhenAlbumIdIsNotAGuid_ShouldHaveValidationError()
    {
        // Arrange
        UpdateTrackCommand command = _updateTrackCommandFixture.Create(albumId: "not-a-guid");

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Music.AlbumIdCannotBeEmpty);
    }

    [Fact]
    public void Validate_WhenAlbumIdIsAnEmptyGuid_ShouldHaveValidationError()
    {
        // Arrange
        UpdateTrackCommand command = _updateTrackCommandFixture.Create(albumId: Guid.Empty.ToString());

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Music.AlbumIdCannotBeEmpty);
    }

    [Fact]
    public void Validate_WhenAlbumIdIsValid_ShouldNotHaveValidationError()
    {
        // Arrange
        UpdateTrackCommand command = _updateTrackCommandFixture.Create(albumId: Guid.NewGuid().ToString());

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationError(Errors.Music.AlbumIdCannotBeEmpty);
    }

    [Fact]
    public void Validate_WhenTrackIdIsNull_ShouldHaveValidationError()
    {
        // Arrange
        UpdateTrackCommand command = _updateTrackCommandFixture.Create(includeTrackId: false);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Music.TrackIdCannotBeEmpty);
    }

    [Fact]
    public void Validate_WhenTrackIdIsEmpty_ShouldHaveValidationError()
    {
        // Arrange
        UpdateTrackCommand command = _updateTrackCommandFixture.Create(trackId: string.Empty);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Music.TrackIdCannotBeEmpty);
    }

    [Fact]
    public void Validate_WhenTrackIdIsWhitespace_ShouldHaveValidationError()
    {
        // Arrange
        UpdateTrackCommand command = _updateTrackCommandFixture.Create(trackId: " ");

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Music.TrackIdCannotBeEmpty);
    }

    [Fact]
    public void Validate_WhenTrackIdIsNotAGuid_ShouldHaveValidationError()
    {
        // Arrange
        UpdateTrackCommand command = _updateTrackCommandFixture.Create(trackId: "not-a-guid");

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Music.TrackIdCannotBeEmpty);
    }

    [Fact]
    public void Validate_WhenTrackIdIsAnEmptyGuid_ShouldHaveValidationError()
    {
        // Arrange
        UpdateTrackCommand command = _updateTrackCommandFixture.Create(trackId: Guid.Empty.ToString());

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Music.TrackIdCannotBeEmpty);
    }

    [Fact]
    public void Validate_WhenTrackIdIsValid_ShouldNotHaveValidationError()
    {
        // Arrange
        UpdateTrackCommand command = _updateTrackCommandFixture.Create(trackId: Guid.NewGuid().ToString());

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationError(Errors.Music.TrackIdCannotBeEmpty);
    }

    [Fact]
    public void Validate_WhenPathIsNull_ShouldHaveValidationError()
    {
        // Arrange
        UpdateTrackCommand command = _updateTrackCommandFixture.Create(includePath: false);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Music.TrackPathCannotBeEmpty);
    }

    [Fact]
    public void Validate_WhenPathIsEmpty_ShouldHaveValidationError()
    {
        // Arrange
        UpdateTrackCommand command = _updateTrackCommandFixture.Create(path: string.Empty);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Music.TrackPathCannotBeEmpty);
    }

    [Fact]
    public void Validate_WhenPathIsWhitespace_ShouldHaveValidationError()
    {
        // Arrange
        UpdateTrackCommand command = _updateTrackCommandFixture.Create(path: " ");

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Music.TrackPathCannotBeEmpty);
    }

    [Fact]
    public void Validate_WhenPathExceeds2048Characters_ShouldHaveValidationError()
    {
        // Arrange
        UpdateTrackCommand command = _updateTrackCommandFixture.Create(path: new Faker().Random.String2(2049));

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Music.TrackPathMustBeMaximum2048CharactersLong);
    }

    [Fact]
    public void Validate_WhenPathIs2048Characters_ShouldNotHaveValidationError()
    {
        // Arrange
        UpdateTrackCommand command = _updateTrackCommandFixture.Create(path: new Faker().Random.String2(2048));

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationError(Errors.Music.TrackPathMustBeMaximum2048CharactersLong);
    }

    [Fact]
    public void Validate_WhenMetadataIsNull_ShouldHaveValidationError()
    {
        // Arrange
        UpdateTrackCommand command = _updateTrackCommandFixture.Create(includeMetadata: false);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Metadata.MetadataCannotBeNull);
    }

    [Fact]
    public void Validate_WhenTitleIsNull_ShouldHaveValidationError()
    {
        // Arrange
        UpdateTrackCommand command = _updateTrackCommandFixture.Create(metadata: _musicTrackMetadataDtoFixture.Create(includeTitle: false));

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Metadata.TitleCannotBeEmpty);
    }

    [Fact]
    public void Validate_WhenTitleIsEmpty_ShouldHaveValidationError()
    {
        // Arrange
        UpdateTrackCommand command = _updateTrackCommandFixture.Create(metadata: _musicTrackMetadataDtoFixture.Create(title: string.Empty));

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Metadata.TitleCannotBeEmpty);
    }

    [Fact]
    public void Validate_WhenTitleIsWhitespace_ShouldHaveValidationError()
    {
        // Arrange
        UpdateTrackCommand command = _updateTrackCommandFixture.Create(metadata: _musicTrackMetadataDtoFixture.Create(title: " "));

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Metadata.TitleCannotBeEmpty);
    }

    [Fact]
    public void Validate_WhenTitleExceeds255Characters_ShouldHaveValidationError()
    {
        // Arrange
        UpdateTrackCommand command = _updateTrackCommandFixture.Create(metadata: _musicTrackMetadataDtoFixture.Create(title: new Faker().Random.String2(256)));

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Metadata.TitleMustBeMaximum255CharactersLong);
    }

    [Fact]
    public void Validate_WhenTitleIs255Characters_ShouldNotHaveValidationError()
    {
        // Arrange
        UpdateTrackCommand command = _updateTrackCommandFixture.Create(metadata: _musicTrackMetadataDtoFixture.Create(title: new Faker().Random.String2(255)));

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationError(Errors.Metadata.TitleMustBeMaximum255CharactersLong);
    }

    [Fact]
    public void Validate_WhenOriginalTitleExceeds255Characters_ShouldHaveValidationError()
    {
        // Arrange
        UpdateTrackCommand command = _updateTrackCommandFixture.Create(metadata: _musicTrackMetadataDtoFixture.Create(originalTitle: new Faker().Random.String2(256)));

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Metadata.OriginalTitleMustBeMaximum255CharactersLong);
    }

    [Fact]
    public void Validate_WhenOriginalTitleIs255Characters_ShouldNotHaveValidationError()
    {
        // Arrange
        UpdateTrackCommand command = _updateTrackCommandFixture.Create(metadata: _musicTrackMetadataDtoFixture.Create(originalTitle: new Faker().Random.String2(255)));

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationError(Errors.Metadata.OriginalTitleMustBeMaximum255CharactersLong);
    }

    [Fact]
    public void Validate_WhenOriginalTitleIsNull_ShouldNotHaveValidationError()
    {
        // Arrange
        UpdateTrackCommand command = _updateTrackCommandFixture.Create(metadata: _musicTrackMetadataDtoFixture.Create(includeOriginalTitle: false));

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationError(Errors.Metadata.OriginalTitleMustBeMaximum255CharactersLong);
    }

    [Fact]
    public void Validate_WhenDescriptionExceeds2000Characters_ShouldHaveValidationError()
    {
        // Arrange
        UpdateTrackCommand command = _updateTrackCommandFixture.Create(metadata: _musicTrackMetadataDtoFixture.Create(description: new Faker().Random.String2(2001)));

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Metadata.DescriptionMustBeMaximum2000CharactersLong);
    }

    [Fact]
    public void Validate_WhenDescriptionIs2000Characters_ShouldNotHaveValidationError()
    {
        // Arrange
        UpdateTrackCommand command = _updateTrackCommandFixture.Create(metadata: _musicTrackMetadataDtoFixture.Create(description: new Faker().Random.String2(2000)));

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationError(Errors.Metadata.DescriptionMustBeMaximum2000CharactersLong);
    }

    [Fact]
    public void Validate_WhenDescriptionIsNull_ShouldNotHaveValidationError()
    {
        // Arrange
        UpdateTrackCommand command = _updateTrackCommandFixture.Create(metadata: _musicTrackMetadataDtoFixture.Create(includeDescription: false));

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationError(Errors.Metadata.DescriptionMustBeMaximum2000CharactersLong);
    }

    [Fact]
    public void Validate_WhenReleaseInfoIsNull_ShouldHaveValidationError()
    {
        // Arrange
        UpdateTrackCommand command = _updateTrackCommandFixture.Create(metadata: _musicTrackMetadataDtoFixture.Create(includeReleaseInfo: false));

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Metadata.ReleaseInfoCannotBeNull);
    }

    [Fact]
    public void Validate_WhenOriginalReleaseYearIsZero_ShouldHaveValidationError()
    {
        // Arrange
        UpdateTrackCommand command = _updateTrackCommandFixture.Create(metadata: _musicTrackMetadataDtoFixture.Create(releaseInfo: _releaseInfoDtoFixture.Create(includeOriginalReleaseDate: false, originalReleaseYear: 0, includeReReleaseDate: false, includeReReleaseYear: false)));

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Metadata.OriginalReleaseYearMustBeBetween1And9999);
    }

    [Fact]
    public void Validate_WhenOriginalReleaseYearIsGreaterThan9999_ShouldHaveValidationError()
    {
        // Arrange
        UpdateTrackCommand command = _updateTrackCommandFixture.Create(metadata: _musicTrackMetadataDtoFixture.Create(releaseInfo: _releaseInfoDtoFixture.Create(includeOriginalReleaseDate: false, originalReleaseYear: 10000, includeReReleaseDate: false, includeReReleaseYear: false)));

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Metadata.OriginalReleaseYearMustBeBetween1And9999);
    }

    [Fact]
    public void Validate_WhenOriginalReleaseYearIsValid_ShouldNotHaveValidationError()
    {
        // Arrange
        UpdateTrackCommand command = _updateTrackCommandFixture.Create(metadata: _musicTrackMetadataDtoFixture.Create(releaseInfo: _releaseInfoDtoFixture.Create(includeOriginalReleaseDate: false, originalReleaseYear: 2000, includeReReleaseDate: false, includeReReleaseYear: false)));

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationError(Errors.Metadata.OriginalReleaseYearMustBeBetween1And9999);
    }

    [Fact]
    public void Validate_WhenOriginalReleaseYearIsNull_ShouldNotHaveValidationError()
    {
        // Arrange
        UpdateTrackCommand command = _updateTrackCommandFixture.Create(metadata: _musicTrackMetadataDtoFixture.Create(releaseInfo: _releaseInfoDtoFixture.Create(includeOriginalReleaseDate: false, includeOriginalReleaseYear: false, includeReReleaseDate: false, includeReReleaseYear: false)));

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationError(Errors.Metadata.OriginalReleaseYearMustBeBetween1And9999);
    }

    [Fact]
    public void Validate_WhenReReleaseYearIsZero_ShouldHaveValidationError()
    {
        // Arrange
        UpdateTrackCommand command = _updateTrackCommandFixture.Create(metadata: _musicTrackMetadataDtoFixture.Create(releaseInfo: _releaseInfoDtoFixture.Create(includeOriginalReleaseDate: false, includeOriginalReleaseYear: false, includeReReleaseDate: false, reReleaseYear: 0)));

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Metadata.ReReleaseYearMustBeBetween1And9999);
    }

    [Fact]
    public void Validate_WhenReReleaseYearIsGreaterThan9999_ShouldHaveValidationError()
    {
        // Arrange
        UpdateTrackCommand command = _updateTrackCommandFixture.Create(metadata: _musicTrackMetadataDtoFixture.Create(releaseInfo: _releaseInfoDtoFixture.Create(includeOriginalReleaseDate: false, includeOriginalReleaseYear: false, includeReReleaseDate: false, reReleaseYear: 10000)));

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Metadata.ReReleaseYearMustBeBetween1And9999);
    }

    [Fact]
    public void Validate_WhenReReleaseYearIsValid_ShouldNotHaveValidationError()
    {
        // Arrange
        UpdateTrackCommand command = _updateTrackCommandFixture.Create(metadata: _musicTrackMetadataDtoFixture.Create(releaseInfo: _releaseInfoDtoFixture.Create(includeOriginalReleaseDate: false, originalReleaseYear: 2000, includeReReleaseDate: false, reReleaseYear: 2001)));

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationError(Errors.Metadata.ReReleaseYearMustBeBetween1And9999);
    }

    [Fact]
    public void Validate_WhenReReleaseYearIsNull_ShouldNotHaveValidationError()
    {
        // Arrange
        UpdateTrackCommand command = _updateTrackCommandFixture.Create(metadata: _musicTrackMetadataDtoFixture.Create(releaseInfo: _releaseInfoDtoFixture.Create(includeOriginalReleaseDate: false, includeOriginalReleaseYear: false, includeReReleaseDate: false, includeReReleaseYear: false)));

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationError(Errors.Metadata.ReReleaseYearMustBeBetween1And9999);
    }

    [Fact]
    public void Validate_WhenReleaseVersionExceeds50Characters_ShouldHaveValidationError()
    {
        // Arrange
        UpdateTrackCommand command = _updateTrackCommandFixture.Create(metadata: _musicTrackMetadataDtoFixture.Create(releaseInfo: _releaseInfoDtoFixture.Create(releaseVersion: new Faker().Random.String2(51))));

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Metadata.ReleaseVersionMustBeMaximum50CharactersLong);
    }

    [Fact]
    public void Validate_WhenReleaseVersionIs50Characters_ShouldNotHaveValidationError()
    {
        // Arrange
        UpdateTrackCommand command = _updateTrackCommandFixture.Create(metadata: _musicTrackMetadataDtoFixture.Create(releaseInfo: _releaseInfoDtoFixture.Create(releaseVersion: new Faker().Random.String2(50))));

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationError(Errors.Metadata.ReleaseVersionMustBeMaximum50CharactersLong);
    }

    [Fact]
    public void Validate_WhenReleaseVersionIsNull_ShouldNotHaveValidationError()
    {
        // Arrange
        UpdateTrackCommand command = _updateTrackCommandFixture.Create(metadata: _musicTrackMetadataDtoFixture.Create(releaseInfo: _releaseInfoDtoFixture.Create(includeReleaseVersion: false)));

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationError(Errors.Metadata.ReleaseVersionMustBeMaximum50CharactersLong);
    }

    [Fact]
    public void Validate_WhenOriginalReleaseDateAndYearDoNotMatch_ShouldHaveValidationError()
    {
        // Arrange
        UpdateTrackCommand command = _updateTrackCommandFixture.Create(metadata: _musicTrackMetadataDtoFixture.Create(releaseInfo: _releaseInfoDtoFixture.Create(originalReleaseDate: new DateOnly(2020, 1, 1), originalReleaseYear: 2021, includeReReleaseDate: false, includeReReleaseYear: false)));

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Metadata.OriginalReleaseDateAndYearMustMatch);
    }

    [Fact]
    public void Validate_WhenOriginalReleaseDateAndYearMatch_ShouldNotHaveValidationError()
    {
        // Arrange
        UpdateTrackCommand command = _updateTrackCommandFixture.Create(metadata: _musicTrackMetadataDtoFixture.Create(releaseInfo: _releaseInfoDtoFixture.Create(originalReleaseDate: new DateOnly(2020, 1, 1), originalReleaseYear: 2020, includeReReleaseDate: false, includeReReleaseYear: false)));

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationError(Errors.Metadata.OriginalReleaseDateAndYearMustMatch);
    }

    [Fact]
    public void Validate_WhenReReleaseDateAndYearDoNotMatch_ShouldHaveValidationError()
    {
        // Arrange
        UpdateTrackCommand command = _updateTrackCommandFixture.Create(metadata: _musicTrackMetadataDtoFixture.Create(releaseInfo: _releaseInfoDtoFixture.Create(includeOriginalReleaseDate: false, includeOriginalReleaseYear: false, reReleaseDate: new DateOnly(2020, 1, 1), reReleaseYear: 2021)));

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Metadata.ReReleaseDateAndYearMustMatch);
    }

    [Fact]
    public void Validate_WhenReReleaseDateAndYearMatch_ShouldNotHaveValidationError()
    {
        // Arrange
        UpdateTrackCommand command = _updateTrackCommandFixture.Create(metadata: _musicTrackMetadataDtoFixture.Create(releaseInfo: _releaseInfoDtoFixture.Create(includeOriginalReleaseDate: false, includeOriginalReleaseYear: false, reReleaseDate: new DateOnly(2020, 1, 1), reReleaseYear: 2020)));

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationError(Errors.Metadata.ReReleaseDateAndYearMustMatch);
    }

    [Fact]
    public void Validate_WhenReReleaseYearIsBeforeOriginalReleaseYear_ShouldHaveValidationError()
    {
        // Arrange
        UpdateTrackCommand command = _updateTrackCommandFixture.Create(metadata: _musicTrackMetadataDtoFixture.Create(releaseInfo: _releaseInfoDtoFixture.Create(includeOriginalReleaseDate: false, originalReleaseYear: 2001, includeReReleaseDate: false, reReleaseYear: 2000)));

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Metadata.ReReleaseYearCannotBeEarlierThanOriginalReleaseYear);
    }

    [Fact]
    public void Validate_WhenReReleaseYearIsAfterOriginalReleaseYear_ShouldNotHaveValidationError()
    {
        // Arrange
        UpdateTrackCommand command = _updateTrackCommandFixture.Create(metadata: _musicTrackMetadataDtoFixture.Create(releaseInfo: _releaseInfoDtoFixture.Create(includeOriginalReleaseDate: false, originalReleaseYear: 2000, includeReReleaseDate: false, reReleaseYear: 2001)));

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationError(Errors.Metadata.ReReleaseYearCannotBeEarlierThanOriginalReleaseYear);
    }

    [Fact]
    public void Validate_WhenReReleaseDateIsBeforeOriginalReleaseDate_ShouldHaveValidationError()
    {
        // Arrange
        UpdateTrackCommand command = _updateTrackCommandFixture.Create(metadata: _musicTrackMetadataDtoFixture.Create(releaseInfo: _releaseInfoDtoFixture.Create(originalReleaseDate: new DateOnly(2001, 1, 1), includeOriginalReleaseYear: false, reReleaseDate: new DateOnly(2000, 1, 1), includeReReleaseYear: false)));

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Metadata.ReReleaseDateCannotBeEarlierThanOriginalReleaseDate);
    }

    [Fact]
    public void Validate_WhenReReleaseDateIsAfterOriginalReleaseDate_ShouldNotHaveValidationError()
    {
        // Arrange
        UpdateTrackCommand command = _updateTrackCommandFixture.Create(metadata: _musicTrackMetadataDtoFixture.Create(releaseInfo: _releaseInfoDtoFixture.Create(originalReleaseDate: new DateOnly(2000, 1, 1), includeOriginalReleaseYear: false, reReleaseDate: new DateOnly(2001, 1, 1), includeReReleaseYear: false)));

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationError(Errors.Metadata.ReReleaseDateCannotBeEarlierThanOriginalReleaseDate);
    }

    [Fact]
    public void Validate_WhenGenresIsNull_ShouldHaveValidationError()
    {
        // Arrange
        UpdateTrackCommand command = _updateTrackCommandFixture.Create(metadata: _musicTrackMetadataDtoFixture.Create(includeGenres: false));

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Metadata.GenresListCannotBeNull);
    }

    [Fact]
    public void Validate_WhenGenreNameIsEmpty_ShouldHaveValidationError()
    {
        // Arrange
        UpdateTrackCommand command = _updateTrackCommandFixture.Create(metadata: _musicTrackMetadataDtoFixture.Create(genres: [_genreDtoFixture.Create(name: string.Empty)]));

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Metadata.GenreNameCannotBeEmpty);
    }

    [Fact]
    public void Validate_WhenGenreNameExceeds50Characters_ShouldHaveValidationError()
    {
        // Arrange
        UpdateTrackCommand command = _updateTrackCommandFixture.Create(metadata: _musicTrackMetadataDtoFixture.Create(genres: [_genreDtoFixture.Create(name: new Faker().Random.String2(51))]));

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Metadata.GenreNameMustBeMaximum50CharactersLong);
    }

    [Fact]
    public void Validate_WhenGenresAreValid_ShouldNotHaveValidationError()
    {
        // Arrange
        UpdateTrackCommand command = _updateTrackCommandFixture.Create(metadata: _musicTrackMetadataDtoFixture.Create(genres: [_genreDtoFixture.Create(name: new Faker().Random.String2(50))]));

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationError(Errors.Metadata.GenresListCannotBeNull);
    }

    [Fact]
    public void Validate_WhenTagsIsNull_ShouldHaveValidationError()
    {
        // Arrange
        UpdateTrackCommand command = _updateTrackCommandFixture.Create(metadata: _musicTrackMetadataDtoFixture.Create(includeTags: false));

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Metadata.TagsListCannotBeNull);
    }

    [Fact]
    public void Validate_WhenTagNameIsEmpty_ShouldHaveValidationError()
    {
        // Arrange
        UpdateTrackCommand command = _updateTrackCommandFixture.Create(metadata: _musicTrackMetadataDtoFixture.Create(tags: [_tagDtoFixture.Create(name: string.Empty)]));

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Metadata.TagNameCannotBeEmpty);
    }

    [Fact]
    public void Validate_WhenTagNameExceeds50Characters_ShouldHaveValidationError()
    {
        // Arrange
        UpdateTrackCommand command = _updateTrackCommandFixture.Create(metadata: _musicTrackMetadataDtoFixture.Create(tags: [_tagDtoFixture.Create(name: new Faker().Random.String2(51))]));

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Metadata.TagNameMustBeMaximum50CharactersLong);
    }

    [Fact]
    public void Validate_WhenTagsAreValid_ShouldNotHaveValidationError()
    {
        // Arrange
        UpdateTrackCommand command = _updateTrackCommandFixture.Create(metadata: _musicTrackMetadataDtoFixture.Create(tags: [_tagDtoFixture.Create(name: new Faker().Random.String2(50))]));

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationError(Errors.Metadata.TagsListCannotBeNull);
    }

    [Fact]
    public void Validate_WhenLanguageIsNull_ShouldNotHaveValidationError()
    {
        // Arrange
        UpdateTrackCommand command = _updateTrackCommandFixture.Create(metadata: _musicTrackMetadataDtoFixture.Create(includeLanguage: false));

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationError(Errors.Metadata.LanguageCodeCannotBeEmpty);
    }

    [Fact]
    public void Validate_WhenLanguageCodeIsEmpty_ShouldHaveValidationError()
    {
        // Arrange
        UpdateTrackCommand command = _updateTrackCommandFixture.Create(metadata: _musicTrackMetadataDtoFixture.Create(language: _languageInfoDtoFixture.Create(languageCode: string.Empty)));

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Metadata.LanguageCodeCannotBeEmpty);
    }

    [Fact]
    public void Validate_WhenLanguageCodeIsShorterThan2Characters_ShouldHaveValidationError()
    {
        // Arrange
        UpdateTrackCommand command = _updateTrackCommandFixture.Create(metadata: _musicTrackMetadataDtoFixture.Create(language: _languageInfoDtoFixture.Create(languageCode: new Faker().Random.String2(1))));

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Metadata.LanguageCodeMustBe2CharactersLong);
    }

    [Fact]
    public void Validate_WhenLanguageCodeExceeds2Characters_ShouldHaveValidationError()
    {
        // Arrange
        UpdateTrackCommand command = _updateTrackCommandFixture.Create(metadata: _musicTrackMetadataDtoFixture.Create(language: _languageInfoDtoFixture.Create(languageCode: new Faker().Random.String2(3))));

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Metadata.LanguageCodeMustBe2CharactersLong);
    }

    [Fact]
    public void Validate_WhenLanguageNameIsEmpty_ShouldHaveValidationError()
    {
        // Arrange
        UpdateTrackCommand command = _updateTrackCommandFixture.Create(metadata: _musicTrackMetadataDtoFixture.Create(language: _languageInfoDtoFixture.Create(languageName: string.Empty)));

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Metadata.LanguageNameCannotBeEmpty);
    }

    [Fact]
    public void Validate_WhenLanguageNameExceeds50Characters_ShouldHaveValidationError()
    {
        // Arrange
        UpdateTrackCommand command = _updateTrackCommandFixture.Create(metadata: _musicTrackMetadataDtoFixture.Create(language: _languageInfoDtoFixture.Create(languageName: new Faker().Random.String2(51))));

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Metadata.LanguageNameMustBeMaximum50CharactersLong);
    }

    [Fact]
    public void Validate_WhenLanguageNativeNameIsNull_ShouldNotHaveValidationError()
    {
        // Arrange
        UpdateTrackCommand command = _updateTrackCommandFixture.Create(metadata: _musicTrackMetadataDtoFixture.Create(language: _languageInfoDtoFixture.Create(includeNativeName: false)));

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationError(Errors.Metadata.LanguageNativeNameMustBeMaximum50CharactersLong);
    }

    [Fact]
    public void Validate_WhenLanguageNativeNameExceeds50Characters_ShouldHaveValidationError()
    {
        // Arrange
        UpdateTrackCommand command = _updateTrackCommandFixture.Create(metadata: _musicTrackMetadataDtoFixture.Create(language: _languageInfoDtoFixture.Create(nativeName: new Faker().Random.String2(51))));

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Metadata.LanguageNativeNameMustBeMaximum50CharactersLong);
    }

    [Fact]
    public void Validate_WhenOriginalLanguageIsNull_ShouldNotHaveValidationError()
    {
        // Arrange
        UpdateTrackCommand command = _updateTrackCommandFixture.Create(metadata: _musicTrackMetadataDtoFixture.Create(includeOriginalLanguage: false));

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationError(Errors.Metadata.LanguageCodeCannotBeEmpty);
    }

    [Fact]
    public void Validate_WhenOriginalLanguageCodeIsEmpty_ShouldHaveValidationError()
    {
        // Arrange
        UpdateTrackCommand command = _updateTrackCommandFixture.Create(metadata: _musicTrackMetadataDtoFixture.Create(originalLanguage: _languageInfoDtoFixture.Create(languageCode: string.Empty)));

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Metadata.LanguageCodeCannotBeEmpty);
    }

    [Fact]
    public void Validate_WhenOriginalLanguageCodeIsShorterThan2Characters_ShouldHaveValidationError()
    {
        // Arrange
        UpdateTrackCommand command = _updateTrackCommandFixture.Create(metadata: _musicTrackMetadataDtoFixture.Create(originalLanguage: _languageInfoDtoFixture.Create(languageCode: new Faker().Random.String2(1))));

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Metadata.LanguageCodeMustBe2CharactersLong);
    }

    [Fact]
    public void Validate_WhenOriginalLanguageCodeExceeds2Characters_ShouldHaveValidationError()
    {
        // Arrange
        UpdateTrackCommand command = _updateTrackCommandFixture.Create(metadata: _musicTrackMetadataDtoFixture.Create(originalLanguage: _languageInfoDtoFixture.Create(languageCode: new Faker().Random.String2(3))));

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Metadata.LanguageCodeMustBe2CharactersLong);
    }

    [Fact]
    public void Validate_WhenOriginalLanguageNameIsEmpty_ShouldHaveValidationError()
    {
        // Arrange
        UpdateTrackCommand command = _updateTrackCommandFixture.Create(metadata: _musicTrackMetadataDtoFixture.Create(originalLanguage: _languageInfoDtoFixture.Create(languageName: string.Empty)));

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Metadata.LanguageNameCannotBeEmpty);
    }

    [Fact]
    public void Validate_WhenOriginalLanguageNameExceeds50Characters_ShouldHaveValidationError()
    {
        // Arrange
        UpdateTrackCommand command = _updateTrackCommandFixture.Create(metadata: _musicTrackMetadataDtoFixture.Create(originalLanguage: _languageInfoDtoFixture.Create(languageName: new Faker().Random.String2(51))));

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Metadata.LanguageNameMustBeMaximum50CharactersLong);
    }

    [Fact]
    public void Validate_WhenOriginalLanguageNativeNameIsNull_ShouldNotHaveValidationError()
    {
        // Arrange
        UpdateTrackCommand command = _updateTrackCommandFixture.Create(metadata: _musicTrackMetadataDtoFixture.Create(originalLanguage: _languageInfoDtoFixture.Create(includeNativeName: false)));

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationError(Errors.Metadata.LanguageNativeNameMustBeMaximum50CharactersLong);
    }

    [Fact]
    public void Validate_WhenOriginalLanguageNativeNameExceeds50Characters_ShouldHaveValidationError()
    {
        // Arrange
        UpdateTrackCommand command = _updateTrackCommandFixture.Create(metadata: _musicTrackMetadataDtoFixture.Create(originalLanguage: _languageInfoDtoFixture.Create(nativeName: new Faker().Random.String2(51))));

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Metadata.LanguageNativeNameMustBeMaximum50CharactersLong);
    }

    [Fact]
    public void Validate_WhenMusicBrainzRecordingIdIsEmptyGuid_ShouldHaveValidationError()
    {
        // Arrange
        UpdateTrackCommand command = _updateTrackCommandFixture.Create(musicBrainzRecordingId: Guid.Empty);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Music.MusicBrainzIdInvalidFormat);
    }

    [Fact]
    public void Validate_WhenMusicBrainzRecordingIdIsValid_ShouldNotHaveValidationError()
    {
        // Arrange
        UpdateTrackCommand command = _updateTrackCommandFixture.Create(musicBrainzRecordingId: Guid.NewGuid());

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationError(Errors.Music.MusicBrainzIdInvalidFormat);
    }

    [Fact]
    public void Validate_WhenMusicBrainzRecordingIdIsNull_ShouldNotHaveValidationError()
    {
        // Arrange
        UpdateTrackCommand command = _updateTrackCommandFixture.Create(includeDiscNumber: false, includeScript: false, includeKey: false, includeBpm: false, includeWork: false, includeMusicBrainzRecordingId: false, includeMusicBrainzTrackId: false, includeMoods: false, includeIsrcs: false);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationError(Errors.Music.MusicBrainzIdInvalidFormat);
    }

    [Fact]
    public void Validate_WhenMusicBrainzTrackIdIsEmptyGuid_ShouldHaveValidationError()
    {
        // Arrange
        UpdateTrackCommand command = _updateTrackCommandFixture.Create(musicBrainzTrackId: Guid.Empty);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Music.MusicBrainzIdInvalidFormat);
    }

    [Fact]
    public void Validate_WhenMusicBrainzTrackIdIsValid_ShouldNotHaveValidationError()
    {
        // Arrange
        UpdateTrackCommand command = _updateTrackCommandFixture.Create(musicBrainzTrackId: Guid.NewGuid());

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationError(Errors.Music.MusicBrainzIdInvalidFormat);
    }

    [Fact]
    public void Validate_WhenMusicBrainzTrackIdIsNull_ShouldNotHaveValidationError()
    {
        // Arrange
        UpdateTrackCommand command = _updateTrackCommandFixture.Create(includeDiscNumber: false, includeScript: false, includeKey: false, includeBpm: false, includeWork: false, includeMusicBrainzRecordingId: false, includeMusicBrainzTrackId: false, includeMoods: false, includeIsrcs: false);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationError(Errors.Music.MusicBrainzIdInvalidFormat);
    }

    [Fact]
    public void Validate_WhenMusicBrainzWorkIdIsEmptyGuid_ShouldHaveValidationError()
    {
        // Arrange
        UpdateTrackCommand command = _updateTrackCommandFixture.Create(work: _musicWorkDtoFixture.Create(musicBrainzWorkId: Guid.Empty));

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Music.MusicBrainzIdInvalidFormat);
    }

    [Fact]
    public void Validate_WhenMusicBrainzWorkIdIsValid_ShouldNotHaveValidationError()
    {
        // Arrange
        UpdateTrackCommand command = _updateTrackCommandFixture.Create(work: _musicWorkDtoFixture.Create(musicBrainzWorkId: Guid.NewGuid()));

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationError(Errors.Music.MusicBrainzIdInvalidFormat);
    }

    [Fact]
    public void Validate_WhenMusicBrainzWorkIdIsNull_ShouldNotHaveValidationError()
    {
        // Arrange
        UpdateTrackCommand command = _updateTrackCommandFixture.Create(includeDiscNumber: false, includeScript: false, includeKey: false, includeBpm: false, includeWork: false, includeMusicBrainzRecordingId: false, includeMusicBrainzTrackId: false, includeMoods: false, includeIsrcs: false);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationError(Errors.Music.MusicBrainzIdInvalidFormat);
    }

    [Fact]
    public void Validate_WhenTrackNumberIsNull_ShouldHaveValidationError()
    {
        // Arrange
        UpdateTrackCommand command = _updateTrackCommandFixture.Create(includeTrackNumber: false);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Music.TrackNumberMustBeGreaterThanZero);
    }

    [Fact]
    public void Validate_WhenTrackNumberIsZero_ShouldHaveValidationError()
    {
        // Arrange
        UpdateTrackCommand command = _updateTrackCommandFixture.Create(trackNumber: 0);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Music.TrackNumberMustBeGreaterThanZero);
    }

    [Fact]
    public void Validate_WhenTrackNumberIsNegative_ShouldHaveValidationError()
    {
        // Arrange
        UpdateTrackCommand command = _updateTrackCommandFixture.Create(trackNumber: -1);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Music.TrackNumberMustBeGreaterThanZero);
    }

    [Fact]
    public void Validate_WhenTrackNumberIsPositive_ShouldNotHaveValidationError()
    {
        // Arrange
        UpdateTrackCommand command = _updateTrackCommandFixture.Create(trackNumber: 1);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationError(Errors.Music.TrackNumberMustBeGreaterThanZero);
    }

    [Fact]
    public void Validate_WhenDiscNumberIsZero_ShouldHaveValidationError()
    {
        // Arrange
        UpdateTrackCommand command = _updateTrackCommandFixture.Create(discNumber: 0);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Music.DiscNumberMustBeGreaterThanZero);
    }

    [Fact]
    public void Validate_WhenDiscNumberIsNegative_ShouldHaveValidationError()
    {
        // Arrange
        UpdateTrackCommand command = _updateTrackCommandFixture.Create(discNumber: -1);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Music.DiscNumberMustBeGreaterThanZero);
    }

    [Fact]
    public void Validate_WhenDiscNumberIsPositive_ShouldNotHaveValidationError()
    {
        // Arrange
        UpdateTrackCommand command = _updateTrackCommandFixture.Create(discNumber: 1);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationError(Errors.Music.DiscNumberMustBeGreaterThanZero);
    }

    [Fact]
    public void Validate_WhenDiscNumberIsNull_ShouldNotHaveValidationError()
    {
        // Arrange
        UpdateTrackCommand command = _updateTrackCommandFixture.Create(includeDiscNumber: false, includeScript: false, includeKey: false, includeBpm: false, includeWork: false, includeMusicBrainzRecordingId: false, includeMusicBrainzTrackId: false, includeMoods: false, includeIsrcs: false);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationError(Errors.Music.DiscNumberMustBeGreaterThanZero);
    }

    [Fact]
    public void Validate_WhenScriptExceeds50Characters_ShouldHaveValidationError()
    {
        // Arrange
        UpdateTrackCommand command = _updateTrackCommandFixture.Create(script: new Faker().Random.String2(51));

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Music.ScriptMustBeMaximum50CharactersLong);
    }

    [Fact]
    public void Validate_WhenScriptIs50Characters_ShouldNotHaveValidationError()
    {
        // Arrange
        UpdateTrackCommand command = _updateTrackCommandFixture.Create(script: new Faker().Random.String2(50));

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationError(Errors.Music.ScriptMustBeMaximum50CharactersLong);
    }

    [Fact]
    public void Validate_WhenScriptIsNull_ShouldNotHaveValidationError()
    {
        // Arrange
        UpdateTrackCommand command = _updateTrackCommandFixture.Create(includeDiscNumber: false, includeScript: false, includeKey: false, includeBpm: false, includeWork: false, includeMusicBrainzRecordingId: false, includeMusicBrainzTrackId: false, includeMoods: false, includeIsrcs: false);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationError(Errors.Music.ScriptMustBeMaximum50CharactersLong);
    }

    [Fact]
    public void Validate_WhenKeyIsInvalidEnum_ShouldHaveValidationError()
    {
        // Arrange
        UpdateTrackCommand command = _updateTrackCommandFixture.Create(key: (MusicKey)9999);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Music.UnknownMusicKey);
    }

    [Fact]
    public void Validate_WhenKeyIsValid_ShouldNotHaveValidationError()
    {
        // Arrange
        UpdateTrackCommand command = _updateTrackCommandFixture.Create(key: MusicKey.CMajor);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationError(Errors.Music.UnknownMusicKey);
    }

    [Fact]
    public void Validate_WhenKeyIsNull_ShouldNotHaveValidationError()
    {
        // Arrange
        UpdateTrackCommand command = _updateTrackCommandFixture.Create(includeDiscNumber: false, includeScript: false, includeKey: false, includeBpm: false, includeWork: false, includeMusicBrainzRecordingId: false, includeMusicBrainzTrackId: false, includeMoods: false, includeIsrcs: false);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationError(Errors.Music.UnknownMusicKey);
    }

    [Fact]
    public void Validate_WhenBpmIsZero_ShouldHaveValidationError()
    {
        // Arrange
        UpdateTrackCommand command = _updateTrackCommandFixture.Create(bpm: 0);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Music.BpmMustBeGreaterThanZero);
    }

    [Fact]
    public void Validate_WhenBpmIsNegative_ShouldHaveValidationError()
    {
        // Arrange
        UpdateTrackCommand command = _updateTrackCommandFixture.Create(bpm: -1);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Music.BpmMustBeGreaterThanZero);
    }

    [Fact]
    public void Validate_WhenBpmIsPositive_ShouldNotHaveValidationError()
    {
        // Arrange
        UpdateTrackCommand command = _updateTrackCommandFixture.Create(bpm: 120);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationError(Errors.Music.BpmMustBeGreaterThanZero);
    }

    [Fact]
    public void Validate_WhenBpmIsNull_ShouldNotHaveValidationError()
    {
        // Arrange
        UpdateTrackCommand command = _updateTrackCommandFixture.Create(includeDiscNumber: false, includeScript: false, includeKey: false, includeBpm: false, includeWork: false, includeMusicBrainzRecordingId: false, includeMusicBrainzTrackId: false, includeMoods: false, includeIsrcs: false);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationError(Errors.Music.BpmMustBeGreaterThanZero);
    }

    [Fact]
    public void Validate_WhenWorkExceeds255Characters_ShouldHaveValidationError()
    {
        // Arrange
        UpdateTrackCommand command = _updateTrackCommandFixture.Create(work: _musicWorkDtoFixture.Create(title: new Faker().Random.String2(256)));

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Music.WorkMustBeMaximum255CharactersLong);
    }

    [Fact]
    public void Validate_WhenWorkIs255Characters_ShouldNotHaveValidationError()
    {
        // Arrange
        UpdateTrackCommand command = _updateTrackCommandFixture.Create(work: _musicWorkDtoFixture.Create(title: new Faker().Random.String2(255)));

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationError(Errors.Music.WorkMustBeMaximum255CharactersLong);
    }

    [Fact]
    public void Validate_WhenWorkIsNull_ShouldNotHaveValidationError()
    {
        // Arrange
        UpdateTrackCommand command = _updateTrackCommandFixture.Create(includeDiscNumber: false, includeScript: false, includeKey: false, includeBpm: false, includeWork: false, includeMusicBrainzRecordingId: false, includeMusicBrainzTrackId: false, includeMoods: false, includeIsrcs: false);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationError(Errors.Music.WorkMustBeMaximum255CharactersLong);
    }

    [Fact]
    public void Validate_WhenMoodNameIsEmpty_ShouldHaveValidationError()
    {
        // Arrange
        UpdateTrackCommand command = _updateTrackCommandFixture.Create(moods: [_moodDtoFixture.Create(name: string.Empty)]);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Metadata.MoodNameCannotBeEmpty);
    }

    [Fact]
    public void Validate_WhenMoodsAreValid_ShouldNotHaveValidationError()
    {
        // Arrange
        UpdateTrackCommand command = _updateTrackCommandFixture.Create(moods: [_moodDtoFixture.Create(name: "dramatic")]);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationError(Errors.Metadata.MoodNameCannotBeEmpty);
    }

    [Fact]
    public void Validate_WhenIsrcValueIsEmpty_ShouldHaveValidationError()
    {
        // Arrange
        UpdateTrackCommand command = _updateTrackCommandFixture.Create(isrcs: [_isrcDtoFixture.Create(value: string.Empty)]);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Music.IsrcValueCannotBeEmpty);
    }

    [Fact]
    public void Validate_WhenIsrcValuesAreValid_ShouldNotHaveValidationError()
    {
        // Arrange
        UpdateTrackCommand command = _updateTrackCommandFixture.Create(isrcs: [_isrcDtoFixture.Create()]);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationError(Errors.Music.IsrcValueCannotBeEmpty);
    }

    [Fact]
    public void Validate_WhenContributorsIsNull_ShouldHaveValidationError()
    {
        // Arrange
        UpdateTrackCommand command = _updateTrackCommandFixture.Create(includeContributors: false);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.MediaContributor.ContributorsListCannotBeNull);
    }

    [Fact]
    public void Validate_WhenContributorIdIsEmptyGuid_ShouldHaveValidationError()
    {
        // Arrange
        UpdateTrackCommand command = _updateTrackCommandFixture.Create(contributors: [_mediaContributorReferenceDtoFixture.Create(contributorId: Guid.Empty, role: MediaContributorRole.Vocals)]);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.MediaContributor.MediaContributorIdCannotBeEmpty);
    }

    [Fact]
    public void Validate_WhenContributorRoleIsInvalid_ShouldHaveValidationError()
    {
        // Arrange
        UpdateTrackCommand command = _updateTrackCommandFixture.Create(contributors: [_mediaContributorReferenceDtoFixture.Create(role: (MediaContributorRole)9999)]);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.MediaContributor.UnknownMediaContributorRole);
    }

    [Fact]
    public void Validate_WhenContributorsAreValid_ShouldNotHaveValidationError()
    {
        // Arrange
        UpdateTrackCommand command = _updateTrackCommandFixture.Create(contributors: [_mediaContributorReferenceDtoFixture.Create(role: MediaContributorRole.Vocals)]);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationError(Errors.MediaContributor.ContributorsListCannotBeNull);
    }

    [Fact]
    public void Validate_WhenRatingsIsNull_ShouldHaveValidationError()
    {
        // Arrange
        UpdateTrackCommand command = _updateTrackCommandFixture.Create(includeRatings: false);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Metadata.RatingsListCannotBeNull);
    }

    [Fact]
    public void Validate_WhenRatingValueIsZero_ShouldHaveValidationError()
    {
        // Arrange
        UpdateTrackCommand command = _updateTrackCommandFixture.Create(ratings: [_audioRatingDtoFixture.Create(value: 0, maxValue: 5)]);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Metadata.RatingValueMustBePositive);
    }

    [Fact]
    public void Validate_WhenRatingValueIsNegative_ShouldHaveValidationError()
    {
        // Arrange
        UpdateTrackCommand command = _updateTrackCommandFixture.Create(ratings: [_audioRatingDtoFixture.Create(value: -1, maxValue: 5)]);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Metadata.RatingValueMustBePositive);
    }

    [Fact]
    public void Validate_WhenRatingValueIsGreaterThanMaxValue_ShouldHaveValidationError()
    {
        // Arrange
        UpdateTrackCommand command = _updateTrackCommandFixture.Create(ratings: [_audioRatingDtoFixture.Create(value: 6, maxValue: 5)]);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Metadata.RatingValueCannotBeGreaterThanMaxValue);
    }

    [Fact]
    public void Validate_WhenRatingMaxValueIsZero_ShouldHaveValidationError()
    {
        // Arrange
        UpdateTrackCommand command = _updateTrackCommandFixture.Create(ratings: [_audioRatingDtoFixture.Create(value: 1, maxValue: 0)]);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Metadata.RatingMaxValueMustBePositive);
    }

    [Fact]
    public void Validate_WhenRatingMaxValueIsNegative_ShouldHaveValidationError()
    {
        // Arrange
        UpdateTrackCommand command = _updateTrackCommandFixture.Create(ratings: [_audioRatingDtoFixture.Create(value: 1, maxValue: -1)]);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Metadata.RatingMaxValueMustBePositive);
    }

    [Fact]
    public void Validate_WhenRatingVoteCountIsNegative_ShouldHaveValidationError()
    {
        // Arrange
        UpdateTrackCommand command = _updateTrackCommandFixture.Create(ratings: [_audioRatingDtoFixture.Create(value: 4, maxValue: 5, voteCount: -1)]);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Metadata.RatingVoteCountMustBePositive);
    }

    [Fact]
    public void Validate_WhenRatingsAreValid_ShouldNotHaveValidationError()
    {
        // Arrange
        UpdateTrackCommand command = _updateTrackCommandFixture.Create(ratings: [_audioRatingDtoFixture.Create(value: 4, maxValue: 5, voteCount: 100)]);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationError(Errors.Metadata.RatingsListCannotBeNull);
    }

    [Fact]
    public void Validate_WhenRatingVoteCountIsNull_ShouldNotHaveValidationError()
    {
        // Arrange
        UpdateTrackCommand command = _updateTrackCommandFixture.Create(ratings: [_audioRatingDtoFixture.Create(value: 4, maxValue: 5, includeVoteCount: false)]);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationError(Errors.Metadata.RatingVoteCountMustBePositive);
    }

    [Fact]
    public void Validate_WhenCommandIsValid_ShouldNotHaveValidationError()
    {
        // Arrange
        UpdateTrackCommand command = _updateTrackCommandFixture.Create();

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }
}
