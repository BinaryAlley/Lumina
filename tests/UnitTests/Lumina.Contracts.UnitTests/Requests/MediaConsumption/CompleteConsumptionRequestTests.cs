#region ========================================================================= USING =====================================================================================
using Lumina.Contracts.Fixtures.Core.Requests.MediaConsumption;
using Lumina.Contracts.Requests.MediaConsumption;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Contracts.UnitTests.Requests.MediaConsumption;

/// <summary>
/// Contains unit tests for the <see cref="CompleteConsumptionRequest"/> record.
/// </summary>
[ExcludeFromCodeCoverage]
public class CompleteConsumptionRequestTests
{
    private readonly CompleteConsumptionRequestFixture _completeConsumptionRequestFixture = new();

    [Fact]
    public void Constructor_WhenOmittingConsecutivePagesRead_ShouldDefaultToNull()
    {
        // Act
        CompleteConsumptionRequest sut = _completeConsumptionRequestFixture.Create(includeConsecutivePagesRead: false);

        // Assert
        Assert.Null(sut.ConsecutivePagesRead);
    }

    [Fact]
    public void Constructor_WhenConsecutivePagesReadIsSupplied_ShouldPreserveIt()
    {
        // Act
        CompleteConsumptionRequest sut = _completeConsumptionRequestFixture.Create(consecutivePagesRead: 42);

        // Assert
        Assert.True(sut.ConsecutivePagesRead.HasValue);
        Assert.Equal(42, sut.ConsecutivePagesRead.Value);
    }
}
