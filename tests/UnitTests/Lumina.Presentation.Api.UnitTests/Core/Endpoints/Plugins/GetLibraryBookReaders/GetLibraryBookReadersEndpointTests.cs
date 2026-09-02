#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Application.Common.CQRS;
using Lumina.Application.Core.Plugins.Queries.GetLibraryBookReaders;
using Lumina.Contracts.Fixtures.Core.Requests.Plugins;
using Lumina.Contracts.Fixtures.Core.Responses.Plugins;
using Lumina.Contracts.Requests.Plugins;
using Lumina.Contracts.Responses.Plugins;
using Lumina.Domain.Common.Primitives;
using Lumina.Presentation.Api.Core.Endpoints.Plugins.GetLibraryBookReaders;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Api.UnitTests.Core.Endpoints.Plugins.GetLibraryBookReaders;

/// <summary>
/// Contains unit tests for the <see cref="GetLibraryBookReadersEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetLibraryBookReadersEndpointTests
{
    private readonly IQueryHandler<GetLibraryBookReadersQuery, Result<IReadOnlyList<LibraryBookReaderResponse>>> _mockHandler;
    private readonly GetLibraryBookReadersEndpoint _sut;
    private readonly GetLibraryBookReadersRequestFixture _getLibraryBookReadersRequestFixture = new();
    private readonly LibraryBookReaderResponseFixture _libraryBookReaderResponseFixture = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="GetLibraryBookReadersEndpointTests"/> class.
    /// </summary>
    public GetLibraryBookReadersEndpointTests()
    {
        _mockHandler = Substitute.For<IQueryHandler<GetLibraryBookReadersQuery, Result<IReadOnlyList<LibraryBookReaderResponse>>>>();
        _sut = Factory.Create<GetLibraryBookReadersEndpoint>(_mockHandler);
    }

    [Fact]
    public async Task ExecuteAsync_WhenSuccessful_ShouldReturnOkResultWithBookReaders()
    {
        // Arrange
        GetLibraryBookReadersRequest request = _getLibraryBookReadersRequestFixture.Create();
        CancellationToken cancellationToken = CancellationToken.None;
        IReadOnlyList<LibraryBookReaderResponse> expectedResponse = _libraryBookReaderResponseFixture.CreateMany(2);
        _mockHandler.HandleAsync(Arg.Any<GetLibraryBookReadersQuery>(), Arg.Any<CancellationToken>())
            .Returns(Result.From(expectedResponse));

        // Act
        IResult result = await _sut.ExecuteAsync(request, cancellationToken);

        // Assert
        Ok<IReadOnlyList<LibraryBookReaderResponse>> okResult = Assert.IsType<Ok<IReadOnlyList<LibraryBookReaderResponse>>>(result);
        Assert.Equal(expectedResponse, okResult.Value);
    }

    [Fact]
    public async Task ExecuteAsync_WhenHandlerReturnsError_ShouldReturnProblemResult()
    {
        // Arrange
        GetLibraryBookReadersRequest request = _getLibraryBookReadersRequestFixture.Create();
        CancellationToken cancellationToken = CancellationToken.None;
        Error expectedError = Error.Failure("Authorization.NotAuthorized", "NotAuthorized");
        _mockHandler.HandleAsync(Arg.Any<GetLibraryBookReadersQuery>(), Arg.Any<CancellationToken>())
            .Returns(expectedError);

        // Act
        IResult result = await _sut.ExecuteAsync(request, cancellationToken);

        // Assert
        ProblemHttpResult problemResult = Assert.IsType<ProblemHttpResult>(result);
        Assert.Equal(StatusCodes.Status403Forbidden, problemResult.StatusCode);
        Assert.Equal("application/problem+json", problemResult.ContentType);
        Assert.Equal("Authorization.NotAuthorized", problemResult.ProblemDetails.Title);
        Assert.Equal("NotAuthorized", problemResult.ProblemDetails.Detail);
    }

    [Fact]
    public async Task ExecuteAsync_WhenCalled_ShouldSendGetLibraryBookReadersQueryToSender()
    {
        // Arrange
        GetLibraryBookReadersRequest request = _getLibraryBookReadersRequestFixture.Create();
        CancellationToken cancellationToken = CancellationToken.None;
        IReadOnlyList<LibraryBookReaderResponse> response = _libraryBookReaderResponseFixture.CreateMany(2);
        _mockHandler.HandleAsync(Arg.Any<GetLibraryBookReadersQuery>(), Arg.Any<CancellationToken>())
            .Returns(Result.From(response));

        // Act
        await _sut.ExecuteAsync(request, cancellationToken);

        // Assert
        await _mockHandler.Received(1).HandleAsync(
            Arg.Is<GetLibraryBookReadersQuery>(query => query.LibraryId == request.LibraryId),
            Arg.Is(cancellationToken));
    }

    [Fact]
    public async Task ExecuteAsync_WhenCancellationRequested_ShouldCancelOperation()
    {
        // Arrange
        GetLibraryBookReadersRequest request = _getLibraryBookReadersRequestFixture.Create();
        CancellationTokenSource cts = new();
        TaskCompletionSource<bool> operationStarted = new();
        TaskCompletionSource<bool> cancellationRequested = new();

        _mockHandler.HandleAsync(Arg.Any<GetLibraryBookReadersQuery>(), Arg.Any<CancellationToken>())
            .Returns(info => Task.Run(async () =>
            {
                operationStarted.SetResult(true);
                await cancellationRequested.Task;
                info.Arg<CancellationToken>().ThrowIfCancellationRequested();
                return Result.From<IReadOnlyList<LibraryBookReaderResponse>>([]);
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
