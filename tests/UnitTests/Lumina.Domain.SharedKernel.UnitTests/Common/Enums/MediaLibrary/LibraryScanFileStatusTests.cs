#region ========================================================================= USING =====================================================================================
using Lumina.Domain.SharedKernel.Common.Enums.MediaLibrary;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Domain.SharedKernel.UnitTests.Common.Enums.MediaLibrary;

/// <summary>
/// Contains unit tests for the <see cref="LibraryScanFileStatus"/> enumeration.
/// </summary>
[ExcludeFromCodeCoverage]
public class LibraryScanFileStatusTests
{
    [Fact]
    public void LibraryScanFileStatus_WhenEnumeratingValues_ShouldHaveNoDuplicateValues()
    {
        // Act
        LibraryScanFileStatus[] values = Enum.GetValues<LibraryScanFileStatus>();

        // Assert
        Assert.Equal(values.Length, values.Distinct().Count());
    }
}
