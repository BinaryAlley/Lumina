#region ========================================================================= USING =====================================================================================
using Lumina.Domain.Common.Errors;
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.Core.BoundedContexts.AudioLibraryBoundedContext.MusicLibraryAggregate;
using Lumina.Domain.Core.BoundedContexts.AudioLibraryBoundedContext.MusicLibraryAggregate.Entities;
using Lumina.Domain.Fixtures.Core.BoundedContexts.AudioLibraryBoundedContext.MusicLibraryAggregate;
using Lumina.Domain.Fixtures.Core.BoundedContexts.AudioLibraryBoundedContext.MusicLibraryAggregate.Entities;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Domain.UnitTests.Core.BoundedContexts.AudioLibraryBoundedContext.MusicLibraryAggregate;

/// <summary>
/// Contains unit tests for the <see cref="Artist"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class ArtistTests
{
    private readonly ArtistFixture _artistFixture = new();
    private readonly AlbumFixture _albumFixture = new();
    private readonly TrackFixture _trackFixture = new();

    [Fact]
    public void AddTrackToAlbum_WhenAlbumIsOwnedByTheArtist_ShouldAddTheTrackAndReturnSuccess()
    {
        // Arrange
        Album album = _albumFixture.Create(tracks: []);
        Artist artist = _artistFixture.Create(albums: [album]);
        Track track = _trackFixture.Create();

        // Act
        Result<Created> result = artist.AddTrackToAlbum(album, track);

        // Assert
        Assert.False(result.IsFailure);
        Track storedTrack = Assert.Single(album.Tracks);
        Assert.Equal(track, storedTrack);
    }

    [Fact]
    public void AddTrackToAlbum_WhenAlbumIsNotOwnedByTheArtist_ShouldReturnAlbumNotFoundError()
    {
        // Arrange
        Album ownedAlbum = _albumFixture.Create(tracks: []);
        Album foreignAlbum = _albumFixture.Create(tracks: []);
        Artist artist = _artistFixture.Create(albums: [ownedAlbum]);
        Track track = _trackFixture.Create();

        // Act
        Result<Created> result = artist.AddTrackToAlbum(foreignAlbum, track);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.Music.AlbumNotFound, result.FirstError);
        Assert.Empty(foreignAlbum.Tracks);
    }

    [Fact]
    public void AddTrackToAlbum_WhenTrackIsAlreadyInTheAlbum_ShouldPropagateTheTrackIsAlreadyInTheAlbumError()
    {
        // Arrange
        Album album = _albumFixture.Create(tracks: []);
        Artist artist = _artistFixture.Create(albums: [album]);
        Track track = _trackFixture.Create();
        artist.AddTrackToAlbum(album, track);

        // Act
        Result<Created> result = artist.AddTrackToAlbum(album, track);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.Music.TheTrackIsAlreadyInTheAlbum, result.FirstError);
        Assert.Single(album.Tracks);
    }
}
