#region ========================================================================= USING =====================================================================================
using Nemera.Domain.SharedKernel.Common.Enums.BookLibrary;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Nemera.Domain.SharedKernel.UnitTests.Common.Enums.BookLibrary;

/// <summary>
/// Contains unit tests for the <see cref="IsbnFormat"/> enumeration.
/// </summary>
[ExcludeFromCodeCoverage]
public class IsbnFormatTests
{
    [Fact]
    public void IsbnFormat_WhenEnumeratingValues_ShouldHaveNoDuplicateValues()
    {
        // Act
        IsbnFormat[] values = Enum.GetValues<IsbnFormat>();

        // Assert
        Assert.Equal(values.Length, values.Distinct().Count());
    }
}
