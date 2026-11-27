#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.Mapping.Playlists;
using Lumina.Application.Core.Playlists.Commands.UpdatePlaylist;
using Lumina.Contracts.Fixtures.Core.Requests.Playlists;
using Lumina.Contracts.Requests.Playlists;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Application.UnitTests.Common.Mapping.Playlists;

/// <summary>
/// Contains unit tests for the <see cref="UpdatePlaylistRequestMapping"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class UpdatePlaylistRequestMappingTests
{
    private readonly UpdatePlaylistRequestFixture _updatePlaylistRequestFixture = new();

    [Fact]
    public void ToCommand_WhenEveryMemberIsSupplied_ShouldPreserveThem()
    {
        // Arrange
        UpdatePlaylistRequest request = _updatePlaylistRequestFixture.Create(
            name: "Deep Focus",
            description: "Long sessions");

        // Act
        UpdatePlaylistCommand command = request.ToCommand("playlist-id");

        // Assert
        Assert.Equal("playlist-id", command.PlaylistId);
        Assert.Equal("Deep Focus", command.Name);
        Assert.Equal("Long sessions", command.Description);
    }

    [Fact]
    public void ToCommand_WhenTheOptionalMembersAreOmitted_ShouldPreserveThemAsNull()
    {
        // Arrange
        UpdatePlaylistRequest request = _updatePlaylistRequestFixture.Create(
            name: "Deep Focus",
            includeDescription: false);

        // Act
        UpdatePlaylistCommand command = request.ToCommand("playlist-id");

        // Assert
        Assert.Equal("Deep Focus", command.Name);
        Assert.Null(command.Description);
    }

    [Fact]
    public void ToCommand_WhenThePlaylistIdIsNull_ShouldPreserveNull()
    {
        // Arrange
        UpdatePlaylistRequest request = _updatePlaylistRequestFixture.Create();

        // Act
        UpdatePlaylistCommand command = request.ToCommand(null);

        // Assert
        Assert.Null(command.PlaylistId);
        Assert.NotNull(command.Name);
    }

    [Fact]
    public void ToCommand_WhenThePlaylistIdIsEmpty_ShouldPreserveEmpty()
    {
        // Arrange
        UpdatePlaylistRequest request = _updatePlaylistRequestFixture.Create();

        // Act
        UpdatePlaylistCommand command = request.ToCommand(string.Empty);

        // Assert
        Assert.Equal(string.Empty, command.PlaylistId);
    }
}
