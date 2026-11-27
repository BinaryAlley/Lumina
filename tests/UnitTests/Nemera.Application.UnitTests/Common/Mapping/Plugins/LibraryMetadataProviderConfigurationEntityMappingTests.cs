#region ========================================================================= USING =====================================================================================
using Nemera.Application.Common.DataAccess.Entities.Plugins;
using Nemera.Application.Common.Mapping.Plugins;
using Nemera.Application.Fixtures.Common.DataAccess.Entities.Plugins;
using Nemera.Contracts.Responses.Plugins;
using System;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Nemera.Application.UnitTests.Common.Mapping.Plugins;

/// <summary>
/// Contains unit tests for the <see cref="LibraryMetadataProviderConfigurationEntityMapping"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class LibraryMetadataProviderConfigurationEntityMappingTests
{
    private readonly LibraryMetadataProviderConfigurationEntityFixture _libraryMetadataProviderConfigurationEntityFixture = new();

    [Fact]
    public void ToResponse_WhenMappingValidConfiguration_ShouldMapCorrectly()
    {
        // Arrange
        Guid libraryId = Guid.NewGuid();
        Guid pluginId = Guid.NewGuid();
        LibraryMetadataProviderConfigurationEntity configuration = _libraryMetadataProviderConfigurationEntityFixture.Create(libraryId, pluginId, 3);

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
