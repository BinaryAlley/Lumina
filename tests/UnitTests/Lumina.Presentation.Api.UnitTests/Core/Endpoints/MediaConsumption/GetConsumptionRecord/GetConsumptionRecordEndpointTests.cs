#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.CQRS;
using Lumina.Application.Core.MediaConsumption.Queries.GetConsumptionRecord;
using Lumina.Contracts.Fixtures.Core.Responses.MediaConsumption;
using Lumina.Contracts.Responses.MediaConsumption;
using Lumina.Domain.Common.Primitives;
using Lumina.Presentation.Api.Core.Endpoints.MediaConsumption.GetConsumptionRecord;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using NSubstitute;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using ApplicationErrors = Lumina.Application.Common.Errors.Errors;
using DomainErrors = Lumina.Domain.Common.Errors.Errors;
#endregion

namespace Lumina.Presentation.Api.UnitTests.Core.Endpoints.MediaConsumption.GetConsumptionRecord;

/// <summary>
/// Contains unit tests for the <see cref="GetConsumptionRecordEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetConsumptionRecordEndpointTests
{
    private readonly IQueryHandler<GetConsumptionRecordQuery, Result<ConsumptionRecordResponse>> _mockHandler;
    private readonly GetConsumptionRecordEndpoint _sut;
    private readonly ConsumptionRecordResponseFixture _consumptionRecordResponseFixture = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="GetConsumptionRecordEndpointTests"/> class.
    /// </summary>
    public GetConsumptionRecordEndpointTests()
    {
        _mockHandler = Substitute.For<IQueryHandler<GetConsumptionRecordQuery, Result<ConsumptionRecordResponse>>>();
        _sut = FastEndpoints.Factory.Create<GetConsumptionRecordEndpoint>(_mockHandler);
    }

    [Fact]
    public async Task ExecuteAsync_WhenSuccessful_ShouldReturnOkResultWithConsumptionRecordResponse()
    {
        // Arrange
        Guid recordId = Guid.NewGuid();
        CancellationToken cancellationToken = CancellationToken.None;
        ConsumptionRecordResponse expectedResponse = _consumptionRecordResponseFixture.Create();
        _mockHandler.HandleAsync(Arg.Any<GetConsumptionRecordQuery>(), Arg.Any<CancellationToken>())
            .Returns(Result.From(expectedResponse));
        _sut.HttpContext.Request.RouteValues["recordId"] = recordId.ToString();

        // Act
        IResult result = await _sut.ExecuteAsync(FastEndpoints.EmptyRequest.Instance, cancellationToken);

        // Assert
        Ok<ConsumptionRecordResponse> okResult = Assert.IsType<Ok<ConsumptionRecordResponse>>(result);
        Assert.Equal(expectedResponse, okResult.Value);
    }

    [Fact]
    public async Task ExecuteAsync_WhenSuccessful_ShouldSetPrivateNoStoreCacheHeaders()
    {
        // Arrange
        _mockHandler.HandleAsync(Arg.Any<GetConsumptionRecordQuery>(), Arg.Any<CancellationToken>())
            .Returns(Result.From(_consumptionRecordResponseFixture.Create()));

        // Act
        await _sut.ExecuteAsync(FastEndpoints.EmptyRequest.Instance, CancellationToken.None);

        // Assert
        Assert.Equal("private, no-store", _sut.HttpContext.Response.Headers["Cache-Control"].ToString());
        Assert.Contains("Authorization", _sut.HttpContext.Response.Headers["Vary"].ToString());
    }

    [Fact]
    public async Task ExecuteAsync_WhenHandlerReturnsError_ShouldStillSetPrivateNoStoreCacheHeaders()
    {
        // Arrange
        Error expectedError = DomainErrors.MediaConsumption.ConsumptionRecordNotFound;
        _mockHandler.HandleAsync(Arg.Any<GetConsumptionRecordQuery>(), Arg.Any<CancellationToken>())
            .Returns(expectedError);

        // Act
        await _sut.ExecuteAsync(FastEndpoints.EmptyRequest.Instance, CancellationToken.None);

        // Assert
        Assert.Equal("private, no-store", _sut.HttpContext.Response.Headers["Cache-Control"].ToString());
        Assert.Contains("Authorization", _sut.HttpContext.Response.Headers["Vary"].ToString());
    }

    [Fact]
    public async Task ExecuteAsync_WhenHandlerReturnsValidationError_ShouldReturnValidationProblemResult()
    {
        // Arrange
        CancellationToken cancellationToken = CancellationToken.None;
        Error expectedError = DomainErrors.MediaConsumption.ConsumptionRecordIdCannotBeEmpty;
        _mockHandler.HandleAsync(Arg.Any<GetConsumptionRecordQuery>(), Arg.Any<CancellationToken>())
            .Returns(expectedError);

        // Act
        IResult result = await _sut.ExecuteAsync(FastEndpoints.EmptyRequest.Instance, cancellationToken);

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
        Assert.Single(validationProblemDetails.Errors);
        Assert.Equal(new[] { "ConsumptionRecordIdCannotBeEmpty" }, validationProblemDetails.Errors["General.Validation"]);
    }

    [Fact]
    public async Task ExecuteAsync_WhenHandlerReturnsNotAuthorizedError_ShouldReturnForbiddenProblemResult()
    {
        // Arrange
        CancellationToken cancellationToken = CancellationToken.None;
        Error expectedError = ApplicationErrors.Authorization.NotAuthorized;
        _mockHandler.HandleAsync(Arg.Any<GetConsumptionRecordQuery>(), Arg.Any<CancellationToken>())
            .Returns(expectedError);

        // Act
        IResult result = await _sut.ExecuteAsync(FastEndpoints.EmptyRequest.Instance, cancellationToken);

        // Assert
        ProblemHttpResult problemResult = Assert.IsType<ProblemHttpResult>(result);
        Assert.Equal(StatusCodes.Status403Forbidden, problemResult.StatusCode);
        Assert.Equal("application/problem+json", problemResult.ContentType);
        Microsoft.AspNetCore.Mvc.ProblemDetails problemDetails = Assert.IsType<Microsoft.AspNetCore.Mvc.ProblemDetails>(problemResult.ProblemDetails);
        Assert.Equal(StatusCodes.Status403Forbidden, problemDetails.Status);
        Assert.Equal("General.Unauthorized", problemDetails.Title);
        Assert.Equal("NotAuthorized", problemDetails.Detail);
        Assert.NotNull(problemDetails.Extensions["traceId"]);
    }

    [Fact]
    public async Task ExecuteAsync_WhenHandlerReturnsNotFoundError_ShouldReturnNotFoundProblemResult()
    {
        // Arrange
        CancellationToken cancellationToken = CancellationToken.None;
        Error expectedError = DomainErrors.MediaConsumption.ConsumptionRecordNotFound;
        _mockHandler.HandleAsync(Arg.Any<GetConsumptionRecordQuery>(), Arg.Any<CancellationToken>())
            .Returns(expectedError);

        // Act
        IResult result = await _sut.ExecuteAsync(FastEndpoints.EmptyRequest.Instance, cancellationToken);

        // Assert
        ProblemHttpResult problemResult = Assert.IsType<ProblemHttpResult>(result);
        Assert.Equal(StatusCodes.Status404NotFound, problemResult.StatusCode);
        Assert.Equal("application/problem+json", problemResult.ContentType);
        Microsoft.AspNetCore.Mvc.ProblemDetails problemDetails = Assert.IsType<Microsoft.AspNetCore.Mvc.ProblemDetails>(problemResult.ProblemDetails);
        Assert.Equal(StatusCodes.Status404NotFound, problemDetails.Status);
        Assert.Equal("General.NotFound", problemDetails.Title);
        Assert.Equal("ConsumptionRecordNotFound", problemDetails.Detail);
        Assert.NotNull(problemDetails.Extensions["traceId"]);
    }

    [Fact]
    public async Task ExecuteAsync_WhenCalled_ShouldSendGetConsumptionRecordQueryToSender()
    {
        // Arrange
        Guid recordId = Guid.NewGuid();
        CancellationToken cancellationToken = CancellationToken.None;
        _mockHandler.HandleAsync(Arg.Any<GetConsumptionRecordQuery>(), Arg.Any<CancellationToken>())
            .Returns(Result.From(_consumptionRecordResponseFixture.Create()));
        _sut.HttpContext.Request.RouteValues["recordId"] = recordId.ToString();

        // Act
        await _sut.ExecuteAsync(FastEndpoints.EmptyRequest.Instance, cancellationToken);

        // Assert
        await _mockHandler.Received(1).HandleAsync(
            Arg.Is<GetConsumptionRecordQuery>(query => query.ConsumptionRecordId == recordId.ToString()),
            Arg.Is(cancellationToken));
    }

    [Fact]
    public async Task ExecuteAsync_WhenRouteValueIsMissing_ShouldSendQueryWithNullId()
    {
        // Arrange
        CancellationToken cancellationToken = CancellationToken.None;
        _mockHandler.HandleAsync(Arg.Any<GetConsumptionRecordQuery>(), Arg.Any<CancellationToken>())
            .Returns(Result.From(_consumptionRecordResponseFixture.Create()));
        _sut.HttpContext.Request.RouteValues.Remove("recordId");

        // Act
        await _sut.ExecuteAsync(FastEndpoints.EmptyRequest.Instance, cancellationToken);

        // Assert
        await _mockHandler.Received(1).HandleAsync(
            Arg.Is<GetConsumptionRecordQuery>(query => query.ConsumptionRecordId == null),
            Arg.Is(cancellationToken));
    }

    [Fact]
    public async Task ExecuteAsync_WhenCancellationRequested_ShouldCancelOperation()
    {
        // Arrange
        CancellationTokenSource cts = new();
        TaskCompletionSource<bool> operationStarted = new();
        TaskCompletionSource<bool> cancellationRequested = new();

        _mockHandler.HandleAsync(Arg.Any<GetConsumptionRecordQuery>(), Arg.Any<CancellationToken>())
            .Returns(callInfo => Task.Run(async () =>
            {
                operationStarted.SetResult(true);
                await cancellationRequested.Task;
                callInfo.Arg<CancellationToken>().ThrowIfCancellationRequested();
                return Result.From(_consumptionRecordResponseFixture.Create());
            }, callInfo.Arg<CancellationToken>()));
        _sut.HttpContext.Request.RouteValues["recordId"] = Guid.NewGuid().ToString();

        // Act
        Task<IResult> operationTask = _sut.ExecuteAsync(FastEndpoints.EmptyRequest.Instance, cts.Token);
        await operationStarted.Task;
        cts.Cancel();
        cancellationRequested.SetResult(true);

        // Assert
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => operationTask);
    }
}
