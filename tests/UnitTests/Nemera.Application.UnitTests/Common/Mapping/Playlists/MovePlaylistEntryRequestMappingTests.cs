#region ========================================================================= USING =====================================================================================
using Nemera.Application.Common.Mapping.Playlists;
using Nemera.Application.Core.Playlists.Commands.MovePlaylistEntry;
using Nemera.Contracts.Fixtures.Core.Requests.Playlists;
using Nemera.Contracts.Requests.Playlists;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Nemera.Application.UnitTests.Common.Mapping.Playlists;

/// <summary>
/// Contains unit tests for the <see cref="MovePlaylistEntryRequestMapping"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class MovePlaylistEntryRequestMappingTests
{
    private readonly MovePlaylistEntryRequestFixture _movePlaylistEntryRequestFixture = new();

    [Fact]
    public void ToCommand_WhenEveryMemberIsSupplied_ShouldPreserveThem()
    {
        // Arrange
        MovePlaylistEntryRequest request = _movePlaylistEntryRequestFixture.Create(targetPosition: 7);

        // Act
        MovePlaylistEntryCommand command = request.ToCommand("playlist-id", "entry-id");

        // Assert
        Assert.Equal("playlist-id", command.PlaylistId);
        Assert.Equal("entry-id", command.PlaylistEntryId);
        Assert.Equal(7, command.TargetPosition);
    }

    [Fact]
    public void ToCommand_WhenTargetPositionIsNull_ShouldPreserveNull()
    {
        // Arrange
        MovePlaylistEntryRequest request = _movePlaylistEntryRequestFixture.Create(includeTargetPosition: false);

        // Act
        MovePlaylistEntryCommand command = request.ToCommand("playlist-id", "entry-id");

        // Assert
        Assert.Null(command.TargetPosition);
        Assert.Equal("playlist-id", command.PlaylistId);
        Assert.Equal("entry-id", command.PlaylistEntryId);
    }

    [Fact]
    public void ToCommand_WhenTheRouteIdentifiersAreNull_ShouldPreserveNull()
    {
        // Arrange
        MovePlaylistEntryRequest request = _movePlaylistEntryRequestFixture.Create();

        // Act
        MovePlaylistEntryCommand command = request.ToCommand(null, null);

        // Assert
        Assert.Null(command.PlaylistId);
        Assert.Null(command.PlaylistEntryId);
        Assert.NotNull(command.TargetPosition);
    }

    [Fact]
    public void ToCommand_WhenTheRouteIdentifiersAreEmpty_ShouldPreserveEmpty()
    {
        // Arrange
        MovePlaylistEntryRequest request = _movePlaylistEntryRequestFixture.Create();

        // Act
        MovePlaylistEntryCommand command = request.ToCommand(string.Empty, string.Empty);

        // Assert
        Assert.Equal(string.Empty, command.PlaylistId);
        Assert.Equal(string.Empty, command.PlaylistEntryId);
    }
}
