#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.CQRS;
using Lumina.Application.Core.Playlists.Queries.GetPlaylists;
using Lumina.Contracts.Fixtures.Core.Responses.Playlists;
using Lumina.Contracts.Responses.Playlists;
using Lumina.Domain.Common.Primitives;
using Lumina.Presentation.Api.Core.Endpoints.Playlists.GetPlaylists;
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

namespace Lumina.Presentation.Api.UnitTests.Core.Endpoints.Playlists.GetPlaylists;

/// <summary>
/// Contains unit tests for the <see cref="GetPlaylistsEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetPlaylistsEndpointTests
{
    private readonly IQueryHandler<GetPlaylistsQuery, Result<PlaylistSummaryResponse[]>> _mockHandler;
    private readonly GetPlaylistsEndpoint _sut;
    private readonly PlaylistSummaryResponseFixture _playlistSummaryResponseFixture = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="GetPlaylistsEndpointTests"/> class.
    /// </summary>
    public GetPlaylistsEndpointTests()
    {
        _mockHandler = Substitute.For<IQueryHandler<GetPlaylistsQuery, Result<PlaylistSummaryResponse[]>>>();
        _sut = FastEndpoints.Factory.Create<GetPlaylistsEndpoint>(_mockHandler);
    }

    [Fact]
    public async Task ExecuteAsync_WhenSuccessful_ShouldReturnOkResultWithPlaylistSummaries()
    {
        // Arrange
        CancellationToken cancellationToken = CancellationToken.None;
        PlaylistSummaryResponse[] expectedResponse = [.. _playlistSummaryResponseFixture.CreateMany(3)];
        _mockHandler.HandleAsync(Arg.Any<GetPlaylistsQuery>(), Arg.Any<CancellationToken>())
            .Returns(Result.From(expectedResponse));

        // Act
        IResult result = await _sut.ExecuteAsync(FastEndpoints.EmptyRequest.Instance, cancellationToken);

        // Assert
        Ok<PlaylistSummaryResponse[]> okResult = Assert.IsType<Ok<PlaylistSummaryResponse[]>>(result);
        Assert.Equal(expectedResponse, okResult.Value);
    }

    [Fact]
    public async Task ExecuteAsync_WhenTheUserHasNoPlaylists_ShouldReturnOkResultWithAnEmptyArray()
    {
        // Arrange
        CancellationToken cancellationToken = CancellationToken.None;
        PlaylistSummaryResponse[] expectedResponse = [];
        _mockHandler.HandleAsync(Arg.Any<GetPlaylistsQuery>(), Arg.Any<CancellationToken>())
            .Returns(Result.From(expectedResponse));

        // Act
        IResult result = await _sut.ExecuteAsync(FastEndpoints.EmptyRequest.Instance, cancellationToken);

        // Assert
        Ok<PlaylistSummaryResponse[]> okResult = Assert.IsType<Ok<PlaylistSummaryResponse[]>>(result);
        Assert.Empty(okResult.Value!);
    }

    [Fact]
    public async Task ExecuteAsync_WhenSuccessful_ShouldSetPrivateNoStoreCacheHeaders()
    {
        // Arrange
        PlaylistSummaryResponse[] expectedResponse = [.. _playlistSummaryResponseFixture.CreateMany(2)];
        _mockHandler.HandleAsync(Arg.Any<GetPlaylistsQuery>(), Arg.Any<CancellationToken>())
            .Returns(Result.From(expectedResponse));

        // Act
        await _sut.ExecuteAsync(FastEndpoints.EmptyRequest.Instance, CancellationToken.None);

        // Assert
        Assert.Equal("private, no-store", _sut.HttpContext.Response.Headers["Cache-Control"].ToString());
        Assert.Equal("Authorization", _sut.HttpContext.Response.Headers["Vary"].ToString());
    }

    [Fact]
    public async Task ExecuteAsync_WhenHandlerReturnsNotAuthorizedError_ShouldReturnForbiddenProblemResultAndStillSetCacheHeaders()
    {
        // Arrange
        CancellationToken cancellationToken = CancellationToken.None;
        _mockHandler.HandleAsync(Arg.Any<GetPlaylistsQuery>(), Arg.Any<CancellationToken>())
            .Returns(ApplicationErrors.Authorization.NotAuthorized);

        // Act
        IResult result = await _sut.ExecuteAsync(FastEndpoints.EmptyRequest.Instance, cancellationToken);

        // Assert
        ProblemHttpResult problemResult = Assert.IsType<ProblemHttpResult>(result);
        Assert.Equal(StatusCodes.Status403Forbidden, problemResult.StatusCode);
        Assert.Equal("application/problem+json", problemResult.ContentType);
        ProblemDetails problemDetails = Assert.IsType<ProblemDetails>(problemResult.ProblemDetails);
        Assert.Equal(StatusCodes.Status403Forbidden, problemDetails.Status);
        Assert.Equal("General.Unauthorized", problemDetails.Title);
        Assert.Equal("NotAuthorized", problemDetails.Detail);
        Assert.Equal("https://tools.ietf.org/html/rfc9110#section-15.5.4", problemDetails.Type);
        Assert.NotNull(problemDetails.Extensions["traceId"]);
        Assert.Equal("private, no-store", _sut.HttpContext.Response.Headers["Cache-Control"].ToString());
        Assert.Equal("Authorization", _sut.HttpContext.Response.Headers["Vary"].ToString());
    }

    [Fact]
    public async Task ExecuteAsync_WhenHandlerReturnsInternalError_ShouldReturnInternalServerErrorProblemResult()
    {
        // Arrange
        CancellationToken cancellationToken = CancellationToken.None;
        _mockHandler.HandleAsync(Arg.Any<GetPlaylistsQuery>(), Arg.Any<CancellationToken>())
            .Returns(Error.Internal("Playlists.Failed", "An internal error has occurred."));

        // Act
        IResult result = await _sut.ExecuteAsync(FastEndpoints.EmptyRequest.Instance, cancellationToken);

        // Assert
        ProblemHttpResult problemResult = Assert.IsType<ProblemHttpResult>(result);
        Assert.Equal(StatusCodes.Status500InternalServerError, problemResult.StatusCode);
        Assert.Equal("application/problem+json", problemResult.ContentType);
        ProblemDetails problemDetails = Assert.IsType<ProblemDetails>(problemResult.ProblemDetails);
        Assert.Equal(StatusCodes.Status500InternalServerError, problemDetails.Status);
        Assert.Equal("Playlists.Failed", problemDetails.Title);
        Assert.Equal("An internal error has occurred.", problemDetails.Detail);
        Assert.Equal("https://tools.ietf.org/html/rfc9110#section-15.6.1", problemDetails.Type);
        Assert.NotNull(problemDetails.Extensions["traceId"]);
    }

    [Fact]
    public async Task ExecuteAsync_WhenCalled_ShouldSendOneGetPlaylistsQueryWithTheCallerToken()
    {
        // Arrange
        PlaylistSummaryResponse[] expectedResponse = [.. _playlistSummaryResponseFixture.CreateMany(1)];
        CancellationToken cancellationToken = CancellationToken.None;
        _mockHandler.HandleAsync(Arg.Any<GetPlaylistsQuery>(), Arg.Any<CancellationToken>())
            .Returns(Result.From(expectedResponse));

        // Act
        await _sut.ExecuteAsync(FastEndpoints.EmptyRequest.Instance, cancellationToken);

        // Assert
        await _mockHandler.Received(1).HandleAsync(
            Arg.Any<GetPlaylistsQuery>(),
            Arg.Is(cancellationToken));
    }

    [Fact]
    public async Task ExecuteAsync_WhenCancellationRequested_ShouldCancelOperation()
    {
        // Arrange
        PlaylistSummaryResponse[] expectedResponse = [.. _playlistSummaryResponseFixture.CreateMany(1)];
        CancellationTokenSource cts = new();
        TaskCompletionSource<bool> operationStarted = new();
        TaskCompletionSource<bool> cancellationRequested = new();
        _mockHandler.HandleAsync(Arg.Any<GetPlaylistsQuery>(), Arg.Any<CancellationToken>())
            .Returns(callInfo => Task.Run(async () =>
            {
                operationStarted.SetResult(true);
                await cancellationRequested.Task;
                callInfo.Arg<CancellationToken>().ThrowIfCancellationRequested();
                return Result.From(expectedResponse);
            }, callInfo.Arg<CancellationToken>()));

        // Act
        Task<IResult> operationTask = _sut.ExecuteAsync(FastEndpoints.EmptyRequest.Instance, cts.Token);
        await operationStarted.Task;
        cts.Cancel();
        cancellationRequested.SetResult(true);

        // Assert
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => operationTask);
    }
}
