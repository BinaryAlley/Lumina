#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Application.Common.CQRS;
using Lumina.Application.Core.MediaLibrary.WrittenContentLibrary.BookLibrary.Books.Reading.Queries.GetReadingManifest;
using Lumina.Contracts.Fixtures.Core.Requests.MediaLibrary.WrittenContentLibrary.BookLibrary.Books.Reading;
using Lumina.Contracts.Fixtures.Core.Responses.MediaLibrary.WrittenContentLibrary.BookLibrary.Books.Reading;
using Lumina.Contracts.Requests.MediaLibrary.WrittenContentLibrary.BookLibrary.Books.Reading;
using Lumina.Contracts.Responses.MediaLibrary.WrittenContentLibrary.BookLibrary.Books.Reading;
using Lumina.Domain.Common.Primitives;
using Lumina.Presentation.Api.Core.Endpoints.Library.WrittenContentLibrary.BookLibrary.Books.Reading.GetReadingManifest;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using NSubstitute;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Api.UnitTests.Core.Endpoints.Library.WrittenContentLibrary.BookLibrary.Books.Reading.GetReadingManifest;

/// <summary>
/// Contains unit tests for the <see cref="GetReadingManifestEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetReadingManifestEndpointTests
{
    private readonly IQueryHandler<GetReadingManifestQuery, Result<ReadingManifestResponse>> _mockHandler;
    private readonly GetReadingManifestEndpoint _sut;
    private readonly GetReadingManifestRequestFixture _getReadingManifestRequestFixture = new();
    private readonly ReadingManifestResponseFixture _readingManifestResponseFixture = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="GetReadingManifestEndpointTests"/> class.
    /// </summary>
    public GetReadingManifestEndpointTests()
    {
        _mockHandler = Substitute.For<IQueryHandler<GetReadingManifestQuery, Result<ReadingManifestResponse>>>();
        _sut = Factory.Create<GetReadingManifestEndpoint>(_mockHandler);
    }

    [Fact]
    public async Task ExecuteAsync_WhenSuccessful_ShouldReturnOkResultWithReadingManifest()
    {
        // Arrange
        GetReadingManifestRequest request = _getReadingManifestRequestFixture.Create();
        CancellationToken cancellationToken = CancellationToken.None;
        ReadingManifestResponse expectedResponse = _readingManifestResponseFixture.Create();
        _mockHandler.HandleAsync(Arg.Any<GetReadingManifestQuery>(), Arg.Any<CancellationToken>())
            .Returns(Result.From(expectedResponse));

        // Act
        IResult result = await _sut.ExecuteAsync(request, cancellationToken);

        // Assert
        Ok<ReadingManifestResponse> okResult = Assert.IsType<Ok<ReadingManifestResponse>>(result);
        Assert.Equal(expectedResponse, okResult.Value);
    }

    [Fact]
    public async Task ExecuteAsync_WhenHandlerReturnsError_ShouldReturnProblemResult()
    {
        // Arrange
        GetReadingManifestRequest request = _getReadingManifestRequestFixture.Create();
        CancellationToken cancellationToken = CancellationToken.None;
        Error expectedError = Error.NotFound("Reading.BookNotFound", "BookNotFound");
        _mockHandler.HandleAsync(Arg.Any<GetReadingManifestQuery>(), Arg.Any<CancellationToken>())
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
    public async Task ExecuteAsync_WhenCalled_ShouldSendGetReadingManifestQueryToSender()
    {
        // Arrange
        GetReadingManifestRequest request = _getReadingManifestRequestFixture.Create();
        CancellationToken cancellationToken = CancellationToken.None;
        ReadingManifestResponse response = _readingManifestResponseFixture.Create();
        _mockHandler.HandleAsync(Arg.Any<GetReadingManifestQuery>(), Arg.Any<CancellationToken>())
            .Returns(Result.From(response));

        // Act
        await _sut.ExecuteAsync(request, cancellationToken);

        // Assert
        await _mockHandler.Received(1).HandleAsync(
            Arg.Is<GetReadingManifestQuery>(query => query.BookId == request.BookId),
            Arg.Is(cancellationToken));
    }

    [Fact]
    public async Task ExecuteAsync_WhenCancellationRequested_ShouldCancelOperation()
    {
        // Arrange
        GetReadingManifestRequest request = _getReadingManifestRequestFixture.Create();
        CancellationTokenSource cts = new();
        TaskCompletionSource<bool> operationStarted = new();
        TaskCompletionSource<bool> cancellationRequested = new();

        _mockHandler.HandleAsync(Arg.Any<GetReadingManifestQuery>(), Arg.Any<CancellationToken>())
            .Returns(info => Task.Run(async () =>
            {
                operationStarted.SetResult(true);
                await cancellationRequested.Task;
                info.Arg<CancellationToken>().ThrowIfCancellationRequested();
                return Result.From(_readingManifestResponseFixture.Create());
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
