#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.Mapping.MediaLibrary.AudioLibrary.MusicLibrary.Tracks;
using Lumina.Application.Core.MediaLibrary.AudioLibrary.MusicLibrary.Tracks.Commands.UpdateTrack;
using Lumina.Contracts.Fixtures.Core.Requests.MediaLibrary.AudioLibrary.MusicLibrary.Tracks;
using Lumina.Contracts.Requests.MediaLibrary.AudioLibrary.MusicLibrary.Tracks;
using Lumina.Domain.SharedKernel.Common.Enums.AudioLibrary;
using Lumina.Domain.SharedKernel.Common.Enums.MediaContributors;
using System;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Application.UnitTests.Common.Mapping.MediaLibrary.AudioLibrary.MusicLibrary.Tracks;

/// <summary>
/// Contains unit tests for the <see cref="UpdateTrackRequestMapping"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class UpdateTrackRequestMappingTests
{
    private readonly UpdateTrackRequestFixture _updateTrackRequestFixture = new();

    [Fact]
    public void ToCommand_WhenMappingCompleteRequest_ShouldMapAllPropertiesCorrectly()
    {
        // Arrange
        Guid libraryId = Guid.NewGuid();
        Guid artistId = Guid.NewGuid();
        Guid albumId = Guid.NewGuid();
        Guid trackId = Guid.NewGuid();
        UpdateTrackRequest request = _updateTrackRequestFixture.Create();

        // Act
        UpdateTrackCommand result = request.ToCommand(libraryId.ToString(), artistId.ToString(), albumId.ToString(), trackId.ToString());

        // Assert
        Assert.NotNull(result);
        Assert.Equal(libraryId.ToString(), result.LibraryId);
        Assert.Equal(artistId.ToString(), result.ArtistId);
        Assert.Equal(albumId.ToString(), result.AlbumId);
        Assert.Equal(trackId.ToString(), result.TrackId);
        Assert.Equal(request.Path, result.Path);
        Assert.Equal(request.Metadata, result.Metadata);
        Assert.Equal(request.TrackNumber, result.TrackNumber);
        Assert.Equal(request.DiscNumber, result.DiscNumber);
        Assert.Equal(request.Script, result.Script);
        Assert.Equal(request.Key, result.Key);
        Assert.Equal(request.Bpm, result.Bpm);
        Assert.Equal(request.Work, result.Work);
        Assert.Equal(request.MusicBrainzRecordingId, result.MusicBrainzRecordingId);
        Assert.Equal(request.MusicBrainzTrackId, result.MusicBrainzTrackId);
        Assert.Equal(request.MusicBrainzWorkId, result.MusicBrainzWorkId);
        Assert.Equal(request.Moods, result.Moods);
        Assert.Equal(request.Isrcs, result.Isrcs);
        Assert.Equal(request.Contributors, result.Contributors);
        Assert.Equal(request.Ratings, result.Ratings);
    }

    [Fact]
    public void ToCommand_WhenMappingMinimalRequest_ShouldMapCorrectly()
    {
        // Arrange
        Guid libraryId = Guid.NewGuid();
        Guid artistId = Guid.NewGuid();
        Guid albumId = Guid.NewGuid();
        Guid trackId = Guid.NewGuid();
        UpdateTrackRequest request = _updateTrackRequestFixture.Create(
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

        // Act
        UpdateTrackCommand result = request.ToCommand(libraryId.ToString(), artistId.ToString(), albumId.ToString(), trackId.ToString());

        // Assert
        Assert.NotNull(result);
        Assert.Equal(libraryId.ToString(), result.LibraryId);
        Assert.Equal(artistId.ToString(), result.ArtistId);
        Assert.Equal(albumId.ToString(), result.AlbumId);
        Assert.Equal(trackId.ToString(), result.TrackId);
        Assert.NotNull(result.Metadata);
        Assert.Equal(request.Metadata, result.Metadata);
        Assert.Null(result.DiscNumber);
        Assert.Null(result.Script);
        Assert.Null(result.Key);
        Assert.Null(result.Bpm);
        Assert.Null(result.Work);
        Assert.Null(result.MusicBrainzRecordingId);
        Assert.Null(result.MusicBrainzTrackId);
        Assert.Null(result.MusicBrainzWorkId);
        Assert.Null(result.Moods);
        Assert.Null(result.Isrcs);
        Assert.Empty(result.Contributors!);
        Assert.Empty(result.Ratings!);
    }

    [Fact]
    public void ToCommand_WhenMappingRequestWithCollections_ShouldMapCollectionsCorrectly()
    {
        // Arrange
        Guid libraryId = Guid.NewGuid();
        Guid artistId = Guid.NewGuid();
        Guid albumId = Guid.NewGuid();
        Guid trackId = Guid.NewGuid();
        UpdateTrackRequest request = _updateTrackRequestFixture.Create(
            moods: [new("dramatic")],
            isrcs: [new("GBUM71029604")],
            contributors: [new(Guid.NewGuid(), MediaContributorRole.Vocals)],
            ratings: [new(4.5m, 5m, AudioRatingSource.MusicBrainz, 1000)],
            includeDiscNumber: false,
            includeScript: false,
            includeKey: false,
            includeBpm: false,
            includeWork: false,
            includeMusicBrainzRecordingId: false,
            includeMusicBrainzTrackId: false,
            includeMusicBrainzWorkId: false);

        // Act
        UpdateTrackCommand result = request.ToCommand(libraryId.ToString(), artistId.ToString(), albumId.ToString(), trackId.ToString());

        // Assert
        Assert.NotNull(result);
        Assert.Equal(request.Moods, result.Moods);
        Assert.Equal(request.Isrcs, result.Isrcs);
        Assert.Equal(request.Contributors, result.Contributors);
        Assert.Equal(request.Ratings, result.Ratings);
    }

    [Fact]
    public void ToCommand_WhenCalledWithRouteIds_ShouldMapRouteIdsFromArguments()
    {
        // Arrange
        UpdateTrackRequest request = _updateTrackRequestFixture.Create();

        // Act
        UpdateTrackCommand result = request.ToCommand("library-route", "artist-route", "album-route", "track-route");

        // Assert
        Assert.Equal("library-route", result.LibraryId);
        Assert.Equal("artist-route", result.ArtistId);
        Assert.Equal("album-route", result.AlbumId);
        Assert.Equal("track-route", result.TrackId);
    }
}
