#region ========================================================================= USING =====================================================================================
using ErrorOr;
using FluentAssertions;
using Lumina.Domain.Common.Enums.FileSystem;
using Lumina.Domain.Core.BoundedContexts.FileSystemManagementBoundedContext.FileSystemManagementAggregate;
using Lumina.Domain.Core.BoundedContexts.FileSystemManagementBoundedContext.FileSystemManagementAggregate.Entities;
using Lumina.Domain.Core.BoundedContexts.FileSystemManagementBoundedContext.FileSystemManagementAggregate.Services;
using Lumina.Domain.Core.BoundedContexts.FileSystemManagementBoundedContext.FileSystemManagementAggregate.Strategies.Platform;
using NSubstitute;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO.Abstractions;
using System.Linq;
#endregion

namespace Lumina.Domain.UnitTests.Core.Aggregates.FileSystemManagementAggregate.Services;

/// <summary>
/// Contains unit tests for the <see cref="DriveService"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class DriveServiceTests
{
    private readonly IFileSystem _mockFileSystem;
    private readonly IPlatformContextManager _mockPlatformContextManager;
    private readonly IPlatformContext _mockPlatformContext;
    private readonly DriveService _sut;

    /// <summary>
    /// Initializes a new instance of the <see cref="DriveServiceTests"/> class.
    /// </summary>
    public DriveServiceTests()
    {
        _mockFileSystem = Substitute.For<IFileSystem>();
        _mockPlatformContextManager = Substitute.For<IPlatformContextManager>();
        _mockPlatformContext = Substitute.For<IPlatformContext>();
        _mockPlatformContextManager.GetCurrentContext().Returns(_mockPlatformContext);
        _sut = new DriveService(_mockFileSystem, _mockPlatformContextManager);
    }

    [Fact]
    public void GetDrives_OnUnixPlatform_ShouldReturnSingleUnixRootItem()
    {
        // Arrange
        _mockPlatformContext.Platform.Returns(PlatformType.Unix);

        // Act
        ErrorOr<IEnumerable<FileSystemItem>> result = _sut.GetDrives();

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Should().HaveCount(1);
        result.Value.First().Should().BeOfType<UnixRootItem>();
        result.Value.First().Status.Should().Be(FileSystemItemStatus.Accessible);
    }

    [Fact]
    public void GetDrives_OnWindowsPlatform_ShouldReturnWindowsRootItems()
    {
        // Arrange
        _mockPlatformContext.Platform.Returns(PlatformType.Windows);
        IDriveInfo[] mockDrives =
        [
            CreateMockDriveInfo("C:\\", true),
            CreateMockDriveInfo("D:\\", true),
            CreateMockDriveInfo("E:\\", false) // Not ready
        ];
        _mockFileSystem.DriveInfo.GetDrives().Returns(mockDrives);

        // Act
        ErrorOr<IEnumerable<FileSystemItem>> result = _sut.GetDrives();

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Should().HaveCount(2); // Only ready drives
        result.Value.Should().AllBeOfType<WindowsRootItem>();
        result.Value.Select(d => d.Name).Should().BeEquivalentTo("C:\\", "D:\\");
    }

    private static IDriveInfo CreateMockDriveInfo(string name, bool isReady)
    {
        IDriveInfo mockDriveInfo = Substitute.For<IDriveInfo>();
        mockDriveInfo.Name.Returns(name);
        mockDriveInfo.IsReady.Returns(isReady);
        return mockDriveInfo;
    }
}
