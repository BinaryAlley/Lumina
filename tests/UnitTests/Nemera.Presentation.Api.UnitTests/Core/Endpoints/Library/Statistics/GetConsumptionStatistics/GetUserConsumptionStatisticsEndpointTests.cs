#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.CQRS;
using Lumina.Application.Core.MediaConsumption.Queries.GetConsumptionStatistics;
using Lumina.Contracts.Fixtures.Core.Requests.MediaConsumption;
using Lumina.Contracts.Fixtures.Core.Responses.MediaConsumption;
using Lumina.Contracts.Requests.MediaConsumption;
using Lumina.Contracts.Responses.MediaConsumption;
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.SharedKernel.Common.Enums.MediaLibrary;
using Lumina.Presentation.Api.Core.Endpoints.Library.Statistics.GetConsumptionStatistics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using ApplicationErrors = Lumina.Application.Common.Errors.Errors;
using DomainErrors = Lumina.Domain.Common.Errors.Errors;
#endregion

namespace Lumina.Presentation.Api.UnitTests.Core.Endpoints.Library.Statistics.GetConsumptionStatistics;

/// <summary>
/// Contains unit tests for the <see cref="GetUserConsumptionStatisticsEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetUserConsumptionStatisticsEndpointTests
{
    private readonly IQueryHandler<GetConsumptionStatisticsQuery, Result<ConsumptionSummaryResponse>> _mockHandler;
    private readonly GetUserConsumptionStatisticsEndpoint _sut;
    private readonly GetConsumptionStatisticsRequestFixture _getConsumptionStatisticsRequestFixture = new();
    private readonly ConsumptionSummaryResponseFixture _consumptionSummaryResponseFixture = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="GetUserConsumptionStatisticsEndpointTests"/> class.
    /// </summary>
    public GetUserConsumptionStatisticsEndpointTests()
    {
        _mockHandler = Substitute.For<IQueryHandler<GetConsumptionStatisticsQuery, Result<ConsumptionSummaryResponse>>>();
        _sut = FastEndpoints.Factory.Create<GetUserConsumptionStatisticsEndpoint>(_mockHandler);
    }

    [Fact]
    public async Task ExecuteAsync_WhenSuccessful_ShouldReturnOkResultWithConsumptionSummaryResponse()
    {
        // Arrange
        GetConsumptionStatisticsRequest request = _getConsumptionStatisticsRequestFixture.Create();
        CancellationToken cancellationToken = CancellationToken.None;
        ConsumptionSummaryResponse expectedResponse = _consumptionSummaryResponseFixture.Create();
        _mockHandler.HandleAsync(Arg.Any<GetConsumptionStatisticsQuery>(), Arg.Any<CancellationToken>())
            .Returns(Result.From(expectedResponse));
        _sut.HttpContext.Request.RouteValues["userId"] = Guid.NewGuid().ToString();

        // Act
        IResult result = await _sut.ExecuteAsync(request, cancellationToken);

        // Assert
        Ok<ConsumptionSummaryResponse> okResult = Assert.IsType<Ok<ConsumptionSummaryResponse>>(result);
        Assert.Equal(expectedResponse, okResult.Value);
    }

    [Fact]
    public async Task ExecuteAsync_WhenSuccessful_ShouldSetPrivateNoStoreCacheHeaders()
    {
        // Arrange
        GetConsumptionStatisticsRequest request = _getConsumptionStatisticsRequestFixture.Create();
        CancellationToken cancellationToken = CancellationToken.None;
        _mockHandler.HandleAsync(Arg.Any<GetConsumptionStatisticsQuery>(), Arg.Any<CancellationToken>())
            .Returns(Result.From(_consumptionSummaryResponseFixture.Create()));
        _sut.HttpContext.Request.RouteValues["userId"] = Guid.NewGuid().ToString();

        // Act
        await _sut.ExecuteAsync(request, cancellationToken);

        // Assert
        Assert.Equal("private, no-store", _sut.HttpContext.Response.Headers["Cache-Control"].ToString());
        Assert.Contains("Authorization", _sut.HttpContext.Response.Headers["Vary"].ToString());
    }

    [Fact]
    public async Task ExecuteAsync_WhenHandlerReturnsError_ShouldStillSetPrivateNoStoreCacheHeaders()
    {
        // Arrange
        GetConsumptionStatisticsRequest request = _getConsumptionStatisticsRequestFixture.Create();
        Error expectedError = ApplicationErrors.Authorization.NotAuthorized;
        _mockHandler.HandleAsync(Arg.Any<GetConsumptionStatisticsQuery>(), Arg.Any<CancellationToken>())
            .Returns(expectedError);
        _sut.HttpContext.Request.RouteValues["userId"] = Guid.NewGuid().ToString();

        // Act
        await _sut.ExecuteAsync(request, CancellationToken.None);

        // Assert
        Assert.Equal("private, no-store", _sut.HttpContext.Response.Headers["Cache-Control"].ToString());
        Assert.Contains("Authorization", _sut.HttpContext.Response.Headers["Vary"].ToString());
    }

    [Fact]
    public async Task ExecuteAsync_WhenHandlerReturnsValidationErrors_ShouldReturnValidationProblemResultWithErrorsGroupedByCode()
    {
        // Arrange
        GetConsumptionStatisticsRequest request = _getConsumptionStatisticsRequestFixture.Create();
        CancellationToken cancellationToken = CancellationToken.None;
        List<Error> expectedErrors = [DomainErrors.MediaConsumption.TargetUserIdIsInvalid, DomainErrors.MediaConsumption.ConsumptionDateRangeIsInvalid];
        _mockHandler.HandleAsync(Arg.Any<GetConsumptionStatisticsQuery>(), Arg.Any<CancellationToken>())
            .Returns(expectedErrors);
        _sut.HttpContext.Request.RouteValues["userId"] = "not-a-user-guid";

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
        Assert.Equal(new[] { "TargetUserIdIsInvalid", "ConsumptionDateRangeIsInvalid" }, singleErrorGroup.Value);
    }

    [Fact]
    public async Task ExecuteAsync_WhenHandlerReturnsNotAuthorizedError_ShouldReturnForbiddenProblemResult()
    {
        // Arrange
        GetConsumptionStatisticsRequest request = _getConsumptionStatisticsRequestFixture.Create();
        CancellationToken cancellationToken = CancellationToken.None;
        Error expectedError = ApplicationErrors.Authorization.NotAuthorized;
        _mockHandler.HandleAsync(Arg.Any<GetConsumptionStatisticsQuery>(), Arg.Any<CancellationToken>())
            .Returns(expectedError);
        _sut.HttpContext.Request.RouteValues["userId"] = Guid.NewGuid().ToString();

        // Act
        IResult result = await _sut.ExecuteAsync(request, cancellationToken);

        // Assert
        ProblemHttpResult problemResult = Assert.IsType<ProblemHttpResult>(result);
        Assert.Equal(StatusCodes.Status403Forbidden, problemResult.StatusCode);
        Assert.Equal("application/problem+json", problemResult.ContentType);
        Microsoft.AspNetCore.Mvc.ProblemDetails problemDetails = Assert.IsType<Microsoft.AspNetCore.Mvc.ProblemDetails>(problemResult.ProblemDetails);
        Assert.Equal(StatusCodes.Status403Forbidden, problemDetails.Status);
        Assert.Equal("General.Unauthorized", problemDetails.Title);
        Assert.Equal("NotAuthorized", problemDetails.Detail);
        Assert.Equal("https://tools.ietf.org/html/rfc9110#section-15.5.4", problemDetails.Type);
        Assert.NotNull(problemDetails.Extensions["traceId"]);
    }

    [Fact]
    public async Task ExecuteAsync_WhenCalled_ShouldSendGetConsumptionStatisticsQueryWithRouteUserIdAndMappedRequest()
    {
        // Arrange
        Guid userId = Guid.NewGuid();
        string libraryId = Guid.NewGuid().ToString();
        string fromUtc = "2026-01-01T00:00:00Z";
        string toUtc = "2026-02-01T00:00:00Z";
        GetConsumptionStatisticsRequest request = _getConsumptionStatisticsRequestFixture.Create(
            libraryId: libraryId,
            mediaKind: nameof(MediaKind.TvEpisode),
            fromUtc: fromUtc,
            toUtc: toUtc);
        CancellationToken cancellationToken = CancellationToken.None;
        _mockHandler.HandleAsync(Arg.Any<GetConsumptionStatisticsQuery>(), Arg.Any<CancellationToken>())
            .Returns(Result.From(_consumptionSummaryResponseFixture.Create()));
        _sut.HttpContext.Request.RouteValues["userId"] = userId.ToString();

        // Act
        await _sut.ExecuteAsync(request, cancellationToken);

        // Assert
        await _mockHandler.Received(1).HandleAsync(
            Arg.Is<GetConsumptionStatisticsQuery>(query =>
                query.TargetUserId == userId.ToString() &&
                query.LibraryId == libraryId &&
                query.MediaKind == nameof(MediaKind.TvEpisode) &&
                query.FromUtc == fromUtc &&
                query.ToUtc == toUtc),
            Arg.Is(cancellationToken));
    }

    [Fact]
    public async Task ExecuteAsync_WhenRouteValueIsMissing_ShouldSendQueryWithNullTargetUserId()
    {
        // Arrange
        GetConsumptionStatisticsRequest request = _getConsumptionStatisticsRequestFixture.Create();
        CancellationToken cancellationToken = CancellationToken.None;
        _mockHandler.HandleAsync(Arg.Any<GetConsumptionStatisticsQuery>(), Arg.Any<CancellationToken>())
            .Returns(Result.From(_consumptionSummaryResponseFixture.Create()));
        _sut.HttpContext.Request.RouteValues.Remove("userId");

        // Act
        await _sut.ExecuteAsync(request, cancellationToken);

        // Assert
        await _mockHandler.Received(1).HandleAsync(
            Arg.Is<GetConsumptionStatisticsQuery>(query =>
                query.TargetUserId == null &&
                query.LibraryId == request.LibraryId &&
                query.MediaKind == request.MediaKind &&
                query.FromUtc == request.FromUtc &&
                query.ToUtc == request.ToUtc),
            Arg.Is(cancellationToken));
    }

    [Fact]
    public async Task ExecuteAsync_WhenCancellationRequested_ShouldCancelOperation()
    {
        // Arrange
        GetConsumptionStatisticsRequest request = _getConsumptionStatisticsRequestFixture.Create();
        CancellationTokenSource cts = new();
        TaskCompletionSource<bool> operationStarted = new();
        TaskCompletionSource<bool> cancellationRequested = new();

        _mockHandler.HandleAsync(Arg.Any<GetConsumptionStatisticsQuery>(), Arg.Any<CancellationToken>())
            .Returns(callInfo => Task.Run(async () =>
            {
                operationStarted.SetResult(true);
                await cancellationRequested.Task;
                callInfo.Arg<CancellationToken>().ThrowIfCancellationRequested();
                return Result.From(_consumptionSummaryResponseFixture.Create());
            }, callInfo.Arg<CancellationToken>()));
        _sut.HttpContext.Request.RouteValues["userId"] = Guid.NewGuid().ToString();

        // Act
        Task<IResult> operationTask = _sut.ExecuteAsync(request, cts.Token);
        await operationStarted.Task;
        cts.Cancel();
        cancellationRequested.SetResult(true);

        // Assert
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => operationTask);
    }
}
