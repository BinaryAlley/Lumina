#region ========================================================================= USING =====================================================================================
using Lumina.Application.Core.Playlists.Commands.DeletePlaylist;
using Lumina.Application.Fixtures.Core.Playlists.Commands.DeletePlaylist;
using Lumina.Application.UnitTests.Common.Setup;
using Lumina.Domain.Common.Errors;
using Lumina.Domain.Common.Primitives;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Application.UnitTests.Core.Playlists.Commands.DeletePlaylist;

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
    [InlineData("")] // an empty identifier
    [InlineData("   ")] // a white-space only identifier
    [InlineData("not-a-guid")] // an identifier that cannot be parsed
    [InlineData("00000000-0000-0000-0000-000000000000")] // the empty Guid, which is never a valid playlist
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
