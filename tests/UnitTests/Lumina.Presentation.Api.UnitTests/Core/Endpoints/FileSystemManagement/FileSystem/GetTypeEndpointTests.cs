#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Application.Core.FileSystemManagement.FileSystem.Queries.GetFileSystem;
using Lumina.Domain.Common.Enums.FileSystem;
using Lumina.Contracts.Responses.FileSystemManagement.FileSystem;
using Lumina.Presentation.Api.Core.Endpoints.FileSystemManagement.FileSystem.GetType;
using Mediator;
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
public class GetFileSystemTypeEndpointTests
{
    private readonly ISender _mockSender;
    private readonly GetTypeEndpoint _sut;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetFileSystemTypeEndpointTests"/> class.
    /// </summary>
    public GetFileSystemTypeEndpointTests()
    {
        _mockSender = Substitute.For<ISender>();
        _sut = Factory.Create<GetTypeEndpoint>(_mockSender);
    }

    [Fact]
    public async Task ExecuteAsync_WhenCalled_ShouldReturnOkResultWithFileSystemTypeResponse()
    {
        // Arrange
        CancellationToken cancellationToken = CancellationToken.None;
        FileSystemTypeResponse expectedResponse = new(PlatformType.Windows);
        _mockSender.Send(Arg.Any<GetFileSystemQuery>(), Arg.Any<CancellationToken>())
            .Returns(expectedResponse);

        // Act
        IResult result = await _sut.ExecuteAsync(new EmptyRequest(), cancellationToken);

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
        FileSystemTypeResponse expectedResponse = new(platformType);
        _mockSender.Send(Arg.Any<GetFileSystemQuery>(), Arg.Any<CancellationToken>())
            .Returns(expectedResponse);

        // Act
        IResult result = await _sut.ExecuteAsync(new EmptyRequest(), cancellationToken);

        // Assert
        Ok<FileSystemTypeResponse> okResult = Assert.IsType<Ok<FileSystemTypeResponse>>(result);
        Assert.Equal(platformType, okResult.Value!.PlatformType);
    }

    [Fact]
    public async Task ExecuteAsync_WhenCalled_ShouldSendGetFileSystemQueryToMediator()
    {
        // Arrange
        CancellationToken cancellationToken = CancellationToken.None;
        _mockSender.Send(Arg.Any<GetFileSystemQuery>(), Arg.Any<CancellationToken>())
            .Returns(new FileSystemTypeResponse(PlatformType.Windows));

        // Act
        await _sut.ExecuteAsync(new EmptyRequest(), cancellationToken);

        // Assert
        await _mockSender.Received(1).Send(Arg.Any<GetFileSystemQuery>(), Arg.Is(cancellationToken));
    }

    [Fact]
    public async Task ExecuteAsync_WhenCancellationRequested_ShouldCancelOperation()
    {
        // Arrange
        CancellationTokenSource cts = new();
        TaskCompletionSource<bool> operationStarted = new();
        TaskCompletionSource<bool> cancellationRequested = new();

        _mockSender.Send(Arg.Any<GetFileSystemQuery>(), Arg.Any<CancellationToken>())
            .Returns(callInfo => new ValueTask<FileSystemTypeResponse>(Task.Run(async () =>
            {
                operationStarted.SetResult(true);
                await cancellationRequested.Task;
                callInfo.Arg<CancellationToken>().ThrowIfCancellationRequested();
                return new FileSystemTypeResponse(PlatformType.Windows);
            }, callInfo.Arg<CancellationToken>())));

        // Act
        Task<IResult> operationTask = _sut.ExecuteAsync(new EmptyRequest(), cts.Token);
        await operationStarted.Task;
        cts.Cancel();
        cancellationRequested.SetResult(true);

        // Assert
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => operationTask);
    }
}
