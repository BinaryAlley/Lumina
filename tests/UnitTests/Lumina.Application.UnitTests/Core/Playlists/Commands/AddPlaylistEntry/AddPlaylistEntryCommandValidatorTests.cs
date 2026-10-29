#region ========================================================================= USING =====================================================================================
using Lumina.Application.Core.Playlists.Commands.AddPlaylistEntry;
using Lumina.Application.Fixtures.Core.Playlists.Commands.AddPlaylistEntry;
using Lumina.Application.UnitTests.Common.Setup;
using Lumina.Domain.Common.Errors;
using Lumina.Domain.Common.Primitives;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Application.UnitTests.Core.Playlists.Commands.AddPlaylistEntry;

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
    [InlineData("")] // an empty identifier
    [InlineData("   ")] // a white-space only identifier
    [InlineData("not-a-guid")] // an identifier that cannot be parsed
    [InlineData("00000000-0000-0000-0000-000000000000")] // the empty Guid, which is never a valid playlist
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
