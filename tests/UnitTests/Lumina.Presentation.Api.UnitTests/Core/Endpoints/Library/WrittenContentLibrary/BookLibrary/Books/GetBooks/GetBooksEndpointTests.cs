#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Application.Common.CQRS;
using Lumina.Application.Core.MediaLibrary.WrittenContentLibrary.BooksLibrary.Books.Queries.GetBooks;
using Lumina.Contracts.Fixtures.Core.Requests.MediaLibrary.WrittenContentLibrary.BookLibrary.Books;
using Lumina.Contracts.Fixtures.Core.Responses.MediaLibrary.WrittenContentLibrary.BookLibrary.Books;
using Lumina.Contracts.Fixtures.Responses.Common;
using Lumina.Contracts.Requests.MediaLibrary.WrittenContentLibrary.BookLibrary.Books;
using Lumina.Contracts.Responses.Common;
using Lumina.Contracts.Responses.MediaLibrary.WrittenContentLibrary.BookLibrary.Books;
using Lumina.Domain.Common.Errors;
using Lumina.Domain.Common.Primitives;
using Lumina.Presentation.Api.Core.Endpoints.Library.WrittenContentLibrary.BookLibrary.Books.GetBooks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Api.UnitTests.Core.Endpoints.Library.WrittenContentLibrary.BookLibrary.Books.GetBooks;

/// <summary>
/// Contains unit tests for the <see cref="GetBooksEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetBooksEndpointTests
{
    private readonly IQueryHandler<GetBooksQuery, Result<PaginatedResponse<BookResponse>>> _mockHandler;
    private readonly GetBooksEndpoint _sut;
    private readonly GetBooksRequestFixture _getBooksRequestFixture = new();
    private readonly BookResponseFixture _bookResponseFixture = new();
    private readonly PaginatedResponseFixture<BookResponse> _paginatedResponseFixture = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="GetBooksEndpointTests"/> class.
    /// </summary>
    public GetBooksEndpointTests()
    {
        _mockHandler = Substitute.For<IQueryHandler<GetBooksQuery, Result<PaginatedResponse<BookResponse>>>>();
        _sut = Factory.Create<GetBooksEndpoint>(_mockHandler);
    }

    [Fact]
    public async Task ExecuteAsync_WhenSuccessful_ShouldReturnOkResultWithPaginatedBookResponses()
    {
        // Arrange
        GetBooksRequest request = _getBooksRequestFixture.Create();
        CancellationToken cancellationToken = CancellationToken.None;
        PaginatedResponse<BookResponse> expectedResponse = CreatePaginatedResponse(bookCount: 2, _bookResponseFixture);
        _mockHandler.HandleAsync(Arg.Any<GetBooksQuery>(), Arg.Any<CancellationToken>())
            .Returns(Result.From(expectedResponse));

        // Act
        IResult result = await _sut.ExecuteAsync(request, cancellationToken);

        // Assert
        Ok<PaginatedResponse<BookResponse>> okResult = Assert.IsType<Ok<PaginatedResponse<BookResponse>>>(result);
        Assert.Equal(expectedResponse, okResult.Value);
    }

    [Fact]
    public async Task ExecuteAsync_WhenHandlerReturnsError_ShouldReturnProblemResult()
    {
        // Arrange
        GetBooksRequest request = _getBooksRequestFixture.Create();
        CancellationToken cancellationToken = CancellationToken.None;
        Error expectedError = Error.NotFound("Book.NotFound", "The requested book was not found.");
        _mockHandler.HandleAsync(Arg.Any<GetBooksQuery>(), Arg.Any<CancellationToken>())
            .Returns(expectedError);

        // Act
        IResult result = await _sut.ExecuteAsync(request, cancellationToken);

        // Assert
        ProblemHttpResult problemDetails = Assert.IsType<ProblemHttpResult>(result);
        Assert.Equal(StatusCodes.Status404NotFound, problemDetails.StatusCode);
        Assert.Equal("application/problem+json", problemDetails.ContentType);
        Assert.IsType<Microsoft.AspNetCore.Mvc.ProblemDetails>(problemDetails.ProblemDetails);

        Assert.Equal("Book.NotFound", problemDetails.ProblemDetails.Title);
        Assert.Equal("The requested book was not found.", problemDetails.ProblemDetails.Detail);
        Assert.Equal(StatusCodes.Status404NotFound, problemDetails.ProblemDetails.Status);
        Assert.Equal("https://tools.ietf.org/html/rfc9110#section-15.5.5", problemDetails.ProblemDetails.Type);
        Assert.NotNull(problemDetails.ProblemDetails.Extensions["traceId"]);
    }

    [Fact]
    public async Task ExecuteAsync_WhenHandlerReturnsValidationError_ShouldReturnValidationProblemResult()
    {
        // Arrange
        GetBooksRequest request = _getBooksRequestFixture.Create();
        CancellationToken cancellationToken = CancellationToken.None;
        Error expectedError = Errors.Library.LibraryIdCannotBeEmpty;
        _mockHandler.HandleAsync(Arg.Any<GetBooksQuery>(), Arg.Any<CancellationToken>())
            .Returns(expectedError);

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
    public async Task ExecuteAsync_WhenCalled_ShouldSendGetBooksQueryToSender()
    {
        // Arrange
        GetBooksRequest request = _getBooksRequestFixture.Create();
        CancellationToken cancellationToken = CancellationToken.None;
        _mockHandler.HandleAsync(Arg.Any<GetBooksQuery>(), Arg.Any<CancellationToken>())
            .Returns(Result.From(CreatePaginatedResponse(bookCount: 0, _bookResponseFixture)));

        // Act
        await _sut.ExecuteAsync(request, cancellationToken);

        // Assert
        await _mockHandler.Received(1).HandleAsync(Arg.Is<GetBooksQuery>(query =>
            query.Filter.LibraryId == request.LibraryId &&
            query.Filter.SearchTerm == request.SearchTerm &&
            query.SortBy == request.SortBy &&
            query.SortOrder == request.SortOrder &&
            query.PaginationData != null &&
            query.PaginationData.CurrentPage == request.CurrentPage &&
            query.PaginationData.PerPage == request.PerPage),
            Arg.Is(cancellationToken));
    }

    [Fact]
    public async Task ExecuteAsync_WhenCancellationRequested_ShouldCancelOperation()
    {
        // Arrange
        GetBooksRequest request = _getBooksRequestFixture.Create();
        CancellationTokenSource cts = new();
        TaskCompletionSource<bool> operationStarted = new();
        TaskCompletionSource<bool> cancellationRequested = new();

        _mockHandler.HandleAsync(Arg.Any<GetBooksQuery>(), Arg.Any<CancellationToken>())
            .Returns(callInfo => Task.Run(async () =>
            {
                operationStarted.SetResult(true);
                await cancellationRequested.Task;
                callInfo.Arg<CancellationToken>().ThrowIfCancellationRequested();
                return Result.From(CreatePaginatedResponse(bookCount: 0, _bookResponseFixture));
            }, callInfo.Arg<CancellationToken>()));

        // Act
        Task<IResult> operationTask = _sut.ExecuteAsync(request, cts.Token);
        await operationStarted.Task;
        cts.Cancel();
        cancellationRequested.SetResult(true);

        // Assert
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => operationTask);
    }

    /// <summary>
    /// Creates a <see cref="PaginatedResponse{BookResponse}"/> with the specified number of books.
    /// </summary>
    /// <param name="bookCount">The number of book responses to include in the data collection.</param>
    /// <returns>A configured paginated response instance.</returns>
    private PaginatedResponse<BookResponse> CreatePaginatedResponse(int bookCount, BookResponseFixture bookResponseFixture)
    {
        List<BookResponse> books = [];
        for (int index = 0; index < bookCount; index++)
            books.Add(bookResponseFixture.Create());

        return _paginatedResponseFixture.Create(data: books, currentPage: 1, perPage: 10, count: bookCount, numberOfPages: bookCount > 0 ? 1 : 0);
    }
}
