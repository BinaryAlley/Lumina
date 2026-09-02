#region ========================================================================= USING =====================================================================================
using Lumina.Infrastructure.Core.Plugins;
using Lumina.Plugins.Contracts.Core.Plugins;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Infrastructure.UnitTests.Core.Plugins;

/// <summary>
/// Contains unit tests for the <see cref="PluginManager"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class PluginManagerTests
{
    [Fact]
    public void GetPlugins_WhenCalled_ShouldReturnAllLoadedPlugins()
    {
        // Arrange
        IPlugin firstPlugin = CreatePlugin(Guid.NewGuid(), "First Plugin");
        IPlugin secondPlugin = CreatePlugin(Guid.NewGuid(), "Second Plugin");
        PluginManager sut = new([firstPlugin, secondPlugin], []);

        // Act
        IReadOnlyList<IPlugin> result = sut.GetPlugins();

        // Assert
        Assert.Equal(2, result.Count);
        Assert.Contains(firstPlugin, result);
        Assert.Contains(secondPlugin, result);
    }

    [Fact]
    public void GetPlugins_WhenNoPluginsWereLoaded_ShouldReturnEmptyList()
    {
        // Arrange
        PluginManager sut = new([], []);

        // Act
        IReadOnlyList<IPlugin> result = sut.GetPlugins();

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public void GetPlugin_WhenPluginWithIdWasLoaded_ShouldReturnThePlugin()
    {
        // Arrange
        Guid pluginId = Guid.NewGuid();
        IPlugin expectedPlugin = CreatePlugin(pluginId, "Expected Plugin");
        PluginManager sut = new([expectedPlugin, CreatePlugin(Guid.NewGuid(), "Other Plugin")], []);

        // Act
        IPlugin? result = sut.GetPlugin(pluginId);

        // Assert
        Assert.Same(expectedPlugin, result);
    }

    [Fact]
    public void GetPlugin_WhenPluginWithIdWasNotLoaded_ShouldReturnNull()
    {
        // Arrange
        PluginManager sut = new([CreatePlugin(Guid.NewGuid(), "Loaded Plugin")], []);

        // Act
        IPlugin? result = sut.GetPlugin(Guid.NewGuid());

        // Assert
        Assert.Null(result);
    }

    /// <summary>
    /// Creates a mocked plugin with the provided identity.
    /// </summary>
    /// <param name="id">The Id of the plugin.</param>
    /// <param name="name">The name of the plugin.</param>
    /// <returns>The created plugin mock.</returns>
    private static IPlugin CreatePlugin(Guid id, string name)
    {
        IPlugin plugin = Substitute.For<IPlugin>();
        plugin.Id.Returns(id);
        plugin.Name.Returns(name);
        return plugin;
    }
}
