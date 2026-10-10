#region ========================================================================= USING =====================================================================================
using Lumina.Domain.SharedKernel.Common.Enums.MediaLibrary;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Domain.SharedKernel.UnitTests.Common.Enums.MediaLibrary;

/// <summary>
/// Contains unit tests for the <see cref="MetadataStatus"/> enumeration.
/// </summary>
[ExcludeFromCodeCoverage]
public class MetadataStatusTests
{
    [Fact]
    public void MetadataStatus_WhenEnumeratingValues_ShouldHaveNoDuplicateValues()
    {
        // Act
        MetadataStatus[] values = Enum.GetValues<MetadataStatus>();

        // Assert
        Assert.Equal(values.Length, values.Distinct().Count());
    }
}
