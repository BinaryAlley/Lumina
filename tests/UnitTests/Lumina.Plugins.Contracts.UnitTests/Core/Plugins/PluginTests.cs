#region ========================================================================= USING =====================================================================================
using Lumina.Plugins.Contracts.Core.Plugins;
using Lumina.Plugins.Contracts.Core.Plugins.Settings;
using Lumina.Plugins.Contracts.UnitTests.Common.Fakes;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Plugins.Contracts.UnitTests.Core.Plugins;

/// <summary>
/// Contains unit tests for the <see cref="IPlugin"/> contract.
/// </summary>
[ExcludeFromCodeCoverage]
public class PluginTests
{
    private readonly IPlugin _sut = new FakeBookMetadataProvider();

    [Fact]
    public void GetSettingsSchema_WhenCalled_ShouldReturnDeclaredSettings()
    {
        // Arrange

        // Act
        IReadOnlyList<PluginSettingDescriptor> schema = _sut.GetSettingsSchema();

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
