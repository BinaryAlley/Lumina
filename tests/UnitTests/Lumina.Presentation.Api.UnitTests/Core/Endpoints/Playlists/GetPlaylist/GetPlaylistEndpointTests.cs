#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.CQRS;
using Lumina.Application.Core.Playlists.Queries.GetPlaylist;
using Lumina.Contracts.Fixtures.Core.Requests.Playlists;
using Lumina.Contracts.Fixtures.Core.Responses.Playlists;
using Lumina.Contracts.Requests.Playlists;
using Lumina.Contracts.Responses.Playlists;
using Lumina.Domain.Common.Primitives;
using Lumina.Presentation.Api.Core.Endpoints.Playlists.GetPlaylist;
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
using DomainErrors = Lumina.Domain.Common.Errors.Errors;
#endregion

namespace Lumina.Presentation.Api.UnitTests.Core.Endpoints.Playlists.GetPlaylist;

/// <summary>
/// Contains unit tests for the <see cref="GetPlaylistEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetPlaylistEndpointTests
{
    private const int DEFAULT_PER_PAGE = 200;

    private readonly IQueryHandler<GetPlaylistQuery, Result<PlaylistResponse>> _mockHandler;
    private readonly GetPlaylistEndpoint _sut;
    private readonly GetPlaylistRequestFixture _getPlaylistRequestFixture = new();
    private readonly PlaylistResponseFixture _playlistResponseFixture = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="GetPlaylistEndpointTests"/> class.
    /// </summary>
    public GetPlaylistEndpointTests()
    {
        _mockHandler = Substitute.For<IQueryHandler<GetPlaylistQuery, Result<PlaylistResponse>>>();
        _sut = FastEndpoints.Factory.Create<GetPlaylistEndpoint>(_mockHandler);
    }

    [Fact]
    public async Task ExecuteAsync_WhenSuccessful_ShouldReturnOkResultWithPlaylistResponse()
    {
        // Arrange
        Guid playlistId = Guid.NewGuid();
        GetPlaylistRequest request = _getPlaylistRequestFixture.Create();
        CancellationToken cancellationToken = CancellationToken.None;
        PlaylistResponse expectedResponse = _playlistResponseFixture.Create();
        _mockHandler.HandleAsync(Arg.Any<GetPlaylistQuery>(), Arg.Any<CancellationToken>())
            .Returns(Result.From(expectedResponse));
        _sut.HttpContext.Request.RouteValues["playlistId"] = playlistId.ToString();

        // Act
        IResult result = await _sut.ExecuteAsync(request, cancellationToken);

        // Assert
        Ok<PlaylistResponse> okResult = Assert.IsType<Ok<PlaylistResponse>>(result);
        Assert.Equal(expectedResponse, okResult.Value);
    }

    [Fact]
    public async Task ExecuteAsync_WhenSuccessful_ShouldSetPrivateNoStoreCacheHeaders()
    {
        // Arrange
        GetPlaylistRequest request = _getPlaylistRequestFixture.Create();
        CancellationToken cancellationToken = CancellationToken.None;
        _mockHandler.HandleAsync(Arg.Any<GetPlaylistQuery>(), Arg.Any<CancellationToken>())
            .Returns(Result.From(_playlistResponseFixture.Create()));
        _sut.HttpContext.Request.RouteValues["playlistId"] = Guid.NewGuid().ToString();

        // Act
        await _sut.ExecuteAsync(request, cancellationToken);

        // Assert
        Assert.Equal("private, no-store", _sut.HttpContext.Response.Headers["Cache-Control"].ToString());
        Assert.Equal("Authorization", _sut.HttpContext.Response.Headers["Vary"].ToString());
    }

    [Fact]
    public async Task ExecuteAsync_WhenHandlerReturnsValidationErrors_ShouldReturnValidationProblemResultWithErrorsGroupedByCode()
    {
        // Arrange
        GetPlaylistRequest request = _getPlaylistRequestFixture.Create();
        CancellationToken cancellationToken = CancellationToken.None;
        Error expectedError = DomainErrors.Playlists.PlaylistIdCannotBeEmpty;
        _mockHandler.HandleAsync(Arg.Any<GetPlaylistQuery>(), Arg.Any<CancellationToken>())
            .Returns(expectedError);
        _sut.HttpContext.Request.RouteValues["playlistId"] = "not-a-guid";

        // Act
        IResult result = await _sut.ExecuteAsync(request, cancellationToken);

        // Assert
        ProblemHttpResult problemResult = Assert.IsType<ProblemHttpResult>(result);
        Assert.Equal(StatusCodes.Status422UnprocessableEntity, problemResult.StatusCode);
        Assert.Equal("application/problem+json", problemResult.ContentType);
        HttpValidationProblemDetails validationProblemDetails = Assert.IsType<HttpValidationProblemDetails>(problemResult.ProblemDetails);
        Assert.Equal(StatusCodes.Status422UnprocessableEntity, validationProblemDetails.Status);
        Assert.Equal("General.Validation", validationProblemDetails.Title);
        Assert.Equal("OneOrMoreValidationErrorsOccurred", validationProblemDetails.Detail);
        Assert.Equal("https://tools.ietf.org/html/rfc4918#section-11.2", validationProblemDetails.Type);
        Assert.NotNull(validationProblemDetails.Extensions["traceId"]);
        KeyValuePair<string, string[]> singleErrorGroup = Assert.Single(validationProblemDetails.Errors);
        Assert.Equal("General.Validation", singleErrorGroup.Key);
        Assert.Equal(new[] { "PlaylistIdCannotBeEmpty" }, singleErrorGroup.Value);
    }

    [Fact]
    public async Task ExecuteAsync_WhenHandlerReturnsError_ShouldStillSetPrivateNoStoreCacheHeaders()
    {
        // Arrange
        GetPlaylistRequest request = _getPlaylistRequestFixture.Create();
        Error expectedError = DomainErrors.Playlists.PlaylistIdCannotBeEmpty;
        _mockHandler.HandleAsync(Arg.Any<GetPlaylistQuery>(), Arg.Any<CancellationToken>())
            .Returns(expectedError);
        _sut.HttpContext.Request.RouteValues["playlistId"] = "not-a-guid";

        // Act
        await _sut.ExecuteAsync(request, CancellationToken.None);

        // Assert
        Assert.Equal("private, no-store", _sut.HttpContext.Response.Headers["Cache-Control"].ToString());
        Assert.Equal("Authorization", _sut.HttpContext.Response.Headers["Vary"].ToString());
    }

    [Fact]
    public async Task ExecuteAsync_WhenHandlerReturnsPlaylistNotFoundError_ShouldReturnNotFoundProblemResult()
    {
        // Arrange
        GetPlaylistRequest request = _getPlaylistRequestFixture.Create();
        CancellationToken cancellationToken = CancellationToken.None;
        Error expectedError = DomainErrors.Playlists.PlaylistNotFound;
        _mockHandler.HandleAsync(Arg.Any<GetPlaylistQuery>(), Arg.Any<CancellationToken>())
            .Returns(expectedError);
        _sut.HttpContext.Request.RouteValues["playlistId"] = Guid.NewGuid().ToString();

        // Act
        IResult result = await _sut.ExecuteAsync(request, cancellationToken);

        // Assert
        ProblemHttpResult problemResult = Assert.IsType<ProblemHttpResult>(result);
        Assert.Equal(StatusCodes.Status404NotFound, problemResult.StatusCode);
        Assert.Equal("application/problem+json", problemResult.ContentType);
        ProblemDetails problemDetails = Assert.IsType<ProblemDetails>(problemResult.ProblemDetails);
        Assert.Equal(StatusCodes.Status404NotFound, problemDetails.Status);
        Assert.Equal("General.NotFound", problemDetails.Title);
        Assert.Equal("PlaylistNotFound", problemDetails.Detail);
        Assert.Equal("https://tools.ietf.org/html/rfc9110#section-15.5.5", problemDetails.Type);
        Assert.NotNull(problemDetails.Extensions["traceId"]);
    }

    [Fact]
    public async Task ExecuteAsync_WhenHandlerReturnsNotAuthorizedError_ShouldReturnForbiddenProblemResult()
    {
        // Arrange
        GetPlaylistRequest request = _getPlaylistRequestFixture.Create();
        CancellationToken cancellationToken = CancellationToken.None;
        Error expectedError = ApplicationErrors.Authorization.NotAuthorized;
        _mockHandler.HandleAsync(Arg.Any<GetPlaylistQuery>(), Arg.Any<CancellationToken>())
            .Returns(expectedError);
        _sut.HttpContext.Request.RouteValues["playlistId"] = Guid.NewGuid().ToString();

        // Act
        IResult result = await _sut.ExecuteAsync(request, cancellationToken);

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
    }

    [Fact]
    public async Task ExecuteAsync_WhenCalled_ShouldSendGetPlaylistQueryWithTheRawRouteValue()
    {
        // Arrange
        Guid playlistId = Guid.NewGuid();
        GetPlaylistRequest request = _getPlaylistRequestFixture.Create(currentPage: 2, perPage: 5);
        CancellationToken cancellationToken = CancellationToken.None;
        _mockHandler.HandleAsync(Arg.Any<GetPlaylistQuery>(), Arg.Any<CancellationToken>())
            .Returns(Result.From(_playlistResponseFixture.Create()));
        _sut.HttpContext.Request.RouteValues["playlistId"] = playlistId.ToString();

        // Act
        await _sut.ExecuteAsync(request, cancellationToken);

        // Assert
        await _mockHandler.Received(1).HandleAsync(
            Arg.Is<GetPlaylistQuery>(query =>
                query.PlaylistId == playlistId.ToString() &&
                query.PaginationData != null &&
                query.PaginationData.CurrentPage == 2 &&
                query.PaginationData.PerPage == 5),
            Arg.Is(cancellationToken));
    }

    [Fact]
    public async Task ExecuteAsync_WhenNoPaginationIsProvided_ShouldSendQueryWithNullPaginationData()
    {
        // Arrange
        Guid playlistId = Guid.NewGuid();
        GetPlaylistRequest request = _getPlaylistRequestFixture.Create(includeCurrentPage: false, includePerPage: false);
        CancellationToken cancellationToken = CancellationToken.None;
        _mockHandler.HandleAsync(Arg.Any<GetPlaylistQuery>(), Arg.Any<CancellationToken>())
            .Returns(Result.From(_playlistResponseFixture.Create()));
        _sut.HttpContext.Request.RouteValues["playlistId"] = playlistId.ToString();

        // Act
        await _sut.ExecuteAsync(request, cancellationToken);

        // Assert
        await _mockHandler.Received(1).HandleAsync(
            Arg.Is<GetPlaylistQuery>(query => query.PlaylistId == playlistId.ToString() && query.PaginationData == null),
            Arg.Is(cancellationToken));
    }

    [Fact]
    public async Task ExecuteAsync_WhenOnlyCurrentPageIsProvided_ShouldSendQueryWithTheDefaultPerPage()
    {
        // Arrange
        GetPlaylistRequest request = _getPlaylistRequestFixture.Create(currentPage: 7, includePerPage: false);
        CancellationToken cancellationToken = CancellationToken.None;
        _mockHandler.HandleAsync(Arg.Any<GetPlaylistQuery>(), Arg.Any<CancellationToken>())
            .Returns(Result.From(_playlistResponseFixture.Create()));
        _sut.HttpContext.Request.RouteValues["playlistId"] = Guid.NewGuid().ToString();

        // Act
        await _sut.ExecuteAsync(request, cancellationToken);

        // Assert
        await _mockHandler.Received(1).HandleAsync(
            Arg.Is<GetPlaylistQuery>(query =>
                query.PaginationData != null &&
                query.PaginationData.CurrentPage == 7 &&
                query.PaginationData.PerPage == DEFAULT_PER_PAGE),
            Arg.Is(cancellationToken));
    }

    [Fact]
    public async Task ExecuteAsync_WhenOnlyPerPageIsProvided_ShouldSendQueryWithTheFirstPage()
    {
        // Arrange
        GetPlaylistRequest request = _getPlaylistRequestFixture.Create(perPage: 50, includeCurrentPage: false);
        CancellationToken cancellationToken = CancellationToken.None;
        _mockHandler.HandleAsync(Arg.Any<GetPlaylistQuery>(), Arg.Any<CancellationToken>())
            .Returns(Result.From(_playlistResponseFixture.Create()));
        _sut.HttpContext.Request.RouteValues["playlistId"] = Guid.NewGuid().ToString();

        // Act
        await _sut.ExecuteAsync(request, cancellationToken);

        // Assert
        await _mockHandler.Received(1).HandleAsync(
            Arg.Is<GetPlaylistQuery>(query =>
                query.PaginationData != null &&
                query.PaginationData.CurrentPage == 1 &&
                query.PaginationData.PerPage == 50),
            Arg.Is(cancellationToken));
    }

    [Fact]
    public async Task ExecuteAsync_WhenPaginationValuesAreBelowOne_ShouldSendQueryWithClampedPaginationData()
    {
        // Arrange
        GetPlaylistRequest request = _getPlaylistRequestFixture.Create(currentPage: 0, perPage: 0);
        CancellationToken cancellationToken = CancellationToken.None;
        _mockHandler.HandleAsync(Arg.Any<GetPlaylistQuery>(), Arg.Any<CancellationToken>())
            .Returns(Result.From(_playlistResponseFixture.Create()));
        _sut.HttpContext.Request.RouteValues["playlistId"] = Guid.NewGuid().ToString();

        // Act
        await _sut.ExecuteAsync(request, cancellationToken);

        // Assert
        await _mockHandler.Received(1).HandleAsync(
            Arg.Is<GetPlaylistQuery>(query =>
                query.PaginationData != null &&
                query.PaginationData.CurrentPage == 1 &&
                query.PaginationData.PerPage == 1),
            Arg.Is(cancellationToken));
    }

    [Fact]
    public async Task ExecuteAsync_WhenRouteValueIsNotParseable_ShouldSendQueryWithTheRawRouteValue()
    {
        // Arrange
        GetPlaylistRequest request = _getPlaylistRequestFixture.Create();
        CancellationToken cancellationToken = CancellationToken.None;
        _mockHandler.HandleAsync(Arg.Any<GetPlaylistQuery>(), Arg.Any<CancellationToken>())
            .Returns(Result.From(_playlistResponseFixture.Create()));
        _sut.HttpContext.Request.RouteValues["playlistId"] = "not-a-playlist-guid";

        // Act
        await _sut.ExecuteAsync(request, cancellationToken);

        // Assert
        await _mockHandler.Received(1).HandleAsync(
            Arg.Is<GetPlaylistQuery>(query => query.PlaylistId == "not-a-playlist-guid"),
            Arg.Is(cancellationToken));
    }

    [Fact]
    public async Task ExecuteAsync_WhenRouteValueIsMissing_ShouldSendQueryWithNullPlaylistId()
    {
        // Arrange
        GetPlaylistRequest request = _getPlaylistRequestFixture.Create();
        CancellationToken cancellationToken = CancellationToken.None;
        _mockHandler.HandleAsync(Arg.Any<GetPlaylistQuery>(), Arg.Any<CancellationToken>())
            .Returns(Result.From(_playlistResponseFixture.Create()));
        _sut.HttpContext.Request.RouteValues.Remove("playlistId");

        // Act
        await _sut.ExecuteAsync(request, cancellationToken);

        // Assert
        await _mockHandler.Received(1).HandleAsync(
            Arg.Is<GetPlaylistQuery>(query => query.PlaylistId == null),
            Arg.Is(cancellationToken));
    }

    [Fact]
    public async Task ExecuteAsync_WhenCancellationRequested_ShouldCancelOperation()
    {
        // Arrange
        GetPlaylistRequest request = _getPlaylistRequestFixture.Create();
        CancellationTokenSource cts = new();
        TaskCompletionSource<bool> operationStarted = new();
        TaskCompletionSource<bool> cancellationRequested = new();

        _mockHandler.HandleAsync(Arg.Any<GetPlaylistQuery>(), Arg.Any<CancellationToken>())
            .Returns(callInfo => Task.Run(async () =>
            {
                operationStarted.SetResult(true);
                await cancellationRequested.Task;
                callInfo.Arg<CancellationToken>().ThrowIfCancellationRequested();
                return Result.From(_playlistResponseFixture.Create());
            }, callInfo.Arg<CancellationToken>()));
        _sut.HttpContext.Request.RouteValues["playlistId"] = Guid.NewGuid().ToString();

        // Act
        Task<IResult> operationTask = _sut.ExecuteAsync(request, cts.Token);
        await operationStarted.Task;
        cts.Cancel();
        cancellationRequested.SetResult(true);

        // Assert
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => operationTask);
    }
}
