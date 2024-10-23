#region ========================================================================= USING =====================================================================================
using FluentAssertions;
using Lumina.Contracts.Enums.FileSystem;
using Lumina.Domain.Core.Aggregates.FileSystemManagement.FileSystemManagementAggregate.Strategies.Platform;
using NSubstitute;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
#endregion

namespace Lumina.Domain.UnitTests.Core.Aggregates.FileSystemManagementAggregate.Strategies.Platform;

/// <summary>
/// Contains unit tests for the <see cref="PlatformContextManager"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class PlatformContextManagerTests
{
    private readonly IPlatformContextFactory _mockPlatformContextFactory;
    private PlatformContextManager? sut;

    /// <summary>
    /// Initializes a new instance of the <see cref="PlatformContextManagerTests"/> class.
    /// </summary>
    public PlatformContextManagerTests()
    {
        _mockPlatformContextFactory = Substitute.For<IPlatformContextFactory>();
    }

    [Fact]
    public void GetCurrentContext_WhenNoContextSet_ShouldSetDefaultContext()
    {
        // Arrange
        IWindowsPlatformContext mockWindowsContext = Substitute.For<IWindowsPlatformContext>();
        _mockPlatformContextFactory.CreateStrategy<IWindowsPlatformContext>().Returns(mockWindowsContext);
        IOperatingSystemInfo mockOsInfo = Substitute.For<IOperatingSystemInfo>();
        mockOsInfo.IsOSPlatform(OSPlatform.Linux).Returns(false);
        sut = new(_mockPlatformContextFactory, mockOsInfo);

        // Act
        IPlatformContext result = sut.GetCurrentContext();

        // Assert
        result.Should().Be(mockWindowsContext);
        _mockPlatformContextFactory.Received(1).CreateStrategy<IWindowsPlatformContext>();
    }

    [Fact]
    public void GetCurrentContext_WhenContextAlreadySet_ShouldReturnExistingContext()
    {
        // Arrange
        IUnixPlatformContext mockUnixContext = Substitute.For<IUnixPlatformContext>();
        _mockPlatformContextFactory.CreateStrategy<IUnixPlatformContext>().Returns(mockUnixContext);
        IOperatingSystemInfo mockOsInfo = Substitute.For<IOperatingSystemInfo>();
        sut = new(_mockPlatformContextFactory, mockOsInfo);
        sut.SetCurrentPlatform(PlatformType.Unix);

        // Act
        IPlatformContext result = sut.GetCurrentContext();

        // Assert
        result.Should().Be(mockUnixContext);
        _mockPlatformContextFactory.Received(1).CreateStrategy<IUnixPlatformContext>();
    }

    [Fact]
    public void SetCurrentPlatform_WhenUnixPlatform_ShouldSetUnixContext()
    {
        // Arrange
        IUnixPlatformContext mockUnixContext = Substitute.For<IUnixPlatformContext>();
        _mockPlatformContextFactory.CreateStrategy<IUnixPlatformContext>().Returns(mockUnixContext);
        IOperatingSystemInfo mockOsInfo = Substitute.For<IOperatingSystemInfo>();
        mockOsInfo.IsOSPlatform(OSPlatform.Linux).Returns(true);
        sut = new(_mockPlatformContextFactory, mockOsInfo);

        // Act
        sut.SetCurrentPlatform(PlatformType.Unix);
        IPlatformContext result = sut.GetCurrentContext();

        // Assert
        result.Should().Be(mockUnixContext);
        _mockPlatformContextFactory.Received(1).CreateStrategy<IUnixPlatformContext>();
    }

    [Fact]
    public void SetCurrentPlatform_WhenWindowsPlatform_ShouldSetWindowsContext()
    {
        // Arrange
        IWindowsPlatformContext mockWindowsContext = Substitute.For<IWindowsPlatformContext>();
        _mockPlatformContextFactory.CreateStrategy<IWindowsPlatformContext>().Returns(mockWindowsContext);
        IOperatingSystemInfo mockOsInfo = Substitute.For<IOperatingSystemInfo>();
        mockOsInfo.IsOSPlatform(OSPlatform.Linux).Returns(false);
        sut = new(_mockPlatformContextFactory, mockOsInfo);

        // Act
        sut.SetCurrentPlatform(PlatformType.Windows);
        IPlatformContext result = sut.GetCurrentContext();

        // Assert
        result.Should().Be(mockWindowsContext);
        _mockPlatformContextFactory.Received(1).CreateStrategy<IWindowsPlatformContext>();
    }

    [Fact]
    public void SetCurrentPlatform_WhenUnsupportedPlatform_ShouldThrowArgumentException()
    {
        // Arrange
        PlatformType unsupportedPlatform = (PlatformType)999; // An unsupported platform type
        IOperatingSystemInfo mockOsInfo = Substitute.For<IOperatingSystemInfo>();
        sut = new(_mockPlatformContextFactory, mockOsInfo);

        // Act & Assert
        Action act = () => sut.SetCurrentPlatform(unsupportedPlatform);
        act.Should().Throw<ArgumentException>()
           .WithMessage($"Unsupported platform type: {unsupportedPlatform}");
    }

    [Fact]
    public void GetCurrentContext_WhenRunningOnLinux_ShouldSetUnixContextByDefault()
    {
        // Arrange
        IUnixPlatformContext mockUnixContext = Substitute.For<IUnixPlatformContext>();
        _mockPlatformContextFactory.CreateStrategy<IUnixPlatformContext>().Returns(mockUnixContext);
        IOperatingSystemInfo mockOsInfo = Substitute.For<IOperatingSystemInfo>();
        mockOsInfo.IsOSPlatform(OSPlatform.Linux).Returns(true);
        sut = new(_mockPlatformContextFactory, mockOsInfo);

        // Act
        IPlatformContext result = sut.GetCurrentContext();

        // Assert
        result.Should().Be(mockUnixContext);
        _mockPlatformContextFactory.Received(1).CreateStrategy<IUnixPlatformContext>();
    }

    [Fact]
    public void GetCurrentContext_WhenRunningOnWindows_ShouldSetWindowsContextByDefault()
    {
        // Arrange
        IWindowsPlatformContext mockWindowsContext = Substitute.For<IWindowsPlatformContext>();
        _mockPlatformContextFactory.CreateStrategy<IWindowsPlatformContext>().Returns(mockWindowsContext);
        IOperatingSystemInfo mockOsInfo = Substitute.For<IOperatingSystemInfo>();
        mockOsInfo.IsOSPlatform(OSPlatform.Linux).Returns(false);
        sut = new(_mockPlatformContextFactory, mockOsInfo);

        // Act
        IPlatformContext result = sut.GetCurrentContext();

        // Assert
        result.Should().Be(mockWindowsContext);
        _mockPlatformContextFactory.Received(1).CreateStrategy<IWindowsPlatformContext>();
    }
}
