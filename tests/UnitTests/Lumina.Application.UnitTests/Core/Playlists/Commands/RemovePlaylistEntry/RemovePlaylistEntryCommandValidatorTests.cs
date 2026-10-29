#region ========================================================================= USING =====================================================================================
using Lumina.Application.Core.Playlists.Commands.RemovePlaylistEntry;
using Lumina.Application.Fixtures.Core.Playlists.Commands.RemovePlaylistEntry;
using Lumina.Application.UnitTests.Common.Setup;
using Lumina.Domain.Common.Errors;
using Lumina.Domain.Common.Primitives;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Application.UnitTests.Core.Playlists.Commands.RemovePlaylistEntry;

/// <summary>
/// Contains unit tests for the <see cref="RemovePlaylistEntryCommandValidator"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class RemovePlaylistEntryCommandValidatorTests
{
    private readonly RemovePlaylistEntryCommandValidator _validator = new();
    private readonly RemovePlaylistEntryCommandFixture _removePlaylistEntryCommandFixture = new();

    [Fact]
    public void Validate_WhenPlaylistIdIsNotSupplied_ShouldHaveValidationError()
    {
        // Arrange
        RemovePlaylistEntryCommand command = _removePlaylistEntryCommandFixture.Create(includePlaylistId: false);

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
        RemovePlaylistEntryCommand command = _removePlaylistEntryCommandFixture.Create(playlistId: playlistId);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Playlists.PlaylistIdCannotBeEmpty);
    }

    [Fact]
    public void Validate_WhenPlaylistEntryIdIsNotSupplied_ShouldHaveValidationError()
    {
        // Arrange
        RemovePlaylistEntryCommand command = _removePlaylistEntryCommandFixture.Create(includePlaylistEntryId: false);

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
        RemovePlaylistEntryCommand command = _removePlaylistEntryCommandFixture.Create(playlistEntryId: playlistEntryId);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Playlists.PlaylistEntryIdCannotBeEmpty);
    }

    [Fact]
    public void Validate_WhenCommandIsValid_ShouldNotHaveAnyValidationError()
    {
        // Arrange
        RemovePlaylistEntryCommand command = _removePlaylistEntryCommandFixture.Create();

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }
}
