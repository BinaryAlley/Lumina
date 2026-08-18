#region ========================================================================= USING =====================================================================================
using Lumina.Plugins.Contracts.Core.Metadata;
using Lumina.Plugins.Contracts.Core.Plugins;
using Lumina.Plugins.OpenLibrary.Common.DependencyInjection;
using Lumina.Plugins.OpenLibrary.Common.Models.DTO.Settings;
using Lumina.Plugins.OpenLibrary.Core.Api;
using Lumina.Plugins.OpenLibrary.Core.Settings;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Plugins.OpenLibrary.UnitTests.Common.DependencyInjection;

/// <summary>
/// Contains unit tests for the <see cref="OpenLibraryServices"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class OpenLibraryServicesTests
{
    [Fact]
    public void AddOpenLibraryBookMetadataProvider_WhenServicesIsNull_ShouldThrowArgumentNullException()
    {
        // Arrange
        IServiceCollection services = null!;

        // Act
        Action act = () => services.AddOpenLibraryBookMetadataProvider(pluginId: Guid.NewGuid());

        // Assert
        Assert.Throws<ArgumentNullException>(act);
    }

    [Fact]
    public void AddOpenLibraryBookMetadataProvider_WhenCalled_ShouldReturnTheSameServiceCollection()
    {
        // Arrange
        ServiceCollection services = new();

        // Act
        IServiceCollection result = services.AddOpenLibraryBookMetadataProvider(pluginId: Guid.NewGuid());

        // Assert
        Assert.Same(services, result);
    }

    [Fact]
    public void AddOpenLibraryBookMetadataProvider_WhenCalled_ShouldRegisterTheMetadataProviderForKeyedResolution()
    {
        // Arrange
        Guid pluginId = Guid.NewGuid();
        ServiceCollection services = new();

        // Act
        services.AddOpenLibraryBookMetadataProvider(pluginId: pluginId);
        ServiceProvider serviceProvider = services.BuildServiceProvider();

        // Assert
        Assert.NotNull(serviceProvider.GetKeyedService<IRemoteMetadataProvider>(pluginId));
        Assert.Null(serviceProvider.GetKeyedService<IRemoteMetadataProvider>(Guid.NewGuid()));
    }

    [Fact]
    public void AddOpenLibraryBookMetadataProvider_WhenCalled_ShouldRegisterTheOpenLibraryHttpClient()
    {
        // Arrange
        ServiceCollection services = new();

        // Act
        services.AddOpenLibraryBookMetadataProvider(pluginId: Guid.NewGuid());
        ServiceProvider serviceProvider = services.BuildServiceProvider();

        // Assert
        Assert.NotNull(serviceProvider.GetService<OpenLibraryHttpClient>());
    }

    [Fact]
    public void AddOpenLibraryBookMetadataProvider_WhenSettingsCallbackIsProvided_ShouldApplyTheCallbackToTheRuntimeSettings()
    {
        // Arrange
        ServiceCollection services = new();

        // Act
        services.AddOpenLibraryBookMetadataProvider(
            pluginId: Guid.NewGuid(),
            settingsCallback: settings => settings.ContactEmail = "callback@example.com");
        ServiceProvider serviceProvider = services.BuildServiceProvider();

        // Assert
        OpenLibrarySettingsDto settings = serviceProvider.GetRequiredService<OpenLibrarySettingsDto>();
        Assert.Equal("callback@example.com", settings.ContactEmail);
        Assert.Equal(10, settings.SearchResultLimit);
    }

    [Fact]
    public void AddOpenLibraryBookMetadataProvider_WhenASettingsStoreIsRegistered_ShouldApplyTheStoredSettings()
    {
        // Arrange
        Guid pluginId = Guid.NewGuid();
        IPluginSettingsStore settingsStore = Substitute.For<IPluginSettingsStore>();
        settingsStore.GetSettingsAsync(pluginId, Arg.Any<CancellationToken>())
            .Returns(new Dictionary<string, string>
            {
                [OpenLibrarySettingsKeys.SEARCH_RESULT_LIMIT] = "25"
            });
        ServiceCollection services = new();
        services.AddSingleton(settingsStore);

        // Act
        services.AddOpenLibraryBookMetadataProvider(pluginId: pluginId);
        ServiceProvider serviceProvider = services.BuildServiceProvider();

        // Assert
        OpenLibrarySettingsDto settings = serviceProvider.GetRequiredService<OpenLibrarySettingsDto>();
        Assert.Equal(25, settings.SearchResultLimit);
        Assert.Equal(50, settings.WorkEditionLimit);
    }

    [Fact]
    public void AddOpenLibraryBookMetadataProvider_WhenTheSettingsStoreReturnsNull_ShouldKeepTheDefaultSettings()
    {
        // Arrange
        Guid pluginId = Guid.NewGuid();
        IPluginSettingsStore settingsStore = Substitute.For<IPluginSettingsStore>();
        settingsStore.GetSettingsAsync(pluginId, Arg.Any<CancellationToken>())
            .Returns((IReadOnlyDictionary<string, string>?)null);
        ServiceCollection services = new();
        services.AddSingleton(settingsStore);

        // Act
        services.AddOpenLibraryBookMetadataProvider(pluginId: pluginId);
        ServiceProvider serviceProvider = services.BuildServiceProvider();

        // Assert
        OpenLibrarySettingsDto settings = serviceProvider.GetRequiredService<OpenLibrarySettingsDto>();
        Assert.Equal(10, settings.SearchResultLimit);
        Assert.Equal(50, settings.WorkEditionLimit);
    }

    [Fact]
    public void AddOpenLibraryBookMetadataProvider_WhenResolvedAcrossScopes_ShouldReturnDistinctRuntimeSettingsInstances()
    {
        // Arrange
        ServiceCollection services = new();
        services.AddOpenLibraryBookMetadataProvider(pluginId: Guid.NewGuid());
        ServiceProvider serviceProvider = services.BuildServiceProvider();

        // Act
        using IServiceScope firstScope = serviceProvider.CreateScope();
        using IServiceScope secondScope = serviceProvider.CreateScope();
        OpenLibrarySettingsDto first = firstScope.ServiceProvider.GetRequiredService<OpenLibrarySettingsDto>();
        OpenLibrarySettingsDto second = secondScope.ServiceProvider.GetRequiredService<OpenLibrarySettingsDto>();

        // Assert
        Assert.NotSame(first, second);
    }
}
