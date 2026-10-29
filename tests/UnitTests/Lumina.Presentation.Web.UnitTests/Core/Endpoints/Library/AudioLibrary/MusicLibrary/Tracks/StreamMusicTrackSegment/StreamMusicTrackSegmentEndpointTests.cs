#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Presentation.Web.Common.Api;
using Lumina.Presentation.Web.Common.DTO.Common;
using Lumina.Presentation.Web.Common.Exceptions;
using Lumina.Presentation.Web.Common.Requests.Library.AudioLibrary.MusicLibrary.Tracks;
using Lumina.Presentation.Web.Common.Routes;
using Lumina.Presentation.Web.Core.Endpoints.Library.AudioLibrary.MusicLibrary.Tracks.StreamMusicTrackSegment;
using Lumina.Presentation.Web.Fixtures.Common.Requests.Library.AudioLibrary.MusicLibrary.Tracks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Web.UnitTests.Core.Endpoints.Library.AudioLibrary.MusicLibrary.Tracks.StreamMusicTrackSegment;

/// <summary>
/// Contains unit tests for the <see cref="StreamMusicTrackSegmentEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class StreamMusicTrackSegmentEndpointTests
{
    private readonly IStreamingApiHttpClient _mockStreamingApiHttpClient;
    private readonly StreamMusicTrackSegmentEndpoint _sut;
    private readonly StreamMusicTrackSegmentRequestFixture _streamMusicTrackSegmentRequestFixture = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="StreamMusicTrackSegmentEndpointTests"/> class.
    /// </summary>
    public StreamMusicTrackSegmentEndpointTests()
    {
        _mockStreamingApiHttpClient = Substitute.For<IStreamingApiHttpClient>();
        _sut = Factory.Create<StreamMusicTrackSegmentEndpoint>(_mockStreamingApiHttpClient);
        _sut.HttpContext.Response.Body = new MemoryStream();
    }

    [Theory]
    [InlineData("uppercase")] // the handle format only accepts lowercase hexadecimal characters
    [InlineData("dashed")] // the handle format is the N format of a GUID, without dashes
    [InlineData("short")] // a handle that is shorter than the format allows
    public async Task ExecuteAsync_WhenDeliveryIdIsInvalid_ShouldReturnNotFoundWithoutCallingTheApi(string malformedHandleKind)
    {
        // Arrange
        string validDeliveryId = Guid.NewGuid().ToString("N");
        string malformedDeliveryId = malformedHandleKind switch
        {
            "uppercase" => validDeliveryId.ToUpperInvariant(),
            "dashed" => Guid.Parse(validDeliveryId).ToString("D"),
            _ => validDeliveryId[..31],
        };
        StreamMusicTrackSegmentRequest request = _streamMusicTrackSegmentRequestFixture.Create(deliveryId: malformedDeliveryId);

        // Act
        IResult result = await _sut.ExecuteAsync(request, CancellationToken.None);

        // Assert
        Assert.IsType<EmptyHttpResult>(result);
        Assert.Equal(StatusCodes.Status404NotFound, _sut.HttpContext.Response.StatusCode);
        Assert.Equal(0, _sut.HttpContext.Response.ContentLength);
        Assert.Equal(0, _sut.HttpContext.Response.Body.Length);
        await _mockStreamingApiHttpClient.DidNotReceive().GetStreamAsync(
            Arg.Any<string>(),
            Arg.Any<IReadOnlyDictionary<string, string>>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_WhenRequestIsValid_ShouldStreamTheSegmentFromTheApi()
    {
        // Arrange
        StreamMusicTrackSegmentRequest request = _streamMusicTrackSegmentRequestFixture.Create();
        string expectedEndpoint = BuildExpectedEndpoint(request);
        byte[] contentBytes = CreateBytes(512);
        ApiStreamResult streamResult = await CreateStreamResultAsync(HttpStatusCode.PartialContent, contentBytes, "audio/mp4");
        _mockStreamingApiHttpClient.GetStreamAsync(Arg.Any<string>(), Arg.Any<IReadOnlyDictionary<string, string>>(), Arg.Any<CancellationToken>())
            .Returns(streamResult);

        // Act
        IResult result = await _sut.ExecuteAsync(request, CancellationToken.None);

        // Assert
        Assert.IsType<EmptyHttpResult>(result);
        await _mockStreamingApiHttpClient.Received(1).GetStreamAsync(
            Arg.Is<string>(endpoint => endpoint == expectedEndpoint),
            Arg.Is<IReadOnlyDictionary<string, string>>(headers => headers.Count == 0),
            Arg.Any<CancellationToken>());
        Assert.Equal(StatusCodes.Status206PartialContent, _sut.HttpContext.Response.StatusCode);
        Assert.Equal("audio/mp4", _sut.HttpContext.Response.ContentType);
        Assert.Equal(contentBytes.Length, _sut.HttpContext.Response.ContentLength);
        Assert.Equal(contentBytes, ReadResponseBody(_sut.HttpContext));
        Assert.Equal("Cookie", _sut.HttpContext.Response.Headers["Vary"].ToString());
        Assert.Contains("private", _sut.HttpContext.Response.Headers["Cache-Control"].ToString());
    }

    [Theory]
    [InlineData("artistId", "' OR '1'='1")] // an unparseable artist Id is normalized to an empty GUID
    [InlineData("albumId", "'; DROP TABLE Tracks; --")] // an unparseable album Id is normalized to an empty GUID
    [InlineData("trackId", "not-a-guid")] // an unparseable track Id is normalized to an empty GUID
    public async Task ExecuteAsync_WhenRouteIdsAreNotParseable_ShouldForwardEmptyGuidsToTheApi(string routeValue, string invalidValue)
    {
        // Arrange
        StreamMusicTrackSegmentRequest request = routeValue switch
        {
            "artistId" => _streamMusicTrackSegmentRequestFixture.Create(artistId: invalidValue),
            "albumId" => _streamMusicTrackSegmentRequestFixture.Create(albumId: invalidValue),
            _ => _streamMusicTrackSegmentRequestFixture.Create(trackId: invalidValue),
        };
        string expectedEndpoint = ApiRoutes.Music.STREAM_TRACK_SEGMENT
            .Replace("{libraryId}", request.LibraryId.ToString())
            .Replace("{artistId}", routeValue == "artistId" ? Guid.Empty.ToString() : request.ArtistId)
            .Replace("{albumId}", routeValue == "albumId" ? Guid.Empty.ToString() : request.AlbumId)
            .Replace("{trackId}", routeValue == "trackId" ? Guid.Empty.ToString() : request.TrackId)
            .Replace("{deliveryId}", request.DeliveryId)
            .Replace("{segmentIndex}", request.SegmentIndex.ToString(CultureInfo.InvariantCulture));
        _mockStreamingApiHttpClient.GetStreamAsync(Arg.Any<string>(), Arg.Any<IReadOnlyDictionary<string, string>>(), Arg.Any<CancellationToken>())
            .Returns(await CreateStreamResultAsync(HttpStatusCode.OK, CreateBytes(8), "audio/mpeg"));

        // Act
        await _sut.ExecuteAsync(request, CancellationToken.None);

        // Assert
        await _mockStreamingApiHttpClient.Received(1).GetStreamAsync(
            Arg.Is<string>(endpoint => endpoint == expectedEndpoint),
            Arg.Any<IReadOnlyDictionary<string, string>>(),
            Arg.Any<CancellationToken>());
    }

    [Theory]
    [InlineData(HttpStatusCode.ServiceUnavailable)] // a busy normalization pipeline is a transient condition
    [InlineData(HttpStatusCode.TooManyRequests)] // the stream concurrency limit was reached
    public async Task ExecuteAsync_WhenApiReportsATransientCondition_ShouldReturnABareStatusWithoutThrowing(HttpStatusCode statusCode)
    {
        // Arrange
        StreamMusicTrackSegmentRequest request = _streamMusicTrackSegmentRequestFixture.Create();
        ApiException apiException = new(
            new ProblemDetailsDto { Title = "General.Failure", Detail = "InternalServerError", Status = (int)statusCode },
            statusCode,
            BuildExpectedEndpoint(request));
        _mockStreamingApiHttpClient.GetStreamAsync(Arg.Any<string>(), Arg.Any<IReadOnlyDictionary<string, string>>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromException<ApiStreamResult>(apiException));

        // Act
        IResult result = await _sut.ExecuteAsync(request, CancellationToken.None);

        // Assert
        Assert.IsType<EmptyHttpResult>(result);
        Assert.Equal((int)statusCode, _sut.HttpContext.Response.StatusCode);
        Assert.Equal(0, _sut.HttpContext.Response.ContentLength);
        Assert.Equal(0, _sut.HttpContext.Response.Body.Length);
    }

    [Fact]
    public async Task ExecuteAsync_WhenApiReportsAnUnexpectedFailure_ShouldPropagateTheApiException()
    {
        // Arrange
        StreamMusicTrackSegmentRequest request = _streamMusicTrackSegmentRequestFixture.Create();
        ApiException apiException = new(
            new ProblemDetailsDto { Title = "General.Failure", Detail = "InternalServerError", Status = StatusCodes.Status500InternalServerError },
            HttpStatusCode.InternalServerError,
            BuildExpectedEndpoint(request));
        _mockStreamingApiHttpClient.GetStreamAsync(Arg.Any<string>(), Arg.Any<IReadOnlyDictionary<string, string>>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromException<ApiStreamResult>(apiException));

        // Act
        Task<IResult> operationTask = _sut.ExecuteAsync(request, CancellationToken.None);

        // Assert
        await Assert.ThrowsAsync<ApiException>(() => operationTask);
    }

    [Fact]
    public async Task ExecuteAsync_WhenCancellationRequested_ShouldCancelOperation()
    {
        // Arrange
        StreamMusicTrackSegmentRequest request = _streamMusicTrackSegmentRequestFixture.Create();
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
    /// Builds the upstream endpoint the segment request is expected to be sent to.
    /// </summary>
    /// <param name="request">The request to build the endpoint for.</param>
    /// <returns>The expected upstream endpoint.</returns>
    private static string BuildExpectedEndpoint(StreamMusicTrackSegmentRequest request)
    {
        return ApiRoutes.Music.STREAM_TRACK_SEGMENT
            .Replace("{libraryId}", request.LibraryId.ToString())
            .Replace("{artistId}", request.ArtistId)
            .Replace("{albumId}", request.AlbumId)
            .Replace("{trackId}", request.TrackId)
            .Replace("{deliveryId}", request.DeliveryId)
            .Replace("{segmentIndex}", request.SegmentIndex.ToString(CultureInfo.InvariantCulture));
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
    /// Creates a deterministic byte array that stands in for the content of a segment.
    /// </summary>
    /// <param name="length">The number of bytes to create.</param>
    /// <returns>The created bytes.</returns>
    private static byte[] CreateBytes(int length)
    {
        return [.. Enumerable.Range(0, length).Select(index => (byte)(index % 256))];
    }
}
