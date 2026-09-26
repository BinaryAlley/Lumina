#region ========================================================================= USING =====================================================================================
using Lumina.Domain.SharedKernel.Common.Enums.BookLibrary;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Domain.SharedKernel.UnitTests.Common.Enums.BookLibrary;

/// <summary>
/// Contains unit tests for the <see cref="BookRatingSource"/> enumeration.
/// </summary>
[ExcludeFromCodeCoverage]
public class BookRatingSourceTests
{
    [Fact]
    public void BookRatingSource_WhenEnumeratingValues_ShouldHaveNoDuplicateValues()
    {
        // Act
        BookRatingSource[] values = Enum.GetValues<BookRatingSource>();

        // Assert
        Assert.Equal(values.Length, values.Distinct().Count());
    }
}
