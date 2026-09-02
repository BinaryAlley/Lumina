#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.Infrastructure.Plugins;
using Lumina.Application.Common.Infrastructure.Reading;
using Lumina.Domain.SharedKernel.Common.Enums.MediaLibrary;
using Lumina.Infrastructure.Core.MediaLibrary.WrittenContentLibrary.BookLibrary.Reading;
using Lumina.Plugins.Contracts.Core.Plugins;
using Lumina.Plugins.Contracts.Core.Reading;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Infrastructure.UnitTests.Core.MediaLibrary.WrittenContentLibrary.BookLibrary.Reading;

/// <summary>
/// Contains unit tests for the <see cref="BookReaderRegistry"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class BookReaderRegistryTests
{
    private readonly IPluginManager _mockPluginManager;
    private readonly IServiceProvider _serviceProvider;
    private readonly BookReaderRegistry _sut;
    private readonly Guid _pluginId = Guid.NewGuid();

    /// <summary>
    /// Initializes a new instance of the <see cref="BookReaderRegistryTests"/> class.
    /// </summary>
    public BookReaderRegistryTests()
    {
        _mockPluginManager = Substitute.For<IPluginManager>();
        _serviceProvider = new ServiceCollection().BuildServiceProvider();
        _sut = new BookReaderRegistry(_mockPluginManager, _serviceProvider);
    }

    [Fact]
    public void GetSupportedExtensionsByPluginId_WhenReadersAreRegistered_ShouldReturnExtensionsKeyedByPluginId()
    {
        // Arrange
        IPlugin plugin = CreatePlugin(_pluginId, "Test Plugin");
        IBookReader reader = CreateReader(".epub", ".EPUB");
        _mockPluginManager.GetPlugins().Returns([plugin]);
        ServiceProvider serviceProvider = BuildServiceProviderWithReaders(plugin.Id, reader);

        // Act
        IReadOnlyDictionary<Guid, IReadOnlyList<string>> result = new BookReaderRegistry(_mockPluginManager, serviceProvider).GetSupportedExtensionsByPluginId();

        // Assert
        IReadOnlyList<string> extensions = Assert.Single(result.Values);
        Assert.Equal([".epub"], extensions);
    }

    [Fact]
    public void GetSupportedExtensionsByPluginId_WhenReaderHasNoUsableExtensions_ShouldSkipThePlugin()
    {
        // Arrange
        IPlugin plugin = CreatePlugin(_pluginId, "Test Plugin");
        IBookReader reader = CreateReader(string.Empty);
        _mockPluginManager.GetPlugins().Returns([plugin]);
        ServiceProvider serviceProvider = BuildServiceProviderWithReaders(plugin.Id, reader);

        // Act
        IReadOnlyDictionary<Guid, IReadOnlyList<string>> result = new BookReaderRegistry(_mockPluginManager, serviceProvider).GetSupportedExtensionsByPluginId();

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public void GetSupportedExtensionsByPluginId_WhenNoReadersAreRegistered_ShouldReturnEmptyDictionary()
    {
        // Arrange
        _mockPluginManager.GetPlugins().Returns([]);

        // Act
        IReadOnlyDictionary<Guid, IReadOnlyList<string>> result = _sut.GetSupportedExtensionsByPluginId();

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public void GetSupportedExtensionsByPluginId_WhenMultipleReadersOfOnePlugin_ShouldMergeAndDistinctExtensions()
    {
        // Arrange
        IPlugin plugin = CreatePlugin(_pluginId, "Test Plugin");
        IBookReader firstReader = CreateReader(".pdf");
        IBookReader secondReader = CreateReader(".pdf", ".epub");
        _mockPluginManager.GetPlugins().Returns([plugin]);
        ServiceCollection services = new();
        services.AddKeyedSingleton(plugin.Id, firstReader);
        services.AddKeyedSingleton(plugin.Id, secondReader);
        ServiceProvider serviceProvider = services.BuildServiceProvider();

        // Act
        IReadOnlyDictionary<Guid, IReadOnlyList<string>> result = new BookReaderRegistry(_mockPluginManager, serviceProvider).GetSupportedExtensionsByPluginId();

        // Assert
        IReadOnlyList<string> extensions = Assert.Single(result.Values);
        Assert.Equal([".epub", ".pdf"], extensions);
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

    /// <summary>
    /// Creates a mocked book reader with the provided extensions.
    /// </summary>
    /// <param name="extensions">The supported extensions of the reader.</param>
    /// <returns>The created reader mock.</returns>
    private static IBookReader CreateReader(params string[] extensions)
    {
        IBookReader reader = Substitute.For<IBookReader>();
        reader.SupportedExtensions.Returns(extensions);
        reader.SupportedLibraryTypes.Returns([LibraryType.EBook]);
        return reader;
    }

    /// <summary>
    /// Builds a service provider in which the provided readers are registered as keyed services of the plugin.
    /// </summary>
    /// <param name="pluginId">The Id of the plugin providing the readers.</param>
    /// <param name="readers">The readers to register.</param>
    /// <returns>The built service provider.</returns>
    private static ServiceProvider BuildServiceProviderWithReaders(Guid pluginId, params IBookReader[] readers)
    {
        ServiceCollection services = new();
        foreach (IBookReader reader in readers)
            services.AddKeyedSingleton<IBookReader>(pluginId, reader);
        return services.BuildServiceProvider();
    }
}
