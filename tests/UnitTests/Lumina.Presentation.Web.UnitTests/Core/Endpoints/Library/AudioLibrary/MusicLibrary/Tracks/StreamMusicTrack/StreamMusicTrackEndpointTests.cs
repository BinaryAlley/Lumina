#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Presentation.Web.Common.Api;
using Lumina.Presentation.Web.Common.Requests.Library.AudioLibrary.MusicLibrary.Tracks;
using Lumina.Presentation.Web.Common.Routes;
using Lumina.Presentation.Web.Core.Endpoints.Library.AudioLibrary.MusicLibrary.Tracks.StreamMusicTrack;
using Lumina.Presentation.Web.Fixtures.Common.Requests.Library.AudioLibrary.MusicLibrary.Tracks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Web.UnitTests.Core.Endpoints.Library.AudioLibrary.MusicLibrary.Tracks.StreamMusicTrack;

/// <summary>
/// Contains unit tests for the <see cref="StreamMusicTrackEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class StreamMusicTrackEndpointTests
{
    private readonly IStreamingApiHttpClient _mockStreamingApiHttpClient;
    private readonly StreamMusicTrackEndpoint _sut;
    private readonly StreamMusicTrackRequestFixture _streamMusicTrackRequestFixture = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="StreamMusicTrackEndpointTests"/> class.
    /// </summary>
    public StreamMusicTrackEndpointTests()
    {
        _mockStreamingApiHttpClient = Substitute.For<IStreamingApiHttpClient>();
        _sut = Factory.Create<StreamMusicTrackEndpoint>(_mockStreamingApiHttpClient);
        _sut.HttpContext.Response.Body = new MemoryStream();
    }

    [Fact]
    public async Task ExecuteAsync_WhenCalled_ShouldStreamTheTrackAndReturnEmptyResult()
    {
        // Arrange
        StreamMusicTrackRequest request = _streamMusicTrackRequestFixture.Create();
        string expectedEndpoint = BuildExpectedEndpoint(request);
        byte[] contentBytes = CreateBytes(1024);
        ApiStreamResult streamResult = await CreateStreamResultAsync(HttpStatusCode.OK, contentBytes, "audio/flac");
        _mockStreamingApiHttpClient.GetStreamAsync(Arg.Any<string>(), Arg.Any<IReadOnlyDictionary<string, string>>(), Arg.Any<CancellationToken>())
            .Returns(streamResult);

        // Act
        IResult result = await _sut.ExecuteAsync(request, CancellationToken.None);

        // Assert
        Assert.IsType<EmptyHttpResult>(result);
        await _mockStreamingApiHttpClient.Received(1).GetStreamAsync(
            Arg.Is<string>(endpoint => endpoint == expectedEndpoint),
            Arg.Any<IReadOnlyDictionary<string, string>>(),
            Arg.Any<CancellationToken>());
        Assert.Equal(StatusCodes.Status200OK, _sut.HttpContext.Response.StatusCode);
        Assert.Equal("audio/flac", _sut.HttpContext.Response.ContentType);
        Assert.Equal(contentBytes.Length, _sut.HttpContext.Response.ContentLength);
        Assert.Equal(contentBytes, ReadResponseBody(_sut.HttpContext));
        Assert.Equal("Cookie", _sut.HttpContext.Response.Headers["Vary"].ToString());
        Assert.Contains("private", _sut.HttpContext.Response.Headers["Cache-Control"].ToString());
    }

    [Fact]
    public async Task ExecuteAsync_WhenConditionalHeadersArePresent_ShouldForwardThemExactly()
    {
        // Arrange
        StreamMusicTrackRequest request = _streamMusicTrackRequestFixture.Create();
        _sut.HttpContext.Request.Headers["Range"] = "bytes=100-199";
        _sut.HttpContext.Request.Headers["If-Range"] = "\"etag-value\"";
        _sut.HttpContext.Request.Headers["If-None-Match"] = "\"etag-value\"";
        _sut.HttpContext.Request.Headers["If-Modified-Since"] = "Wed, 21 Oct 2015 07:28:00 GMT";
        _mockStreamingApiHttpClient.GetStreamAsync(Arg.Any<string>(), Arg.Any<IReadOnlyDictionary<string, string>>(), Arg.Any<CancellationToken>())
            .Returns(await CreateStreamResultAsync(HttpStatusCode.OK, CreateBytes(16)));

        // Act
        await _sut.ExecuteAsync(request, CancellationToken.None);

        // Assert
        await _mockStreamingApiHttpClient.Received(1).GetStreamAsync(
            Arg.Any<string>(),
            Arg.Is<IReadOnlyDictionary<string, string>>(headers =>
                headers.Count == 4 &&
                headers["Range"] == "bytes=100-199" &&
                headers["If-Range"] == "\"etag-value\"" &&
                headers["If-None-Match"] == "\"etag-value\"" &&
                headers["If-Modified-Since"] == "Wed, 21 Oct 2015 07:28:00 GMT"),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_WhenConditionalHeadersAreAbsentOrWhitespace_ShouldNotForwardThem()
    {
        // Arrange
        StreamMusicTrackRequest request = _streamMusicTrackRequestFixture.Create();
        _sut.HttpContext.Request.Headers["Range"] = "   ";
        _mockStreamingApiHttpClient.GetStreamAsync(Arg.Any<string>(), Arg.Any<IReadOnlyDictionary<string, string>>(), Arg.Any<CancellationToken>())
            .Returns(await CreateStreamResultAsync(HttpStatusCode.OK, CreateBytes(16)));

        // Act
        await _sut.ExecuteAsync(request, CancellationToken.None);

        // Assert
        await _mockStreamingApiHttpClient.Received(1).GetStreamAsync(
            Arg.Any<string>(),
            Arg.Is<IReadOnlyDictionary<string, string>>(headers => headers.Count == 0),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_WhenRouteIdsAreNotParseable_ShouldForwardEmptyGuidsToTheApi()
    {
        // Arrange
        StreamMusicTrackRequest request = _streamMusicTrackRequestFixture.Create(artistId: "not-a-guid", albumId: "' OR '1'='1", trackId: "'; DROP TABLE Tracks; --");
        string expectedEndpoint = ApiRoutes.Music.STREAM_TRACK
            .Replace("{libraryId}", request.LibraryId.ToString())
            .Replace("{artistId}", Guid.Empty.ToString())
            .Replace("{albumId}", Guid.Empty.ToString())
            .Replace("{trackId}", Guid.Empty.ToString());
        _mockStreamingApiHttpClient.GetStreamAsync(Arg.Any<string>(), Arg.Any<IReadOnlyDictionary<string, string>>(), Arg.Any<CancellationToken>())
            .Returns(await CreateStreamResultAsync(HttpStatusCode.OK, CreateBytes(8)));

        // Act
        await _sut.ExecuteAsync(request, CancellationToken.None);

        // Assert
        await _mockStreamingApiHttpClient.Received(1).GetStreamAsync(
            Arg.Is<string>(endpoint => endpoint == expectedEndpoint),
            Arg.Any<IReadOnlyDictionary<string, string>>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_WhenCancellationRequested_ShouldCancelOperation()
    {
        // Arrange
        StreamMusicTrackRequest request = _streamMusicTrackRequestFixture.Create();
        CancellationTokenSource cancellationTokenSource = new();
        TaskCompletionSource<bool> operationStarted = new();
        TaskCompletionSource<bool> cancellationRequested = new();
        _mockStreamingApiHttpClient.GetStreamAsync(Arg.Any<string>(), Arg.Any<IReadOnlyDictionary<string, string>>(), Arg.Any<CancellationToken>())
            .Returns(callInfo => Task.Run(async () =>
            {
                operationStarted.SetResult(true);
                await cancellationRequested.Task;
                callInfo.Arg<CancellationToken>().ThrowIfCancellationRequested();
                return await CreateStreamResultAsync(HttpStatusCode.OK, CreateBytes(8));
            }, callInfo.Arg<CancellationToken>()));

        // Act
        Task<IResult> operationTask = _sut.ExecuteAsync(request, cancellationTokenSource.Token);
        await operationStarted.Task;
        cancellationTokenSource.Cancel();
        cancellationRequested.SetResult(true);

        // Assert
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => operationTask);
    }

    /// <summary>
    /// Builds the upstream endpoint the track stream request is expected to be sent to.
    /// </summary>
    /// <param name="request">The request to build the endpoint for.</param>
    /// <returns>The expected upstream endpoint.</returns>
    private static string BuildExpectedEndpoint(StreamMusicTrackRequest request)
    {
        return ApiRoutes.Music.STREAM_TRACK
            .Replace("{libraryId}", request.LibraryId.ToString())
            .Replace("{artistId}", request.ArtistId)
            .Replace("{albumId}", request.AlbumId)
            .Replace("{trackId}", request.TrackId);
    }

    /// <summary>
    /// Creates a streamed API response with a status code, a body and an optional media type.
    /// </summary>
    /// <param name="statusCode">The status code of the response.</param>
    /// <param name="contentBytes">The bytes of the content of the response.</param>
    /// <param name="contentType">The optional media type of the content.</param>
    /// <returns>The created streamed response.</returns>
    private static async Task<ApiStreamResult> CreateStreamResultAsync(HttpStatusCode statusCode, byte[] contentBytes, string? contentType = null)
    {
        HttpResponseMessage response = new(statusCode)
        {
            Content = new ByteArrayContent(contentBytes)
        };
        if (contentType is not null)
            response.Content.Headers.ContentType = new MediaTypeHeaderValue(contentType);
        return await ApiStreamResult.CreateAsync(response, CancellationToken.None);
    }

    /// <summary>
    /// Reads the bytes written to the response body of the provided HTTP context.
    /// </summary>
    /// <param name="httpContext">The HTTP context whose response body is read.</param>
    /// <returns>The bytes written to the response body.</returns>
    private static byte[] ReadResponseBody(HttpContext httpContext)
    {
        httpContext.Response.Body.Position = 0;
        using MemoryStream writtenBody = new();
        httpContext.Response.Body.CopyTo(writtenBody);
        return writtenBody.ToArray();
    }

    /// <summary>
    /// Creates a deterministic byte array that stands in for the content of a track.
    /// </summary>
    /// <param name="length">The number of bytes to create.</param>
    /// <returns>The created bytes.</returns>
    private static byte[] CreateBytes(int length)
    {
        return [.. Enumerable.Range(0, length).Select(index => (byte)(index % 256))];
    }
}
