#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.Mapping.Playlists;
using Lumina.Application.Core.Playlists.Commands.AddPlaylistEntry;
using Lumina.Contracts.Fixtures.Core.Requests.Playlists;
using Lumina.Contracts.Requests.Playlists;
using System;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Application.UnitTests.Common.Mapping.Playlists;

/// <summary>
/// Contains unit tests for the <see cref="AddPlaylistEntryRequestMapping"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class AddPlaylistEntryRequestMappingTests
{
    private readonly AddPlaylistEntryRequestFixture _addPlaylistEntryRequestFixture = new();

    [Fact]
    public void ToCommand_WhenEveryMemberIsSupplied_ShouldPreserveThem()
    {
        // Arrange
        Guid trackId = Guid.NewGuid();
        AddPlaylistEntryRequest request = _addPlaylistEntryRequestFixture.Create(trackId: trackId);

        // Act
        AddPlaylistEntryCommand command = request.ToCommand("playlist-id");

        // Assert
        Assert.Equal("playlist-id", command.PlaylistId);
        Assert.Equal(trackId, command.TrackId);
    }

    [Fact]
    public void ToCommand_WhenTrackIdIsNull_ShouldPreserveNull()
    {
        // Arrange
        AddPlaylistEntryRequest request = _addPlaylistEntryRequestFixture.Create(includeTrackId: false);

        // Act
        AddPlaylistEntryCommand command = request.ToCommand("playlist-id");

        // Assert
        Assert.Null(command.TrackId);
        Assert.Equal("playlist-id", command.PlaylistId);
    }

    [Fact]
    public void ToCommand_WhenPlaylistIdIsNull_ShouldPreserveNull()
    {
        // Arrange
        AddPlaylistEntryRequest request = _addPlaylistEntryRequestFixture.Create();

        // Act
        AddPlaylistEntryCommand command = request.ToCommand(null);

        // Assert
        Assert.Null(command.PlaylistId);
        Assert.NotNull(command.TrackId);
    }

    [Fact]
    public void ToCommand_WhenPlaylistIdIsEmpty_ShouldPreserveEmpty()
    {
        // Arrange
        AddPlaylistEntryRequest request = _addPlaylistEntryRequestFixture.Create();

        // Act
        AddPlaylistEntryCommand command = request.ToCommand(string.Empty);

        // Assert
        Assert.Equal(string.Empty, command.PlaylistId);
    }
}
