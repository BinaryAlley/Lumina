#region ========================================================================= USING =====================================================================================
using Lumina.Plugins.Contracts.Core.Metadata;
using Lumina.Plugins.ID3.Common.DependencyInjection;
using Lumina.Plugins.ID3.Core.Tags;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Plugins.ID3.UnitTests.Common.DependencyInjection;

/// <summary>
/// Contains unit tests for the <see cref="Id3Services"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class Id3ServicesTests
{
    [Fact]
    public void AddId3MetadataProviders_WhenServicesIsNull_ShouldThrowArgumentNullException()
    {
        // Arrange
        IServiceCollection services = null!;

        // Act
        Action act = () => services.AddId3MetadataProviders(pluginId: Guid.NewGuid());

        // Assert
        Assert.Throws<ArgumentNullException>(act);
    }

    [Fact]
    public void AddId3MetadataProviders_WhenCalled_ShouldReturnTheSameServiceCollection()
    {
        // Arrange
        ServiceCollection services = new();

        // Act
        IServiceCollection result = services.AddId3MetadataProviders(pluginId: Guid.NewGuid());

        // Assert
        Assert.Same(services, result);
    }

    [Fact]
    public void AddId3MetadataProviders_WhenCalled_ShouldRegisterTheThreeMetadataProvidersForKeyedResolution()
    {
        // Arrange
        Guid pluginId = Guid.NewGuid();
        ServiceCollection services = new();

        // Act
        services.AddId3MetadataProviders(pluginId: pluginId);
        ServiceProvider serviceProvider = services.BuildServiceProvider();

        // Assert
        Assert.Equal(3, serviceProvider.GetKeyedServices<IMetadataProvider>(pluginId).Count());
        Assert.Empty(serviceProvider.GetKeyedServices<IMetadataProvider>(Guid.NewGuid()));
    }

    [Fact]
    public void AddId3MetadataProviders_WhenCalled_ShouldRegisterTheTagReaderAsScoped()
    {
        // Arrange
        ServiceCollection services = new();

        // Act
        services.AddId3MetadataProviders(pluginId: Guid.NewGuid());

        // Assert
        ServiceDescriptor tagReaderDescriptor = Assert.Single(services, descriptor => descriptor.ServiceType == typeof(Id3TagReader));
        Assert.Equal(ServiceLifetime.Scoped, tagReaderDescriptor.Lifetime);
    }
}
