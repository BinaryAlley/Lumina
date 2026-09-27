#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.CQRS;
using Lumina.Application.Core.MediaLibrary.WrittenContentLibrary.BookLibrary.Books.Reading.Queries.GetReadingSection;
using Lumina.Contracts.DTO.MediaLibrary.WrittenContentLibrary.BookLibrary.Reading;
using Lumina.Contracts.Fixtures.Core.DTO.MediaLibrary.WrittenContentLibrary.BookLibrary.Reading;
using Lumina.Domain.Common.Errors;
using Lumina.Domain.Common.Primitives;
using Lumina.Presentation.Api.Core.Endpoints.Library.WrittenContentLibrary.BookLibrary.Books.Reading.GetReadingSection;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using NSubstitute;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Api.UnitTests.Core.Endpoints.Library.WrittenContentLibrary.BookLibrary.Books.Reading.GetReadingSection;

/// <summary>
/// Contains unit tests for the <see cref="GetReadingSectionEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetReadingSectionEndpointTests
{
    private readonly IQueryHandler<GetReadingSectionQuery, Result<ReadingSectionDto>> _mockHandler;
    private readonly GetReadingSectionEndpoint _sut;
    private readonly ReadingSectionDtoFixture _readingSectionDtoFixture = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="GetReadingSectionEndpointTests"/> class.
    /// </summary>
    public GetReadingSectionEndpointTests()
    {
        _mockHandler = Substitute.For<IQueryHandler<GetReadingSectionQuery, Result<ReadingSectionDto>>>();
        _sut = FastEndpoints.Factory.Create<GetReadingSectionEndpoint>(_mockHandler);
    }

    [Fact]
    public async Task ExecuteAsync_WhenSuccessful_ShouldReturnOkResultWithReadingSection()
    {
        // Arrange
        Guid libraryId = Guid.NewGuid();
        Guid bookId = Guid.NewGuid();
        CancellationToken cancellationToken = CancellationToken.None;
        ReadingSectionDto expectedResponse = _readingSectionDtoFixture.Create(locationRef: "chapter-1");
        _mockHandler.HandleAsync(Arg.Any<GetReadingSectionQuery>(), Arg.Any<CancellationToken>())
            .Returns(Result.From(expectedResponse));
        _sut.HttpContext.Request.RouteValues["libraryId"] = libraryId.ToString();
        _sut.HttpContext.Request.RouteValues["bookId"] = bookId.ToString();
        _sut.HttpContext.Request.RouteValues["locationRef"] = "chapter-1";

        // Act
        IResult result = await _sut.ExecuteAsync(FastEndpoints.EmptyRequest.Instance, cancellationToken);

        // Assert
        Ok<ReadingSectionDto> okResult = Assert.IsType<Ok<ReadingSectionDto>>(result);
        Assert.Equal(expectedResponse, okResult.Value);
    }

    [Fact]
    public async Task ExecuteAsync_WhenHandlerReturnsNotFoundError_ShouldReturnProblemResult()
    {
        // Arrange
        Guid libraryId = Guid.NewGuid();
        Guid bookId = Guid.NewGuid();
        CancellationToken cancellationToken = CancellationToken.None;
        Error expectedError = Error.NotFound("Reading.SectionNotFound", "SectionNotFound");
        _mockHandler.HandleAsync(Arg.Any<GetReadingSectionQuery>(), Arg.Any<CancellationToken>())
            .Returns(expectedError);
        _sut.HttpContext.Request.RouteValues["libraryId"] = libraryId.ToString();
        _sut.HttpContext.Request.RouteValues["bookId"] = bookId.ToString();
        _sut.HttpContext.Request.RouteValues["locationRef"] = "chapter-1";

        // Act
        IResult result = await _sut.ExecuteAsync(FastEndpoints.EmptyRequest.Instance, cancellationToken);

        // Assert
        ProblemHttpResult problemResult = Assert.IsType<ProblemHttpResult>(result);
        Assert.Equal(StatusCodes.Status404NotFound, problemResult.StatusCode);
        Assert.Equal("application/problem+json", problemResult.ContentType);
        Assert.Equal("Reading.SectionNotFound", problemResult.ProblemDetails.Title);
        Assert.Equal("SectionNotFound", problemResult.ProblemDetails.Detail);
    }

    [Fact]
    public async Task ExecuteAsync_WhenHandlerReturnsValidationErrors_ShouldReturnValidationProblemResult()
    {
        // Arrange
        Guid libraryId = Guid.NewGuid();
        Guid bookId = Guid.NewGuid();
        CancellationToken cancellationToken = CancellationToken.None;
        Error validationError = Errors.WrittenContent.BookIdCannotBeEmpty;
        _mockHandler.HandleAsync(Arg.Any<GetReadingSectionQuery>(), Arg.Any<CancellationToken>())
            .Returns(validationError);
        _sut.HttpContext.Request.RouteValues["libraryId"] = libraryId.ToString();
        _sut.HttpContext.Request.RouteValues["bookId"] = bookId.ToString();
        _sut.HttpContext.Request.RouteValues["locationRef"] = "chapter-1";

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
    public async Task ExecuteAsync_WhenCalled_ShouldSendGetReadingSectionQueryToSender()
    {
        // Arrange
        Guid libraryId = Guid.NewGuid();
        Guid bookId = Guid.NewGuid();
        CancellationToken cancellationToken = CancellationToken.None;
        _mockHandler.HandleAsync(Arg.Any<GetReadingSectionQuery>(), Arg.Any<CancellationToken>())
            .Returns(Result.From(_readingSectionDtoFixture.Create(locationRef: "chapter-1")));
        _sut.HttpContext.Request.RouteValues["libraryId"] = libraryId.ToString();
        _sut.HttpContext.Request.RouteValues["bookId"] = bookId.ToString();
        _sut.HttpContext.Request.RouteValues["locationRef"] = "chapter-1";

        // Act
        await _sut.ExecuteAsync(FastEndpoints.EmptyRequest.Instance, cancellationToken);

        // Assert
        await _mockHandler.Received(1).HandleAsync(
            Arg.Is<GetReadingSectionQuery>(query =>
                query.LibraryId == libraryId.ToString() &&
                query.BookId == bookId.ToString() &&
                query.LocationRef == "chapter-1"),
            Arg.Is(cancellationToken));
    }

    [Fact]
    public async Task ExecuteAsync_WhenRouteValuesAreNotParseable_ShouldSendQueryWithRawRouteValues()
    {
        // Arrange
        CancellationToken cancellationToken = CancellationToken.None;
        _mockHandler.HandleAsync(Arg.Any<GetReadingSectionQuery>(), Arg.Any<CancellationToken>())
            .Returns(Result.From(_readingSectionDtoFixture.Create()));
        _sut.HttpContext.Request.RouteValues["libraryId"] = "not-a-library-guid";
        _sut.HttpContext.Request.RouteValues["bookId"] = "not-a-guid";
        _sut.HttpContext.Request.RouteValues["locationRef"] = "chapter-1";

        // Act
        await _sut.ExecuteAsync(FastEndpoints.EmptyRequest.Instance, cancellationToken);

        // Assert
        await _mockHandler.Received(1).HandleAsync(
            Arg.Is<GetReadingSectionQuery>(query =>
                query.LibraryId == "not-a-library-guid" &&
                query.BookId == "not-a-guid" &&
                query.LocationRef == "chapter-1"),
            Arg.Is(cancellationToken));
    }

    [Fact]
    public async Task ExecuteAsync_WhenRouteValuesAreMissing_ShouldSendQueryWithNullValues()
    {
        // Arrange
        CancellationToken cancellationToken = CancellationToken.None;
        _mockHandler.HandleAsync(Arg.Any<GetReadingSectionQuery>(), Arg.Any<CancellationToken>())
            .Returns(Result.From(_readingSectionDtoFixture.Create()));
        _sut.HttpContext.Request.RouteValues.Remove("libraryId");
        _sut.HttpContext.Request.RouteValues.Remove("bookId");
        _sut.HttpContext.Request.RouteValues.Remove("locationRef");

        // Act
        await _sut.ExecuteAsync(FastEndpoints.EmptyRequest.Instance, cancellationToken);

        // Assert
        await _mockHandler.Received(1).HandleAsync(
            Arg.Is<GetReadingSectionQuery>(query => query.LibraryId == null && query.BookId == null && query.LocationRef == null),
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

        _mockHandler.HandleAsync(Arg.Any<GetReadingSectionQuery>(), Arg.Any<CancellationToken>())
            .Returns(info => Task.Run(async () =>
            {
                operationStarted.SetResult(true);
                await cancellationRequested.Task;
                info.Arg<CancellationToken>().ThrowIfCancellationRequested();
                return Result.From(_readingSectionDtoFixture.Create());
            }, info.Arg<CancellationToken>()));
        _sut.HttpContext.Request.RouteValues["libraryId"] = libraryId.ToString();
        _sut.HttpContext.Request.RouteValues["bookId"] = bookId.ToString();
        _sut.HttpContext.Request.RouteValues["locationRef"] = "chapter-1";

        // Act
        Task<IResult> operationTask = _sut.ExecuteAsync(FastEndpoints.EmptyRequest.Instance, cts.Token);
        await operationStarted.Task;
        cts.Cancel();
        cancellationRequested.SetResult(true);

        // Assert
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => operationTask);
    }
}
