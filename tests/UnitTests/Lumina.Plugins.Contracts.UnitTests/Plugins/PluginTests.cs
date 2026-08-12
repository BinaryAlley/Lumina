#region ========================================================================= USING =====================================================================================
using Lumina.Plugins.Contracts.Core.Plugins;
using Lumina.Plugins.Contracts.Core.Plugins.Settings;
using Lumina.Plugins.Contracts.UnitTests.Fakes;
using System.Collections.Generic;
#endregion

namespace Lumina.Plugins.Contracts.UnitTests.Plugins;

/// <summary>
/// Contains unit tests for the <see cref="IPlugin"/> contract.
/// </summary>
public class PluginTests
{
    private readonly IPlugin _plugin = new FakeBookMetadataProvider();

    [Fact]
    public void GetSettingsSchema_WhenCalled_ShouldReturnDeclaredSettings()
    {
        // Act
        IReadOnlyList<PluginSettingDescriptor> schema = _plugin.GetSettingsSchema();

        // Assert
        Assert.Equal(2, schema.Count);
        Assert.Equal("preferredLanguage", schema[0].Key);
        Assert.Equal(PluginSettingType.Text, schema[0].Type);
        Assert.Equal("en", schema[0].DefaultValue);
        Assert.Equal("selectionStrategy", schema[1].Key);
        Assert.Equal(PluginSettingType.Select, schema[1].Type);
        Assert.Contains("first", schema[1].AllowedValues!);
    }
}
