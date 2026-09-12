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
    private readonly Lumina.Application.Common.CQRS.ICommandHandler<UpdateBookCommand, Result<BookResponse>> _mockHandler;
    private readonly UpdateBookEndpoint _sut;
    private readonly UpdateBookRequestFixture _updateBookRequestFixture = new();
    private readonly BookResponseFixture _bookResponseFixture = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="UpdateBookEndpointTests"/> class.
    /// </summary>
    public UpdateBookEndpointTests()
    {
        _mockHandler = Substitute.For<Lumina.Application.Common.CQRS.ICommandHandler<UpdateBookCommand, Result<BookResponse>>>();
        _sut = Factory.Create<UpdateBookEndpoint>(_mockHandler);
    }

    [Fact]
    public async Task ExecuteAsync_WhenSuccessful_ShouldReturnOkResultWithBookResponse()
    {
        // Arrange
        Guid libraryId = Guid.NewGuid();
        Guid routeId = Guid.NewGuid();
        UpdateBookRequest request = _updateBookRequestFixture.Create();
        CancellationToken cancellationToken = CancellationToken.None;
        BookResponse expectedResponse = _bookResponseFixture.Create();
        _mockHandler.HandleAsync(Arg.Any<UpdateBookCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.From(expectedResponse));
        ConfigureRequest(libraryId.ToString(), routeId.ToString());

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
        Guid libraryId = Guid.NewGuid();
        Guid routeId = Guid.NewGuid();
        UpdateBookRequest request = _updateBookRequestFixture.Create();
        CancellationToken cancellationToken = CancellationToken.None;
        Error expectedError = Error.NotFound("Book.NotFound", "BookNotFound");
        _mockHandler.HandleAsync(Arg.Any<UpdateBookCommand>(), Arg.Any<CancellationToken>())
            .Returns(expectedError);
        ConfigureRequest(libraryId.ToString(), routeId.ToString());

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
        Guid libraryId = Guid.NewGuid();
        Guid routeId = Guid.NewGuid();
        UpdateBookRequest request = _updateBookRequestFixture.Create();
        CancellationToken cancellationToken = CancellationToken.None;
        Error validationError = Error.Validation(description: "BookIdCannotBeEmpty");
        _mockHandler.HandleAsync(Arg.Any<UpdateBookCommand>(), Arg.Any<CancellationToken>())
            .Returns(validationError);
        ConfigureRequest(libraryId.ToString(), routeId.ToString());

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
    public async Task ExecuteAsync_WhenCalled_ShouldSendUpdateBookCommandToHandler()
    {
        // Arrange
        Guid libraryId = Guid.NewGuid();
        Guid routeId = Guid.NewGuid();
        UpdateBookRequest request = _updateBookRequestFixture.Create();
        CancellationToken cancellationToken = CancellationToken.None;
        _mockHandler.HandleAsync(Arg.Any<UpdateBookCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.From(_bookResponseFixture.Create()));
        ConfigureRequest(libraryId.ToString(), routeId.ToString());

        // Act
        await _sut.ExecuteAsync(request, cancellationToken);

        // Assert
        await _mockHandler.Received(1).HandleAsync(
            Arg.Is<UpdateBookCommand>(command =>
                command.LibraryId == libraryId.ToString() &&
                command.BookId == routeId.ToString() &&
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
    public async Task ExecuteAsync_WhenRouteValuesAreNotParseable_ShouldSendCommandWithRawRouteValues()
    {
        // Arrange
        UpdateBookRequest request = _updateBookRequestFixture.Create();
        CancellationToken cancellationToken = CancellationToken.None;
        _mockHandler.HandleAsync(Arg.Any<UpdateBookCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.From(_bookResponseFixture.Create()));
        ConfigureRequest("not-a-library-guid", "not-a-guid");

        // Act
        await _sut.ExecuteAsync(request, cancellationToken);

        // Assert
        await _mockHandler.Received(1).HandleAsync(
            Arg.Is<UpdateBookCommand>(command => command.LibraryId == "not-a-library-guid" && command.BookId == "not-a-guid"),
            Arg.Is(cancellationToken));
    }

    [Fact]
    public async Task ExecuteAsync_WhenRouteValuesAreMissing_ShouldSendCommandWithNullIds()
    {
        // Arrange
        UpdateBookRequest request = _updateBookRequestFixture.Create();
        CancellationToken cancellationToken = CancellationToken.None;
        _mockHandler.HandleAsync(Arg.Any<UpdateBookCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.From(_bookResponseFixture.Create()));
        ConfigureRequestWithoutIds();

        // Act
        await _sut.ExecuteAsync(request, cancellationToken);

        // Assert
        await _mockHandler.Received(1).HandleAsync(
            Arg.Is<UpdateBookCommand>(command => command.LibraryId == null && command.BookId == null),
            Arg.Is(cancellationToken));
    }

    [Fact]
    public async Task ExecuteAsync_WhenCancellationRequested_ShouldCancelOperation()
    {
        // Arrange
        Guid libraryId = Guid.NewGuid();
        Guid routeId = Guid.NewGuid();
        UpdateBookRequest request = _updateBookRequestFixture.Create();
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
        ConfigureRequest(libraryId.ToString(), routeId.ToString());

        // Act
        Task<IResult> operationTask = _sut.ExecuteAsync(request, cts.Token);
        await operationStarted.Task;
        cts.Cancel();
        cancellationRequested.SetResult(true);

        // Assert
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => operationTask);
    }

    /// <summary>
    /// Sets the <c>libraryId</c> and <c>id</c> route values the endpoint reads the identifiers from.
    /// </summary>
    /// <param name="libraryId">The raw route value of the library Id.</param>
    /// <param name="bookId">The raw route value of the book Id.</param>
    private void ConfigureRequest(string libraryId, string bookId)
    {
        _sut.HttpContext.Request.RouteValues["libraryId"] = libraryId;
        _sut.HttpContext.Request.RouteValues["bookId"] = bookId;
    }

    /// <summary>
    /// Leaves the <c>libraryId</c> and <c>id</c> route values unset.
    /// </summary>
    private void ConfigureRequestWithoutIds()
    {
        _sut.HttpContext.Request.RouteValues.Remove("libraryId");
        _sut.HttpContext.Request.RouteValues.Remove("bookId");
    }
}
