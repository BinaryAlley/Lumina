#region ========================================================================= USING =====================================================================================
using FluentAssertions;
using Lumina.Domain.Common.Enums.FileSystem;
using Lumina.Domain.Core.BoundedContexts.FileSystemManagementBoundedContext.FileSystemManagementAggregate.Strategies.Path;
using Lumina.Domain.Core.BoundedContexts.FileSystemManagementBoundedContext.FileSystemManagementAggregate.Strategies.Platform;
using NSubstitute;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Domain.UnitTests.Core.Aggregates.FileSystemManagementAggregate.Strategies.Platform;

/// <summary>
/// Contains unit tests for the <see cref="WindowsPlatformContext"/> and <see cref="UnixPlatformContext"/> classes.
/// </summary>
[ExcludeFromCodeCoverage]
public class PlatformContextTests
{
    [Fact]
    public void WindowsPlatformContext_Constructor_ShouldSetPropertiesCorrectly()
    {
        // Arrange
        IWindowsPathStrategy mockWindowsPathStrategy = Substitute.For<IWindowsPathStrategy>();

        // Act
        WindowsPlatformContext windowsPlatformContext = new(mockWindowsPathStrategy);

        // Assert
        windowsPlatformContext.Platform.Should().Be(PlatformType.Windows);
        windowsPlatformContext.PathStrategy.Should().Be(mockWindowsPathStrategy);
    }

    [Fact]
    public void UnixPlatformContext_Constructor_ShouldSetPropertiesCorrectly()
    {
        // Arrange
        IUnixPathStrategy mockUnixPathStrategy = Substitute.For<IUnixPathStrategy>();

        // Act
        UnixPlatformContext unixPlatformContext = new(mockUnixPathStrategy);

        // Assert
        unixPlatformContext.Platform.Should().Be(PlatformType.Unix);
        unixPlatformContext.PathStrategy.Should().Be(mockUnixPathStrategy);
    }

    [Fact]
    public void WindowsPlatformContext_ShouldImplementIWindowsPlatformContext()
    {
        // Arrange
        IWindowsPathStrategy mockWindowsPathStrategy = Substitute.For<IWindowsPathStrategy>();

        // Act
        WindowsPlatformContext windowsPlatformContext = new(mockWindowsPathStrategy);

        // Assert
        windowsPlatformContext.Should().BeAssignableTo<IWindowsPlatformContext>();
    }

    [Fact]
    public void UnixPlatformContext_ShouldImplementIUnixPlatformContext()
    {
        // Arrange
        IUnixPathStrategy mockUnixPathStrategy = Substitute.For<IUnixPathStrategy>();

        // Act
        UnixPlatformContext unixPlatformContext = new(mockUnixPathStrategy);

        // Assert
        unixPlatformContext.Should().BeAssignableTo<IUnixPlatformContext>();
    }

    [Fact]
    public void WindowsPlatformContext_PathStrategy_ShouldBeReadOnly()
    {
        // Arrange
        IWindowsPathStrategy mockWindowsPathStrategy = Substitute.For<IWindowsPathStrategy>();
        WindowsPlatformContext windowsPlatformContext = new(mockWindowsPathStrategy);

        // Act & Assert
        windowsPlatformContext.GetType().GetProperty(nameof(WindowsPlatformContext.PathStrategy))!
            .SetMethod.Should().BeNull();
    }

    [Fact]
    public void UnixPlatformContext_PathStrategy_ShouldBeReadOnly()
    {
        // Arrange
        IUnixPathStrategy mockUnixPathStrategy = Substitute.For<IUnixPathStrategy>();
        UnixPlatformContext unixPlatformContext = new(mockUnixPathStrategy);

        // Act & Assert
        unixPlatformContext.GetType().GetProperty(nameof(UnixPlatformContext.PathStrategy))!
            .SetMethod.Should().BeNull();
    }

    [Fact]
    public void WindowsPlatformContext_Platform_ShouldBeReadOnly()
    {
        // Arrange
        IWindowsPathStrategy mockWindowsPathStrategy = Substitute.For<IWindowsPathStrategy>();
        WindowsPlatformContext windowsPlatformContext = new(mockWindowsPathStrategy);

        // Act & Assert
        windowsPlatformContext.GetType().GetProperty(nameof(WindowsPlatformContext.Platform))!
            .SetMethod.Should().BeNull();
    }

    [Fact]
    public void UnixPlatformContext_Platform_ShouldBeReadOnly()
    {
        // Arrange
        IUnixPathStrategy mockUnixPathStrategy = Substitute.For<IUnixPathStrategy>();
        UnixPlatformContext unixPlatformContext = new(mockUnixPathStrategy);

        // Act & Assert
        unixPlatformContext.GetType().GetProperty(nameof(UnixPlatformContext.Platform))!
            .SetMethod.Should().BeNull();
    }
}
