#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.CQRS;
using Lumina.Application.Core.MediaLibrary.AudioLibrary.MusicLibrary.Tracks.Queries.ShuffleTracks;
using Lumina.Contracts.Fixtures.Core.Requests.MediaLibrary.AudioLibrary.MusicLibrary.Tracks;
using Lumina.Contracts.Fixtures.Core.Responses.MediaLibrary.AudioLibrary.MusicLibrary.Tracks;
using Lumina.Contracts.Requests.MediaLibrary.AudioLibrary.MusicLibrary.Tracks;
using Lumina.Contracts.Responses.MediaLibrary.AudioLibrary.MusicLibrary.Tracks;
using Lumina.Domain.Common.Errors;
using Lumina.Domain.Common.Primitives;
using Lumina.Presentation.Api.Core.Endpoints.Library.AudioLibrary.MusicLibrary.Tracks.ShuffleTracks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using ApplicationErrors = Lumina.Application.Common.Errors.Errors;
#endregion

namespace Lumina.Presentation.Api.UnitTests.Core.Endpoints.Library.AudioLibrary.MusicLibrary.Tracks.ShuffleTracks;

/// <summary>
/// Contains unit tests for the <see cref="ShuffleTracksEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class ShuffleTracksEndpointTests
{
    private readonly IQueryHandler<ShuffleTracksQuery, Result<IReadOnlyList<ShuffleTrackResponse>>> _mockHandler;
    private readonly ShuffleTracksEndpoint _sut;
    private readonly ShuffleTracksRequestFixture _shuffleTracksRequestFixture = new();
    private readonly ShuffleTrackResponseFixture _shuffleTrackResponseFixture = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="ShuffleTracksEndpointTests"/> class.
    /// </summary>
    public ShuffleTracksEndpointTests()
    {
        _mockHandler = Substitute.For<IQueryHandler<ShuffleTracksQuery, Result<IReadOnlyList<ShuffleTrackResponse>>>>();
        _sut = FastEndpoints.Factory.Create<ShuffleTracksEndpoint>(_mockHandler);
    }

    [Fact]
    public async Task ExecuteAsync_WhenSuccessful_ShouldReturnOkResultWithTrackResponses()
    {
        // Arrange
        Guid libraryId = Guid.NewGuid();
        CancellationToken cancellationToken = CancellationToken.None;
        List<ShuffleTrackResponse> expectedResponse = _shuffleTrackResponseFixture.CreateMany(3);
        _mockHandler.HandleAsync(Arg.Any<ShuffleTracksQuery>(), Arg.Any<CancellationToken>())
            .Returns(Result.From<IReadOnlyList<ShuffleTrackResponse>>(expectedResponse));
        _sut.HttpContext.Request.RouteValues["libraryId"] = libraryId.ToString();

        // Act
        IResult result = await _sut.ExecuteAsync(_shuffleTracksRequestFixture.Create(), cancellationToken);

        // Assert
        Ok<IReadOnlyList<ShuffleTrackResponse>> okResult = Assert.IsType<Ok<IReadOnlyList<ShuffleTrackResponse>>>(result);
        Assert.Equal(expectedResponse, okResult.Value);
    }

    [Fact]
    public async Task ExecuteAsync_WhenHandlerReturnsValidationErrors_ShouldReturnValidationProblemResult()
    {
        // Arrange
        Guid libraryId = Guid.NewGuid();
        CancellationToken cancellationToken = CancellationToken.None;
        Error validationError = Errors.Music.ShuffleTrackCountMustBeAtMost500;
        _mockHandler.HandleAsync(Arg.Any<ShuffleTracksQuery>(), Arg.Any<CancellationToken>())
            .Returns(validationError);
        _sut.HttpContext.Request.RouteValues["libraryId"] = libraryId.ToString();

        // Act
        IResult result = await _sut.ExecuteAsync(_shuffleTracksRequestFixture.Create(), cancellationToken);

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
        Assert.Equal(new[] { "ShuffleTrackCountMustBeAtMost500" }, validationProblemDetails.Errors["General.Validation"]);
    }

    [Fact]
    public async Task ExecuteAsync_WhenHandlerReturnsForbiddenError_ShouldReturnForbiddenProblemResult()
    {
        // Arrange
        Guid libraryId = Guid.NewGuid();
        CancellationToken cancellationToken = CancellationToken.None;
        _mockHandler.HandleAsync(Arg.Any<ShuffleTracksQuery>(), Arg.Any<CancellationToken>())
            .Returns(ApplicationErrors.Authorization.NotAuthorized);
        _sut.HttpContext.Request.RouteValues["libraryId"] = libraryId.ToString();

        // Act
        IResult result = await _sut.ExecuteAsync(_shuffleTracksRequestFixture.Create(), cancellationToken);

        // Assert
        ProblemHttpResult problemDetails = Assert.IsType<ProblemHttpResult>(result);
        Assert.Equal(StatusCodes.Status403Forbidden, problemDetails.StatusCode);
        Assert.Equal("application/problem+json", problemDetails.ContentType);
        ProblemDetails problemDetailsBody = Assert.IsType<ProblemDetails>(problemDetails.ProblemDetails);
        Assert.Equal(StatusCodes.Status403Forbidden, problemDetailsBody.Status);
        Assert.Equal("General.Unauthorized", problemDetailsBody.Title);
        Assert.Equal("NotAuthorized", problemDetailsBody.Detail);
        Assert.NotNull(problemDetailsBody.Extensions["traceId"]);
    }

    [Fact]
    public async Task ExecuteAsync_WhenHandlerReturnsNotFoundError_ShouldReturnNotFoundProblemResult()
    {
        // Arrange
        Guid libraryId = Guid.NewGuid();
        CancellationToken cancellationToken = CancellationToken.None;
        _mockHandler.HandleAsync(Arg.Any<ShuffleTracksQuery>(), Arg.Any<CancellationToken>())
            .Returns(Error.NotFound("Library.NotFound", "LibraryNotFound"));
        _sut.HttpContext.Request.RouteValues["libraryId"] = libraryId.ToString();

        // Act
        IResult result = await _sut.ExecuteAsync(_shuffleTracksRequestFixture.Create(), cancellationToken);

        // Assert
        ProblemHttpResult problemDetails = Assert.IsType<ProblemHttpResult>(result);
        Assert.Equal(StatusCodes.Status404NotFound, problemDetails.StatusCode);
        Assert.Equal("application/problem+json", problemDetails.ContentType);
        ProblemDetails problemDetailsBody = Assert.IsType<ProblemDetails>(problemDetails.ProblemDetails);
        Assert.Equal(StatusCodes.Status404NotFound, problemDetailsBody.Status);
        Assert.Equal("Library.NotFound", problemDetailsBody.Title);
        Assert.Equal("LibraryNotFound", problemDetailsBody.Detail);
    }

    [Fact]
    public async Task ExecuteAsync_WhenCalled_ShouldSendQueryWithTheRouteAndRequestValues()
    {
        // Arrange
        Guid libraryId = Guid.NewGuid();
        Guid artistId = Guid.NewGuid();
        Guid albumId = Guid.NewGuid();
        Guid excludedTrackId = Guid.NewGuid();
        CancellationToken cancellationToken = CancellationToken.None;
        ShuffleTracksRequest request = _shuffleTracksRequestFixture.Create(
            count: 25,
            artistId: artistId,
            albumId: albumId,
            excludedTrackIds: [excludedTrackId]);
        _mockHandler.HandleAsync(Arg.Any<ShuffleTracksQuery>(), Arg.Any<CancellationToken>())
            .Returns(Result.From<IReadOnlyList<ShuffleTrackResponse>>([]));
        _sut.HttpContext.Request.RouteValues["libraryId"] = libraryId.ToString();

        // Act
        await _sut.ExecuteAsync(request, cancellationToken);

        // Assert
        await _mockHandler.Received(1).HandleAsync(
            Arg.Is<ShuffleTracksQuery>(query =>
                query.LibraryId == libraryId.ToString() &&
                query.Count == 25 &&
                query.ArtistId == artistId &&
                query.AlbumId == albumId &&
                query.ExcludedTrackIds != null &&
                query.ExcludedTrackIds.Count == 1 &&
                query.ExcludedTrackIds[0] == excludedTrackId),
            Arg.Is(cancellationToken));
    }

    [Fact]
    public async Task ExecuteAsync_WhenRouteValueIsMissing_ShouldSendQueryWithNullLibraryId()
    {
        // Arrange
        CancellationToken cancellationToken = CancellationToken.None;
        _mockHandler.HandleAsync(Arg.Any<ShuffleTracksQuery>(), Arg.Any<CancellationToken>())
            .Returns(Result.From<IReadOnlyList<ShuffleTrackResponse>>([]));
        _sut.HttpContext.Request.RouteValues.Remove("libraryId");

        // Act
        await _sut.ExecuteAsync(_shuffleTracksRequestFixture.Create(), cancellationToken);

        // Assert
        await _mockHandler.Received(1).HandleAsync(
            Arg.Is<ShuffleTracksQuery>(query => query.LibraryId == null),
            Arg.Is(cancellationToken));
    }

    [Fact]
    public async Task ExecuteAsync_WhenRouteValueIsNotParseable_ShouldSendQueryWithTheRawRouteValue()
    {
        // Arrange
        CancellationToken cancellationToken = CancellationToken.None;
        _mockHandler.HandleAsync(Arg.Any<ShuffleTracksQuery>(), Arg.Any<CancellationToken>())
            .Returns(Result.From<IReadOnlyList<ShuffleTrackResponse>>([]));
        _sut.HttpContext.Request.RouteValues["libraryId"] = "'; DROP TABLE Tracks; --";

        // Act
        await _sut.ExecuteAsync(_shuffleTracksRequestFixture.Create(), cancellationToken);

        // Assert
        await _mockHandler.Received(1).HandleAsync(
            Arg.Is<ShuffleTracksQuery>(query => query.LibraryId == "'; DROP TABLE Tracks; --"),
            Arg.Is(cancellationToken));
    }

    [Fact]
    public async Task ExecuteAsync_WhenCancellationRequested_ShouldCancelOperation()
    {
        // Arrange
        Guid libraryId = Guid.NewGuid();
        CancellationTokenSource cts = new();
        TaskCompletionSource<bool> operationStarted = new();
        TaskCompletionSource<bool> cancellationRequested = new();

        _mockHandler.HandleAsync(Arg.Any<ShuffleTracksQuery>(), Arg.Any<CancellationToken>())
            .Returns(callInfo => Task.Run(async () =>
            {
                operationStarted.SetResult(true);
                await cancellationRequested.Task;
                callInfo.Arg<CancellationToken>().ThrowIfCancellationRequested();
                return Result.From<IReadOnlyList<ShuffleTrackResponse>>(_shuffleTrackResponseFixture.CreateMany(2));
            }, callInfo.Arg<CancellationToken>()));
        _sut.HttpContext.Request.RouteValues["libraryId"] = libraryId.ToString();

        // Act
        Task<IResult> operationTask = _sut.ExecuteAsync(_shuffleTracksRequestFixture.Create(), cts.Token);
        await operationStarted.Task;
        cts.Cancel();
        cancellationRequested.SetResult(true);

        // Assert
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => operationTask);
    }
}
