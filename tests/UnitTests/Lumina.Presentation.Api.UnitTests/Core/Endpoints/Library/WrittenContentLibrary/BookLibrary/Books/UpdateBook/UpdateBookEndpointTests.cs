#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Application.Core.MediaLibrary.WrittenContentLibrary.BookLibrary.Books.Commands.UpdateBook;
using Lumina.Contracts.Fixtures.Core.Requests.MediaLibrary.WrittenContentLibrary.BookLibrary.Books;
using Lumina.Contracts.Fixtures.Core.Responses.MediaLibrary.WrittenContentLibrary.BookLibrary.Books;
using Lumina.Contracts.Requests.MediaLibrary.WrittenContentLibrary.BookLibrary.Books;
using Lumina.Contracts.Responses.MediaLibrary.WrittenContentLibrary.BookLibrary.Books;
using Lumina.Domain.Common.Primitives;
using Lumina.Presentation.Api.Core.Endpoints.Library.WrittenContentLibrary.BookLibrary.Books.UpdateBook;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using NSubstitute;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Api.UnitTests.Core.Endpoints.Library.WrittenContentLibrary.BookLibrary.Books.UpdateBook;

/// <summary>
/// Contains unit tests for the <see cref="UpdateBookEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class UpdateBookEndpointTests
{
    private readonly Application.Common.CQRS.ICommandHandler<UpdateBookCommand, Result<BookResponse>> _mockHandler;
    private readonly UpdateBookEndpoint _sut;
    private readonly UpdateBookRequestFixture _updateBookRequestFixture = new();
    private readonly BookResponseFixture _bookResponseFixture = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="UpdateBookEndpointTests"/> class.
    /// </summary>
    public UpdateBookEndpointTests()
    {
        _mockHandler = Substitute.For<Application.Common.CQRS.ICommandHandler<UpdateBookCommand, Result<BookResponse>>>();
        _sut = Factory.Create<UpdateBookEndpoint>(_mockHandler);
    }

    [Fact]
    public async Task ExecuteAsync_WhenSuccessful_ShouldReturnOkResultWithBookResponse()
    {
        // Arrange
        Guid routeId = Guid.NewGuid();
        UpdateBookRequest request = _updateBookRequestFixture.Create(id: routeId);
        CancellationToken cancellationToken = CancellationToken.None;
        BookResponse expectedResponse = _bookResponseFixture.Create();
        _mockHandler.HandleAsync(Arg.Any<UpdateBookCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.From(expectedResponse));
        ConfigureRequest(routeId.ToString());

        // Act
        IResult result = await _sut.ExecuteAsync(request, cancellationToken);

        // Assert
        Ok<BookResponse> okResult = Assert.IsType<Ok<BookResponse>>(result);
        Assert.Equal(expectedResponse, okResult.Value);
    }

    [Fact]
    public async Task ExecuteAsync_WhenHandlerReturnsNotFoundError_ShouldReturnProblemResult()
    {
        // Arrange
        Guid routeId = Guid.NewGuid();
        UpdateBookRequest request = _updateBookRequestFixture.Create(id: routeId);
        CancellationToken cancellationToken = CancellationToken.None;
        Error expectedError = Error.NotFound("Book.NotFound", "BookNotFound");
        _mockHandler.HandleAsync(Arg.Any<UpdateBookCommand>(), Arg.Any<CancellationToken>())
            .Returns(expectedError);
        ConfigureRequest(routeId.ToString());

        // Act
        IResult result = await _sut.ExecuteAsync(request, cancellationToken);

        // Assert
        ProblemHttpResult problemDetails = Assert.IsType<ProblemHttpResult>(result);
        Assert.Equal(StatusCodes.Status404NotFound, problemDetails.StatusCode);
        Assert.Equal("application/problem+json", problemDetails.ContentType);
        Microsoft.AspNetCore.Mvc.ProblemDetails problemDetailsBody = Assert.IsType<Microsoft.AspNetCore.Mvc.ProblemDetails>(problemDetails.ProblemDetails);
        Assert.Equal(StatusCodes.Status404NotFound, problemDetailsBody.Status);
        Assert.Equal("Book.NotFound", problemDetailsBody.Title);
        Assert.Equal("BookNotFound", problemDetailsBody.Detail);
        Assert.NotNull(problemDetailsBody.Extensions["traceId"]);
    }

    [Fact]
    public async Task ExecuteAsync_WhenHandlerReturnsValidationErrors_ShouldReturnValidationProblemResult()
    {
        // Arrange
        Guid routeId = Guid.NewGuid();
        UpdateBookRequest request = _updateBookRequestFixture.Create(id: routeId);
        CancellationToken cancellationToken = CancellationToken.None;
        Error validationError = Error.Validation(description: "BookIdCannotBeEmpty");
        _mockHandler.HandleAsync(Arg.Any<UpdateBookCommand>(), Arg.Any<CancellationToken>())
            .Returns(validationError);
        ConfigureRequest(routeId.ToString());

        // Act
        IResult result = await _sut.ExecuteAsync(request, cancellationToken);

        // Assert
        ProblemHttpResult problemDetails = Assert.IsType<ProblemHttpResult>(result);
        Assert.Equal(StatusCodes.Status422UnprocessableEntity, problemDetails.StatusCode);
        Assert.Equal("application/problem+json", problemDetails.ContentType);
        HttpValidationProblemDetails validationProblemDetails = Assert.IsType<HttpValidationProblemDetails>(problemDetails.ProblemDetails);
        Assert.Equal(StatusCodes.Status422UnprocessableEntity, validationProblemDetails.Status);
        Assert.Equal("General.Validation", validationProblemDetails.Title);
        Assert.Equal("https://tools.ietf.org/html/rfc4918#section-11.2", validationProblemDetails.Type);
        Assert.Single(validationProblemDetails.Errors);
        Assert.Equal(new[] { "BookIdCannotBeEmpty" }, validationProblemDetails.Errors["General.Validation"]);
    }

    [Fact]
    public async Task ExecuteAsync_WhenCalledWithMatchingRouteId_ShouldSendUpdateBookCommandToHandler()
    {
        // Arrange
        Guid routeId = Guid.NewGuid();
        UpdateBookRequest request = _updateBookRequestFixture.Create(id: routeId);
        CancellationToken cancellationToken = CancellationToken.None;
        _mockHandler.HandleAsync(Arg.Any<UpdateBookCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.From(_bookResponseFixture.Create()));
        ConfigureRequest(routeId.ToString());

        // Act
        await _sut.ExecuteAsync(request, cancellationToken);

        // Assert
        await _mockHandler.Received(1).HandleAsync(
            Arg.Is<UpdateBookCommand>(command =>
                command.Id == routeId &&
                command.Metadata == request.Metadata &&
                command.Format == request.Format &&
                command.Edition == request.Edition &&
                command.ASIN == request.ASIN &&
                command.ISBNs == request.ISBNs &&
                command.Contributors == request.Contributors &&
                command.Ratings == request.Ratings),
            Arg.Is(cancellationToken));
    }

    [Fact]
    public async Task ExecuteAsync_WhenRouteIdDiffersFromBodyId_ShouldSendCommandWithRouteId()
    {
        // Arrange
        Guid routeId = Guid.NewGuid();
        Guid bodyId = Guid.NewGuid();
        UpdateBookRequest request = _updateBookRequestFixture.Create(id: bodyId);
        CancellationToken cancellationToken = CancellationToken.None;
        _mockHandler.HandleAsync(Arg.Any<UpdateBookCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.From(_bookResponseFixture.Create()));
        ConfigureRequest(routeId.ToString());

        // Act
        await _sut.ExecuteAsync(request, cancellationToken);

        // Assert
        await _mockHandler.Received(1).HandleAsync(
            Arg.Is<UpdateBookCommand>(command => command.Id == routeId && command.Id != bodyId),
            Arg.Is(cancellationToken));
    }

    [Fact]
    public async Task ExecuteAsync_WhenRouteIdIsNotParseable_ShouldSendCommandWithEmptyBookId()
    {
        // Arrange
        UpdateBookRequest request = _updateBookRequestFixture.Create();
        CancellationToken cancellationToken = CancellationToken.None;
        _mockHandler.HandleAsync(Arg.Any<UpdateBookCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.From(_bookResponseFixture.Create()));
        ConfigureRequest("not-a-guid");

        // Act
        await _sut.ExecuteAsync(request, cancellationToken);

        // Assert
        await _mockHandler.Received(1).HandleAsync(
            Arg.Is<UpdateBookCommand>(command => command.Id == Guid.Empty),
            Arg.Is(cancellationToken));
    }

    [Fact]
    public async Task ExecuteAsync_WhenRouteIdIsEmpty_ShouldSendCommandWithEmptyBookId()
    {
        // Arrange
        UpdateBookRequest request = _updateBookRequestFixture.Create();
        CancellationToken cancellationToken = CancellationToken.None;
        _mockHandler.HandleAsync(Arg.Any<UpdateBookCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.From(_bookResponseFixture.Create()));
        ConfigureRequest(string.Empty);

        // Act
        await _sut.ExecuteAsync(request, cancellationToken);

        // Assert
        await _mockHandler.Received(1).HandleAsync(
            Arg.Is<UpdateBookCommand>(command => command.Id == Guid.Empty),
            Arg.Is(cancellationToken));
    }

    [Fact]
    public async Task ExecuteAsync_WhenRouteIdIsMissing_ShouldSendCommandWithEmptyBookId()
    {
        // Arrange
        UpdateBookRequest request = _updateBookRequestFixture.Create();
        CancellationToken cancellationToken = CancellationToken.None;
        _mockHandler.HandleAsync(Arg.Any<UpdateBookCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.From(_bookResponseFixture.Create()));
        ConfigureRequestWithoutId();

        // Act
        await _sut.ExecuteAsync(request, cancellationToken);

        // Assert
        await _mockHandler.Received(1).HandleAsync(
            Arg.Is<UpdateBookCommand>(command => command.Id == Guid.Empty),
            Arg.Is(cancellationToken));
    }

    [Fact]
    public async Task ExecuteAsync_WhenCancellationRequested_ShouldCancelOperation()
    {
        // Arrange
        Guid routeId = Guid.NewGuid();
        UpdateBookRequest request = _updateBookRequestFixture.Create(id: routeId);
        CancellationTokenSource cts = new();
        TaskCompletionSource<bool> operationStarted = new();
        TaskCompletionSource<bool> cancellationRequested = new();

        _mockHandler.HandleAsync(Arg.Any<UpdateBookCommand>(), Arg.Any<CancellationToken>())
            .Returns(callInfo => Task.Run(async () =>
            {
                operationStarted.SetResult(true);
                await cancellationRequested.Task;
                callInfo.Arg<CancellationToken>().ThrowIfCancellationRequested();
                return Result.From(_bookResponseFixture.Create());
            }, callInfo.Arg<CancellationToken>()));
        ConfigureRequest(routeId.ToString());

        // Act
        Task<IResult> operationTask = _sut.ExecuteAsync(request, cts.Token);
        await operationStarted.Task;
        cts.Cancel();
        cancellationRequested.SetResult(true);

        // Assert
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => operationTask);
    }

    /// <summary>
    /// Sets the <c>id</c> route value the endpoint reads the book Id from.
    /// </summary>
    /// <param name="routeId">The raw route value of the book Id.</param>
    private void ConfigureRequest(string routeId)
    {
        _sut.HttpContext.Request.RouteValues["id"] = routeId;
    }

    /// <summary>
    /// Leaves the <c>id</c> route value unset.
    /// </summary>
    private void ConfigureRequestWithoutId()
    {
        _sut.HttpContext.Request.RouteValues.Remove("id");
    }
}
