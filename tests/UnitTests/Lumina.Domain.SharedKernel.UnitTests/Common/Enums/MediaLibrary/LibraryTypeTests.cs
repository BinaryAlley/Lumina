#region ========================================================================= USING =====================================================================================
using Lumina.Domain.SharedKernel.Common.Enums.MediaLibrary;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Domain.SharedKernel.UnitTests.Common.Enums.MediaLibrary;

/// <summary>
/// Contains unit tests for the <see cref="LibraryType"/> enumeration.
/// </summary>
[ExcludeFromCodeCoverage]
public class LibraryTypeTests
{
    [Fact]
    public void LibraryType_WhenEnumeratingValues_ShouldHaveNoDuplicateValues()
    {
        // Act
        LibraryType[] values = Enum.GetValues<LibraryType>();

        // Assert
        Assert.Equal(values.Length, values.Distinct().Count());
    }

    [Fact]
    public void Book_WhenCastingToInteger_ShouldBeZero()
    {
        // Act
        int value = (int)LibraryType.Book;

        // Assert
        Assert.Equal(0, value);
    }
}
