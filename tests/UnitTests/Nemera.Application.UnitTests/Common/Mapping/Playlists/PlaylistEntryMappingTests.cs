#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.Playlists;
using Lumina.Application.Common.Mapping.Playlists;
using Lumina.Domain.Core.BoundedContexts.PlaylistBoundedContext.PlaylistAggregate.Entities;
using Lumina.Domain.Fixtures.Core.BoundedContexts.PlaylistBoundedContext.PlaylistAggregate.Entities;
using Lumina.Domain.SharedKernel.Common.Enums.Playlists;
using System;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Application.UnitTests.Common.Mapping.Playlists;

/// <summary>
/// Contains unit tests for the <see cref="PlaylistEntryMapping"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class PlaylistEntryMappingTests
{
    private readonly PlaylistEntryFixture _playlistEntryFixture = new();

    [Fact]
    public void ToRepositoryEntity_WhenMappingTheDomainEntry_ShouldMapEveryFieldIncludingTheOwnerPlaylist()
    {
        // Arrange
        Guid playlistId = Guid.NewGuid();
        PlaylistEntry entry = _playlistEntryFixture.Create(
            mediaItemType: PlaylistMediaItemType.MusicTrack,
            position: 1_024);

        // Act
        PlaylistEntryEntity result = entry.ToRepositoryEntity(playlistId);

        // Assert
        Assert.Equal(entry.Id.Value, result.Id);
        Assert.Equal(playlistId, result.PlaylistId);
        Assert.Equal(entry.MediaItemId.Value, result.MediaItemId);
        Assert.Equal(PlaylistMediaItemType.MusicTrack, result.MediaItemType);
        Assert.Equal(1_024, result.Position);
    }
}
