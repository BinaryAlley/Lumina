#region ========================================================================= USING =====================================================================================
using Lumina.Domain.SharedKernel.Common.Enums.MediaLibrary;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Domain.SharedKernel.UnitTests.Common.Enums.MediaLibrary;

/// <summary>
/// Contains unit tests for the <see cref="MiscellaneousContentType"/> enumeration.
/// </summary>
[ExcludeFromCodeCoverage]
public class MiscellaneousContentTypeTests
{
    [Fact]
    public void MiscellaneousContentType_WhenEnumeratingValues_ShouldHaveNoDuplicateValues()
    {
        // Act
        MiscellaneousContentType[] values = Enum.GetValues<MiscellaneousContentType>();

        // Assert
        Assert.Equal(values.Length, values.Distinct().Count());
    }
}
