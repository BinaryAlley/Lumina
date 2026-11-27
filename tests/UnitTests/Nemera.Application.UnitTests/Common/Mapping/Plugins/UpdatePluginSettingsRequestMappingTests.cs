#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.Mapping.Plugins;
using Lumina.Application.Core.Plugins.Commands.UpdatePluginSettings;
using Lumina.Contracts.Fixtures.Core.Requests.Plugins;
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
    private readonly UpdatePluginSettingsRequestFixture _updatePluginSettingsRequestFixture = new();

    [Fact]
    public void ToCommand_WhenMappingValidRequest_ShouldMapCorrectly()
    {
        // Arrange
        UpdatePluginSettingsRequest request = _updatePluginSettingsRequestFixture.Create(Guid.NewGuid(), new Dictionary<string, string> { ["key"] = "value" });

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
        UpdatePluginSettingsRequest request = _updatePluginSettingsRequestFixture.Create(Guid.NewGuid(), includeSettings: false);

        // Act
        UpdatePluginSettingsCommand result = request.ToCommand();

        // Assert
        Assert.Null(result.Settings);
    }
}
