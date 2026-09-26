#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.CQRS;
using Lumina.Application.Core.MediaLibrary.AudioLibrary.MusicLibrary.Tracks.Queries.GetTrack;
using Lumina.Contracts.Fixtures.Core.Responses.MediaLibrary.AudioLibrary.MusicLibrary.Tracks;
using Lumina.Contracts.Responses.MediaLibrary.AudioLibrary.MusicLibrary.Tracks;
using Lumina.Domain.Common.Errors;
using Lumina.Domain.Common.Primitives;
using Lumina.Presentation.Api.Core.Endpoints.Library.AudioLibrary.MusicLibrary.Tracks.GetTrack;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using NSubstitute;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Api.UnitTests.Core.Endpoints.Library.AudioLibrary.MusicLibrary.Tracks.GetTrack;

/// <summary>
/// Contains unit tests for the <see cref="GetTrackEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetTrackEndpointTests
{
    private readonly IQueryHandler<GetTrackQuery, Result<TrackResponse>> _mockHandler;
    private readonly GetTrackEndpoint _sut;
    private readonly TrackResponseFixture _trackResponseFixture = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="GetTrackEndpointTests"/> class.
    /// </summary>
    public GetTrackEndpointTests()
    {
        _mockHandler = Substitute.For<IQueryHandler<GetTrackQuery, Result<TrackResponse>>>();
        _sut = FastEndpoints.Factory.Create<GetTrackEndpoint>(_mockHandler);
    }

    [Fact]
    public async Task ExecuteAsync_WhenSuccessful_ShouldReturnOkResultWithTrackResponse()
    {
        // Arrange
        Guid libraryId = Guid.NewGuid();
        Guid artistId = Guid.NewGuid();
        Guid albumId = Guid.NewGuid();
        Guid trackId = Guid.NewGuid();
        CancellationToken cancellationToken = CancellationToken.None;
        TrackResponse expectedResponse = _trackResponseFixture.Create();
        _mockHandler.HandleAsync(Arg.Any<GetTrackQuery>(), Arg.Any<CancellationToken>())
            .Returns(Result.From(expectedResponse));
        _sut.HttpContext.Request.RouteValues["libraryId"] = libraryId.ToString();
        _sut.HttpContext.Request.RouteValues["artistId"] = artistId.ToString();
        _sut.HttpContext.Request.RouteValues["albumId"] = albumId.ToString();
        _sut.HttpContext.Request.RouteValues["trackId"] = trackId.ToString();

        // Act
        IResult result = await _sut.ExecuteAsync(FastEndpoints.EmptyRequest.Instance, cancellationToken);

        // Assert
        Ok<TrackResponse> okResult = Assert.IsType<Ok<TrackResponse>>(result);
        Assert.Equal(expectedResponse, okResult.Value);
    }

    [Fact]
    public async Task ExecuteAsync_WhenHandlerReturnsNotFoundError_ShouldReturnProblemResult()
    {
        // Arrange
        Guid libraryId = Guid.NewGuid();
        Guid artistId = Guid.NewGuid();
        Guid albumId = Guid.NewGuid();
        Guid trackId = Guid.NewGuid();
        CancellationToken cancellationToken = CancellationToken.None;
        Error expectedError = Error.NotFound("Track.NotFound", "TrackNotFound");
        _mockHandler.HandleAsync(Arg.Any<GetTrackQuery>(), Arg.Any<CancellationToken>())
            .Returns(expectedError);
        _sut.HttpContext.Request.RouteValues["libraryId"] = libraryId.ToString();
        _sut.HttpContext.Request.RouteValues["artistId"] = artistId.ToString();
        _sut.HttpContext.Request.RouteValues["albumId"] = albumId.ToString();
        _sut.HttpContext.Request.RouteValues["trackId"] = trackId.ToString();

        // Act
        IResult result = await _sut.ExecuteAsync(FastEndpoints.EmptyRequest.Instance, cancellationToken);

        // Assert
        ProblemHttpResult problemDetails = Assert.IsType<ProblemHttpResult>(result);
        Assert.Equal(StatusCodes.Status404NotFound, problemDetails.StatusCode);
        Assert.Equal("application/problem+json", problemDetails.ContentType);
        Microsoft.AspNetCore.Mvc.ProblemDetails problemDetailsBody = Assert.IsType<Microsoft.AspNetCore.Mvc.ProblemDetails>(problemDetails.ProblemDetails);
        Assert.Equal(StatusCodes.Status404NotFound, problemDetailsBody.Status);
        Assert.Equal("Track.NotFound", problemDetailsBody.Title);
        Assert.Equal("TrackNotFound", problemDetailsBody.Detail);
        Assert.NotNull(problemDetailsBody.Extensions["traceId"]);
    }

    [Fact]
    public async Task ExecuteAsync_WhenHandlerReturnsValidationErrors_ShouldReturnValidationProblemResult()
    {
        // Arrange
        Guid libraryId = Guid.NewGuid();
        Guid artistId = Guid.NewGuid();
        Guid albumId = Guid.NewGuid();
        Guid trackId = Guid.NewGuid();
        CancellationToken cancellationToken = CancellationToken.None;
        Error validationError = Errors.Music.TrackIdCannotBeEmpty;
        _mockHandler.HandleAsync(Arg.Any<GetTrackQuery>(), Arg.Any<CancellationToken>())
            .Returns(validationError);
        _sut.HttpContext.Request.RouteValues["libraryId"] = libraryId.ToString();
        _sut.HttpContext.Request.RouteValues["artistId"] = artistId.ToString();
        _sut.HttpContext.Request.RouteValues["albumId"] = albumId.ToString();
        _sut.HttpContext.Request.RouteValues["trackId"] = trackId.ToString();

        // Act
        IResult result = await _sut.ExecuteAsync(FastEndpoints.EmptyRequest.Instance, cancellationToken);

        // Assert
        ProblemHttpResult problemDetails = Assert.IsType<ProblemHttpResult>(result);
        Assert.Equal(StatusCodes.Status422UnprocessableEntity, problemDetails.StatusCode);
        Assert.Equal("application/problem+json", problemDetails.ContentType);
        HttpValidationProblemDetails validationProblemDetails = Assert.IsType<HttpValidationProblemDetails>(problemDetails.ProblemDetails);
        Assert.Equal(StatusCodes.Status422UnprocessableEntity, validationProblemDetails.Status);
        Assert.Equal("General.Validation", validationProblemDetails.Title);
        Assert.Equal("OneOrMoreValidationErrorsOccurred", validationProblemDetails.Detail);
        Assert.Equal("https://tools.ietf.org/html/rfc4918#section-11.2", validationProblemDetails.Type);
        Assert.Single(validationProblemDetails.Errors);
        Assert.Equal(new[] { "TrackIdCannotBeEmpty" }, validationProblemDetails.Errors["General.Validation"]);
    }

    [Fact]
    public async Task ExecuteAsync_WhenCalled_ShouldSendGetTrackQueryToSender()
    {
        // Arrange
        Guid libraryId = Guid.NewGuid();
        Guid artistId = Guid.NewGuid();
        Guid albumId = Guid.NewGuid();
        Guid trackId = Guid.NewGuid();
        CancellationToken cancellationToken = CancellationToken.None;
        _mockHandler.HandleAsync(Arg.Any<GetTrackQuery>(), Arg.Any<CancellationToken>())
            .Returns(Result.From(_trackResponseFixture.Create()));
        _sut.HttpContext.Request.RouteValues["libraryId"] = libraryId.ToString();
        _sut.HttpContext.Request.RouteValues["artistId"] = artistId.ToString();
        _sut.HttpContext.Request.RouteValues["albumId"] = albumId.ToString();
        _sut.HttpContext.Request.RouteValues["trackId"] = trackId.ToString();

        // Act
        await _sut.ExecuteAsync(FastEndpoints.EmptyRequest.Instance, cancellationToken);

        // Assert
        await _mockHandler.Received(1).HandleAsync(
            Arg.Is<GetTrackQuery>(query =>
                query.LibraryId == libraryId.ToString() &&
                query.ArtistId == artistId.ToString() &&
                query.AlbumId == albumId.ToString() &&
                query.TrackId == trackId.ToString()),
            Arg.Is(cancellationToken));
    }

    [Fact]
    public async Task ExecuteAsync_WhenRouteValuesAreNotParseable_ShouldSendQueryWithRawRouteValues()
    {
        // Arrange
        CancellationToken cancellationToken = CancellationToken.None;
        _mockHandler.HandleAsync(Arg.Any<GetTrackQuery>(), Arg.Any<CancellationToken>())
            .Returns(Result.From(_trackResponseFixture.Create()));
        _sut.HttpContext.Request.RouteValues["libraryId"] = "not-a-library-guid";
        _sut.HttpContext.Request.RouteValues["artistId"] = "not-an-artist-guid";
        _sut.HttpContext.Request.RouteValues["albumId"] = "not-an-album-guid";
        _sut.HttpContext.Request.RouteValues["trackId"] = "not-a-track-guid";

        // Act
        await _sut.ExecuteAsync(FastEndpoints.EmptyRequest.Instance, cancellationToken);

        // Assert
        await _mockHandler.Received(1).HandleAsync(
            Arg.Is<GetTrackQuery>(query =>
                query.LibraryId == "not-a-library-guid" &&
                query.ArtistId == "not-an-artist-guid" &&
                query.AlbumId == "not-an-album-guid" &&
                query.TrackId == "not-a-track-guid"),
            Arg.Is(cancellationToken));
    }

    [Fact]
    public async Task ExecuteAsync_WhenRouteValuesAreMissing_ShouldSendQueryWithNullIds()
    {
        // Arrange
        CancellationToken cancellationToken = CancellationToken.None;
        _mockHandler.HandleAsync(Arg.Any<GetTrackQuery>(), Arg.Any<CancellationToken>())
            .Returns(Result.From(_trackResponseFixture.Create()));
        _sut.HttpContext.Request.RouteValues.Remove("libraryId");
        _sut.HttpContext.Request.RouteValues.Remove("artistId");
        _sut.HttpContext.Request.RouteValues.Remove("albumId");
        _sut.HttpContext.Request.RouteValues.Remove("trackId");

        // Act
        await _sut.ExecuteAsync(FastEndpoints.EmptyRequest.Instance, cancellationToken);

        // Assert
        await _mockHandler.Received(1).HandleAsync(
            Arg.Is<GetTrackQuery>(query =>
                query.LibraryId == null &&
                query.ArtistId == null &&
                query.AlbumId == null &&
                query.TrackId == null),
            Arg.Is(cancellationToken));
    }

    [Fact]
    public async Task ExecuteAsync_WhenCancellationRequested_ShouldCancelOperation()
    {
        // Arrange
        Guid libraryId = Guid.NewGuid();
        Guid artistId = Guid.NewGuid();
        Guid albumId = Guid.NewGuid();
        Guid trackId = Guid.NewGuid();
        CancellationTokenSource cts = new();
        TaskCompletionSource<bool> operationStarted = new();
        TaskCompletionSource<bool> cancellationRequested = new();

        _mockHandler.HandleAsync(Arg.Any<GetTrackQuery>(), Arg.Any<CancellationToken>())
            .Returns(callInfo => Task.Run(async () =>
            {
                operationStarted.SetResult(true);
                await cancellationRequested.Task;
                callInfo.Arg<CancellationToken>().ThrowIfCancellationRequested();
                return Result.From(_trackResponseFixture.Create());
            }, callInfo.Arg<CancellationToken>()));
        _sut.HttpContext.Request.RouteValues["libraryId"] = libraryId.ToString();
        _sut.HttpContext.Request.RouteValues["artistId"] = artistId.ToString();
        _sut.HttpContext.Request.RouteValues["albumId"] = albumId.ToString();
        _sut.HttpContext.Request.RouteValues["trackId"] = trackId.ToString();

        // Act
        Task<IResult> operationTask = _sut.ExecuteAsync(FastEndpoints.EmptyRequest.Instance, cts.Token);
        await operationStarted.Task;
        cts.Cancel();
        cancellationRequested.SetResult(true);

        // Assert
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => operationTask);
    }
}
