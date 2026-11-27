#region ========================================================================= USING =====================================================================================
using Nemera.Application.Common.CQRS;
using Nemera.Application.Core.MediaLibrary.AudioLibrary.MusicLibrary.Tracks.Commands.RestoreTrack;
using Nemera.Application.Fixtures.Core.MediaLibrary.AudioLibrary.MusicLibrary.Tracks.Commands.RestoreTrack;
using Nemera.Domain.Common.Primitives;
using Nemera.Presentation.Api.Core.Endpoints.Library.AudioLibrary.MusicLibrary.Tracks.RestoreTrack;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using ApplicationErrors = Nemera.Application.Common.Errors.Errors;
using DomainErrors = Nemera.Domain.Common.Errors.Errors;
#endregion

namespace Nemera.Presentation.Api.UnitTests.Core.Endpoints.Library.AudioLibrary.MusicLibrary.Tracks.RestoreTrack;

/// <summary>
/// Contains unit tests for the <see cref="RestoreTrackEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class RestoreTrackEndpointTests
{
    private readonly ICommandHandler<RestoreTrackCommand, Result<Updated>> _mockHandler;
    private readonly RestoreTrackEndpoint _sut;
    private readonly RestoreTrackCommandFixture _restoreTrackCommandFixture = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="RestoreTrackEndpointTests"/> class.
    /// </summary>
    public RestoreTrackEndpointTests()
    {
        _mockHandler = Substitute.For<ICommandHandler<RestoreTrackCommand, Result<Updated>>>();
        _sut = FastEndpoints.Factory.Create<RestoreTrackEndpoint>(_mockHandler);
    }

    [Fact]
    public async Task ExecuteAsync_WhenSuccessful_ShouldReturnOkResultWithUpdated()
    {
        // Arrange
        CancellationToken cancellationToken = CancellationToken.None;
        _mockHandler.HandleAsync(Arg.Any<RestoreTrackCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.From(Result.Updated));
        ConfigureRouteValues();

        // Act
        IResult result = await _sut.ExecuteAsync(FastEndpoints.EmptyRequest.Instance, cancellationToken);

        // Assert
        Ok<Updated> okResult = Assert.IsType<Ok<Updated>>(result);
        Assert.Equal(Result.Updated, okResult.Value);
    }

    [Fact]
    public async Task ExecuteAsync_WhenHandlerReturnsNotFoundError_ShouldReturnProblemResult()
    {
        // Arrange
        CancellationToken cancellationToken = CancellationToken.None;
        Error expectedError = DomainErrors.Music.TrackNotFound;
        _mockHandler.HandleAsync(Arg.Any<RestoreTrackCommand>(), Arg.Any<CancellationToken>())
            .Returns(expectedError);
        ConfigureRouteValues();

        // Act
        IResult result = await _sut.ExecuteAsync(FastEndpoints.EmptyRequest.Instance, cancellationToken);

        // Assert
        ProblemHttpResult problemDetails = Assert.IsType<ProblemHttpResult>(result);
        Assert.Equal(StatusCodes.Status404NotFound, problemDetails.StatusCode);
        Assert.Equal("application/problem+json", problemDetails.ContentType);
        ProblemDetails problemDetailsBody = Assert.IsType<ProblemDetails>(problemDetails.ProblemDetails);
        Assert.Equal(StatusCodes.Status404NotFound, problemDetailsBody.Status);
        Assert.Equal("General.NotFound", problemDetailsBody.Title);
        Assert.Equal("TrackNotFound", problemDetailsBody.Detail);
        Assert.Equal("https://tools.ietf.org/html/rfc9110#section-15.5.5", problemDetailsBody.Type);
        Assert.NotNull(problemDetailsBody.Extensions["traceId"]);
    }

    [Fact]
    public async Task ExecuteAsync_WhenHandlerReturnsNotAuthorizedError_ShouldReturnProblemResult()
    {
        // Arrange
        CancellationToken cancellationToken = CancellationToken.None;
        Error expectedError = ApplicationErrors.Authorization.NotAuthorized;
        _mockHandler.HandleAsync(Arg.Any<RestoreTrackCommand>(), Arg.Any<CancellationToken>())
            .Returns(expectedError);
        ConfigureRouteValues();

        // Act
        IResult result = await _sut.ExecuteAsync(FastEndpoints.EmptyRequest.Instance, cancellationToken);

        // Assert
        ProblemHttpResult problemDetails = Assert.IsType<ProblemHttpResult>(result);
        Assert.Equal(StatusCodes.Status403Forbidden, problemDetails.StatusCode);
        Assert.Equal("application/problem+json", problemDetails.ContentType);
        ProblemDetails problemDetailsBody = Assert.IsType<ProblemDetails>(problemDetails.ProblemDetails);
        Assert.Equal(StatusCodes.Status403Forbidden, problemDetailsBody.Status);
        Assert.Equal("General.Unauthorized", problemDetailsBody.Title);
        Assert.Equal("NotAuthorized", problemDetailsBody.Detail);
        Assert.Equal("https://tools.ietf.org/html/rfc9110#section-15.5.4", problemDetailsBody.Type);
        Assert.NotNull(problemDetailsBody.Extensions["traceId"]);
    }

    [Fact]
    public async Task ExecuteAsync_WhenHandlerReturnsValidationErrors_ShouldReturnValidationProblemResult()
    {
        // Arrange
        CancellationToken cancellationToken = CancellationToken.None;
        Error validationError = DomainErrors.Music.TrackIdCannotBeEmpty;
        _mockHandler.HandleAsync(Arg.Any<RestoreTrackCommand>(), Arg.Any<CancellationToken>())
            .Returns(validationError);
        ConfigureRouteValues();

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
    public async Task ExecuteAsync_WhenCalled_ShouldSendTheRouteIdsToTheHandler()
    {
        // Arrange
        Guid libraryId = Guid.NewGuid();
        Guid artistId = Guid.NewGuid();
        Guid albumId = Guid.NewGuid();
        Guid trackId = Guid.NewGuid();
        CancellationToken cancellationToken = CancellationToken.None;
        _mockHandler.HandleAsync(Arg.Any<RestoreTrackCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.From(Result.Updated));
        _sut.HttpContext.Request.RouteValues["libraryId"] = libraryId.ToString();
        _sut.HttpContext.Request.RouteValues["artistId"] = artistId.ToString();
        _sut.HttpContext.Request.RouteValues["albumId"] = albumId.ToString();
        _sut.HttpContext.Request.RouteValues["trackId"] = trackId.ToString();

        // Act
        await _sut.ExecuteAsync(FastEndpoints.EmptyRequest.Instance, cancellationToken);

        // Assert
        RestoreTrackCommand expectedCommand = _restoreTrackCommandFixture.Create(
            libraryId: libraryId.ToString(),
            artistId: artistId.ToString(),
            albumId: albumId.ToString(),
            trackId: trackId.ToString());
        await _mockHandler.Received(1).HandleAsync(expectedCommand, Arg.Is(cancellationToken));
    }

    [Fact]
    public async Task ExecuteAsync_WhenRouteValuesAreNotParseable_ShouldSendCommandWithRawRouteValues()
    {
        // Arrange
        CancellationToken cancellationToken = CancellationToken.None;
        _mockHandler.HandleAsync(Arg.Any<RestoreTrackCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.From(Result.Updated));
        _sut.HttpContext.Request.RouteValues["libraryId"] = "not-a-library-guid";
        _sut.HttpContext.Request.RouteValues["artistId"] = "not-an-artist-guid";
        _sut.HttpContext.Request.RouteValues["albumId"] = "not-an-album-guid";
        _sut.HttpContext.Request.RouteValues["trackId"] = "not-a-track-guid";

        // Act
        await _sut.ExecuteAsync(FastEndpoints.EmptyRequest.Instance, cancellationToken);

        // Assert
        RestoreTrackCommand expectedCommand = _restoreTrackCommandFixture.Create(
            libraryId: "not-a-library-guid",
            artistId: "not-an-artist-guid",
            albumId: "not-an-album-guid",
            trackId: "not-a-track-guid");
        await _mockHandler.Received(1).HandleAsync(expectedCommand, Arg.Is(cancellationToken));
    }

    [Fact]
    public async Task ExecuteAsync_WhenRouteValuesAreMissing_ShouldSendCommandWithNullIds()
    {
        // Arrange
        CancellationToken cancellationToken = CancellationToken.None;
        _mockHandler.HandleAsync(Arg.Any<RestoreTrackCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.From(Result.Updated));
        _sut.HttpContext.Request.RouteValues.Remove("libraryId");
        _sut.HttpContext.Request.RouteValues.Remove("artistId");
        _sut.HttpContext.Request.RouteValues.Remove("albumId");
        _sut.HttpContext.Request.RouteValues.Remove("trackId");

        // Act
        await _sut.ExecuteAsync(FastEndpoints.EmptyRequest.Instance, cancellationToken);

        // Assert
        RestoreTrackCommand expectedCommand = _restoreTrackCommandFixture.Create(
            includeLibraryId: false,
            includeArtistId: false,
            includeAlbumId: false,
            includeTrackId: false);
        await _mockHandler.Received(1).HandleAsync(expectedCommand, Arg.Is(cancellationToken));
    }

    [Fact]
    public async Task ExecuteAsync_WhenCancellationRequested_ShouldCancelOperation()
    {
        // Arrange
        CancellationTokenSource cts = new();
        TaskCompletionSource<bool> operationStarted = new();
        TaskCompletionSource<bool> cancellationRequested = new();
        _mockHandler.HandleAsync(Arg.Any<RestoreTrackCommand>(), Arg.Any<CancellationToken>())
            .Returns(callInfo => Task.Run(async () =>
            {
                operationStarted.SetResult(true);
                await cancellationRequested.Task;
                callInfo.Arg<CancellationToken>().ThrowIfCancellationRequested();
                return Result.From(Result.Updated);
            }, callInfo.Arg<CancellationToken>()));
        ConfigureRouteValues();

        // Act
        Task<IResult> operationTask = _sut.ExecuteAsync(FastEndpoints.EmptyRequest.Instance, cts.Token);
        await operationStarted.Task;
        cts.Cancel();
        cancellationRequested.SetResult(true);

        // Assert
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => operationTask);
    }

    /// <summary>
    /// Configures the route values of a valid restore request.
    /// </summary>
    private void ConfigureRouteValues()
    {
        _sut.HttpContext.Request.RouteValues["libraryId"] = Guid.NewGuid().ToString();
        _sut.HttpContext.Request.RouteValues["artistId"] = Guid.NewGuid().ToString();
        _sut.HttpContext.Request.RouteValues["albumId"] = Guid.NewGuid().ToString();
        _sut.HttpContext.Request.RouteValues["trackId"] = Guid.NewGuid().ToString();
    }
}
