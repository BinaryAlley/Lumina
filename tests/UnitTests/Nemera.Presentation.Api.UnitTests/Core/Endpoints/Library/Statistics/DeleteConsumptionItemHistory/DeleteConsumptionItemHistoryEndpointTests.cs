#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.CQRS;
using Lumina.Application.Core.MediaConsumption.Commands.DeleteConsumptionItemHistory;
using Lumina.Contracts.Fixtures.Core.Requests.MediaConsumption;
using Lumina.Contracts.Requests.MediaConsumption;
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.SharedKernel.Common.Enums.MediaLibrary;
using Lumina.Presentation.Api.Core.Endpoints.Library.Statistics.DeleteConsumptionItemHistory;
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

namespace Lumina.Presentation.Api.UnitTests.Core.Endpoints.Library.Statistics.DeleteConsumptionItemHistory;

/// <summary>
/// Contains unit tests for the <see cref="DeleteConsumptionItemHistoryEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class DeleteConsumptionItemHistoryEndpointTests
{
    private readonly ICommandHandler<DeleteConsumptionItemHistoryCommand, Result<Deleted>> _mockHandler;
    private readonly DeleteConsumptionItemHistoryEndpoint _sut;
    private readonly DeleteConsumptionItemHistoryRequestFixture _deleteConsumptionItemHistoryRequestFixture = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="DeleteConsumptionItemHistoryEndpointTests"/> class.
    /// </summary>
    public DeleteConsumptionItemHistoryEndpointTests()
    {
        _mockHandler = Substitute.For<ICommandHandler<DeleteConsumptionItemHistoryCommand, Result<Deleted>>>();
        _sut = FastEndpoints.Factory.Create<DeleteConsumptionItemHistoryEndpoint>(_mockHandler);
    }

    [Fact]
    public async Task ExecuteAsync_WhenSuccessful_ShouldReturnOkResultWithDeleted()
    {
        // Arrange
        DeleteConsumptionItemHistoryRequest request = _deleteConsumptionItemHistoryRequestFixture.Create();
        CancellationToken cancellationToken = CancellationToken.None;
        _mockHandler.HandleAsync(Arg.Any<DeleteConsumptionItemHistoryCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.From(Result.Deleted));

        // Act
        IResult result = await _sut.ExecuteAsync(request, cancellationToken);

        // Assert
        Ok<Deleted> okResult = Assert.IsType<Ok<Deleted>>(result);
        Assert.Equal(Result.Deleted, okResult.Value);
    }

    [Fact]
    public async Task ExecuteAsync_WhenSuccessful_ShouldSetPrivateNoStoreCacheHeaders()
    {
        // Arrange
        DeleteConsumptionItemHistoryRequest request = _deleteConsumptionItemHistoryRequestFixture.Create();
        CancellationToken cancellationToken = CancellationToken.None;
        _mockHandler.HandleAsync(Arg.Any<DeleteConsumptionItemHistoryCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.From(Result.Deleted));

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
        DeleteConsumptionItemHistoryRequest request = _deleteConsumptionItemHistoryRequestFixture.Create();
        Error expectedError = ApplicationErrors.Authorization.NotAuthorized;
        _mockHandler.HandleAsync(Arg.Any<DeleteConsumptionItemHistoryCommand>(), Arg.Any<CancellationToken>())
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
        DeleteConsumptionItemHistoryRequest request = _deleteConsumptionItemHistoryRequestFixture.Create(
            includeMediaKind: false,
            includeMediaItemId: false,
            includeLibraryId: false);
        CancellationToken cancellationToken = CancellationToken.None;
        List<Error> expectedErrors =
        [
            DomainErrors.MediaConsumption.UnsupportedMediaKind,
            DomainErrors.MediaConsumption.MediaItemIdCannotBeEmpty,
            DomainErrors.Library.LibraryIdCannotBeEmpty
        ];
        _mockHandler.HandleAsync(Arg.Any<DeleteConsumptionItemHistoryCommand>(), Arg.Any<CancellationToken>())
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
        Assert.Equal(new[] { "UnsupportedMediaKind", "MediaItemIdCannotBeEmpty", "LibraryIdCannotBeEmpty" }, singleErrorGroup.Value);
    }

    [Fact]
    public async Task ExecuteAsync_WhenHandlerReturnsNotAuthorizedError_ShouldReturnForbiddenProblemResult()
    {
        // Arrange
        DeleteConsumptionItemHistoryRequest request = _deleteConsumptionItemHistoryRequestFixture.Create();
        CancellationToken cancellationToken = CancellationToken.None;
        Error expectedError = ApplicationErrors.Authorization.NotAuthorized;
        _mockHandler.HandleAsync(Arg.Any<DeleteConsumptionItemHistoryCommand>(), Arg.Any<CancellationToken>())
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
    public async Task ExecuteAsync_WhenHandlerReturnsNotFoundError_ShouldReturnNotFoundProblemResult()
    {
        // Arrange
        DeleteConsumptionItemHistoryRequest request = _deleteConsumptionItemHistoryRequestFixture.Create();
        CancellationToken cancellationToken = CancellationToken.None;
        Error expectedError = DomainErrors.MediaConsumption.ConsumptionRecordNotFound;
        _mockHandler.HandleAsync(Arg.Any<DeleteConsumptionItemHistoryCommand>(), Arg.Any<CancellationToken>())
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
        Assert.Equal("https://tools.ietf.org/html/rfc9110#section-15.5.5", problemDetails.Type);
        Assert.NotNull(problemDetails.Extensions["traceId"]);
    }

    [Fact]
    public async Task ExecuteAsync_WhenHandlerReturnsUnexpectedError_ShouldReturnInternalServerErrorProblemResult()
    {
        // Arrange
        DeleteConsumptionItemHistoryRequest request = _deleteConsumptionItemHistoryRequestFixture.Create();
        CancellationToken cancellationToken = CancellationToken.None;
        Error expectedError = Error.Internal("ConsumptionItemHistory.Failed", "An internal error has occurred.");
        _mockHandler.HandleAsync(Arg.Any<DeleteConsumptionItemHistoryCommand>(), Arg.Any<CancellationToken>())
            .Returns(expectedError);

        // Act
        IResult result = await _sut.ExecuteAsync(request, cancellationToken);

        // Assert
        ProblemHttpResult problemResult = Assert.IsType<ProblemHttpResult>(result);
        Assert.Equal(StatusCodes.Status500InternalServerError, problemResult.StatusCode);
        Assert.Equal("application/problem+json", problemResult.ContentType);
        Microsoft.AspNetCore.Mvc.ProblemDetails problemDetails = Assert.IsType<Microsoft.AspNetCore.Mvc.ProblemDetails>(problemResult.ProblemDetails);
        Assert.Equal(StatusCodes.Status500InternalServerError, problemDetails.Status);
        Assert.Equal("ConsumptionItemHistory.Failed", problemDetails.Title);
        Assert.Equal("An internal error has occurred.", problemDetails.Detail);
        Assert.Equal("https://tools.ietf.org/html/rfc9110#section-15.6.1", problemDetails.Type);
        Assert.NotNull(problemDetails.Extensions["traceId"]);
    }

    [Fact]
    public async Task ExecuteAsync_WhenCalled_ShouldSendDeleteConsumptionItemHistoryCommandWithMappedRequestAndNullTargetUserId()
    {
        // Arrange
        string mediaKind = nameof(MediaKind.Movie);
        string mediaItemId = Guid.NewGuid().ToString();
        string libraryId = Guid.NewGuid().ToString();
        DeleteConsumptionItemHistoryRequest request = _deleteConsumptionItemHistoryRequestFixture.Create(
            mediaKind: mediaKind,
            mediaItemId: mediaItemId,
            libraryId: libraryId);
        CancellationToken cancellationToken = CancellationToken.None;
        _mockHandler.HandleAsync(Arg.Any<DeleteConsumptionItemHistoryCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.From(Result.Deleted));

        // Act
        await _sut.ExecuteAsync(request, cancellationToken);

        // Assert
        await _mockHandler.Received(1).HandleAsync(
            Arg.Is<DeleteConsumptionItemHistoryCommand>(command =>
                command.TargetUserId == null &&
                command.MediaKind == mediaKind &&
                command.MediaItemId == mediaItemId &&
                command.LibraryId == libraryId),
            Arg.Is(cancellationToken));
    }

    [Fact]
    public async Task ExecuteAsync_WhenCancellationRequested_ShouldCancelOperation()
    {
        // Arrange
        DeleteConsumptionItemHistoryRequest request = _deleteConsumptionItemHistoryRequestFixture.Create();
        CancellationTokenSource cts = new();
        TaskCompletionSource<bool> operationStarted = new();
        TaskCompletionSource<bool> cancellationRequested = new();

        _mockHandler.HandleAsync(Arg.Any<DeleteConsumptionItemHistoryCommand>(), Arg.Any<CancellationToken>())
            .Returns(callInfo => Task.Run(async () =>
            {
                operationStarted.SetResult(true);
                await cancellationRequested.Task;
                callInfo.Arg<CancellationToken>().ThrowIfCancellationRequested();
                return Result.From(Result.Deleted);
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
