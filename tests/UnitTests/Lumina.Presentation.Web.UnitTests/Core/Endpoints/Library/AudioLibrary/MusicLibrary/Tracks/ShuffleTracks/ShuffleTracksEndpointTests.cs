#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Presentation.Web.Common.Api;
using Lumina.Presentation.Web.Common.DTO.AudioLibrary.MusicLibrary;
using Lumina.Presentation.Web.Common.Requests.Library.AudioLibrary.MusicLibrary.Tracks;
using Lumina.Presentation.Web.Common.Routes;
using Lumina.Presentation.Web.Core.Endpoints.Library.AudioLibrary.MusicLibrary.Tracks.ShuffleTracks;
using Lumina.Presentation.Web.Fixtures.Common.DTO.AudioLibrary.MusicLibrary;
using Lumina.Presentation.Web.Fixtures.Common.Requests.Library.AudioLibrary.MusicLibrary.Tracks;
using Lumina.Presentation.Web.Fixtures.Common.TestHelpers;
using Microsoft.AspNetCore.Http;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Web.UnitTests.Core.Endpoints.Library.AudioLibrary.MusicLibrary.Tracks.ShuffleTracks;

/// <summary>
/// Contains unit tests for the <see cref="ShuffleTracksEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class ShuffleTracksEndpointTests
{
    private readonly IApiHttpClient _mockApiHttpClient;
    private readonly ShuffleTracksEndpoint _sut;
    private readonly ShuffleTracksRequestFixture _shuffleTracksRequestFixture = new();
    private readonly MusicShuffleTrackDtoFixture _musicShuffleTrackDtoFixture = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="ShuffleTracksEndpointTests"/> class.
    /// </summary>
    public ShuffleTracksEndpointTests()
    {
        _mockApiHttpClient = Substitute.For<IApiHttpClient>();
        _sut = Factory.Create<ShuffleTracksEndpoint>(_mockApiHttpClient);
    }

    [Fact]
    public async Task ExecuteAsync_WhenApiReturnsTracks_ShouldReturnSuccessJsonWithTheMappedTracks()
    {
        // Arrange
        Guid libraryId = Guid.NewGuid();
        Guid trackId = Guid.NewGuid();
        Guid albumId = Guid.NewGuid();
        Guid artistId = Guid.NewGuid();
        ShuffleTracksRequest request = _shuffleTracksRequestFixture.Create(libraryId: libraryId);
        MusicShuffleTrackDto expectedTrack = _musicShuffleTrackDtoFixture.Create(
            trackId: trackId,
            title: "Bohemian Rhapsody",
            durationInSeconds: 355,
            albumId: albumId,
            albumTitle: "A Night at the Opera",
            artistId: artistId,
            coverPath: "a-night-at-the-opera-cover.jpg");
        _mockApiHttpClient.PostAsync<List<MusicShuffleTrackDto>, ShuffleTracksRequest>(Arg.Any<string>(), Arg.Any<ShuffleTracksRequest>(), Arg.Any<CancellationToken>())
            .Returns([expectedTrack]);

        // Act
        IResult result = await _sut.ExecuteAsync(request, CancellationToken.None);
        string body = await JsonResultTestHelper.GetResponseBodyAsync(result);

        // Assert
        using JsonDocument jsonDocument = JsonDocument.Parse(body);
        Assert.True(jsonDocument.RootElement.GetProperty("success").GetBoolean());
        JsonElement data = jsonDocument.RootElement.GetProperty("data");
        Assert.Equal(1, data.GetArrayLength());
        JsonElement track = data[0];
        Assert.Equal(trackId, track.GetProperty("trackId").GetGuid());
        Assert.Equal("Bohemian Rhapsody", track.GetProperty("title").GetString());
        Assert.Equal(355, track.GetProperty("durationInSeconds").GetInt32());
        Assert.Equal(albumId, track.GetProperty("albumId").GetGuid());
        Assert.Equal("A Night at the Opera", track.GetProperty("albumTitle").GetString());
        Assert.Equal(artistId, track.GetProperty("artistId").GetGuid());
        Assert.Equal("a-night-at-the-opera-cover.jpg", track.GetProperty("coverPath").GetString());
    }

    [Fact]
    public async Task ExecuteAsync_WhenApiReturnsNoTracks_ShouldReturnAnEmptyDataArray()
    {
        // Arrange
        ShuffleTracksRequest request = _shuffleTracksRequestFixture.Create();
        _mockApiHttpClient.PostAsync<List<MusicShuffleTrackDto>, ShuffleTracksRequest>(Arg.Any<string>(), Arg.Any<ShuffleTracksRequest>(), Arg.Any<CancellationToken>())
            .Returns([]);

        // Act
        IResult result = await _sut.ExecuteAsync(request, CancellationToken.None);
        string body = await JsonResultTestHelper.GetResponseBodyAsync(result);

        // Assert
        using JsonDocument jsonDocument = JsonDocument.Parse(body);
        Assert.True(jsonDocument.RootElement.GetProperty("success").GetBoolean());
        JsonElement data = jsonDocument.RootElement.GetProperty("data");
        Assert.Equal(JsonValueKind.Array, data.ValueKind);
        Assert.Equal(0, data.GetArrayLength());
    }

    [Fact]
    public async Task ExecuteAsync_WhenCalled_ShouldPostTheRequestToTheLibraryShuffleEndpoint()
    {
        // Arrange
        Guid libraryId = Guid.NewGuid();
        Guid artistId = Guid.NewGuid();
        Guid albumId = Guid.NewGuid();
        Guid firstExcludedTrackId = Guid.NewGuid();
        Guid secondExcludedTrackId = Guid.NewGuid();
        ShuffleTracksRequest request = _shuffleTracksRequestFixture.Create(
            libraryId: libraryId,
            count: 25,
            artistId: artistId,
            albumId: albumId,
            excludedTrackIds: [firstExcludedTrackId, secondExcludedTrackId]);
        string expectedEndpoint = ApiRoutes.Music.SHUFFLE_TRACKS.Replace("{libraryId}", libraryId.ToString());
        _mockApiHttpClient.PostAsync<List<MusicShuffleTrackDto>, ShuffleTracksRequest>(Arg.Any<string>(), Arg.Any<ShuffleTracksRequest>(), Arg.Any<CancellationToken>())
            .Returns([]);

        // Act
        await _sut.ExecuteAsync(request, CancellationToken.None);

        // Assert
        await _mockApiHttpClient.Received(1).PostAsync<List<MusicShuffleTrackDto>, ShuffleTracksRequest>(
            Arg.Is<string>(endpoint => endpoint == expectedEndpoint),
            Arg.Is<ShuffleTracksRequest>(forwarded =>
                forwarded.LibraryId == libraryId &&
                forwarded.Count == 25 &&
                forwarded.ArtistId == artistId &&
                forwarded.AlbumId == albumId &&
                forwarded.ExcludedTrackIds != null &&
                forwarded.ExcludedTrackIds.SequenceEqual(new[] { firstExcludedTrackId, secondExcludedTrackId })),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_WhenCancellationRequested_ShouldCancelOperation()
    {
        // Arrange
        ShuffleTracksRequest request = _shuffleTracksRequestFixture.Create();
        CancellationTokenSource cancellationTokenSource = new();
        TaskCompletionSource<bool> operationStarted = new();
        TaskCompletionSource<bool> cancellationRequested = new();
        _mockApiHttpClient.PostAsync<List<MusicShuffleTrackDto>, ShuffleTracksRequest>(Arg.Any<string>(), Arg.Any<ShuffleTracksRequest>(), Arg.Any<CancellationToken>())
            .Returns(callInfo => Task.Run(async () =>
            {
                operationStarted.SetResult(true);
                await cancellationRequested.Task;
                callInfo.Arg<CancellationToken>().ThrowIfCancellationRequested();
                return new List<MusicShuffleTrackDto> { _musicShuffleTrackDtoFixture.Create() };
            }, callInfo.Arg<CancellationToken>()));

        // Act
        Task<IResult> operationTask = _sut.ExecuteAsync(request, cancellationTokenSource.Token);
        await operationStarted.Task;
        cancellationTokenSource.Cancel();
        cancellationRequested.SetResult(true);

        // Assert
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => operationTask);
    }
}
