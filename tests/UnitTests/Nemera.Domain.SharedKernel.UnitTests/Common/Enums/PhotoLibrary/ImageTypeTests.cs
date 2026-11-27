#region ========================================================================= USING =====================================================================================
using Lumina.Domain.SharedKernel.Common.Enums.PhotoLibrary;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Domain.SharedKernel.UnitTests.Common.Enums.PhotoLibrary;

/// <summary>
/// Contains unit tests for the <see cref="ImageType"/> enumeration.
/// </summary>
[ExcludeFromCodeCoverage]
public class ImageTypeTests
{
    [Fact]
    public void ImageType_WhenEnumeratingValues_ShouldHaveNoDuplicateValues()
    {
        // Act
        ImageType[] values = Enum.GetValues<ImageType>();

        // Assert
        Assert.Equal(values.Length, values.Distinct().Count());
    }

    [Fact]
    public void None_WhenCastingToInteger_ShouldBeZero()
    {
        // Act
        int value = (int)ImageType.None;

        // Assert
        Assert.Equal(0, value);
    }
}
