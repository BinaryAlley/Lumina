#region ========================================================================= USING =====================================================================================
using Lumina.Plugins.Contracts.Core.Reading;
using Lumina.Plugins.Epub.Common.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Plugins.Epub.UnitTests.Common.DependencyInjection;

/// <summary>
/// Contains unit tests for the <see cref="EpubServices"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class EpubServicesTests
{
    [Fact]
    public void AddEpubReader_WhenServicesIsNull_ShouldThrowArgumentNullException()
    {
        // Arrange
        IServiceCollection services = null!;

        // Act
        Action act = () => services.AddEpubReader(pluginId: Guid.NewGuid());

        // Assert
        Assert.Throws<ArgumentNullException>(act);
    }

    [Fact]
    public void AddEpubReader_WhenCalled_ShouldReturnTheSameServiceCollection()
    {
        // Arrange
        ServiceCollection services = new();

        // Act
        IServiceCollection result = services.AddEpubReader(pluginId: Guid.NewGuid());

        // Assert
        Assert.Same(services, result);
    }

    [Fact]
    public void AddEpubReader_WhenCalled_ShouldRegisterTheBookReaderForKeyedResolution()
    {
        // Arrange
        Guid pluginId = Guid.NewGuid();
        ServiceCollection services = new();

        // Act
        services.AddEpubReader(pluginId: pluginId);
        ServiceProvider serviceProvider = services.BuildServiceProvider();

        // Assert
        Assert.NotNull(serviceProvider.GetKeyedService<IBookReader>(pluginId));
        Assert.Null(serviceProvider.GetKeyedService<IBookReader>(Guid.NewGuid()));
    }
}
