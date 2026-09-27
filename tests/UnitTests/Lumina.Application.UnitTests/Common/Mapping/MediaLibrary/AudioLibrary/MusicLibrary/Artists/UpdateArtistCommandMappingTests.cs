#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.MediaLibrary.AudioLibrary.MusicLibrary;
using Lumina.Application.Common.Mapping.MediaLibrary.AudioLibrary.MusicLibrary.Artists;
using Lumina.Application.Core.MediaLibrary.AudioLibrary.MusicLibrary.Albums.Commands.AddAlbum;
using Lumina.Application.Core.MediaLibrary.AudioLibrary.MusicLibrary.Artists.Commands.UpdateArtist;
using Lumina.Application.Core.MediaLibrary.AudioLibrary.MusicLibrary.Tracks.Commands.AddTrack;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.MediaLibrary.AudioLibrary.MusicLibrary;
using Lumina.Application.Fixtures.Core.MediaLibrary.AudioLibrary.MusicLibrary.Albums.Commands.AddAlbum;
using Lumina.Application.Fixtures.Core.MediaLibrary.AudioLibrary.MusicLibrary.Artists.Commands.UpdateArtist;
using Lumina.Application.Fixtures.Core.MediaLibrary.AudioLibrary.MusicLibrary.Tracks.Commands.AddTrack;
using Lumina.Contracts.Fixtures.Core.DTO.Common;
using Lumina.Contracts.Fixtures.Core.DTO.MediaLibrary.AudioLibrary.MusicLibrary;
using Lumina.Domain.Common.Errors;
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.Core.BoundedContexts.AudioLibraryBoundedContext.MusicLibraryAggregate;
using Lumina.Domain.Core.BoundedContexts.AudioLibraryBoundedContext.MusicLibraryAggregate.Entities;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Application.UnitTests.Common.Mapping.MediaLibrary.AudioLibrary.MusicLibrary.Artists;

/// <summary>
/// Contains unit tests for the <see cref="UpdateArtistCommandMapping"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class UpdateArtistCommandMappingTests
{
    private readonly UpdateArtistCommandFixture _updateArtistCommandFixture = new();
    private readonly AddAlbumCommandFixture _addAlbumCommandFixture = new();
    private readonly AddTrackCommandFixture _addTrackCommandFixture = new();
    private readonly ArtistEntityFixture _artistEntityFixture = new();
    private readonly AlbumEntityFixture _albumEntityFixture = new();
    private readonly TrackEntityFixture _trackEntityFixture = new();
    private readonly AlbumMetadataDtoFixture _albumMetadataDtoFixture = new();
    private readonly GenreDtoFixture _genreDtoFixture = new();

    [Fact]
    public void ToDomainEntity_WhenMappingCompleteCommand_ShouldMapAllPropertiesCorrectly()
    {
        // Arrange
        Guid libraryId = Guid.NewGuid();
        (Artist artist, Guid albumId, Guid trackId) = CreateDomainArtist(libraryId);
        string updatedPath = "/music/queen/a-night-at-the-opera/03-you-re-my-best-friend.flac";
        AddTrackCommand trackCommand = _addTrackCommandFixture.Create(trackId: trackId, path: updatedPath, trackNumber: 7);
        AddAlbumCommand albumCommand = _addAlbumCommandFixture.Create(albumId: albumId, tracks: [trackCommand]);
        UpdateArtistCommand command = _updateArtistCommandFixture.Create(
            libraryId: libraryId.ToString(),
            artistId: artist.Id.Value.ToString(),
            albums: [albumCommand]);

        // Act
        Result<Artist> result = command.ToDomainEntity(artist);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Same(artist, result.Value);
        Assert.Equal(command.Name, result.Value.Name);
        Assert.True(result.Value.Website.HasValue);
        Assert.Equal(command.Website, result.Value.Website.Value);
        Assert.True(result.Value.MusicBrainzArtistId.HasValue);
        Assert.Equal(command.MusicBrainzArtistId, result.Value.MusicBrainzArtistId.Value.Value);
        Assert.Equal(command.Contributors!.Count, result.Value.Contributors.Count);
        Album updatedAlbum = result.Value.Albums.First(album => album.Id.Value == albumId);
        Assert.Equal(albumCommand.Metadata!.Title, updatedAlbum.Metadata.Title);
        Track updatedTrack = updatedAlbum.Tracks.First(track => track.Id.Value == trackId);
        Assert.Equal(updatedPath, updatedTrack.Path);
        Assert.Equal(7, updatedTrack.TrackNumber);
        Assert.True(updatedTrack.DiscNumber.HasValue);
        Assert.Equal(trackCommand.DiscNumber, updatedTrack.DiscNumber.Value);
        Assert.Equal(trackCommand.Moods!.Count, updatedTrack.Moods.Count);
        Assert.Equal(trackCommand.Isrcs!.Count, updatedTrack.Isrcs.Count);
        Assert.Equal(trackCommand.Contributors!.Count, updatedTrack.Contributors.Count);
        Assert.Equal(trackCommand.Ratings!.Count, updatedTrack.Ratings.Count);
    }

    [Fact]
    public void ToDomainEntity_WhenOptionalPropertiesAreMissing_ShouldMapTheRequiredPropertiesAndNones()
    {
        // Arrange
        Guid libraryId = Guid.NewGuid();
        (Artist artist, Guid albumId, Guid trackId) = CreateDomainArtist(libraryId);
        AddTrackCommand trackCommand = _addTrackCommandFixture.Create(
            trackId: trackId,
            includeDiscNumber: false,
            includeScript: false,
            includeKey: false,
            includeBpm: false,
            includeWork: false,
            includeMusicBrainzRecordingId: false,
            includeMusicBrainzTrackId: false,
            includeMusicBrainzWorkId: false,
            includeMoods: false,
            includeIsrcs: false,
            contributors: [],
            ratings: []);
        AddAlbumCommand albumCommand = _addAlbumCommandFixture.Create(albumId: albumId, tracks: [trackCommand]);
        UpdateArtistCommand command = _updateArtistCommandFixture.Create(
            libraryId: libraryId.ToString(),
            artistId: artist.Id.Value.ToString(),
            includeWebsite: false,
            includeMusicBrainzArtistId: false,
            contributors: [],
            albums: [albumCommand]);

        // Act
        Result<Artist> result = command.ToDomainEntity(artist);

        // Assert
        Assert.False(result.IsFailure);
        Assert.False(result.Value.Website.HasValue);
        Assert.False(result.Value.MusicBrainzArtistId.HasValue);
        Assert.Empty(result.Value.Contributors);
        Track updatedTrack = result.Value.Albums.First(album => album.Id.Value == albumId).Tracks.First(track => track.Id.Value == trackId);
        Assert.False(updatedTrack.DiscNumber.HasValue);
        Assert.False(updatedTrack.Script.HasValue);
        Assert.False(updatedTrack.Key.HasValue);
        Assert.False(updatedTrack.Bpm.HasValue);
        Assert.False(updatedTrack.Work.HasValue);
        Assert.Empty(updatedTrack.Moods);
        Assert.Empty(updatedTrack.Isrcs);
        Assert.Empty(updatedTrack.Contributors);
        Assert.Empty(updatedTrack.Ratings);
    }

    [Fact]
    public void ToDomainEntity_WhenArtistNameIsEmpty_ShouldReturnError()
    {
        // Arrange
        Guid libraryId = Guid.NewGuid();
        (Artist artist, Guid albumId, Guid trackId) = CreateDomainArtist(libraryId);
        AddAlbumCommand albumCommand = _addAlbumCommandFixture.Create(albumId: albumId, tracks: []);
        UpdateArtistCommand command = _updateArtistCommandFixture.Create(
            libraryId: libraryId.ToString(),
            artistId: artist.Id.Value.ToString(),
            name: string.Empty,
            albums: [albumCommand]);

        // Act
        Result<Artist> result = command.ToDomainEntity(artist);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.Music.ArtistNameCannotBeEmpty, result.FirstError);
    }

    [Fact]
    public void ToDomainEntity_WhenAlbumCreationFails_ShouldReturnError()
    {
        // Arrange
        Guid libraryId = Guid.NewGuid();
        (Artist artist, Guid albumId, Guid trackId) = CreateDomainArtist(libraryId);
        AddAlbumCommand invalidAlbum = _addAlbumCommandFixture.Create(
            albumId: albumId,
            metadata: _albumMetadataDtoFixture.Create(genres: [_genreDtoFixture.Create(name: string.Empty)]));
        UpdateArtistCommand command = _updateArtistCommandFixture.Create(
            libraryId: libraryId.ToString(),
            artistId: artist.Id.Value.ToString(),
            albums: [invalidAlbum]);

        // Act
        Result<Artist> result = command.ToDomainEntity(artist);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Contains(result.Errors, error => error.Description == Errors.Metadata.GenreNameCannotBeEmpty.Description);
    }

    [Fact]
    public void ToDomainEntity_WhenAllAlbumsAreRemoved_ShouldReturnError()
    {
        // Arrange
        Guid libraryId = Guid.NewGuid();
        (Artist artist, Guid albumId, Guid trackId) = CreateDomainArtist(libraryId);
        UpdateArtistCommand command = _updateArtistCommandFixture.Create(
            libraryId: libraryId.ToString(),
            artistId: artist.Id.Value.ToString(),
            albums: []);

        // Act
        Result<Artist> result = command.ToDomainEntity(artist);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.Music.ArtistMustHaveAtLeastOneAlbum, result.FirstError);
    }

    [Fact]
    public void ToDomainEntity_WhenAlbumIsNew_ShouldAddItAndRemoveTheStaleOne()
    {
        // Arrange
        Guid libraryId = Guid.NewGuid();
        (Artist artist, Guid albumId, Guid trackId) = CreateDomainArtist(libraryId);
        AddAlbumCommand newAlbum = _addAlbumCommandFixture.Create();
        UpdateArtistCommand command = _updateArtistCommandFixture.Create(
            libraryId: libraryId.ToString(),
            artistId: artist.Id.Value.ToString(),
            albums: [newAlbum]);

        // Act
        Result<Artist> result = command.ToDomainEntity(artist);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Single(result.Value.Albums);
        Assert.DoesNotContain(result.Value.Albums, album => album.Id.Value == albumId);
    }

    [Fact]
    public void ToDomainEntity_WhenTrackIsNew_ShouldAddItToTheExistingAlbum()
    {
        // Arrange
        Guid libraryId = Guid.NewGuid();
        (Artist artist, Guid albumId, Guid trackId) = CreateDomainArtist(libraryId);
        Guid newTrackId = Guid.NewGuid();
        AddTrackCommand existingTrack = _addTrackCommandFixture.Create(trackId: trackId);
        AddTrackCommand newTrack = _addTrackCommandFixture.Create(trackId: newTrackId);
        AddAlbumCommand albumCommand = _addAlbumCommandFixture.Create(albumId: albumId, tracks: [existingTrack, newTrack]);
        UpdateArtistCommand command = _updateArtistCommandFixture.Create(
            libraryId: libraryId.ToString(),
            artistId: artist.Id.Value.ToString(),
            albums: [albumCommand]);

        // Act
        Result<Artist> result = command.ToDomainEntity(artist);

        // Assert
        Assert.False(result.IsFailure);
        Album updatedAlbum = result.Value.Albums.First(album => album.Id.Value == albumId);
        Assert.Equal(2, updatedAlbum.Tracks.Count);
        Assert.Contains(updatedAlbum.Tracks, track => track.Id.Value == newTrackId);
        Assert.Contains(updatedAlbum.Tracks, track => track.Id.Value == trackId);
    }

    [Fact]
    public void ToDomainEntity_WhenTrackIsStale_ShouldRemoveItFromTheExistingAlbum()
    {
        // Arrange
        Guid libraryId = Guid.NewGuid();
        (Artist artist, Guid albumId, Guid trackId) = CreateDomainArtist(libraryId);
        AddAlbumCommand albumCommand = _addAlbumCommandFixture.Create(albumId: albumId, tracks: []);
        UpdateArtistCommand command = _updateArtistCommandFixture.Create(
            libraryId: libraryId.ToString(),
            artistId: artist.Id.Value.ToString(),
            albums: [albumCommand]);

        // Act
        Result<Artist> result = command.ToDomainEntity(artist);

        // Assert
        Assert.False(result.IsFailure);
        Album updatedAlbum = result.Value.Albums.First(album => album.Id.Value == albumId);
        Assert.Empty(updatedAlbum.Tracks);
    }

    /// <summary>
    /// Creates a domain artist that owns a single album, which in turn owns a single track, all belonging to <paramref name="libraryId"/>.
    /// </summary>
    /// <param name="libraryId">The Id of the media library the artist belongs to.</param>
    /// <returns>The created domain artist, together with the Ids of its album and track.</returns>
    private (Artist artist, Guid albumId, Guid trackId) CreateDomainArtist(Guid libraryId)
    {
        Guid albumId = Guid.NewGuid();
        Guid trackId = Guid.NewGuid();
        AlbumEntity albumEntity = _albumEntityFixture.Create(
            id: albumId,
            libraryId: libraryId,
            tracks: [_trackEntityFixture.Create(id: trackId, albumId: albumId, libraryId: libraryId)]);
        ArtistEntity artistEntity = _artistEntityFixture.Create(libraryId: libraryId, albums: [albumEntity]);
        Result<Artist> artistResult = artistEntity.ToDomainEntity();
        return (artistResult.Value, albumId, trackId);
    }
}
