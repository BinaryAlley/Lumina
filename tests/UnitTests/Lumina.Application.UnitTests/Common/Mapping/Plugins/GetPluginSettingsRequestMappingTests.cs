#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.Mapping.Plugins;
using Lumina.Application.Core.Plugins.Queries.GetPluginSettings;
using Lumina.Contracts.Fixtures.Core.Requests.Plugins;
using Lumina.Contracts.Requests.Plugins;
using System;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Application.UnitTests.Common.Mapping.Plugins;

/// <summary>
/// Contains unit tests for the <see cref="GetPluginSettingsRequestMapping"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetPluginSettingsRequestMappingTests
{
    private readonly GetPluginSettingsRequestFixture _getPluginSettingsRequestFixture = new();

    [Fact]
    public void ToQuery_WhenMappingValidRequest_ShouldMapCorrectly()
    {
        // Arrange
        GetPluginSettingsRequest request = _getPluginSettingsRequestFixture.Create(Guid.NewGuid());

        // Act
        GetPluginSettingsQuery result = request.ToQuery();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(request.PluginId, result.PluginId);
    }
}
