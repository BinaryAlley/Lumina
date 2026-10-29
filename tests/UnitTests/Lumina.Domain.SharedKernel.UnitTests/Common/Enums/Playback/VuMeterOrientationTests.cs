#region ========================================================================= USING =====================================================================================
using Lumina.Domain.SharedKernel.Common.Enums.Playback;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Domain.SharedKernel.UnitTests.Common.Enums.Playback;

/// <summary>
/// Contains unit tests for the <see cref="VuMeterOrientation"/> enumeration.
/// </summary>
[ExcludeFromCodeCoverage]
public class VuMeterOrientationTests
{
    [Fact]
    public void VuMeterOrientation_WhenEnumeratingValues_ShouldHaveNoDuplicateValues()
    {
        // Act
        VuMeterOrientation[] values = Enum.GetValues<VuMeterOrientation>();

        // Assert
        Assert.Equal(values.Length, values.Distinct().Count());
    }

    [Fact]
    public void VuMeterOrientation_WhenEnumeratingNames_ShouldHaveTheTwoDocumentedMembersInOrder()
    {
        // Act
        string[] names = Enum.GetNames<VuMeterOrientation>();

        // Assert
        string[] expectedNames =
        [
            nameof(VuMeterOrientation.Vertical),
            nameof(VuMeterOrientation.Horizontal)
        ];
        Assert.Equal(expectedNames, names);
    }

    [Fact]
    public void VuMeterOrientation_WhenEnumeratingValues_ShouldHaveTheDocumentedUnderlyingValues()
    {
        // Act
        VuMeterOrientation[] values = Enum.GetValues<VuMeterOrientation>();

        // Assert
        VuMeterOrientation[] expectedValues =
        [
            VuMeterOrientation.Vertical,
            VuMeterOrientation.Horizontal
        ];
        int[] expectedUnderlyingValues = [0, 1];
        Assert.Equal(expectedValues, values);
        Assert.Equal(expectedUnderlyingValues, values.Select(value => (int)value));
    }

    [Fact]
    public void VuMeterOrientation_WhenDefaulted_ShouldBeVertical()
    {
        // Act
        VuMeterOrientation orientation = default;

        // Assert
        Assert.Equal(VuMeterOrientation.Vertical, orientation);
    }
}
