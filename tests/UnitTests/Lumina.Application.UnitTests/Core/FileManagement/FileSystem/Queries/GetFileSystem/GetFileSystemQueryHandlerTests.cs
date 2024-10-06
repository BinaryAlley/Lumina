#region ========================================================================= USING =====================================================================================
using AutoFixture;
using AutoFixture.AutoNSubstitute;
using FluentAssertions;
using Lumina.Application.Core.FileManagement.FileSystem.Queries.GetFileSystem;
using Lumina.Contracts.Enums.FileSystem;
using Lumina.Contracts.Responses.FileManagement;
using Lumina.Domain.Core.Aggregates.FileManagement.FileManagementAggregate.Strategies.Platform;
using NSubstitute;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Application.UnitTests.Core.FileManagement.FileSystem.Queries.GetFileSystem;

/// <summary>
/// Contains unit tests for the <see cref="GetFileSystemQueryHandler"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetFileSystemQueryHandlerTests
{
    #region ================================================================== FIELD MEMBERS ================================================================================
    private readonly IFixture _fixture;
    private readonly IPlatformContextManager _mockPlatformContextManager;
    private readonly IPlatformContext _mockPlatformContext;
    private readonly GetFileSystemQueryHandler _sut;
    #endregion

    #region ====================================================================== CTOR =====================================================================================
    /// <summary>
    /// Initializes a new instance of the <see cref="GetFileSystemQueryHandlerTests"/> class.
    /// </summary>
    public GetFileSystemQueryHandlerTests()
    {
        _fixture = new Fixture().Customize(new AutoNSubstituteCustomization());
        _mockPlatformContextManager = Substitute.For<IPlatformContextManager>();
        _mockPlatformContext = Substitute.For<IPlatformContext>();
        _mockPlatformContextManager.GetCurrentContext().Returns(_mockPlatformContext);
        _sut = new GetFileSystemQueryHandler(_mockPlatformContextManager);
    }
    #endregion

    #region ===================================================================== METHODS ===================================================================================
    [Theory]
    [InlineData(PlatformType.Unix)]
    [InlineData(PlatformType.Windows)]
    public async Task Handle_WhenCalled_ShouldReturnCorrectFileSystemTypeResponse(PlatformType platformType)
    {
        // Arrange
        GetFileSystemQuery query = _fixture.Create<GetFileSystemQuery>();
        _mockPlatformContext.Platform.Returns(platformType);

        // Act
        FileSystemTypeResponse result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.PlatformType.Should().Be(platformType);
        _mockPlatformContextManager.Received(1).GetCurrentContext();
    }

    [Fact]
    public async Task Handle_WhenCalled_ShouldReturnValueTaskResult()
    {
        // Arrange
        GetFileSystemQuery query = _fixture.Create<GetFileSystemQuery>();
        _mockPlatformContext.Platform.Returns(PlatformType.Unix);

        // Act
        ValueTask<FileSystemTypeResponse> resultTask = _sut.Handle(query, CancellationToken.None);

        // Assert
        resultTask.IsCompleted.Should().BeTrue();
        FileSystemTypeResponse result = await resultTask;
        result.Should().NotBeNull();
        result.PlatformType.Should().Be(PlatformType.Unix);
    }

    [Fact]
    public async Task Handle_WhenCalled_ShouldNotDependOnQueryContent()
    {
        // Arrange
        GetFileSystemQuery query1 = _fixture.Create<GetFileSystemQuery>();
        GetFileSystemQuery query2 = _fixture.Create<GetFileSystemQuery>();
        _mockPlatformContext.Platform.Returns(PlatformType.Windows);

        // Act
        FileSystemTypeResponse result1 = await _sut.Handle(query1, CancellationToken.None);
        FileSystemTypeResponse result2 = await _sut.Handle(query2, CancellationToken.None);

        // Assert
        result1.Should().BeEquivalentTo(result2);
        _mockPlatformContextManager.Received(1).GetCurrentContext();
    }

    [Fact]
    public async Task Handle_WhenCalled_ShouldIgnoreCancellationToken()
    {
        // Arrange
        GetFileSystemQuery query = _fixture.Create<GetFileSystemQuery>();
        _mockPlatformContext.Platform.Returns(PlatformType.Unix);
        CancellationToken cancellationToken = new(true);

        // Act
        Func<Task> act = async () => await _sut.Handle(query, cancellationToken);

        // Assert
        await act.Should().NotThrowAsync();
        _mockPlatformContextManager.Received(1).GetCurrentContext();
    }
    #endregion
}