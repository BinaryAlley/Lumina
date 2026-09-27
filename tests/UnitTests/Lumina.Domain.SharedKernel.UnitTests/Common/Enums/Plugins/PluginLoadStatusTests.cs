#region ========================================================================= USING =====================================================================================
using Lumina.Domain.SharedKernel.Common.Enums.Plugins;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Domain.SharedKernel.UnitTests.Common.Enums.Plugins;

/// <summary>
/// Contains unit tests for the <see cref="PluginLoadStatus"/> enumeration.
/// </summary>
[ExcludeFromCodeCoverage]
public class PluginLoadStatusTests
{
    [Fact]
    public void PluginLoadStatus_WhenEnumeratingValues_ShouldHaveNoDuplicateValues()
    {
        // Act
        PluginLoadStatus[] values = Enum.GetValues<PluginLoadStatus>();

        // Assert
        Assert.Equal(values.Length, values.Distinct().Count());
    }
}
