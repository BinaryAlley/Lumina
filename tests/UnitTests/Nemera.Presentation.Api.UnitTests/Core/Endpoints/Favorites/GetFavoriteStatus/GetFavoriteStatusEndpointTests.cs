#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.CQRS;
using Lumina.Application.Core.Favorites.Queries.GetFavoriteStatus;
using Lumina.Contracts.Fixtures.Core.Requests.Favorites;
using Lumina.Contracts.Fixtures.Core.Responses.Favorites;
using Lumina.Contracts.Requests.Favorites;
using Lumina.Contracts.Responses.Favorites;
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.SharedKernel.Common.Enums.Favorites;
using Lumina.Presentation.Api.Core.Endpoints.Favorites.GetFavoriteStatus;
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

namespace Lumina.Presentation.Api.UnitTests.Core.Endpoints.Favorites.GetFavoriteStatus;

/// <summary>
/// Contains unit tests for the <see cref="GetFavoriteStatusEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetFavoriteStatusEndpointTests
{
    private readonly IQueryHandler<GetFavoriteStatusQuery, Result<FavoriteStatusResponse>> _mockHandler;
    private readonly GetFavoriteStatusEndpoint _sut;
    private readonly GetFavoriteStatusRequestFixture _getFavoriteStatusRequestFixture = new();
    private readonly FavoriteStatusResponseFixture _favoriteStatusResponseFixture = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="GetFavoriteStatusEndpointTests"/> class.
    /// </summary>
    public GetFavoriteStatusEndpointTests()
    {
        _mockHandler = Substitute.For<IQueryHandler<GetFavoriteStatusQuery, Result<FavoriteStatusResponse>>>();
        _sut = FastEndpoints.Factory.Create<GetFavoriteStatusEndpoint>(_mockHandler);
    }

    [Fact]
    public async Task ExecuteAsync_WhenSuccessful_ShouldReturnOkResultWithFavoriteStatusResponse()
    {
        // Arrange
        GetFavoriteStatusRequest request = _getFavoriteStatusRequestFixture.Create();
        CancellationToken cancellationToken = CancellationToken.None;
        FavoriteStatusResponse expectedResponse = _favoriteStatusResponseFixture.Create();
        _mockHandler.HandleAsync(Arg.Any<GetFavoriteStatusQuery>(), Arg.Any<CancellationToken>())
            .Returns(Result.From(expectedResponse));

        // Act
        IResult result = await _sut.ExecuteAsync(request, cancellationToken);

        // Assert
        Ok<FavoriteStatusResponse> okResult = Assert.IsType<Ok<FavoriteStatusResponse>>(result);
        Assert.Equal(expectedResponse, okResult.Value);
    }

    [Fact]
    public async Task ExecuteAsync_WhenSuccessful_ShouldSetPrivateNoStoreCacheHeaders()
    {
        // Arrange
        GetFavoriteStatusRequest request = _getFavoriteStatusRequestFixture.Create();
        CancellationToken cancellationToken = CancellationToken.None;
        _mockHandler.HandleAsync(Arg.Any<GetFavoriteStatusQuery>(), Arg.Any<CancellationToken>())
            .Returns(Result.From(_favoriteStatusResponseFixture.Create()));

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
        GetFavoriteStatusRequest request = _getFavoriteStatusRequestFixture.Create();
        CancellationToken cancellationToken = CancellationToken.None;
        List<Error> expectedErrors =
        [
            DomainErrors.Favorites.FavoriteMediaItemIdsCannotBeEmpty,
            DomainErrors.Favorites.FavoriteMediaItemIdsExceedMaximumCount,
            DomainErrors.Favorites.FavoriteMediaItemIdCannotBeEmpty
        ];
        _mockHandler.HandleAsync(Arg.Any<GetFavoriteStatusQuery>(), Arg.Any<CancellationToken>())
            .Returns(expectedErrors);

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
        Assert.Equal(
            new[] { "FavoriteMediaItemIdsCannotBeEmpty", "FavoriteMediaItemIdsExceedMaximumCount", "FavoriteMediaItemIdCannotBeEmpty" },
            singleErrorGroup.Value);
    }

    [Fact]
    public async Task ExecuteAsync_WhenHandlerReturnsError_ShouldStillSetPrivateNoStoreCacheHeaders()
    {
        // Arrange
        GetFavoriteStatusRequest request = _getFavoriteStatusRequestFixture.Create();
        Error expectedError = DomainErrors.Favorites.FavoriteMediaKindIsInvalid;
        _mockHandler.HandleAsync(Arg.Any<GetFavoriteStatusQuery>(), Arg.Any<CancellationToken>())
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
        GetFavoriteStatusRequest request = _getFavoriteStatusRequestFixture.Create();
        CancellationToken cancellationToken = CancellationToken.None;
        Error expectedError = ApplicationErrors.Authorization.NotAuthorized;
        _mockHandler.HandleAsync(Arg.Any<GetFavoriteStatusQuery>(), Arg.Any<CancellationToken>())
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
    public async Task ExecuteAsync_WhenCalled_ShouldSendGetFavoriteStatusQueryToSender()
    {
        // Arrange
        List<Guid> mediaItemIds = [Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid()];
        GetFavoriteStatusRequest request = _getFavoriteStatusRequestFixture.Create(mediaKind: FavoriteMediaKind.Book, mediaItemIds: mediaItemIds);
        CancellationToken cancellationToken = CancellationToken.None;
        _mockHandler.HandleAsync(Arg.Any<GetFavoriteStatusQuery>(), Arg.Any<CancellationToken>())
            .Returns(Result.From(_favoriteStatusResponseFixture.Create()));

        // Act
        await _sut.ExecuteAsync(request, cancellationToken);

        // Assert
        await _mockHandler.Received(1).HandleAsync(
            Arg.Is<GetFavoriteStatusQuery>(query =>
                query.MediaKind == FavoriteMediaKind.Book &&
                query.MediaItemIds == mediaItemIds),
            Arg.Is(cancellationToken));
    }

    [Fact]
    public async Task ExecuteAsync_WhenRequestValuesAreNull_ShouldSendQueryWithNullValues()
    {
        // Arrange
        GetFavoriteStatusRequest request = _getFavoriteStatusRequestFixture.Create(includeMediaKind: false, includeMediaItemIds: false);
        CancellationToken cancellationToken = CancellationToken.None;
        _mockHandler.HandleAsync(Arg.Any<GetFavoriteStatusQuery>(), Arg.Any<CancellationToken>())
            .Returns(Result.From(_favoriteStatusResponseFixture.Create()));

        // Act
        await _sut.ExecuteAsync(request, cancellationToken);

        // Assert
        await _mockHandler.Received(1).HandleAsync(
            Arg.Is<GetFavoriteStatusQuery>(query => query.MediaKind == null && query.MediaItemIds == null),
            Arg.Is(cancellationToken));
    }

    [Fact]
    public async Task ExecuteAsync_WhenCancellationRequested_ShouldCancelOperation()
    {
        // Arrange
        GetFavoriteStatusRequest request = _getFavoriteStatusRequestFixture.Create();
        CancellationTokenSource cts = new();
        TaskCompletionSource<bool> operationStarted = new();
        TaskCompletionSource<bool> cancellationRequested = new();

        _mockHandler.HandleAsync(Arg.Any<GetFavoriteStatusQuery>(), Arg.Any<CancellationToken>())
            .Returns(callInfo => Task.Run(async () =>
            {
                operationStarted.SetResult(true);
                await cancellationRequested.Task;
                callInfo.Arg<CancellationToken>().ThrowIfCancellationRequested();
                return Result.From(_favoriteStatusResponseFixture.Create());
            }, callInfo.Arg<CancellationToken>()));

        // Act
        Task<IResult> operationTask = _sut.ExecuteAsync(request, cts.Token);
        await operationStarted.Task;
        cts.Cancel();
        cancellationRequested.SetResult(true);

        // Assert
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => operationTask);
    }
}
