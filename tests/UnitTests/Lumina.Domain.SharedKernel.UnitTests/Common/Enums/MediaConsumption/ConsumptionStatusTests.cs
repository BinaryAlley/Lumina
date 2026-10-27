#region ========================================================================= USING =====================================================================================
using Lumina.Domain.SharedKernel.Common.Enums.MediaConsumption;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Domain.SharedKernel.UnitTests.Common.Enums.MediaConsumption;

/// <summary>
/// Contains unit tests for the <see cref="ConsumptionStatus"/> enumeration.
/// </summary>
[ExcludeFromCodeCoverage]
public class ConsumptionStatusTests
{
    [Fact]
    public void ConsumptionStatus_WhenEnumeratingValues_ShouldHaveNoDuplicateValues()
    {
        // Act
        ConsumptionStatus[] values = Enum.GetValues<ConsumptionStatus>();

        // Assert
        Assert.Equal(values.Length, values.Distinct().Count());
    }
}
