#region ========================================================================= USING =====================================================================================
using Lumina.Domain.SharedKernel.Common.Enums.MediaConsumption;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Domain.SharedKernel.UnitTests.Common.Enums.MediaConsumption;

/// <summary>
/// Contains unit tests for the <see cref="MediaPositionKind"/> enumeration.
/// </summary>
[ExcludeFromCodeCoverage]
public class MediaPositionKindTests
{
    [Fact]
    public void MediaPositionKind_WhenEnumeratingValues_ShouldHaveNoDuplicateValues()
    {
        // Act
        MediaPositionKind[] values = Enum.GetValues<MediaPositionKind>();

        // Assert
        Assert.Equal(values.Length, values.Distinct().Count());
    }
}
