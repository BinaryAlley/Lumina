#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.Plugins;
using Lumina.Application.Common.Mapping.Plugins;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.Plugins;
using Lumina.Contracts.Responses.Plugins;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Application.UnitTests.Common.Mapping.Plugins;

/// <summary>
/// Contains unit tests for the <see cref="LibraryBookReaderConfigurationEntityMapping"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class LibraryBookReaderConfigurationEntityMappingTests
{
    private readonly LibraryBookReaderConfigurationEntityFixture _configurationEntityFixture = new();

    [Fact]
    public void ToResponse_WhenMappingValidConfiguration_ShouldMapCorrectly()
    {
        // Arrange
        Guid pluginId = Guid.NewGuid();
        LibraryBookReaderConfigurationEntity configuration = _configurationEntityFixture.Create(pluginId: pluginId, isEnabled: false);
        IReadOnlyList<string> supportedExtensions = [".epub", ".pdf"];

        // Act
        LibraryBookReaderResponse result = configuration.ToResponse("My Reader", supportedExtensions);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(pluginId, result.PluginId);
        Assert.Equal("My Reader", result.Name);
        Assert.Equal(supportedExtensions, result.SupportedExtensions);
        Assert.False(result.IsEnabled);
    }
}
