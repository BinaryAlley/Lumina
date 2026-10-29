#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.CQRS;
using Lumina.Application.Core.Favorites.Queries.GetFavorites;
using Lumina.Contracts.Fixtures.Core.Requests.Favorites;
using Lumina.Contracts.Fixtures.Core.Responses.Favorites;
using Lumina.Contracts.Fixtures.Responses.Common;
using Lumina.Contracts.Requests.Favorites;
using Lumina.Contracts.Responses.Common;
using Lumina.Contracts.Responses.Favorites;
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.SharedKernel.Common.Enums.Favorites;
using Lumina.Presentation.Api.Core.Endpoints.Favorites.GetFavorites;
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

namespace Lumina.Presentation.Api.UnitTests.Core.Endpoints.Favorites.GetFavorites;

/// <summary>
/// Contains unit tests for the <see cref="GetFavoritesEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetFavoritesEndpointTests
{
    private const int DEFAULT_PER_PAGE = 200;

    private readonly IQueryHandler<GetFavoritesQuery, Result<PaginatedResponse<FavoriteItemResponse>>> _mockHandler;
    private readonly GetFavoritesEndpoint _sut;
    private readonly GetFavoritesRequestFixture _getFavoritesRequestFixture = new();
    private readonly FavoriteItemResponseFixture _favoriteItemResponseFixture = new();
    private readonly PaginatedResponseFixture<FavoriteItemResponse> _paginatedResponseFixture = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="GetFavoritesEndpointTests"/> class.
    /// </summary>
    public GetFavoritesEndpointTests()
    {
        _mockHandler = Substitute.For<IQueryHandler<GetFavoritesQuery, Result<PaginatedResponse<FavoriteItemResponse>>>>();
        _sut = FastEndpoints.Factory.Create<GetFavoritesEndpoint>(_mockHandler);
    }

    [Fact]
    public async Task ExecuteAsync_WhenSuccessful_ShouldReturnOkResultWithPaginatedFavoriteItemResponses()
    {
        // Arrange
        GetFavoritesRequest request = _getFavoritesRequestFixture.Create();
        CancellationToken cancellationToken = CancellationToken.None;
        PaginatedResponse<FavoriteItemResponse> expectedResponse = CreatePaginatedResponse(favoriteCount: 2);
        _mockHandler.HandleAsync(Arg.Any<GetFavoritesQuery>(), Arg.Any<CancellationToken>())
            .Returns(Result.From(expectedResponse));

        // Act
        IResult result = await _sut.ExecuteAsync(request, cancellationToken);

        // Assert
        Ok<PaginatedResponse<FavoriteItemResponse>> okResult = Assert.IsType<Ok<PaginatedResponse<FavoriteItemResponse>>>(result);
        Assert.Equal(expectedResponse, okResult.Value);
    }

    [Fact]
    public async Task ExecuteAsync_WhenSuccessful_ShouldSetPrivateNoStoreCacheHeaders()
    {
        // Arrange
        GetFavoritesRequest request = _getFavoritesRequestFixture.Create();
        CancellationToken cancellationToken = CancellationToken.None;
        _mockHandler.HandleAsync(Arg.Any<GetFavoritesQuery>(), Arg.Any<CancellationToken>())
            .Returns(Result.From(CreatePaginatedResponse(favoriteCount: 0)));

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
        GetFavoritesRequest request = _getFavoritesRequestFixture.Create();
        CancellationToken cancellationToken = CancellationToken.None;
        Error expectedError = DomainErrors.Favorites.FavoriteMediaKindIsInvalid;
        _mockHandler.HandleAsync(Arg.Any<GetFavoritesQuery>(), Arg.Any<CancellationToken>())
            .Returns(expectedError);

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
        Assert.Equal(new[] { "FavoriteMediaKindIsInvalid" }, singleErrorGroup.Value);
    }

    [Fact]
    public async Task ExecuteAsync_WhenHandlerReturnsError_ShouldStillSetPrivateNoStoreCacheHeaders()
    {
        // Arrange
        GetFavoritesRequest request = _getFavoritesRequestFixture.Create();
        Error expectedError = DomainErrors.Favorites.FavoriteMediaKindIsInvalid;
        _mockHandler.HandleAsync(Arg.Any<GetFavoritesQuery>(), Arg.Any<CancellationToken>())
            .Returns(expectedError);

        // Act
        await _sut.ExecuteAsync(request, CancellationToken.None);

        // Assert
        Assert.Equal("private, no-store", _sut.HttpContext.Response.Headers["Cache-Control"].ToString());
        Assert.Equal("Authorization", _sut.HttpContext.Response.Headers["Vary"].ToString());
    }

    [Fact]
    public async Task ExecuteAsync_WhenHandlerReturnsNotAuthorizedError_ShouldReturnForbiddenProblemResult()
    {
        // Arrange
        GetFavoritesRequest request = _getFavoritesRequestFixture.Create();
        CancellationToken cancellationToken = CancellationToken.None;
        Error expectedError = ApplicationErrors.Authorization.NotAuthorized;
        _mockHandler.HandleAsync(Arg.Any<GetFavoritesQuery>(), Arg.Any<CancellationToken>())
            .Returns(expectedError);

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
    public async Task ExecuteAsync_WhenCalled_ShouldSendGetFavoritesQueryToSender()
    {
        // Arrange
        GetFavoritesRequest request = _getFavoritesRequestFixture.Create(mediaKind: FavoriteMediaKind.MusicTrack, currentPage: 3, perPage: 10);
        CancellationToken cancellationToken = CancellationToken.None;
        _mockHandler.HandleAsync(Arg.Any<GetFavoritesQuery>(), Arg.Any<CancellationToken>())
            .Returns(Result.From(CreatePaginatedResponse(favoriteCount: 0)));

        // Act
        await _sut.ExecuteAsync(request, cancellationToken);

        // Assert
        await _mockHandler.Received(1).HandleAsync(
            Arg.Is<GetFavoritesQuery>(query =>
                query.MediaKind == FavoriteMediaKind.MusicTrack &&
                query.PaginationData != null &&
                query.PaginationData.CurrentPage == 3 &&
                query.PaginationData.PerPage == 10),
            Arg.Is(cancellationToken));
    }

    [Fact]
    public async Task ExecuteAsync_WhenNoPaginationIsProvided_ShouldSendQueryWithNullPaginationData()
    {
        // Arrange
        GetFavoritesRequest request = _getFavoritesRequestFixture.Create(mediaKind: FavoriteMediaKind.Book, includeCurrentPage: false, includePerPage: false);
        CancellationToken cancellationToken = CancellationToken.None;
        _mockHandler.HandleAsync(Arg.Any<GetFavoritesQuery>(), Arg.Any<CancellationToken>())
            .Returns(Result.From(CreatePaginatedResponse(favoriteCount: 0)));

        // Act
        await _sut.ExecuteAsync(request, cancellationToken);

        // Assert
        await _mockHandler.Received(1).HandleAsync(
            Arg.Is<GetFavoritesQuery>(query => query.MediaKind == FavoriteMediaKind.Book && query.PaginationData == null),
            Arg.Is(cancellationToken));
    }

    [Fact]
    public async Task ExecuteAsync_WhenOnlyCurrentPageIsProvided_ShouldSendQueryWithTheDefaultPerPage()
    {
        // Arrange
        GetFavoritesRequest request = _getFavoritesRequestFixture.Create(currentPage: 4, includePerPage: false);
        CancellationToken cancellationToken = CancellationToken.None;
        _mockHandler.HandleAsync(Arg.Any<GetFavoritesQuery>(), Arg.Any<CancellationToken>())
            .Returns(Result.From(CreatePaginatedResponse(favoriteCount: 0)));

        // Act
        await _sut.ExecuteAsync(request, cancellationToken);

        // Assert
        await _mockHandler.Received(1).HandleAsync(
            Arg.Is<GetFavoritesQuery>(query =>
                query.PaginationData != null &&
                query.PaginationData.CurrentPage == 4 &&
                query.PaginationData.PerPage == DEFAULT_PER_PAGE),
            Arg.Is(cancellationToken));
    }

    [Fact]
    public async Task ExecuteAsync_WhenOnlyPerPageIsProvided_ShouldSendQueryWithTheFirstPage()
    {
        // Arrange
        GetFavoritesRequest request = _getFavoritesRequestFixture.Create(perPage: 25, includeCurrentPage: false);
        CancellationToken cancellationToken = CancellationToken.None;
        _mockHandler.HandleAsync(Arg.Any<GetFavoritesQuery>(), Arg.Any<CancellationToken>())
            .Returns(Result.From(CreatePaginatedResponse(favoriteCount: 0)));

        // Act
        await _sut.ExecuteAsync(request, cancellationToken);

        // Assert
        await _mockHandler.Received(1).HandleAsync(
            Arg.Is<GetFavoritesQuery>(query =>
                query.PaginationData != null &&
                query.PaginationData.CurrentPage == 1 &&
                query.PaginationData.PerPage == 25),
            Arg.Is(cancellationToken));
    }

    [Fact]
    public async Task ExecuteAsync_WhenPaginationValuesAreBelowOne_ShouldSendQueryWithClampedPaginationData()
    {
        // Arrange
        GetFavoritesRequest request = _getFavoritesRequestFixture.Create(currentPage: 0, perPage: 0);
        CancellationToken cancellationToken = CancellationToken.None;
        _mockHandler.HandleAsync(Arg.Any<GetFavoritesQuery>(), Arg.Any<CancellationToken>())
            .Returns(Result.From(CreatePaginatedResponse(favoriteCount: 0)));

        // Act
        await _sut.ExecuteAsync(request, cancellationToken);

        // Assert
        await _mockHandler.Received(1).HandleAsync(
            Arg.Is<GetFavoritesQuery>(query =>
                query.PaginationData != null &&
                query.PaginationData.CurrentPage == 1 &&
                query.PaginationData.PerPage == 1),
            Arg.Is(cancellationToken));
    }

    [Fact]
    public async Task ExecuteAsync_WhenCancellationRequested_ShouldCancelOperation()
    {
        // Arrange
        GetFavoritesRequest request = _getFavoritesRequestFixture.Create();
        CancellationTokenSource cts = new();
        TaskCompletionSource<bool> operationStarted = new();
        TaskCompletionSource<bool> cancellationRequested = new();

        _mockHandler.HandleAsync(Arg.Any<GetFavoritesQuery>(), Arg.Any<CancellationToken>())
            .Returns(callInfo => Task.Run(async () =>
            {
                operationStarted.SetResult(true);
                await cancellationRequested.Task;
                callInfo.Arg<CancellationToken>().ThrowIfCancellationRequested();
                return Result.From(CreatePaginatedResponse(favoriteCount: 0));
            }, callInfo.Arg<CancellationToken>()));

        // Act
        Task<IResult> operationTask = _sut.ExecuteAsync(request, cts.Token);
        await operationStarted.Task;
        cts.Cancel();
        cancellationRequested.SetResult(true);

        // Assert
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => operationTask);
    }

    /// <summary>
    /// Creates a <see cref="PaginatedResponse{FavoriteItemResponse}"/> with the specified number of favorites.
    /// </summary>
    /// <param name="favoriteCount">The number of favorite item responses to include in the data collection.</param>
    /// <returns>A configured paginated response instance.</returns>
    private PaginatedResponse<FavoriteItemResponse> CreatePaginatedResponse(int favoriteCount)
    {
        List<FavoriteItemResponse> favorites = [];
        for (int index = 0; index < favoriteCount; index++)
            favorites.Add(_favoriteItemResponseFixture.Create());

        return _paginatedResponseFixture.Create(data: favorites, currentPage: 1, perPage: 10, count: favoriteCount, numberOfPages: favoriteCount > 0 ? 1 : 0);
    }
}
