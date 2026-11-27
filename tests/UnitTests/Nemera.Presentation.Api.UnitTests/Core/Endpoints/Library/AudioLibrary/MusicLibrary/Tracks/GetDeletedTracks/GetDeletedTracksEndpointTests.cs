#region ========================================================================= USING =====================================================================================
using Nemera.Application.Common.CQRS;
using Nemera.Application.Core.MediaLibrary.AudioLibrary.MusicLibrary.Tracks.Queries.GetDeletedTracks;
using Nemera.Application.Fixtures.Core.MediaLibrary.AudioLibrary.MusicLibrary.Tracks.Queries.GetDeletedTracks;
using Nemera.Contracts.Fixtures.Responses.MediaLibrary.AudioLibrary.MusicLibrary.Tracks;
using Nemera.Contracts.Responses.MediaLibrary.AudioLibrary.MusicLibrary.Tracks;
using Nemera.Domain.Common.Primitives;
using Nemera.Presentation.Api.Core.Endpoints.Library.AudioLibrary.MusicLibrary.Tracks.GetDeletedTracks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using ApplicationErrors = Nemera.Application.Common.Errors.Errors;
using DomainErrors = Nemera.Domain.Common.Errors.Errors;
#endregion

namespace Nemera.Presentation.Api.UnitTests.Core.Endpoints.Library.AudioLibrary.MusicLibrary.Tracks.GetDeletedTracks;

/// <summary>
/// Contains unit tests for the <see cref="GetDeletedTracksEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetDeletedTracksEndpointTests
{
    private readonly IQueryHandler<GetDeletedTracksQuery, Result<IReadOnlyList<DeletedTrackResponse>>> _mockHandler;
    private readonly GetDeletedTracksEndpoint _sut;
    private readonly DeletedTrackResponseFixture _deletedTrackResponseFixture = new();
    private readonly GetDeletedTracksQueryFixture _getDeletedTracksQueryFixture = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="GetDeletedTracksEndpointTests"/> class.
    /// </summary>
    public GetDeletedTracksEndpointTests()
    {
        _mockHandler = Substitute.For<IQueryHandler<GetDeletedTracksQuery, Result<IReadOnlyList<DeletedTrackResponse>>>>();
        _sut = FastEndpoints.Factory.Create<GetDeletedTracksEndpoint>(_mockHandler);
    }

    [Fact]
    public async Task ExecuteAsync_WhenSuccessful_ShouldReturnOkResultWithDeletedTracks()
    {
        // Arrange
        Guid libraryId = Guid.NewGuid();
        IReadOnlyList<DeletedTrackResponse> expectedTracks =
        [
            _deletedTrackResponseFixture.Create(
                libraryId: libraryId,
                artistName: "Queen",
                albumTitle: "A Night at the Opera",
                title: "Bohemian Rhapsody")
        ];
        CancellationToken cancellationToken = CancellationToken.None;
        _mockHandler.HandleAsync(Arg.Any<GetDeletedTracksQuery>(), Arg.Any<CancellationToken>())
            .Returns(Result.From(expectedTracks));
        _sut.HttpContext.Request.RouteValues["libraryId"] = libraryId.ToString();

        // Act
        IResult result = await _sut.ExecuteAsync(FastEndpoints.EmptyRequest.Instance, cancellationToken);

        // Assert
        Ok<IReadOnlyList<DeletedTrackResponse>> okResult = Assert.IsType<Ok<IReadOnlyList<DeletedTrackResponse>>>(result);
        Assert.Equal(expectedTracks, okResult.Value);
    }

    [Fact]
    public async Task ExecuteAsync_WhenHandlerReturnsNotAuthorizedError_ShouldReturnProblemResult()
    {
        // Arrange
        CancellationToken cancellationToken = CancellationToken.None;
        Error expectedError = ApplicationErrors.Authorization.NotAuthorized;
        _mockHandler.HandleAsync(Arg.Any<GetDeletedTracksQuery>(), Arg.Any<CancellationToken>())
            .Returns(expectedError);
        _sut.HttpContext.Request.RouteValues["libraryId"] = Guid.NewGuid().ToString();

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
        Error validationError = DomainErrors.Library.LibraryIdCannotBeEmpty;
        _mockHandler.HandleAsync(Arg.Any<GetDeletedTracksQuery>(), Arg.Any<CancellationToken>())
            .Returns(validationError);
        _sut.HttpContext.Request.RouteValues["libraryId"] = Guid.NewGuid().ToString();

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
        Assert.Equal(new[] { "LibraryIdCannotBeEmpty" }, validationProblemDetails.Errors["General.Validation"]);
    }

    [Fact]
    public async Task ExecuteAsync_WhenCalled_ShouldSendTheRouteLibraryIdToTheHandler()
    {
        // Arrange
        Guid libraryId = Guid.NewGuid();
        CancellationToken cancellationToken = CancellationToken.None;
        _mockHandler.HandleAsync(Arg.Any<GetDeletedTracksQuery>(), Arg.Any<CancellationToken>())
            .Returns(Result.From<IReadOnlyList<DeletedTrackResponse>>([]));
        _sut.HttpContext.Request.RouteValues["libraryId"] = libraryId.ToString();

        // Act
        await _sut.ExecuteAsync(FastEndpoints.EmptyRequest.Instance, cancellationToken);

        // Assert
        GetDeletedTracksQuery expectedQuery = _getDeletedTracksQueryFixture.Create(libraryId: libraryId.ToString());
        await _mockHandler.Received(1).HandleAsync(expectedQuery, Arg.Is(cancellationToken));
    }

    [Fact]
    public async Task ExecuteAsync_WhenRouteValueIsNotParseable_ShouldSendQueryWithRawRouteValue()
    {
        // Arrange
        CancellationToken cancellationToken = CancellationToken.None;
        _mockHandler.HandleAsync(Arg.Any<GetDeletedTracksQuery>(), Arg.Any<CancellationToken>())
            .Returns(Result.From<IReadOnlyList<DeletedTrackResponse>>([]));
        _sut.HttpContext.Request.RouteValues["libraryId"] = "not-a-library-guid";

        // Act
        await _sut.ExecuteAsync(FastEndpoints.EmptyRequest.Instance, cancellationToken);

        // Assert
        GetDeletedTracksQuery expectedQuery = _getDeletedTracksQueryFixture.Create(libraryId: "not-a-library-guid");
        await _mockHandler.Received(1).HandleAsync(expectedQuery, Arg.Is(cancellationToken));
    }

    [Fact]
    public async Task ExecuteAsync_WhenRouteValueIsMissing_ShouldSendQueryWithNullLibraryId()
    {
        // Arrange
        CancellationToken cancellationToken = CancellationToken.None;
        _mockHandler.HandleAsync(Arg.Any<GetDeletedTracksQuery>(), Arg.Any<CancellationToken>())
            .Returns(Result.From<IReadOnlyList<DeletedTrackResponse>>([]));
        _sut.HttpContext.Request.RouteValues.Remove("libraryId");

        // Act
        await _sut.ExecuteAsync(FastEndpoints.EmptyRequest.Instance, cancellationToken);

        // Assert
        GetDeletedTracksQuery expectedQuery = _getDeletedTracksQueryFixture.Create(includeLibraryId: false);
        await _mockHandler.Received(1).HandleAsync(expectedQuery, Arg.Is(cancellationToken));
    }

    [Fact]
    public async Task ExecuteAsync_WhenCancellationRequested_ShouldCancelOperation()
    {
        // Arrange
        CancellationTokenSource cts = new();
        TaskCompletionSource<bool> operationStarted = new();
        TaskCompletionSource<bool> cancellationRequested = new();
        _mockHandler.HandleAsync(Arg.Any<GetDeletedTracksQuery>(), Arg.Any<CancellationToken>())
            .Returns(callInfo => Task.Run(async () =>
            {
                operationStarted.SetResult(true);
                await cancellationRequested.Task;
                callInfo.Arg<CancellationToken>().ThrowIfCancellationRequested();
                return Result.From<IReadOnlyList<DeletedTrackResponse>>([]);
            }, callInfo.Arg<CancellationToken>()));
        _sut.HttpContext.Request.RouteValues["libraryId"] = Guid.NewGuid().ToString();

        // Act
        Task<IResult> operationTask = _sut.ExecuteAsync(FastEndpoints.EmptyRequest.Instance, cts.Token);
        await operationStarted.Task;
        cts.Cancel();
        cancellationRequested.SetResult(true);

        // Assert
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => operationTask);
    }
}
