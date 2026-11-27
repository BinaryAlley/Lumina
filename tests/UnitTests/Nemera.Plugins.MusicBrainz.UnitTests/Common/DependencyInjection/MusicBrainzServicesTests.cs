#region ========================================================================= USING =====================================================================================
using Nemera.Plugins.Contracts.Core.Metadata;
using Nemera.Plugins.MusicBrainz.Common.DependencyInjection;
using Nemera.Plugins.MusicBrainz.Common.Models.DTO.Settings;
using Nemera.Plugins.MusicBrainz.Core.Api;
using Nemera.Plugins.MusicBrainz.Core.Settings;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Nemera.Plugins.MusicBrainz.UnitTests.Common.DependencyInjection;

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
    public void AddMusicBrainzMetadataProviders_WhenTheHostConfiguresHttpClientDefaults_ShouldKeepThePrimaryHandlerOfTheHost()
    {
        // Arrange
        ServiceCollection services = new();
        StubPrimaryHttpMessageHandler hostPrimaryHandler = new();
        services.ConfigureHttpClientDefaults(builder => builder.ConfigurePrimaryHttpMessageHandler(() => hostPrimaryHandler));
        services.AddMusicBrainzMetadataProviders(pluginId: Guid.NewGuid());
        ServiceProvider serviceProvider = services.BuildServiceProvider();
        using (IServiceScope scope = serviceProvider.CreateScope())
        {
            // Act
            MusicBrainzHttpClient client = scope.ServiceProvider.GetRequiredService<MusicBrainzHttpClient>();

            // Assert
            // The host owns the primary handler, because it is what validates and pins the address of every connection. The plugin must not replace it.
            Assert.Same(hostPrimaryHandler, GetPrimaryHandler(client));
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

    /// <summary>
    /// Gets the primary HTTP message handler of the provided client, unwrapping the delegating handlers built by the HTTP client factory around it.
    /// </summary>
    /// <param name="client">The client whose primary handler is retrieved.</param>
    /// <returns>The primary HTTP message handler of the client.</returns>
    private static HttpMessageHandler GetPrimaryHandler(MusicBrainzHttpClient client)
    {
        HttpClient httpClient = (HttpClient)GetFieldOfType(client, typeof(HttpClient))!;
        HttpMessageHandler? handler = (HttpMessageHandler?)GetFieldOfType(httpClient, typeof(HttpMessageHandler));
        while (handler is DelegatingHandler delegatingHandler)
            handler = delegatingHandler.InnerHandler;
        return handler ?? throw new InvalidOperationException("The HTTP client has no primary handler.");
    }

    /// <summary>
    /// Gets the value of the first field, declared on the provided <paramref name="instance"/> or any of its base types, whose type is assignable to
    /// <paramref name="fieldType"/>, without relying on the field name, so the test keeps working across runtime versions.
    /// </summary>
    /// <param name="instance">The instance whose field is read.</param>
    /// <param name="fieldType">The type the field must be assignable to.</param>
    /// <returns>The value of the found field.</returns>
    /// <exception cref="InvalidOperationException">Thrown when no field assignable to <paramref name="fieldType"/> is found.</exception>
    private static object GetFieldOfType(object instance, Type fieldType)
    {
        Type? type = instance.GetType();
        while (type is not null)
        {
            FieldInfo? field = type
                .GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.DeclaredOnly)
                .FirstOrDefault(candidate => fieldType.IsAssignableFrom(candidate.FieldType));
            if (field is not null)
                return field.GetValue(instance)!;
            type = type.BaseType;
        }
        throw new InvalidOperationException($"No field assignable to {fieldType} was found on {instance.GetType()} or its base types.");
    }

    /// <summary>
    /// Primary HTTP message handler that identifies the handler the host configured, so a test can verify that the plugin does not replace it.
    /// </summary>
    [ExcludeFromCodeCoverage]
    private sealed class StubPrimaryHttpMessageHandler : HttpMessageHandler
    {
        /// <summary>
        /// Sends the provided request. The tests never send a request through this handler.
        /// </summary>
        /// <param name="request">The request to send.</param>
        /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
        /// <returns>The response of the request.</returns>
        /// <exception cref="NotSupportedException">Always thrown, because this handler only identifies the handler configured by the host.</exception>
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            throw new NotSupportedException();
        }
    }
}
