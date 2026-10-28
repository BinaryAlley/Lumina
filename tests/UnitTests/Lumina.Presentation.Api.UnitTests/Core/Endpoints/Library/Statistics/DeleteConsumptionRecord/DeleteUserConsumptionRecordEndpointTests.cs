#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.CQRS;
using Lumina.Application.Core.MediaConsumption.Commands.DeleteConsumptionRecord;
using Lumina.Domain.Common.Primitives;
using Lumina.Presentation.Api.Core.Endpoints.Library.Statistics.DeleteConsumptionRecord;
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

namespace Lumina.Presentation.Api.UnitTests.Core.Endpoints.Library.Statistics.DeleteConsumptionRecord;

/// <summary>
/// Contains unit tests for the <see cref="DeleteUserConsumptionRecordEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class DeleteUserConsumptionRecordEndpointTests
{
    private readonly ICommandHandler<DeleteConsumptionRecordCommand, Result<Deleted>> _mockHandler;
    private readonly DeleteUserConsumptionRecordEndpoint _sut;

    /// <summary>
    /// Initializes a new instance of the <see cref="DeleteUserConsumptionRecordEndpointTests"/> class.
    /// </summary>
    public DeleteUserConsumptionRecordEndpointTests()
    {
        _mockHandler = Substitute.For<ICommandHandler<DeleteConsumptionRecordCommand, Result<Deleted>>>();
        _sut = FastEndpoints.Factory.Create<DeleteUserConsumptionRecordEndpoint>(_mockHandler);
    }

    [Fact]
    public async Task ExecuteAsync_WhenSuccessful_ShouldReturnOkResultWithDeleted()
    {
        // Arrange
        CancellationToken cancellationToken = CancellationToken.None;
        _mockHandler.HandleAsync(Arg.Any<DeleteConsumptionRecordCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.From(Result.Deleted));
        _sut.HttpContext.Request.RouteValues["userId"] = Guid.NewGuid().ToString();
        _sut.HttpContext.Request.RouteValues["recordId"] = Guid.NewGuid().ToString();

        // Act
        IResult result = await _sut.ExecuteAsync(FastEndpoints.EmptyRequest.Instance, cancellationToken);

        // Assert
        Ok<Deleted> okResult = Assert.IsType<Ok<Deleted>>(result);
        Assert.Equal(Result.Deleted, okResult.Value);
    }

    [Fact]
    public async Task ExecuteAsync_WhenSuccessful_ShouldSetPrivateNoStoreCacheHeaders()
    {
        // Arrange
        CancellationToken cancellationToken = CancellationToken.None;
        _mockHandler.HandleAsync(Arg.Any<DeleteConsumptionRecordCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.From(Result.Deleted));
        _sut.HttpContext.Request.RouteValues["userId"] = Guid.NewGuid().ToString();
        _sut.HttpContext.Request.RouteValues["recordId"] = Guid.NewGuid().ToString();

        // Act
        await _sut.ExecuteAsync(FastEndpoints.EmptyRequest.Instance, cancellationToken);

        // Assert
        Assert.Equal("private, no-store", _sut.HttpContext.Response.Headers["Cache-Control"].ToString());
        Assert.Contains("Authorization", _sut.HttpContext.Response.Headers["Vary"].ToString());
    }

    [Fact]
    public async Task ExecuteAsync_WhenHandlerReturnsError_ShouldStillSetPrivateNoStoreCacheHeaders()
    {
        // Arrange
        Error expectedError = ApplicationErrors.Authorization.NotAuthorized;
        _mockHandler.HandleAsync(Arg.Any<DeleteConsumptionRecordCommand>(), Arg.Any<CancellationToken>())
            .Returns(expectedError);
        _sut.HttpContext.Request.RouteValues["userId"] = Guid.NewGuid().ToString();
        _sut.HttpContext.Request.RouteValues["recordId"] = Guid.NewGuid().ToString();

        // Act
        await _sut.ExecuteAsync(FastEndpoints.EmptyRequest.Instance, CancellationToken.None);

        // Assert
        Assert.Equal("private, no-store", _sut.HttpContext.Response.Headers["Cache-Control"].ToString());
        Assert.Contains("Authorization", _sut.HttpContext.Response.Headers["Vary"].ToString());
    }

    [Fact]
    public async Task ExecuteAsync_WhenHandlerReturnsValidationErrors_ShouldReturnValidationProblemResultWithErrorsGroupedByCode()
    {
        // Arrange
        CancellationToken cancellationToken = CancellationToken.None;
        List<Error> expectedErrors = [DomainErrors.MediaConsumption.ConsumptionRecordIdCannotBeEmpty, DomainErrors.MediaConsumption.TargetUserIdIsInvalid];
        _mockHandler.HandleAsync(Arg.Any<DeleteConsumptionRecordCommand>(), Arg.Any<CancellationToken>())
            .Returns(expectedErrors);
        _sut.HttpContext.Request.RouteValues["userId"] = "not-a-user-guid";
        _sut.HttpContext.Request.RouteValues.Remove("recordId");

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
        KeyValuePair<string, string[]> singleErrorGroup = Assert.Single(validationProblemDetails.Errors);
        Assert.Equal("General.Validation", singleErrorGroup.Key);
        Assert.Equal(new[] { "ConsumptionRecordIdCannotBeEmpty", "TargetUserIdIsInvalid" }, singleErrorGroup.Value);
    }

    [Fact]
    public async Task ExecuteAsync_WhenHandlerReturnsNotAuthorizedError_ShouldReturnForbiddenProblemResult()
    {
        // Arrange
        CancellationToken cancellationToken = CancellationToken.None;
        Error expectedError = ApplicationErrors.Authorization.NotAuthorized;
        _mockHandler.HandleAsync(Arg.Any<DeleteConsumptionRecordCommand>(), Arg.Any<CancellationToken>())
            .Returns(expectedError);
        _sut.HttpContext.Request.RouteValues["userId"] = Guid.NewGuid().ToString();
        _sut.HttpContext.Request.RouteValues["recordId"] = Guid.NewGuid().ToString();

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
        Assert.Equal("https://tools.ietf.org/html/rfc9110#section-15.5.4", problemDetails.Type);
        Assert.NotNull(problemDetails.Extensions["traceId"]);
    }

    [Fact]
    public async Task ExecuteAsync_WhenCalled_ShouldSendDeleteConsumptionRecordCommandWithRouteValues()
    {
        // Arrange
        Guid userId = Guid.NewGuid();
        Guid recordId = Guid.NewGuid();
        CancellationToken cancellationToken = CancellationToken.None;
        _mockHandler.HandleAsync(Arg.Any<DeleteConsumptionRecordCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.From(Result.Deleted));
        _sut.HttpContext.Request.RouteValues["userId"] = userId.ToString();
        _sut.HttpContext.Request.RouteValues["recordId"] = recordId.ToString();

        // Act
        await _sut.ExecuteAsync(FastEndpoints.EmptyRequest.Instance, cancellationToken);

        // Assert
        await _mockHandler.Received(1).HandleAsync(
            Arg.Is<DeleteConsumptionRecordCommand>(command =>
                command.ConsumptionRecordId == recordId.ToString() &&
                command.TargetUserId == userId.ToString()),
            Arg.Is(cancellationToken));
    }

    [Fact]
    public async Task ExecuteAsync_WhenRouteValuesAreMissing_ShouldSendCommandWithNullIdentifiers()
    {
        // Arrange
        CancellationToken cancellationToken = CancellationToken.None;
        _mockHandler.HandleAsync(Arg.Any<DeleteConsumptionRecordCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.From(Result.Deleted));
        _sut.HttpContext.Request.RouteValues.Remove("userId");
        _sut.HttpContext.Request.RouteValues.Remove("recordId");

        // Act
        await _sut.ExecuteAsync(FastEndpoints.EmptyRequest.Instance, cancellationToken);

        // Assert
        await _mockHandler.Received(1).HandleAsync(
            Arg.Is<DeleteConsumptionRecordCommand>(command =>
                command.ConsumptionRecordId == null &&
                command.TargetUserId == null),
            Arg.Is(cancellationToken));
    }

    [Fact]
    public async Task ExecuteAsync_WhenCancellationRequested_ShouldCancelOperation()
    {
        // Arrange
        CancellationTokenSource cts = new();
        TaskCompletionSource<bool> operationStarted = new();
        TaskCompletionSource<bool> cancellationRequested = new();

        _mockHandler.HandleAsync(Arg.Any<DeleteConsumptionRecordCommand>(), Arg.Any<CancellationToken>())
            .Returns(callInfo => Task.Run(async () =>
            {
                operationStarted.SetResult(true);
                await cancellationRequested.Task;
                callInfo.Arg<CancellationToken>().ThrowIfCancellationRequested();
                return Result.From(Result.Deleted);
            }, callInfo.Arg<CancellationToken>()));
        _sut.HttpContext.Request.RouteValues["userId"] = Guid.NewGuid().ToString();
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
