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

    [Fact]
    public void Constructor_WhenOmittingPosition_ShouldDefaultToNull()
    {
        // Act
        ReportConsumptionProgressRequest sut = _reportConsumptionProgressRequestFixture.Create(includePosition: false);

        // Assert
        Assert.Null(sut.Position);
    }

    [Fact]
    public void Constructor_WhenOmittingProgressFraction_ShouldDefaultToNull()
    {
        // Act
        ReportConsumptionProgressRequest sut = _reportConsumptionProgressRequestFixture.Create(includeProgressFraction: false);

        // Assert
        Assert.Null(sut.ProgressFraction);
    }

    [Fact]
    public void Constructor_WhenOmittingConsumedDurationInSeconds_ShouldDefaultToNull()
    {
        // Act
        ReportConsumptionProgressRequest sut = _reportConsumptionProgressRequestFixture.Create(includeConsumedDurationInSeconds: false);

        // Assert
        Assert.Null(sut.ConsumedDurationInSeconds);
    }

    [Fact]
    public void Constructor_WhenProgressFractionAndConsumedDurationAreZero_ShouldPreserveZero()
    {
        // Act
        ReportConsumptionProgressRequest sut = _reportConsumptionProgressRequestFixture.Create(
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
        ReportConsumptionProgressRequest sut = _reportConsumptionProgressRequestFixture.Create(
            progressFraction: 0.25,
            consumedDurationInSeconds: 600,
            consecutivePagesRead: 7);

        // Assert
        Assert.True(sut.ProgressFraction.HasValue);
        Assert.Equal(0.25, sut.ProgressFraction.Value);
        Assert.True(sut.ConsumedDurationInSeconds.HasValue);
        Assert.Equal(600, sut.ConsumedDurationInSeconds.Value);
        Assert.Equal(7, sut.ConsecutivePagesRead);
    }
}
