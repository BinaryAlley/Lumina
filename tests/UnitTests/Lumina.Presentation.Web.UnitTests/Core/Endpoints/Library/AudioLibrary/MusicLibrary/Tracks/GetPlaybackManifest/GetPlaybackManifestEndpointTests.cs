#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Presentation.Web.Common.Api;
using Lumina.Presentation.Web.Common.DTO.AudioLibrary.MusicLibrary;
using Lumina.Presentation.Web.Common.Exceptions;
using Lumina.Presentation.Web.Common.Requests.Library.AudioLibrary.MusicLibrary.Tracks;
using Lumina.Presentation.Web.Common.Routes;
using Lumina.Presentation.Web.Core.Endpoints.Library.AudioLibrary.MusicLibrary.Tracks.GetPlaybackManifest;
using Lumina.Presentation.Web.Fixtures.Common.DTO.AudioLibrary.MusicLibrary;
using Lumina.Presentation.Web.Fixtures.Common.Requests.Library.AudioLibrary.MusicLibrary.Tracks;
using Lumina.Presentation.Web.Fixtures.Common.TestHelpers;
using Microsoft.AspNetCore.Http;
using NSubstitute;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Web.UnitTests.Core.Endpoints.Library.AudioLibrary.MusicLibrary.Tracks.GetPlaybackManifest;

/// <summary>
/// Contains unit tests for the <see cref="GetPlaybackManifestEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetPlaybackManifestEndpointTests
{
    private const string VALID_DELIVERY_ID = "0123456789abcdef0123456789abcdef";
    private readonly IApiHttpClient _mockApiHttpClient;
    private readonly GetPlaybackManifestEndpoint _sut;
    private readonly GetPlaybackManifestRequestFixture _getPlaybackManifestRequestFixture = new();
    private readonly PlaybackManifestDtoFixture _playbackManifestDtoFixture = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="GetPlaybackManifestEndpointTests"/> class.
    /// </summary>
    public GetPlaybackManifestEndpointTests()
    {
        _mockApiHttpClient = Substitute.For<IApiHttpClient>();
        _sut = Factory.Create<GetPlaybackManifestEndpoint>(_mockApiHttpClient);
    }

    [Fact]
    public async Task ExecuteAsync_WhenManifestIsNotADirectPlay_ShouldRewriteTheStreamPathsToTheWebProxyRoutes()
    {
        // Arrange
        Guid libraryId = Guid.NewGuid();
        string artistId = Guid.NewGuid().ToString();
        string albumId = Guid.NewGuid().ToString();
        string trackId = Guid.NewGuid().ToString();
        GetPlaybackManifestRequest request = _getPlaybackManifestRequestFixture.Create(
            artistId: artistId,
            albumId: albumId,
            trackId: trackId,
            libraryId: libraryId,
            includeCapabilities: false);
        PlaybackManifestDto manifest = _playbackManifestDtoFixture.Create(
            isDirectPlay: false,
            deliveryId: VALID_DELIVERY_ID,
            streamPath: "/api/v1/libraries/ancient/stream",
            segmentStreamPathTemplate: "/api/v1/libraries/ancient/stream/deliveries/{deliveryId}/segments/{segmentIndex}");
        _mockApiHttpClient.GetAsync<PlaybackManifestDto>(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(manifest);
        string expectedStreamPath = $"/en-US/library/audio-library/music-library/artists/{artistId}/albums/{albumId}/tracks/{trackId}/api-stream-track?libraryId={libraryId}";
        string expectedSegmentStreamPathTemplate = $"/en-US/library/audio-library/music-library/artists/{artistId}/albums/{albumId}/tracks/{trackId}/api-stream-track-segment/{{deliveryId}}/{{segmentIndex}}?libraryId={libraryId}";

        // Act
        IResult result = await _sut.ExecuteAsync(request, CancellationToken.None);

        // Assert
        using JsonDocument json = JsonDocument.Parse(await JsonResultTestHelper.GetResponseBodyAsync(result));
        Assert.True(json.RootElement.GetProperty("success").GetBoolean());
        JsonElement data = json.RootElement.GetProperty("data");
        Assert.Equal(expectedStreamPath, data.GetProperty("streamPath").GetString());
        Assert.Equal(expectedSegmentStreamPathTemplate, data.GetProperty("segmentStreamPathTemplate").GetString());
        Assert.Equal(VALID_DELIVERY_ID, data.GetProperty("deliveryId").GetString());
        Assert.DoesNotContain("http", data.GetProperty("streamPath").GetString(), StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("http", data.GetProperty("segmentStreamPathTemplate").GetString(), StringComparison.OrdinalIgnoreCase);
        Assert.Equal("private, no-store", _sut.HttpContext.Response.Headers["Cache-Control"].ToString());
        Assert.Equal("Cookie", _sut.HttpContext.Response.Headers["Vary"].ToString());
    }

    [Theory]
    [InlineData(true)] // a null template means the delivery has no segment route
    [InlineData(false)] // an empty template means the delivery has no segment route
    public async Task ExecuteAsync_WhenSegmentPathTemplateIsEmpty_ShouldClearTheDeliveryIdAndTheTemplate(bool useNullTemplate)
    {
        // Arrange
        GetPlaybackManifestRequest request = _getPlaybackManifestRequestFixture.Create(includeCapabilities: false);
        PlaybackManifestDto manifest = _playbackManifestDtoFixture.Create(
            isDirectPlay: false,
            deliveryId: VALID_DELIVERY_ID,
            segmentStreamPathTemplate: useNullTemplate ? null : string.Empty,
            includeSegmentStreamPathTemplate: !useNullTemplate);
        _mockApiHttpClient.GetAsync<PlaybackManifestDto>(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(manifest);

        // Act
        IResult result = await _sut.ExecuteAsync(request, CancellationToken.None);

        // Assert
        using JsonDocument json = JsonDocument.Parse(await JsonResultTestHelper.GetResponseBodyAsync(result));
        JsonElement data = json.RootElement.GetProperty("data");
        Assert.Equal(JsonValueKind.Null, data.GetProperty("deliveryId").ValueKind);
        Assert.Equal(JsonValueKind.Null, data.GetProperty("segmentStreamPathTemplate").ValueKind);
        Assert.NotEqual(JsonValueKind.Null, data.GetProperty("streamPath").ValueKind);
    }

    [Fact]
    public async Task ExecuteAsync_WhenSegmentPathTemplateIsPresentButDeliveryIdIsInvalid_ShouldThrowBadGatewayApiException()
    {
        // Arrange
        GetPlaybackManifestRequest request = _getPlaybackManifestRequestFixture.Create(includeCapabilities: false);
        PlaybackManifestDto manifest = _playbackManifestDtoFixture.Create(
            isDirectPlay: false,
            deliveryId: "not-a-valid-handle",
            segmentStreamPathTemplate: "/api/v1/libraries/ancient/stream/deliveries/{deliveryId}/segments/{segmentIndex}");
        _mockApiHttpClient.GetAsync<PlaybackManifestDto>(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(manifest);

        // Act
        Task<IResult> operationTask = _sut.ExecuteAsync(request, CancellationToken.None);

        // Assert
        ApiException apiException = await Assert.ThrowsAsync<ApiException>(() => operationTask);
        Assert.Equal(HttpStatusCode.BadGateway, apiException.HttpStatusCode);
        Assert.NotNull(apiException.ProblemDetails);
        Assert.Equal("General.Failure", apiException.ProblemDetails!.Title);
        Assert.Equal("InternalServerError", apiException.ProblemDetails.Detail);
        Assert.Equal(StatusCodes.Status502BadGateway, apiException.ProblemDetails.Status);
    }

    [Fact]
    public async Task ExecuteAsync_WhenApiReturnsNoManifest_ShouldThrowBadGatewayApiException()
    {
        // Arrange
        GetPlaybackManifestRequest request = _getPlaybackManifestRequestFixture.Create(includeCapabilities: false);
        _mockApiHttpClient.GetAsync<PlaybackManifestDto>(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<PlaybackManifestDto>(null!));

        // Act
        Task<IResult> operationTask = _sut.ExecuteAsync(request, CancellationToken.None);

        // Assert
        ApiException apiException = await Assert.ThrowsAsync<ApiException>(() => operationTask);
        Assert.Equal(HttpStatusCode.BadGateway, apiException.HttpStatusCode);
        Assert.NotNull(apiException.ProblemDetails);
        Assert.Equal("General.Failure", apiException.ProblemDetails!.Title);
        Assert.Equal("InternalServerError", apiException.ProblemDetails.Detail);
        Assert.Equal(StatusCodes.Status502BadGateway, apiException.ProblemDetails.Status);
    }

    [Theory]
    [InlineData("mp3/mp3", "?capabilities=mp3%2Fmp3")] // the capabilities are forwarded and URL-escaped
    [InlineData("flac/flac,mp3/mp3", "?capabilities=flac%2Fflac%2Cmp3%2Fmp3")] // a capability list is forwarded as one escaped value
    [InlineData("   ", "")] // whitespace only capabilities are not forwarded
    [InlineData(null, "")] // absent capabilities are not forwarded
    public async Task ExecuteAsync_WhenCapabilitiesAreProvided_ShouldForwardThemOnlyWhenTheyAreNotEmpty(string? capabilities, string expectedSuffix)
    {
        // Arrange
        string artistId = Guid.NewGuid().ToString();
        string albumId = Guid.NewGuid().ToString();
        string trackId = Guid.NewGuid().ToString();
        Guid libraryId = Guid.NewGuid();
        GetPlaybackManifestRequest request = _getPlaybackManifestRequestFixture.Create(
            artistId: artistId,
            albumId: albumId,
            trackId: trackId,
            libraryId: libraryId,
            capabilities: capabilities,
            includeCapabilities: capabilities is not null);
        string expectedEndpoint = ApiRoutes.Music.GET_PLAYBACK_MANIFEST
            .Replace("{libraryId}", libraryId.ToString())
            .Replace("{artistId}", artistId)
            .Replace("{albumId}", albumId)
            .Replace("{trackId}", trackId) + expectedSuffix;
        _mockApiHttpClient.GetAsync<PlaybackManifestDto>(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(_playbackManifestDtoFixture.Create(deliveryId: VALID_DELIVERY_ID));

        // Act
        await _sut.ExecuteAsync(request, CancellationToken.None);

        // Assert
        await _mockApiHttpClient.Received(1).GetAsync<PlaybackManifestDto>(
            Arg.Is<string>(endpoint => endpoint == expectedEndpoint),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_WhenRouteIdsAreNotParseable_ShouldForwardEmptyGuidsToTheApi()
    {
        // Arrange
        GetPlaybackManifestRequest request = _getPlaybackManifestRequestFixture.Create(artistId: "not-a-guid", albumId: "' OR '1'='1", trackId: "'; DROP TABLE Tracks; --", includeCapabilities: false);
        string expectedEndpoint = ApiRoutes.Music.GET_PLAYBACK_MANIFEST
            .Replace("{libraryId}", request.LibraryId.ToString())
            .Replace("{artistId}", Guid.Empty.ToString())
            .Replace("{albumId}", Guid.Empty.ToString())
            .Replace("{trackId}", Guid.Empty.ToString());
        _mockApiHttpClient.GetAsync<PlaybackManifestDto>(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(_playbackManifestDtoFixture.Create(deliveryId: VALID_DELIVERY_ID));

        // Act
        await _sut.ExecuteAsync(request, CancellationToken.None);

        // Assert
        await _mockApiHttpClient.Received(1).GetAsync<PlaybackManifestDto>(
            Arg.Is<string>(endpoint => endpoint == expectedEndpoint),
            Arg.Any<CancellationToken>());
    }

    [Theory]
    [InlineData("/lumina", "/lumina")] // a plain absolute path base is prefixed
    [InlineData("//evil", "")] // a protocol relative path base would point the stream at another origin
    [InlineData("/a?b", "")] // a path base with a query would truncate the built path
    public async Task ExecuteAsync_WhenPathBaseIsPresent_ShouldUseItOnlyWhenItIsSafe(string pathBase, string expectedPrefix)
    {
        // Arrange
        GetPlaybackManifestRequest request = _getPlaybackManifestRequestFixture.Create(includeCapabilities: false);
        _sut.HttpContext.Request.PathBase = pathBase;
        _mockApiHttpClient.GetAsync<PlaybackManifestDto>(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(_playbackManifestDtoFixture.Create(deliveryId: VALID_DELIVERY_ID));

        // Act
        IResult result = await _sut.ExecuteAsync(request, CancellationToken.None);

        // Assert
        using JsonDocument json = JsonDocument.Parse(await JsonResultTestHelper.GetResponseBodyAsync(result));
        string? streamPath = json.RootElement.GetProperty("data").GetProperty("streamPath").GetString();
        Assert.NotNull(streamPath);
        Assert.StartsWith($"{expectedPrefix}/en-US/library/audio-library/music-library/artists/", streamPath);
    }

    [Fact]
    public async Task ExecuteAsync_WhenCancellationRequested_ShouldCancelOperation()
    {
        // Arrange
        GetPlaybackManifestRequest request = _getPlaybackManifestRequestFixture.Create(includeCapabilities: false);
        CancellationTokenSource cancellationTokenSource = new();
        TaskCompletionSource<bool> operationStarted = new();
        TaskCompletionSource<bool> cancellationRequested = new();
        _mockApiHttpClient.GetAsync<PlaybackManifestDto>(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(callInfo => Task.Run(async () =>
            {
                operationStarted.SetResult(true);
                await cancellationRequested.Task;
                callInfo.Arg<CancellationToken>().ThrowIfCancellationRequested();
                return _playbackManifestDtoFixture.Create(deliveryId: VALID_DELIVERY_ID);
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
