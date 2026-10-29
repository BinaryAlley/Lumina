#region ========================================================================= USING =====================================================================================
using Lumina.Contracts.Fixtures.Core.Requests.MediaConsumption;
using Lumina.Contracts.Requests.MediaConsumption;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Contracts.UnitTests.Requests.MediaConsumption;

/// <summary>
/// Contains unit tests for the <see cref="StopConsumptionRequest"/> record.
/// </summary>
[ExcludeFromCodeCoverage]
public class StopConsumptionRequestTests
{
    private readonly StopConsumptionRequestFixture _stopConsumptionRequestFixture = new();

    [Fact]
    public void Constructor_WhenOmittingConsecutivePagesRead_ShouldDefaultToNull()
    {
        // Act
        StopConsumptionRequest sut = _stopConsumptionRequestFixture.Create(includeConsecutivePagesRead: false);

        // Assert
        Assert.Null(sut.ConsecutivePagesRead);
    }

    [Fact]
    public void Constructor_WhenConsecutivePagesReadIsSupplied_ShouldPreserveIt()
    {
        // Act
        StopConsumptionRequest sut = _stopConsumptionRequestFixture.Create(consecutivePagesRead: 42);

        // Assert
        Assert.True(sut.ConsecutivePagesRead.HasValue);
        Assert.Equal(42, sut.ConsecutivePagesRead.Value);
    }

    [Fact]
    public void Constructor_WhenOmittingPosition_ShouldDefaultToNull()
    {
        // Act
        StopConsumptionRequest sut = _stopConsumptionRequestFixture.Create(includePosition: false);

        // Assert
        Assert.Null(sut.Position);
    }

    [Fact]
    public void Constructor_WhenOmittingProgressFraction_ShouldDefaultToNull()
    {
        // Act
        StopConsumptionRequest sut = _stopConsumptionRequestFixture.Create(includeProgressFraction: false);

        // Assert
        Assert.Null(sut.ProgressFraction);
    }

    [Fact]
    public void Constructor_WhenOmittingConsumedDurationInSeconds_ShouldDefaultToNull()
    {
        // Act
        StopConsumptionRequest sut = _stopConsumptionRequestFixture.Create(includeConsumedDurationInSeconds: false);

        // Assert
        Assert.Null(sut.ConsumedDurationInSeconds);
    }

    [Fact]
    public void Constructor_WhenProgressFractionAndConsumedDurationAreZero_ShouldPreserveZero()
    {
        // Act
        StopConsumptionRequest sut = _stopConsumptionRequestFixture.Create(
            progressFraction: 0,
            consumedDurationInSeconds: 0);

        // Assert
        Assert.True(sut.ProgressFraction.HasValue);
        Assert.Equal(0.0, sut.ProgressFraction.Value);
        Assert.True(sut.ConsumedDurationInSeconds.HasValue);
        Assert.Equal(0.0, sut.ConsumedDurationInSeconds.Value);
    }

    [Fact]
    public void Constructor_WhenNonDefaultValuesAreSupplied_ShouldPreserveThem()
    {
        // Act
        StopConsumptionRequest sut = _stopConsumptionRequestFixture.Create(
            progressFraction: 0.25,
            consumedDurationInSeconds: 600,
            consecutivePagesRead: 7);

        // Assert
        Assert.True(sut.ProgressFraction.HasValue);
        Assert.Equal(0.25, sut.ProgressFraction.Value);
        Assert.True(sut.ConsumedDurationInSeconds.HasValue);
        Assert.Equal(600, sut.ConsumedDurationInSeconds.Value);
        Assert.True(sut.ConsecutivePagesRead.HasValue);
        Assert.Equal(7, sut.ConsecutivePagesRead.Value);
    }
}
