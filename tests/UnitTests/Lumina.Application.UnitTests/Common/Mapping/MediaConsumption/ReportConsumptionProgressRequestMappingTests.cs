#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.Mapping.MediaConsumption;
using Lumina.Application.Core.MediaConsumption.Commands.ReportConsumptionProgress;
using Lumina.Contracts.Fixtures.Core.Requests.MediaConsumption;
using Lumina.Contracts.Requests.MediaConsumption;
using Lumina.Domain.SharedKernel.Common.Enums.MediaConsumption;
using System;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Application.UnitTests.Common.Mapping.MediaConsumption;

/// <summary>
/// Contains unit tests for the <see cref="ReportConsumptionProgressRequestMapping"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class ReportConsumptionProgressRequestMappingTests
{
    private readonly ReportConsumptionProgressRequestFixture _reportConsumptionProgressRequestFixture = new();
    private readonly MediaPositionRequestFixture _mediaPositionRequestFixture = new();

    [Fact]
    public void ToCommand_WhenMappingValidTimeOffsetRequest_ShouldMapEveryRequestField()
    {
        // Arrange
        Guid consumptionRecordId = Guid.NewGuid();
        MediaPositionRequest position = _mediaPositionRequestFixture.Create(
            kind: nameof(MediaPositionKind.TimeOffset),
            timeOffsetSeconds: 1234.5);
        ReportConsumptionProgressRequest request = _reportConsumptionProgressRequestFixture.Create(
            position: position,
            progressFraction: 0.75,
            consumedDurationInSeconds: 300,
            consecutivePagesRead: 12);

        // Act
        ReportConsumptionProgressCommand result = request.ToCommand(consumptionRecordId.ToString());

        // Assert
        Assert.Equal(consumptionRecordId.ToString(), result.ConsumptionRecordId);
        Assert.Equal(nameof(MediaPositionKind.TimeOffset), result.PositionKind);
        Assert.True(result.PositionTimeOffsetInSeconds.HasValue);
        Assert.Equal(1234.5, result.PositionTimeOffsetInSeconds.Value);
        Assert.Null(result.PositionLocation);
        Assert.True(result.ProgressFraction.HasValue);
        Assert.Equal(0.75, result.ProgressFraction.Value);
        Assert.True(result.ConsumedDurationInSeconds.HasValue);
        Assert.Equal(300, result.ConsumedDurationInSeconds.Value);
        Assert.Equal(12, result.ConsecutivePagesRead);
    }

    [Fact]
    public void ToCommand_WhenMappingValidLocationRequest_ShouldMapLocationPosition()
    {
        // Arrange
        Guid consumptionRecordId = Guid.NewGuid();
        MediaPositionRequest position = _mediaPositionRequestFixture.Create(
            kind: nameof(MediaPositionKind.Location),
            location: "chapter-2");
        ReportConsumptionProgressRequest request = _reportConsumptionProgressRequestFixture.Create(position: position);

        // Act
        ReportConsumptionProgressCommand result = request.ToCommand(consumptionRecordId.ToString());

        // Assert
        Assert.Equal(nameof(MediaPositionKind.Location), result.PositionKind);
        Assert.Null(result.PositionTimeOffsetInSeconds);
        Assert.Equal("chapter-2", result.PositionLocation);
    }

    [Fact]
    public void ToCommand_WhenPositionIsMissing_ShouldMapNullPositionFields()
    {
        // Arrange
        ReportConsumptionProgressRequest request = _reportConsumptionProgressRequestFixture.Create(includePosition: false);

        // Act
        ReportConsumptionProgressCommand result = request.ToCommand(Guid.NewGuid().ToString());

        // Assert
        Assert.Null(result.PositionKind);
        Assert.Null(result.PositionTimeOffsetInSeconds);
        Assert.Null(result.PositionLocation);
    }

    [Fact]
    public void ToCommand_WhenProgressFractionIsExplicitlyNull_ShouldPreserveNull()
    {
        // Arrange
        ReportConsumptionProgressRequest request = _reportConsumptionProgressRequestFixture.Create(includeProgressFraction: false);

        // Act
        ReportConsumptionProgressCommand result = request.ToCommand(Guid.NewGuid().ToString());

        // Assert
        Assert.Null(result.ProgressFraction);
    }

    [Fact]
    public void ToCommand_WhenConsumedDurationIsExplicitlyNull_ShouldPreserveNull()
    {
        // Arrange
        ReportConsumptionProgressRequest request = _reportConsumptionProgressRequestFixture.Create(includeConsumedDurationInSeconds: false);

        // Act
        ReportConsumptionProgressCommand result = request.ToCommand(Guid.NewGuid().ToString());

        // Assert
        Assert.Null(result.ConsumedDurationInSeconds);
    }

    [Fact]
    public void ToCommand_WhenProgressFractionAndConsumedDurationAreZero_ShouldPreserveZero()
    {
        // Arrange
        ReportConsumptionProgressRequest request = _reportConsumptionProgressRequestFixture.Create(
            progressFraction: 0,
            consumedDurationInSeconds: 0);

        // Act
        ReportConsumptionProgressCommand result = request.ToCommand(Guid.NewGuid().ToString());

        // Assert
        Assert.True(result.ProgressFraction.HasValue);
        Assert.Equal(0.0, result.ProgressFraction.Value);
        Assert.True(result.ConsumedDurationInSeconds.HasValue);
        Assert.Equal(0.0, result.ConsumedDurationInSeconds.Value);
    }

    [Fact]
    public void ToCommand_WhenConsecutivePagesReadIsOmitted_ShouldMapTheDefaultOfZero()
    {
        // Arrange
        ReportConsumptionProgressRequest request = _reportConsumptionProgressRequestFixture.Create(includeConsecutivePagesRead: false);

        // Act
        ReportConsumptionProgressCommand result = request.ToCommand(Guid.NewGuid().ToString());

        // Assert
        Assert.Equal(0, result.ConsecutivePagesRead);
    }

    [Fact]
    public void ToCommand_WhenConsecutivePagesReadIsSupplied_ShouldPreserveIt()
    {
        // Arrange
        ReportConsumptionProgressRequest request = _reportConsumptionProgressRequestFixture.Create(consecutivePagesRead: 12);

        // Act
        ReportConsumptionProgressCommand result = request.ToCommand(Guid.NewGuid().ToString());

        // Assert
        Assert.Equal(12, result.ConsecutivePagesRead);
    }

    [Fact]
    public void ToCommand_WhenRouteIdIsSupplied_ShouldUseItForTheConsumptionRecordId()
    {
        // Arrange
        Guid routeConsumptionRecordId = Guid.NewGuid();
        ReportConsumptionProgressRequest request = _reportConsumptionProgressRequestFixture.Create();

        // Act
        ReportConsumptionProgressCommand result = request.ToCommand(routeConsumptionRecordId.ToString());

        // Assert
        Assert.Equal(routeConsumptionRecordId.ToString(), result.ConsumptionRecordId);
    }
}
