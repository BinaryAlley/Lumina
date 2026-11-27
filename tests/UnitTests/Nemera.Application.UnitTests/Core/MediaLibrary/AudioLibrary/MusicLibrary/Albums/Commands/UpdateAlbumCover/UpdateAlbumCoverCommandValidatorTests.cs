#region ========================================================================= USING =====================================================================================
using Lumina.Application.Core.MediaLibrary.AudioLibrary.MusicLibrary.Albums.Commands.UpdateAlbumCover;
using Lumina.Application.Fixtures.Core.MediaLibrary.AudioLibrary.MusicLibrary.Albums.Commands.UpdateAlbumCover;
using Lumina.Application.UnitTests.Common.Setup;
using Lumina.Domain.Common.Errors;
using Lumina.Domain.Common.Primitives;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Application.UnitTests.Core.MediaLibrary.AudioLibrary.MusicLibrary.Albums.Commands.UpdateAlbumCover;

/// <summary>
/// Contains unit tests for the <see cref="UpdateAlbumCoverCommandValidator"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class UpdateAlbumCoverCommandValidatorTests
{
    private readonly UpdateAlbumCoverCommandFixture _updateAlbumCoverCommandFixture = new();
    private readonly UpdateAlbumCoverCommandValidator _validator = new();

    [Fact]
    public void Validate_WhenCommandIsValid_ShouldNotHaveValidationErrors()
    {
        // Arrange
        UpdateAlbumCoverCommand command = _updateAlbumCoverCommandFixture.Create();

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public void Validate_WhenLibraryIdIsNull_ShouldHaveValidationError()
    {
        // Arrange
        UpdateAlbumCoverCommand command = _updateAlbumCoverCommandFixture.Create(includeLibraryId: false);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Library.LibraryIdCannotBeEmpty);
    }

    [Fact]
    public void Validate_WhenLibraryIdIsEmpty_ShouldHaveValidationError()
    {
        // Arrange
        UpdateAlbumCoverCommand command = _updateAlbumCoverCommandFixture.Create(libraryId: Guid.Empty.ToString());

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Library.LibraryIdCannotBeEmpty);
    }

    [Fact]
    public void Validate_WhenLibraryIdIsWhitespace_ShouldHaveValidationError()
    {
        // Arrange
        UpdateAlbumCoverCommand command = _updateAlbumCoverCommandFixture.Create(libraryId: "   ");

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Library.LibraryIdCannotBeEmpty);
    }

    [Fact]
    public void Validate_WhenLibraryIdIsNotAGuid_ShouldHaveValidationError()
    {
        // Arrange
        UpdateAlbumCoverCommand command = _updateAlbumCoverCommandFixture.Create(libraryId: "not-a-guid");

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Library.LibraryIdCannotBeEmpty);
    }

    [Fact]
    public void Validate_WhenLibraryIdIsValid_ShouldNotHaveValidationError()
    {
        // Arrange
        UpdateAlbumCoverCommand command = _updateAlbumCoverCommandFixture.Create(libraryId: Guid.NewGuid().ToString());

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationError(Errors.Library.LibraryIdCannotBeEmpty);
    }

    [Fact]
    public void Validate_WhenArtistIdIsNull_ShouldHaveValidationError()
    {
        // Arrange
        UpdateAlbumCoverCommand command = _updateAlbumCoverCommandFixture.Create(includeArtistId: false);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Music.ArtistIdCannotBeEmpty);
    }

    [Fact]
    public void Validate_WhenArtistIdIsEmpty_ShouldHaveValidationError()
    {
        // Arrange
        UpdateAlbumCoverCommand command = _updateAlbumCoverCommandFixture.Create(artistId: Guid.Empty.ToString());

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Music.ArtistIdCannotBeEmpty);
    }

    [Fact]
    public void Validate_WhenArtistIdIsWhitespace_ShouldHaveValidationError()
    {
        // Arrange
        UpdateAlbumCoverCommand command = _updateAlbumCoverCommandFixture.Create(artistId: "   ");

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Music.ArtistIdCannotBeEmpty);
    }

    [Fact]
    public void Validate_WhenArtistIdIsNotAGuid_ShouldHaveValidationError()
    {
        // Arrange
        UpdateAlbumCoverCommand command = _updateAlbumCoverCommandFixture.Create(artistId: "not-a-guid");

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Music.ArtistIdCannotBeEmpty);
    }

    [Fact]
    public void Validate_WhenArtistIdIsValid_ShouldNotHaveValidationError()
    {
        // Arrange
        UpdateAlbumCoverCommand command = _updateAlbumCoverCommandFixture.Create(artistId: Guid.NewGuid().ToString());

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationError(Errors.Music.ArtistIdCannotBeEmpty);
    }

    [Fact]
    public void Validate_WhenAlbumIdIsNull_ShouldHaveValidationError()
    {
        // Arrange
        UpdateAlbumCoverCommand command = _updateAlbumCoverCommandFixture.Create(includeAlbumId: false);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Music.AlbumIdCannotBeEmpty);
    }

    [Fact]
    public void Validate_WhenAlbumIdIsEmpty_ShouldHaveValidationError()
    {
        // Arrange
        UpdateAlbumCoverCommand command = _updateAlbumCoverCommandFixture.Create(albumId: Guid.Empty.ToString());

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Music.AlbumIdCannotBeEmpty);
    }

    [Fact]
    public void Validate_WhenAlbumIdIsWhitespace_ShouldHaveValidationError()
    {
        // Arrange
        UpdateAlbumCoverCommand command = _updateAlbumCoverCommandFixture.Create(albumId: "   ");

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Music.AlbumIdCannotBeEmpty);
    }

    [Fact]
    public void Validate_WhenAlbumIdIsNotAGuid_ShouldHaveValidationError()
    {
        // Arrange
        UpdateAlbumCoverCommand command = _updateAlbumCoverCommandFixture.Create(albumId: "not-a-guid");

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Music.AlbumIdCannotBeEmpty);
    }

    [Fact]
    public void Validate_WhenAlbumIdIsValid_ShouldNotHaveValidationError()
    {
        // Arrange
        UpdateAlbumCoverCommand command = _updateAlbumCoverCommandFixture.Create(albumId: Guid.NewGuid().ToString());

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationError(Errors.Music.AlbumIdCannotBeEmpty);
    }

    [Fact]
    public void Validate_WhenCoverIsNull_ShouldHaveValidationError()
    {
        // Arrange
        UpdateAlbumCoverCommand command = _updateAlbumCoverCommandFixture.Create(includeCover: false);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Music.CoverCannotBeNull);
    }

    [Fact]
    public void Validate_WhenCoverIsPresentAndFileNameIsEmpty_ShouldHaveValidationError()
    {
        // Arrange
        UpdateAlbumCoverCommand command = _updateAlbumCoverCommandFixture.Create(fileName: string.Empty);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.FileSystemManagement.FileNameCannotBeEmpty);
    }

    [Fact]
    public void Validate_WhenCoverIsPresentAndFileNameIsNull_ShouldHaveValidationError()
    {
        // Arrange
        UpdateAlbumCoverCommand command = _updateAlbumCoverCommandFixture.Create(includeFileName: false);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.FileSystemManagement.FileNameCannotBeEmpty);
    }

    [Fact]
    public void Validate_WhenCoverIsMissingAndFileNameIsEmpty_ShouldNotHaveFileNameValidationError()
    {
        // Arrange
        UpdateAlbumCoverCommand command = _updateAlbumCoverCommandFixture.Create(fileName: string.Empty, includeCover: false);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationError(Errors.FileSystemManagement.FileNameCannotBeEmpty);
    }
}
