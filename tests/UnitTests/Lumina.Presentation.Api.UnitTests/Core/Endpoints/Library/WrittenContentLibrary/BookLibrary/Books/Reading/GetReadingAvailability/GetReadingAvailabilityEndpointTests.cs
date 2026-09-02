#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Application.Common.CQRS;
using Lumina.Application.Core.MediaLibrary.WrittenContentLibrary.BookLibrary.Books.Reading.Queries.GetReadingAvailability;
using Lumina.Contracts.Fixtures.Core.Requests.MediaLibrary.WrittenContentLibrary.BookLibrary.Books.Reading;
using Lumina.Contracts.Fixtures.Core.Responses.MediaLibrary.WrittenContentLibrary.BookLibrary.Books.Reading;
using Lumina.Contracts.Requests.MediaLibrary.WrittenContentLibrary.BookLibrary.Books.Reading;
using Lumina.Contracts.Responses.MediaLibrary.WrittenContentLibrary.BookLibrary.Books.Reading;
using Lumina.Domain.Common.Primitives;
using Lumina.Presentation.Api.Core.Endpoints.Library.WrittenContentLibrary.BookLibrary.Books.Reading.GetReadingAvailability;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using NSubstitute;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Api.UnitTests.Core.Endpoints.Library.WrittenContentLibrary.BookLibrary.Books.Reading.GetReadingAvailability;

/// <summary>
/// Contains unit tests for the <see cref="GetReadingAvailabilityEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetReadingAvailabilityEndpointTests
{
    private readonly IQueryHandler<GetReadingAvailabilityQuery, Result<ReadingAvailabilityResponse>> _mockHandler;
    private readonly GetReadingAvailabilityEndpoint _sut;
    private readonly GetReadingAvailabilityRequestFixture _getReadingAvailabilityRequestFixture = new();
    private readonly ReadingAvailabilityResponseFixture _readingAvailabilityResponseFixture = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="GetReadingAvailabilityEndpointTests"/> class.
    /// </summary>
    public GetReadingAvailabilityEndpointTests()
    {
        _mockHandler = Substitute.For<IQueryHandler<GetReadingAvailabilityQuery, Result<ReadingAvailabilityResponse>>>();
        _sut = Factory.Create<GetReadingAvailabilityEndpoint>(_mockHandler);
    }

    [Fact]
    public async Task ExecuteAsync_WhenSuccessful_ShouldReturnOkResultWithReadingAvailability()
    {
        // Arrange
        GetReadingAvailabilityRequest request = _getReadingAvailabilityRequestFixture.Create();
        CancellationToken cancellationToken = CancellationToken.None;
        ReadingAvailabilityResponse expectedResponse = _readingAvailabilityResponseFixture.Create(bookId: request.BookId, isAvailable: true, errorCode: null);
        _mockHandler.HandleAsync(Arg.Any<GetReadingAvailabilityQuery>(), Arg.Any<CancellationToken>())
            .Returns(Result.From(expectedResponse));

        // Act
        IResult result = await _sut.ExecuteAsync(request, cancellationToken);

        // Assert
        Ok<ReadingAvailabilityResponse> okResult = Assert.IsType<Ok<ReadingAvailabilityResponse>>(result);
        Assert.Equal(expectedResponse, okResult.Value);
    }

    [Fact]
    public async Task ExecuteAsync_WhenBookIsNotAvailable_ShouldReturnOkResultWithUnavailableReadingAvailability()
    {
        // Arrange
        GetReadingAvailabilityRequest request = _getReadingAvailabilityRequestFixture.Create();
        CancellationToken cancellationToken = CancellationToken.None;
        ReadingAvailabilityResponse expectedResponse = _readingAvailabilityResponseFixture.Create(bookId: request.BookId, isAvailable: false, errorCode: "ReaderDisabled");
        _mockHandler.HandleAsync(Arg.Any<GetReadingAvailabilityQuery>(), Arg.Any<CancellationToken>())
            .Returns(Result.From(expectedResponse));

        // Act
        IResult result = await _sut.ExecuteAsync(request, cancellationToken);

        // Assert
        Ok<ReadingAvailabilityResponse> okResult = Assert.IsType<Ok<ReadingAvailabilityResponse>>(result);
        Assert.Equal(expectedResponse, okResult.Value);
        Assert.False(okResult.Value.IsAvailable);
        Assert.Equal("ReaderDisabled", okResult.Value.ErrorCode);
    }

    [Fact]
    public async Task ExecuteAsync_WhenHandlerReturnsError_ShouldReturnProblemResult()
    {
        // Arrange
        GetReadingAvailabilityRequest request = _getReadingAvailabilityRequestFixture.Create();
        CancellationToken cancellationToken = CancellationToken.None;
        Error expectedError = Error.NotFound("Reading.BookNotFound", "BookNotFound");
        _mockHandler.HandleAsync(Arg.Any<GetReadingAvailabilityQuery>(), Arg.Any<CancellationToken>())
            .Returns(expectedError);

        // Act
        IResult result = await _sut.ExecuteAsync(request, cancellationToken);

        // Assert
        ProblemHttpResult problemResult = Assert.IsType<ProblemHttpResult>(result);
        Assert.Equal(StatusCodes.Status404NotFound, problemResult.StatusCode);
        Assert.Equal("application/problem+json", problemResult.ContentType);
        Assert.Equal("Reading.BookNotFound", problemResult.ProblemDetails.Title);
        Assert.Equal("BookNotFound", problemResult.ProblemDetails.Detail);
    }

    [Fact]
    public async Task ExecuteAsync_WhenCalled_ShouldSendGetReadingAvailabilityQueryToSender()
    {
        // Arrange
        GetReadingAvailabilityRequest request = _getReadingAvailabilityRequestFixture.Create();
        CancellationToken cancellationToken = CancellationToken.None;
        ReadingAvailabilityResponse response = _readingAvailabilityResponseFixture.Create(bookId: request.BookId, isAvailable: true, errorCode: null);
        _mockHandler.HandleAsync(Arg.Any<GetReadingAvailabilityQuery>(), Arg.Any<CancellationToken>())
            .Returns(Result.From(response));

        // Act
        await _sut.ExecuteAsync(request, cancellationToken);

        // Assert
        await _mockHandler.Received(1).HandleAsync(
            Arg.Is<GetReadingAvailabilityQuery>(query => query.BookId == request.BookId),
            Arg.Is(cancellationToken));
    }

    [Fact]
    public async Task ExecuteAsync_WhenCancellationRequested_ShouldCancelOperation()
    {
        // Arrange
        GetReadingAvailabilityRequest request = _getReadingAvailabilityRequestFixture.Create();
        CancellationTokenSource cts = new();
        TaskCompletionSource<bool> operationStarted = new();
        TaskCompletionSource<bool> cancellationRequested = new();

        _mockHandler.HandleAsync(Arg.Any<GetReadingAvailabilityQuery>(), Arg.Any<CancellationToken>())
            .Returns(info => Task.Run(async () =>
            {
                operationStarted.SetResult(true);
                await cancellationRequested.Task;
                info.Arg<CancellationToken>().ThrowIfCancellationRequested();
                return Result.From(_readingAvailabilityResponseFixture.Create(bookId: request.BookId, isAvailable: true, errorCode: null));
            }, info.Arg<CancellationToken>()));

        // Act
        Task<IResult> operationTask = _sut.ExecuteAsync(request, cts.Token);
        await operationStarted.Task;
        cts.Cancel();
        cancellationRequested.SetResult(true);

        // Assert
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => operationTask);
    }
}
