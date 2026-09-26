#region ========================================================================= USING =====================================================================================
using Lumina.Domain.Common.Errors;
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.Core.BoundedContexts.AudioLibraryBoundedContext.MusicLibraryAggregate.Entities;
using Lumina.Domain.Fixtures.Core.BoundedContexts.AudioLibraryBoundedContext.MusicLibraryAggregate.Entities;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Domain.UnitTests.Core.BoundedContexts.AudioLibraryBoundedContext.MusicLibraryAggregate.Entities;

/// <summary>
/// Contains unit tests for the <see cref="Album"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class AlbumTests
{
    private readonly AlbumFixture _albumFixture = new();
    private readonly TrackFixture _trackFixture = new();

    [Fact]
    public void AddTrack_WhenTrackIsNotInTheAlbum_ShouldAddTheTrackAndReturnSuccess()
    {
        // Arrange
        Album album = _albumFixture.Create(tracks: []);
        Track track = _trackFixture.Create();

        // Act
        Result<Created> result = album.AddTrack(track);

        // Assert
        Assert.False(result.IsFailure);
        Track storedTrack = Assert.Single(album.Tracks);
        Assert.Equal(track, storedTrack);
    }

    [Fact]
    public void AddTrack_WhenTrackIsAlreadyInTheAlbum_ShouldReturnTheTrackIsAlreadyInTheAlbumErrorAndNotAddDuplicate()
    {
        // Arrange
        Album album = _albumFixture.Create(tracks: []);
        Track track = _trackFixture.Create();
        album.AddTrack(track);

        // Act
        Result<Created> result = album.AddTrack(track);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.Music.TheTrackIsAlreadyInTheAlbum, result.FirstError);
        Assert.Single(album.Tracks);
    }
}
