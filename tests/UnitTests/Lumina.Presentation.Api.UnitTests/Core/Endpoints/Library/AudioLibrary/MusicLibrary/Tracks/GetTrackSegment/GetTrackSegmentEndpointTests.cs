#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Application.Common.CQRS;
using Lumina.Application.Common.DTO.MediaLibrary.AudioLibrary.MusicLibrary.Playback;
using Lumina.Application.Core.MediaLibrary.AudioLibrary.MusicLibrary.Tracks.Queries.GetTrackSegment;
using Lumina.Application.Fixtures.Common.DTO.MediaLibrary.AudioLibrary.MusicLibrary.Playback;
using Lumina.Domain.Common.Primitives;
using Lumina.Presentation.Api.Core.Endpoints.Common;
using Lumina.Presentation.Api.Core.Endpoints.Library.AudioLibrary.MusicLibrary.Tracks.GetTrackSegment;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DomainErrors = Lumina.Domain.Common.Errors.Errors;
#endregion

namespace Lumina.Presentation.Api.UnitTests.Core.Endpoints.Library.AudioLibrary.MusicLibrary.Tracks.GetTrackSegment;

/// <summary>
/// Contains unit tests for the <see cref="GetTrackSegmentEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetTrackSegmentEndpointTests
{
    private readonly IQueryHandler<GetTrackSegmentQuery, Result<NormalizedTrackSegmentDataDto>> _mockHandler;
    private readonly GetTrackSegmentEndpoint _sut;
    private readonly NormalizedTrackSegmentDataDtoFixture _normalizedTrackSegmentDataDtoFixture = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="GetTrackSegmentEndpointTests"/> class.
    /// </summary>
    public GetTrackSegmentEndpointTests()
    {
        _mockHandler = Substitute.For<IQueryHandler<GetTrackSegmentQuery, Result<NormalizedTrackSegmentDataDto>>>();
        _sut = Factory.Create<GetTrackSegmentEndpoint>(_mockHandler);
    }

    [Theory]
    [InlineData("MP4", "audio/mp4")] // mp4 delivery container
    [InlineData("OGG", "audio/ogg")] // ogg delivery container
    [InlineData("MP3", "audio/mpeg")] // mp3 delivery container
    [InlineData("not-a-container", "application/octet-stream")] // an unknown delivery container falls back to opaque binary
    public async Task ExecuteAsync_WhenSegmentIsProduced_ShouldReturnDisposingStreamResultWithTheSegment(string container, string expectedContentType)
    {
        // Arrange
        byte[] segmentBytes = CreateBytes(256);
        MemoryStream segmentContent = new(segmentBytes);
        NormalizedTrackSegmentDataDto segment = _normalizedTrackSegmentDataDtoFixture.Create(
            content: segmentContent,
            length: segmentBytes.Length,
            startSeconds: 6.5m,
            container: container,
            segmentCount: 12,
            durationInSeconds: 300);
        _mockHandler.HandleAsync(Arg.Any<GetTrackSegmentQuery>(), Arg.Any<CancellationToken>())
            .Returns(Result.From(segment));
        SetRouteValues();

        // Act
        IResult result = await _sut.ExecuteAsync(EmptyRequest.Instance, CancellationToken.None);

        // Assert
        DisposingStreamResult streamResult = Assert.IsType<DisposingStreamResult>(result);
        Assert.Equal("nosniff", _sut.HttpContext.Response.Headers["X-Content-Type-Options"].ToString());
        Assert.Equal("private, no-store", _sut.HttpContext.Response.Headers["Cache-Control"].ToString());
        Assert.Equal("Authorization", _sut.HttpContext.Response.Headers["Vary"].ToString());
        Assert.Equal("12", _sut.HttpContext.Response.Headers["X-Lumina-Segment-Count"].ToString());
        Assert.Equal("6.5", _sut.HttpContext.Response.Headers["X-Segment-Start-Seconds"].ToString());
        Assert.Equal("300", _sut.HttpContext.Response.Headers["X-Lumina-Track-Duration-Seconds"].ToString());
        _sut.HttpContext.Response.Body = new MemoryStream();
        await streamResult.ExecuteAsync(_sut.HttpContext);
        Assert.Equal(expectedContentType, _sut.HttpContext.Response.ContentType);
        Assert.Equal(segmentBytes.Length, _sut.HttpContext.Response.ContentLength);
        Assert.Equal(segmentBytes, ReadResponseBody(_sut.HttpContext));
        Assert.Throws<ObjectDisposedException>(() => segmentContent.ReadByte());
    }

    [Fact]
    public async Task ExecuteAsync_WhenCurrentCultureUsesACommaDecimalSeparator_ShouldWriteTheSegmentStartInvariantly()
    {
        // Arrange
        CultureInfo previousCulture = CultureInfo.CurrentCulture;
        MemoryStream segmentContent = new(CreateBytes(16));
        NormalizedTrackSegmentDataDto segment = _normalizedTrackSegmentDataDtoFixture.Create(content: segmentContent, startSeconds: 3.25m);
        _mockHandler.HandleAsync(Arg.Any<GetTrackSegmentQuery>(), Arg.Any<CancellationToken>())
            .Returns(Result.From(segment));
        SetRouteValues();

        try
        {
            CultureInfo.CurrentCulture = new CultureInfo("de-DE");

            // Act
            await _sut.ExecuteAsync(EmptyRequest.Instance, CancellationToken.None);

            // Assert
            Assert.Equal("3.25", _sut.HttpContext.Response.Headers["X-Segment-Start-Seconds"].ToString());
        }
        finally
        {
            CultureInfo.CurrentCulture = previousCulture;
        }
    }

    [Fact]
    public async Task ExecuteAsync_WhenHandlerReturnsValidationErrors_ShouldReturnValidationProblemResult()
    {
        // Arrange
        List<Error> errors = [DomainErrors.Playback.DeliveryIdIsInvalid, DomainErrors.Playback.SegmentIndexMustBeValid];
        _mockHandler.HandleAsync(Arg.Any<GetTrackSegmentQuery>(), Arg.Any<CancellationToken>())
            .Returns(errors);
        SetRouteValues();

        // Act
        IResult result = await _sut.ExecuteAsync(EmptyRequest.Instance, CancellationToken.None);

        // Assert
        ProblemHttpResult problemResult = Assert.IsType<ProblemHttpResult>(result);
        Assert.Equal(StatusCodes.Status422UnprocessableEntity, problemResult.StatusCode);
        Assert.Equal("application/problem+json", problemResult.ContentType);
        HttpValidationProblemDetails validationProblemDetails = Assert.IsType<HttpValidationProblemDetails>(problemResult.ProblemDetails);
        Assert.Equal(StatusCodes.Status422UnprocessableEntity, validationProblemDetails.Status);
        Assert.Equal("General.Validation", validationProblemDetails.Title);
        Assert.Equal("OneOrMoreValidationErrorsOccurred", validationProblemDetails.Detail);
        Assert.Contains("General.Validation", validationProblemDetails.Errors.Keys);
        Assert.Contains("DeliveryIdIsInvalid", validationProblemDetails.Errors["General.Validation"]);
        Assert.Contains("SegmentIndexMustBeValid", validationProblemDetails.Errors["General.Validation"]);
    }

    [Theory]
    [InlineData("NotFound", StatusCodes.Status404NotFound)] // the delivery, track or segment does not exist
    [InlineData("Conflict", StatusCodes.Status409Conflict)] // the delivery changed since the manifest declared it
    [InlineData("ResourceUnavailable", StatusCodes.Status503ServiceUnavailable)] // the normalization pipeline is temporarily unavailable
    public async Task ExecuteAsync_WhenHandlerReturnsAProblemError_ShouldReturnTheMappedStatusCode(string errorKind, int expectedStatusCode)
    {
        // Arrange
        Error error = errorKind switch
        {
            "NotFound" => DomainErrors.Music.TrackFileNotFound,
            "Conflict" => DomainErrors.Playback.PlaybackDeliveryChanged,
            _ => DomainErrors.Playback.NormalizationUnavailable,
        };
        _mockHandler.HandleAsync(Arg.Any<GetTrackSegmentQuery>(), Arg.Any<CancellationToken>())
            .Returns(error);
        SetRouteValues();

        // Act
        IResult result = await _sut.ExecuteAsync(EmptyRequest.Instance, CancellationToken.None);

        // Assert
        ProblemHttpResult problemResult = Assert.IsType<ProblemHttpResult>(result);
        Assert.Equal(expectedStatusCode, problemResult.StatusCode);
        Assert.Equal(expectedStatusCode, problemResult.ProblemDetails.Status);
        Assert.Equal(error.Code, problemResult.ProblemDetails.Title);
        Assert.Equal(error.Description, problemResult.ProblemDetails.Detail);
    }

    [Fact]
    public async Task ExecuteAsync_WhenCalled_ShouldSendTheExactRouteValuesToTheHandler()
    {
        // Arrange
        string libraryId = "11111111-1111-1111-1111-111111111111";
        string artistId = "22222222-2222-2222-2222-222222222222";
        string albumId = "33333333-3333-3333-3333-333333333333";
        string trackId = "44444444-4444-4444-4444-444444444444";
        string deliveryId = "0123456789abcdef0123456789abcdef";
        string segmentIndex = "7";
        _mockHandler.HandleAsync(Arg.Any<GetTrackSegmentQuery>(), Arg.Any<CancellationToken>())
            .Returns(Result.From(_normalizedTrackSegmentDataDtoFixture.Create()));
        _sut.HttpContext.Request.RouteValues["libraryId"] = libraryId;
        _sut.HttpContext.Request.RouteValues["artistId"] = artistId;
        _sut.HttpContext.Request.RouteValues["albumId"] = albumId;
        _sut.HttpContext.Request.RouteValues["trackId"] = trackId;
        _sut.HttpContext.Request.RouteValues["deliveryId"] = deliveryId;
        _sut.HttpContext.Request.RouteValues["segmentIndex"] = segmentIndex;
        CancellationToken cancellationToken = CancellationToken.None;

        // Act
        await _sut.ExecuteAsync(EmptyRequest.Instance, cancellationToken);

        // Assert
        await _mockHandler.Received(1).HandleAsync(
            Arg.Is<GetTrackSegmentQuery>(query =>
                query.LibraryId == libraryId &&
                query.ArtistId == artistId &&
                query.AlbumId == albumId &&
                query.TrackId == trackId &&
                query.DeliveryId == deliveryId &&
                query.SegmentIndex == segmentIndex),
            Arg.Is(cancellationToken));
    }

    [Fact]
    public async Task ExecuteAsync_WhenRouteValuesAreMissing_ShouldSendNullRouteValuesToTheHandler()
    {
        // Arrange
        _mockHandler.HandleAsync(Arg.Any<GetTrackSegmentQuery>(), Arg.Any<CancellationToken>())
            .Returns(Result.From(_normalizedTrackSegmentDataDtoFixture.Create()));
        _sut.HttpContext.Request.RouteValues.Clear();

        // Act
        await _sut.ExecuteAsync(EmptyRequest.Instance, CancellationToken.None);

        // Assert
        await _mockHandler.Received(1).HandleAsync(
            Arg.Is<GetTrackSegmentQuery>(query =>
                query.LibraryId == null &&
                query.ArtistId == null &&
                query.AlbumId == null &&
                query.TrackId == null &&
                query.DeliveryId == null &&
                query.SegmentIndex == null),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_WhenCancellationRequested_ShouldCancelOperation()
    {
        // Arrange
        CancellationTokenSource cancellationTokenSource = new();
        TaskCompletionSource<bool> operationStarted = new();
        TaskCompletionSource<bool> cancellationRequested = new();

        _mockHandler.HandleAsync(Arg.Any<GetTrackSegmentQuery>(), Arg.Any<CancellationToken>())
            .Returns(callInfo => Task.Run(async () =>
            {
                operationStarted.SetResult(true);
                await cancellationRequested.Task;
                callInfo.Arg<CancellationToken>().ThrowIfCancellationRequested();
                return Result.From(_normalizedTrackSegmentDataDtoFixture.Create());
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
    /// Sets the six route values the endpoint reads from the route.
    /// </summary>
    private void SetRouteValues()
    {
        _sut.HttpContext.Request.RouteValues["libraryId"] = Guid.NewGuid().ToString();
        _sut.HttpContext.Request.RouteValues["artistId"] = Guid.NewGuid().ToString();
        _sut.HttpContext.Request.RouteValues["albumId"] = Guid.NewGuid().ToString();
        _sut.HttpContext.Request.RouteValues["trackId"] = Guid.NewGuid().ToString();
        _sut.HttpContext.Request.RouteValues["deliveryId"] = Guid.NewGuid().ToString("N");
        _sut.HttpContext.Request.RouteValues["segmentIndex"] = "0";
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
