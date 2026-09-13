#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.CQRS;
using Lumina.Application.Core.MediaLibrary.WrittenContentLibrary.BookLibrary.Books.Queries.GetBook;
using Lumina.Contracts.Fixtures.Core.Responses.MediaLibrary.WrittenContentLibrary.BookLibrary.Books;
using Lumina.Contracts.Responses.MediaLibrary.WrittenContentLibrary.BookLibrary.Books;
using Lumina.Domain.Common.Errors;
using Lumina.Domain.Common.Primitives;
using Lumina.Presentation.Api.Core.Endpoints.Library.WrittenContentLibrary.BookLibrary.Books.GetBook;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using NSubstitute;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Api.UnitTests.Core.Endpoints.Library.WrittenContentLibrary.BookLibrary.Books.GetBook;

/// <summary>
/// Contains unit tests for the <see cref="GetBookEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetBookEndpointTests
{
    private readonly IQueryHandler<GetBookQuery, Result<BookResponse>> _mockHandler;
    private readonly GetBookEndpoint _sut;
    private readonly BookResponseFixture _bookResponseFixture = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="GetBookEndpointTests"/> class.
    /// </summary>
    public GetBookEndpointTests()
    {
        _mockHandler = Substitute.For<IQueryHandler<GetBookQuery, Result<BookResponse>>>();
        _sut = FastEndpoints.Factory.Create<GetBookEndpoint>(_mockHandler);
    }

    [Fact]
    public async Task ExecuteAsync_WhenSuccessful_ShouldReturnOkResultWithBookResponse()
    {
        // Arrange
        Guid libraryId = Guid.NewGuid();
        Guid bookId = Guid.NewGuid();
        CancellationToken cancellationToken = CancellationToken.None;
        BookResponse expectedResponse = _bookResponseFixture.Create();
        _mockHandler.HandleAsync(Arg.Any<GetBookQuery>(), Arg.Any<CancellationToken>())
            .Returns(Result.From(expectedResponse));
        _sut.HttpContext.Request.RouteValues["libraryId"] = libraryId.ToString();
        _sut.HttpContext.Request.RouteValues["bookId"] = bookId.ToString();

        // Act
        IResult result = await _sut.ExecuteAsync(FastEndpoints.EmptyRequest.Instance, cancellationToken);

        // Assert
        Ok<BookResponse> okResult = Assert.IsType<Ok<BookResponse>>(result);
        Assert.Equal(expectedResponse, okResult.Value);
    }

    [Fact]
    public async Task ExecuteAsync_WhenHandlerReturnsNotFoundError_ShouldReturnProblemResult()
    {
        // Arrange
        Guid libraryId = Guid.NewGuid();
        Guid bookId = Guid.NewGuid();
        CancellationToken cancellationToken = CancellationToken.None;
        Error expectedError = Error.NotFound("Book.NotFound", "BookNotFound");
        _mockHandler.HandleAsync(Arg.Any<GetBookQuery>(), Arg.Any<CancellationToken>())
            .Returns(expectedError);
        _sut.HttpContext.Request.RouteValues["libraryId"] = libraryId.ToString();
        _sut.HttpContext.Request.RouteValues["bookId"] = bookId.ToString();

        // Act
        IResult result = await _sut.ExecuteAsync(FastEndpoints.EmptyRequest.Instance, cancellationToken);

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
        Guid bookId = Guid.NewGuid();
        CancellationToken cancellationToken = CancellationToken.None;
        Error validationError = Errors.WrittenContent.BookIdCannotBeEmpty;
        _mockHandler.HandleAsync(Arg.Any<GetBookQuery>(), Arg.Any<CancellationToken>())
            .Returns(validationError);
        _sut.HttpContext.Request.RouteValues["libraryId"] = libraryId.ToString();
        _sut.HttpContext.Request.RouteValues["bookId"] = bookId.ToString();

        // Act
        IResult result = await _sut.ExecuteAsync(FastEndpoints.EmptyRequest.Instance, cancellationToken);

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
        Assert.Equal(new[] { "BookIdCannotBeEmpty" }, validationProblemDetails.Errors["General.Validation"]);
    }

    [Fact]
    public async Task ExecuteAsync_WhenCalled_ShouldSendGetBookQueryToSender()
    {
        // Arrange
        Guid libraryId = Guid.NewGuid();
        Guid bookId = Guid.NewGuid();
        CancellationToken cancellationToken = CancellationToken.None;
        _mockHandler.HandleAsync(Arg.Any<GetBookQuery>(), Arg.Any<CancellationToken>())
            .Returns(Result.From(_bookResponseFixture.Create()));
        _sut.HttpContext.Request.RouteValues["libraryId"] = libraryId.ToString();
        _sut.HttpContext.Request.RouteValues["bookId"] = bookId.ToString();

        // Act
        await _sut.ExecuteAsync(FastEndpoints.EmptyRequest.Instance, cancellationToken);

        // Assert
        await _mockHandler.Received(1).HandleAsync(
            Arg.Is<GetBookQuery>(query => query.LibraryId == libraryId.ToString() && query.BookId == bookId.ToString()),
            Arg.Is(cancellationToken));
    }

    [Fact]
    public async Task ExecuteAsync_WhenRouteValuesAreNotParseable_ShouldSendQueryWithRawRouteValues()
    {
        // Arrange
        CancellationToken cancellationToken = CancellationToken.None;
        _mockHandler.HandleAsync(Arg.Any<GetBookQuery>(), Arg.Any<CancellationToken>())
            .Returns(Result.From(_bookResponseFixture.Create()));
        _sut.HttpContext.Request.RouteValues["libraryId"] = "not-a-library-guid";
        _sut.HttpContext.Request.RouteValues["bookId"] = "not-a-guid";

        // Act
        await _sut.ExecuteAsync(FastEndpoints.EmptyRequest.Instance, cancellationToken);

        // Assert
        await _mockHandler.Received(1).HandleAsync(
            Arg.Is<GetBookQuery>(query => query.LibraryId == "not-a-library-guid" && query.BookId == "not-a-guid"),
            Arg.Is(cancellationToken));
    }

    [Fact]
    public async Task ExecuteAsync_WhenRouteValuesAreMissing_ShouldSendQueryWithNullIds()
    {
        // Arrange
        CancellationToken cancellationToken = CancellationToken.None;
        _mockHandler.HandleAsync(Arg.Any<GetBookQuery>(), Arg.Any<CancellationToken>())
            .Returns(Result.From(_bookResponseFixture.Create()));
        _sut.HttpContext.Request.RouteValues.Remove("libraryId");
        _sut.HttpContext.Request.RouteValues.Remove("bookId");

        // Act
        await _sut.ExecuteAsync(FastEndpoints.EmptyRequest.Instance, cancellationToken);

        // Assert
        await _mockHandler.Received(1).HandleAsync(
            Arg.Is<GetBookQuery>(query => query.LibraryId == null && query.BookId == null),
            Arg.Is(cancellationToken));
    }

    [Fact]
    public async Task ExecuteAsync_WhenCancellationRequested_ShouldCancelOperation()
    {
        // Arrange
        Guid libraryId = Guid.NewGuid();
        Guid bookId = Guid.NewGuid();
        CancellationTokenSource cts = new();
        TaskCompletionSource<bool> operationStarted = new();
        TaskCompletionSource<bool> cancellationRequested = new();

        _mockHandler.HandleAsync(Arg.Any<GetBookQuery>(), Arg.Any<CancellationToken>())
            .Returns(callInfo => Task.Run(async () =>
            {
                operationStarted.SetResult(true);
                await cancellationRequested.Task;
                callInfo.Arg<CancellationToken>().ThrowIfCancellationRequested();
                return Result.From(_bookResponseFixture.Create());
            }, callInfo.Arg<CancellationToken>()));
        _sut.HttpContext.Request.RouteValues["libraryId"] = libraryId.ToString();
        _sut.HttpContext.Request.RouteValues["bookId"] = bookId.ToString();

        // Act
        Task<IResult> operationTask = _sut.ExecuteAsync(FastEndpoints.EmptyRequest.Instance, cts.Token);
        await operationStarted.Task;
        cts.Cancel();
        cancellationRequested.SetResult(true);

        // Assert
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => operationTask);
    }
}
