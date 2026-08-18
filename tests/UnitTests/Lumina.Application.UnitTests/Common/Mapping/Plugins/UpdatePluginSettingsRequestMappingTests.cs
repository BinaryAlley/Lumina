#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.Mapping.Plugins;
using Lumina.Application.Core.Plugins.Commands.UpdatePluginSettings;
using Lumina.Contracts.Requests.Plugins;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Application.UnitTests.Common.Mapping.Plugins;

/// <summary>
/// Contains unit tests for the <see cref="UpdatePluginSettingsRequestMapping"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class UpdatePluginSettingsRequestMappingTests
{
    [Fact]
    public void ToCommand_WhenMappingValidRequest_ShouldMapCorrectly()
    {
        // Arrange
        UpdatePluginSettingsRequest request = new(PluginId: Guid.NewGuid(), Settings: new Dictionary<string, string> { ["key"] = "value" });

        // Act
        UpdatePluginSettingsCommand result = request.ToCommand();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(request.PluginId, result.PluginId);
        Assert.Equal(request.Settings, result.Settings);
    }

    [Fact]
    public void ToCommand_WhenMappingRequestWithNullSettings_ShouldMapNullSettings()
    {
        // Arrange
        UpdatePluginSettingsRequest request = new(PluginId: Guid.NewGuid(), Settings: null);

        // Act
        UpdatePluginSettingsCommand result = request.ToCommand();

        // Assert
        Assert.Null(result.Settings);
    }
}
