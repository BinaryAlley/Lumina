#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.CQRS;
using Lumina.Application.Core.MediaConsumption.Commands.StartConsumption;
using Lumina.Contracts.Fixtures.Core.Responses.MediaConsumption;
using Lumina.Contracts.Responses.MediaConsumption;
using Lumina.Domain.Common.Primitives;
using Lumina.Presentation.Api.Core.Endpoints.MediaConsumption.StartConsumption;
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

namespace Lumina.Presentation.Api.UnitTests.Core.Endpoints.MediaConsumption.StartConsumption;

/// <summary>
/// Contains unit tests for the <see cref="StartConsumptionEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class StartConsumptionEndpointTests
{
    private readonly ICommandHandler<StartConsumptionCommand, Result<ConsumptionRecordResponse>> _mockHandler;
    private readonly StartConsumptionEndpoint _sut;
    private readonly ConsumptionRecordResponseFixture _consumptionRecordResponseFixture = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="StartConsumptionEndpointTests"/> class.
    /// </summary>
    public StartConsumptionEndpointTests()
    {
        _mockHandler = Substitute.For<ICommandHandler<StartConsumptionCommand, Result<ConsumptionRecordResponse>>>();
        _sut = FastEndpoints.Factory.Create<StartConsumptionEndpoint>(_mockHandler);
    }

    [Fact]
    public async Task ExecuteAsync_WhenSuccessful_ShouldReturnOkResultWithConsumptionRecordResponse()
    {
        // Arrange
        Guid libraryId = Guid.NewGuid();
        Guid mediaItemId = Guid.NewGuid();
        CancellationToken cancellationToken = CancellationToken.None;
        ConsumptionRecordResponse expectedResponse = _consumptionRecordResponseFixture.Create();
        _mockHandler.HandleAsync(Arg.Any<StartConsumptionCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.From(expectedResponse));
        _sut.HttpContext.Request.RouteValues["libraryId"] = libraryId.ToString();
        _sut.HttpContext.Request.RouteValues["mediaKind"] = "MusicTrack";
        _sut.HttpContext.Request.RouteValues["mediaItemId"] = mediaItemId.ToString();

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
        CancellationToken cancellationToken = CancellationToken.None;
        _mockHandler.HandleAsync(Arg.Any<StartConsumptionCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.From(_consumptionRecordResponseFixture.Create()));

        // Act
        await _sut.ExecuteAsync(FastEndpoints.EmptyRequest.Instance, cancellationToken);

        // Assert
        Assert.Equal("private, no-store", _sut.HttpContext.Response.Headers["Cache-Control"].ToString());
        Assert.Contains("Authorization", _sut.HttpContext.Response.Headers["Vary"].ToString());
    }

    [Fact]
    public async Task ExecuteAsync_WhenHandlerReturnsValidationErrors_ShouldReturnValidationProblemResultWithErrorsGroupedByCode()
    {
        // Arrange
        CancellationToken cancellationToken = CancellationToken.None;
        List<Error> expectedErrors = [DomainErrors.Library.LibraryIdCannotBeEmpty, DomainErrors.MediaConsumption.MediaItemIdCannotBeEmpty];
        _mockHandler.HandleAsync(Arg.Any<StartConsumptionCommand>(), Arg.Any<CancellationToken>())
            .Returns(expectedErrors);

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
        Assert.Equal(new[] { "LibraryIdCannotBeEmpty", "MediaItemIdCannotBeEmpty" }, singleErrorGroup.Value);
    }

    [Fact]
    public async Task ExecuteAsync_WhenHandlerReturnsError_ShouldStillSetPrivateNoStoreCacheHeaders()
    {
        // Arrange
        Error expectedError = DomainErrors.MediaConsumption.ConsumptionRecordNotFound;
        _mockHandler.HandleAsync(Arg.Any<StartConsumptionCommand>(), Arg.Any<CancellationToken>())
            .Returns(expectedError);

        // Act
        await _sut.ExecuteAsync(FastEndpoints.EmptyRequest.Instance, CancellationToken.None);

        // Assert
        Assert.Equal("private, no-store", _sut.HttpContext.Response.Headers["Cache-Control"].ToString());
        Assert.Contains("Authorization", _sut.HttpContext.Response.Headers["Vary"].ToString());
    }

    [Fact]
    public async Task ExecuteAsync_WhenHandlerReturnsNotAuthorizedError_ShouldReturnForbiddenProblemResult()
    {
        // Arrange
        CancellationToken cancellationToken = CancellationToken.None;
        Error expectedError = ApplicationErrors.Authorization.NotAuthorized;
        _mockHandler.HandleAsync(Arg.Any<StartConsumptionCommand>(), Arg.Any<CancellationToken>())
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
    public async Task ExecuteAsync_WhenHandlerReturnsUnsupportedMediaKindError_ShouldReturnValidationProblemResult()
    {
        // Arrange
        CancellationToken cancellationToken = CancellationToken.None;
        Error expectedError = DomainErrors.MediaConsumption.UnsupportedMediaKind;
        _mockHandler.HandleAsync(Arg.Any<StartConsumptionCommand>(), Arg.Any<CancellationToken>())
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
        Assert.Single(validationProblemDetails.Errors);
        Assert.Equal(new[] { "UnsupportedMediaKind" }, validationProblemDetails.Errors["General.Validation"]);
    }

    [Fact]
    public async Task ExecuteAsync_WhenHandlerReturnsLibraryNotFoundError_ShouldReturnNotFoundProblemResult()
    {
        // Arrange
        CancellationToken cancellationToken = CancellationToken.None;
        Error expectedError = DomainErrors.Library.LibraryNotFound;
        _mockHandler.HandleAsync(Arg.Any<StartConsumptionCommand>(), Arg.Any<CancellationToken>())
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
        Assert.Equal("LibraryNotFound", problemDetails.Detail);
        Assert.NotNull(problemDetails.Extensions["traceId"]);
    }

    [Fact]
    public async Task ExecuteAsync_WhenHandlerReturnsTrackNotFoundError_ShouldReturnNotFoundProblemResult()
    {
        // Arrange
        CancellationToken cancellationToken = CancellationToken.None;
        Error expectedError = DomainErrors.Music.TrackNotFound;
        _mockHandler.HandleAsync(Arg.Any<StartConsumptionCommand>(), Arg.Any<CancellationToken>())
            .Returns(expectedError);

        // Act
        IResult result = await _sut.ExecuteAsync(FastEndpoints.EmptyRequest.Instance, cancellationToken);

        // Assert
        ProblemHttpResult problemResult = Assert.IsType<ProblemHttpResult>(result);
        Assert.Equal(StatusCodes.Status404NotFound, problemResult.StatusCode);
        Microsoft.AspNetCore.Mvc.ProblemDetails problemDetails = Assert.IsType<Microsoft.AspNetCore.Mvc.ProblemDetails>(problemResult.ProblemDetails);
        Assert.Equal(StatusCodes.Status404NotFound, problemDetails.Status);
        Assert.Equal("General.NotFound", problemDetails.Title);
        Assert.Equal("TrackNotFound", problemDetails.Detail);
        Assert.NotNull(problemDetails.Extensions["traceId"]);
    }

    [Fact]
    public async Task ExecuteAsync_WhenHandlerReturnsBookNotFoundError_ShouldReturnNotFoundProblemResult()
    {
        // Arrange
        CancellationToken cancellationToken = CancellationToken.None;
        Error expectedError = DomainErrors.WrittenContent.BookNotFound;
        _mockHandler.HandleAsync(Arg.Any<StartConsumptionCommand>(), Arg.Any<CancellationToken>())
            .Returns(expectedError);

        // Act
        IResult result = await _sut.ExecuteAsync(FastEndpoints.EmptyRequest.Instance, cancellationToken);

        // Assert
        ProblemHttpResult problemResult = Assert.IsType<ProblemHttpResult>(result);
        Assert.Equal(StatusCodes.Status404NotFound, problemResult.StatusCode);
        Microsoft.AspNetCore.Mvc.ProblemDetails problemDetails = Assert.IsType<Microsoft.AspNetCore.Mvc.ProblemDetails>(problemResult.ProblemDetails);
        Assert.Equal(StatusCodes.Status404NotFound, problemDetails.Status);
        Assert.Equal("General.NotFound", problemDetails.Title);
        Assert.Equal("BookNotFound", problemDetails.Detail);
        Assert.NotNull(problemDetails.Extensions["traceId"]);
    }

    [Fact]
    public async Task ExecuteAsync_WhenHandlerReturnsConcurrencyConflictError_ShouldReturnConflictProblemResult()
    {
        // Arrange
        CancellationToken cancellationToken = CancellationToken.None;
        Error expectedError = ApplicationErrors.Persistence.ConcurrencyConflict;
        _mockHandler.HandleAsync(Arg.Any<StartConsumptionCommand>(), Arg.Any<CancellationToken>())
            .Returns(expectedError);

        // Act
        IResult result = await _sut.ExecuteAsync(FastEndpoints.EmptyRequest.Instance, cancellationToken);

        // Assert
        ProblemHttpResult problemResult = Assert.IsType<ProblemHttpResult>(result);
        Assert.Equal(StatusCodes.Status409Conflict, problemResult.StatusCode);
        Assert.Equal("application/problem+json", problemResult.ContentType);
        Microsoft.AspNetCore.Mvc.ProblemDetails problemDetails = Assert.IsType<Microsoft.AspNetCore.Mvc.ProblemDetails>(problemResult.ProblemDetails);
        Assert.Equal(StatusCodes.Status409Conflict, problemDetails.Status);
        Assert.Equal("General.Conflict", problemDetails.Title);
        Assert.Equal("ConcurrencyConflict", problemDetails.Detail);
        Assert.NotNull(problemDetails.Extensions["traceId"]);
    }

    [Fact]
    public async Task ExecuteAsync_WhenCalled_ShouldSendStartConsumptionCommandToSender()
    {
        // Arrange
        Guid libraryId = Guid.NewGuid();
        Guid mediaItemId = Guid.NewGuid();
        CancellationToken cancellationToken = CancellationToken.None;
        _mockHandler.HandleAsync(Arg.Any<StartConsumptionCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.From(_consumptionRecordResponseFixture.Create()));
        _sut.HttpContext.Request.RouteValues["libraryId"] = libraryId.ToString();
        _sut.HttpContext.Request.RouteValues["mediaKind"] = "Book";
        _sut.HttpContext.Request.RouteValues["mediaItemId"] = mediaItemId.ToString();

        // Act
        await _sut.ExecuteAsync(FastEndpoints.EmptyRequest.Instance, cancellationToken);

        // Assert
        await _mockHandler.Received(1).HandleAsync(
            Arg.Is<StartConsumptionCommand>(command =>
                command.LibraryId == libraryId.ToString() &&
                command.MediaKind == "Book" &&
                command.MediaItemId == mediaItemId.ToString()),
            Arg.Is(cancellationToken));
    }

    [Fact]
    public async Task ExecuteAsync_WhenRouteValuesAreNotParseable_ShouldSendCommandWithRawRouteValues()
    {
        // Arrange
        CancellationToken cancellationToken = CancellationToken.None;
        _mockHandler.HandleAsync(Arg.Any<StartConsumptionCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.From(_consumptionRecordResponseFixture.Create()));
        _sut.HttpContext.Request.RouteValues["libraryId"] = "not-a-library-guid";
        _sut.HttpContext.Request.RouteValues["mediaKind"] = "not-a-media-kind";
        _sut.HttpContext.Request.RouteValues["mediaItemId"] = "not-a-guid";

        // Act
        await _sut.ExecuteAsync(FastEndpoints.EmptyRequest.Instance, cancellationToken);

        // Assert
        await _mockHandler.Received(1).HandleAsync(
            Arg.Is<StartConsumptionCommand>(command =>
                command.LibraryId == "not-a-library-guid" &&
                command.MediaKind == "not-a-media-kind" &&
                command.MediaItemId == "not-a-guid"),
            Arg.Is(cancellationToken));
    }

    [Fact]
    public async Task ExecuteAsync_WhenRouteValuesAreMissing_ShouldSendCommandWithNullRouteValues()
    {
        // Arrange
        CancellationToken cancellationToken = CancellationToken.None;
        _mockHandler.HandleAsync(Arg.Any<StartConsumptionCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.From(_consumptionRecordResponseFixture.Create()));
        _sut.HttpContext.Request.RouteValues.Remove("libraryId");
        _sut.HttpContext.Request.RouteValues.Remove("mediaKind");
        _sut.HttpContext.Request.RouteValues.Remove("mediaItemId");

        // Act
        await _sut.ExecuteAsync(FastEndpoints.EmptyRequest.Instance, cancellationToken);

        // Assert
        await _mockHandler.Received(1).HandleAsync(
            Arg.Is<StartConsumptionCommand>(command =>
                command.LibraryId == null &&
                command.MediaKind == null &&
                command.MediaItemId == null),
            Arg.Is(cancellationToken));
    }

    [Fact]
    public async Task ExecuteAsync_WhenCancellationRequested_ShouldCancelOperation()
    {
        // Arrange
        CancellationTokenSource cts = new();
        TaskCompletionSource<bool> operationStarted = new();
        TaskCompletionSource<bool> cancellationRequested = new();

        _mockHandler.HandleAsync(Arg.Any<StartConsumptionCommand>(), Arg.Any<CancellationToken>())
            .Returns(callInfo => Task.Run(async () =>
            {
                operationStarted.SetResult(true);
                await cancellationRequested.Task;
                callInfo.Arg<CancellationToken>().ThrowIfCancellationRequested();
                return Result.From(_consumptionRecordResponseFixture.Create());
            }, callInfo.Arg<CancellationToken>()));
        _sut.HttpContext.Request.RouteValues["libraryId"] = Guid.NewGuid().ToString();
        _sut.HttpContext.Request.RouteValues["mediaKind"] = "MusicTrack";
        _sut.HttpContext.Request.RouteValues["mediaItemId"] = Guid.NewGuid().ToString();

        // Act
        Task<IResult> operationTask = _sut.ExecuteAsync(FastEndpoints.EmptyRequest.Instance, cts.Token);
        await operationStarted.Task;
        cts.Cancel();
        cancellationRequested.SetResult(true);

        // Assert
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => operationTask);
    }
}
