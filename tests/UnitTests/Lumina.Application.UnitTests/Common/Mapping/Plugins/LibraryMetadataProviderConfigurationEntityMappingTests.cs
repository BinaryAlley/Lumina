#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.Plugins;
using Lumina.Application.Common.Mapping.Plugins;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.Plugins;
using Lumina.Contracts.Responses.Plugins;
using System;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Application.UnitTests.Common.Mapping.Plugins;

/// <summary>
/// Contains unit tests for the <see cref="LibraryMetadataProviderConfigurationEntityMapping"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class LibraryMetadataProviderConfigurationEntityMappingTests
{
    private readonly LibraryMetadataProviderConfigurationEntityFixture _configurationEntityFixture = new();

    [Fact]
    public void ToResponse_WhenMappingValidConfiguration_ShouldMapCorrectly()
    {
        // Arrange
        Guid libraryId = Guid.NewGuid();
        Guid pluginId = Guid.NewGuid();
        LibraryMetadataProviderConfigurationEntity configuration = _configurationEntityFixture.Create(libraryId, pluginId, 3);

        // Act
        LibraryMetadataProviderResponse result = configuration.ToResponse("My Provider");

        // Assert
        Assert.NotNull(result);
        Assert.Equal(pluginId, result.PluginId);
        Assert.Equal("My Provider", result.Name);
        Assert.Equal(configuration.IsEnabled, result.IsEnabled);
        Assert.Equal(3, result.Rank);
    }
}
