#region ========================================================================= USING =====================================================================================
using Lumina.Application.Core.Playlists.Commands.AddPlaylistScopeEntries;
using Lumina.Application.Fixtures.Core.Playlists.Commands.AddPlaylistScopeEntries;
using Lumina.Application.UnitTests.Common.Setup;
using Lumina.Domain.Common.Errors;
using Lumina.Domain.Common.Primitives;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Application.UnitTests.Core.Playlists.Commands.AddPlaylistScopeEntries;

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
    [InlineData("")] // an empty identifier
    [InlineData("   ")] // a white-space only identifier
    [InlineData("not-a-guid")] // an identifier that cannot be parsed
    [InlineData("00000000-0000-0000-0000-000000000000")] // the empty Guid, which is never a valid playlist
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
