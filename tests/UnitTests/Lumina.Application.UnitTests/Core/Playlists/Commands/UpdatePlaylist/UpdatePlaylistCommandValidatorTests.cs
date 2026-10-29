#region ========================================================================= USING =====================================================================================
using Lumina.Application.Core.Playlists.Commands.UpdatePlaylist;
using Lumina.Application.Fixtures.Core.Playlists.Commands.UpdatePlaylist;
using Lumina.Application.UnitTests.Common.Setup;
using Lumina.Domain.Common.Errors;
using Lumina.Domain.Common.Primitives;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Application.UnitTests.Core.Playlists.Commands.UpdatePlaylist;

/// <summary>
/// Contains unit tests for the <see cref="UpdatePlaylistCommandValidator"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class UpdatePlaylistCommandValidatorTests
{
    private const int MAXIMUM_PLAYLIST_NAME_LENGTH = 255;

    private readonly UpdatePlaylistCommandValidator _validator = new();
    private readonly UpdatePlaylistCommandFixture _updatePlaylistCommandFixture = new();

    [Fact]
    public void Validate_WhenPlaylistIdIsNotSupplied_ShouldHaveValidationError()
    {
        // Arrange
        UpdatePlaylistCommand command = _updatePlaylistCommandFixture.Create(includePlaylistId: false);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Playlists.PlaylistIdCannotBeEmpty);
    }

    [Theory]
    [InlineData("")] // an empty identifier
    [InlineData("   ")] // a white-space only identifier
    [InlineData("not-a-guid")] // an identifier that cannot be parsed
    [InlineData("00000000-0000-0000-0000-000000000000")] // the empty Guid, which is never a valid playlist
    public void Validate_WhenPlaylistIdIsNotAValidIdentifier_ShouldHaveValidationError(string playlistId)
    {
        // Arrange
        UpdatePlaylistCommand command = _updatePlaylistCommandFixture.Create(playlistId: playlistId);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Playlists.PlaylistIdCannotBeEmpty);
    }

    [Fact]
    public void Validate_WhenNameIsNotSupplied_ShouldHaveValidationError()
    {
        // Arrange
        UpdatePlaylistCommand command = _updatePlaylistCommandFixture.Create(includeName: false);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Playlists.PlaylistNameCannotBeEmpty);
    }

    [Fact]
    public void Validate_WhenNameIsEmpty_ShouldHaveValidationError()
    {
        // Arrange
        UpdatePlaylistCommand command = _updatePlaylistCommandFixture.Create(name: string.Empty);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Playlists.PlaylistNameCannotBeEmpty);
    }

    [Fact]
    public void Validate_WhenNameIsWhiteSpaceOnly_ShouldHaveValidationError()
    {
        // Arrange
        UpdatePlaylistCommand command = _updatePlaylistCommandFixture.Create(name: "   ");

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Playlists.PlaylistNameCannotBeEmpty);
    }

    [Fact]
    public void Validate_WhenNameIsLongerThanTheMaximumLength_ShouldHaveValidationError()
    {
        // Arrange
        UpdatePlaylistCommand command = _updatePlaylistCommandFixture.Create(
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
        UpdatePlaylistCommand command = _updatePlaylistCommandFixture.Create(
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
        UpdatePlaylistCommand command = _updatePlaylistCommandFixture.Create(description: new string('a', 2001));

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Playlists.PlaylistDescriptionMustBeMaximum2000CharactersLong);
    }

    [Fact]
    public void Validate_WhenDescriptionIsOmitted_ShouldNotHaveValidationError()
    {
        // Arrange
        UpdatePlaylistCommand command = _updatePlaylistCommandFixture.Create(includeDescription: false);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationError(Errors.Playlists.PlaylistDescriptionMustBeMaximum2000CharactersLong);
    }

    [Fact]
    public void Validate_WhenCommandIsValid_ShouldNotHaveAnyValidationError()
    {
        // Arrange
        UpdatePlaylistCommand command = _updatePlaylistCommandFixture.Create();

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }
}
