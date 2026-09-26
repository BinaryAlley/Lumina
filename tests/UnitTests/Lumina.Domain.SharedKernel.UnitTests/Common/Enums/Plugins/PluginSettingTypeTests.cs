#region ========================================================================= USING =====================================================================================
using Lumina.Domain.SharedKernel.Common.Enums.Plugins;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Domain.SharedKernel.UnitTests.Common.Enums.Plugins;

/// <summary>
/// Contains unit tests for the <see cref="PluginSettingType"/> enumeration.
/// </summary>
[ExcludeFromCodeCoverage]
public class PluginSettingTypeTests
{
    [Fact]
    public void PluginSettingType_WhenEnumeratingValues_ShouldHaveNoDuplicateValues()
    {
        // Act
        PluginSettingType[] values = Enum.GetValues<PluginSettingType>();

        // Assert
        Assert.Equal(values.Length, values.Distinct().Count());
    }
}
