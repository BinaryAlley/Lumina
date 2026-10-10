#region ========================================================================= USING =====================================================================================
using Lumina.Plugins.Contracts.Core.Metadata;
using Lumina.Plugins.MusicBrainz.Common.DependencyInjection;
using Lumina.Plugins.MusicBrainz.Common.Models.DTO.Settings;
using Lumina.Plugins.MusicBrainz.Core;
using Lumina.Plugins.MusicBrainz.Core.Api;
using Lumina.Plugins.MusicBrainz.Core.Settings;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Plugins.MusicBrainz.UnitTests.Common.DependencyInjection;

/// <summary>
/// Contains unit tests for the <see cref="MusicBrainzServices"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class MusicBrainzServicesTests
{
    [Fact]
    public void AddMusicBrainzMetadataProviders_WhenServicesIsNull_ShouldThrowArgumentNullException()
    {
        // Arrange
        IServiceCollection services = null!;

        // Act
        Action act = () => services.AddMusicBrainzMetadataProviders(pluginId: Guid.NewGuid());

        // Assert
        Assert.Throws<ArgumentNullException>(act);
    }

    [Fact]
    public void AddMusicBrainzMetadataProviders_WhenCalled_ShouldReturnTheSameServiceCollection()
    {
        // Arrange
        ServiceCollection services = new();

        // Act
        IServiceCollection result = services.AddMusicBrainzMetadataProviders(pluginId: Guid.NewGuid());

        // Assert
        Assert.Same(services, result);
    }

    [Fact]
    public void AddMusicBrainzMetadataProviders_WhenCalled_ShouldRegisterTheThreeMetadataProvidersForKeyedResolution()
    {
        // Arrange
        Guid pluginId = Guid.NewGuid();
        ServiceCollection services = new();

        // Act
        services.AddMusicBrainzMetadataProviders(pluginId: pluginId);
        ServiceProvider serviceProvider = services.BuildServiceProvider();

        // Assert
        Assert.Equal(3, serviceProvider.GetKeyedServices<IMetadataProvider>(pluginId).Count());
        Assert.Empty(serviceProvider.GetKeyedServices<IMetadataProvider>(Guid.NewGuid()));
    }

    [Fact]
    public void AddMusicBrainzMetadataProviders_WhenCalled_ShouldRegisterTheSettingsProviderAsScoped()
    {
        // Arrange
        ServiceCollection services = new();

        // Act
        services.AddMusicBrainzMetadataProviders(pluginId: Guid.NewGuid());

        // Assert
        ServiceDescriptor settingsProviderDescriptor = Assert.Single(services, descriptor => descriptor.ServiceType == typeof(MusicBrainzSettingsProvider));
        Assert.Equal(ServiceLifetime.Scoped, settingsProviderDescriptor.Lifetime);
    }

    [Fact]
    public void AddMusicBrainzMetadataProviders_WhenCalled_ShouldRegisterTheThrottleAndTheResponseCacheAsSingletons()
    {
        // Arrange
        ServiceCollection services = new();

        // Act
        services.AddMusicBrainzMetadataProviders(pluginId: Guid.NewGuid());

        // Assert
        ServiceDescriptor throttleDescriptor = Assert.Single(services, descriptor => descriptor.ServiceType == typeof(MusicBrainzRequestThrottle));
        ServiceDescriptor responseCacheDescriptor = Assert.Single(services, descriptor => descriptor.ServiceType == typeof(MusicBrainzResponseCache));
        Assert.Equal(ServiceLifetime.Singleton, throttleDescriptor.Lifetime);
        Assert.Equal(ServiceLifetime.Singleton, responseCacheDescriptor.Lifetime);
    }

    [Fact]
    public void AddMusicBrainzMetadataProviders_WhenCalled_ShouldRegisterTheHttpClient()
    {
        // Arrange
        ServiceCollection services = new();

        // Act
        services.AddMusicBrainzMetadataProviders(pluginId: Guid.NewGuid());
        ServiceProvider serviceProvider = services.BuildServiceProvider();

        // Assert
        Assert.NotNull(serviceProvider.GetService<MusicBrainzHttpClient>());
    }

    [Fact]
    public async Task AddMusicBrainzMetadataProviders_WhenASettingsCallbackIsProvided_ShouldApplyItToTheDefaults()
    {
        // Arrange
        Guid pluginId = Guid.NewGuid();
        ServiceCollection services = new();

        // Act
        services.AddMusicBrainzMetadataProviders(pluginId: pluginId, settings => settings.BaseUrl = "http://custom.example/ws/2/");
        ServiceProvider serviceProvider = services.BuildServiceProvider();
        using (IServiceScope scope = serviceProvider.CreateScope())
        {
            MusicBrainzSettingsProvider settingsProvider = scope.ServiceProvider.GetRequiredService<MusicBrainzSettingsProvider>();
            MusicBrainzSettingsDto settings = await settingsProvider.GetAsync(CancellationToken.None);

            // Assert
            Assert.Equal("http://custom.example/ws/2/", settings.BaseUrl);
        }
    }

    [Fact]
    public void AddMusicBrainzMetadataProviders_WhenCalledTwice_ShouldNotDuplicateTheTryAddServices()
    {
        // Arrange
        Guid pluginId = Guid.NewGuid();
        ServiceCollection services = new();

        // Act
        services.AddMusicBrainzMetadataProviders(pluginId: pluginId);
        services.AddMusicBrainzMetadataProviders(pluginId: pluginId);

        // Assert
        // The settings provider, the throttle and the response cache are registered with TryAdd, so a second call must not add them again.
        Assert.Single(services, descriptor => descriptor.ServiceType == typeof(MusicBrainzRequestThrottle));
        Assert.Single(services, descriptor => descriptor.ServiceType == typeof(MusicBrainzResponseCache));
        Assert.Single(services, descriptor => descriptor.ServiceType == typeof(MusicBrainzSettingsProvider));
    }
}
