#region ========================================================================= USING =====================================================================================
using Lumina.Domain.SharedKernel.Common.Enums.PhotoLibrary;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Domain.SharedKernel.UnitTests.Common.Enums.PhotoLibrary;

/// <summary>
/// Contains unit tests for the <see cref="VisualContentType"/> enumeration.
/// </summary>
[ExcludeFromCodeCoverage]
public class VisualContentTypeTests
{
    [Fact]
    public void VisualContentType_WhenEnumeratingValues_ShouldHaveNoDuplicateValues()
    {
        // Act
        VisualContentType[] values = Enum.GetValues<VisualContentType>();

        // Assert
        Assert.Equal(values.Length, values.Distinct().Count());
    }
}
