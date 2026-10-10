#region ========================================================================= USING =====================================================================================
using Lumina.Plugins.Contracts.Core.Metadata;
using Lumina.Plugins.CoverArtArchive.Common.DependencyInjection;
using Lumina.Plugins.CoverArtArchive.Common.Models.DTO.Settings;
using Lumina.Plugins.CoverArtArchive.Core.Api;
using Lumina.Plugins.CoverArtArchive.Core.Settings;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Plugins.CoverArtArchive.UnitTests.Common.DependencyInjection;

/// <summary>
/// Contains unit tests for the <see cref="CoverArtArchiveServices"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class CoverArtArchiveServicesTests
{
    [Fact]
    public void AddCoverArtArchiveArtworkProvider_WhenServicesIsNull_ShouldThrowArgumentNullException()
    {
        // Arrange
        IServiceCollection services = null!;

        // Act
        Action act = () => services.AddCoverArtArchiveArtworkProvider(pluginId: Guid.NewGuid());

        // Assert
        Assert.Throws<ArgumentNullException>(act);
    }

    [Fact]
    public void AddCoverArtArchiveArtworkProvider_WhenCalled_ShouldReturnTheSameServiceCollection()
    {
        // Arrange
        ServiceCollection services = new();

        // Act
        IServiceCollection result = services.AddCoverArtArchiveArtworkProvider(pluginId: Guid.NewGuid());

        // Assert
        Assert.Same(services, result);
    }

    [Fact]
    public void AddCoverArtArchiveArtworkProvider_WhenCalled_ShouldRegisterTheArtworkProviderForKeyedResolution()
    {
        // Arrange
        Guid pluginId = Guid.NewGuid();
        ServiceCollection services = new();

        // Act
        services.AddCoverArtArchiveArtworkProvider(pluginId: pluginId);
        ServiceProvider serviceProvider = services.BuildServiceProvider();

        // Assert
        Assert.Single(serviceProvider.GetKeyedServices<IArtworkProvider>(pluginId));
        Assert.Empty(serviceProvider.GetKeyedServices<IArtworkProvider>(Guid.NewGuid()));
    }

    [Fact]
    public void AddCoverArtArchiveArtworkProvider_WhenCalled_ShouldRegisterTheSettingsProviderAsScoped()
    {
        // Arrange
        ServiceCollection services = new();

        // Act
        services.AddCoverArtArchiveArtworkProvider(pluginId: Guid.NewGuid());

        // Assert
        ServiceDescriptor settingsProviderDescriptor = Assert.Single(services, descriptor => descriptor.ServiceType == typeof(CoverArtArchiveSettingsProvider));
        Assert.Equal(ServiceLifetime.Scoped, settingsProviderDescriptor.Lifetime);
    }

    [Fact]
    public void AddCoverArtArchiveArtworkProvider_WhenCalled_ShouldRegisterTheThrottleAndTheResponseCacheAsSingletons()
    {
        // Arrange
        ServiceCollection services = new();

        // Act
        services.AddCoverArtArchiveArtworkProvider(pluginId: Guid.NewGuid());

        // Assert
        ServiceDescriptor throttleDescriptor = Assert.Single(services, descriptor => descriptor.ServiceType == typeof(CoverArtArchiveRequestThrottle));
        ServiceDescriptor responseCacheDescriptor = Assert.Single(services, descriptor => descriptor.ServiceType == typeof(CoverArtArchiveResponseCache));
        Assert.Equal(ServiceLifetime.Singleton, throttleDescriptor.Lifetime);
        Assert.Equal(ServiceLifetime.Singleton, responseCacheDescriptor.Lifetime);
    }

    [Fact]
    public void AddCoverArtArchiveArtworkProvider_WhenCalled_ShouldRegisterTheHttpClient()
    {
        // Arrange
        ServiceCollection services = new();

        // Act
        services.AddCoverArtArchiveArtworkProvider(pluginId: Guid.NewGuid());
        ServiceProvider serviceProvider = services.BuildServiceProvider();

        // Assert
        Assert.NotNull(serviceProvider.GetService<CoverArtArchiveHttpClient>());
    }

    [Fact]
    public async Task AddCoverArtArchiveArtworkProvider_WhenASettingsCallbackIsProvided_ShouldApplyItToTheDefaults()
    {
        // Arrange
        Guid pluginId = Guid.NewGuid();
        ServiceCollection services = new();

        // Act
        services.AddCoverArtArchiveArtworkProvider(pluginId: pluginId, settings => settings.ContactEmail = "custom@example.com");
        ServiceProvider serviceProvider = services.BuildServiceProvider();
        using (IServiceScope scope = serviceProvider.CreateScope())
        {
            CoverArtArchiveSettingsProvider settingsProvider = scope.ServiceProvider.GetRequiredService<CoverArtArchiveSettingsProvider>();
            CoverArtArchiveSettingsDto settings = await settingsProvider.GetAsync(CancellationToken.None);

            // Assert
            Assert.Equal("custom@example.com", settings.ContactEmail);
        }
    }

    [Fact]
    public void AddCoverArtArchiveArtworkProvider_WhenTheClientIsResolved_ShouldDisableAutomaticRedirectsOnItsPrimaryHandler()
    {
        // Arrange
        ServiceCollection services = new();
        services.AddCoverArtArchiveArtworkProvider(pluginId: Guid.NewGuid());
        ServiceProvider serviceProvider = services.BuildServiceProvider();
        using (IServiceScope scope = serviceProvider.CreateScope())
        {
            // Act
            CoverArtArchiveHttpClient client = scope.ServiceProvider.GetRequiredService<CoverArtArchiveHttpClient>();
            HttpClientHandler primaryHandler = GetPrimaryHandler(client);

            // Assert
            // Automatic redirects must stay disabled on the primary handler, otherwise it would follow a server controlled redirect on its own,
            // before the client can check the target host against the Cover Art Archive and the Internet Archive, bypassing that control.
            Assert.False(primaryHandler.AllowAutoRedirect);
        }
    }

    [Fact]
    public void AddCoverArtArchiveArtworkProvider_WhenCalledTwice_ShouldNotDuplicateTheTryAddServices()
    {
        // Arrange
        Guid pluginId = Guid.NewGuid();
        ServiceCollection services = new();

        // Act
        services.AddCoverArtArchiveArtworkProvider(pluginId: pluginId);
        services.AddCoverArtArchiveArtworkProvider(pluginId: pluginId);

        // Assert
        // The settings provider, the throttle and the response cache are registered with TryAdd, so a second call must not add them again.
        Assert.Single(services, descriptor => descriptor.ServiceType == typeof(CoverArtArchiveRequestThrottle));
        Assert.Single(services, descriptor => descriptor.ServiceType == typeof(CoverArtArchiveResponseCache));
        Assert.Single(services, descriptor => descriptor.ServiceType == typeof(CoverArtArchiveSettingsProvider));
    }

    /// <summary>
    /// Gets the primary HTTP message handler of the provided client, unwrapping the delegating handlers built by the HTTP client factory around it.
    /// </summary>
    /// <param name="client">The client whose primary handler is retrieved.</param>
    /// <returns>The primary HTTP client handler of the client.</returns>
    private static HttpClientHandler GetPrimaryHandler(CoverArtArchiveHttpClient client)
    {
        HttpClient httpClient = (HttpClient)GetFieldOfType(client, typeof(HttpClient))!;
        HttpMessageHandler? handler = (HttpMessageHandler?)GetFieldOfType(httpClient, typeof(HttpMessageHandler));
        while (handler is DelegatingHandler delegatingHandler)
            handler = delegatingHandler.InnerHandler;
        return Assert.IsType<HttpClientHandler>(handler);
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
}
