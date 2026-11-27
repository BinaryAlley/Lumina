#region ========================================================================= USING =====================================================================================
using Lumina.Domain.SharedKernel.Common.Enums.FileSystem;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Domain.SharedKernel.UnitTests.Common.Enums.FileSystem;

/// <summary>
/// Contains unit tests for the <see cref="PlatformType"/> enumeration.
/// </summary>
[ExcludeFromCodeCoverage]
public class PlatformTypeTests
{
    [Fact]
    public void PlatformType_WhenEnumeratingValues_ShouldHaveNoDuplicateValues()
    {
        // Act
        PlatformType[] values = Enum.GetValues<PlatformType>();

        // Assert
        Assert.Equal(values.Length, values.Distinct().Count());
    }
}
