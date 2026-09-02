#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Application.Common.CQRS;
using Lumina.Application.Core.MediaLibrary.WrittenContentLibrary.BookLibrary.Books.Reading.Queries.GetReadingResource;
using Lumina.Contracts.DTO.MediaLibrary.WrittenContentLibrary.BookLibrary.Reading;
using Lumina.Contracts.Fixtures.Core.DTO.MediaLibrary.WrittenContentLibrary.BookLibrary.Reading;
using Lumina.Contracts.Fixtures.Core.Requests.MediaLibrary.WrittenContentLibrary.BookLibrary.Books.Reading;
using Lumina.Contracts.Requests.MediaLibrary.WrittenContentLibrary.BookLibrary.Books.Reading;
using Lumina.Domain.Common.Primitives;
using Lumina.Presentation.Api.Core.Endpoints.Library.WrittenContentLibrary.BookLibrary.Books.Reading.GetReadingResource;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using NSubstitute;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Api.UnitTests.Core.Endpoints.Library.WrittenContentLibrary.BookLibrary.Books.Reading.GetReadingResource;

/// <summary>
/// Contains unit tests for the <see cref="GetReadingResourceEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetReadingResourceEndpointTests
{
    private readonly IQueryHandler<GetReadingResourceQuery, Result<ReadingResourceDataDto>> _mockHandler;
    private readonly GetReadingResourceEndpoint _sut;
    private readonly GetReadingResourceRequestFixture _getReadingResourceRequestFixture = new();
    private readonly ReadingResourceDataDtoFixture _readingResourceDataDtoFixture = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="GetReadingResourceEndpointTests"/> class.
    /// </summary>
    public GetReadingResourceEndpointTests()
    {
        _mockHandler = Substitute.For<IQueryHandler<GetReadingResourceQuery, Result<ReadingResourceDataDto>>>();
        _sut = Factory.Create<GetReadingResourceEndpoint>(_mockHandler);
    }

    [Fact]
    public async Task ExecuteAsync_WhenSuccessful_ShouldReturnBytesResultWithResourceData()
    {
        // Arrange
        GetReadingResourceRequest request = _getReadingResourceRequestFixture.Create();
        CancellationToken cancellationToken = CancellationToken.None;
        ReadingResourceDataDto expectedResponse = _readingResourceDataDtoFixture.Create();
        _mockHandler.HandleAsync(Arg.Any<GetReadingResourceQuery>(), Arg.Any<CancellationToken>())
            .Returns(Result.From(expectedResponse));

        // Act
        IResult result = await _sut.ExecuteAsync(request, cancellationToken);

        // Assert
        FileContentHttpResult bytesResult = Assert.IsType<FileContentHttpResult>(result);
        Assert.Equal(expectedResponse.Data, bytesResult.FileContents);
        Assert.Equal(expectedResponse.MimeType, bytesResult.ContentType);
    }

    [Fact]
    public async Task ExecuteAsync_WhenHandlerReturnsError_ShouldReturnProblemResult()
    {
        // Arrange
        GetReadingResourceRequest request = _getReadingResourceRequestFixture.Create();
        CancellationToken cancellationToken = CancellationToken.None;
        Error expectedError = Error.NotFound("Reading.ResourceNotFound", "ResourceNotFound");
        _mockHandler.HandleAsync(Arg.Any<GetReadingResourceQuery>(), Arg.Any<CancellationToken>())
            .Returns(expectedError);

        // Act
        IResult result = await _sut.ExecuteAsync(request, cancellationToken);

        // Assert
        ProblemHttpResult problemResult = Assert.IsType<ProblemHttpResult>(result);
        Assert.Equal(StatusCodes.Status404NotFound, problemResult.StatusCode);
        Assert.Equal("application/problem+json", problemResult.ContentType);
        Assert.Equal("Reading.ResourceNotFound", problemResult.ProblemDetails.Title);
        Assert.Equal("ResourceNotFound", problemResult.ProblemDetails.Detail);
    }

    [Fact]
    public async Task ExecuteAsync_WhenCalled_ShouldSendGetReadingResourceQueryToSender()
    {
        // Arrange
        GetReadingResourceRequest request = _getReadingResourceRequestFixture.Create();
        CancellationToken cancellationToken = CancellationToken.None;
        ReadingResourceDataDto response = _readingResourceDataDtoFixture.Create();
        _mockHandler.HandleAsync(Arg.Any<GetReadingResourceQuery>(), Arg.Any<CancellationToken>())
            .Returns(Result.From(response));

        // Act
        await _sut.ExecuteAsync(request, cancellationToken);

        // Assert
        await _mockHandler.Received(1).HandleAsync(
            Arg.Is<GetReadingResourceQuery>(query => query.BookId == request.BookId && query.ResourceKey == request.ResourceKey),
            Arg.Is(cancellationToken));
    }

    [Fact]
    public async Task ExecuteAsync_WhenCancellationRequested_ShouldCancelOperation()
    {
        // Arrange
        GetReadingResourceRequest request = _getReadingResourceRequestFixture.Create();
        CancellationTokenSource cts = new();
        TaskCompletionSource<bool> operationStarted = new();
        TaskCompletionSource<bool> cancellationRequested = new();

        _mockHandler.HandleAsync(Arg.Any<GetReadingResourceQuery>(), Arg.Any<CancellationToken>())
            .Returns(info => Task.Run(async () =>
            {
                operationStarted.SetResult(true);
                await cancellationRequested.Task;
                info.Arg<CancellationToken>().ThrowIfCancellationRequested();
                return Result.From(_readingResourceDataDtoFixture.Create());
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
