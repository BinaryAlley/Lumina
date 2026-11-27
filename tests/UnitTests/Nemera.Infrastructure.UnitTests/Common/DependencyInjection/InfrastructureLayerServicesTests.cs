#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DomainEvents;
using Lumina.Application.Common.Infrastructure.Authorization.Policies.ConsumptionActivity;
using Lumina.Application.Common.Infrastructure.Models.DTO.Configuration;
using Lumina.Application.Common.Infrastructure.Plugins;
using Lumina.Application.Common.Infrastructure.Security;
using Lumina.Application.Common.Infrastructure.Time;
using Lumina.Application.Common.Infrastructure.Validation;
using Lumina.Application.Core.MediaLibrary.AudioLibrary.MusicLibrary.Playback;
using Lumina.Application.Core.MediaLibrary.Management.Progress;
using Lumina.Domain.Common.Events;
using Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryScanAggregate.Services.Cancellation;
using Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryScanAggregate.Services.Queue;
using Lumina.Infrastructure.Common.DependencyInjection;
using Lumina.Infrastructure.Common.Networking;
using Lumina.Infrastructure.Core.Authorization.Policies.ConsumptionActivity;
using Lumina.Infrastructure.Core.MediaLibrary.AudioLibrary.MusicLibrary.Playback;
using Lumina.Infrastructure.Core.Security;
using Lumina.Infrastructure.Core.Time;
using Lumina.Plugins.Contracts.Core.Plugins;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Net.Http;
#endregion

namespace Lumina.Infrastructure.UnitTests.Common.DependencyInjection;

/// <summary>
/// Contains unit tests for the <see cref="InfrastructureLayerServices"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class InfrastructureLayerServicesTests
{
    [Fact]
    public void AddInfrastructureLayerServices_WhenCalled_ShouldRegisterCoreInfrastructureServices()
    {
        // Arrange
        IServiceCollection services = new ServiceCollection();
        IConfiguration configuration = CreateConfiguration();

        // Act
        services.AddInfrastructureLayerServices(configuration);

        // Assert
        Assert.Contains(services, descriptor => descriptor.ServiceType == typeof(IFileHashService) && descriptor.ImplementationType == typeof(FileHashService));
        Assert.Contains(services, descriptor => descriptor.ServiceType == typeof(IDateTimeProvider) && descriptor.ImplementationType == typeof(DateTimeProvider));
        Assert.Contains(services, descriptor => descriptor.ServiceType == typeof(IPlaybackFormatService) && descriptor.ImplementationType == typeof(PlaybackFormatService));
        Assert.Contains(services, descriptor => descriptor.ServiceType == typeof(IPluginManager));
        Assert.Contains(services, descriptor => descriptor.ServiceType == typeof(IPluginSettingsStore));
        Assert.Contains(services, descriptor => descriptor.ServiceType == typeof(IDomainEventsQueue));
        Assert.Contains(services, descriptor => descriptor.ServiceType == typeof(IDomainEventPublisher));
        Assert.Contains(services, descriptor => descriptor.ServiceType == typeof(IMediaLibrariesScanQueue));
        Assert.Contains(services, descriptor => descriptor.ServiceType == typeof(IMediaLibrariesScanCancellationTracker));
        Assert.Contains(services, descriptor => descriptor.ServiceType == typeof(IMediaLibraryScanProgressNotifier));
        Assert.Contains(services, descriptor => descriptor.ServiceType == typeof(IHostedService));
        Assert.Contains(services, descriptor => descriptor.ServiceType == typeof(IConsumptionActivityAccessPolicy) && descriptor.ImplementationType == typeof(ConsumptionActivityAccessPolicy));
    }

    [Fact]
    public void AddInfrastructureLayerServices_WhenCalled_ShouldRegisterAllValidatorsFromTheAssembly()
    {
        // Arrange
        IServiceCollection services = new ServiceCollection();
        IConfiguration configuration = CreateConfiguration();

        // Act
        services.AddInfrastructureLayerServices(configuration);

        // Assert
        Assert.Contains(services, descriptor => descriptor.ServiceType == typeof(IValidator<PluginsSettingsDto>));
        Assert.Contains(services, descriptor => descriptor.ServiceType == typeof(IValidator<Lumina.Infrastructure.Common.Models.DTO.Configuration.CorsSettingsDto>));
        Assert.Contains(services, descriptor => descriptor.ServiceType == typeof(IValidator<Lumina.Infrastructure.Common.Models.DTO.Configuration.JwtSettingsDto>));
    }

    [Fact]
    public void AddInfrastructureLayerServices_WhenCalled_ShouldGuardEveryOutboundHttpClientWithThePublicHostChecks()
    {
        // Arrange
        IServiceCollection services = new ServiceCollection();
        IConfiguration configuration = CreateConfiguration();
        services.AddInfrastructureLayerServices(configuration);
        // A plugin registers its typed client after the infrastructure layer, so the defaults must reach it too.
        services.AddHttpClient("plugin-client");
        using ServiceProvider serviceProvider = services.BuildServiceProvider();
        IHttpMessageHandlerFactory httpMessageHandlerFactory = serviceProvider.GetRequiredService<IHttpMessageHandlerFactory>();

        // Act
        using HttpMessageHandler pluginClientHandler = httpMessageHandlerFactory.CreateHandler("plugin-client");
        using HttpMessageHandler artworkClientHandler = httpMessageHandlerFactory.CreateHandler(NamedHttpClients.ARTWORK_DOWNLOAD);

        // Assert
        AssertUsesThePublicHostGuards(pluginClientHandler);
        AssertUsesThePublicHostGuards(artworkClientHandler);
    }

    /// <summary>
    /// Creates an in-memory configuration pointing the plugins directory to a non-existent folder, so no plugin assemblies are loaded.
    /// </summary>
    /// <returns>The created configuration.</returns>
    private static IConfiguration CreateConfiguration()
    {
        Dictionary<string, string?> configurationValues = new()
        {
            [$"{PluginsSettingsDto.SECTION_NAME}:Directory"] = "non-existent-plugins-directory"
        };
        return new ConfigurationBuilder().AddInMemoryCollection(configurationValues).Build();
    }

    /// <summary>
    /// Asserts that the provided handler pipeline validates the host of every request, follows redirects only to public HTTPS hosts, and connects only to an
    /// address checked to be public.
    /// </summary>
    /// <param name="handler">The handler pipeline to check.</param>
    private static void AssertUsesThePublicHostGuards(HttpMessageHandler handler)
    {
        List<DelegatingHandler> delegatingHandlers = GetDelegatingHandlers(handler);
        int validationIndex = delegatingHandlers.FindIndex(delegatingHandler => delegatingHandler is PublicHostValidationHandler);
        int redirectIndex = delegatingHandlers.FindIndex(delegatingHandler => delegatingHandler is PublicHostRedirectHandler);
        // The validation handler must run before the redirect handler, because the redirect handler follows a redirect below the validation handler.
        Assert.True(validationIndex >= 0, "The public host validation handler must be part of the outbound pipeline.");
        Assert.True(redirectIndex > validationIndex, "The public host validation handler must run before the public host redirect handler.");
        SocketsHttpHandler primaryHandler = Assert.IsType<SocketsHttpHandler>(GetPrimaryHandler(handler));
        Assert.False(primaryHandler.AllowAutoRedirect);
        Assert.NotNull(primaryHandler.ConnectCallback);
    }

    /// <summary>
    /// Gets the delegating handlers of the provided handler pipeline, from the outermost to the innermost one.
    /// </summary>
    /// <param name="handler">The handler pipeline to walk.</param>
    /// <returns>The delegating handlers of the pipeline.</returns>
    private static List<DelegatingHandler> GetDelegatingHandlers(HttpMessageHandler handler)
    {
        List<DelegatingHandler> delegatingHandlers = [];
        HttpMessageHandler currentHandler = handler;
        while (currentHandler is DelegatingHandler delegatingHandler)
        {
            delegatingHandlers.Add(delegatingHandler);
            currentHandler = delegatingHandler.InnerHandler ?? throw new InvalidOperationException("The delegating handler has no inner handler.");
        }
        return delegatingHandlers;
    }

    /// <summary>
    /// Gets the primary, non delegating handler of the provided handler pipeline.
    /// </summary>
    /// <param name="handler">The handler pipeline to walk.</param>
    /// <returns>The primary handler of the pipeline.</returns>
    private static HttpMessageHandler GetPrimaryHandler(HttpMessageHandler handler)
    {
        HttpMessageHandler currentHandler = handler;
        while (currentHandler is DelegatingHandler delegatingHandler)
            currentHandler = delegatingHandler.InnerHandler ?? throw new InvalidOperationException("The delegating handler has no inner handler.");
        return currentHandler;
    }
}
