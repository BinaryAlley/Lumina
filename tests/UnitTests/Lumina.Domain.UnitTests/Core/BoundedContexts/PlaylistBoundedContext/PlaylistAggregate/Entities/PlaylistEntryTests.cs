#region ========================================================================= USING =====================================================================================
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.Core.BoundedContexts.PlaylistBoundedContext.ExternalIdentifiers.MediaLibraryBoundedContext.MediaItemAggregate;
using Lumina.Domain.Core.BoundedContexts.PlaylistBoundedContext.PlaylistAggregate.Entities;
using Lumina.Domain.Core.BoundedContexts.PlaylistBoundedContext.PlaylistAggregate.ValueObjects;
using Lumina.Domain.Fixtures.Core.BoundedContexts.PlaylistBoundedContext.ExternalIdentifiers.MediaLibraryBoundedContext.MediaItemAggregate;
using Lumina.Domain.Fixtures.Core.BoundedContexts.PlaylistBoundedContext.PlaylistAggregate.Entities;
using Lumina.Domain.Fixtures.Core.BoundedContexts.PlaylistBoundedContext.PlaylistAggregate.ValueObjects;
using Lumina.Domain.SharedKernel.Common.Enums.Playlists;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Domain.UnitTests.Core.BoundedContexts.PlaylistBoundedContext.PlaylistAggregate.Entities;

/// <summary>
/// Contains unit tests for the <see cref="PlaylistEntry"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class PlaylistEntryTests
{
    private readonly PlaylistEntryFixture _playlistEntryFixture = new();
    private readonly PlaylistEntryIdFixture _playlistEntryIdFixture = new();
    private readonly MediaItemIdFixture _mediaItemIdFixture = new();

    [Fact]
    public void Create_WhenCalled_ShouldSetEveryProperty()
    {
        // Arrange
        PlaylistEntryId id = _playlistEntryIdFixture.Create();
        MediaItemId mediaItemId = _mediaItemIdFixture.Create();

        // Act
        Result<PlaylistEntry> result = PlaylistEntry.Create(id, mediaItemId, PlaylistMediaItemType.MusicTrack, 2_048);

        // Assert
        Assert.False(result.IsFailure);
        PlaylistEntry entry = result.Value;
        Assert.Equal(id, entry.Id);
        Assert.Equal(mediaItemId, entry.MediaItemId);
        Assert.Equal(PlaylistMediaItemType.MusicTrack, entry.MediaItemType);
        Assert.Equal(2_048, entry.Position);
    }

    [Theory]
    [InlineData(PlaylistMediaItemType.MusicTrack)] // a track of a music library
    [InlineData(PlaylistMediaItemType.VideoFile)] // a video file, kept whatever the playlist that references it accepts today
    public void Create_WhenCalled_ShouldPreserveTheMediaItemType(PlaylistMediaItemType mediaItemType)
    {
        // Act
        Result<PlaylistEntry> result = PlaylistEntry.Create(
            _playlistEntryIdFixture.Create(),
            _mediaItemIdFixture.Create(),
            mediaItemType,
            0);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(mediaItemType, result.Value.MediaItemType);
    }

    [Fact]
    public void Create_WhenTwoEntriesShareTheSameId_ShouldBeEqual()
    {
        // Arrange
        PlaylistEntryId id = _playlistEntryIdFixture.Create();
        PlaylistEntry firstEntry = _playlistEntryFixture.Create(id: id);
        PlaylistEntry secondEntry = _playlistEntryFixture.Create(id: id);

        // Act
        bool areEqual = firstEntry.Equals(secondEntry);

        // Assert
        Assert.True(areEqual);
    }
}
