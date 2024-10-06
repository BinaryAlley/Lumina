#region ========================================================================= USING =====================================================================================
using FluentAssertions;
using Lumina.Domain.Core.Aggregates.FileManagement.FileManagementAggregate.Strategies.Path;
using Lumina.Domain.Core.Aggregates.FileManagement.FileManagementAggregate.Strategies.Platform;
using Lumina.Domain.UnitTests.Core.Aggregates.FileManagementAggregate.Strategies.Platform.Fixtures;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Diagnostics.CodeAnalysis;
using System.IO.Abstractions;
#endregion

namespace Lumina.Domain.UnitTests.Core.Aggregates.FileManagementAggregate.Strategies.Platform;

/// <summary>
/// Contains unit tests for the <see cref="PlatformContextFactory"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class PlatformContextFactoryTests
{
    #region ================================================================== FIELD MEMBERS ================================================================================
    private readonly IServiceProvider _serviceProvider;
    private readonly PlatformContextFactory _sut;
    #endregion

    #region ====================================================================== CTOR =====================================================================================
    /// <summary>
    /// Initializes a new instance of the <see cref="PlatformContextFactoryTests"/> class.
    /// </summary>
    public PlatformContextFactoryTests()
    {
        ServiceCollection services = new();
        services.AddTransient<IWindowsPlatformContext, WindowsPlatformContext>();
        services.AddTransient<IUnixPlatformContext, UnixPlatformContext>();
        services.AddTransient<IUnixPathStrategy, UnixPathStrategy>();
        services.AddTransient<IWindowsPathStrategy, WindowsPathStrategy>();
        services.AddTransient<IFileSystem, FileSystem>();
        _serviceProvider = services.BuildServiceProvider();
        _sut = new PlatformContextFactory(_serviceProvider);
    }
    #endregion

    #region ===================================================================== METHODS ===================================================================================
    [Fact]
    public void CreateStrategy_WhenWindowsServiceExists_ShouldReturnWindowsService()
    {
        // Act
        IWindowsPlatformContext result = _sut.CreateStrategy<IWindowsPlatformContext>();

        // Assert
        result.Should().NotBeNull();
        result.Should().BeOfType<WindowsPlatformContext>();
    }

    [Fact]
    public void CreateStrategy_WhenUnixServiceExists_ShouldReturnUnixService()
    {
        // Act
        IUnixPlatformContext result = _sut.CreateStrategy<IUnixPlatformContext>();

        // Assert
        result.Should().NotBeNull();
        result.Should().BeOfType<UnixPlatformContext>();
    }

    [Fact]
    public void CreateStrategy_WhenServiceDoesNotExist_ShouldThrowInvalidOperationException()
    {
        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => _sut.CreateStrategy<INonExistentPlatformContextFixture>());
    }
    #endregion
}
