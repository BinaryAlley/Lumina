#region ========================================================================= USING =====================================================================================
using Lumina.Domain.SharedKernel.Common.Enums.Common;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Domain.SharedKernel.UnitTests.Common.Enums.Common;

/// <summary>
/// Contains unit tests for the <see cref="SortOrder"/> enumeration.
/// </summary>
[ExcludeFromCodeCoverage]
public class SortOrderTests
{
    [Fact]
    public void SortOrder_WhenEnumeratingValues_ShouldHaveNoDuplicateValues()
    {
        // Act
        SortOrder[] values = Enum.GetValues<SortOrder>();

        // Assert
        Assert.Equal(values.Length, values.Distinct().Count());
    }

    [Fact]
    public void Ascending_WhenCastingToInteger_ShouldBeZero()
    {
        // Act
        int value = (int)SortOrder.Ascending;

        // Assert
        Assert.Equal(0, value);
    }
}
