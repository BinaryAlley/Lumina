#region ========================================================================= USING =====================================================================================
using Nemera.Application.Core.Playlists.Commands.AddPlaylistScopeEntries;
using Nemera.Application.Fixtures.Core.Playlists.Commands.AddPlaylistScopeEntries;
using Nemera.Application.UnitTests.Common.Setup;
using Nemera.Domain.Common.Errors;
using Nemera.Domain.Common.Primitives;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Nemera.Application.UnitTests.Core.Playlists.Commands.AddPlaylistScopeEntries;

/// <summary>
/// Contains unit tests for the <see cref="AddPlaylistScopeEntriesCommandValidator"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class AddPlaylistScopeEntriesCommandValidatorTests
{
    private readonly AddPlaylistScopeEntriesCommandValidator _validator = new();
    private readonly AddPlaylistScopeEntriesCommandFixture _addPlaylistScopeEntriesCommandFixture = new();

    [Fact]
    public void Validate_WhenPlaylistIdIsNotSupplied_ShouldHaveValidationError()
    {
        // Arrange
        AddPlaylistScopeEntriesCommand command = _addPlaylistScopeEntriesCommandFixture.Create(includePlaylistId: false);

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
        AddPlaylistScopeEntriesCommand command = _addPlaylistScopeEntriesCommandFixture.Create(playlistId: playlistId);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Playlists.PlaylistIdCannotBeEmpty);
    }

    [Fact]
    public void Validate_WhenLibraryIdIsNotSupplied_ShouldHaveValidationError()
    {
        // Arrange
        AddPlaylistScopeEntriesCommand command = _addPlaylistScopeEntriesCommandFixture.Create(includeLibraryId: false);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Library.LibraryIdCannotBeEmpty);
    }

    [Fact]
    public void Validate_WhenLibraryIdIsTheEmptyGuid_ShouldHaveValidationError()
    {
        // Arrange
        AddPlaylistScopeEntriesCommand command = _addPlaylistScopeEntriesCommandFixture.Create(libraryId: Guid.Empty);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Library.LibraryIdCannotBeEmpty);
    }

    [Fact]
    public void Validate_WhenArtistIdIsTheEmptyGuid_ShouldHaveValidationError()
    {
        // Arrange
        AddPlaylistScopeEntriesCommand command = _addPlaylistScopeEntriesCommandFixture.Create(
            artistId: Guid.Empty,
            includeArtistId: true);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Music.ArtistIdCannotBeEmpty);
    }

    [Fact]
    public void Validate_WhenAlbumIdIsTheEmptyGuid_ShouldHaveValidationError()
    {
        // Arrange
        AddPlaylistScopeEntriesCommand command = _addPlaylistScopeEntriesCommandFixture.Create(
            albumId: Guid.Empty,
            includeAlbumId: true);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Music.AlbumIdCannotBeEmpty);
    }

    [Fact]
    public void Validate_WhenArtistAndAlbumAreOmitted_ShouldNotHaveAnyValidationError()
    {
        // Arrange
        AddPlaylistScopeEntriesCommand command = _addPlaylistScopeEntriesCommandFixture.Create();

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_WhenCommandIsValid_ShouldNotHaveAnyValidationError()
    {
        // Arrange
        AddPlaylistScopeEntriesCommand command = _addPlaylistScopeEntriesCommandFixture.Create(
            includeArtistId: true,
            includeAlbumId: true);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }
}
