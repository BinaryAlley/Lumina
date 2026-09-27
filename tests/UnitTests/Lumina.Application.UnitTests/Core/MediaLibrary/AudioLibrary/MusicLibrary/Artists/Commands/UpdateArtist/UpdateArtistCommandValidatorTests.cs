#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Application.Core.MediaLibrary.AudioLibrary.MusicLibrary.Artists.Commands.UpdateArtist;
using Lumina.Application.Fixtures.Core.MediaLibrary.AudioLibrary.MusicLibrary.Albums.Commands.AddAlbum;
using Lumina.Application.Fixtures.Core.MediaLibrary.AudioLibrary.MusicLibrary.Artists.Commands.UpdateArtist;
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

namespace Lumina.Application.UnitTests.Core.MediaLibrary.AudioLibrary.MusicLibrary.Artists.Commands.UpdateArtist;

/// <summary>
/// Contains unit tests for the <see cref="UpdateArtistCommandValidator"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class UpdateArtistCommandValidatorTests
{
    private readonly UpdateArtistCommandFixture _updateArtistCommandFixture = new();
    private readonly UpdateArtistCommandValidator _validator = new();
    private readonly AddAlbumCommandFixture _addAlbumCommandFixture = new();
    private readonly AlbumMetadataDtoFixture _albumMetadataDtoFixture = new();
    private readonly AudioMetadataDtoFixture _audioMetadataDtoFixture = new();
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
        UpdateArtistCommand command = _updateArtistCommandFixture.Create(libraryId: libraryId, includeLibraryId: libraryId is not null);

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
        UpdateArtistCommand command = _updateArtistCommandFixture.Create(artistId: artistId, includeArtistId: artistId is not null);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Music.ArtistIdCannotBeEmpty);
    }

    [Fact]
    public void Validate_WhenNameIsNull_ShouldHaveValidationError()
    {
        // Arrange
        UpdateArtistCommand command = _updateArtistCommandFixture.Create(includeName: false);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Music.ArtistNameCannotBeEmpty);
    }

    [Theory]
    [InlineData("")] // empty name
    [InlineData("   ")] // whitespace name, which the not empty rule rejects
    public void Validate_WhenNameIsEmpty_ShouldHaveValidationError(string name)
    {
        // Arrange
        UpdateArtistCommand command = _updateArtistCommandFixture.Create(name: name);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Music.ArtistNameCannotBeEmpty);
    }

    [Fact]
    public void Validate_WhenNameExceeds255Characters_ShouldHaveValidationError()
    {
        // Arrange
        UpdateArtistCommand command = _updateArtistCommandFixture.Create(name: new Faker().Random.String2(256));

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Music.ArtistNameMustBeMaximum255CharactersLong);
    }

    [Fact]
    public void Validate_WhenNameIsValid_ShouldNotHaveValidationError()
    {
        // Arrange
        UpdateArtistCommand command = _updateArtistCommandFixture.Create(name: new Faker().Random.String2(255));

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationError(Errors.Music.ArtistNameCannotBeEmpty);
        result.ShouldNotHaveValidationError(Errors.Music.ArtistNameMustBeMaximum255CharactersLong);
    }

    [Fact]
    public void Validate_WhenWebsiteExceeds2048Characters_ShouldHaveValidationError()
    {
        // Arrange
        UpdateArtistCommand command = _updateArtistCommandFixture.Create(website: new Faker().Random.String2(2049));

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Music.ArtistWebsiteMustBeMaximum2048CharactersLong);
    }

    [Fact]
    public void Validate_WhenWebsiteIsNull_ShouldNotHaveValidationError()
    {
        // Arrange
        UpdateArtistCommand command = _updateArtistCommandFixture.Create(includeWebsite: false);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationError(Errors.Music.ArtistWebsiteMustBeMaximum2048CharactersLong);
    }

    [Fact]
    public void Validate_WhenWebsiteIsValid_ShouldNotHaveValidationError()
    {
        // Arrange
        UpdateArtistCommand command = _updateArtistCommandFixture.Create(website: new Faker().Internet.Url());

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationError(Errors.Music.ArtistWebsiteMustBeMaximum2048CharactersLong);
    }

    [Fact]
    public void Validate_WhenMusicBrainzArtistIdIsEmptyGuid_ShouldHaveValidationError()
    {
        // Arrange
        UpdateArtistCommand command = _updateArtistCommandFixture.Create(musicBrainzArtistId: Guid.Empty);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Music.MusicBrainzIdInvalidFormat);
    }

    [Fact]
    public void Validate_WhenMusicBrainzArtistIdIsNull_ShouldNotHaveValidationError()
    {
        // Arrange
        UpdateArtistCommand command = _updateArtistCommandFixture.Create(includeMusicBrainzArtistId: false);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationError(Errors.Music.MusicBrainzIdInvalidFormat);
    }

    [Fact]
    public void Validate_WhenMusicBrainzArtistIdIsValid_ShouldNotHaveValidationError()
    {
        // Arrange
        UpdateArtistCommand command = _updateArtistCommandFixture.Create(musicBrainzArtistId: Guid.NewGuid());

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationError(Errors.Music.MusicBrainzIdInvalidFormat);
    }

    [Fact]
    public void Validate_WhenContributorsIsNull_ShouldHaveValidationError()
    {
        // Arrange
        UpdateArtistCommand command = _updateArtistCommandFixture.Create(includeContributors: false);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.MediaContributor.ContributorsListCannotBeNull);
    }

    [Fact]
    public void Validate_WhenContributorIdIsEmpty_ShouldHaveValidationError()
    {
        // Arrange
        UpdateArtistCommand command = _updateArtistCommandFixture.Create(contributors: [_mediaContributorReferenceDtoFixture.Create(contributorId: Guid.Empty, role: MediaContributorRole.Vocals)]);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.MediaContributor.MediaContributorIdCannotBeEmpty);
    }

    [Fact]
    public void Validate_WhenContributorRoleIsNotDefined_ShouldHaveValidationError()
    {
        // Arrange
        UpdateArtistCommand command = _updateArtistCommandFixture.Create(contributors: [_mediaContributorReferenceDtoFixture.Create(contributorId: Guid.NewGuid(), role: (MediaContributorRole)999)]);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.MediaContributor.UnknownMediaContributorRole);
    }

    [Fact]
    public void Validate_WhenContributorsAreValid_ShouldNotHaveValidationError()
    {
        // Arrange
        UpdateArtistCommand command = _updateArtistCommandFixture.Create(contributors: [_mediaContributorReferenceDtoFixture.Create(contributorId: Guid.NewGuid(), role: MediaContributorRole.Vocals)]);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationError(Errors.MediaContributor.ContributorsListCannotBeNull);
        result.ShouldNotHaveValidationError(Errors.MediaContributor.MediaContributorIdCannotBeEmpty);
        result.ShouldNotHaveValidationError(Errors.MediaContributor.UnknownMediaContributorRole);
    }

    [Fact]
    public void Validate_WhenAlbumsIsNull_ShouldHaveValidationError()
    {
        // Arrange
        UpdateArtistCommand command = _updateArtistCommandFixture.Create(includeAlbums: false);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Music.AlbumsListCannotBeNull);
    }

    [Fact]
    public void Validate_WhenAlbumIdIsEmptyGuid_ShouldHaveValidationError()
    {
        // Arrange
        UpdateArtistCommand command = _updateArtistCommandFixture.Create(albums: [_addAlbumCommandFixture.Create(albumId: Guid.Empty)]);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Music.AlbumIdCannotBeEmpty);
    }

    [Fact]
    public void Validate_WhenAlbumIdIsNull_ShouldNotHaveValidationError()
    {
        // Arrange
        UpdateArtistCommand command = _updateArtistCommandFixture.Create(albums: [_addAlbumCommandFixture.Create(includeAlbumId: false)]);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationError(Errors.Music.AlbumIdCannotBeEmpty);
    }

    [Fact]
    public void Validate_WhenAlbumIdIsValid_ShouldNotHaveValidationError()
    {
        // Arrange
        UpdateArtistCommand command = _updateArtistCommandFixture.Create(albums: [_addAlbumCommandFixture.Create(albumId: Guid.NewGuid())]);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationError(Errors.Music.AlbumIdCannotBeEmpty);
    }

    [Fact]
    public void Validate_WhenAlbumMetadataIsNull_ShouldHaveValidationError()
    {
        // Arrange
        UpdateArtistCommand command = _updateArtistCommandFixture.Create(albums: [_addAlbumCommandFixture.Create(includeMetadata: false)]);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Metadata.MetadataCannotBeNull);
    }

    [Fact]
    public void Validate_WhenAlbumTitleIsNull_ShouldHaveValidationError()
    {
        // Arrange
        UpdateArtistCommand command = _updateArtistCommandFixture.Create(albums: [_addAlbumCommandFixture.Create(metadata: _albumMetadataDtoFixture.Create(includeTitle: false))]);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Music.AlbumTitleCannotBeEmpty);
    }

    [Theory]
    [InlineData("")] // empty title
    [InlineData("   ")] // whitespace title, which the not empty rule rejects
    public void Validate_WhenAlbumTitleIsEmpty_ShouldHaveValidationError(string title)
    {
        // Arrange
        UpdateArtistCommand command = _updateArtistCommandFixture.Create(albums: [_addAlbumCommandFixture.Create(metadata: _albumMetadataDtoFixture.Create(title: title))]);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Music.AlbumTitleCannotBeEmpty);
    }

    [Fact]
    public void Validate_WhenAlbumTitleExceeds255Characters_ShouldHaveValidationError()
    {
        // Arrange
        UpdateArtistCommand command = _updateArtistCommandFixture.Create(albums: [_addAlbumCommandFixture.Create(metadata: _albumMetadataDtoFixture.Create(title: new Faker().Random.String2(300)))]);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Music.AlbumTitleMustBeMaximum255CharactersLong);
    }

    [Fact]
    public void Validate_WhenAlbumTitleIsValid_ShouldNotHaveValidationError()
    {
        // Arrange
        UpdateArtistCommand command = _updateArtistCommandFixture.Create(albums: [_addAlbumCommandFixture.Create(metadata: _albumMetadataDtoFixture.Create(title: new Faker().Random.String2(200)))]);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationError(Errors.Music.AlbumTitleCannotBeEmpty);
        result.ShouldNotHaveValidationError(Errors.Music.AlbumTitleMustBeMaximum255CharactersLong);
    }

    [Fact]
    public void Validate_WhenAlbumOriginalTitleExceeds255Characters_ShouldHaveValidationError()
    {
        // Arrange
        UpdateArtistCommand command = _updateArtistCommandFixture.Create(albums: [_addAlbumCommandFixture.Create(metadata: _albumMetadataDtoFixture.Create(originalTitle: new Faker().Random.String2(300)))]);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Metadata.OriginalTitleMustBeMaximum255CharactersLong);
    }

    [Fact]
    public void Validate_WhenAlbumOriginalTitleIsNull_ShouldNotHaveValidationError()
    {
        // Arrange
        UpdateArtistCommand command = _updateArtistCommandFixture.Create(albums: [_addAlbumCommandFixture.Create(metadata: _albumMetadataDtoFixture.Create(includeOriginalTitle: false))]);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationError(Errors.Metadata.OriginalTitleMustBeMaximum255CharactersLong);
    }

    [Fact]
    public void Validate_WhenAlbumDescriptionExceeds2000Characters_ShouldHaveValidationError()
    {
        // Arrange
        UpdateArtistCommand command = _updateArtistCommandFixture.Create(albums: [_addAlbumCommandFixture.Create(metadata: _albumMetadataDtoFixture.Create(description: new Faker().Random.String2(2001)))]);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Metadata.DescriptionMustBeMaximum2000CharactersLong);
    }

    [Fact]
    public void Validate_WhenAlbumDescriptionIsNull_ShouldNotHaveValidationError()
    {
        // Arrange
        UpdateArtistCommand command = _updateArtistCommandFixture.Create(albums: [_addAlbumCommandFixture.Create(metadata: _albumMetadataDtoFixture.Create(includeDescription: false))]);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationError(Errors.Metadata.DescriptionMustBeMaximum2000CharactersLong);
    }

    [Fact]
    public void Validate_WhenAlbumReleaseTypeIsNotDefined_ShouldHaveValidationError()
    {
        // Arrange
        UpdateArtistCommand command = _updateArtistCommandFixture.Create(albums: [_addAlbumCommandFixture.Create(metadata: _albumMetadataDtoFixture.Create(releaseType: (MusicReleaseType)999))]);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Music.UnknownMusicReleaseType);
    }

    [Fact]
    public void Validate_WhenAlbumReleaseTypeIsNull_ShouldNotHaveValidationError()
    {
        // Arrange
        UpdateArtistCommand command = _updateArtistCommandFixture.Create(albums: [_addAlbumCommandFixture.Create(metadata: _albumMetadataDtoFixture.Create(includeReleaseType: false))]);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationError(Errors.Music.UnknownMusicReleaseType);
    }

    [Fact]
    public void Validate_WhenAlbumReleaseTypeIsDefined_ShouldNotHaveValidationError()
    {
        // Arrange
        UpdateArtistCommand command = _updateArtistCommandFixture.Create(albums: [_addAlbumCommandFixture.Create(metadata: _albumMetadataDtoFixture.Create(releaseType: MusicReleaseType.Album))]);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationError(Errors.Music.UnknownMusicReleaseType);
    }

    [Fact]
    public void Validate_WhenAlbumReleaseStatusIsNotDefined_ShouldHaveValidationError()
    {
        // Arrange
        UpdateArtistCommand command = _updateArtistCommandFixture.Create(albums: [_addAlbumCommandFixture.Create(metadata: _albumMetadataDtoFixture.Create(releaseStatus: (MusicReleaseStatus)999))]);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Music.UnknownMusicReleaseStatus);
    }

    [Fact]
    public void Validate_WhenAlbumReleaseStatusIsNull_ShouldNotHaveValidationError()
    {
        // Arrange
        UpdateArtistCommand command = _updateArtistCommandFixture.Create(albums: [_addAlbumCommandFixture.Create(metadata: _albumMetadataDtoFixture.Create(includeReleaseStatus: false))]);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationError(Errors.Music.UnknownMusicReleaseStatus);
    }

    [Fact]
    public void Validate_WhenAlbumReleaseStatusIsDefined_ShouldNotHaveValidationError()
    {
        // Arrange
        UpdateArtistCommand command = _updateArtistCommandFixture.Create(albums: [_addAlbumCommandFixture.Create(metadata: _albumMetadataDtoFixture.Create(releaseStatus: MusicReleaseStatus.Official))]);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationError(Errors.Music.UnknownMusicReleaseStatus);
    }

    [Theory]
    [InlineData(0)] // zero is not a valid total discs value
    [InlineData(-1)] // a negative total discs value is not valid
    public void Validate_WhenAlbumTotalDiscsIsNotPositive_ShouldHaveValidationError(int totalDiscs)
    {
        // Arrange
        UpdateArtistCommand command = _updateArtistCommandFixture.Create(albums: [_addAlbumCommandFixture.Create(metadata: _albumMetadataDtoFixture.Create(totalDiscs: totalDiscs))]);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Music.TotalDiscsMustBeGreaterThanZero);
    }

    [Fact]
    public void Validate_WhenAlbumTotalDiscsIsNull_ShouldNotHaveValidationError()
    {
        // Arrange
        UpdateArtistCommand command = _updateArtistCommandFixture.Create(albums: [_addAlbumCommandFixture.Create(metadata: _albumMetadataDtoFixture.Create(includeTotalDiscs: false))]);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationError(Errors.Music.TotalDiscsMustBeGreaterThanZero);
    }

    [Theory]
    [InlineData(0)] // zero is not a valid total tracks value
    [InlineData(-1)] // a negative total tracks value is not valid
    public void Validate_WhenAlbumTotalTracksIsNotPositive_ShouldHaveValidationError(int totalTracks)
    {
        // Arrange
        UpdateArtistCommand command = _updateArtistCommandFixture.Create(albums: [_addAlbumCommandFixture.Create(metadata: _albumMetadataDtoFixture.Create(totalTracks: totalTracks))]);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Music.TotalTracksMustBeGreaterThanZero);
    }

    [Fact]
    public void Validate_WhenAlbumTotalTracksIsNull_ShouldNotHaveValidationError()
    {
        // Arrange
        UpdateArtistCommand command = _updateArtistCommandFixture.Create(albums: [_addAlbumCommandFixture.Create(metadata: _albumMetadataDtoFixture.Create(includeTotalTracks: false))]);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationError(Errors.Music.TotalTracksMustBeGreaterThanZero);
    }

    [Fact]
    public void Validate_WhenAlbumReleaseInfoIsNull_ShouldHaveValidationError()
    {
        // Arrange
        UpdateArtistCommand command = _updateArtistCommandFixture.Create(albums: [_addAlbumCommandFixture.Create(metadata: _albumMetadataDtoFixture.Create(includeReleaseInfo: false))]);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Metadata.ReleaseInfoCannotBeNull);
    }

    [Fact]
    public void Validate_WhenAlbumOriginalReleaseYearIsLessThan1_ShouldHaveValidationError()
    {
        // Arrange
        UpdateArtistCommand command = _updateArtistCommandFixture.Create(albums: [_addAlbumCommandFixture.Create(metadata: _albumMetadataDtoFixture.Create(releaseInfo: _releaseInfoDtoFixture.Create(originalReleaseYear: 0)))]);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Metadata.OriginalReleaseYearMustBeBetween1And9999);
    }

    [Fact]
    public void Validate_WhenAlbumOriginalReleaseYearIsGreaterThan9999_ShouldHaveValidationError()
    {
        // Arrange
        UpdateArtistCommand command = _updateArtistCommandFixture.Create(albums: [_addAlbumCommandFixture.Create(metadata: _albumMetadataDtoFixture.Create(releaseInfo: _releaseInfoDtoFixture.Create(originalReleaseYear: 10000)))]);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Metadata.OriginalReleaseYearMustBeBetween1And9999);
    }

    [Fact]
    public void Validate_WhenAlbumReReleaseYearIsLessThan1_ShouldHaveValidationError()
    {
        // Arrange
        UpdateArtistCommand command = _updateArtistCommandFixture.Create(albums: [_addAlbumCommandFixture.Create(metadata: _albumMetadataDtoFixture.Create(releaseInfo: _releaseInfoDtoFixture.Create(reReleaseYear: 0)))]);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Metadata.ReReleaseYearMustBeBetween1And9999);
    }

    [Fact]
    public void Validate_WhenAlbumReReleaseYearIsGreaterThan9999_ShouldHaveValidationError()
    {
        // Arrange
        UpdateArtistCommand command = _updateArtistCommandFixture.Create(albums: [_addAlbumCommandFixture.Create(metadata: _albumMetadataDtoFixture.Create(releaseInfo: _releaseInfoDtoFixture.Create(reReleaseYear: 10000)))]);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Metadata.ReReleaseYearMustBeBetween1And9999);
    }

    [Fact]
    public void Validate_WhenAlbumReleaseVersionExceeds50Characters_ShouldHaveValidationError()
    {
        // Arrange
        UpdateArtistCommand command = _updateArtistCommandFixture.Create(albums: [_addAlbumCommandFixture.Create(metadata: _albumMetadataDtoFixture.Create(releaseInfo: _releaseInfoDtoFixture.Create(releaseVersion: new Faker().Random.String2(51))))]);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Metadata.ReleaseVersionMustBeMaximum50CharactersLong);
    }

    [Fact]
    public void Validate_WhenAlbumOriginalReleaseDateAndYearDoNotMatch_ShouldHaveValidationError()
    {
        // Arrange
        UpdateArtistCommand command = _updateArtistCommandFixture.Create(albums: [_addAlbumCommandFixture.Create(metadata: _albumMetadataDtoFixture.Create(releaseInfo: _releaseInfoDtoFixture.Create(originalReleaseDate: new DateOnly(2020, 1, 1), originalReleaseYear: 2019, includeReReleaseDate: false)))]);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Metadata.OriginalReleaseDateAndYearMustMatch);
    }

    [Fact]
    public void Validate_WhenAlbumOriginalReleaseDateAndYearMatch_ShouldNotHaveValidationError()
    {
        // Arrange
        UpdateArtistCommand command = _updateArtistCommandFixture.Create(albums: [_addAlbumCommandFixture.Create(metadata: _albumMetadataDtoFixture.Create(releaseInfo: _releaseInfoDtoFixture.Create(originalReleaseDate: new DateOnly(2020, 1, 1), originalReleaseYear: 2020, includeReReleaseDate: false)))]);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationError(Errors.Metadata.OriginalReleaseDateAndYearMustMatch);
    }

    [Fact]
    public void Validate_WhenAlbumReReleaseDateAndYearDoNotMatch_ShouldHaveValidationError()
    {
        // Arrange
        UpdateArtistCommand command = _updateArtistCommandFixture.Create(albums: [_addAlbumCommandFixture.Create(metadata: _albumMetadataDtoFixture.Create(releaseInfo: _releaseInfoDtoFixture.Create(reReleaseDate: new DateOnly(2021, 1, 1), reReleaseYear: 2020)))]);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Metadata.ReReleaseDateAndYearMustMatch);
    }

    [Fact]
    public void Validate_WhenAlbumReReleaseDateAndYearMatch_ShouldNotHaveValidationError()
    {
        // Arrange
        UpdateArtistCommand command = _updateArtistCommandFixture.Create(albums: [_addAlbumCommandFixture.Create(metadata: _albumMetadataDtoFixture.Create(releaseInfo: _releaseInfoDtoFixture.Create(reReleaseDate: new DateOnly(2021, 1, 1), reReleaseYear: 2021)))]);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationError(Errors.Metadata.ReReleaseDateAndYearMustMatch);
    }

    [Fact]
    public void Validate_WhenAlbumReReleaseYearIsBeforeOriginalReleaseYear_ShouldHaveValidationError()
    {
        // Arrange
        UpdateArtistCommand command = _updateArtistCommandFixture.Create(albums: [_addAlbumCommandFixture.Create(metadata: _albumMetadataDtoFixture.Create(releaseInfo: _releaseInfoDtoFixture.Create(originalReleaseYear: 2001, reReleaseYear: 2000, includeReReleaseDate: false)))]);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Metadata.ReReleaseYearCannotBeEarlierThanOriginalReleaseYear);
    }

    [Fact]
    public void Validate_WhenAlbumReReleaseYearIsAfterOriginalReleaseYear_ShouldNotHaveValidationError()
    {
        // Arrange
        UpdateArtistCommand command = _updateArtistCommandFixture.Create(albums: [_addAlbumCommandFixture.Create(metadata: _albumMetadataDtoFixture.Create(releaseInfo: _releaseInfoDtoFixture.Create(originalReleaseYear: 2000, reReleaseYear: 2001, includeReReleaseDate: false)))]);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationError(Errors.Metadata.ReReleaseYearCannotBeEarlierThanOriginalReleaseYear);
    }

    [Fact]
    public void Validate_WhenAlbumReReleaseDateIsBeforeOriginalReleaseDate_ShouldHaveValidationError()
    {
        // Arrange
        UpdateArtistCommand command = _updateArtistCommandFixture.Create(albums: [_addAlbumCommandFixture.Create(metadata: _albumMetadataDtoFixture.Create(releaseInfo: _releaseInfoDtoFixture.Create(originalReleaseDate: new DateOnly(2001, 1, 1), reReleaseDate: new DateOnly(2000, 1, 1))))]);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Metadata.ReReleaseDateCannotBeEarlierThanOriginalReleaseDate);
    }

    [Fact]
    public void Validate_WhenAlbumReReleaseDateIsAfterOriginalReleaseDate_ShouldNotHaveValidationError()
    {
        // Arrange
        UpdateArtistCommand command = _updateArtistCommandFixture.Create(albums: [_addAlbumCommandFixture.Create(metadata: _albumMetadataDtoFixture.Create(releaseInfo: _releaseInfoDtoFixture.Create(originalReleaseDate: new DateOnly(2000, 1, 1), reReleaseDate: new DateOnly(2001, 1, 1))))]);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationError(Errors.Metadata.ReReleaseDateCannotBeEarlierThanOriginalReleaseDate);
    }

    [Fact]
    public void Validate_WhenAlbumGenresIsNull_ShouldHaveValidationError()
    {
        // Arrange
        UpdateArtistCommand command = _updateArtistCommandFixture.Create(albums: [_addAlbumCommandFixture.Create(metadata: _albumMetadataDtoFixture.Create(includeGenres: false))]);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Metadata.GenresListCannotBeNull);
    }

    [Fact]
    public void Validate_WhenAlbumGenreNameIsEmpty_ShouldHaveValidationError()
    {
        // Arrange
        UpdateArtistCommand command = _updateArtistCommandFixture.Create(albums: [_addAlbumCommandFixture.Create(metadata: _albumMetadataDtoFixture.Create(genres: [_genreDtoFixture.Create(name: string.Empty)]))]);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Metadata.GenreNameCannotBeEmpty);
    }

    [Fact]
    public void Validate_WhenAlbumGenreNameExceeds50Characters_ShouldHaveValidationError()
    {
        // Arrange
        UpdateArtistCommand command = _updateArtistCommandFixture.Create(albums: [_addAlbumCommandFixture.Create(metadata: _albumMetadataDtoFixture.Create(genres: [_genreDtoFixture.Create(name: new Faker().Random.String2(51))]))]);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Metadata.GenreNameMustBeMaximum50CharactersLong);
    }

    [Fact]
    public void Validate_WhenAlbumGenresAreValid_ShouldNotHaveValidationError()
    {
        // Arrange
        UpdateArtistCommand command = _updateArtistCommandFixture.Create(albums: [_addAlbumCommandFixture.Create(metadata: _albumMetadataDtoFixture.Create(genres: [_genreDtoFixture.Create(name: new Faker().Random.String2(50))]))]);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationError(Errors.Metadata.GenresListCannotBeNull);
        result.ShouldNotHaveValidationError(Errors.Metadata.GenreNameCannotBeEmpty);
        result.ShouldNotHaveValidationError(Errors.Metadata.GenreNameMustBeMaximum50CharactersLong);
    }

    [Fact]
    public void Validate_WhenAlbumTagsIsNull_ShouldHaveValidationError()
    {
        // Arrange
        UpdateArtistCommand command = _updateArtistCommandFixture.Create(albums: [_addAlbumCommandFixture.Create(metadata: _albumMetadataDtoFixture.Create(includeTags: false))]);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Metadata.TagsListCannotBeNull);
    }

    [Fact]
    public void Validate_WhenAlbumTagNameIsEmpty_ShouldHaveValidationError()
    {
        // Arrange
        UpdateArtistCommand command = _updateArtistCommandFixture.Create(albums: [_addAlbumCommandFixture.Create(metadata: _albumMetadataDtoFixture.Create(tags: [_tagDtoFixture.Create(name: string.Empty)]))]);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Metadata.TagNameCannotBeEmpty);
    }

    [Fact]
    public void Validate_WhenAlbumTagNameExceeds50Characters_ShouldHaveValidationError()
    {
        // Arrange
        UpdateArtistCommand command = _updateArtistCommandFixture.Create(albums: [_addAlbumCommandFixture.Create(metadata: _albumMetadataDtoFixture.Create(tags: [_tagDtoFixture.Create(name: new Faker().Random.String2(51))]))]);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Metadata.TagNameMustBeMaximum50CharactersLong);
    }

    [Fact]
    public void Validate_WhenAlbumTagsAreValid_ShouldNotHaveValidationError()
    {
        // Arrange
        UpdateArtistCommand command = _updateArtistCommandFixture.Create(albums: [_addAlbumCommandFixture.Create(metadata: _albumMetadataDtoFixture.Create(tags: [_tagDtoFixture.Create(name: new Faker().Random.String2(50))]))]);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationError(Errors.Metadata.TagsListCannotBeNull);
        result.ShouldNotHaveValidationError(Errors.Metadata.TagNameCannotBeEmpty);
        result.ShouldNotHaveValidationError(Errors.Metadata.TagNameMustBeMaximum50CharactersLong);
    }

    [Fact]
    public void Validate_WhenAlbumLanguageIsNull_ShouldNotHaveValidationError()
    {
        // Arrange
        UpdateArtistCommand command = _updateArtistCommandFixture.Create(albums: [_addAlbumCommandFixture.Create(metadata: _albumMetadataDtoFixture.Create(includeLanguage: false))]);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationError(Errors.Metadata.LanguageCodeCannotBeEmpty);
    }

    [Fact]
    public void Validate_WhenAlbumLanguageCodeIsEmpty_ShouldHaveValidationError()
    {
        // Arrange
        UpdateArtistCommand command = _updateArtistCommandFixture.Create(albums: [_addAlbumCommandFixture.Create(metadata: _albumMetadataDtoFixture.Create(language: _languageInfoDtoFixture.Create(languageCode: string.Empty)))]);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Metadata.LanguageCodeCannotBeEmpty);
    }

    [Fact]
    public void Validate_WhenAlbumLanguageCodeIsNot2CharactersLong_ShouldHaveValidationError()
    {
        // Arrange
        UpdateArtistCommand command = _updateArtistCommandFixture.Create(albums: [_addAlbumCommandFixture.Create(metadata: _albumMetadataDtoFixture.Create(language: _languageInfoDtoFixture.Create(languageCode: new Faker().Random.String2(3))))]);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Metadata.LanguageCodeMustBe2CharactersLong);
    }

    [Fact]
    public void Validate_WhenAlbumLanguageCodeIsValid_ShouldNotHaveValidationError()
    {
        // Arrange
        UpdateArtistCommand command = _updateArtistCommandFixture.Create(albums: [_addAlbumCommandFixture.Create(metadata: _albumMetadataDtoFixture.Create(language: _languageInfoDtoFixture.Create(languageCode: new Faker().Random.String2(2))))]);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationError(Errors.Metadata.LanguageCodeMustBe2CharactersLong);
    }

    [Fact]
    public void Validate_WhenAlbumLanguageNameIsEmpty_ShouldHaveValidationError()
    {
        // Arrange
        UpdateArtistCommand command = _updateArtistCommandFixture.Create(albums: [_addAlbumCommandFixture.Create(metadata: _albumMetadataDtoFixture.Create(language: _languageInfoDtoFixture.Create(languageName: string.Empty)))]);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Metadata.LanguageNameCannotBeEmpty);
    }

    [Fact]
    public void Validate_WhenAlbumLanguageNameExceeds50Characters_ShouldHaveValidationError()
    {
        // Arrange
        UpdateArtistCommand command = _updateArtistCommandFixture.Create(albums: [_addAlbumCommandFixture.Create(metadata: _albumMetadataDtoFixture.Create(language: _languageInfoDtoFixture.Create(languageName: new Faker().Random.String2(51))))]);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Metadata.LanguageNameMustBeMaximum50CharactersLong);
    }

    [Fact]
    public void Validate_WhenAlbumLanguageNativeNameIsNull_ShouldNotHaveValidationError()
    {
        // Arrange
        UpdateArtistCommand command = _updateArtistCommandFixture.Create(albums: [_addAlbumCommandFixture.Create(metadata: _albumMetadataDtoFixture.Create(language: _languageInfoDtoFixture.Create(includeNativeName: false)))]);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationError(Errors.Metadata.LanguageNativeNameMustBeMaximum50CharactersLong);
    }

    [Fact]
    public void Validate_WhenAlbumLanguageNativeNameExceeds50Characters_ShouldHaveValidationError()
    {
        // Arrange
        UpdateArtistCommand command = _updateArtistCommandFixture.Create(albums: [_addAlbumCommandFixture.Create(metadata: _albumMetadataDtoFixture.Create(language: _languageInfoDtoFixture.Create(nativeName: new Faker().Random.String2(51))))]);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Metadata.LanguageNativeNameMustBeMaximum50CharactersLong);
    }

    [Fact]
    public void Validate_WhenAlbumOriginalLanguageIsNull_ShouldNotHaveValidationError()
    {
        // Arrange
        UpdateArtistCommand command = _updateArtistCommandFixture.Create(albums: [_addAlbumCommandFixture.Create(metadata: _albumMetadataDtoFixture.Create(includeOriginalLanguage: false))]);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationError(Errors.Metadata.LanguageCodeCannotBeEmpty);
    }

    [Fact]
    public void Validate_WhenAlbumOriginalLanguageCodeIsEmpty_ShouldHaveValidationError()
    {
        // Arrange
        UpdateArtistCommand command = _updateArtistCommandFixture.Create(albums: [_addAlbumCommandFixture.Create(metadata: _albumMetadataDtoFixture.Create(originalLanguage: _languageInfoDtoFixture.Create(languageCode: string.Empty)))]);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Metadata.LanguageCodeCannotBeEmpty);
    }

    [Fact]
    public void Validate_WhenAlbumOriginalLanguageCodeIsNot2CharactersLong_ShouldHaveValidationError()
    {
        // Arrange
        UpdateArtistCommand command = _updateArtistCommandFixture.Create(albums: [_addAlbumCommandFixture.Create(metadata: _albumMetadataDtoFixture.Create(originalLanguage: _languageInfoDtoFixture.Create(languageCode: new Faker().Random.String2(3))))]);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Metadata.LanguageCodeMustBe2CharactersLong);
    }

    [Fact]
    public void Validate_WhenAlbumOriginalLanguageNameIsEmpty_ShouldHaveValidationError()
    {
        // Arrange
        UpdateArtistCommand command = _updateArtistCommandFixture.Create(albums: [_addAlbumCommandFixture.Create(metadata: _albumMetadataDtoFixture.Create(originalLanguage: _languageInfoDtoFixture.Create(languageName: string.Empty)))]);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Metadata.LanguageNameCannotBeEmpty);
    }

    [Fact]
    public void Validate_WhenAlbumOriginalLanguageNameExceeds50Characters_ShouldHaveValidationError()
    {
        // Arrange
        UpdateArtistCommand command = _updateArtistCommandFixture.Create(albums: [_addAlbumCommandFixture.Create(metadata: _albumMetadataDtoFixture.Create(originalLanguage: _languageInfoDtoFixture.Create(languageName: new Faker().Random.String2(51))))]);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Metadata.LanguageNameMustBeMaximum50CharactersLong);
    }

    [Fact]
    public void Validate_WhenAlbumOriginalLanguageNativeNameIsNull_ShouldNotHaveValidationError()
    {
        // Arrange
        UpdateArtistCommand command = _updateArtistCommandFixture.Create(albums: [_addAlbumCommandFixture.Create(metadata: _albumMetadataDtoFixture.Create(originalLanguage: _languageInfoDtoFixture.Create(includeNativeName: false)))]);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationError(Errors.Metadata.LanguageNativeNameMustBeMaximum50CharactersLong);
    }

    [Fact]
    public void Validate_WhenAlbumOriginalLanguageNativeNameExceeds50Characters_ShouldHaveValidationError()
    {
        // Arrange
        UpdateArtistCommand command = _updateArtistCommandFixture.Create(albums: [_addAlbumCommandFixture.Create(metadata: _albumMetadataDtoFixture.Create(originalLanguage: _languageInfoDtoFixture.Create(nativeName: new Faker().Random.String2(51))))]);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Metadata.LanguageNativeNameMustBeMaximum50CharactersLong);
    }

    [Fact]
    public void Validate_WhenAlbumMediaFormatIsNotDefined_ShouldHaveValidationError()
    {
        // Arrange
        UpdateArtistCommand command = _updateArtistCommandFixture.Create(albums: [_addAlbumCommandFixture.Create(mediaFormat: (MusicMediaFormat)999)]);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Music.UnknownMusicMediaFormat);
    }

    [Fact]
    public void Validate_WhenAlbumMediaFormatIsNull_ShouldNotHaveValidationError()
    {
        // Arrange
        UpdateArtistCommand command = _updateArtistCommandFixture.Create(albums: [_addAlbumCommandFixture.Create(includeMediaFormat: false, includeBarcode: false, includeCatalogNumber: false, includeMusicBrainzReleaseId: false, includeMusicBrainzReleaseGroupId: false, includeMusicBrainzReleaseArtistId: false)]);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationError(Errors.Music.UnknownMusicMediaFormat);
    }

    [Fact]
    public void Validate_WhenAlbumMediaFormatIsDefined_ShouldNotHaveValidationError()
    {
        // Arrange
        UpdateArtistCommand command = _updateArtistCommandFixture.Create(albums: [_addAlbumCommandFixture.Create(mediaFormat: MusicMediaFormat.CD)]);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationError(Errors.Music.UnknownMusicMediaFormat);
    }

    [Fact]
    public void Validate_WhenAlbumCatalogNumberExceeds50Characters_ShouldHaveValidationError()
    {
        // Arrange
        UpdateArtistCommand command = _updateArtistCommandFixture.Create(albums: [_addAlbumCommandFixture.Create(catalogNumber: new Faker().Random.String2(51))]);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Music.CatalogNumberMustBeMaximum50CharactersLong);
    }

    [Fact]
    public void Validate_WhenAlbumCatalogNumberIsNull_ShouldNotHaveValidationError()
    {
        // Arrange
        UpdateArtistCommand command = _updateArtistCommandFixture.Create(albums: [_addAlbumCommandFixture.Create(includeMediaFormat: false, includeBarcode: false, includeCatalogNumber: false, includeMusicBrainzReleaseId: false, includeMusicBrainzReleaseGroupId: false, includeMusicBrainzReleaseArtistId: false)]);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationError(Errors.Music.CatalogNumberMustBeMaximum50CharactersLong);
    }

    [Fact]
    public void Validate_WhenAlbumBarcodeIsEmpty_ShouldHaveValidationError()
    {
        // Arrange
        UpdateArtistCommand command = _updateArtistCommandFixture.Create(albums: [_addAlbumCommandFixture.Create(barcode: string.Empty)]);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Music.BarcodeValueCannotBeEmpty);
    }

    [Theory]
    [InlineData("123")] // too short to be a barcode
    [InlineData("abcdefghijkl")] // not numeric
    [InlineData("12345678901234")] // too long to be a barcode
    public void Validate_WhenAlbumBarcodeHasInvalidFormat_ShouldHaveValidationError(string barcode)
    {
        // Arrange
        UpdateArtistCommand command = _updateArtistCommandFixture.Create(albums: [_addAlbumCommandFixture.Create(barcode: barcode)]);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Music.InvalidFormatForBarcode);
    }

    [Theory]
    [InlineData("123456789012")] // 12 digit barcode
    [InlineData("1234567890123")] // 13 digit barcode
    public void Validate_WhenAlbumBarcodeIsValid_ShouldNotHaveValidationError(string barcode)
    {
        // Arrange
        UpdateArtistCommand command = _updateArtistCommandFixture.Create(albums: [_addAlbumCommandFixture.Create(barcode: barcode)]);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationError(Errors.Music.BarcodeValueCannotBeEmpty);
        result.ShouldNotHaveValidationError(Errors.Music.InvalidFormatForBarcode);
    }

    [Fact]
    public void Validate_WhenAlbumBarcodeIsNull_ShouldNotHaveValidationError()
    {
        // Arrange
        UpdateArtistCommand command = _updateArtistCommandFixture.Create(albums: [_addAlbumCommandFixture.Create(includeMediaFormat: false, includeBarcode: false, includeCatalogNumber: false, includeMusicBrainzReleaseId: false, includeMusicBrainzReleaseGroupId: false, includeMusicBrainzReleaseArtistId: false)]);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationError(Errors.Music.BarcodeValueCannotBeEmpty);
    }

    [Fact]
    public void Validate_WhenAlbumMusicBrainzReleaseIdIsEmptyGuid_ShouldHaveValidationError()
    {
        // Arrange
        UpdateArtistCommand command = _updateArtistCommandFixture.Create(albums: [_addAlbumCommandFixture.Create(musicBrainzReleaseId: Guid.Empty)]);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Music.MusicBrainzIdInvalidFormat);
    }

    [Fact]
    public void Validate_WhenAlbumMusicBrainzReleaseGroupIdIsEmptyGuid_ShouldHaveValidationError()
    {
        // Arrange
        UpdateArtistCommand command = _updateArtistCommandFixture.Create(albums: [_addAlbumCommandFixture.Create(musicBrainzReleaseGroupId: Guid.Empty)]);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Music.MusicBrainzIdInvalidFormat);
    }

    [Fact]
    public void Validate_WhenAlbumMusicBrainzReleaseArtistIdIsEmptyGuid_ShouldHaveValidationError()
    {
        // Arrange
        UpdateArtistCommand command = _updateArtistCommandFixture.Create(albums: [_addAlbumCommandFixture.Create(musicBrainzReleaseArtistId: Guid.Empty)]);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Music.MusicBrainzIdInvalidFormat);
    }

    [Fact]
    public void Validate_WhenAlbumMusicBrainzIdsAreNull_ShouldNotHaveValidationError()
    {
        // Arrange
        UpdateArtistCommand command = _updateArtistCommandFixture.Create(albums: [_addAlbumCommandFixture.Create(includeMediaFormat: false, includeBarcode: false, includeCatalogNumber: false, includeMusicBrainzReleaseId: false, includeMusicBrainzReleaseGroupId: false, includeMusicBrainzReleaseArtistId: false)]);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationError(Errors.Music.MusicBrainzIdInvalidFormat);
    }

    [Fact]
    public void Validate_WhenAlbumMusicBrainzIdsAreValid_ShouldNotHaveValidationError()
    {
        // Arrange
        UpdateArtistCommand command = _updateArtistCommandFixture.Create(albums: [_addAlbumCommandFixture.Create(musicBrainzReleaseId: Guid.NewGuid(), musicBrainzReleaseGroupId: Guid.NewGuid(), musicBrainzReleaseArtistId: Guid.NewGuid())]);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationError(Errors.Music.MusicBrainzIdInvalidFormat);
    }

    [Fact]
    public void Validate_WhenAlbumContributorsIsNull_ShouldHaveValidationError()
    {
        // Arrange
        UpdateArtistCommand command = _updateArtistCommandFixture.Create(albums: [_addAlbumCommandFixture.Create(includeContributors: false)]);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.MediaContributor.ContributorsListCannotBeNull);
    }

    [Fact]
    public void Validate_WhenAlbumContributorIdIsEmpty_ShouldHaveValidationError()
    {
        // Arrange
        UpdateArtistCommand command = _updateArtistCommandFixture.Create(albums: [_addAlbumCommandFixture.Create(contributors: [_mediaContributorReferenceDtoFixture.Create(contributorId: Guid.Empty, role: MediaContributorRole.Vocals)])]);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.MediaContributor.MediaContributorIdCannotBeEmpty);
    }

    [Fact]
    public void Validate_WhenAlbumContributorRoleIsNotDefined_ShouldHaveValidationError()
    {
        // Arrange
        UpdateArtistCommand command = _updateArtistCommandFixture.Create(albums: [_addAlbumCommandFixture.Create(contributors: [_mediaContributorReferenceDtoFixture.Create(contributorId: Guid.NewGuid(), role: (MediaContributorRole)999)])]);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.MediaContributor.UnknownMediaContributorRole);
    }

    [Fact]
    public void Validate_WhenAlbumContributorsAreValid_ShouldNotHaveValidationError()
    {
        // Arrange
        UpdateArtistCommand command = _updateArtistCommandFixture.Create(albums: [_addAlbumCommandFixture.Create(contributors: [_mediaContributorReferenceDtoFixture.Create(contributorId: Guid.NewGuid(), role: MediaContributorRole.Vocals)])]);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationError(Errors.MediaContributor.ContributorsListCannotBeNull);
        result.ShouldNotHaveValidationError(Errors.MediaContributor.MediaContributorIdCannotBeEmpty);
        result.ShouldNotHaveValidationError(Errors.MediaContributor.UnknownMediaContributorRole);
    }

    [Fact]
    public void Validate_WhenAlbumRatingsIsNull_ShouldHaveValidationError()
    {
        // Arrange
        UpdateArtistCommand command = _updateArtistCommandFixture.Create(albums: [_addAlbumCommandFixture.Create(includeRatings: false)]);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Metadata.RatingsListCannotBeNull);
    }

    [Theory]
    [InlineData(0)] // zero is not a valid rating value
    [InlineData(-1)] // a negative rating value is not valid
    public void Validate_WhenAlbumRatingValueIsNotPositive_ShouldHaveValidationError(decimal value)
    {
        // Arrange
        UpdateArtistCommand command = _updateArtistCommandFixture.Create(albums: [_addAlbumCommandFixture.Create(ratings: [_audioRatingDtoFixture.Create(value: value, maxValue: 5)])]);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Metadata.RatingValueMustBePositive);
    }

    [Fact]
    public void Validate_WhenAlbumRatingValueIsNull_ShouldNotHaveValidationError()
    {
        // Arrange
        UpdateArtistCommand command = _updateArtistCommandFixture.Create(albums: [_addAlbumCommandFixture.Create(ratings: [_audioRatingDtoFixture.Create(includeValue: false, maxValue: 5)])]);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationError(Errors.Metadata.RatingValueMustBePositive);
    }

    [Fact]
    public void Validate_WhenAlbumRatingValueIsGreaterThanMaxValue_ShouldHaveValidationError()
    {
        // Arrange
        UpdateArtistCommand command = _updateArtistCommandFixture.Create(albums: [_addAlbumCommandFixture.Create(ratings: [_audioRatingDtoFixture.Create(value: 6, maxValue: 5)])]);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Metadata.RatingValueCannotBeGreaterThanMaxValue);
    }

    [Theory]
    [InlineData(0)] // zero is not a valid maximum rating value
    [InlineData(-1)] // a negative maximum rating value is not valid
    public void Validate_WhenAlbumRatingMaxValueIsNotPositive_ShouldHaveValidationError(decimal maxValue)
    {
        // Arrange
        UpdateArtistCommand command = _updateArtistCommandFixture.Create(albums: [_addAlbumCommandFixture.Create(ratings: [_audioRatingDtoFixture.Create(value: 1, maxValue: maxValue)])]);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Metadata.RatingMaxValueMustBePositive);
    }

    [Fact]
    public void Validate_WhenAlbumRatingMaxValueIsNull_ShouldNotHaveValidationError()
    {
        // Arrange
        UpdateArtistCommand command = _updateArtistCommandFixture.Create(albums: [_addAlbumCommandFixture.Create(ratings: [_audioRatingDtoFixture.Create(value: 1, includeMaxValue: false)])]);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationError(Errors.Metadata.RatingMaxValueMustBePositive);
    }

    [Fact]
    public void Validate_WhenAlbumRatingVoteCountIsNegative_ShouldHaveValidationError()
    {
        // Arrange
        UpdateArtistCommand command = _updateArtistCommandFixture.Create(albums: [_addAlbumCommandFixture.Create(ratings: [_audioRatingDtoFixture.Create(value: 4, maxValue: 5, voteCount: -1)])]);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Metadata.RatingVoteCountMustBePositive);
    }

    [Fact]
    public void Validate_WhenAlbumRatingVoteCountIsNull_ShouldNotHaveValidationError()
    {
        // Arrange
        UpdateArtistCommand command = _updateArtistCommandFixture.Create(albums: [_addAlbumCommandFixture.Create(ratings: [_audioRatingDtoFixture.Create(value: 4, maxValue: 5, includeVoteCount: false)])]);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationError(Errors.Metadata.RatingVoteCountMustBePositive);
    }

    [Fact]
    public void Validate_WhenAlbumRatingsAreValid_ShouldNotHaveValidationError()
    {
        // Arrange
        UpdateArtistCommand command = _updateArtistCommandFixture.Create(albums: [_addAlbumCommandFixture.Create(ratings: [_audioRatingDtoFixture.Create(value: 4, maxValue: 5, voteCount: 100)])]);

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
    public void Validate_WhenAlbumTracksIsNull_ShouldHaveValidationError()
    {
        // Arrange
        UpdateArtistCommand command = _updateArtistCommandFixture.Create(albums: [_addAlbumCommandFixture.Create(includeTracks: false)]);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Music.TracksListCannotBeNull);
    }

    [Fact]
    public void Validate_WhenTrackIdIsEmptyGuid_ShouldHaveValidationError()
    {
        // Arrange
        UpdateArtistCommand command = _updateArtistCommandFixture.Create(albums: [_addAlbumCommandFixture.Create(tracks: [_addTrackCommandFixture.Create(trackId: Guid.Empty)])]);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Music.TrackIdCannotBeEmpty);
    }

    [Fact]
    public void Validate_WhenTrackIdIsNull_ShouldNotHaveValidationError()
    {
        // Arrange
        UpdateArtistCommand command = _updateArtistCommandFixture.Create(albums: [_addAlbumCommandFixture.Create(tracks: [_addTrackCommandFixture.Create()])]);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationError(Errors.Music.TrackIdCannotBeEmpty);
    }

    [Fact]
    public void Validate_WhenTrackIdIsValid_ShouldNotHaveValidationError()
    {
        // Arrange
        UpdateArtistCommand command = _updateArtistCommandFixture.Create(albums: [_addAlbumCommandFixture.Create(tracks: [_addTrackCommandFixture.Create(trackId: Guid.NewGuid())])]);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationError(Errors.Music.TrackIdCannotBeEmpty);
    }

    [Fact]
    public void Validate_WhenTrackPathIsNull_ShouldHaveValidationError()
    {
        // Arrange
        UpdateArtistCommand command = _updateArtistCommandFixture.Create(albums: [_addAlbumCommandFixture.Create(tracks: [_addTrackCommandFixture.Create(includePath: false)])]);

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
        UpdateArtistCommand command = _updateArtistCommandFixture.Create(albums: [_addAlbumCommandFixture.Create(tracks: [_addTrackCommandFixture.Create(path: path)])]);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Music.TrackPathCannotBeEmpty);
    }

    [Fact]
    public void Validate_WhenTrackPathExceeds2048Characters_ShouldHaveValidationError()
    {
        // Arrange
        UpdateArtistCommand command = _updateArtistCommandFixture.Create(albums: [_addAlbumCommandFixture.Create(tracks: [_addTrackCommandFixture.Create(path: "/" + new Faker().Random.String2(2048) + ".flac")])]);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Music.TrackPathMustBeMaximum2048CharactersLong);
    }

    [Fact]
    public void Validate_WhenTrackPathIsValid_ShouldNotHaveValidationError()
    {
        // Arrange
        UpdateArtistCommand command = _updateArtistCommandFixture.Create(albums: [_addAlbumCommandFixture.Create(tracks: [_addTrackCommandFixture.Create(path: "/" + new Faker().Random.String2(2000) + ".flac")])]);

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
        UpdateArtistCommand command = _updateArtistCommandFixture.Create(albums: [_addAlbumCommandFixture.Create(tracks: [_addTrackCommandFixture.Create(includeMetadata: false)])]);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Metadata.MetadataCannotBeNull);
    }

    [Fact]
    public void Validate_WhenTrackTitleIsNull_ShouldHaveValidationError()
    {
        // Arrange
        UpdateArtistCommand command = _updateArtistCommandFixture.Create(albums: [_addAlbumCommandFixture.Create(tracks: [_addTrackCommandFixture.Create(metadata: _audioMetadataDtoFixture.Create(includeTitle: false))])]);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Metadata.TitleCannotBeEmpty);
    }

    [Fact]
    public void Validate_WhenTrackTitleExceeds255Characters_ShouldHaveValidationError()
    {
        // Arrange
        UpdateArtistCommand command = _updateArtistCommandFixture.Create(albums: [_addAlbumCommandFixture.Create(tracks: [_addTrackCommandFixture.Create(metadata: _audioMetadataDtoFixture.Create(title: new Faker().Random.String2(300)))])]);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Metadata.TitleMustBeMaximum255CharactersLong);
    }

    [Fact]
    public void Validate_WhenTrackOriginalTitleExceeds255Characters_ShouldHaveValidationError()
    {
        // Arrange
        UpdateArtistCommand command = _updateArtistCommandFixture.Create(albums: [_addAlbumCommandFixture.Create(tracks: [_addTrackCommandFixture.Create(metadata: _audioMetadataDtoFixture.Create(originalTitle: new Faker().Random.String2(300)))])]);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Metadata.OriginalTitleMustBeMaximum255CharactersLong);
    }

    [Fact]
    public void Validate_WhenTrackDescriptionExceeds2000Characters_ShouldHaveValidationError()
    {
        // Arrange
        UpdateArtistCommand command = _updateArtistCommandFixture.Create(albums: [_addAlbumCommandFixture.Create(tracks: [_addTrackCommandFixture.Create(metadata: _audioMetadataDtoFixture.Create(description: new Faker().Random.String2(2001)))])]);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Metadata.DescriptionMustBeMaximum2000CharactersLong);
    }

    [Fact]
    public void Validate_WhenTrackReleaseInfoIsNull_ShouldHaveValidationError()
    {
        // Arrange
        UpdateArtistCommand command = _updateArtistCommandFixture.Create(albums: [_addAlbumCommandFixture.Create(tracks: [_addTrackCommandFixture.Create(metadata: _audioMetadataDtoFixture.Create(includeReleaseInfo: false))])]);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Metadata.ReleaseInfoCannotBeNull);
    }

    [Fact]
    public void Validate_WhenTrackOriginalReleaseYearIsLessThan1_ShouldHaveValidationError()
    {
        // Arrange
        UpdateArtistCommand command = _updateArtistCommandFixture.Create(albums: [_addAlbumCommandFixture.Create(tracks: [_addTrackCommandFixture.Create(metadata: _audioMetadataDtoFixture.Create(releaseInfo: _releaseInfoDtoFixture.Create(originalReleaseYear: 0)))])]);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Metadata.OriginalReleaseYearMustBeBetween1And9999);
    }

    [Fact]
    public void Validate_WhenTrackOriginalReleaseYearIsGreaterThan9999_ShouldHaveValidationError()
    {
        // Arrange
        UpdateArtistCommand command = _updateArtistCommandFixture.Create(albums: [_addAlbumCommandFixture.Create(tracks: [_addTrackCommandFixture.Create(metadata: _audioMetadataDtoFixture.Create(releaseInfo: _releaseInfoDtoFixture.Create(originalReleaseYear: 10000)))])]);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Metadata.OriginalReleaseYearMustBeBetween1And9999);
    }

    [Fact]
    public void Validate_WhenTrackReReleaseYearIsLessThan1_ShouldHaveValidationError()
    {
        // Arrange
        UpdateArtistCommand command = _updateArtistCommandFixture.Create(albums: [_addAlbumCommandFixture.Create(tracks: [_addTrackCommandFixture.Create(metadata: _audioMetadataDtoFixture.Create(releaseInfo: _releaseInfoDtoFixture.Create(reReleaseYear: 0)))])]);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Metadata.ReReleaseYearMustBeBetween1And9999);
    }

    [Fact]
    public void Validate_WhenTrackReReleaseYearIsGreaterThan9999_ShouldHaveValidationError()
    {
        // Arrange
        UpdateArtistCommand command = _updateArtistCommandFixture.Create(albums: [_addAlbumCommandFixture.Create(tracks: [_addTrackCommandFixture.Create(metadata: _audioMetadataDtoFixture.Create(releaseInfo: _releaseInfoDtoFixture.Create(reReleaseYear: 10000)))])]);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Metadata.ReReleaseYearMustBeBetween1And9999);
    }

    [Fact]
    public void Validate_WhenTrackReleaseVersionExceeds50Characters_ShouldHaveValidationError()
    {
        // Arrange
        UpdateArtistCommand command = _updateArtistCommandFixture.Create(albums: [_addAlbumCommandFixture.Create(tracks: [_addTrackCommandFixture.Create(metadata: _audioMetadataDtoFixture.Create(releaseInfo: _releaseInfoDtoFixture.Create(releaseVersion: new Faker().Random.String2(51))))])]);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Metadata.ReleaseVersionMustBeMaximum50CharactersLong);
    }

    [Fact]
    public void Validate_WhenTrackOriginalReleaseDateAndYearDoNotMatch_ShouldHaveValidationError()
    {
        // Arrange
        UpdateArtistCommand command = _updateArtistCommandFixture.Create(albums: [_addAlbumCommandFixture.Create(tracks: [_addTrackCommandFixture.Create(metadata: _audioMetadataDtoFixture.Create(releaseInfo: _releaseInfoDtoFixture.Create(originalReleaseDate: new DateOnly(2020, 1, 1), originalReleaseYear: 2019, includeReReleaseDate: false)))])]);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Metadata.OriginalReleaseDateAndYearMustMatch);
    }

    [Fact]
    public void Validate_WhenTrackReReleaseDateAndYearDoNotMatch_ShouldHaveValidationError()
    {
        // Arrange
        UpdateArtistCommand command = _updateArtistCommandFixture.Create(albums: [_addAlbumCommandFixture.Create(tracks: [_addTrackCommandFixture.Create(metadata: _audioMetadataDtoFixture.Create(releaseInfo: _releaseInfoDtoFixture.Create(reReleaseDate: new DateOnly(2021, 1, 1), reReleaseYear: 2020)))])]);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Metadata.ReReleaseDateAndYearMustMatch);
    }

    [Fact]
    public void Validate_WhenTrackReReleaseYearIsBeforeOriginalReleaseYear_ShouldHaveValidationError()
    {
        // Arrange
        UpdateArtistCommand command = _updateArtistCommandFixture.Create(albums: [_addAlbumCommandFixture.Create(tracks: [_addTrackCommandFixture.Create(metadata: _audioMetadataDtoFixture.Create(releaseInfo: _releaseInfoDtoFixture.Create(originalReleaseYear: 2001, reReleaseYear: 2000, includeReReleaseDate: false)))])]);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Metadata.ReReleaseYearCannotBeEarlierThanOriginalReleaseYear);
    }

    [Fact]
    public void Validate_WhenTrackReReleaseDateIsBeforeOriginalReleaseDate_ShouldHaveValidationError()
    {
        // Arrange
        UpdateArtistCommand command = _updateArtistCommandFixture.Create(albums: [_addAlbumCommandFixture.Create(tracks: [_addTrackCommandFixture.Create(metadata: _audioMetadataDtoFixture.Create(releaseInfo: _releaseInfoDtoFixture.Create(originalReleaseDate: new DateOnly(2001, 1, 1), reReleaseDate: new DateOnly(2000, 1, 1))))])]);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Metadata.ReReleaseDateCannotBeEarlierThanOriginalReleaseDate);
    }

    [Fact]
    public void Validate_WhenTrackGenresIsNull_ShouldHaveValidationError()
    {
        // Arrange
        UpdateArtistCommand command = _updateArtistCommandFixture.Create(albums: [_addAlbumCommandFixture.Create(tracks: [_addTrackCommandFixture.Create(metadata: _audioMetadataDtoFixture.Create(includeGenres: false))])]);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Metadata.GenresListCannotBeNull);
    }

    [Fact]
    public void Validate_WhenTrackGenreNameIsEmpty_ShouldHaveValidationError()
    {
        // Arrange
        UpdateArtistCommand command = _updateArtistCommandFixture.Create(albums: [_addAlbumCommandFixture.Create(tracks: [_addTrackCommandFixture.Create(metadata: _audioMetadataDtoFixture.Create(genres: [_genreDtoFixture.Create(name: string.Empty)]))])]);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Metadata.GenreNameCannotBeEmpty);
    }

    [Fact]
    public void Validate_WhenTrackGenreNameExceeds50Characters_ShouldHaveValidationError()
    {
        // Arrange
        UpdateArtistCommand command = _updateArtistCommandFixture.Create(albums: [_addAlbumCommandFixture.Create(tracks: [_addTrackCommandFixture.Create(metadata: _audioMetadataDtoFixture.Create(genres: [_genreDtoFixture.Create(name: new Faker().Random.String2(51))]))])]);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Metadata.GenreNameMustBeMaximum50CharactersLong);
    }

    [Fact]
    public void Validate_WhenTrackTagsIsNull_ShouldHaveValidationError()
    {
        // Arrange
        UpdateArtistCommand command = _updateArtistCommandFixture.Create(albums: [_addAlbumCommandFixture.Create(tracks: [_addTrackCommandFixture.Create(metadata: _audioMetadataDtoFixture.Create(includeTags: false))])]);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Metadata.TagsListCannotBeNull);
    }

    [Fact]
    public void Validate_WhenTrackTagNameIsEmpty_ShouldHaveValidationError()
    {
        // Arrange
        UpdateArtistCommand command = _updateArtistCommandFixture.Create(albums: [_addAlbumCommandFixture.Create(tracks: [_addTrackCommandFixture.Create(metadata: _audioMetadataDtoFixture.Create(tags: [_tagDtoFixture.Create(name: string.Empty)]))])]);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Metadata.TagNameCannotBeEmpty);
    }

    [Fact]
    public void Validate_WhenTrackTagNameExceeds50Characters_ShouldHaveValidationError()
    {
        // Arrange
        UpdateArtistCommand command = _updateArtistCommandFixture.Create(albums: [_addAlbumCommandFixture.Create(tracks: [_addTrackCommandFixture.Create(metadata: _audioMetadataDtoFixture.Create(tags: [_tagDtoFixture.Create(name: new Faker().Random.String2(51))]))])]);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Metadata.TagNameMustBeMaximum50CharactersLong);
    }

    [Fact]
    public void Validate_WhenTrackLanguageCodeIsEmpty_ShouldHaveValidationError()
    {
        // Arrange
        UpdateArtistCommand command = _updateArtistCommandFixture.Create(albums: [_addAlbumCommandFixture.Create(tracks: [_addTrackCommandFixture.Create(metadata: _audioMetadataDtoFixture.Create(language: _languageInfoDtoFixture.Create(languageCode: string.Empty)))])]);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Metadata.LanguageCodeCannotBeEmpty);
    }

    [Fact]
    public void Validate_WhenTrackLanguageCodeIsNot2CharactersLong_ShouldHaveValidationError()
    {
        // Arrange
        UpdateArtistCommand command = _updateArtistCommandFixture.Create(albums: [_addAlbumCommandFixture.Create(tracks: [_addTrackCommandFixture.Create(metadata: _audioMetadataDtoFixture.Create(language: _languageInfoDtoFixture.Create(languageCode: new Faker().Random.String2(3))))])]);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Metadata.LanguageCodeMustBe2CharactersLong);
    }

    [Fact]
    public void Validate_WhenTrackLanguageNameIsEmpty_ShouldHaveValidationError()
    {
        // Arrange
        UpdateArtistCommand command = _updateArtistCommandFixture.Create(albums: [_addAlbumCommandFixture.Create(tracks: [_addTrackCommandFixture.Create(metadata: _audioMetadataDtoFixture.Create(language: _languageInfoDtoFixture.Create(languageName: string.Empty)))])]);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Metadata.LanguageNameCannotBeEmpty);
    }

    [Fact]
    public void Validate_WhenTrackLanguageNameExceeds50Characters_ShouldHaveValidationError()
    {
        // Arrange
        UpdateArtistCommand command = _updateArtistCommandFixture.Create(albums: [_addAlbumCommandFixture.Create(tracks: [_addTrackCommandFixture.Create(metadata: _audioMetadataDtoFixture.Create(language: _languageInfoDtoFixture.Create(languageName: new Faker().Random.String2(51))))])]);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Metadata.LanguageNameMustBeMaximum50CharactersLong);
    }

    [Fact]
    public void Validate_WhenTrackLanguageNativeNameExceeds50Characters_ShouldHaveValidationError()
    {
        // Arrange
        UpdateArtistCommand command = _updateArtistCommandFixture.Create(albums: [_addAlbumCommandFixture.Create(tracks: [_addTrackCommandFixture.Create(metadata: _audioMetadataDtoFixture.Create(language: _languageInfoDtoFixture.Create(nativeName: new Faker().Random.String2(51))))])]);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Metadata.LanguageNativeNameMustBeMaximum50CharactersLong);
    }

    [Fact]
    public void Validate_WhenTrackOriginalLanguageCodeIsEmpty_ShouldHaveValidationError()
    {
        // Arrange
        UpdateArtistCommand command = _updateArtistCommandFixture.Create(albums: [_addAlbumCommandFixture.Create(tracks: [_addTrackCommandFixture.Create(metadata: _audioMetadataDtoFixture.Create(originalLanguage: _languageInfoDtoFixture.Create(languageCode: string.Empty)))])]);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Metadata.LanguageCodeCannotBeEmpty);
    }

    [Fact]
    public void Validate_WhenTrackOriginalLanguageNameIsEmpty_ShouldHaveValidationError()
    {
        // Arrange
        UpdateArtistCommand command = _updateArtistCommandFixture.Create(albums: [_addAlbumCommandFixture.Create(tracks: [_addTrackCommandFixture.Create(metadata: _audioMetadataDtoFixture.Create(originalLanguage: _languageInfoDtoFixture.Create(languageName: string.Empty)))])]);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Metadata.LanguageNameCannotBeEmpty);
    }

    [Fact]
    public void Validate_WhenTrackOriginalLanguageNativeNameExceeds50Characters_ShouldHaveValidationError()
    {
        // Arrange
        UpdateArtistCommand command = _updateArtistCommandFixture.Create(albums: [_addAlbumCommandFixture.Create(tracks: [_addTrackCommandFixture.Create(metadata: _audioMetadataDtoFixture.Create(originalLanguage: _languageInfoDtoFixture.Create(nativeName: new Faker().Random.String2(51))))])]);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Metadata.LanguageNativeNameMustBeMaximum50CharactersLong);
    }

    [Fact]
    public void Validate_WhenTrackMusicBrainzRecordingIdIsEmptyGuid_ShouldHaveValidationError()
    {
        // Arrange
        UpdateArtistCommand command = _updateArtistCommandFixture.Create(albums: [_addAlbumCommandFixture.Create(tracks: [_addTrackCommandFixture.Create(musicBrainzRecordingId: Guid.Empty)])]);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Music.MusicBrainzIdInvalidFormat);
    }

    [Fact]
    public void Validate_WhenTrackMusicBrainzTrackIdIsEmptyGuid_ShouldHaveValidationError()
    {
        // Arrange
        UpdateArtistCommand command = _updateArtistCommandFixture.Create(albums: [_addAlbumCommandFixture.Create(tracks: [_addTrackCommandFixture.Create(musicBrainzTrackId: Guid.Empty)])]);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Music.MusicBrainzIdInvalidFormat);
    }

    [Fact]
    public void Validate_WhenTrackMusicBrainzWorkIdIsEmptyGuid_ShouldHaveValidationError()
    {
        // Arrange
        UpdateArtistCommand command = _updateArtistCommandFixture.Create(albums: [_addAlbumCommandFixture.Create(tracks: [_addTrackCommandFixture.Create(musicBrainzWorkId: Guid.Empty)])]);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Music.MusicBrainzIdInvalidFormat);
    }

    [Fact]
    public void Validate_WhenTrackNumberIsNull_ShouldHaveValidationError()
    {
        // Arrange
        UpdateArtistCommand command = _updateArtistCommandFixture.Create(albums: [_addAlbumCommandFixture.Create(tracks: [_addTrackCommandFixture.Create(includeTrackNumber: false)])]);

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
        UpdateArtistCommand command = _updateArtistCommandFixture.Create(albums: [_addAlbumCommandFixture.Create(tracks: [_addTrackCommandFixture.Create(trackNumber: trackNumber)])]);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Music.TrackNumberMustBeGreaterThanZero);
    }

    [Theory]
    [InlineData(0)] // zero is not a valid disc number
    [InlineData(-1)] // a negative disc number is not valid
    public void Validate_WhenDiscNumberIsNotPositive_ShouldHaveValidationError(int discNumber)
    {
        // Arrange
        UpdateArtistCommand command = _updateArtistCommandFixture.Create(albums: [_addAlbumCommandFixture.Create(tracks: [_addTrackCommandFixture.Create(discNumber: discNumber)])]);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Music.DiscNumberMustBeGreaterThanZero);
    }

    [Fact]
    public void Validate_WhenTrackScriptExceeds50Characters_ShouldHaveValidationError()
    {
        // Arrange
        UpdateArtistCommand command = _updateArtistCommandFixture.Create(albums: [_addAlbumCommandFixture.Create(tracks: [_addTrackCommandFixture.Create(script: new Faker().Random.String2(51))])]);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Music.ScriptMustBeMaximum50CharactersLong);
    }

    [Fact]
    public void Validate_WhenTrackKeyIsNotDefined_ShouldHaveValidationError()
    {
        // Arrange
        UpdateArtistCommand command = _updateArtistCommandFixture.Create(albums: [_addAlbumCommandFixture.Create(tracks: [_addTrackCommandFixture.Create(key: (MusicKey)999)])]);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Music.UnknownMusicKey);
    }

    [Theory]
    [InlineData(0)] // zero is not a valid tempo
    [InlineData(-1)] // a negative tempo is not valid
    public void Validate_WhenTrackBpmIsNotPositive_ShouldHaveValidationError(int bpm)
    {
        // Arrange
        UpdateArtistCommand command = _updateArtistCommandFixture.Create(albums: [_addAlbumCommandFixture.Create(tracks: [_addTrackCommandFixture.Create(bpm: bpm)])]);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Music.BpmMustBeGreaterThanZero);
    }

    [Fact]
    public void Validate_WhenTrackWorkExceeds255Characters_ShouldHaveValidationError()
    {
        // Arrange
        UpdateArtistCommand command = _updateArtistCommandFixture.Create(albums: [_addAlbumCommandFixture.Create(tracks: [_addTrackCommandFixture.Create(work: new Faker().Random.String2(256))])]);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Music.WorkMustBeMaximum255CharactersLong);
    }

    [Fact]
    public void Validate_WhenTrackMoodNameIsEmpty_ShouldHaveValidationError()
    {
        // Arrange
        UpdateArtistCommand command = _updateArtistCommandFixture.Create(albums: [_addAlbumCommandFixture.Create(tracks: [_addTrackCommandFixture.Create(moods: [_moodDtoFixture.Create(includeName: false)])])]);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Metadata.MoodNameCannotBeEmpty);
    }

    [Fact]
    public void Validate_WhenTrackIsrcValueIsEmpty_ShouldHaveValidationError()
    {
        // Arrange
        UpdateArtistCommand command = _updateArtistCommandFixture.Create(albums: [_addAlbumCommandFixture.Create(tracks: [_addTrackCommandFixture.Create(isrcs: [_isrcDtoFixture.Create(includeValue: false)])])]);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Music.IsrcValueCannotBeEmpty);
    }

    [Fact]
    public void Validate_WhenTrackContributorsIsNull_ShouldHaveValidationError()
    {
        // Arrange
        UpdateArtistCommand command = _updateArtistCommandFixture.Create(albums: [_addAlbumCommandFixture.Create(tracks: [_addTrackCommandFixture.Create(includeContributors: false)])]);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.MediaContributor.ContributorsListCannotBeNull);
    }

    [Fact]
    public void Validate_WhenTrackContributorIdIsEmpty_ShouldHaveValidationError()
    {
        // Arrange
        UpdateArtistCommand command = _updateArtistCommandFixture.Create(albums: [_addAlbumCommandFixture.Create(tracks: [_addTrackCommandFixture.Create(contributors: [_mediaContributorReferenceDtoFixture.Create(contributorId: Guid.Empty, role: MediaContributorRole.Vocals)])])]);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.MediaContributor.MediaContributorIdCannotBeEmpty);
    }

    [Fact]
    public void Validate_WhenTrackContributorRoleIsNotDefined_ShouldHaveValidationError()
    {
        // Arrange
        UpdateArtistCommand command = _updateArtistCommandFixture.Create(albums: [_addAlbumCommandFixture.Create(tracks: [_addTrackCommandFixture.Create(contributors: [_mediaContributorReferenceDtoFixture.Create(contributorId: Guid.NewGuid(), role: (MediaContributorRole)999)])])]);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.MediaContributor.UnknownMediaContributorRole);
    }

    [Fact]
    public void Validate_WhenTrackRatingsIsNull_ShouldHaveValidationError()
    {
        // Arrange
        UpdateArtistCommand command = _updateArtistCommandFixture.Create(albums: [_addAlbumCommandFixture.Create(tracks: [_addTrackCommandFixture.Create(includeRatings: false)])]);

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
        UpdateArtistCommand command = _updateArtistCommandFixture.Create(albums: [_addAlbumCommandFixture.Create(tracks: [_addTrackCommandFixture.Create(ratings: [_audioRatingDtoFixture.Create(value: value, maxValue: 5)])])]);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Metadata.RatingValueMustBePositive);
    }

    [Fact]
    public void Validate_WhenTrackRatingValueIsGreaterThanMaxValue_ShouldHaveValidationError()
    {
        // Arrange
        UpdateArtistCommand command = _updateArtistCommandFixture.Create(albums: [_addAlbumCommandFixture.Create(tracks: [_addTrackCommandFixture.Create(ratings: [_audioRatingDtoFixture.Create(value: 6, maxValue: 5)])])]);

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
        UpdateArtistCommand command = _updateArtistCommandFixture.Create(albums: [_addAlbumCommandFixture.Create(tracks: [_addTrackCommandFixture.Create(ratings: [_audioRatingDtoFixture.Create(value: 1, maxValue: maxValue)])])]);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Metadata.RatingMaxValueMustBePositive);
    }

    [Fact]
    public void Validate_WhenTrackRatingVoteCountIsNegative_ShouldHaveValidationError()
    {
        // Arrange
        UpdateArtistCommand command = _updateArtistCommandFixture.Create(albums: [_addAlbumCommandFixture.Create(tracks: [_addTrackCommandFixture.Create(ratings: [_audioRatingDtoFixture.Create(value: 4, maxValue: 5, voteCount: -1)])])]);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Metadata.RatingVoteCountMustBePositive);
    }

    [Fact]
    public void Validate_WhenAlbumOriginalTitleIsValid_ShouldNotHaveValidationError()
    {
        // Arrange
        UpdateArtistCommand command = _updateArtistCommandFixture.Create(albums: [_addAlbumCommandFixture.Create(metadata: _albumMetadataDtoFixture.Create(originalTitle: new Faker().Random.String2(200)))]);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationError(Errors.Metadata.OriginalTitleMustBeMaximum255CharactersLong);
    }

    [Fact]
    public void Validate_WhenAlbumDescriptionIsValid_ShouldNotHaveValidationError()
    {
        // Arrange
        UpdateArtistCommand command = _updateArtistCommandFixture.Create(albums: [_addAlbumCommandFixture.Create(metadata: _albumMetadataDtoFixture.Create(description: new Faker().Random.String2(1500)))]);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationError(Errors.Metadata.DescriptionMustBeMaximum2000CharactersLong);
    }

    [Fact]
    public void Validate_WhenAlbumOriginalReleaseYearIsValid_ShouldNotHaveValidationError()
    {
        // Arrange
        UpdateArtistCommand command = _updateArtistCommandFixture.Create(albums: [_addAlbumCommandFixture.Create(metadata: _albumMetadataDtoFixture.Create(releaseInfo: _releaseInfoDtoFixture.Create(includeOriginalReleaseDate: false, originalReleaseYear: 2000, reReleaseYear: 2001, includeReReleaseDate: false)))]);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationError(Errors.Metadata.OriginalReleaseYearMustBeBetween1And9999);
    }

    [Fact]
    public void Validate_WhenAlbumReReleaseYearIsValid_ShouldNotHaveValidationError()
    {
        // Arrange
        UpdateArtistCommand command = _updateArtistCommandFixture.Create(albums: [_addAlbumCommandFixture.Create(metadata: _albumMetadataDtoFixture.Create(releaseInfo: _releaseInfoDtoFixture.Create(originalReleaseYear: 2000, reReleaseYear: 2001, includeReReleaseDate: false)))]);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationError(Errors.Metadata.ReReleaseYearMustBeBetween1And9999);
    }

    [Fact]
    public void Validate_WhenAlbumReleaseVersionIsValid_ShouldNotHaveValidationError()
    {
        // Arrange
        UpdateArtistCommand command = _updateArtistCommandFixture.Create(albums: [_addAlbumCommandFixture.Create(metadata: _albumMetadataDtoFixture.Create(releaseInfo: _releaseInfoDtoFixture.Create(releaseVersion: new Faker().Random.String2(50))))]);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationError(Errors.Metadata.ReleaseVersionMustBeMaximum50CharactersLong);
    }

    [Fact]
    public void Validate_WhenAlbumLanguageNameIsValid_ShouldNotHaveValidationError()
    {
        // Arrange
        UpdateArtistCommand command = _updateArtistCommandFixture.Create(albums: [_addAlbumCommandFixture.Create(metadata: _albumMetadataDtoFixture.Create(language: _languageInfoDtoFixture.Create(languageName: new Faker().Random.String2(20))))]);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationError(Errors.Metadata.LanguageNameCannotBeEmpty);
        result.ShouldNotHaveValidationError(Errors.Metadata.LanguageNameMustBeMaximum50CharactersLong);
    }

    [Fact]
    public void Validate_WhenAlbumLanguageNativeNameIsValid_ShouldNotHaveValidationError()
    {
        // Arrange
        UpdateArtistCommand command = _updateArtistCommandFixture.Create(albums: [_addAlbumCommandFixture.Create(metadata: _albumMetadataDtoFixture.Create(language: _languageInfoDtoFixture.Create(nativeName: new Faker().Random.String2(20))))]);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationError(Errors.Metadata.LanguageNativeNameMustBeMaximum50CharactersLong);
    }

    [Fact]
    public void Validate_WhenAlbumOriginalLanguageCodeIsValid_ShouldNotHaveValidationError()
    {
        // Arrange
        UpdateArtistCommand command = _updateArtistCommandFixture.Create(albums: [_addAlbumCommandFixture.Create(metadata: _albumMetadataDtoFixture.Create(originalLanguage: _languageInfoDtoFixture.Create(languageCode: new Faker().Random.String2(2))))]);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationError(Errors.Metadata.LanguageCodeMustBe2CharactersLong);
    }

    [Fact]
    public void Validate_WhenAlbumOriginalLanguageNameIsValid_ShouldNotHaveValidationError()
    {
        // Arrange
        UpdateArtistCommand command = _updateArtistCommandFixture.Create(albums: [_addAlbumCommandFixture.Create(metadata: _albumMetadataDtoFixture.Create(originalLanguage: _languageInfoDtoFixture.Create(languageName: new Faker().Random.String2(20))))]);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationError(Errors.Metadata.LanguageNameCannotBeEmpty);
        result.ShouldNotHaveValidationError(Errors.Metadata.LanguageNameMustBeMaximum50CharactersLong);
    }

    [Fact]
    public void Validate_WhenAlbumOriginalLanguageNativeNameIsValid_ShouldNotHaveValidationError()
    {
        // Arrange
        UpdateArtistCommand command = _updateArtistCommandFixture.Create(albums: [_addAlbumCommandFixture.Create(metadata: _albumMetadataDtoFixture.Create(originalLanguage: _languageInfoDtoFixture.Create(nativeName: new Faker().Random.String2(20))))]);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationError(Errors.Metadata.LanguageNativeNameMustBeMaximum50CharactersLong);
    }

    [Fact]
    public void Validate_WhenAlbumCatalogNumberIsValid_ShouldNotHaveValidationError()
    {
        // Arrange
        UpdateArtistCommand command = _updateArtistCommandFixture.Create(albums: [_addAlbumCommandFixture.Create(catalogNumber: new Faker().Random.String2(50))]);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationError(Errors.Music.CatalogNumberMustBeMaximum50CharactersLong);
    }

    [Fact]
    public void Validate_WhenTrackOriginalTitleIsNull_ShouldNotHaveValidationError()
    {
        // Arrange
        UpdateArtistCommand command = _updateArtistCommandFixture.Create(albums: [_addAlbumCommandFixture.Create(tracks: [_addTrackCommandFixture.Create(metadata: _audioMetadataDtoFixture.Create(includeOriginalTitle: false))])]);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationError(Errors.Metadata.OriginalTitleMustBeMaximum255CharactersLong);
    }

    [Fact]
    public void Validate_WhenTrackOriginalTitleIsValid_ShouldNotHaveValidationError()
    {
        // Arrange
        UpdateArtistCommand command = _updateArtistCommandFixture.Create(albums: [_addAlbumCommandFixture.Create(tracks: [_addTrackCommandFixture.Create(metadata: _audioMetadataDtoFixture.Create(originalTitle: new Faker().Random.String2(200)))])]);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationError(Errors.Metadata.OriginalTitleMustBeMaximum255CharactersLong);
    }

    [Fact]
    public void Validate_WhenTrackDescriptionIsNull_ShouldNotHaveValidationError()
    {
        // Arrange
        UpdateArtistCommand command = _updateArtistCommandFixture.Create(albums: [_addAlbumCommandFixture.Create(tracks: [_addTrackCommandFixture.Create(metadata: _audioMetadataDtoFixture.Create(includeDescription: false))])]);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationError(Errors.Metadata.DescriptionMustBeMaximum2000CharactersLong);
    }

    [Fact]
    public void Validate_WhenTrackDescriptionIsValid_ShouldNotHaveValidationError()
    {
        // Arrange
        UpdateArtistCommand command = _updateArtistCommandFixture.Create(albums: [_addAlbumCommandFixture.Create(tracks: [_addTrackCommandFixture.Create(metadata: _audioMetadataDtoFixture.Create(description: new Faker().Random.String2(1500)))])]);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationError(Errors.Metadata.DescriptionMustBeMaximum2000CharactersLong);
    }

    [Fact]
    public void Validate_WhenTrackOriginalReleaseYearIsValid_ShouldNotHaveValidationError()
    {
        // Arrange
        UpdateArtistCommand command = _updateArtistCommandFixture.Create(albums: [_addAlbumCommandFixture.Create(tracks: [_addTrackCommandFixture.Create(metadata: _audioMetadataDtoFixture.Create(releaseInfo: _releaseInfoDtoFixture.Create(includeOriginalReleaseDate: false, originalReleaseYear: 2000, reReleaseYear: 2001, includeReReleaseDate: false)))])]);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationError(Errors.Metadata.OriginalReleaseYearMustBeBetween1And9999);
    }

    [Fact]
    public void Validate_WhenTrackReReleaseYearIsValid_ShouldNotHaveValidationError()
    {
        // Arrange
        UpdateArtistCommand command = _updateArtistCommandFixture.Create(albums: [_addAlbumCommandFixture.Create(tracks: [_addTrackCommandFixture.Create(metadata: _audioMetadataDtoFixture.Create(releaseInfo: _releaseInfoDtoFixture.Create(originalReleaseYear: 2000, reReleaseYear: 2001, includeReReleaseDate: false)))])]);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationError(Errors.Metadata.ReReleaseYearMustBeBetween1And9999);
    }

    [Fact]
    public void Validate_WhenTrackReleaseVersionIsValid_ShouldNotHaveValidationError()
    {
        // Arrange
        UpdateArtistCommand command = _updateArtistCommandFixture.Create(albums: [_addAlbumCommandFixture.Create(tracks: [_addTrackCommandFixture.Create(metadata: _audioMetadataDtoFixture.Create(releaseInfo: _releaseInfoDtoFixture.Create(releaseVersion: new Faker().Random.String2(50))))])]);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationError(Errors.Metadata.ReleaseVersionMustBeMaximum50CharactersLong);
    }

    [Fact]
    public void Validate_WhenTrackOriginalReleaseDateAndYearMatch_ShouldNotHaveValidationError()
    {
        // Arrange
        UpdateArtistCommand command = _updateArtistCommandFixture.Create(albums: [_addAlbumCommandFixture.Create(tracks: [_addTrackCommandFixture.Create(metadata: _audioMetadataDtoFixture.Create(releaseInfo: _releaseInfoDtoFixture.Create(originalReleaseDate: new DateOnly(2020, 1, 1), originalReleaseYear: 2020, includeReReleaseDate: false)))])]);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationError(Errors.Metadata.OriginalReleaseDateAndYearMustMatch);
    }

    [Fact]
    public void Validate_WhenTrackReReleaseDateAndYearMatch_ShouldNotHaveValidationError()
    {
        // Arrange
        UpdateArtistCommand command = _updateArtistCommandFixture.Create(albums: [_addAlbumCommandFixture.Create(tracks: [_addTrackCommandFixture.Create(metadata: _audioMetadataDtoFixture.Create(releaseInfo: _releaseInfoDtoFixture.Create(reReleaseDate: new DateOnly(2021, 1, 1), reReleaseYear: 2021)))])]);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationError(Errors.Metadata.ReReleaseDateAndYearMustMatch);
    }

    [Fact]
    public void Validate_WhenTrackReReleaseYearIsAfterOriginalReleaseYear_ShouldNotHaveValidationError()
    {
        // Arrange
        UpdateArtistCommand command = _updateArtistCommandFixture.Create(albums: [_addAlbumCommandFixture.Create(tracks: [_addTrackCommandFixture.Create(metadata: _audioMetadataDtoFixture.Create(releaseInfo: _releaseInfoDtoFixture.Create(originalReleaseYear: 2000, reReleaseYear: 2001, includeReReleaseDate: false)))])]);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationError(Errors.Metadata.ReReleaseYearCannotBeEarlierThanOriginalReleaseYear);
    }

    [Fact]
    public void Validate_WhenTrackReReleaseDateIsAfterOriginalReleaseDate_ShouldNotHaveValidationError()
    {
        // Arrange
        UpdateArtistCommand command = _updateArtistCommandFixture.Create(albums: [_addAlbumCommandFixture.Create(tracks: [_addTrackCommandFixture.Create(metadata: _audioMetadataDtoFixture.Create(releaseInfo: _releaseInfoDtoFixture.Create(originalReleaseDate: new DateOnly(2000, 1, 1), reReleaseDate: new DateOnly(2001, 1, 1))))])]);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationError(Errors.Metadata.ReReleaseDateCannotBeEarlierThanOriginalReleaseDate);
    }

    [Fact]
    public void Validate_WhenTrackGenresAreValid_ShouldNotHaveValidationError()
    {
        // Arrange
        UpdateArtistCommand command = _updateArtistCommandFixture.Create(albums: [_addAlbumCommandFixture.Create(tracks: [_addTrackCommandFixture.Create(metadata: _audioMetadataDtoFixture.Create(genres: [_genreDtoFixture.Create(name: new Faker().Random.String2(50))]))])]);

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
        UpdateArtistCommand command = _updateArtistCommandFixture.Create(albums: [_addAlbumCommandFixture.Create(tracks: [_addTrackCommandFixture.Create(metadata: _audioMetadataDtoFixture.Create(tags: [_tagDtoFixture.Create(name: new Faker().Random.String2(50))]))])]);

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
        UpdateArtistCommand command = _updateArtistCommandFixture.Create(albums: [_addAlbumCommandFixture.Create(tracks: [_addTrackCommandFixture.Create(metadata: _audioMetadataDtoFixture.Create(includeLanguage: false))])]);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationError(Errors.Metadata.LanguageCodeCannotBeEmpty);
    }

    [Fact]
    public void Validate_WhenTrackLanguageCodeIsValid_ShouldNotHaveValidationError()
    {
        // Arrange
        UpdateArtistCommand command = _updateArtistCommandFixture.Create(albums: [_addAlbumCommandFixture.Create(tracks: [_addTrackCommandFixture.Create(metadata: _audioMetadataDtoFixture.Create(language: _languageInfoDtoFixture.Create(languageCode: new Faker().Random.String2(2))))])]);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationError(Errors.Metadata.LanguageCodeMustBe2CharactersLong);
    }

    [Fact]
    public void Validate_WhenTrackLanguageNameIsValid_ShouldNotHaveValidationError()
    {
        // Arrange
        UpdateArtistCommand command = _updateArtistCommandFixture.Create(albums: [_addAlbumCommandFixture.Create(tracks: [_addTrackCommandFixture.Create(metadata: _audioMetadataDtoFixture.Create(language: _languageInfoDtoFixture.Create(languageName: new Faker().Random.String2(20))))])]);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationError(Errors.Metadata.LanguageNameCannotBeEmpty);
        result.ShouldNotHaveValidationError(Errors.Metadata.LanguageNameMustBeMaximum50CharactersLong);
    }

    [Fact]
    public void Validate_WhenTrackLanguageNativeNameIsNull_ShouldNotHaveValidationError()
    {
        // Arrange
        UpdateArtistCommand command = _updateArtistCommandFixture.Create(albums: [_addAlbumCommandFixture.Create(tracks: [_addTrackCommandFixture.Create(metadata: _audioMetadataDtoFixture.Create(language: _languageInfoDtoFixture.Create(includeNativeName: false)))])]);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationError(Errors.Metadata.LanguageNativeNameMustBeMaximum50CharactersLong);
    }

    [Fact]
    public void Validate_WhenTrackLanguageNativeNameIsValid_ShouldNotHaveValidationError()
    {
        // Arrange
        UpdateArtistCommand command = _updateArtistCommandFixture.Create(albums: [_addAlbumCommandFixture.Create(tracks: [_addTrackCommandFixture.Create(metadata: _audioMetadataDtoFixture.Create(language: _languageInfoDtoFixture.Create(nativeName: new Faker().Random.String2(20))))])]);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationError(Errors.Metadata.LanguageNativeNameMustBeMaximum50CharactersLong);
    }

    [Fact]
    public void Validate_WhenTrackOriginalLanguageIsNull_ShouldNotHaveValidationError()
    {
        // Arrange
        UpdateArtistCommand command = _updateArtistCommandFixture.Create(albums: [_addAlbumCommandFixture.Create(tracks: [_addTrackCommandFixture.Create(metadata: _audioMetadataDtoFixture.Create(includeOriginalLanguage: false))])]);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationError(Errors.Metadata.LanguageCodeCannotBeEmpty);
    }

    [Fact]
    public void Validate_WhenTrackOriginalLanguageCodeIsValid_ShouldNotHaveValidationError()
    {
        // Arrange
        UpdateArtistCommand command = _updateArtistCommandFixture.Create(albums: [_addAlbumCommandFixture.Create(tracks: [_addTrackCommandFixture.Create(metadata: _audioMetadataDtoFixture.Create(originalLanguage: _languageInfoDtoFixture.Create(languageCode: new Faker().Random.String2(2))))])]);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationError(Errors.Metadata.LanguageCodeMustBe2CharactersLong);
    }

    [Fact]
    public void Validate_WhenTrackOriginalLanguageNameIsValid_ShouldNotHaveValidationError()
    {
        // Arrange
        UpdateArtistCommand command = _updateArtistCommandFixture.Create(albums: [_addAlbumCommandFixture.Create(tracks: [_addTrackCommandFixture.Create(metadata: _audioMetadataDtoFixture.Create(originalLanguage: _languageInfoDtoFixture.Create(languageName: new Faker().Random.String2(20))))])]);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationError(Errors.Metadata.LanguageNameCannotBeEmpty);
        result.ShouldNotHaveValidationError(Errors.Metadata.LanguageNameMustBeMaximum50CharactersLong);
    }

    [Fact]
    public void Validate_WhenTrackOriginalLanguageNativeNameIsNull_ShouldNotHaveValidationError()
    {
        // Arrange
        UpdateArtistCommand command = _updateArtistCommandFixture.Create(albums: [_addAlbumCommandFixture.Create(tracks: [_addTrackCommandFixture.Create(metadata: _audioMetadataDtoFixture.Create(originalLanguage: _languageInfoDtoFixture.Create(includeNativeName: false)))])]);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationError(Errors.Metadata.LanguageNativeNameMustBeMaximum50CharactersLong);
    }

    [Fact]
    public void Validate_WhenTrackOriginalLanguageNativeNameIsValid_ShouldNotHaveValidationError()
    {
        // Arrange
        UpdateArtistCommand command = _updateArtistCommandFixture.Create(albums: [_addAlbumCommandFixture.Create(tracks: [_addTrackCommandFixture.Create(metadata: _audioMetadataDtoFixture.Create(originalLanguage: _languageInfoDtoFixture.Create(nativeName: new Faker().Random.String2(20))))])]);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationError(Errors.Metadata.LanguageNativeNameMustBeMaximum50CharactersLong);
    }

    [Fact]
    public void Validate_WhenTrackMusicBrainzIdsAreValid_ShouldNotHaveValidationError()
    {
        // Arrange
        UpdateArtistCommand command = _updateArtistCommandFixture.Create(albums: [_addAlbumCommandFixture.Create(tracks: [_addTrackCommandFixture.Create(musicBrainzRecordingId: Guid.NewGuid(), musicBrainzTrackId: Guid.NewGuid(), musicBrainzWorkId: Guid.NewGuid())])]);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationError(Errors.Music.MusicBrainzIdInvalidFormat);
    }

    [Fact]
    public void Validate_WhenTrackNumberIsPositive_ShouldNotHaveValidationError()
    {
        // Arrange
        UpdateArtistCommand command = _updateArtistCommandFixture.Create(albums: [_addAlbumCommandFixture.Create(tracks: [_addTrackCommandFixture.Create(trackNumber: 7)])]);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationError(Errors.Music.TrackNumberMustBeGreaterThanZero);
    }

    [Fact]
    public void Validate_WhenDiscNumberIsNull_ShouldNotHaveValidationError()
    {
        // Arrange
        UpdateArtistCommand command = _updateArtistCommandFixture.Create(albums: [_addAlbumCommandFixture.Create(tracks: [_addTrackCommandFixture.Create(includeDiscNumber: false, includeScript: false, includeKey: false, includeBpm: false, includeWork: false, includeMusicBrainzRecordingId: false, includeMusicBrainzTrackId: false, includeMusicBrainzWorkId: false, includeMoods: false, includeIsrcs: false)])]);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationError(Errors.Music.DiscNumberMustBeGreaterThanZero);
    }

    [Fact]
    public void Validate_WhenDiscNumberIsPositive_ShouldNotHaveValidationError()
    {
        // Arrange
        UpdateArtistCommand command = _updateArtistCommandFixture.Create(albums: [_addAlbumCommandFixture.Create(tracks: [_addTrackCommandFixture.Create(discNumber: 1)])]);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationError(Errors.Music.DiscNumberMustBeGreaterThanZero);
    }

    [Fact]
    public void Validate_WhenTrackScriptIsNull_ShouldNotHaveValidationError()
    {
        // Arrange
        UpdateArtistCommand command = _updateArtistCommandFixture.Create(albums: [_addAlbumCommandFixture.Create(tracks: [_addTrackCommandFixture.Create(includeDiscNumber: false, includeScript: false, includeKey: false, includeBpm: false, includeWork: false, includeMusicBrainzRecordingId: false, includeMusicBrainzTrackId: false, includeMusicBrainzWorkId: false, includeMoods: false, includeIsrcs: false)])]);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationError(Errors.Music.ScriptMustBeMaximum50CharactersLong);
    }

    [Fact]
    public void Validate_WhenTrackScriptIsValid_ShouldNotHaveValidationError()
    {
        // Arrange
        UpdateArtistCommand command = _updateArtistCommandFixture.Create(albums: [_addAlbumCommandFixture.Create(tracks: [_addTrackCommandFixture.Create(script: new Faker().Random.String2(50))])]);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationError(Errors.Music.ScriptMustBeMaximum50CharactersLong);
    }

    [Fact]
    public void Validate_WhenTrackKeyIsNull_ShouldNotHaveValidationError()
    {
        // Arrange
        UpdateArtistCommand command = _updateArtistCommandFixture.Create(albums: [_addAlbumCommandFixture.Create(tracks: [_addTrackCommandFixture.Create(includeDiscNumber: false, includeScript: false, includeKey: false, includeBpm: false, includeWork: false, includeMusicBrainzRecordingId: false, includeMusicBrainzTrackId: false, includeMusicBrainzWorkId: false, includeMoods: false, includeIsrcs: false)])]);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationError(Errors.Music.UnknownMusicKey);
    }

    [Fact]
    public void Validate_WhenTrackKeyIsDefined_ShouldNotHaveValidationError()
    {
        // Arrange
        UpdateArtistCommand command = _updateArtistCommandFixture.Create(albums: [_addAlbumCommandFixture.Create(tracks: [_addTrackCommandFixture.Create(key: MusicKey.CMajor)])]);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationError(Errors.Music.UnknownMusicKey);
    }

    [Fact]
    public void Validate_WhenTrackBpmIsNull_ShouldNotHaveValidationError()
    {
        // Arrange
        UpdateArtistCommand command = _updateArtistCommandFixture.Create(albums: [_addAlbumCommandFixture.Create(tracks: [_addTrackCommandFixture.Create(includeDiscNumber: false, includeScript: false, includeKey: false, includeBpm: false, includeWork: false, includeMusicBrainzRecordingId: false, includeMusicBrainzTrackId: false, includeMusicBrainzWorkId: false, includeMoods: false, includeIsrcs: false)])]);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationError(Errors.Music.BpmMustBeGreaterThanZero);
    }

    [Fact]
    public void Validate_WhenTrackBpmIsPositive_ShouldNotHaveValidationError()
    {
        // Arrange
        UpdateArtistCommand command = _updateArtistCommandFixture.Create(albums: [_addAlbumCommandFixture.Create(tracks: [_addTrackCommandFixture.Create(bpm: 120)])]);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationError(Errors.Music.BpmMustBeGreaterThanZero);
    }

    [Fact]
    public void Validate_WhenTrackWorkIsNull_ShouldNotHaveValidationError()
    {
        // Arrange
        UpdateArtistCommand command = _updateArtistCommandFixture.Create(albums: [_addAlbumCommandFixture.Create(tracks: [_addTrackCommandFixture.Create(includeDiscNumber: false, includeScript: false, includeKey: false, includeBpm: false, includeWork: false, includeMusicBrainzRecordingId: false, includeMusicBrainzTrackId: false, includeMusicBrainzWorkId: false, includeMoods: false, includeIsrcs: false)])]);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationError(Errors.Music.WorkMustBeMaximum255CharactersLong);
    }

    [Fact]
    public void Validate_WhenTrackWorkIsValid_ShouldNotHaveValidationError()
    {
        // Arrange
        UpdateArtistCommand command = _updateArtistCommandFixture.Create(albums: [_addAlbumCommandFixture.Create(tracks: [_addTrackCommandFixture.Create(work: new Faker().Random.String2(255))])]);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationError(Errors.Music.WorkMustBeMaximum255CharactersLong);
    }

    [Fact]
    public void Validate_WhenTrackMoodNameIsValid_ShouldNotHaveValidationError()
    {
        // Arrange
        UpdateArtistCommand command = _updateArtistCommandFixture.Create(albums: [_addAlbumCommandFixture.Create(tracks: [_addTrackCommandFixture.Create(moods: [_moodDtoFixture.Create(name: "dramatic")])])]);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationError(Errors.Metadata.MoodNameCannotBeEmpty);
    }

    [Fact]
    public void Validate_WhenTrackMoodsIsNull_ShouldNotHaveValidationError()
    {
        // Arrange
        UpdateArtistCommand command = _updateArtistCommandFixture.Create(albums: [_addAlbumCommandFixture.Create(tracks: [_addTrackCommandFixture.Create(includeDiscNumber: false, includeScript: false, includeKey: false, includeBpm: false, includeWork: false, includeMusicBrainzRecordingId: false, includeMusicBrainzTrackId: false, includeMusicBrainzWorkId: false, includeMoods: false, includeIsrcs: false)])]);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationError(Errors.Metadata.MoodNameCannotBeEmpty);
    }

    [Fact]
    public void Validate_WhenTrackIsrcValueIsValid_ShouldNotHaveValidationError()
    {
        // Arrange
        UpdateArtistCommand command = _updateArtistCommandFixture.Create(albums: [_addAlbumCommandFixture.Create(tracks: [_addTrackCommandFixture.Create(isrcs: [_isrcDtoFixture.Create(value: "GBUM71029604")])])]);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationError(Errors.Music.IsrcValueCannotBeEmpty);
    }

    [Fact]
    public void Validate_WhenTrackIsrcsIsNull_ShouldNotHaveValidationError()
    {
        // Arrange
        UpdateArtistCommand command = _updateArtistCommandFixture.Create(albums: [_addAlbumCommandFixture.Create(tracks: [_addTrackCommandFixture.Create(includeDiscNumber: false, includeScript: false, includeKey: false, includeBpm: false, includeWork: false, includeMusicBrainzRecordingId: false, includeMusicBrainzTrackId: false, includeMusicBrainzWorkId: false, includeMoods: false, includeIsrcs: false)])]);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationError(Errors.Music.IsrcValueCannotBeEmpty);
    }

    [Fact]
    public void Validate_WhenTrackContributorsAreValid_ShouldNotHaveValidationError()
    {
        // Arrange
        UpdateArtistCommand command = _updateArtistCommandFixture.Create(albums: [_addAlbumCommandFixture.Create(tracks: [_addTrackCommandFixture.Create(contributors: [_mediaContributorReferenceDtoFixture.Create(contributorId: Guid.NewGuid(), role: MediaContributorRole.Vocals)])])]);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationError(Errors.MediaContributor.ContributorsListCannotBeNull);
        result.ShouldNotHaveValidationError(Errors.MediaContributor.MediaContributorIdCannotBeEmpty);
        result.ShouldNotHaveValidationError(Errors.MediaContributor.UnknownMediaContributorRole);
    }

    [Fact]
    public void Validate_WhenTrackRatingsAreValid_ShouldNotHaveValidationError()
    {
        // Arrange
        UpdateArtistCommand command = _updateArtistCommandFixture.Create(albums: [_addAlbumCommandFixture.Create(tracks: [_addTrackCommandFixture.Create(ratings: [_audioRatingDtoFixture.Create(value: 4, maxValue: 5, voteCount: 100)])])]);

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
    public void Validate_WhenTrackRatingVoteCountIsNull_ShouldNotHaveValidationError()
    {
        // Arrange
        UpdateArtistCommand command = _updateArtistCommandFixture.Create(albums: [_addAlbumCommandFixture.Create(tracks: [_addTrackCommandFixture.Create(ratings: [_audioRatingDtoFixture.Create(value: 4, maxValue: 5, includeVoteCount: false)])])]);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationError(Errors.Metadata.RatingVoteCountMustBePositive);
    }

    [Fact]
    public void Validate_WhenCommandIsValid_ShouldNotHaveAnyValidationErrors()
    {
        // Arrange
        UpdateArtistCommand command = _updateArtistCommandFixture.Create();

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }
}
