#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.Mapping.Playlists;
using Lumina.Application.Core.Playlists.Commands.AddPlaylistEntries;
using Lumina.Contracts.Fixtures.Core.Requests.Playlists;
using Lumina.Contracts.Requests.Playlists;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Application.UnitTests.Common.Mapping.Playlists;

/// <summary>
/// Contains unit tests for the <see cref="AddPlaylistEntriesRequestMapping"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class AddPlaylistEntriesRequestMappingTests
{
    private readonly AddPlaylistEntriesRequestFixture _addPlaylistEntriesRequestFixture = new();

    [Fact]
    public void ToCommand_WhenEveryMemberIsSupplied_ShouldPreserveTheTrackOrderAndThePlaylistId()
    {
        // Arrange
        List<Guid> trackIds = [Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid()];
        AddPlaylistEntriesRequest request = _addPlaylistEntriesRequestFixture.Create(trackIds: trackIds);

        // Act
        AddPlaylistEntriesCommand command = request.ToCommand("playlist-id");

        // Assert
        Assert.Equal("playlist-id", command.PlaylistId);
        Assert.NotNull(command.TrackIds);
        Assert.Equal(trackIds, command.TrackIds);
    }

    [Fact]
    public void ToCommand_WhenTrackIdsAreNull_ShouldPreserveNull()
    {
        // Arrange
        AddPlaylistEntriesRequest request = _addPlaylistEntriesRequestFixture.Create(includeTrackIds: false);

        // Act
        AddPlaylistEntriesCommand command = request.ToCommand("playlist-id");

        // Assert
        Assert.Null(command.TrackIds);
        Assert.Equal("playlist-id", command.PlaylistId);
    }

    [Fact]
    public void ToCommand_WhenPlaylistIdIsNull_ShouldPreserveNull()
    {
        // Arrange
        AddPlaylistEntriesRequest request = _addPlaylistEntriesRequestFixture.Create();

        // Act
        AddPlaylistEntriesCommand command = request.ToCommand(null);

        // Assert
        Assert.Null(command.PlaylistId);
        Assert.NotNull(command.TrackIds);
    }

    [Fact]
    public void ToCommand_WhenPlaylistIdIsEmpty_ShouldPreserveEmpty()
    {
        // Arrange
        AddPlaylistEntriesRequest request = _addPlaylistEntriesRequestFixture.Create();

        // Act
        AddPlaylistEntriesCommand command = request.ToCommand(string.Empty);

        // Assert
        Assert.Equal(string.Empty, command.PlaylistId);
    }
}
