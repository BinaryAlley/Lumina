#region ========================================================================= USING =====================================================================================
using Lumina.Domain.SharedKernel.Common.Enums.BookLibrary;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Domain.SharedKernel.UnitTests.Common.Enums.BookLibrary;

/// <summary>
/// Contains unit tests for the <see cref="BookFormat"/> enumeration.
/// </summary>
[ExcludeFromCodeCoverage]
public class BookFormatTests
{
    [Fact]
    public void BookFormat_WhenEnumeratingValues_ShouldHaveNoDuplicateValues()
    {
        // Act
        BookFormat[] values = Enum.GetValues<BookFormat>();

        // Assert
        Assert.Equal(values.Length, values.Distinct().Count());
    }
}
