#region ========================================================================= USING =====================================================================================
using Lumina.Domain.SharedKernel.Common.Enums.Plugins;
using Lumina.Plugins.Contracts.Common.Models.DTO.Settings;
using Lumina.Plugins.Contracts.Fixtures.Common.Models.DTO.Settings;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Plugins.Contracts.UnitTests.Common.Models.DTO.Settings;

/// <summary>
/// Contains unit tests for the <see cref="PluginSettingDescriptorDto"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class PluginSettingDescriptorDtoTests
{
    private readonly PluginSettingDescriptorDtoFixture _pluginSettingDescriptorDtoFixture = new();

    [Fact]
    public void Create_WhenConstructedWithValues_ShouldPreserveAllValues()
    {
        // Arrange
        IReadOnlyList<string> allowedValues = ["Fiction", "Non-Fiction"];

        // Act
        PluginSettingDescriptorDto result = _pluginSettingDescriptorDtoFixture.Create(
            key: "Theme",
            label: "Theme",
            type: PluginSettingType.Select,
            defaultValue: "Fiction",
            allowedValues: allowedValues);

        // Assert
        Assert.Equal("Theme", result.Key);
        Assert.Equal("Theme", result.Label);
        Assert.Equal(PluginSettingType.Select, result.Type);
        Assert.Equal("Fiction", result.DefaultValue);
        Assert.Same(allowedValues, result.AllowedValues);
    }

    [Fact]
    public void Create_WhenDefaultValuesAreNotProvided_ShouldDefaultToNull()
    {
        // Act
        PluginSettingDescriptorDto result = _pluginSettingDescriptorDtoFixture.Create(defaultValue: null, allowedValues: null);

        // Assert
        Assert.Null(result.DefaultValue);
        Assert.Null(result.AllowedValues);
    }
}
