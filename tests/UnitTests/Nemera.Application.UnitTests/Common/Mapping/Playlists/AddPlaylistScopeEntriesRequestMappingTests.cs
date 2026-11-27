#region ========================================================================= USING =====================================================================================
using Nemera.Application.Common.Mapping.Playlists;
using Nemera.Application.Core.Playlists.Commands.AddPlaylistScopeEntries;
using Nemera.Contracts.Fixtures.Core.Requests.Playlists;
using Nemera.Contracts.Requests.Playlists;
using System;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Nemera.Application.UnitTests.Common.Mapping.Playlists;

/// <summary>
/// Contains unit tests for the <see cref="AddPlaylistScopeEntriesRequestMapping"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class AddPlaylistScopeEntriesRequestMappingTests
{
    private readonly AddPlaylistScopeEntriesRequestFixture _addPlaylistScopeEntriesRequestFixture = new();

    [Fact]
    public void ToCommand_WhenEveryMemberIsSupplied_ShouldPreserveThem()
    {
        // Arrange
        Guid libraryId = Guid.NewGuid();
        Guid artistId = Guid.NewGuid();
        Guid albumId = Guid.NewGuid();
        AddPlaylistScopeEntriesRequest request = _addPlaylistScopeEntriesRequestFixture.Create(
            libraryId: libraryId,
            artistId: artistId,
            albumId: albumId,
            includeArtistId: true,
            includeAlbumId: true);

        // Act
        AddPlaylistScopeEntriesCommand command = request.ToCommand("playlist-id");

        // Assert
        Assert.Equal("playlist-id", command.PlaylistId);
        Assert.Equal(libraryId, command.LibraryId);
        Assert.Equal(artistId, command.ArtistId);
        Assert.Equal(albumId, command.AlbumId);
    }

    [Fact]
    public void ToCommand_WhenArtistAndAlbumAreOmitted_ShouldPreserveThemAsNull()
    {
        // Arrange
        Guid libraryId = Guid.NewGuid();
        AddPlaylistScopeEntriesRequest request = _addPlaylistScopeEntriesRequestFixture.Create(libraryId: libraryId);

        // Act
        AddPlaylistScopeEntriesCommand command = request.ToCommand("playlist-id");

        // Assert
        Assert.Equal(libraryId, command.LibraryId);
        Assert.Null(command.ArtistId);
        Assert.Null(command.AlbumId);
    }

    [Fact]
    public void ToCommand_WhenPlaylistIdIsNull_ShouldPreserveNull()
    {
        // Arrange
        AddPlaylistScopeEntriesRequest request = _addPlaylistScopeEntriesRequestFixture.Create();

        // Act
        AddPlaylistScopeEntriesCommand command = request.ToCommand(null);

        // Assert
        Assert.Null(command.PlaylistId);
        Assert.NotNull(command.LibraryId);
    }

    [Fact]
    public void ToCommand_WhenPlaylistIdIsEmpty_ShouldPreserveEmpty()
    {
        // Arrange
        AddPlaylistScopeEntriesRequest request = _addPlaylistScopeEntriesRequestFixture.Create();

        // Act
        AddPlaylistScopeEntriesCommand command = request.ToCommand(string.Empty);

        // Assert
        Assert.Equal(string.Empty, command.PlaylistId);
    }
}
