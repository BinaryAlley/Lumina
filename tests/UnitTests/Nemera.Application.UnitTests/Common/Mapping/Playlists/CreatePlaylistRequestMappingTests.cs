#region ========================================================================= USING =====================================================================================
using Nemera.Application.Common.Mapping.Playlists;
using Nemera.Application.Core.Playlists.Commands.CreatePlaylist;
using Nemera.Contracts.Fixtures.Core.Requests.Playlists;
using Nemera.Contracts.Requests.Playlists;
using Nemera.Domain.SharedKernel.Common.Enums.Playlists;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Nemera.Application.UnitTests.Common.Mapping.Playlists;

/// <summary>
/// Contains unit tests for the <see cref="CreatePlaylistRequestMapping"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class CreatePlaylistRequestMappingTests
{
    private readonly CreatePlaylistRequestFixture _createPlaylistRequestFixture = new();

    [Fact]
    public void ToCommand_WhenEveryMemberIsSupplied_ShouldPreserveThem()
    {
        // Arrange
        CreatePlaylistRequest request = _createPlaylistRequestFixture.Create(
            name: "Focus",
            description: "Deep work",
            type: PlaylistType.Music);

        // Act
        CreatePlaylistCommand command = request.ToCommand();

        // Assert
        Assert.Equal("Focus", command.Name);
        Assert.Equal("Deep work", command.Description);
        Assert.Equal(PlaylistType.Music, command.Type);
    }

    [Fact]
    public void ToCommand_WhenTheOptionalMembersAreOmitted_ShouldPreserveThemAsNull()
    {
        // Arrange
        CreatePlaylistRequest request = _createPlaylistRequestFixture.Create(
            name: "Focus",
            includeDescription: false,
            includeType: false);

        // Act
        CreatePlaylistCommand command = request.ToCommand();

        // Assert
        Assert.Equal("Focus", command.Name);
        Assert.Null(command.Description);
        Assert.Null(command.Type);
    }

    [Fact]
    public void ToCommand_WhenTheNameIsNull_ShouldPreserveNull()
    {
        // Arrange
        CreatePlaylistRequest request = _createPlaylistRequestFixture.Create(includeName: false);

        // Act
        CreatePlaylistCommand command = request.ToCommand();

        // Assert
        Assert.Null(command.Name);
    }
}
