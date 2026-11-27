#region ========================================================================= USING =====================================================================================
using Lumina.Application.Core.MediaLibrary.AudioLibrary.MusicLibrary.Artists.Commands.UpdateArtistCover;
using Lumina.Application.Fixtures.Core.MediaLibrary.AudioLibrary.MusicLibrary.Artists.Commands.UpdateArtistCover;
using Lumina.Application.UnitTests.Common.Setup;
using Lumina.Domain.Common.Errors;
using Lumina.Domain.Common.Primitives;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Application.UnitTests.Core.MediaLibrary.AudioLibrary.MusicLibrary.Artists.Commands.UpdateArtistCover;

/// <summary>
/// Contains unit tests for the <see cref="UpdateArtistCoverCommandValidator"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class UpdateArtistCoverCommandValidatorTests
{
    private readonly UpdateArtistCoverCommandFixture _updateArtistCoverCommandFixture = new();
    private readonly UpdateArtistCoverCommandValidator _validator = new();

    [Fact]
    public void Validate_WhenCommandIsValid_ShouldNotHaveValidationErrors()
    {
        // Arrange
        UpdateArtistCoverCommand command = _updateArtistCoverCommandFixture.Create();

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public void Validate_WhenLibraryIdIsNull_ShouldHaveValidationError()
    {
        // Arrange
        UpdateArtistCoverCommand command = _updateArtistCoverCommandFixture.Create(includeLibraryId: false);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Library.LibraryIdCannotBeEmpty);
    }

    [Fact]
    public void Validate_WhenLibraryIdIsEmpty_ShouldHaveValidationError()
    {
        // Arrange
        UpdateArtistCoverCommand command = _updateArtistCoverCommandFixture.Create(libraryId: Guid.Empty.ToString());

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Library.LibraryIdCannotBeEmpty);
    }

    [Fact]
    public void Validate_WhenLibraryIdIsWhitespace_ShouldHaveValidationError()
    {
        // Arrange
        UpdateArtistCoverCommand command = _updateArtistCoverCommandFixture.Create(libraryId: "   ");

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Library.LibraryIdCannotBeEmpty);
    }

    [Fact]
    public void Validate_WhenLibraryIdIsNotAGuid_ShouldHaveValidationError()
    {
        // Arrange
        UpdateArtistCoverCommand command = _updateArtistCoverCommandFixture.Create(libraryId: "not-a-guid");

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Library.LibraryIdCannotBeEmpty);
    }

    [Fact]
    public void Validate_WhenLibraryIdIsValid_ShouldNotHaveValidationError()
    {
        // Arrange
        UpdateArtistCoverCommand command = _updateArtistCoverCommandFixture.Create(libraryId: Guid.NewGuid().ToString());

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationError(Errors.Library.LibraryIdCannotBeEmpty);
    }

    [Fact]
    public void Validate_WhenArtistIdIsNull_ShouldHaveValidationError()
    {
        // Arrange
        UpdateArtistCoverCommand command = _updateArtistCoverCommandFixture.Create(includeArtistId: false);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Music.ArtistIdCannotBeEmpty);
    }

    [Fact]
    public void Validate_WhenArtistIdIsEmpty_ShouldHaveValidationError()
    {
        // Arrange
        UpdateArtistCoverCommand command = _updateArtistCoverCommandFixture.Create(artistId: Guid.Empty.ToString());

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Music.ArtistIdCannotBeEmpty);
    }

    [Fact]
    public void Validate_WhenArtistIdIsWhitespace_ShouldHaveValidationError()
    {
        // Arrange
        UpdateArtistCoverCommand command = _updateArtistCoverCommandFixture.Create(artistId: "   ");

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Music.ArtistIdCannotBeEmpty);
    }

    [Fact]
    public void Validate_WhenArtistIdIsNotAGuid_ShouldHaveValidationError()
    {
        // Arrange
        UpdateArtistCoverCommand command = _updateArtistCoverCommandFixture.Create(artistId: "not-a-guid");

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Music.ArtistIdCannotBeEmpty);
    }

    [Fact]
    public void Validate_WhenArtistIdIsValid_ShouldNotHaveValidationError()
    {
        // Arrange
        UpdateArtistCoverCommand command = _updateArtistCoverCommandFixture.Create(artistId: Guid.NewGuid().ToString());

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationError(Errors.Music.ArtistIdCannotBeEmpty);
    }

    [Fact]
    public void Validate_WhenCoverIsNull_ShouldHaveValidationError()
    {
        // Arrange
        UpdateArtistCoverCommand command = _updateArtistCoverCommandFixture.Create(includeCover: false);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Music.CoverCannotBeNull);
    }

    [Fact]
    public void Validate_WhenCoverIsPresentAndFileNameIsEmpty_ShouldHaveValidationError()
    {
        // Arrange
        UpdateArtistCoverCommand command = _updateArtistCoverCommandFixture.Create(fileName: string.Empty);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.FileSystemManagement.FileNameCannotBeEmpty);
    }

    [Fact]
    public void Validate_WhenCoverIsPresentAndFileNameIsNull_ShouldHaveValidationError()
    {
        // Arrange
        UpdateArtistCoverCommand command = _updateArtistCoverCommandFixture.Create(includeFileName: false);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.FileSystemManagement.FileNameCannotBeEmpty);
    }

    [Fact]
    public void Validate_WhenCoverIsMissingAndFileNameIsEmpty_ShouldNotHaveFileNameValidationError()
    {
        // Arrange
        UpdateArtistCoverCommand command = _updateArtistCoverCommandFixture.Create(fileName: string.Empty, includeCover: false);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationError(Errors.FileSystemManagement.FileNameCannotBeEmpty);
    }
}
