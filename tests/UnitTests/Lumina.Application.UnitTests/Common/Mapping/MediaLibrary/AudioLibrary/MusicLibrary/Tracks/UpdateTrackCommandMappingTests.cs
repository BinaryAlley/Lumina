#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.MediaLibrary.AudioLibrary.MusicLibrary;
using Lumina.Application.Common.Mapping.MediaLibrary.AudioLibrary.MusicLibrary.Artists;
using Lumina.Application.Common.Mapping.MediaLibrary.AudioLibrary.MusicLibrary.Tracks;
using Lumina.Application.Core.MediaLibrary.AudioLibrary.MusicLibrary.Tracks.Commands.UpdateTrack;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.MediaLibrary.AudioLibrary.MusicLibrary;
using Lumina.Application.Fixtures.Core.MediaLibrary.AudioLibrary.MusicLibrary.Tracks.Commands.UpdateTrack;
using Lumina.Contracts.Fixtures.Core.DTO.Common;
using Lumina.Contracts.Fixtures.Core.DTO.MediaContributors;
using Lumina.Contracts.Fixtures.Core.DTO.MediaLibrary.AudioLibrary;
using Lumina.Contracts.Fixtures.Core.DTO.MediaLibrary.AudioLibrary.MusicLibrary;
using Lumina.Domain.Common.Errors;
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.Core.BoundedContexts.AudioLibraryBoundedContext.MusicLibraryAggregate;
using Lumina.Domain.Core.BoundedContexts.AudioLibraryBoundedContext.MusicLibraryAggregate.Entities;
using Lumina.Domain.SharedKernel.Common.Enums.MediaContributors;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Application.UnitTests.Common.Mapping.MediaLibrary.AudioLibrary.MusicLibrary.Tracks;

/// <summary>
/// Contains unit tests for the <see cref="UpdateTrackCommandMapping"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class UpdateTrackCommandMappingTests
{
    private readonly UpdateTrackCommandFixture _updateTrackCommandFixture = new();
    private readonly ArtistEntityFixture _artistEntityFixture = new();
    private readonly AlbumEntityFixture _albumEntityFixture = new();
    private readonly TrackEntityFixture _trackEntityFixture = new();
    private readonly MusicTrackMetadataDtoFixture _musicTrackMetadataDtoFixture = new();
    private readonly GenreDtoFixture _genreDtoFixture = new();
    private readonly MoodDtoFixture _moodDtoFixture = new();
    private readonly IsrcDtoFixture _isrcDtoFixture = new();
    private readonly MediaContributorReferenceDtoFixture _mediaContributorReferenceDtoFixture = new();
    private readonly AudioRatingDtoFixture _audioRatingDtoFixture = new();

    [Fact]
    public void ToDomainEntity_WhenMappingCompleteCommand_ShouldMapAllPropertiesCorrectly()
    {
        // Arrange
        Guid libraryId = Guid.NewGuid();
        (Artist artist, Guid albumId, Guid trackId) = CreateDomainArtist(libraryId);
        string originalPath = artist.Albums.SelectMany(album => album.Tracks).First(track => track.Id.Value == trackId).Path;
        UpdateTrackCommand command = _updateTrackCommandFixture.Create(libraryId: libraryId.ToString(), artistId: artist.Id.Value.ToString(), albumId: albumId.ToString(), trackId: trackId.ToString());

        // Act
        Result<Artist> result = command.ToDomainEntity(artist);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Same(artist, result.Value);
        Track updatedTrack = result.Value.Albums.First(album => album.Id.Value == albumId).Tracks.First(track => track.Id.Value == trackId);
        // The file system path is never changed by the update, it is set only when the track is created.
        Assert.Equal(originalPath, updatedTrack.Path);
        Assert.Equal(command.Metadata!.Title, updatedTrack.Metadata.Title);
        Assert.Equal(command.TrackNumber, updatedTrack.TrackNumber);
        Assert.True(updatedTrack.DiscNumber.HasValue);
        Assert.Equal(command.DiscNumber, updatedTrack.DiscNumber.Value);
        Assert.True(updatedTrack.Script.HasValue);
        Assert.Equal(command.Script, updatedTrack.Script.Value);
        Assert.True(updatedTrack.Key.HasValue);
        Assert.Equal(command.Key, updatedTrack.Key.Value);
        Assert.True(updatedTrack.Bpm.HasValue);
        Assert.Equal(command.Bpm, updatedTrack.Bpm.Value);
        Assert.True(updatedTrack.Work.HasValue);
        Assert.Equal(command.Work!.Title, updatedTrack.Work.Value.Title);
        Assert.Equal(command.MusicBrainzRecordingId, updatedTrack.MusicBrainzRecordingId.Value.Value);
        Assert.Equal(command.MusicBrainzTrackId, updatedTrack.MusicBrainzTrackId.Value.Value);
        Assert.Equal(command.Work!.MusicBrainzWorkId, updatedTrack.Work.Value.MusicBrainzWorkId.Value);
        Assert.Equal(command.Moods!.Count, updatedTrack.Moods.Count);
        Assert.Equal(command.Isrcs!.Count, updatedTrack.Isrcs.Count);
        Assert.Equal(command.Contributors!.Count, updatedTrack.Contributors.Count);
        Assert.Equal(command.Ratings!.Count, updatedTrack.Ratings.Count);
    }

    [Fact]
    public void ToDomainEntity_WhenOptionalPropertiesAreMissing_ShouldMapRequiredPropertiesCorrectly()
    {
        // Arrange
        Guid libraryId = Guid.NewGuid();
        (Artist artist, Guid albumId, Guid trackId) = CreateDomainArtist(libraryId);
        string originalPath = artist.Albums.SelectMany(album => album.Tracks).First(track => track.Id.Value == trackId).Path;
        UpdateTrackCommand command = _updateTrackCommandFixture.Create(libraryId: libraryId.ToString(), artistId: artist.Id.Value.ToString(), albumId: albumId.ToString(), trackId: trackId.ToString(), includeDiscNumber: false, includeScript: false, includeKey: false, includeBpm: false, includeWork: false, includeMusicBrainzRecordingId: false, includeMusicBrainzTrackId: false, includeMoods: false, includeIsrcs: false, contributors: [], ratings: []);

        // Act
        Result<Artist> result = command.ToDomainEntity(artist);

        // Assert
        Assert.False(result.IsFailure);
        Track updatedTrack = result.Value.Albums.First(album => album.Id.Value == albumId).Tracks.First(track => track.Id.Value == trackId);
        // the file system path is never changed by the update, it is set only when the track is created
        Assert.Equal(originalPath, updatedTrack.Path);
        Assert.Equal(command.TrackNumber, updatedTrack.TrackNumber);
        Assert.False(updatedTrack.DiscNumber.HasValue);
        Assert.False(updatedTrack.Script.HasValue);
        Assert.False(updatedTrack.Key.HasValue);
        Assert.False(updatedTrack.Bpm.HasValue);
        Assert.False(updatedTrack.Work.HasValue);
        Assert.False(updatedTrack.MusicBrainzRecordingId.HasValue);
        Assert.False(updatedTrack.MusicBrainzTrackId.HasValue);
        Assert.Empty(updatedTrack.Moods);
        Assert.Empty(updatedTrack.Isrcs);
        Assert.Empty(updatedTrack.Contributors);
        Assert.Empty(updatedTrack.Ratings);
    }

    [Fact]
    public void ToDomainEntity_WhenMetadataCreationFails_ShouldReturnError()
    {
        // Arrange
        Guid libraryId = Guid.NewGuid();
        (Artist artist, Guid albumId, Guid trackId) = CreateDomainArtist(libraryId);
        UpdateTrackCommand command = _updateTrackCommandFixture.Create(
            libraryId: libraryId.ToString(),
            artistId: artist.Id.Value.ToString(),
            albumId: albumId.ToString(),
            trackId: trackId.ToString(),
            metadata: _musicTrackMetadataDtoFixture.Create(genres: [_genreDtoFixture.Create(name: string.Empty)]));

        // Act
        Result<Artist> result = command.ToDomainEntity(artist);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Contains(result.Errors, error => error.Description == Errors.Metadata.GenreNameCannotBeEmpty.Description);
    }

    [Fact]
    public void ToDomainEntity_WhenMoodCreationFails_ShouldReturnError()
    {
        // Arrange
        Guid libraryId = Guid.NewGuid();
        (Artist artist, Guid albumId, Guid trackId) = CreateDomainArtist(libraryId);
        UpdateTrackCommand command = _updateTrackCommandFixture.Create(libraryId: libraryId.ToString(), artistId: artist.Id.Value.ToString(), albumId: albumId.ToString(), trackId: trackId.ToString(), moods: [_moodDtoFixture.Create(name: string.Empty)]);

        // Act
        Result<Artist> result = command.ToDomainEntity(artist);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Contains(result.Errors, error => error.Description == Errors.Metadata.MoodNameCannotBeEmpty.Description);
    }

    [Fact]
    public void ToDomainEntity_WhenIsrcCreationFails_ShouldReturnError()
    {
        // Arrange
        Guid libraryId = Guid.NewGuid();
        (Artist artist, Guid albumId, Guid trackId) = CreateDomainArtist(libraryId);
        UpdateTrackCommand command = _updateTrackCommandFixture.Create(libraryId: libraryId.ToString(), artistId: artist.Id.Value.ToString(), albumId: albumId.ToString(), trackId: trackId.ToString(), isrcs: [_isrcDtoFixture.Create(value: string.Empty)]);

        // Act
        Result<Artist> result = command.ToDomainEntity(artist);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Contains(result.Errors, error => error.Description == Errors.Music.IsrcValueCannotBeEmpty.Description);
    }

    [Fact]
    public void ToDomainEntity_WhenRatingCreationFails_ShouldReturnError()
    {
        // Arrange
        Guid libraryId = Guid.NewGuid();
        (Artist artist, Guid albumId, Guid trackId) = CreateDomainArtist(libraryId);
        UpdateTrackCommand command = _updateTrackCommandFixture.Create(libraryId: libraryId.ToString(), artistId: artist.Id.Value.ToString(), albumId: albumId.ToString(), trackId: trackId.ToString(), ratings: [_audioRatingDtoFixture.Create(value: 6, maxValue: 5)]);

        // Act
        Result<Artist> result = command.ToDomainEntity(artist);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Contains(result.Errors, error => error.Description == Errors.Metadata.RatingValueCannotBeGreaterThanMaxValue.Description);
    }

    [Fact]
    public void ToDomainEntity_WhenTheSameContributorHasDifferentRoles_ShouldKeepBothRoles()
    {
        // Arrange
        Guid libraryId = Guid.NewGuid();
        (Artist artist, Guid albumId, Guid trackId) = CreateDomainArtist(libraryId);
        Guid contributorId = Guid.NewGuid();
        UpdateTrackCommand command = _updateTrackCommandFixture.Create(
            libraryId: libraryId.ToString(),
            artistId: artist.Id.Value.ToString(),
            albumId: albumId.ToString(),
            trackId: trackId.ToString(),
            contributors:
            [
                _mediaContributorReferenceDtoFixture.Create(contributorId: contributorId, role: MediaContributorRole.Vocals),
                _mediaContributorReferenceDtoFixture.Create(contributorId: contributorId, role: MediaContributorRole.Guitar)
            ]);

        // Act
        Result<Artist> result = command.ToDomainEntity(artist);

        // Assert
        Assert.False(result.IsFailure);
        Track updatedTrack = result.Value.Albums.First(album => album.Id.Value == albumId).Tracks.First(track => track.Id.Value == trackId);
        Assert.Equal(2, updatedTrack.Contributors.Count);
        Assert.All(updatedTrack.Contributors, contributor => Assert.Equal(contributorId, contributor.ContributorId.Value));
        Assert.Contains(updatedTrack.Contributors, contributor => contributor.Role == MediaContributorRole.Vocals);
        Assert.Contains(updatedTrack.Contributors, contributor => contributor.Role == MediaContributorRole.Guitar);
    }

    [Fact]
    public void ToDomainEntity_WhenAlbumIsNotPartOfTheArtist_ShouldReturnAlbumNotFoundError()
    {
        // Arrange
        Guid libraryId = Guid.NewGuid();
        (Artist artist, Guid _, Guid trackId) = CreateDomainArtist(libraryId);
        UpdateTrackCommand command = _updateTrackCommandFixture.Create(libraryId: libraryId.ToString(), artistId: artist.Id.Value.ToString(), albumId: Guid.NewGuid().ToString(), trackId: trackId.ToString());

        // Act
        Result<Artist> result = command.ToDomainEntity(artist);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.Music.AlbumNotFound, result.FirstError);
    }

    [Fact]
    public void ToDomainEntity_WhenTrackIsNotPartOfTheAlbum_ShouldReturnTrackNotFoundError()
    {
        // Arrange
        Guid libraryId = Guid.NewGuid();
        (Artist artist, Guid albumId, Guid _) = CreateDomainArtist(libraryId);
        UpdateTrackCommand command = _updateTrackCommandFixture.Create(libraryId: libraryId.ToString(), artistId: artist.Id.Value.ToString(), albumId: albumId.ToString(), trackId: Guid.NewGuid().ToString());

        // Act
        Result<Artist> result = command.ToDomainEntity(artist);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.Music.TrackNotFound, result.FirstError);
    }

    [Fact]
    public void ToDomainEntity_WhenMetadataHasDisambiguation_ShouldMapTheDisambiguationThrough()
    {
        // Arrange
        Guid libraryId = Guid.NewGuid();
        (Artist artist, Guid albumId, Guid trackId) = CreateDomainArtist(libraryId);
        string disambiguation = "2011 remaster";
        UpdateTrackCommand command = _updateTrackCommandFixture.Create(
            libraryId: libraryId.ToString(),
            artistId: artist.Id.Value.ToString(),
            albumId: albumId.ToString(),
            trackId: trackId.ToString(),
            metadata: _musicTrackMetadataDtoFixture.Create(disambiguation: disambiguation));

        // Act
        Result<Artist> result = command.ToDomainEntity(artist);

        // Assert
        Assert.False(result.IsFailure);
        Track updatedTrack = result.Value.Albums.First(album => album.Id.Value == albumId).Tracks.First(track => track.Id.Value == trackId);
        Assert.True(updatedTrack.Disambiguation.HasValue);
        Assert.Equal(disambiguation, updatedTrack.Disambiguation.Value);
    }

    [Fact]
    public void ToDomainEntity_WhenMetadataHasNoDisambiguation_ShouldClearTheDisambiguation()
    {
        // Arrange
        Guid libraryId = Guid.NewGuid();
        (Artist artist, Guid albumId, Guid trackId) = CreateDomainArtist(libraryId);
        UpdateTrackCommand command = _updateTrackCommandFixture.Create(
            libraryId: libraryId.ToString(),
            artistId: artist.Id.Value.ToString(),
            albumId: albumId.ToString(),
            trackId: trackId.ToString(),
            metadata: _musicTrackMetadataDtoFixture.Create(includeDisambiguation: false));

        // Act
        Result<Artist> result = command.ToDomainEntity(artist);

        // Assert
        Assert.False(result.IsFailure);
        Track updatedTrack = result.Value.Albums.First(album => album.Id.Value == albumId).Tracks.First(track => track.Id.Value == trackId);
        Assert.False(updatedTrack.Disambiguation.HasValue);
    }

    [Fact]
    public void ToDomainEntity_WhenTheMetadataCarriesNoContainer_ShouldKeepTheTrackContainer()
    {
        // Arrange
        Guid libraryId = Guid.NewGuid();
        (Artist artist, Guid albumId, Guid trackId) = CreateDomainArtist(libraryId);
        Track storedTrack = artist.Albums.First(album => album.Id.Value == albumId).Tracks.First(track => track.Id.Value == trackId);
        Optional<string> storedContainer = storedTrack.Metadata.Container;
        UpdateTrackCommand command = _updateTrackCommandFixture.Create(
            libraryId: libraryId.ToString(),
            artistId: artist.Id.Value.ToString(),
            albumId: albumId.ToString(),
            trackId: trackId.ToString());

        // Act
        Result<Artist> result = command.ToDomainEntity(artist);

        // Assert
        Assert.False(result.IsFailure);
        Track updatedTrack = result.Value.Albums.First(album => album.Id.Value == albumId).Tracks.First(track => track.Id.Value == trackId);
        // The stored container is pinned to a known value, so the assertion cannot pass by coincidence.
        Assert.Equal("FLAC", storedContainer.Value);
        // The container is captured from the file itself and is never part of the edit metadata, so an update keeps the stored container.
        Assert.Equal("FLAC", updatedTrack.Metadata.Container.Value);
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
            tracks: [_trackEntityFixture.Create(id: trackId, albumId: albumId, libraryId: libraryId, container: "FLAC")]);
        ArtistEntity artistEntity = _artistEntityFixture.Create(libraryId: libraryId, albums: [albumEntity]);
        Result<Artist> artistResult = artistEntity.ToDomainEntity();
        return (artistResult.Value, albumId, trackId);
    }
}
