#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DomainEvents;
using Lumina.Application.Common.Infrastructure.Models.DTO.Configuration;
using Lumina.Application.Common.Infrastructure.Plugins;
using Lumina.Application.Common.Infrastructure.Security;
using Lumina.Application.Common.Infrastructure.Time;
using Lumina.Application.Common.Infrastructure.Validation;
using Lumina.Application.Core.MediaLibrary.Management.Progress;
using Lumina.Domain.Common.Events;
using Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryScanAggregate.Services.Cancellation;
using Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryScanAggregate.Services.Queue;
using Lumina.Infrastructure.Common.DependencyInjection;
using Lumina.Infrastructure.Core.Plugins;
using Lumina.Infrastructure.Core.Security;
using Lumina.Infrastructure.Core.Time;
using Lumina.Plugins.Contracts.Core.Plugins;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
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
        Assert.Contains(services, descriptor => descriptor.ServiceType == typeof(IPluginManager));
        Assert.Contains(services, descriptor => descriptor.ServiceType == typeof(IPluginSettingsStore));
        Assert.Contains(services, descriptor => descriptor.ServiceType == typeof(IDomainEventsQueue));
        Assert.Contains(services, descriptor => descriptor.ServiceType == typeof(IDomainEventPublisher));
        Assert.Contains(services, descriptor => descriptor.ServiceType == typeof(IMediaLibrariesScanQueue));
        Assert.Contains(services, descriptor => descriptor.ServiceType == typeof(IMediaLibrariesScanCancellationTracker));
        Assert.Contains(services, descriptor => descriptor.ServiceType == typeof(IMediaLibraryScanProgressNotifier));
        Assert.Contains(services, descriptor => descriptor.ServiceType == typeof(IHostedService));
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
}
