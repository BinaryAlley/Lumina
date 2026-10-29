#region ========================================================================= USING =====================================================================================
using Lumina.Application.Core.Playlists.Commands.AddPlaylistEntries;
using Lumina.Application.Fixtures.Core.Playlists.Commands.AddPlaylistEntries;
using Lumina.Application.UnitTests.Common.Setup;
using Lumina.Domain.Common.Errors;
using Lumina.Domain.Common.Primitives;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Application.UnitTests.Core.Playlists.Commands.AddPlaylistEntries;

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
    [InlineData("")] // an empty identifier
    [InlineData("   ")] // a white-space only identifier
    [InlineData("not-a-guid")] // an identifier that cannot be parsed
    [InlineData("00000000-0000-0000-0000-000000000000")] // the empty Guid, which is never a valid playlist
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
