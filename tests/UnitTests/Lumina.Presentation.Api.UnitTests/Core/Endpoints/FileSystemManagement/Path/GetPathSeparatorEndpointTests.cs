#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Application.Common.CQRS;
using Lumina.Application.Core.FileSystemManagement.Paths.Queries.GetPathSeparator;
using Lumina.Contracts.Responses.FileSystemManagement.Path;
using Lumina.Presentation.Api.Core.Endpoints.FileSystemManagement.Path.GetPathSeparator;
using Lumina.Presentation.Api.UnitTests.Core.Endpoints.FileSystemManagement.Fixtures;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using NSubstitute;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Api.UnitTests.Core.Endpoints.FileSystemManagement.Path;

/// <summary>
/// Contains unit tests for the <see cref="GetPathSeparatorEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetPathSeparatorEndpointTests
{
    private readonly IQueryHandler<GetPathSeparatorQuery, PathSeparatorResponse> _mockHandler;
    private readonly GetPathSeparatorEndpoint _sut;
    private readonly PathSeparatorResponseFixture _pathSeparatorResponseFixture;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetPathSeparatorEndpointTests"/> class.
    /// </summary>
    public GetPathSeparatorEndpointTests()
    {
        _mockHandler = Substitute.For<IQueryHandler<GetPathSeparatorQuery, PathSeparatorResponse>>();
        _sut = Factory.Create<GetPathSeparatorEndpoint>(_mockHandler);
        _pathSeparatorResponseFixture = new PathSeparatorResponseFixture();
    }

    [Fact]
    public async Task ExecuteAsync_WhenCalled_ShouldReturnOkResultWithPathSeparatorResponse()
    {
        // Arrange
        CancellationToken cancellationToken = CancellationToken.None;
        PathSeparatorResponse expectedResponse = _pathSeparatorResponseFixture.Create();
        _mockHandler.HandleAsync(Arg.Any<GetPathSeparatorQuery>(), Arg.Any<CancellationToken>())
            .Returns(expectedResponse);

        // Act
        IResult result = await _sut.ExecuteAsync(cancellationToken);

        // Assert
        Ok<PathSeparatorResponse> okResult = Assert.IsType<Ok<PathSeparatorResponse>>(result);
        Assert.Equal(expectedResponse, okResult.Value);
    }

    [Fact]
    public async Task ExecuteAsync_WhenCalled_ShouldSendGetPathSeparatorQueryToMediator()
    {
        // Arrange
        CancellationToken cancellationToken = CancellationToken.None;
        _mockHandler.HandleAsync(Arg.Any<GetPathSeparatorQuery>(), Arg.Any<CancellationToken>())
            .Returns(_pathSeparatorResponseFixture.Create());

        // Act
        await _sut.ExecuteAsync(cancellationToken);

        // Assert
        await _mockHandler.Received(1).HandleAsync(Arg.Any<GetPathSeparatorQuery>(), Arg.Is(cancellationToken));
    }

    [Fact]
    public async Task ExecuteAsync_WhenCancellationRequested_ShouldCancelOperation()
    {
        // Arrange
        CancellationTokenSource cts = new();
        TaskCompletionSource<bool> operationStarted = new();
        TaskCompletionSource<bool> cancellationRequested = new();

        _mockHandler.HandleAsync(Arg.Any<GetPathSeparatorQuery>(), Arg.Any<CancellationToken>())
            .Returns(callInfo => Task.Run(async () =>
            {
                operationStarted.SetResult(true);
                await cancellationRequested.Task;
                callInfo.Arg<CancellationToken>().ThrowIfCancellationRequested();
                return _pathSeparatorResponseFixture.Create();
            }, callInfo.Arg<CancellationToken>()));

        // Act
        Task<IResult> operationTask = _sut.ExecuteAsync(cts.Token);
        await operationStarted.Task;
        cts.Cancel();
        cancellationRequested.SetResult(true);

        // Assert
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => operationTask);
    }
}
