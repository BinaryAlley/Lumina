#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DTO.MediaConsumption;
using Lumina.Application.Common.Mapping.MediaConsumption;
using Lumina.Application.Fixtures.Common.DTO.MediaConsumption;
using Lumina.Contracts.Responses.MediaConsumption;
using Lumina.Domain.SharedKernel.Common.Enums.MediaConsumption;
using System;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Application.UnitTests.Common.Mapping.MediaConsumption;

/// <summary>
/// Contains unit tests for the <see cref="MediaPositionDtoMapping"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class MediaPositionDtoMappingTests
{
    private readonly MediaPositionDtoFixture _mediaPositionDtoFixture = new();

    [Fact]
    public void ToResponse_WhenKindIsTimeOffset_ShouldMapMillisecondsToSeconds()
    {
        // Arrange
        MediaPositionDto position = _mediaPositionDtoFixture.Create(
            kind: MediaPositionKind.TimeOffset,
            timeOffsetMilliseconds: 1_500,
            includeLocation: false);

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
        MediaPositionDto position = _mediaPositionDtoFixture.Create(
            kind: MediaPositionKind.Location,
            location: "chapter-1",
            includeTimeOffsetMilliseconds: false);

        // Act
        MediaPositionResponse result = position.ToResponse();

        // Assert
        Assert.Equal(MediaPositionKind.Location, result.Kind);
        Assert.Null(result.TimeOffsetSeconds);
        Assert.Equal("chapter-1", result.Location);
    }

    [Fact]
    public void ToResponse_WhenTimeOffsetIsMissing_ShouldMapNullSeconds()
    {
        // Arrange
        MediaPositionDto position = _mediaPositionDtoFixture.Create(
            kind: MediaPositionKind.TimeOffset,
            includeTimeOffsetMilliseconds: false,
            includeLocation: false);

        // Act
        MediaPositionResponse result = position.ToResponse();

        // Assert
        Assert.Equal(MediaPositionKind.TimeOffset, result.Kind);
        Assert.Null(result.TimeOffsetSeconds);
        Assert.Null(result.Location);
    }

    [Fact]
    public void ToResponse_WhenTimeOffsetIsFractional_ShouldPreserveFractionalSeconds()
    {
        // Arrange
        MediaPositionDto position = _mediaPositionDtoFixture.Create(
            kind: MediaPositionKind.TimeOffset,
            timeOffsetMilliseconds: 1_234,
            includeLocation: false);

        // Act
        MediaPositionResponse result = position.ToResponse();

        // Assert
        Assert.Equal(1.234, result.TimeOffsetSeconds);
    }

    [Fact]
    public void ToResponse_WhenTimeOffsetExceedsTimeSpanRange_ShouldClampToMaximumRepresentableSeconds()
    {
        // Arrange
        MediaPositionDto position = _mediaPositionDtoFixture.Create(
            kind: MediaPositionKind.TimeOffset,
            timeOffsetMilliseconds: long.MaxValue,
            includeLocation: false);
        double expectedClampedSeconds = TimeSpan.FromMilliseconds(TimeSpan.MaxValue.Ticks / TimeSpan.TicksPerMillisecond).TotalSeconds;

        // Act
        MediaPositionResponse result = position.ToResponse();

        // Assert
        Assert.Equal(expectedClampedSeconds, result.TimeOffsetSeconds);
        Assert.True(result.TimeOffsetSeconds <= TimeSpan.MaxValue.TotalSeconds);
    }

    [Fact]
    public void ToResponse_WhenMappingLocation_ShouldCarryLocationThroughUnchanged()
    {
        // Arrange
        const string LOCATION = "epubcfi(/6/4!/4/2/2/2,/1:0,/1:10)";
        MediaPositionDto position = _mediaPositionDtoFixture.Create(
            kind: MediaPositionKind.Location,
            location: LOCATION,
            includeTimeOffsetMilliseconds: false);

        // Act
        MediaPositionResponse result = position.ToResponse();

        // Assert
        Assert.Equal(LOCATION, result.Location);
        Assert.Null(result.TimeOffsetSeconds);
    }
}
