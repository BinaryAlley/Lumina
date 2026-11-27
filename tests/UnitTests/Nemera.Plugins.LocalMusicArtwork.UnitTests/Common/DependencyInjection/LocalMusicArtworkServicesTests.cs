#region ========================================================================= USING =====================================================================================
using Lumina.Plugins.Contracts.Core.Metadata;
using Lumina.Plugins.LocalMusicArtwork.Common.DependencyInjection;
using Lumina.Plugins.LocalMusicArtwork.Core;
using Lumina.Plugins.LocalMusicArtwork.Core.Settings;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Plugins.LocalMusicArtwork.UnitTests.Common.DependencyInjection;

/// <summary>
/// Contains unit tests for the <see cref="LocalMusicArtworkServices"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class LocalMusicArtworkServicesTests
{
    [Fact]
    public void AddLocalMusicArtworkProviders_WhenServicesIsNull_ShouldThrowArgumentNullException()
    {
        // Arrange
        IServiceCollection services = null!;

        // Act
        Action act = () => services.AddLocalMusicArtworkProviders(pluginId: Guid.NewGuid());

        // Assert
        Assert.Throws<ArgumentNullException>(act);
    }

    [Fact]
    public void AddLocalMusicArtworkProviders_WhenCalled_ShouldReturnTheSameServiceCollection()
    {
        // Arrange
        ServiceCollection services = new();

        // Act
        IServiceCollection result = services.AddLocalMusicArtworkProviders(pluginId: Guid.NewGuid());

        // Assert
        Assert.Same(services, result);
    }

    [Fact]
    public void AddLocalMusicArtworkProviders_WhenCalled_ShouldRegisterTheAlbumAndArtistProvidersForKeyedResolution()
    {
        // Arrange
        Guid pluginId = Guid.NewGuid();
        ServiceCollection services = new();

        // Act
        services.AddLocalMusicArtworkProviders(pluginId: pluginId);
        ServiceProvider serviceProvider = services.BuildServiceProvider();

        // Assert
        IReadOnlyList<IArtworkProvider> providers = [.. serviceProvider.GetKeyedServices<IArtworkProvider>(pluginId)];
        Assert.Equal(2, providers.Count);
        Assert.Contains(providers, provider => provider is LocalMusicAlbumArtworkProvider);
        Assert.Contains(providers, provider => provider is LocalMusicArtistArtworkProvider);
        Assert.Equal(2, providers.Select(provider => provider.GetType()).Distinct().Count());
        Assert.Empty(serviceProvider.GetKeyedServices<IArtworkProvider>(Guid.NewGuid()));
    }

    [Fact]
    public void AddLocalMusicArtworkProviders_WhenCalled_ShouldRegisterTheSettingsProviderAsScoped()
    {
        // Arrange
        ServiceCollection services = new();

        // Act
        services.AddLocalMusicArtworkProviders(pluginId: Guid.NewGuid());

        // Assert
        ServiceDescriptor settingsProviderDescriptor = Assert.Single(services, descriptor => descriptor.ServiceType == typeof(LocalMusicArtworkSettingsProvider));
        Assert.Equal(ServiceLifetime.Scoped, settingsProviderDescriptor.Lifetime);
    }

    [Fact]
    public void AddLocalMusicArtworkProviders_WhenCalledTwice_ShouldNotDuplicateTheTryAddSettingsProvider()
    {
        // Arrange
        Guid pluginId = Guid.NewGuid();
        ServiceCollection services = new();

        // Act
        services.AddLocalMusicArtworkProviders(pluginId: pluginId);
        services.AddLocalMusicArtworkProviders(pluginId: pluginId);

        // Assert
        // The settings provider is registered with TryAdd, so a second call must not add it again, while the artwork providers stay transient and are added once per call.
        Assert.Single(services, descriptor => descriptor.ServiceType == typeof(LocalMusicArtworkSettingsProvider));
        Assert.Equal(4, services.Count(descriptor => descriptor.ServiceType == typeof(IArtworkProvider)));
    }
}
