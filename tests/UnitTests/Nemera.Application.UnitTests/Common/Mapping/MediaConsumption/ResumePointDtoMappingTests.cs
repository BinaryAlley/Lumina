#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DTO.MediaConsumption;
using Lumina.Application.Common.Mapping.MediaConsumption;
using Lumina.Application.Fixtures.Common.DTO.MediaConsumption;
using Lumina.Contracts.Responses.MediaConsumption;
using Lumina.Domain.SharedKernel.Common.Enums.MediaConsumption;
using Lumina.Domain.SharedKernel.Common.Enums.MediaLibrary;
using System;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Application.UnitTests.Common.Mapping.MediaConsumption;

/// <summary>
/// Contains unit tests for the <see cref="ResumePointDtoMapping"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class ResumePointDtoMappingTests
{
    private readonly ResumePointDtoFixture _resumePointDtoFixture = new();
    private readonly MediaPositionDtoFixture _mediaPositionDtoFixture = new();

    [Fact]
    public void ToResponse_WhenMappingResumePoint_ShouldMapAllPropertiesIncludingThePosition()
    {
        // Arrange
        Guid id = Guid.NewGuid();
        Guid libraryId = Guid.NewGuid();
        Guid mediaItemId = Guid.NewGuid();
        DateTime lastConsumedOnUtc = new(2024, 7, 4, 20, 15, 0, DateTimeKind.Utc);
        MediaPositionDto lastPosition = _mediaPositionDtoFixture.Create(
            kind: MediaPositionKind.TimeOffset,
            timeOffsetMilliseconds: 2_500,
            includeLocation: false);
        ResumePointDto resumePoint = _resumePointDtoFixture.Create(
            id: id,
            libraryId: libraryId,
            mediaItemId: mediaItemId,
            mediaKind: MediaKind.Movie,
            lastPosition: lastPosition,
            lastProgressFraction: 0.6,
            lastConsumedOnUtc: lastConsumedOnUtc);

        // Act
        ResumePointResponse result = resumePoint.ToResponse();

        // Assert
        Assert.Equal(id, result.Id);
        Assert.Equal(libraryId, result.LibraryId);
        Assert.Equal(mediaItemId, result.MediaItemId);
        Assert.Equal(MediaKind.Movie, result.MediaKind);
        Assert.Equal(MediaPositionKind.TimeOffset, result.LastPosition.Kind);
        Assert.Equal(2.5, result.LastPosition.TimeOffsetSeconds);
        Assert.Null(result.LastPosition.Location);
        Assert.Equal(0.6, result.LastProgressFraction);
        Assert.Equal(lastConsumedOnUtc, result.LastConsumedOnUtc);
    }

    [Fact]
    public void ToResponse_WhenLastConsumedOnUtcIsUnspecified_ShouldSpecifyUtcWithoutChangingTicks()
    {
        // Arrange
        DateTime lastConsumedOnUtc = new(2024, 7, 4, 20, 15, 30, DateTimeKind.Unspecified);
        ResumePointDto resumePoint = _resumePointDtoFixture.Create(lastConsumedOnUtc: lastConsumedOnUtc);

        // Act
        ResumePointResponse result = resumePoint.ToResponse();

        // Assert
        Assert.Equal(DateTimeKind.Utc, result.LastConsumedOnUtc.Kind);
        Assert.Equal(lastConsumedOnUtc.Ticks, result.LastConsumedOnUtc.Ticks);
    }

    [Fact]
    public void ToResponse_WhenPositionIsTimeOffset_ShouldMapTheTimeOffsetThroughTheNestedMapping()
    {
        // Arrange
        MediaPositionDto lastPosition = _mediaPositionDtoFixture.Create(
            kind: MediaPositionKind.TimeOffset,
            timeOffsetMilliseconds: 1_234,
            includeLocation: false);
        ResumePointDto resumePoint = _resumePointDtoFixture.Create(lastPosition: lastPosition);

        // Act
        ResumePointResponse result = resumePoint.ToResponse();

        // Assert
        Assert.Equal(MediaPositionKind.TimeOffset, result.LastPosition.Kind);
        Assert.Equal(1.234, result.LastPosition.TimeOffsetSeconds);
        Assert.Null(result.LastPosition.Location);
    }

    [Fact]
    public void ToResponse_WhenPositionIsLocation_ShouldMapTheLocationThroughTheNestedMapping()
    {
        // Arrange
        MediaPositionDto lastPosition = _mediaPositionDtoFixture.Create(
            kind: MediaPositionKind.Location,
            location: "chapter-1",
            includeTimeOffsetMilliseconds: false);
        ResumePointDto resumePoint = _resumePointDtoFixture.Create(
            mediaKind: MediaKind.Book,
            lastPosition: lastPosition);

        // Act
        ResumePointResponse result = resumePoint.ToResponse();

        // Assert
        Assert.Equal(MediaPositionKind.Location, result.LastPosition.Kind);
        Assert.Null(result.LastPosition.TimeOffsetSeconds);
        Assert.Equal("chapter-1", result.LastPosition.Location);
    }
}
