#region ========================================================================= USING =====================================================================================
using Lumina.Application.Core.Playlists.Queries.GetPlaylist;
using Lumina.Application.Fixtures.Core.Playlists.Queries.GetPlaylist;
using Lumina.Application.UnitTests.Common.Setup;
using Lumina.Domain.Common.Errors;
using Lumina.Domain.Common.Primitives;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Application.UnitTests.Core.Playlists.Queries.GetPlaylist;

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
    [InlineData("")] // the playlist Id is empty
    [InlineData("   ")] // the playlist Id is whitespace only
    [InlineData("not-a-guid")] // the playlist Id is not a Guid
    [InlineData("00000000-0000-0000-0000-000000000000")] // the playlist Id is the empty Guid
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
