#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Application.Common.CQRS;
using Lumina.Application.Core.MediaLibrary.WrittenContentLibrary.BookLibrary.Books.Commands.AddBook;
using Lumina.Contracts.Fixtures.Core.Requests.MediaLibrary.WrittenContentLibrary.BookLibrary.Books;
using Lumina.Contracts.Fixtures.Core.Responses.MediaLibrary.WrittenContentLibrary.BookLibrary.Books;
using Lumina.Contracts.Requests.MediaLibrary.WrittenContentLibrary.BookLibrary.Books;
using Lumina.Contracts.Responses.MediaLibrary.WrittenContentLibrary.BookLibrary.Books;
using Lumina.Domain.Common.Errors;
using Lumina.Domain.Common.Primitives;
using Lumina.Presentation.Api.Core.Endpoints.Library.WrittenContentLibrary.BookLibrary.Books.AddBook;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using NSubstitute;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Api.UnitTests.Core.Endpoints.Library.WrittenContentLibrary.BookLibrary.Books.AddBook;

/// <summary>
/// Contains unit tests for the <see cref="AddBookEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class AddBookEndpointTests
{
    private readonly Lumina.Application.Common.CQRS.ICommandHandler<AddBookCommand, Result<BookResponse>> _mockHandler;
    private readonly AddBookEndpoint _sut;
    private readonly AddBookRequestFixture _addBookRequestFixture = new();
    private readonly BookResponseFixture _bookResponseFixture = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="AddBookEndpointTests"/> class.
    /// </summary>
    public AddBookEndpointTests()
    {
        _mockHandler = Substitute.For<Lumina.Application.Common.CQRS.ICommandHandler<AddBookCommand, Result<BookResponse>>>();
        _sut = Factory.Create<AddBookEndpoint>(_mockHandler);
    }

    [Fact]
    public async Task ExecuteAsync_WhenSuccessful_ShouldReturnCreatedResultWithBookResponse()
    {
        // Arrange
        Guid libraryId = Guid.NewGuid();
        AddBookRequest request = _addBookRequestFixture.Create();
        CancellationToken cancellationToken = CancellationToken.None;
        BookResponse expectedResponse = _bookResponseFixture.Create();
        _mockHandler.HandleAsync(Arg.Any<AddBookCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.From(expectedResponse));
        _sut.HttpContext.Request.RouteValues["libraryId"] = libraryId.ToString();

        // Act
        IResult result = await _sut.ExecuteAsync(request, cancellationToken);

        // Assert
        Created<BookResponse> createdResult = Assert.IsType<Created<BookResponse>>(result);
        Assert.Equal(expectedResponse, createdResult.Value);
        Assert.Contains(expectedResponse.Id.ToString(), createdResult.Location);
    }

    [Fact]
    public async Task ExecuteAsync_WhenHandlerReturnsConflictError_ShouldReturnProblemResult()
    {
        // Arrange
        AddBookRequest request = _addBookRequestFixture.Create();
        CancellationToken cancellationToken = CancellationToken.None;
        Error expectedError = Error.Conflict("Book.AlreadyExists", "BookAlreadyExists");
        _mockHandler.HandleAsync(Arg.Any<AddBookCommand>(), Arg.Any<CancellationToken>())
            .Returns(expectedError);

        // Act
        IResult result = await _sut.ExecuteAsync(request, cancellationToken);

        // Assert
        ProblemHttpResult problemDetails = Assert.IsType<ProblemHttpResult>(result);
        Assert.Equal(StatusCodes.Status409Conflict, problemDetails.StatusCode);
        Assert.Equal("application/problem+json", problemDetails.ContentType);
        Microsoft.AspNetCore.Mvc.ProblemDetails problemDetailsBody = Assert.IsType<Microsoft.AspNetCore.Mvc.ProblemDetails>(problemDetails.ProblemDetails);
        Assert.Equal(StatusCodes.Status409Conflict, problemDetailsBody.Status);
        Assert.Equal("Book.AlreadyExists", problemDetailsBody.Title);
        Assert.Equal("BookAlreadyExists", problemDetailsBody.Detail);
        Assert.Equal("https://tools.ietf.org/html/rfc9110#section-15.5.10", problemDetailsBody.Type);
        Assert.NotNull(problemDetailsBody.Extensions["traceId"]);
    }

    [Fact]
    public async Task ExecuteAsync_WhenCalled_ShouldSendAddBookCommandToSender()
    {
        // Arrange
        Guid libraryId = Guid.NewGuid();
        AddBookRequest request = _addBookRequestFixture.Create();
        CancellationToken cancellationToken = CancellationToken.None;
        _mockHandler.HandleAsync(Arg.Any<AddBookCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.From(_bookResponseFixture.Create()));
        _sut.HttpContext.Request.RouteValues["libraryId"] = libraryId.ToString();

        // Act
        await _sut.ExecuteAsync(request, cancellationToken);

        // Assert
        await _mockHandler.Received(1).HandleAsync(
            Arg.Is<AddBookCommand>(command =>
                command.LibraryId == libraryId.ToString() &&
                command.Path == request.Path),
            Arg.Is(cancellationToken));
    }

    [Fact]
    public async Task ExecuteAsync_WhenCancellationRequested_ShouldCancelOperation()
    {
        // Arrange
        AddBookRequest request = _addBookRequestFixture.Create();
        CancellationTokenSource cts = new();
        TaskCompletionSource<bool> operationStarted = new();
        TaskCompletionSource<bool> cancellationRequested = new();

        _mockHandler.HandleAsync(Arg.Any<AddBookCommand>(), Arg.Any<CancellationToken>())
            .Returns(callInfo => Task.Run(async () =>
            {
                operationStarted.SetResult(true);
                await cancellationRequested.Task;
                callInfo.Arg<CancellationToken>().ThrowIfCancellationRequested();
                return Result.From(_bookResponseFixture.Create());
            }, callInfo.Arg<CancellationToken>()));

        // Act
        Task<IResult> operationTask = _sut.ExecuteAsync(request, cts.Token);
        await operationStarted.Task;
        cts.Cancel();
        cancellationRequested.SetResult(true);

        // Assert
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => operationTask);
    }

    [Fact]
    public async Task ExecuteAsync_WhenHandlerReturnsValidationErrors_ShouldReturnValidationProblemResult()
    {
        // Arrange
        Guid libraryId = Guid.NewGuid();
        AddBookRequest request = _addBookRequestFixture.Create();
        CancellationToken cancellationToken = CancellationToken.None;
        Error validationError = Errors.Library.LibraryIdCannotBeEmpty;
        _mockHandler.HandleAsync(Arg.Any<AddBookCommand>(), Arg.Any<CancellationToken>())
            .Returns(validationError);
        _sut.HttpContext.Request.RouteValues["libraryId"] = libraryId.ToString();

        // Act
        IResult result = await _sut.ExecuteAsync(request, cancellationToken);

        // Assert
        ProblemHttpResult problemDetails = Assert.IsType<ProblemHttpResult>(result);
        Assert.Equal(StatusCodes.Status422UnprocessableEntity, problemDetails.StatusCode);
        Assert.Equal("application/problem+json", problemDetails.ContentType);
        HttpValidationProblemDetails validationProblemDetails = Assert.IsType<HttpValidationProblemDetails>(problemDetails.ProblemDetails);
        Assert.Equal(StatusCodes.Status422UnprocessableEntity, validationProblemDetails.Status);
        Assert.Equal("General.Validation", validationProblemDetails.Title);
        Assert.Equal("OneOrMoreValidationErrorsOccurred", validationProblemDetails.Detail);
        Assert.Equal("https://tools.ietf.org/html/rfc4918#section-11.2", validationProblemDetails.Type);
        Assert.Single(validationProblemDetails.Errors);
        Assert.Equal(new[] { "LibraryIdCannotBeEmpty" }, validationProblemDetails.Errors["General.Validation"]);
    }

    [Fact]
    public async Task ExecuteAsync_WhenRouteValueIsNotParseable_ShouldSendCommandWithRawRouteValue()
    {
        // Arrange
        AddBookRequest request = _addBookRequestFixture.Create();
        CancellationToken cancellationToken = CancellationToken.None;
        _mockHandler.HandleAsync(Arg.Any<AddBookCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.From(_bookResponseFixture.Create()));
        _sut.HttpContext.Request.RouteValues["libraryId"] = "not-a-library-guid";

        // Act
        await _sut.ExecuteAsync(request, cancellationToken);

        // Assert
        await _mockHandler.Received(1).HandleAsync(
            Arg.Is<AddBookCommand>(command => command.LibraryId == "not-a-library-guid"),
            Arg.Is(cancellationToken));
    }

    [Fact]
    public async Task ExecuteAsync_WhenRouteValueIsMissing_ShouldSendCommandWithNullLibraryId()
    {
        // Arrange
        AddBookRequest request = _addBookRequestFixture.Create();
        CancellationToken cancellationToken = CancellationToken.None;
        _mockHandler.HandleAsync(Arg.Any<AddBookCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.From(_bookResponseFixture.Create()));
        _sut.HttpContext.Request.RouteValues.Remove("libraryId");

        // Act
        await _sut.ExecuteAsync(request, cancellationToken);

        // Assert
        await _mockHandler.Received(1).HandleAsync(
            Arg.Is<AddBookCommand>(command => command.LibraryId == null),
            Arg.Is(cancellationToken));
    }
}
