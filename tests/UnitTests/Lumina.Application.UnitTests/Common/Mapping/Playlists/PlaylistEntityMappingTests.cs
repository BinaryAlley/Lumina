#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.Playlists;
using Lumina.Application.Common.Mapping.Playlists;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.Playlists;
using Lumina.Domain.Common.Errors;
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.Core.BoundedContexts.PlaylistBoundedContext.PlaylistAggregate;
using Lumina.Domain.SharedKernel.Common.Enums.Playlists;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Application.UnitTests.Common.Mapping.Playlists;

/// <summary>
/// Contains unit tests for the <see cref="PlaylistEntityMapping"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class PlaylistEntityMappingTests
{
    private readonly PlaylistEntityFixture _playlistEntityFixture = new();
    private readonly PlaylistEntryEntityFixture _playlistEntryEntityFixture = new();

    [Fact]
    public void ToDomainEntity_WhenUpdatedOnUtcIsPresent_ShouldMapEveryField()
    {
        // Arrange
        Guid id = Guid.NewGuid();
        Guid userId = Guid.NewGuid();
        DateTime createdOnUtc = new(2024, 1, 2, 3, 4, 5, DateTimeKind.Utc);
        DateTime updatedOnUtc = new(2024, 2, 3, 4, 5, 6, DateTimeKind.Utc);
        PlaylistEntity entity = _playlistEntityFixture.Create(
            id: id,
            userId: userId,
            name: "Focus",
            description: "Deep work",
            type: PlaylistType.Music,
            createdOnUtc: createdOnUtc,
            includeUpdatedOnUtc: true,
            updatedOnUtc: updatedOnUtc);

        // Act
        Result<Playlist> result = entity.ToDomainEntity();

        // Assert
        Assert.False(result.IsFailure);
        Playlist playlist = result.Value;
        Assert.Equal(id, playlist.Id.Value);
        Assert.Equal(userId, playlist.UserId.Value);
        Assert.Equal("Focus", playlist.Name);
        Assert.True(playlist.Description.HasValue);
        Assert.Equal("Deep work", playlist.Description.Value);
        Assert.Equal(PlaylistType.Music, playlist.Type);
        Assert.Equal(createdOnUtc, playlist.CreatedOnUtc);
        Assert.True(playlist.UpdatedOnUtc.HasValue);
        Assert.Equal(updatedOnUtc, playlist.UpdatedOnUtc.Value);
        Assert.False(playlist.IsDeleted);
        Assert.Empty(playlist.Entries);
    }

    [Fact]
    public void ToDomainEntity_WhenTheDescriptionAndUpdatedOnUtcAreMissing_ShouldMapNoValuesForThem()
    {
        // Arrange
        PlaylistEntity entity = _playlistEntityFixture.Create(
            includeDescription: false,
            includeUpdatedOnUtc: false);

        // Act
        Result<Playlist> result = entity.ToDomainEntity();

        // Assert
        Assert.False(result.IsFailure);
        Assert.False(result.Value.Description.HasValue);
        Assert.False(result.Value.UpdatedOnUtc.HasValue);
    }

    [Fact]
    public void ToDomainEntity_WhenTheStoredEntriesAreOutOfOrder_ShouldOrderThemByTheirPosition()
    {
        // Arrange
        Guid playlistId = Guid.NewGuid();
        PlaylistEntryEntity lastEntry = _playlistEntryEntityFixture.Create(playlistId: playlistId, position: 900, mediaItemType: PlaylistMediaItemType.MusicTrack);
        PlaylistEntryEntity firstEntry = _playlistEntryEntityFixture.Create(playlistId: playlistId, position: 100, mediaItemType: PlaylistMediaItemType.MusicTrack);
        PlaylistEntryEntity middleEntry = _playlistEntryEntityFixture.Create(playlistId: playlistId, position: 500, mediaItemType: PlaylistMediaItemType.MusicTrack);
        PlaylistEntity entity = _playlistEntityFixture.Create(
            id: playlistId,
            entries: [lastEntry, firstEntry, middleEntry]);

        // Act
        Result<Playlist> result = entity.ToDomainEntity();

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal([100L, 500L, 900L], result.Value.Entries.Select(entry => entry.Position));
        Assert.Equal([firstEntry.Id, middleEntry.Id, lastEntry.Id], result.Value.Entries.Select(entry => entry.Id.Value));
    }

    [Fact]
    public void ToDomainEntity_WhenTwoStoredEntriesShareAPosition_ShouldReturnThePositionsError()
    {
        // Arrange
        Guid playlistId = Guid.NewGuid();
        PlaylistEntity entity = _playlistEntityFixture.Create(
            id: playlistId,
            entries:
            [
                _playlistEntryEntityFixture.Create(playlistId: playlistId, position: 256, mediaItemType: PlaylistMediaItemType.MusicTrack),
                _playlistEntryEntityFixture.Create(playlistId: playlistId, position: 256, mediaItemType: PlaylistMediaItemType.MusicTrack),
            ]);

        // Act
        Result<Playlist> result = entity.ToDomainEntity();

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.Playlists.PlaylistEntryPositionsAreInvalid, result.FirstError);
    }

    [Fact]
    public void ToDomainEntity_WhenAStoredEntryReferencesAMediaItemThePlaylistCannotContain_ShouldReturnTheMediaItemsError()
    {
        // Arrange
        Guid playlistId = Guid.NewGuid();
        PlaylistEntity entity = _playlistEntityFixture.Create(
            id: playlistId,
            type: PlaylistType.Music,
            entries: [_playlistEntryEntityFixture.Create(playlistId: playlistId, mediaItemType: PlaylistMediaItemType.VideoFile)]);

        // Act
        Result<Playlist> result = entity.ToDomainEntity();

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.Playlists.PlaylistDoesNotAcceptMediaItems, result.FirstError);
    }

    [Fact]
    public void ToDomainEntity_WhenRehydrating_ShouldRaiseNoDomainEvents()
    {
        // Arrange
        PlaylistEntity entity = _playlistEntityFixture.Create();
        entity.Entries = [_playlistEntryEntityFixture.Create(playlistId: entity.Id, mediaItemType: PlaylistMediaItemType.MusicTrack)];

        // Act
        Result<Playlist> result = entity.ToDomainEntity();

        // Assert
        Assert.False(result.IsFailure);
        Assert.Empty(result.Value.GetDomainEvents());
    }
}
