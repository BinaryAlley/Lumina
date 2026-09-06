#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Application.Common.CQRS;
using Lumina.Application.Core.MediaLibrary.WrittenContentLibrary.BookLibrary.Books.Queries.GetBook;
using Lumina.Contracts.Fixtures.Core.Requests.MediaLibrary.WrittenContentLibrary.BookLibrary.Books;
using Lumina.Contracts.Fixtures.Core.Responses.MediaLibrary.WrittenContentLibrary.BookLibrary.Books;
using Lumina.Contracts.Requests.MediaLibrary.WrittenContentLibrary.BookLibrary.Books;
using Lumina.Contracts.Responses.MediaLibrary.WrittenContentLibrary.BookLibrary.Books;
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
    private readonly GetBookRequestFixture _getBookRequestFixture = new();
    private readonly BookResponseFixture _bookResponseFixture = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="GetBookEndpointTests"/> class.
    /// </summary>
    public GetBookEndpointTests()
    {
        _mockHandler = Substitute.For<IQueryHandler<GetBookQuery, Result<BookResponse>>>();
        _sut = Factory.Create<GetBookEndpoint>(_mockHandler);
    }

    [Fact]
    public async Task ExecuteAsync_WhenSuccessful_ShouldReturnOkResultWithBookResponse()
    {
        // Arrange
        GetBookRequest request = _getBookRequestFixture.Create();
        CancellationToken cancellationToken = CancellationToken.None;
        BookResponse expectedResponse = _bookResponseFixture.Create();
        _mockHandler.HandleAsync(Arg.Any<GetBookQuery>(), Arg.Any<CancellationToken>())
            .Returns(Result.From(expectedResponse));

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
        GetBookRequest request = _getBookRequestFixture.Create();
        CancellationToken cancellationToken = CancellationToken.None;
        Error expectedError = Error.NotFound("Book.NotFound", "BookNotFound");
        _mockHandler.HandleAsync(Arg.Any<GetBookQuery>(), Arg.Any<CancellationToken>())
            .Returns(expectedError);

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
        GetBookRequest request = _getBookRequestFixture.Create();
        CancellationToken cancellationToken = CancellationToken.None;
        Error validationError = Error.Validation(description: "BookIdCannotBeEmpty");
        _mockHandler.HandleAsync(Arg.Any<GetBookQuery>(), Arg.Any<CancellationToken>())
            .Returns(validationError);

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
    public async Task ExecuteAsync_WhenCalled_ShouldSendGetBookQueryToHandler()
    {
        // Arrange
        Guid bookId = Guid.NewGuid();
        GetBookRequest request = _getBookRequestFixture.Create(id: bookId.ToString());
        CancellationToken cancellationToken = CancellationToken.None;
        _mockHandler.HandleAsync(Arg.Any<GetBookQuery>(), Arg.Any<CancellationToken>())
            .Returns(Result.From(_bookResponseFixture.Create()));

        // Act
        await _sut.ExecuteAsync(request, cancellationToken);

        // Assert
        await _mockHandler.Received(1).HandleAsync(
            Arg.Is<GetBookQuery>(query => query.Id == bookId),
            Arg.Is(cancellationToken));
    }

    [Fact]
    public async Task ExecuteAsync_WhenCancellationRequested_ShouldCancelOperation()
    {
        // Arrange
        GetBookRequest request = _getBookRequestFixture.Create();
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

        // Act
        Task<IResult> operationTask = _sut.ExecuteAsync(request, cts.Token);
        await operationStarted.Task;
        cts.Cancel();
        cancellationRequested.SetResult(true);

        // Assert
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => operationTask);
    }
}
