#region ========================================================================= USING =====================================================================================
using Lumina.Domain.SharedKernel.Common.Enums.MediaLibrary;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Domain.SharedKernel.UnitTests.Common.Enums.MediaLibrary;

/// <summary>
/// Contains unit tests for the <see cref="MediaKind"/> enumeration.
/// </summary>
[ExcludeFromCodeCoverage]
public class MediaKindTests
{
    [Fact]
    public void MediaKind_WhenEnumeratingValues_ShouldHaveNoDuplicateValues()
    {
        // Act
        MediaKind[] values = Enum.GetValues<MediaKind>();

        // Assert
        Assert.Equal(values.Length, values.Distinct().Count());
    }

    [Fact]
    public void None_WhenCastingToInteger_ShouldBeZero()
    {
        // Act
        int value = (int)MediaKind.None;

        // Assert
        Assert.Equal(0, value);
    }
}
