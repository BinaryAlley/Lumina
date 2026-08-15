#region ========================================================================= USING =====================================================================================
using AutoFixture;
using AutoFixture.AutoNSubstitute;
using Lumina.Application.Core.FileSystemManagement.FileSystem.Queries.GetFileSystem;
using Lumina.Application.Fixtures.Core.FileSystemManagement.FileSystem.Queries.GetFileSystem;
using Lumina.Contracts.Responses.FileSystemManagement.FileSystem;
using Lumina.Domain.Core.BoundedContexts.FileSystemManagementBoundedContext.FileSystemManagementAggregate.Strategies.Platform;
using Lumina.Domain.SharedKernel.Common.Enums.FileSystem;
using NSubstitute;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Application.UnitTests.Core.FileSystemManagement.FileSystem.Queries.GetFileSystem;

/// <summary>
/// Contains unit tests for the <see cref="GetFileSystemQueryHandler"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetFileSystemQueryHandlerTests
{
    private readonly IFixture _fixture;
    private readonly IPlatformContextManager _mockPlatformContextManager;
    private readonly IPlatformContext _mockPlatformContext;
    private readonly GetFileSystemQueryHandler _sut;
    private readonly GetFileSystemQueryFixture _getFileSystemQueryFixture;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetFileSystemQueryHandlerTests"/> class.
    /// </summary>
    public GetFileSystemQueryHandlerTests()
    {
        _fixture = new Fixture().Customize(new AutoNSubstituteCustomization());
        _mockPlatformContextManager = Substitute.For<IPlatformContextManager>();
        _mockPlatformContext = Substitute.For<IPlatformContext>();
        _mockPlatformContextManager.GetCurrentContext().Returns(_mockPlatformContext);
        _getFileSystemQueryFixture = new GetFileSystemQueryFixture();
        _sut = new GetFileSystemQueryHandler(_mockPlatformContextManager);
    }

    [Theory]
    [InlineData(PlatformType.Unix)]
    [InlineData(PlatformType.Windows)]
    public async Task HandleAsync_WhenCalled_ShouldReturnCorrectFileSystemTypeResponse(PlatformType platformType)
    {
        // Arrange
        GetFileSystemQuery query = _getFileSystemQueryFixture.Create();
        _mockPlatformContext.Platform.Returns(platformType);

        // Act
        FileSystemTypeResponse result = await _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(platformType, result.PlatformType);
        _mockPlatformContextManager.Received(1).GetCurrentContext();
    }

    [Fact]
    public async Task HandleAsync_WhenCalled_ShouldReturnValueTaskResult()
    {
        // Arrange
        GetFileSystemQuery query = _getFileSystemQueryFixture.Create();
        _mockPlatformContext.Platform.Returns(PlatformType.Unix);

        // Act
        Task<FileSystemTypeResponse> resultTask = _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        Assert.True(resultTask.IsCompleted);
        FileSystemTypeResponse result = await resultTask;
        Assert.NotNull(result);
        Assert.Equal(PlatformType.Unix, result.PlatformType);
    }

    [Fact]
    public async Task HandleAsync_WhenCalled_ShouldNotDependOnQueryContent()
    {
        // Arrange
        GetFileSystemQuery query1 = _getFileSystemQueryFixture.Create();
        GetFileSystemQuery query2 = _getFileSystemQueryFixture.Create();
        _mockPlatformContext.Platform.Returns(PlatformType.Windows);

        // Act
        FileSystemTypeResponse result1 = await _sut.HandleAsync(query1, CancellationToken.None);
        FileSystemTypeResponse result2 = await _sut.HandleAsync(query2, CancellationToken.None);

        // Assert
        Assert.Equal(result1.PlatformType, result2.PlatformType);
        _mockPlatformContextManager.Received(1).GetCurrentContext();
    }

    [Fact]
    public async Task HandleAsync_WhenCalled_ShouldIgnoreCancellationToken()
    {
        // Arrange
        GetFileSystemQuery query = _getFileSystemQueryFixture.Create();
        _mockPlatformContext.Platform.Returns(PlatformType.Unix);
        CancellationToken cancellationToken = new(true);

        // Act & Assert
        Exception? exception = await Record.ExceptionAsync(async () =>
            await _sut.HandleAsync(query, cancellationToken));
        Assert.Null(exception);
        _mockPlatformContextManager.Received(1).GetCurrentContext();
    }
}
