#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Application.Common.CQRS;
using Lumina.Application.Core.MediaLibrary.AudioLibrary.MusicLibrary.Tracks.Queries.GetPlaybackManifest;
using Lumina.Contracts.Fixtures.Core.Responses.MediaLibrary.AudioLibrary.MusicLibrary.Playback;
using Lumina.Contracts.Responses.MediaLibrary.AudioLibrary.MusicLibrary.Playback;
using Lumina.Domain.Common.Primitives;
using Lumina.Presentation.Api.Common.Routes.Library.AudioLibrary.MusicLibrary;
using Lumina.Presentation.Api.Core.Endpoints.Library.AudioLibrary.MusicLibrary.Tracks.GetPlaybackManifest;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using NSubstitute;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using DomainErrors = Lumina.Domain.Common.Errors.Errors;
#endregion

namespace Lumina.Presentation.Api.UnitTests.Core.Endpoints.Library.AudioLibrary.MusicLibrary.Tracks.GetPlaybackManifest;

/// <summary>
/// Contains unit tests for the <see cref="GetPlaybackManifestEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetPlaybackManifestEndpointTests
{
    private readonly IQueryHandler<GetPlaybackManifestQuery, Result<PlaybackManifestResponse>> _mockHandler;
    private readonly GetPlaybackManifestEndpoint _sut;
    private readonly PlaybackManifestResponseFixture _playbackManifestResponseFixture = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="GetPlaybackManifestEndpointTests"/> class.
    /// </summary>
    public GetPlaybackManifestEndpointTests()
    {
        _mockHandler = Substitute.For<IQueryHandler<GetPlaybackManifestQuery, Result<PlaybackManifestResponse>>>();
        _sut = Factory.Create<GetPlaybackManifestEndpoint>(_mockHandler);
    }

    [Fact]
    public async Task ExecuteAsync_WhenSuccessful_ShouldReturnOkResultWithTheManifest()
    {
        // Arrange
        PlaybackManifestResponse expectedResponse = _playbackManifestResponseFixture.Create();
        _mockHandler.HandleAsync(Arg.Any<GetPlaybackManifestQuery>(), Arg.Any<CancellationToken>())
            .Returns(Result.From(expectedResponse));
        SetRouteValues();

        // Act
        IResult result = await _sut.ExecuteAsync(EmptyRequest.Instance, CancellationToken.None);

        // Assert
        Ok<PlaybackManifestResponse> okResult = Assert.IsType<Ok<PlaybackManifestResponse>>(result);
        Assert.Equal(expectedResponse, okResult.Value);
    }

    [Fact]
    public async Task ExecuteAsync_WhenSuccessful_ShouldSetPrivateNoStoreCacheHeaders()
    {
        // Arrange
        _mockHandler.HandleAsync(Arg.Any<GetPlaybackManifestQuery>(), Arg.Any<CancellationToken>())
            .Returns(Result.From(_playbackManifestResponseFixture.Create()));
        SetRouteValues();

        // Act
        await _sut.ExecuteAsync(EmptyRequest.Instance, CancellationToken.None);

        // Assert
        Assert.Equal("private, no-store", _sut.HttpContext.Response.Headers["Cache-Control"].ToString());
        Assert.Equal("Authorization", _sut.HttpContext.Response.Headers["Vary"].ToString());
    }

    [Fact]
    public async Task ExecuteAsync_WhenHandlerReturnsError_ShouldReturnProblemResult()
    {
        // Arrange
        Error expectedError = DomainErrors.Music.TrackNotFound;
        _mockHandler.HandleAsync(Arg.Any<GetPlaybackManifestQuery>(), Arg.Any<CancellationToken>())
            .Returns(expectedError);
        SetRouteValues();

        // Act
        IResult result = await _sut.ExecuteAsync(EmptyRequest.Instance, CancellationToken.None);

        // Assert
        ProblemHttpResult problemResult = Assert.IsType<ProblemHttpResult>(result);
        Assert.Equal(StatusCodes.Status404NotFound, problemResult.StatusCode);
        Assert.Equal(StatusCodes.Status404NotFound, problemResult.ProblemDetails.Status);
        Assert.Equal(expectedError.Code, problemResult.ProblemDetails.Title);
        Assert.Equal(expectedError.Description, problemResult.ProblemDetails.Detail);
        Assert.Contains("traceId", problemResult.ProblemDetails.Extensions.Keys);
    }

    [Fact]
    public async Task ExecuteAsync_WhenCalled_ShouldSendTheRouteValuesAndTheStreamPathsToTheHandler()
    {
        // Arrange
        string libraryId = "11111111-1111-1111-1111-111111111111";
        string artistId = "22222222-2222-2222-2222-222222222222";
        string albumId = "33333333-3333-3333-3333-333333333333";
        string trackId = "44444444-4444-4444-4444-444444444444";
        _mockHandler.HandleAsync(Arg.Any<GetPlaybackManifestQuery>(), Arg.Any<CancellationToken>())
            .Returns(Result.From(_playbackManifestResponseFixture.Create()));
        _sut.HttpContext.Request.RouteValues["libraryId"] = libraryId;
        _sut.HttpContext.Request.RouteValues["artistId"] = artistId;
        _sut.HttpContext.Request.RouteValues["albumId"] = albumId;
        _sut.HttpContext.Request.RouteValues["trackId"] = trackId;
        _sut.HttpContext.Request.QueryString = new QueryString("?capabilities=mp3/mp3");
        string expectedStreamPath = ApiRoutes.Tracks.STREAM_TRACK
            .Replace("{libraryId}", libraryId)
            .Replace("{artistId}", artistId)
            .Replace("{albumId}", albumId)
            .Replace("{trackId}", trackId);
        string expectedSegmentStreamPathTemplate = ApiRoutes.Tracks.STREAM_TRACK_SEGMENT
            .Replace("{libraryId}", libraryId)
            .Replace("{artistId}", artistId)
            .Replace("{albumId}", albumId)
            .Replace("{trackId}", trackId);
        CancellationToken cancellationToken = CancellationToken.None;

        // Act
        await _sut.ExecuteAsync(EmptyRequest.Instance, cancellationToken);

        // Assert
        await _mockHandler.Received(1).HandleAsync(
            Arg.Is<GetPlaybackManifestQuery>(query =>
                query.LibraryId == libraryId &&
                query.ArtistId == artistId &&
                query.AlbumId == albumId &&
                query.TrackId == trackId &&
                query.Capabilities == "mp3/mp3" &&
                query.StreamPath == expectedStreamPath &&
                query.SegmentStreamPathTemplate == expectedSegmentStreamPathTemplate &&
                query.SegmentStreamPathTemplate.Contains("{deliveryId}") &&
                query.SegmentStreamPathTemplate.Contains("{segmentIndex}")),
            Arg.Is(cancellationToken));
        Assert.DoesNotContain("http", expectedStreamPath, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("http", expectedSegmentStreamPathTemplate, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task ExecuteAsync_WhenCapabilitiesAreAbsent_ShouldSendAnEmptyCapabilitiesValueToTheHandler()
    {
        // Arrange
        _mockHandler.HandleAsync(Arg.Any<GetPlaybackManifestQuery>(), Arg.Any<CancellationToken>())
            .Returns(Result.From(_playbackManifestResponseFixture.Create()));
        SetRouteValues();

        // Act
        await _sut.ExecuteAsync(EmptyRequest.Instance, CancellationToken.None);

        // Assert
        await _mockHandler.Received(1).HandleAsync(
            Arg.Is<GetPlaybackManifestQuery>(query => query.Capabilities == string.Empty),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_WhenCancellationRequested_ShouldCancelOperation()
    {
        // Arrange
        CancellationTokenSource cancellationTokenSource = new();
        TaskCompletionSource<bool> operationStarted = new();
        TaskCompletionSource<bool> cancellationRequested = new();

        _mockHandler.HandleAsync(Arg.Any<GetPlaybackManifestQuery>(), Arg.Any<CancellationToken>())
            .Returns(callInfo => Task.Run(async () =>
            {
                operationStarted.SetResult(true);
                await cancellationRequested.Task;
                callInfo.Arg<CancellationToken>().ThrowIfCancellationRequested();
                return Result.From(_playbackManifestResponseFixture.Create());
            }, callInfo.Arg<CancellationToken>()));
        SetRouteValues();

        // Act
        Task<IResult> operationTask = _sut.ExecuteAsync(EmptyRequest.Instance, cancellationTokenSource.Token);
        await operationStarted.Task;
        cancellationTokenSource.Cancel();
        cancellationRequested.SetResult(true);

        // Assert
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => operationTask);
    }

    /// <summary>
    /// Sets the four route values the endpoint reads from the route.
    /// </summary>
    private void SetRouteValues()
    {
        _sut.HttpContext.Request.RouteValues["libraryId"] = Guid.NewGuid().ToString();
        _sut.HttpContext.Request.RouteValues["artistId"] = Guid.NewGuid().ToString();
        _sut.HttpContext.Request.RouteValues["albumId"] = Guid.NewGuid().ToString();
        _sut.HttpContext.Request.RouteValues["trackId"] = Guid.NewGuid().ToString();
    }
}
