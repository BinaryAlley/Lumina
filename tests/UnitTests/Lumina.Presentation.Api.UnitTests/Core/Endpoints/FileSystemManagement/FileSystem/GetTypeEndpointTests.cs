#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Application.Common.CQRS;
using Lumina.Application.Core.FileSystemManagement.FileSystem.Queries.GetFileSystem;
using Lumina.Contracts.Fixtures.Core.Responses.FileSystemManagement.FileSystem;
using Lumina.Contracts.Responses.FileSystemManagement.FileSystem;
using Lumina.Domain.SharedKernel.Common.Enums.FileSystem;
using Lumina.Presentation.Api.Core.Endpoints.FileSystemManagement.FileSystem.GetType;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using NSubstitute;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Api.UnitTests.Core.Endpoints.FileSystemManagement.FileSystem;

/// <summary>
/// Contains unit tests for the <see cref="GetTypeEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetTypeEndpointTests
{
    private readonly IQueryHandler<GetFileSystemQuery, FileSystemTypeResponse> _mockHandler;
    private readonly GetTypeEndpoint _sut;
    private readonly FileSystemTypeResponseFixture _fileSystemTypeResponseFixture = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="GetTypeEndpointTests"/> class.
    /// </summary>
    public GetTypeEndpointTests()
    {
        _mockHandler = Substitute.For<IQueryHandler<GetFileSystemQuery, FileSystemTypeResponse>>();
        _sut = Factory.Create<GetTypeEndpoint>(_mockHandler);
    }

    [Fact]
    public async Task ExecuteAsync_WhenCalled_ShouldReturnOkResultWithFileSystemTypeResponse()
    {
        // Arrange
        CancellationToken cancellationToken = CancellationToken.None;
        FileSystemTypeResponse expectedResponse = _fileSystemTypeResponseFixture.Create(platformType: PlatformType.Windows);
        _mockHandler.HandleAsync(Arg.Any<GetFileSystemQuery>(), Arg.Any<CancellationToken>())
            .Returns(expectedResponse);

        // Act
        IResult result = await _sut.ExecuteAsync(EmptyRequest.Instance, cancellationToken);

        // Assert
        Ok<FileSystemTypeResponse> okResult = Assert.IsType<Ok<FileSystemTypeResponse>>(result);
        Assert.Equal(expectedResponse, okResult.Value);
    }

    [Theory]
    [InlineData(PlatformType.Windows)]
    [InlineData(PlatformType.Unix)]
    public async Task ExecuteAsync_WithDifferentPlatformTypes_ShouldReturnCorrectResponse(PlatformType platformType)
    {
        // Arrange
        CancellationToken cancellationToken = CancellationToken.None;
        FileSystemTypeResponse expectedResponse = _fileSystemTypeResponseFixture.Create(platformType: platformType);
        _mockHandler.HandleAsync(Arg.Any<GetFileSystemQuery>(), Arg.Any<CancellationToken>())
            .Returns(expectedResponse);

        // Act
        IResult result = await _sut.ExecuteAsync(EmptyRequest.Instance, cancellationToken);

        // Assert
        Ok<FileSystemTypeResponse> okResult = Assert.IsType<Ok<FileSystemTypeResponse>>(result);
        Assert.Equal(platformType, okResult.Value!.PlatformType);
    }

    [Fact]
    public async Task ExecuteAsync_WhenCalled_ShouldSendGetFileSystemQueryToHandler()
    {
        // Arrange
        CancellationToken cancellationToken = CancellationToken.None;
        _mockHandler.HandleAsync(Arg.Any<GetFileSystemQuery>(), Arg.Any<CancellationToken>())
            .Returns(_fileSystemTypeResponseFixture.Create(platformType: PlatformType.Windows));

        // Act
        await _sut.ExecuteAsync(EmptyRequest.Instance, cancellationToken);

        // Assert
        await _mockHandler.Received(1).HandleAsync(Arg.Any<GetFileSystemQuery>(), Arg.Is(cancellationToken));
    }

    [Fact]
    public async Task ExecuteAsync_WhenCancellationRequested_ShouldCancelOperation()
    {
        // Arrange
        CancellationTokenSource cts = new();
        TaskCompletionSource<bool> operationStarted = new();
        TaskCompletionSource<bool> cancellationRequested = new();

        _mockHandler.HandleAsync(Arg.Any<GetFileSystemQuery>(), Arg.Any<CancellationToken>())
            .Returns(callInfo => Task.Run(async () =>
            {
                operationStarted.SetResult(true);
                await cancellationRequested.Task;
                callInfo.Arg<CancellationToken>().ThrowIfCancellationRequested();
                return _fileSystemTypeResponseFixture.Create(platformType: PlatformType.Windows);
            }, callInfo.Arg<CancellationToken>()));

        // Act
        Task<IResult> operationTask = _sut.ExecuteAsync(EmptyRequest.Instance, cts.Token);
        await operationStarted.Task;
        cts.Cancel();
        cancellationRequested.SetResult(true);

        // Assert
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => operationTask);
    }
}
