#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.Playlists;
using Lumina.Application.Common.Mapping.Playlists;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.Playlists;
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.Core.BoundedContexts.PlaylistBoundedContext.PlaylistAggregate.Entities;
using Lumina.Domain.SharedKernel.Common.Enums.Playlists;
using System;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Application.UnitTests.Common.Mapping.Playlists;

/// <summary>
/// Contains unit tests for the <see cref="PlaylistEntryEntityMapping"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class PlaylistEntryEntityMappingTests
{
    private readonly PlaylistEntryEntityFixture _playlistEntryEntityFixture = new();

    [Fact]
    public void ToDomainEntity_WhenMappingTheEntity_ShouldMapEveryField()
    {
        // Arrange
        Guid id = Guid.NewGuid();
        Guid mediaItemId = Guid.NewGuid();
        PlaylistEntryEntity entity = _playlistEntryEntityFixture.Create(
            id: id,
            mediaItemId: mediaItemId,
            mediaItemType: PlaylistMediaItemType.MusicTrack,
            position: 4_096);

        // Act
        Result<PlaylistEntry> result = entity.ToDomainEntity();

        // Assert
        Assert.False(result.IsFailure);
        PlaylistEntry entry = result.Value;
        Assert.Equal(id, entry.Id.Value);
        Assert.Equal(mediaItemId, entry.MediaItemId.Value);
        Assert.Equal(PlaylistMediaItemType.MusicTrack, entry.MediaItemType);
        Assert.Equal(4_096, entry.Position);
    }

    [Fact]
    public void ToDomainEntity_WhenTheStoredMediaItemTypeIsNotAcceptedByAnyPlaylistToday_ShouldStillRehydrateIt()
    {
        // Arrange
        PlaylistEntryEntity entity = _playlistEntryEntityFixture.Create(mediaItemType: PlaylistMediaItemType.VideoFile);

        // Act
        Result<PlaylistEntry> result = entity.ToDomainEntity();

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(PlaylistMediaItemType.VideoFile, result.Value.MediaItemType);
    }
}
