#region ========================================================================= USING =====================================================================================
using Lumina.Contracts.Fixtures.Core.Requests.MediaConsumption;
using Lumina.Contracts.Requests.MediaConsumption;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Contracts.UnitTests.Requests.MediaConsumption;

/// <summary>
/// Contains unit tests for the <see cref="ReportConsumptionProgressRequest"/> record.
/// </summary>
[ExcludeFromCodeCoverage]
public class ReportConsumptionProgressRequestTests
{
    private readonly ReportConsumptionProgressRequestFixture _reportConsumptionProgressRequestFixture = new();

    [Fact]
    public void Constructor_WhenOmittingConsecutivePagesRead_ShouldDefaultToZero()
    {
        // Act
        ReportConsumptionProgressRequest sut = _reportConsumptionProgressRequestFixture.Create(includeConsecutivePagesRead: false);

        // Assert
        Assert.Equal(0, sut.ConsecutivePagesRead);
    }

    [Fact]
    public void Constructor_WhenConsecutivePagesReadIsSupplied_ShouldPreserveIt()
    {
        // Act
        ReportConsumptionProgressRequest sut = _reportConsumptionProgressRequestFixture.Create(consecutivePagesRead: 42);

        // Assert
        Assert.Equal(42, sut.ConsecutivePagesRead);
    }
}
