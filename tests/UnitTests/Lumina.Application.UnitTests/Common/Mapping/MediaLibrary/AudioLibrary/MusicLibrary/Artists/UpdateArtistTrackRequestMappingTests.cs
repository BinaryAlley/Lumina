#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.Mapping.MediaLibrary.AudioLibrary.MusicLibrary.Artists;
using Lumina.Application.Core.MediaLibrary.AudioLibrary.MusicLibrary.Tracks.Commands.AddTrack;
using Lumina.Contracts.Fixtures.Core.DTO.MediaLibrary.AudioLibrary.MusicLibrary;
using Lumina.Contracts.Fixtures.Core.Requests.MediaLibrary.AudioLibrary.MusicLibrary.Artists;
using Lumina.Contracts.Requests.MediaLibrary.AudioLibrary.MusicLibrary.Artists;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Application.UnitTests.Common.Mapping.MediaLibrary.AudioLibrary.MusicLibrary.Artists;

/// <summary>
/// Contains unit tests for the <see cref="UpdateArtistTrackRequestMapping"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class UpdateArtistTrackRequestMappingTests
{
    private readonly UpdateArtistTrackRequestFixture _updateArtistTrackRequestFixture = new();
    private readonly AudioMetadataDtoFixture _audioMetadataDtoFixture = new();

    [Fact]
    public void ToCommand_WhenMappingCompleteRequest_ShouldMapAllPropertiesCorrectly()
    {
        // Arrange
        Guid libraryId = Guid.NewGuid();
        Guid artistId = Guid.NewGuid();
        Guid albumId = Guid.NewGuid();
        UpdateArtistTrackRequest request = _updateArtistTrackRequestFixture.Create();

        // Act
        AddTrackCommand result = request.ToCommand(libraryId.ToString(), artistId.ToString(), albumId.ToString());

        // Assert
        Assert.NotNull(result);
        Assert.Equal(libraryId.ToString(), result.LibraryId);
        Assert.Equal(artistId.ToString(), result.ArtistId);
        Assert.Equal(albumId.ToString(), result.AlbumId);
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
        Assert.Equal(request.TrackId, result.TrackId);
    }

    [Fact]
    public void ToCommand_WhenOptionalPropertiesAreMissing_ShouldMapTheRequiredPropertiesAndNulls()
    {
        // Arrange
        UpdateArtistTrackRequest request = _updateArtistTrackRequestFixture.Create(
            path: "/music/queen/bohemian-rhapsody.flac",
            metadata: _audioMetadataDtoFixture.Create(title: "Love of My Life"),
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
            includeTrackId: false);

        // Act
        AddTrackCommand result = request.ToCommand(Guid.NewGuid().ToString(), Guid.NewGuid().ToString(), Guid.NewGuid().ToString());

        // Assert
        Assert.NotNull(result);
        Assert.Equal("/music/queen/bohemian-rhapsody.flac", result.Path);
        Assert.Equal("Love of My Life", result.Metadata!.Title);
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
        Assert.Null(result.TrackId);
    }

    [Fact]
    public void ToCommand_WhenRouteIdentifiersAreNull_ShouldMapNullRouteIdentifiers()
    {
        // Arrange
        UpdateArtistTrackRequest request = _updateArtistTrackRequestFixture.Create();

        // Act
        AddTrackCommand result = request.ToCommand(null, null, null);

        // Assert
        Assert.Null(result.LibraryId);
        Assert.Null(result.ArtistId);
        Assert.Null(result.AlbumId);
    }

    [Fact]
    public void ToCommands_WhenMappingMultipleRequests_ShouldMapEachRequest()
    {
        // Arrange
        Guid libraryId = Guid.NewGuid();
        Guid artistId = Guid.NewGuid();
        Guid albumId = Guid.NewGuid();
        List<UpdateArtistTrackRequest> requests = _updateArtistTrackRequestFixture.CreateMany(3);

        // Act
        List<AddTrackCommand> result = [.. requests.ToCommands(libraryId.ToString(), artistId.ToString(), albumId.ToString())];

        // Assert
        Assert.Equal(requests.Count, result.Count);
        for (int i = 0; i < requests.Count; i++)
        {
            Assert.Equal(requests[i].Path, result[i].Path);
            Assert.Equal(requests[i].TrackNumber, result[i].TrackNumber);
            Assert.Equal(requests[i].TrackId, result[i].TrackId);
            Assert.Equal(libraryId.ToString(), result[i].LibraryId);
            Assert.Equal(artistId.ToString(), result[i].ArtistId);
            Assert.Equal(albumId.ToString(), result[i].AlbumId);
        }
    }
}
