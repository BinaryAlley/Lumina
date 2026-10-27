#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DTO.MediaConsumption;
using Lumina.Application.Common.Mapping.MediaConsumption;
using Lumina.Contracts.Responses.MediaConsumption;
using Lumina.Domain.Core.BoundedContexts.MediaConsumptionBoundedContext.Common.ValueObjects;
using Lumina.Domain.Fixtures.Core.BoundedContexts.MediaConsumptionBoundedContext.Common.ValueObjects;
using Lumina.Domain.SharedKernel.Common.Enums.MediaConsumption;
using System;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Application.UnitTests.Common.Mapping.MediaConsumption;

/// <summary>
/// Contains unit tests for the <see cref="MediaPositionMapping"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class MediaPositionMappingTests
{
    private readonly MediaPositionFixture _mediaPositionFixture = new();

    [Fact]
    public void ToDto_WhenKindIsTimeOffset_ShouldMapTruncatedMillisecondsAndNullLocation()
    {
        // Arrange
        MediaPosition position = _mediaPositionFixture.Create(
            kind: MediaPositionKind.TimeOffset,
            timeOffset: TimeSpan.FromMilliseconds(1_500));

        // Act
        MediaPositionDto result = position.ToDto();

        // Assert
        Assert.Equal(MediaPositionKind.TimeOffset, result.Kind);
        Assert.Equal(1_500L, result.TimeOffsetMilliseconds);
        Assert.Null(result.Location);
    }

    [Fact]
    public void ToDto_WhenKindIsLocation_ShouldMapLocationAndNullTimeOffset()
    {
        // Arrange
        MediaPosition position = _mediaPositionFixture.Create(
            kind: MediaPositionKind.Location,
            location: "chapter-1");

        // Act
        MediaPositionDto result = position.ToDto();

        // Assert
        Assert.Equal(MediaPositionKind.Location, result.Kind);
        Assert.Null(result.TimeOffsetMilliseconds);
        Assert.Equal("chapter-1", result.Location);
    }

    [Fact]
    public void ToDto_WhenTimeOffsetHasFractionalMilliseconds_ShouldTruncateTowardZero()
    {
        // Arrange
        MediaPosition position = _mediaPositionFixture.Create(
            kind: MediaPositionKind.TimeOffset,
            timeOffset: TimeSpan.FromTicks(15_600));

        // Act
        MediaPositionDto result = position.ToDto();

        // Assert
        Assert.Equal(1L, result.TimeOffsetMilliseconds);
    }

    [Fact]
    public void ToResponse_WhenKindIsTimeOffset_ShouldPreserveFractionalSeconds()
    {
        // Arrange
        MediaPosition position = _mediaPositionFixture.Create(
            kind: MediaPositionKind.TimeOffset,
            timeOffset: TimeSpan.FromMilliseconds(1_500));

        // Act
        MediaPositionResponse result = position.ToResponse();

        // Assert
        Assert.Equal(MediaPositionKind.TimeOffset, result.Kind);
        Assert.Equal(1.5, result.TimeOffsetSeconds);
        Assert.Null(result.Location);
    }

    [Fact]
    public void ToResponse_WhenKindIsLocation_ShouldMapLocationAndNullTimeOffset()
    {
        // Arrange
        MediaPosition position = _mediaPositionFixture.Create(
            kind: MediaPositionKind.Location,
            location: "chapter-1");

        // Act
        MediaPositionResponse result = position.ToResponse();

        // Assert
        Assert.Equal(MediaPositionKind.Location, result.Kind);
        Assert.Null(result.TimeOffsetSeconds);
        Assert.Equal("chapter-1", result.Location);
    }

    [Fact]
    public void ToResponse_WhenTimeOffsetIsMaximumTimeSpan_ShouldMapMaximumSeconds()
    {
        // Arrange
        MediaPosition position = _mediaPositionFixture.Create(
            kind: MediaPositionKind.TimeOffset,
            timeOffset: TimeSpan.MaxValue);

        // Act
        MediaPositionResponse result = position.ToResponse();

        // Assert
        Assert.Equal(TimeSpan.MaxValue.TotalSeconds, result.TimeOffsetSeconds);
    }
}
