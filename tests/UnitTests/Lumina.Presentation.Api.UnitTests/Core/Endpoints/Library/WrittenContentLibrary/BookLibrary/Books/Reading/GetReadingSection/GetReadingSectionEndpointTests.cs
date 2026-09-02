#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Application.Common.CQRS;
using Lumina.Application.Core.MediaLibrary.WrittenContentLibrary.BookLibrary.Books.Reading.Queries.GetReadingSection;
using Lumina.Contracts.DTO.MediaLibrary.WrittenContentLibrary.BookLibrary.Reading;
using Lumina.Contracts.Fixtures.Core.DTO.MediaLibrary.WrittenContentLibrary.BookLibrary.Reading;
using Lumina.Contracts.Fixtures.Core.Requests.MediaLibrary.WrittenContentLibrary.BookLibrary.Books.Reading;
using Lumina.Contracts.Requests.MediaLibrary.WrittenContentLibrary.BookLibrary.Books.Reading;
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
    private readonly GetReadingSectionRequestFixture _getReadingSectionRequestFixture = new();
    private readonly ReadingSectionDtoFixture _readingSectionDtoFixture = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="GetReadingSectionEndpointTests"/> class.
    /// </summary>
    public GetReadingSectionEndpointTests()
    {
        _mockHandler = Substitute.For<IQueryHandler<GetReadingSectionQuery, Result<ReadingSectionDto>>>();
        _sut = Factory.Create<GetReadingSectionEndpoint>(_mockHandler);
    }

    [Fact]
    public async Task ExecuteAsync_WhenSuccessful_ShouldReturnOkResultWithReadingSection()
    {
        // Arrange
        GetReadingSectionRequest request = _getReadingSectionRequestFixture.Create();
        CancellationToken cancellationToken = CancellationToken.None;
        ReadingSectionDto expectedResponse = _readingSectionDtoFixture.Create(locationRef: request.LocationRef);
        _mockHandler.HandleAsync(Arg.Any<GetReadingSectionQuery>(), Arg.Any<CancellationToken>())
            .Returns(Result.From(expectedResponse));

        // Act
        IResult result = await _sut.ExecuteAsync(request, cancellationToken);

        // Assert
        Ok<ReadingSectionDto> okResult = Assert.IsType<Ok<ReadingSectionDto>>(result);
        Assert.Equal(expectedResponse, okResult.Value);
    }

    [Fact]
    public async Task ExecuteAsync_WhenHandlerReturnsError_ShouldReturnProblemResult()
    {
        // Arrange
        GetReadingSectionRequest request = _getReadingSectionRequestFixture.Create();
        CancellationToken cancellationToken = CancellationToken.None;
        Error expectedError = Error.NotFound("Reading.SectionNotFound", "SectionNotFound");
        _mockHandler.HandleAsync(Arg.Any<GetReadingSectionQuery>(), Arg.Any<CancellationToken>())
            .Returns(expectedError);

        // Act
        IResult result = await _sut.ExecuteAsync(request, cancellationToken);

        // Assert
        ProblemHttpResult problemResult = Assert.IsType<ProblemHttpResult>(result);
        Assert.Equal(StatusCodes.Status404NotFound, problemResult.StatusCode);
        Assert.Equal("application/problem+json", problemResult.ContentType);
        Assert.Equal("Reading.SectionNotFound", problemResult.ProblemDetails.Title);
        Assert.Equal("SectionNotFound", problemResult.ProblemDetails.Detail);
    }

    [Fact]
    public async Task ExecuteAsync_WhenCalled_ShouldSendGetReadingSectionQueryToSender()
    {
        // Arrange
        GetReadingSectionRequest request = _getReadingSectionRequestFixture.Create();
        CancellationToken cancellationToken = CancellationToken.None;
        ReadingSectionDto response = _readingSectionDtoFixture.Create(locationRef: request.LocationRef);
        _mockHandler.HandleAsync(Arg.Any<GetReadingSectionQuery>(), Arg.Any<CancellationToken>())
            .Returns(Result.From(response));

        // Act
        await _sut.ExecuteAsync(request, cancellationToken);

        // Assert
        await _mockHandler.Received(1).HandleAsync(
            Arg.Is<GetReadingSectionQuery>(query => query.BookId == request.BookId && query.LocationRef == request.LocationRef),
            Arg.Is(cancellationToken));
    }

    [Fact]
    public async Task ExecuteAsync_WhenCancellationRequested_ShouldCancelOperation()
    {
        // Arrange
        GetReadingSectionRequest request = _getReadingSectionRequestFixture.Create();
        CancellationTokenSource cts = new();
        TaskCompletionSource<bool> operationStarted = new();
        TaskCompletionSource<bool> cancellationRequested = new();

        _mockHandler.HandleAsync(Arg.Any<GetReadingSectionQuery>(), Arg.Any<CancellationToken>())
            .Returns(info => Task.Run(async () =>
            {
                operationStarted.SetResult(true);
                await cancellationRequested.Task;
                info.Arg<CancellationToken>().ThrowIfCancellationRequested();
                return Result.From(_readingSectionDtoFixture.Create(locationRef: request.LocationRef));
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
