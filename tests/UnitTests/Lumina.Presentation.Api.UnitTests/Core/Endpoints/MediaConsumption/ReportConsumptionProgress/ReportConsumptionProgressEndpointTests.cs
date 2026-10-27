#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.CQRS;
using Lumina.Application.Core.MediaConsumption.Commands.ReportConsumptionProgress;
using Lumina.Contracts.Fixtures.Core.Requests.MediaConsumption;
using Lumina.Contracts.Fixtures.Core.Responses.MediaConsumption;
using Lumina.Contracts.Requests.MediaConsumption;
using Lumina.Contracts.Responses.MediaConsumption;
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.SharedKernel.Common.Enums.MediaConsumption;
using Lumina.Presentation.Api.Core.Endpoints.MediaConsumption.ReportConsumptionProgress;
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

namespace Lumina.Presentation.Api.UnitTests.Core.Endpoints.MediaConsumption.ReportConsumptionProgress;

/// <summary>
/// Contains unit tests for the <see cref="ReportConsumptionProgressEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class ReportConsumptionProgressEndpointTests
{
    private readonly ICommandHandler<ReportConsumptionProgressCommand, Result<ConsumptionRecordResponse>> _mockHandler;
    private readonly ReportConsumptionProgressEndpoint _sut;
    private readonly ReportConsumptionProgressRequestFixture _reportConsumptionProgressRequestFixture = new();
    private readonly MediaPositionRequestFixture _mediaPositionRequestFixture = new();
    private readonly ConsumptionRecordResponseFixture _consumptionRecordResponseFixture = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="ReportConsumptionProgressEndpointTests"/> class.
    /// </summary>
    public ReportConsumptionProgressEndpointTests()
    {
        _mockHandler = Substitute.For<ICommandHandler<ReportConsumptionProgressCommand, Result<ConsumptionRecordResponse>>>();
        _sut = FastEndpoints.Factory.Create<ReportConsumptionProgressEndpoint>(_mockHandler);
    }

    [Fact]
    public async Task ExecuteAsync_WhenSuccessful_ShouldReturnOkResultWithConsumptionRecordResponse()
    {
        // Arrange
        Guid recordId = Guid.NewGuid();
        CancellationToken cancellationToken = CancellationToken.None;
        ReportConsumptionProgressRequest request = _reportConsumptionProgressRequestFixture.Create();
        ConsumptionRecordResponse expectedResponse = _consumptionRecordResponseFixture.Create();
        _mockHandler.HandleAsync(Arg.Any<ReportConsumptionProgressCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.From(expectedResponse));
        _sut.HttpContext.Request.RouteValues["recordId"] = recordId.ToString();

        // Act
        IResult result = await _sut.ExecuteAsync(request, cancellationToken);

        // Assert
        Ok<ConsumptionRecordResponse> okResult = Assert.IsType<Ok<ConsumptionRecordResponse>>(result);
        Assert.Equal(expectedResponse, okResult.Value);
    }

    [Fact]
    public async Task ExecuteAsync_WhenSuccessful_ShouldSetPrivateNoStoreCacheHeaders()
    {
        // Arrange
        ReportConsumptionProgressRequest request = _reportConsumptionProgressRequestFixture.Create();
        _mockHandler.HandleAsync(Arg.Any<ReportConsumptionProgressCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.From(_consumptionRecordResponseFixture.Create()));

        // Act
        await _sut.ExecuteAsync(request, CancellationToken.None);

        // Assert
        Assert.Equal("private, no-store", _sut.HttpContext.Response.Headers["Cache-Control"].ToString());
        Assert.Contains("Authorization", _sut.HttpContext.Response.Headers["Vary"].ToString());
    }

    [Fact]
    public async Task ExecuteAsync_WhenHandlerReturnsError_ShouldStillSetPrivateNoStoreCacheHeaders()
    {
        // Arrange
        ReportConsumptionProgressRequest request = _reportConsumptionProgressRequestFixture.Create();
        Error expectedError = DomainErrors.MediaConsumption.ConsumptionRecordNotFound;
        _mockHandler.HandleAsync(Arg.Any<ReportConsumptionProgressCommand>(), Arg.Any<CancellationToken>())
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
        CancellationToken cancellationToken = CancellationToken.None;
        ReportConsumptionProgressRequest request = _reportConsumptionProgressRequestFixture.Create();
        List<Error> expectedErrors = [DomainErrors.MediaConsumption.InvalidMediaPosition, DomainErrors.MediaConsumption.InvalidProgressFraction];
        _mockHandler.HandleAsync(Arg.Any<ReportConsumptionProgressCommand>(), Arg.Any<CancellationToken>())
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
        Assert.Equal(new[] { "InvalidMediaPosition", "InvalidProgressFraction" }, singleErrorGroup.Value);
    }

    [Fact]
    public async Task ExecuteAsync_WhenHandlerReturnsNotAuthorizedError_ShouldReturnForbiddenProblemResult()
    {
        // Arrange
        CancellationToken cancellationToken = CancellationToken.None;
        ReportConsumptionProgressRequest request = _reportConsumptionProgressRequestFixture.Create();
        Error expectedError = ApplicationErrors.Authorization.NotAuthorized;
        _mockHandler.HandleAsync(Arg.Any<ReportConsumptionProgressCommand>(), Arg.Any<CancellationToken>())
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
        Assert.NotNull(problemDetails.Extensions["traceId"]);
    }

    [Fact]
    public async Task ExecuteAsync_WhenHandlerReturnsNotFoundError_ShouldReturnNotFoundProblemResult()
    {
        // Arrange
        CancellationToken cancellationToken = CancellationToken.None;
        ReportConsumptionProgressRequest request = _reportConsumptionProgressRequestFixture.Create();
        Error expectedError = DomainErrors.MediaConsumption.ConsumptionRecordNotFound;
        _mockHandler.HandleAsync(Arg.Any<ReportConsumptionProgressCommand>(), Arg.Any<CancellationToken>())
            .Returns(expectedError);

        // Act
        IResult result = await _sut.ExecuteAsync(request, cancellationToken);

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
    public async Task ExecuteAsync_WhenHandlerReturnsConflictError_ShouldReturnConflictProblemResult()
    {
        // Arrange
        CancellationToken cancellationToken = CancellationToken.None;
        ReportConsumptionProgressRequest request = _reportConsumptionProgressRequestFixture.Create();
        Error expectedError = DomainErrors.MediaConsumption.ConsumptionIsNotInProgress;
        _mockHandler.HandleAsync(Arg.Any<ReportConsumptionProgressCommand>(), Arg.Any<CancellationToken>())
            .Returns(expectedError);

        // Act
        IResult result = await _sut.ExecuteAsync(request, cancellationToken);

        // Assert
        ProblemHttpResult problemResult = Assert.IsType<ProblemHttpResult>(result);
        Assert.Equal(StatusCodes.Status409Conflict, problemResult.StatusCode);
        Assert.Equal("application/problem+json", problemResult.ContentType);
        Microsoft.AspNetCore.Mvc.ProblemDetails problemDetails = Assert.IsType<Microsoft.AspNetCore.Mvc.ProblemDetails>(problemResult.ProblemDetails);
        Assert.Equal(StatusCodes.Status409Conflict, problemDetails.Status);
        Assert.Equal("General.Conflict", problemDetails.Title);
        Assert.Equal("ConsumptionIsNotInProgress", problemDetails.Detail);
        Assert.NotNull(problemDetails.Extensions["traceId"]);
    }

    [Fact]
    public async Task ExecuteAsync_WhenPositionIsATimeOffset_ShouldSendCommandWithTimeOffsetPosition()
    {
        // Arrange
        Guid recordId = Guid.NewGuid();
        CancellationToken cancellationToken = CancellationToken.None;
        MediaPositionRequest position = _mediaPositionRequestFixture.Create(
            kind: nameof(MediaPositionKind.TimeOffset),
            timeOffsetSeconds: 1234.5);
        ReportConsumptionProgressRequest request = _reportConsumptionProgressRequestFixture.Create(
            position: position,
            progressFraction: 0.25,
            consumedDurationInSeconds: 300,
            consecutivePagesRead: 4);
        _mockHandler.HandleAsync(Arg.Any<ReportConsumptionProgressCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.From(_consumptionRecordResponseFixture.Create()));
        _sut.HttpContext.Request.RouteValues["recordId"] = recordId.ToString();

        // Act
        await _sut.ExecuteAsync(request, cancellationToken);

        // Assert
        // The request carries no record Id, so the command takes the identifier from the route only.
        await _mockHandler.Received(1).HandleAsync(
            Arg.Is<ReportConsumptionProgressCommand>(command =>
                command.ConsumptionRecordId == recordId.ToString() &&
                command.PositionKind == nameof(MediaPositionKind.TimeOffset) &&
                command.PositionTimeOffsetInSeconds == 1234.5 &&
                command.PositionLocation == null &&
                command.ProgressFraction == 0.25 &&
                command.ConsumedDurationInSeconds == 300 &&
                command.ConsecutivePagesRead == 4),
            Arg.Is(cancellationToken));
    }

    [Fact]
    public async Task ExecuteAsync_WhenPositionIsALocation_ShouldSendCommandWithLocationPosition()
    {
        // Arrange
        Guid recordId = Guid.NewGuid();
        CancellationToken cancellationToken = CancellationToken.None;
        MediaPositionRequest position = _mediaPositionRequestFixture.Create(
            kind: nameof(MediaPositionKind.Location),
            location: "chapter-3",
            includeTimeOffsetSeconds: false);
        ReportConsumptionProgressRequest request = _reportConsumptionProgressRequestFixture.Create(
            position: position,
            progressFraction: 0.75,
            consumedDurationInSeconds: 600,
            consecutivePagesRead: 12);
        _mockHandler.HandleAsync(Arg.Any<ReportConsumptionProgressCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.From(_consumptionRecordResponseFixture.Create()));
        _sut.HttpContext.Request.RouteValues["recordId"] = recordId.ToString();

        // Act
        await _sut.ExecuteAsync(request, cancellationToken);

        // Assert
        await _mockHandler.Received(1).HandleAsync(
            Arg.Is<ReportConsumptionProgressCommand>(command =>
                command.ConsumptionRecordId == recordId.ToString() &&
                command.PositionKind == nameof(MediaPositionKind.Location) &&
                command.PositionTimeOffsetInSeconds == null &&
                command.PositionLocation == "chapter-3" &&
                command.ProgressFraction == 0.75 &&
                command.ConsumedDurationInSeconds == 600 &&
                command.ConsecutivePagesRead == 12),
            Arg.Is(cancellationToken));
    }

    [Fact]
    public async Task ExecuteAsync_WhenPositionIsNotSupplied_ShouldSendCommandWithNullPositionAndDefaultConsecutivePagesRead()
    {
        // Arrange
        CancellationToken cancellationToken = CancellationToken.None;
        ReportConsumptionProgressRequest request = _reportConsumptionProgressRequestFixture.Create(
            includePosition: false,
            includeConsecutivePagesRead: false,
            progressFraction: 0.5,
            consumedDurationInSeconds: 120);
        _mockHandler.HandleAsync(Arg.Any<ReportConsumptionProgressCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.From(_consumptionRecordResponseFixture.Create()));

        // Act
        await _sut.ExecuteAsync(request, cancellationToken);

        // Assert
        await _mockHandler.Received(1).HandleAsync(
            Arg.Is<ReportConsumptionProgressCommand>(command =>
                command.PositionKind == null &&
                command.PositionTimeOffsetInSeconds == null &&
                command.PositionLocation == null &&
                command.ProgressFraction == 0.5 &&
                command.ConsumedDurationInSeconds == 120 &&
                command.ConsecutivePagesRead == 0),
            Arg.Is(cancellationToken));
    }

    [Fact]
    public async Task ExecuteAsync_WhenRouteValueIsMissing_ShouldSendCommandWithNullRecordId()
    {
        // Arrange
        CancellationToken cancellationToken = CancellationToken.None;
        ReportConsumptionProgressRequest request = _reportConsumptionProgressRequestFixture.Create(
            progressFraction: 0.5,
            consumedDurationInSeconds: 120);
        _mockHandler.HandleAsync(Arg.Any<ReportConsumptionProgressCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.From(_consumptionRecordResponseFixture.Create()));
        _sut.HttpContext.Request.RouteValues.Remove("recordId");

        // Act
        await _sut.ExecuteAsync(request, cancellationToken);

        // Assert
        await _mockHandler.Received(1).HandleAsync(
            Arg.Is<ReportConsumptionProgressCommand>(command => command.ConsumptionRecordId == null),
            Arg.Is(cancellationToken));
    }

    [Fact]
    public async Task ExecuteAsync_WhenCancellationRequested_ShouldCancelOperation()
    {
        // Arrange
        CancellationTokenSource cts = new();
        TaskCompletionSource<bool> operationStarted = new();
        TaskCompletionSource<bool> cancellationRequested = new();

        _mockHandler.HandleAsync(Arg.Any<ReportConsumptionProgressCommand>(), Arg.Any<CancellationToken>())
            .Returns(callInfo => Task.Run(async () =>
            {
                operationStarted.SetResult(true);
                await cancellationRequested.Task;
                callInfo.Arg<CancellationToken>().ThrowIfCancellationRequested();
                return Result.From(_consumptionRecordResponseFixture.Create());
            }, callInfo.Arg<CancellationToken>()));
        _sut.HttpContext.Request.RouteValues["recordId"] = Guid.NewGuid().ToString();

        // Act
        Task<IResult> operationTask = _sut.ExecuteAsync(_reportConsumptionProgressRequestFixture.Create(), cts.Token);
        await operationStarted.Task;
        cts.Cancel();
        cancellationRequested.SetResult(true);

        // Assert
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => operationTask);
    }
}
