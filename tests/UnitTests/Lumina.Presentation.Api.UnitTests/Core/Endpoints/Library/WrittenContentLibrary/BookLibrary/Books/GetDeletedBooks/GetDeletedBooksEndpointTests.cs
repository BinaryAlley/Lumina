#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.CQRS;
using Lumina.Application.Core.MediaLibrary.WrittenContentLibrary.BookLibrary.Books.Queries.GetDeletedBooks;
using Lumina.Application.Fixtures.Core.MediaLibrary.WrittenContentLibrary.BookLibrary.Books.Queries.GetDeletedBooks;
using Lumina.Contracts.Fixtures.Responses.MediaLibrary.WrittenContentLibrary.BookLibrary.Books;
using Lumina.Contracts.Responses.MediaLibrary.WrittenContentLibrary.BookLibrary.Books;
using Lumina.Domain.Common.Primitives;
using Lumina.Presentation.Api.Core.Endpoints.Library.WrittenContentLibrary.BookLibrary.Books.GetDeletedBooks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using ApplicationErrors = Lumina.Application.Common.Errors.Errors;
using DomainErrors = Lumina.Domain.Common.Errors.Errors;
#endregion

namespace Lumina.Presentation.Api.UnitTests.Core.Endpoints.Library.WrittenContentLibrary.BookLibrary.Books.GetDeletedBooks;

/// <summary>
/// Contains unit tests for the <see cref="GetDeletedBooksEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetDeletedBooksEndpointTests
{
    private readonly IQueryHandler<GetDeletedBooksQuery, Result<IReadOnlyList<DeletedBookResponse>>> _mockHandler;
    private readonly GetDeletedBooksEndpoint _sut;
    private readonly DeletedBookResponseFixture _deletedBookResponseFixture = new();
    private readonly GetDeletedBooksQueryFixture _getDeletedBooksQueryFixture = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="GetDeletedBooksEndpointTests"/> class.
    /// </summary>
    public GetDeletedBooksEndpointTests()
    {
        _mockHandler = Substitute.For<IQueryHandler<GetDeletedBooksQuery, Result<IReadOnlyList<DeletedBookResponse>>>>();
        _sut = FastEndpoints.Factory.Create<GetDeletedBooksEndpoint>(_mockHandler);
    }

    [Fact]
    public async Task ExecuteAsync_WhenSuccessful_ShouldReturnOkResultWithDeletedBooks()
    {
        // Arrange
        Guid libraryId = Guid.NewGuid();
        IReadOnlyList<DeletedBookResponse> expectedBooks =
        [
            _deletedBookResponseFixture.Create(
                libraryId: libraryId,
                title: "The Hobbit",
                includeOriginalTitle: false,
                includeCoverPath: false)
        ];
        _mockHandler.HandleAsync(Arg.Any<GetDeletedBooksQuery>(), Arg.Any<CancellationToken>())
            .Returns(Result.From(expectedBooks));
        _sut.HttpContext.Request.RouteValues["libraryId"] = libraryId.ToString();

        // Act
        IResult result = await _sut.ExecuteAsync(FastEndpoints.EmptyRequest.Instance, CancellationToken.None);

        // Assert
        Ok<IReadOnlyList<DeletedBookResponse>> okResult = Assert.IsType<Ok<IReadOnlyList<DeletedBookResponse>>>(result);
        Assert.Equal(expectedBooks, okResult.Value);
    }

    [Fact]
    public async Task ExecuteAsync_WhenHandlerReturnsNotAuthorizedError_ShouldReturnProblemResult()
    {
        // Arrange
        Error expectedError = ApplicationErrors.Authorization.NotAuthorized;
        _mockHandler.HandleAsync(Arg.Any<GetDeletedBooksQuery>(), Arg.Any<CancellationToken>())
            .Returns(expectedError);
        _sut.HttpContext.Request.RouteValues["libraryId"] = Guid.NewGuid().ToString();

        // Act
        IResult result = await _sut.ExecuteAsync(FastEndpoints.EmptyRequest.Instance, CancellationToken.None);

        // Assert
        ProblemHttpResult problemDetails = Assert.IsType<ProblemHttpResult>(result);
        Assert.Equal(StatusCodes.Status403Forbidden, problemDetails.StatusCode);
        Assert.Equal("application/problem+json", problemDetails.ContentType);
        ProblemDetails problemDetailsBody = Assert.IsType<ProblemDetails>(problemDetails.ProblemDetails);
        Assert.Equal(StatusCodes.Status403Forbidden, problemDetailsBody.Status);
        Assert.Equal("General.Unauthorized", problemDetailsBody.Title);
        Assert.Equal("NotAuthorized", problemDetailsBody.Detail);
        Assert.Equal("https://tools.ietf.org/html/rfc9110#section-15.5.4", problemDetailsBody.Type);
        Assert.NotNull(problemDetailsBody.Extensions["traceId"]);
    }

    [Fact]
    public async Task ExecuteAsync_WhenHandlerReturnsValidationErrors_ShouldReturnValidationProblemResult()
    {
        // Arrange
        Error validationError = DomainErrors.Library.LibraryIdCannotBeEmpty;
        _mockHandler.HandleAsync(Arg.Any<GetDeletedBooksQuery>(), Arg.Any<CancellationToken>())
            .Returns(validationError);
        _sut.HttpContext.Request.RouteValues["libraryId"] = Guid.NewGuid().ToString();

        // Act
        IResult result = await _sut.ExecuteAsync(FastEndpoints.EmptyRequest.Instance, CancellationToken.None);

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
    public async Task ExecuteAsync_WhenCalled_ShouldSendTheRouteLibraryIdToTheHandler()
    {
        // Arrange
        Guid libraryId = Guid.NewGuid();
        _mockHandler.HandleAsync(Arg.Any<GetDeletedBooksQuery>(), Arg.Any<CancellationToken>())
            .Returns(Result.From<IReadOnlyList<DeletedBookResponse>>([]));
        _sut.HttpContext.Request.RouteValues["libraryId"] = libraryId.ToString();

        // Act
        await _sut.ExecuteAsync(FastEndpoints.EmptyRequest.Instance, CancellationToken.None);

        // Assert
        GetDeletedBooksQuery expectedQuery = _getDeletedBooksQueryFixture.Create(libraryId: libraryId.ToString());
        await _mockHandler.Received(1).HandleAsync(expectedQuery, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_WhenCancellationRequested_ShouldCancelOperation()
    {
        // Arrange
        CancellationTokenSource cts = new();
        TaskCompletionSource<bool> operationStarted = new();
        TaskCompletionSource<bool> cancellationRequested = new();
        _mockHandler.HandleAsync(Arg.Any<GetDeletedBooksQuery>(), Arg.Any<CancellationToken>())
            .Returns(callInfo => Task.Run(async () =>
            {
                operationStarted.SetResult(true);
                await cancellationRequested.Task;
                callInfo.Arg<CancellationToken>().ThrowIfCancellationRequested();
                return Result.From<IReadOnlyList<DeletedBookResponse>>([]);
            }, callInfo.Arg<CancellationToken>()));
        _sut.HttpContext.Request.RouteValues["libraryId"] = Guid.NewGuid().ToString();

        // Act
        Task<IResult> operationTask = _sut.ExecuteAsync(FastEndpoints.EmptyRequest.Instance, cts.Token);
        await operationStarted.Task;
        cts.Cancel();
        cancellationRequested.SetResult(true);

        // Assert
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => operationTask);
    }
}
