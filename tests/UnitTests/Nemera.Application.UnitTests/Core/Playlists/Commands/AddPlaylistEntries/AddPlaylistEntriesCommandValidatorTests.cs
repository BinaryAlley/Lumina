#region ========================================================================= USING =====================================================================================
using Nemera.Application.Core.Playlists.Commands.AddPlaylistEntries;
using Nemera.Application.Fixtures.Core.Playlists.Commands.AddPlaylistEntries;
using Nemera.Application.UnitTests.Common.Setup;
using Nemera.Domain.Common.Errors;
using Nemera.Domain.Common.Primitives;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Nemera.Application.UnitTests.Core.Playlists.Commands.AddPlaylistEntries;

/// <summary>
/// Contains unit tests for the <see cref="AddPlaylistEntriesCommandValidator"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class AddPlaylistEntriesCommandValidatorTests
{
    private readonly AddPlaylistEntriesCommandValidator _validator = new();
    private readonly AddPlaylistEntriesCommandFixture _addPlaylistEntriesCommandFixture = new();

    [Fact]
    public void Validate_WhenPlaylistIdIsNotSupplied_ShouldHaveValidationError()
    {
        // Arrange
        AddPlaylistEntriesCommand command = _addPlaylistEntriesCommandFixture.Create(includePlaylistId: false);

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
        AddPlaylistEntriesCommand command = _addPlaylistEntriesCommandFixture.Create(playlistId: playlistId);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Playlists.PlaylistIdCannotBeEmpty);
    }

    [Fact]
    public void Validate_WhenTrackIdsAreNotSupplied_ShouldHaveValidationError()
    {
        // Arrange
        AddPlaylistEntriesCommand command = _addPlaylistEntriesCommandFixture.Create(includeTrackIds: false);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Music.TrackIdCannotBeEmpty);
    }

    [Fact]
    public void Validate_WhenTrackIdsAreEmpty_ShouldHaveValidationError()
    {
        // Arrange
        AddPlaylistEntriesCommand command = _addPlaylistEntriesCommandFixture.Create(trackIds: []);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Music.TrackIdCannotBeEmpty);
    }

    [Fact]
    public void Validate_WhenOneTrackIdIsTheEmptyGuid_ShouldHaveValidationError()
    {
        // Arrange
        AddPlaylistEntriesCommand command = _addPlaylistEntriesCommandFixture.Create(
            trackIds: [Guid.NewGuid(), Guid.Empty]);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Music.TrackIdCannotBeEmpty);
    }

    [Fact]
    public void Validate_WhenARepeatedTrackIsRequested_ShouldNotHaveAnyValidationError()
    {
        // Arrange
        Guid repeatedTrackId = Guid.NewGuid();
        AddPlaylistEntriesCommand command = _addPlaylistEntriesCommandFixture.Create(
            trackIds: [repeatedTrackId, repeatedTrackId]);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_WhenCommandIsValid_ShouldNotHaveAnyValidationError()
    {
        // Arrange
        AddPlaylistEntriesCommand command = _addPlaylistEntriesCommandFixture.Create();

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }
}
