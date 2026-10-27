#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.Mapping.MediaConsumption;
using Lumina.Application.Core.MediaConsumption.Commands.StopConsumption;
using Lumina.Contracts.Fixtures.Core.Requests.MediaConsumption;
using Lumina.Contracts.Requests.MediaConsumption;
using Lumina.Domain.SharedKernel.Common.Enums.MediaConsumption;
using System;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Application.UnitTests.Common.Mapping.MediaConsumption;

/// <summary>
/// Contains unit tests for the <see cref="StopConsumptionRequestMapping"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class StopConsumptionRequestMappingTests
{
    private readonly StopConsumptionRequestFixture _stopConsumptionRequestFixture = new();
    private readonly MediaPositionRequestFixture _mediaPositionRequestFixture = new();

    [Fact]
    public void ToCommand_WhenMappingValidTimeOffsetRequest_ShouldMapEveryRequestField()
    {
        // Arrange
        Guid consumptionRecordId = Guid.NewGuid();
        MediaPositionRequest position = _mediaPositionRequestFixture.Create(
            kind: nameof(MediaPositionKind.TimeOffset),
            timeOffsetSeconds: 1234.5);
        StopConsumptionRequest request = _stopConsumptionRequestFixture.Create(
            position: position,
            progressFraction: 0.75,
            consumedDurationInSeconds: 300,
            consecutivePagesRead: 12);

        // Act
        StopConsumptionCommand result = request.ToCommand(consumptionRecordId.ToString());

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
        Assert.True(result.ConsecutivePagesRead.HasValue);
        Assert.Equal(12, result.ConsecutivePagesRead.Value);
    }

    [Fact]
    public void ToCommand_WhenMappingValidLocationRequest_ShouldMapLocationPosition()
    {
        // Arrange
        Guid consumptionRecordId = Guid.NewGuid();
        MediaPositionRequest position = _mediaPositionRequestFixture.Create(
            kind: nameof(MediaPositionKind.Location),
            location: "chapter-2");
        StopConsumptionRequest request = _stopConsumptionRequestFixture.Create(position: position);

        // Act
        StopConsumptionCommand result = request.ToCommand(consumptionRecordId.ToString());

        // Assert
        Assert.Equal(nameof(MediaPositionKind.Location), result.PositionKind);
        Assert.Null(result.PositionTimeOffsetInSeconds);
        Assert.Equal("chapter-2", result.PositionLocation);
    }

    [Fact]
    public void ToCommand_WhenPositionIsMissing_ShouldMapNullPositionFields()
    {
        // Arrange
        StopConsumptionRequest request = _stopConsumptionRequestFixture.Create(includePosition: false);

        // Act
        StopConsumptionCommand result = request.ToCommand(Guid.NewGuid().ToString());

        // Assert
        Assert.Null(result.PositionKind);
        Assert.Null(result.PositionTimeOffsetInSeconds);
        Assert.Null(result.PositionLocation);
    }

    [Fact]
    public void ToCommand_WhenProgressFractionIsExplicitlyNull_ShouldPreserveNull()
    {
        // Arrange
        StopConsumptionRequest request = _stopConsumptionRequestFixture.Create(includeProgressFraction: false);

        // Act
        StopConsumptionCommand result = request.ToCommand(Guid.NewGuid().ToString());

        // Assert
        Assert.Null(result.ProgressFraction);
    }

    [Fact]
    public void ToCommand_WhenConsumedDurationIsExplicitlyNull_ShouldPreserveNull()
    {
        // Arrange
        StopConsumptionRequest request = _stopConsumptionRequestFixture.Create(includeConsumedDurationInSeconds: false);

        // Act
        StopConsumptionCommand result = request.ToCommand(Guid.NewGuid().ToString());

        // Assert
        Assert.Null(result.ConsumedDurationInSeconds);
    }

    [Fact]
    public void ToCommand_WhenProgressFractionAndConsumedDurationAreZero_ShouldPreserveZero()
    {
        // Arrange
        StopConsumptionRequest request = _stopConsumptionRequestFixture.Create(
            progressFraction: 0,
            consumedDurationInSeconds: 0);

        // Act
        StopConsumptionCommand result = request.ToCommand(Guid.NewGuid().ToString());

        // Assert
        Assert.True(result.ProgressFraction.HasValue);
        Assert.Equal(0.0, result.ProgressFraction.Value);
        Assert.True(result.ConsumedDurationInSeconds.HasValue);
        Assert.Equal(0.0, result.ConsumedDurationInSeconds.Value);
    }

    [Fact]
    public void ToCommand_WhenConsecutivePagesReadIsOmitted_ShouldMapNull()
    {
        // Arrange
        StopConsumptionRequest request = _stopConsumptionRequestFixture.Create(includeConsecutivePagesRead: false);

        // Act
        StopConsumptionCommand result = request.ToCommand(Guid.NewGuid().ToString());

        // Assert
        Assert.Null(result.ConsecutivePagesRead);
    }

    [Fact]
    public void ToCommand_WhenRouteIdIsSupplied_ShouldUseItForTheConsumptionRecordId()
    {
        // Arrange
        Guid routeConsumptionRecordId = Guid.NewGuid();
        StopConsumptionRequest request = _stopConsumptionRequestFixture.Create();

        // Act
        StopConsumptionCommand result = request.ToCommand(routeConsumptionRecordId.ToString());

        // Assert
        Assert.Equal(routeConsumptionRecordId.ToString(), result.ConsumptionRecordId);
    }
}
