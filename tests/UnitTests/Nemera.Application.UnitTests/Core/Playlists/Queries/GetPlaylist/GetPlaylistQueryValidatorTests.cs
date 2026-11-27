#region ========================================================================= USING =====================================================================================
using Nemera.Application.Core.Playlists.Queries.GetPlaylist;
using Nemera.Application.Fixtures.Core.Playlists.Queries.GetPlaylist;
using Nemera.Application.UnitTests.Common.Setup;
using Nemera.Domain.Common.Errors;
using Nemera.Domain.Common.Primitives;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Nemera.Application.UnitTests.Core.Playlists.Queries.GetPlaylist;

/// <summary>
/// Contains unit tests for the <see cref="GetPlaylistQueryValidator"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetPlaylistQueryValidatorTests
{
    private readonly GetPlaylistQueryValidator _validator = new();
    private readonly GetPlaylistQueryFixture _getPlaylistQueryFixture = new();

    [Fact]
    public void Validate_WhenPlaylistIdIsNotSupplied_ShouldHaveValidationError()
    {
        // Arrange
        GetPlaylistQuery query = _getPlaylistQueryFixture.Create(includePlaylistId: false);

        // Act
        List<Error> result = _validator.TestValidate(query);

        // Assert
        result.ShouldHaveValidationError(Errors.Playlists.PlaylistIdCannotBeEmpty);
    }

    [Theory]
    [InlineData("")] // The playlist Id is empty.
    [InlineData("   ")] // The playlist Id is whitespace only.
    [InlineData("not-a-guid")] // The playlist Id is not a Guid.
    [InlineData("00000000-0000-0000-0000-000000000000")] // The playlist Id is the empty Guid.
    public void Validate_WhenPlaylistIdIsEmptyOrInvalid_ShouldHaveValidationError(string playlistId)
    {
        // Arrange
        GetPlaylistQuery query = _getPlaylistQueryFixture.Create(playlistId: playlistId);

        // Act
        List<Error> result = _validator.TestValidate(query);

        // Assert
        result.ShouldHaveValidationError(Errors.Playlists.PlaylistIdCannotBeEmpty);
    }

    [Fact]
    public void Validate_WhenPlaylistIdIsAValidGuid_ShouldNotHaveAnyValidationError()
    {
        // Arrange
        GetPlaylistQuery query = _getPlaylistQueryFixture.Create(playlistId: Guid.NewGuid().ToString());

        // Act
        List<Error> result = _validator.TestValidate(query);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }
}
