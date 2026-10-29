#region ========================================================================= USING =====================================================================================
using Lumina.Application.Core.Playlists.Commands.CreatePlaylist;
using Lumina.Application.Fixtures.Core.Playlists.Commands.CreatePlaylist;
using Lumina.Application.UnitTests.Common.Setup;
using Lumina.Domain.Common.Errors;
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.SharedKernel.Common.Enums.Playlists;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Application.UnitTests.Core.Playlists.Commands.CreatePlaylist;

/// <summary>
/// Contains unit tests for the <see cref="CreatePlaylistCommandValidator"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class CreatePlaylistCommandValidatorTests
{
    private const int MAXIMUM_PLAYLIST_NAME_LENGTH = 255;

    private readonly CreatePlaylistCommandValidator _validator = new();
    private readonly CreatePlaylistCommandFixture _createPlaylistCommandFixture = new();

    [Fact]
    public void Validate_WhenNameIsNotSupplied_ShouldHaveValidationError()
    {
        // Arrange
        CreatePlaylistCommand command = _createPlaylistCommandFixture.Create(includeName: false);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Playlists.PlaylistNameCannotBeEmpty);
    }

    [Fact]
    public void Validate_WhenNameIsEmpty_ShouldHaveValidationError()
    {
        // Arrange
        CreatePlaylistCommand command = _createPlaylistCommandFixture.Create(name: string.Empty);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Playlists.PlaylistNameCannotBeEmpty);
    }

    [Fact]
    public void Validate_WhenNameIsWhiteSpaceOnly_ShouldHaveValidationError()
    {
        // Arrange
        CreatePlaylistCommand command = _createPlaylistCommandFixture.Create(name: "   ");

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Playlists.PlaylistNameCannotBeEmpty);
    }

    [Fact]
    public void Validate_WhenNameIsLongerThanTheMaximumLength_ShouldHaveValidationError()
    {
        // Arrange
        CreatePlaylistCommand command = _createPlaylistCommandFixture.Create(
            name: new string('a', MAXIMUM_PLAYLIST_NAME_LENGTH + 1));

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Playlists.PlaylistNameMustBeMaximum255CharactersLong);
    }

    [Fact]
    public void Validate_WhenNameHasTheMaximumLength_ShouldNotHaveValidationError()
    {
        // Arrange
        CreatePlaylistCommand command = _createPlaylistCommandFixture.Create(
            name: new string('a', MAXIMUM_PLAYLIST_NAME_LENGTH));

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationError(Errors.Playlists.PlaylistNameMustBeMaximum255CharactersLong);
    }

    [Fact]
    public void Validate_WhenDescriptionIsLongerThanTheMaximumLength_ShouldHaveValidationError()
    {
        // Arrange
        CreatePlaylistCommand command = _createPlaylistCommandFixture.Create(description: new string('a', 2001));

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Playlists.PlaylistDescriptionMustBeMaximum2000CharactersLong);
    }

    [Fact]
    public void Validate_WhenDescriptionIsOmitted_ShouldNotHaveValidationError()
    {
        // Arrange
        CreatePlaylistCommand command = _createPlaylistCommandFixture.Create(includeDescription: false);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationError(Errors.Playlists.PlaylistDescriptionMustBeMaximum2000CharactersLong);
    }

    [Fact]
    public void Validate_WhenTypeIsNotSupplied_ShouldHaveValidationError()
    {
        // Arrange
        CreatePlaylistCommand command = _createPlaylistCommandFixture.Create(includeType: false);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Playlists.PlaylistTypeIsInvalid);
    }

    [Fact]
    public void Validate_WhenTypeIsNotADefinedMember_ShouldHaveValidationError()
    {
        // Arrange
        CreatePlaylistCommand command = _createPlaylistCommandFixture.Create(type: (PlaylistType)999);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Playlists.PlaylistTypeIsInvalid);
    }

    [Fact]
    public void Validate_WhenCommandIsValid_ShouldNotHaveAnyValidationError()
    {
        // Arrange
        CreatePlaylistCommand command = _createPlaylistCommandFixture.Create();

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }
}
