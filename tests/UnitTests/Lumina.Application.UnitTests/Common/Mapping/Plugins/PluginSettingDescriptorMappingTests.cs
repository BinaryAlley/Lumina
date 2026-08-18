#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.Mapping.Plugins;
using Lumina.Contracts.Responses.Plugins;
using Lumina.Domain.SharedKernel.Common.Enums.Plugins;
using Lumina.Plugins.Contracts.Common.Models.DTO.Settings;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Application.UnitTests.Common.Mapping.Plugins;

/// <summary>
/// Contains unit tests for the <see cref="PluginSettingDescriptorMapping"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class PluginSettingDescriptorMappingTests
{
    [Fact]
    public void ToResponse_WhenMappingValidDescriptor_ShouldMapCorrectly()
    {
        // Arrange
        PluginSettingDescriptorDto descriptor = new("apiKey", "API Key", PluginSettingType.Text, "default", null);

        // Act
        PluginSettingDescriptorResponse result = descriptor.ToResponse();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(descriptor.Key, result.Key);
        Assert.Equal(descriptor.Label, result.Label);
        Assert.Equal(PluginSettingType.Text, result.Type);
        Assert.Equal(descriptor.DefaultValue, result.DefaultValue);
        Assert.Null(result.AllowedValues);
    }

    [Theory]
    [InlineData(PluginSettingType.Text)]
    [InlineData(PluginSettingType.Number)]
    [InlineData(PluginSettingType.Boolean)]
    [InlineData(PluginSettingType.Select)]
    public void ToResponse_WhenMappingDifferentSettingTypes_ShouldMapTypeCorrectly(PluginSettingType type)
    {
        // Arrange
        PluginSettingDescriptorDto descriptor = new("key", "Label", type, null, null);

        // Act
        PluginSettingDescriptorResponse result = descriptor.ToResponse();

        // Assert
        Assert.Equal(type, result.Type);
    }

    [Fact]
    public void ToResponse_WhenMappingDescriptorWithAllowedValues_ShouldMapAllowedValues()
    {
        // Arrange
        PluginSettingDescriptorDto descriptor = new("mode", "Mode", PluginSettingType.Select, null, ["fast", "slow"]);

        // Act
        PluginSettingDescriptorResponse result = descriptor.ToResponse();

        // Assert
        Assert.NotNull(result.AllowedValues);
        Assert.Equal(2, result.AllowedValues.Count);
        Assert.Equal("fast", result.AllowedValues[0]);
        Assert.Equal("slow", result.AllowedValues[1]);
    }
}
