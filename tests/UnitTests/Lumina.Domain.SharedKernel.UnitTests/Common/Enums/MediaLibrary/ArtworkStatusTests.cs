#region ========================================================================= USING =====================================================================================
using Lumina.Domain.SharedKernel.Common.Enums.MediaLibrary;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Domain.SharedKernel.UnitTests.Common.Enums.MediaLibrary;

/// <summary>
/// Contains unit tests for the <see cref="ArtworkStatus"/> enumeration.
/// </summary>
[ExcludeFromCodeCoverage]
public class ArtworkStatusTests
{
    [Fact]
    public void ArtworkStatus_WhenEnumeratingValues_ShouldHaveNoDuplicateValues()
    {
        // Act
        ArtworkStatus[] values = Enum.GetValues<ArtworkStatus>();

        // Assert
        Assert.Equal(values.Length, values.Distinct().Count());
    }
}
