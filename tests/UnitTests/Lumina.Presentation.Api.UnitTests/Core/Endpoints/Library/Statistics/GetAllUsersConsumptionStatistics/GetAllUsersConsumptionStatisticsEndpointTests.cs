#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.CQRS;
using Lumina.Application.Core.MediaConsumption.Queries.GetAllUsersConsumptionStatistics;
using Lumina.Contracts.Fixtures.Core.Requests.MediaConsumption;
using Lumina.Contracts.Fixtures.Core.Responses.MediaConsumption;
using Lumina.Contracts.Requests.MediaConsumption;
using Lumina.Contracts.Responses.MediaConsumption;
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.SharedKernel.Common.Enums.MediaLibrary;
using Lumina.Presentation.Api.Core.Endpoints.Library.Statistics.GetAllUsersConsumptionStatistics;
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

namespace Lumina.Presentation.Api.UnitTests.Core.Endpoints.Library.Statistics.GetAllUsersConsumptionStatistics;

/// <summary>
/// Contains unit tests for the <see cref="GetAllUsersConsumptionStatisticsEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetAllUsersConsumptionStatisticsEndpointTests
{
    private readonly IQueryHandler<GetAllUsersConsumptionStatisticsQuery, Result<AllUsersConsumptionStatisticsResponse>> _mockHandler;
    private readonly GetAllUsersConsumptionStatisticsEndpoint _sut;
    private readonly GetAllUsersConsumptionStatisticsRequestFixture _getAllUsersConsumptionStatisticsRequestFixture = new();
    private readonly AllUsersConsumptionStatisticsResponseFixture _allUsersConsumptionStatisticsResponseFixture = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="GetAllUsersConsumptionStatisticsEndpointTests"/> class.
    /// </summary>
    public GetAllUsersConsumptionStatisticsEndpointTests()
    {
        _mockHandler = Substitute.For<IQueryHandler<GetAllUsersConsumptionStatisticsQuery, Result<AllUsersConsumptionStatisticsResponse>>>();
        _sut = FastEndpoints.Factory.Create<GetAllUsersConsumptionStatisticsEndpoint>(_mockHandler);
    }

    [Fact]
    public async Task ExecuteAsync_WhenSuccessful_ShouldReturnOkResultWithAllUsersConsumptionStatisticsResponse()
    {
        // Arrange
        GetAllUsersConsumptionStatisticsRequest request = _getAllUsersConsumptionStatisticsRequestFixture.Create();
        CancellationToken cancellationToken = CancellationToken.None;
        AllUsersConsumptionStatisticsResponse expectedResponse = _allUsersConsumptionStatisticsResponseFixture.Create();
        _mockHandler.HandleAsync(Arg.Any<GetAllUsersConsumptionStatisticsQuery>(), Arg.Any<CancellationToken>())
            .Returns(Result.From(expectedResponse));

        // Act
        IResult result = await _sut.ExecuteAsync(request, cancellationToken);

        // Assert
        Ok<AllUsersConsumptionStatisticsResponse> okResult = Assert.IsType<Ok<AllUsersConsumptionStatisticsResponse>>(result);
        Assert.Equal(expectedResponse, okResult.Value);
    }

    [Fact]
    public async Task ExecuteAsync_WhenSuccessful_ShouldSetPrivateNoStoreCacheHeaders()
    {
        // Arrange
        GetAllUsersConsumptionStatisticsRequest request = _getAllUsersConsumptionStatisticsRequestFixture.Create();
        CancellationToken cancellationToken = CancellationToken.None;
        _mockHandler.HandleAsync(Arg.Any<GetAllUsersConsumptionStatisticsQuery>(), Arg.Any<CancellationToken>())
            .Returns(Result.From(_allUsersConsumptionStatisticsResponseFixture.Create()));

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
        GetAllUsersConsumptionStatisticsRequest request = _getAllUsersConsumptionStatisticsRequestFixture.Create();
        Error expectedError = ApplicationErrors.Authorization.NotAuthorized;
        _mockHandler.HandleAsync(Arg.Any<GetAllUsersConsumptionStatisticsQuery>(), Arg.Any<CancellationToken>())
            .Returns(expectedError);

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
        GetAllUsersConsumptionStatisticsRequest request = _getAllUsersConsumptionStatisticsRequestFixture.Create();
        CancellationToken cancellationToken = CancellationToken.None;
        List<Error> expectedErrors = [DomainErrors.MediaConsumption.ConsumptionDateRangeIsInvalid, DomainErrors.Library.LibraryIdCannotBeEmpty];
        _mockHandler.HandleAsync(Arg.Any<GetAllUsersConsumptionStatisticsQuery>(), Arg.Any<CancellationToken>())
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
        Assert.Equal(new[] { "ConsumptionDateRangeIsInvalid", "LibraryIdCannotBeEmpty" }, singleErrorGroup.Value);
    }

    [Fact]
    public async Task ExecuteAsync_WhenHandlerReturnsNotAuthorizedError_ShouldReturnForbiddenProblemResult()
    {
        // Arrange
        GetAllUsersConsumptionStatisticsRequest request = _getAllUsersConsumptionStatisticsRequestFixture.Create();
        CancellationToken cancellationToken = CancellationToken.None;
        Error expectedError = ApplicationErrors.Authorization.NotAuthorized;
        _mockHandler.HandleAsync(Arg.Any<GetAllUsersConsumptionStatisticsQuery>(), Arg.Any<CancellationToken>())
            .Returns(expectedError);

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
    public async Task ExecuteAsync_WhenCalled_ShouldSendGetAllUsersConsumptionStatisticsQueryWithMappedRequest()
    {
        // Arrange
        string libraryId = Guid.NewGuid().ToString();
        string fromUtc = "2026-01-01T00:00:00Z";
        string toUtc = "2026-02-01T00:00:00Z";
        GetAllUsersConsumptionStatisticsRequest request = _getAllUsersConsumptionStatisticsRequestFixture.Create(
            libraryId: libraryId,
            mediaKind: nameof(MediaKind.Movie),
            fromUtc: fromUtc,
            toUtc: toUtc);
        CancellationToken cancellationToken = CancellationToken.None;
        _mockHandler.HandleAsync(Arg.Any<GetAllUsersConsumptionStatisticsQuery>(), Arg.Any<CancellationToken>())
            .Returns(Result.From(_allUsersConsumptionStatisticsResponseFixture.Create()));

        // Act
        await _sut.ExecuteAsync(request, cancellationToken);

        // Assert
        await _mockHandler.Received(1).HandleAsync(
            Arg.Is<GetAllUsersConsumptionStatisticsQuery>(query =>
                query.LibraryId == libraryId &&
                query.MediaKind == nameof(MediaKind.Movie) &&
                query.FromUtc == fromUtc &&
                query.ToUtc == toUtc),
            Arg.Is(cancellationToken));
    }

    [Fact]
    public async Task ExecuteAsync_WhenOptionalFiltersAreNotProvided_ShouldSendQueryWithNullValues()
    {
        // Arrange
        GetAllUsersConsumptionStatisticsRequest request = _getAllUsersConsumptionStatisticsRequestFixture.Create(
            includeLibraryId: false,
            includeMediaKind: false,
            includeFromUtc: false,
            includeToUtc: false);
        CancellationToken cancellationToken = CancellationToken.None;
        _mockHandler.HandleAsync(Arg.Any<GetAllUsersConsumptionStatisticsQuery>(), Arg.Any<CancellationToken>())
            .Returns(Result.From(_allUsersConsumptionStatisticsResponseFixture.Create()));

        // Act
        await _sut.ExecuteAsync(request, cancellationToken);

        // Assert
        await _mockHandler.Received(1).HandleAsync(
            Arg.Is<GetAllUsersConsumptionStatisticsQuery>(query =>
                query.LibraryId == null &&
                query.MediaKind == null &&
                query.FromUtc == null &&
                query.ToUtc == null),
            Arg.Is(cancellationToken));
    }

    [Fact]
    public async Task ExecuteAsync_WhenCancellationRequested_ShouldCancelOperation()
    {
        // Arrange
        GetAllUsersConsumptionStatisticsRequest request = _getAllUsersConsumptionStatisticsRequestFixture.Create();
        CancellationTokenSource cts = new();
        TaskCompletionSource<bool> operationStarted = new();
        TaskCompletionSource<bool> cancellationRequested = new();

        _mockHandler.HandleAsync(Arg.Any<GetAllUsersConsumptionStatisticsQuery>(), Arg.Any<CancellationToken>())
            .Returns(callInfo => Task.Run(async () =>
            {
                operationStarted.SetResult(true);
                await cancellationRequested.Task;
                callInfo.Arg<CancellationToken>().ThrowIfCancellationRequested();
                return Result.From(_allUsersConsumptionStatisticsResponseFixture.Create());
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
