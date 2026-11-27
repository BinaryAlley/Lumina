#region ========================================================================= USING =====================================================================================
using Nemera.Application.Core.Playlists.Commands.AddPlaylistEntry;
using Nemera.Application.Fixtures.Core.Playlists.Commands.AddPlaylistEntry;
using Nemera.Application.UnitTests.Common.Setup;
using Nemera.Domain.Common.Errors;
using Nemera.Domain.Common.Primitives;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Nemera.Application.UnitTests.Core.Playlists.Commands.AddPlaylistEntry;

/// <summary>
/// Contains unit tests for the <see cref="AddPlaylistEntryCommandValidator"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class AddPlaylistEntryCommandValidatorTests
{
    private readonly AddPlaylistEntryCommandValidator _validator = new();
    private readonly AddPlaylistEntryCommandFixture _addPlaylistEntryCommandFixture = new();

    [Fact]
    public void Validate_WhenPlaylistIdIsNotSupplied_ShouldHaveValidationError()
    {
        // Arrange
        AddPlaylistEntryCommand command = _addPlaylistEntryCommandFixture.Create(includePlaylistId: false);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Playlists.PlaylistIdCannotBeEmpty);
    }

    [Theory]
    [InlineData("")] // An empty identifier.
    [InlineData("   ")] // A white-space only identifier.
    [InlineData("not-a-guid")] // An identifier that cannot be parsed.
    [InlineData("00000000-0000-0000-0000-000000000000")] // The empty Guid, which is never a valid playlist.
    public void Validate_WhenPlaylistIdIsNotAValidIdentifier_ShouldHaveValidationError(string playlistId)
    {
        // Arrange
        AddPlaylistEntryCommand command = _addPlaylistEntryCommandFixture.Create(playlistId: playlistId);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Playlists.PlaylistIdCannotBeEmpty);
    }

    [Fact]
    public void Validate_WhenTrackIdIsNotSupplied_ShouldHaveValidationError()
    {
        // Arrange
        AddPlaylistEntryCommand command = _addPlaylistEntryCommandFixture.Create(includeTrackId: false);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Music.TrackIdCannotBeEmpty);
    }

    [Fact]
    public void Validate_WhenTrackIdIsTheEmptyGuid_ShouldHaveValidationError()
    {
        // Arrange
        AddPlaylistEntryCommand command = _addPlaylistEntryCommandFixture.Create(trackId: Guid.Empty);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Music.TrackIdCannotBeEmpty);
    }

    [Fact]
    public void Validate_WhenCommandIsValid_ShouldNotHaveAnyValidationError()
    {
        // Arrange
        AddPlaylistEntryCommand command = _addPlaylistEntryCommandFixture.Create();

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }
}
