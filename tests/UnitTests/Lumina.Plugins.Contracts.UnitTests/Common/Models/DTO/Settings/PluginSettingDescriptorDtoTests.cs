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

    [Fact]
    public void Equality_WhenTwoDescriptorsHaveTheSameValues_ShouldBeEqual()
    {
        // Arrange
        IReadOnlyList<string> allowedValues = ["A"];

        // Act
        PluginSettingDescriptorDto first = _pluginSettingDescriptorDtoFixture.Create(key: "Key", label: "Label", type: PluginSettingType.Text, defaultValue: "value", allowedValues: allowedValues);
        PluginSettingDescriptorDto second = _pluginSettingDescriptorDtoFixture.Create(key: "Key", label: "Label", type: PluginSettingType.Text, defaultValue: "value", allowedValues: allowedValues);
        bool result = first == second;

        // Assert
        Assert.True(result);
        Assert.Equal(first, second);
        Assert.Equal(first.GetHashCode(), second.GetHashCode());
    }

    [Fact]
    public void Equality_WhenDescriptorsHaveDifferentAllowedValueInstances_ShouldNotBeEqual()
    {
        // Arrange
        PluginSettingDescriptorDto first = _pluginSettingDescriptorDtoFixture.Create(key: "Key", label: "Label", type: PluginSettingType.Text, defaultValue: "value", allowedValues: ["A"]);
        PluginSettingDescriptorDto second = _pluginSettingDescriptorDtoFixture.Create(key: "Key", label: "Label", type: PluginSettingType.Text, defaultValue: "value", allowedValues: ["A"]);

        // Act
        bool result = first == second;

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void Equality_WhenTwoDescriptorsHaveDifferentKeys_ShouldNotBeEqual()
    {
        // Arrange
        PluginSettingDescriptorDto first = _pluginSettingDescriptorDtoFixture.Create(key: "KeyOne", label: "Label", type: PluginSettingType.Text);
        PluginSettingDescriptorDto second = _pluginSettingDescriptorDtoFixture.Create(key: "KeyTwo", label: "Label", type: PluginSettingType.Text);

        // Act
        bool result = first == second;

        // Assert
        Assert.False(result);
        Assert.NotEqual(first, second);
    }
}
