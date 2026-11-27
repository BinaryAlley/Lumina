#region ========================================================================= USING =====================================================================================
using Nemera.Domain.SharedKernel.Common.Enums.MediaLibrary;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Nemera.Domain.SharedKernel.UnitTests.Common.Enums.MediaLibrary;

/// <summary>
/// Contains unit tests for the <see cref="LibraryScanJobStatus"/> enumeration.
/// </summary>
[ExcludeFromCodeCoverage]
public class LibraryScanJobStatusTests
{
    [Fact]
    public void LibraryScanJobStatus_WhenEnumeratingValues_ShouldHaveNoDuplicateValues()
    {
        // Act
        LibraryScanJobStatus[] values = Enum.GetValues<LibraryScanJobStatus>();

        // Assert
        Assert.Equal(values.Length, values.Distinct().Count());
    }
}
