#region ========================================================================= USING =====================================================================================
using Lumina.Application.Core.Playlists.Commands.MovePlaylistEntry;
using Lumina.Application.Fixtures.Core.Playlists.Commands.MovePlaylistEntry;
using Lumina.Application.UnitTests.Common.Setup;
using Lumina.Domain.Common.Errors;
using Lumina.Domain.Common.Primitives;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Application.UnitTests.Core.Playlists.Commands.MovePlaylistEntry;

/// <summary>
/// Contains unit tests for the <see cref="MovePlaylistEntryCommandValidator"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class MovePlaylistEntryCommandValidatorTests
{
    private readonly MovePlaylistEntryCommandValidator _validator = new();
    private readonly MovePlaylistEntryCommandFixture _movePlaylistEntryCommandFixture = new();

    [Fact]
    public void Validate_WhenPlaylistIdIsNotSupplied_ShouldHaveValidationError()
    {
        // Arrange
        MovePlaylistEntryCommand command = _movePlaylistEntryCommandFixture.Create(includePlaylistId: false);

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
        MovePlaylistEntryCommand command = _movePlaylistEntryCommandFixture.Create(playlistId: playlistId);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Playlists.PlaylistIdCannotBeEmpty);
    }

    [Fact]
    public void Validate_WhenPlaylistEntryIdIsNotSupplied_ShouldHaveValidationError()
    {
        // Arrange
        MovePlaylistEntryCommand command = _movePlaylistEntryCommandFixture.Create(includePlaylistEntryId: false);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Playlists.PlaylistEntryIdCannotBeEmpty);
    }

    [Theory]
    [InlineData("")] // an empty identifier
    [InlineData("   ")] // a white-space only identifier
    [InlineData("not-a-guid")] // an identifier that cannot be parsed
    [InlineData("00000000-0000-0000-0000-000000000000")] // the empty Guid, which is never a valid entry
    public void Validate_WhenPlaylistEntryIdIsNotAValidIdentifier_ShouldHaveValidationError(string playlistEntryId)
    {
        // Arrange
        MovePlaylistEntryCommand command = _movePlaylistEntryCommandFixture.Create(playlistEntryId: playlistEntryId);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Playlists.PlaylistEntryIdCannotBeEmpty);
    }

    [Fact]
    public void Validate_WhenTargetPositionIsNotSupplied_ShouldHaveValidationError()
    {
        // Arrange
        MovePlaylistEntryCommand command = _movePlaylistEntryCommandFixture.Create(includeTargetPosition: false);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Playlists.PlaylistMoveTargetPositionIsInvalid);
    }

    [Fact]
    public void Validate_WhenTargetPositionIsNegative_ShouldHaveValidationError()
    {
        // Arrange
        MovePlaylistEntryCommand command = _movePlaylistEntryCommandFixture.Create(targetPosition: -1);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Playlists.PlaylistMoveTargetPositionIsInvalid);
    }

    [Fact]
    public void Validate_WhenTargetPositionIsZero_ShouldNotHaveValidationError()
    {
        // Arrange
        MovePlaylistEntryCommand command = _movePlaylistEntryCommandFixture.Create(targetPosition: 0);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationError(Errors.Playlists.PlaylistMoveTargetPositionIsInvalid);
    }

    [Fact]
    public void Validate_WhenCommandIsValid_ShouldNotHaveAnyValidationError()
    {
        // Arrange
        MovePlaylistEntryCommand command = _movePlaylistEntryCommandFixture.Create();

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }
}
