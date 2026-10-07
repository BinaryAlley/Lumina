#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.Mapping.MediaLibrary.AudioLibrary.MusicLibrary.Tracks;
using Lumina.Application.Core.MediaLibrary.AudioLibrary.MusicLibrary.Tracks.Commands.AddTrack;
using Lumina.Contracts.Fixtures.Core.DTO.MediaLibrary.AudioLibrary.MusicLibrary;
using Lumina.Contracts.Fixtures.Core.Requests.MediaLibrary.AudioLibrary.MusicLibrary.Tracks;
using Lumina.Contracts.Requests.MediaLibrary.AudioLibrary.MusicLibrary.Tracks;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Application.UnitTests.Common.Mapping.MediaLibrary.AudioLibrary.MusicLibrary.Tracks;

/// <summary>
/// Contains unit tests for the <see cref="AddTrackRequestMapping"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class AddTrackRequestMappingTests
{
    private readonly AddTrackRequestFixture _addTrackRequestFixture = new();
    private readonly MusicTrackMetadataDtoFixture _audioMetadataDtoFixture = new();

    [Fact]
    public void ToCommand_WhenMappingCompleteRequest_ShouldMapAllPropertiesCorrectly()
    {
        // Arrange
        Guid libraryId = Guid.NewGuid();
        Guid artistId = Guid.NewGuid();
        Guid albumId = Guid.NewGuid();
        AddTrackRequest request = _addTrackRequestFixture.Create();

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
        Assert.Equal(request.Moods, result.Moods);
        Assert.Equal(request.Isrcs, result.Isrcs);
        Assert.Equal(request.Contributors, result.Contributors);
        Assert.Equal(request.Ratings, result.Ratings);
        Assert.Null(result.TrackId);
    }

    [Fact]
    public void ToCommand_WhenOptionalPropertiesAreMissing_ShouldMapTheRequiredPropertiesAndNulls()
    {
        // Arrange
        AddTrackRequest request = _addTrackRequestFixture.Create(
            path: "/music/queen/bohemian-rhapsody.flac",
            metadata: _audioMetadataDtoFixture.Create(title: "Love of My Life"),
            includeDiscNumber: false,
            includeScript: false,
            includeKey: false,
            includeBpm: false,
            includeWork: false,
            includeMusicBrainzRecordingId: false,
            includeMusicBrainzTrackId: false,
            includeMoods: false,
            includeIsrcs: false);

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
        Assert.Null(result.Moods);
        Assert.Null(result.Isrcs);
    }

    [Fact]
    public void ToCommand_WhenRouteIdentifiersAreNull_ShouldMapNullRouteIdentifiers()
    {
        // Arrange
        AddTrackRequest request = _addTrackRequestFixture.Create();

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
        List<AddTrackRequest> requests = _addTrackRequestFixture.CreateMany(3);

        // Act
        List<AddTrackCommand> result = [.. requests.ToCommands(libraryId.ToString(), artistId.ToString(), albumId.ToString())];

        // Assert
        Assert.Equal(requests.Count, result.Count);
        for (int i = 0; i < requests.Count; i++)
        {
            Assert.Equal(requests[i].Path, result[i].Path);
            Assert.Equal(requests[i].TrackNumber, result[i].TrackNumber);
            Assert.Equal(libraryId.ToString(), result[i].LibraryId);
            Assert.Equal(artistId.ToString(), result[i].ArtistId);
            Assert.Equal(albumId.ToString(), result[i].AlbumId);
        }
    }
}
