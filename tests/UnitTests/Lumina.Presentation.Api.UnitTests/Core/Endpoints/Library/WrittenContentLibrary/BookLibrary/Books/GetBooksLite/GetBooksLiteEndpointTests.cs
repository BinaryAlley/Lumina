#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Application.Common.CQRS;
using Lumina.Application.Core.MediaLibrary.WrittenContentLibrary.BooksLibrary.Books.Queries.GetBooksLite;
using Lumina.Contracts.Fixtures.Core.Requests.MediaLibrary.WrittenContentLibrary.BookLibrary.Books;
using Lumina.Contracts.Requests.MediaLibrary.WrittenContentLibrary.BookLibrary.Books;
using Lumina.Contracts.Responses.Common;
using Lumina.Contracts.Responses.MediaLibrary.WrittenContentLibrary.BookLibrary.Books;
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.Common.Errors;
using Lumina.Presentation.Api.Core.Endpoints.Library.WrittenContentLibrary.BookLibrary.Books.GetBooksLite;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Api.UnitTests.Core.Endpoints.Library.WrittenContentLibrary.BookLibrary.Books.GetBooksLite;

/// <summary>
/// Contains unit tests for the <see cref="GetBooksLiteEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetBooksLiteEndpointTests
{
    private readonly IQueryHandler<GetBooksLiteQuery, Result<PaginatedResponse<BookLiteResponse>>> _mockHandler;
    private readonly GetBooksLiteEndpoint _sut;
    private readonly GetBooksLiteRequestFixture _getBooksLiteRequestFixture = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="GetBooksLiteEndpointTests"/> class.
    /// </summary>
    public GetBooksLiteEndpointTests()
    {
        _mockHandler = Substitute.For<IQueryHandler<GetBooksLiteQuery, Result<PaginatedResponse<BookLiteResponse>>>>();
        _sut = Factory.Create<GetBooksLiteEndpoint>(_mockHandler);
    }

    [Fact]
    public async Task ExecuteAsync_WhenSuccessful_ShouldReturnOkResultWithPaginatedBookLiteResponses()
    {
        // Arrange
        GetBooksLiteRequest request = _getBooksLiteRequestFixture.Create();
        CancellationToken cancellationToken = CancellationToken.None;
        PaginatedResponse<BookLiteResponse> expectedResponse = CreatePaginatedResponse(bookCount: 2);
        _mockHandler.HandleAsync(Arg.Any<GetBooksLiteQuery>(), Arg.Any<CancellationToken>())
            .Returns(Result.From(expectedResponse));

        // Act
        IResult result = await _sut.ExecuteAsync(request, cancellationToken);

        // Assert
        Ok<PaginatedResponse<BookLiteResponse>> okResult = Assert.IsType<Ok<PaginatedResponse<BookLiteResponse>>>(result);
        Assert.Equal(expectedResponse, okResult.Value);
    }

    [Fact]
    public async Task ExecuteAsync_WhenHandlerReturnsError_ShouldReturnProblemResult()
    {
        // Arrange
        GetBooksLiteRequest request = _getBooksLiteRequestFixture.Create();
        CancellationToken cancellationToken = CancellationToken.None;
        Error expectedError = Error.NotFound("Book.NotFound", "The requested book was not found.");
        _mockHandler.HandleAsync(Arg.Any<GetBooksLiteQuery>(), Arg.Any<CancellationToken>())
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
        GetBooksLiteRequest request = _getBooksLiteRequestFixture.Create();
        CancellationToken cancellationToken = CancellationToken.None;
        Error expectedError = Errors.Library.LibraryIdCannotBeEmpty;
        _mockHandler.HandleAsync(Arg.Any<GetBooksLiteQuery>(), Arg.Any<CancellationToken>())
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
    public async Task ExecuteAsync_WhenCalled_ShouldSendGetBooksLiteQueryToSender()
    {
        // Arrange
        GetBooksLiteRequest request = _getBooksLiteRequestFixture.Create();
        CancellationToken cancellationToken = CancellationToken.None;
        _mockHandler.HandleAsync(Arg.Any<GetBooksLiteQuery>(), Arg.Any<CancellationToken>())
            .Returns(Result.From(CreatePaginatedResponse(bookCount: 0)));

        // Act
        await _sut.ExecuteAsync(request, cancellationToken);

        // Assert
        await _mockHandler.Received(1).HandleAsync(Arg.Is<GetBooksLiteQuery>(query =>
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
        GetBooksLiteRequest request = _getBooksLiteRequestFixture.Create();
        CancellationTokenSource cts = new();
        TaskCompletionSource<bool> operationStarted = new();
        TaskCompletionSource<bool> cancellationRequested = new();

        _mockHandler.HandleAsync(Arg.Any<GetBooksLiteQuery>(), Arg.Any<CancellationToken>())
            .Returns(callInfo => Task.Run(async () =>
            {
                operationStarted.SetResult(true);
                await cancellationRequested.Task;
                callInfo.Arg<CancellationToken>().ThrowIfCancellationRequested();
                return Result.From(CreatePaginatedResponse(bookCount: 0));
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
    /// Creates a <see cref="PaginatedResponse{BookLiteResponse}"/> with the specified number of books.
    /// </summary>
    /// <param name="bookCount">The number of book responses to include in the data collection.</param>
    /// <returns>A configured paginated response instance.</returns>
    private static PaginatedResponse<BookLiteResponse> CreatePaginatedResponse(int bookCount)
    {
        List<BookLiteResponse> books = [];
        for (int index = 0; index < bookCount; index++)
            books.Add(new BookLiteResponse(
                Id: Guid.NewGuid(),
                Title: "Test Book",
                ReleaseYear: 2001,
                CoverPath: null
            ));

        return new PaginatedResponse<BookLiteResponse>
        {
            Data = books,
            CurrentPage = 1,
            PerPage = 10,
            Count = bookCount,
            NumberOfPages = bookCount > 0 ? 1 : 0
        };
    }
}
