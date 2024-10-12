#region ========================================================================= USING =====================================================================================
using FluentAssertions;
using Lumina.Application.Core.FileManagement.FileSystem.Queries.GetFileSystem;
using Lumina.Contracts.Enums.FileSystem;
using Lumina.Contracts.Responses.FileManagement;
using Lumina.Presentation.Api.Core.Controllers.FileManagement;
using Mediator;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Api.UnitTests.Core.Controllers.FileManagement;

/// <summary>
/// Contains unit tests for the <see cref="FileSystemController"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class FileSystemControllerTests
{
    private readonly ISender _mockMediator;
    private readonly FileSystemController _sut;

    /// <summary>
    /// Initializes a new instance of the <see cref="FileSystemControllerTests"/> class.
    /// </summary>
    public FileSystemControllerTests()
    {
        _mockMediator = Substitute.For<ISender>();
        _sut = new FileSystemController(_mockMediator);
    }

    [Fact]
    public async Task GetType_WhenCalled_ShouldReturnOkResultWithFileSystemTypeResponse()
    {
        // Arrange
        CancellationToken cancellationToken = CancellationToken.None;
        FileSystemTypeResponse expectedResponse = new(PlatformType.Windows);
        _mockMediator.Send(Arg.Any<GetFileSystemQuery>(), Arg.Any<CancellationToken>())
            .Returns(expectedResponse);

        // Act
        IActionResult result = await _sut.GetType(cancellationToken);

        // Assert
        OkObjectResult okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        FileSystemTypeResponse actualResponse = okResult.Value.Should().BeAssignableTo<FileSystemTypeResponse>().Subject;
        actualResponse.Should().BeEquivalentTo(expectedResponse);
    }

    [Theory]
    [InlineData(PlatformType.Windows)]
    [InlineData(PlatformType.Unix)]
    public async Task GetType_WithDifferentPlatformTypes_ShouldReturnCorrectResponse(PlatformType platformType)
    {
        // Arrange
        CancellationToken cancellationToken = CancellationToken.None;
        FileSystemTypeResponse expectedResponse = new(platformType);
        _mockMediator.Send(Arg.Any<GetFileSystemQuery>(), Arg.Any<CancellationToken>())
            .Returns(expectedResponse);

        // Act
        IActionResult result = await _sut.GetType(cancellationToken);

        // Assert
        OkObjectResult okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        FileSystemTypeResponse actualResponse = okResult.Value.Should().BeAssignableTo<FileSystemTypeResponse>().Subject;
        actualResponse.PlatformType.Should().Be(platformType);
    }

    [Fact]
    public async Task GetType_WhenCalled_ShouldSendGetFileSystemQueryToMediator()
    {
        // Arrange
        CancellationToken cancellationToken = CancellationToken.None;
        _mockMediator.Send(Arg.Any<GetFileSystemQuery>(), Arg.Any<CancellationToken>())
            .Returns(new FileSystemTypeResponse(PlatformType.Windows));

        // Act
        await _sut.GetType(cancellationToken);

        // Assert
        await _mockMediator.Received(1).Send(Arg.Any<GetFileSystemQuery>(), Arg.Is(cancellationToken));
    }

    [Fact]
    public async Task GetType_WhenCancellationRequested_ShouldCancelOperation()
    {
        // Arrange
        CancellationTokenSource cts = new();
        TaskCompletionSource<bool> operationStarted = new();
        TaskCompletionSource<bool> cancellationRequested = new();

        _mockMediator.Send(Arg.Any<GetFileSystemQuery>(), Arg.Any<CancellationToken>())
            .Returns(callInfo => new ValueTask<FileSystemTypeResponse>(Task.Run(async () =>
            {
                operationStarted.SetResult(true);
                await cancellationRequested.Task;
                callInfo.Arg<CancellationToken>().ThrowIfCancellationRequested();
                return new FileSystemTypeResponse(PlatformType.Windows);
            }, callInfo.Arg<CancellationToken>())));

        // Act
        Task<IActionResult> operationTask = _sut.GetType(cts.Token);
        await operationStarted.Task;
        cts.Cancel();
        cancellationRequested.SetResult(true);

        // Assert
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => operationTask);
    }
}
