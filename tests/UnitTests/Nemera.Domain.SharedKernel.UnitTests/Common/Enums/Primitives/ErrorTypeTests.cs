#region ========================================================================= USING =====================================================================================
using Lumina.Domain.SharedKernel.Common.Enums.Primitives;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Domain.SharedKernel.UnitTests.Common.Enums.Primitives;

/// <summary>
/// Contains unit tests for the <see cref="ErrorType"/> enumeration.
/// </summary>
[ExcludeFromCodeCoverage]
public class ErrorTypeTests
{
    [Fact]
    public void ErrorType_WhenEnumeratingValues_ShouldHaveNoDuplicateValues()
    {
        // Act
        ErrorType[] values = Enum.GetValues<ErrorType>();

        // Assert
        Assert.Equal(values.Length, values.Distinct().Count());
    }
}
