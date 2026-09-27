#region ========================================================================= USING =====================================================================================
using Lumina.Domain.SharedKernel.Common.Enums.BookLibrary;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Domain.SharedKernel.UnitTests.Common.Enums.BookLibrary;

/// <summary>
/// Contains unit tests for the <see cref="WrittenContentType"/> enumeration.
/// </summary>
[ExcludeFromCodeCoverage]
public class WrittenContentTypeTests
{
    [Fact]
    public void WrittenContentType_WhenEnumeratingValues_ShouldHaveNoDuplicateValues()
    {
        // Act
        WrittenContentType[] values = Enum.GetValues<WrittenContentType>();

        // Assert
        Assert.Equal(values.Length, values.Distinct().Count());
    }
}
