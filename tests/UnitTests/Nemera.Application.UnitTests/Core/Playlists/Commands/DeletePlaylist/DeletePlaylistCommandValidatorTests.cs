#region ========================================================================= USING =====================================================================================
using Nemera.Application.Core.Playlists.Commands.DeletePlaylist;
using Nemera.Application.Fixtures.Core.Playlists.Commands.DeletePlaylist;
using Nemera.Application.UnitTests.Common.Setup;
using Nemera.Domain.Common.Errors;
using Nemera.Domain.Common.Primitives;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Nemera.Application.UnitTests.Core.Playlists.Commands.DeletePlaylist;

/// <summary>
/// Contains unit tests for the <see cref="DeletePlaylistCommandValidator"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class DeletePlaylistCommandValidatorTests
{
    private readonly DeletePlaylistCommandValidator _validator = new();
    private readonly DeletePlaylistCommandFixture _deletePlaylistCommandFixture = new();

    [Fact]
    public void Validate_WhenPlaylistIdIsNotSupplied_ShouldHaveValidationError()
    {
        // Arrange
        DeletePlaylistCommand command = _deletePlaylistCommandFixture.Create(includePlaylistId: false);

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
        DeletePlaylistCommand command = _deletePlaylistCommandFixture.Create(playlistId: playlistId);

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationError(Errors.Playlists.PlaylistIdCannotBeEmpty);
    }

    [Fact]
    public void Validate_WhenCommandIsValid_ShouldNotHaveAnyValidationError()
    {
        // Arrange
        DeletePlaylistCommand command = _deletePlaylistCommandFixture.Create();

        // Act
        List<Error> result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }
}
