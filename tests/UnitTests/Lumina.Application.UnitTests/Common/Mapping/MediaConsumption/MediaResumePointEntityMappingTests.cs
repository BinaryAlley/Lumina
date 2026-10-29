#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.MediaConsumption;
using Lumina.Application.Common.DTO.MediaConsumption;
using Lumina.Application.Common.Mapping.MediaConsumption;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.MediaConsumption;
using Lumina.Domain.Common.Errors;
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.Core.BoundedContexts.MediaConsumptionBoundedContext.MediaResumePointAggregate;
using Lumina.Domain.SharedKernel.Common.Enums.MediaConsumption;
using Lumina.Domain.SharedKernel.Common.Enums.MediaLibrary;
using System;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Application.UnitTests.Common.Mapping.MediaConsumption;

/// <summary>
/// Contains unit tests for the <see cref="MediaResumePointEntityMapping"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class MediaResumePointEntityMappingTests
{
    private readonly MediaResumePointEntityFixture _mediaResumePointEntityFixture = new();

    [Fact]
    public void ToDomainEntity_WhenTimeOffsetResumePointIsValid_ShouldMapAllPropertiesCorrectly()
    {
        // Arrange
        Guid id = Guid.NewGuid();
        Guid userId = Guid.NewGuid();
        Guid libraryId = Guid.NewGuid();
        Guid mediaItemId = Guid.NewGuid();
        DateTime lastConsumedOnUtc = new(2024, 10, 1, 18, 0, 0, DateTimeKind.Utc);
        DateTime createdOnUtc = new(2024, 9, 30, 18, 0, 0, DateTimeKind.Utc);
        DateTime updatedOnUtc = new(2024, 10, 1, 18, 0, 0, DateTimeKind.Utc);
        MediaResumePointEntity entity = _mediaResumePointEntityFixture.Create(
            id: id,
            userId: userId,
            libraryId: libraryId,
            mediaKind: MediaKind.MusicTrack,
            mediaItemId: mediaItemId,
            lastPositionKind: MediaPositionKind.TimeOffset,
            lastPositionTimeOffsetMilliseconds: 2_500,
            lastProgressFraction: 0.6,
            lastConsumedOnUtc: lastConsumedOnUtc,
            createdOnUtc: createdOnUtc,
            updatedOnUtc: updatedOnUtc);

        // Act
        Result<MediaResumePoint> result = entity.ToDomainEntity();

        // Assert
        Assert.True(result.IsSuccess);
        MediaResumePoint resumePoint = result.Value;
        Assert.Equal(id, resumePoint.Id.Value);
        Assert.Equal(userId, resumePoint.UserId.Value);
        Assert.Equal(libraryId, resumePoint.LibraryId.Value);
        Assert.Equal(mediaItemId, resumePoint.MediaItemId.Value);
        Assert.Equal(MediaKind.MusicTrack, resumePoint.MediaKind);
        Assert.Equal(MediaPositionKind.TimeOffset, resumePoint.LastPosition.Kind);
        Assert.True(resumePoint.LastPosition.TimeOffset.HasValue);
        Assert.Equal(TimeSpan.FromMilliseconds(2_500), resumePoint.LastPosition.TimeOffset.Value);
        Assert.False(resumePoint.LastPosition.Location.HasValue);
        Assert.Equal(0.6, resumePoint.LastProgressFraction);
        Assert.Equal(lastConsumedOnUtc, resumePoint.LastConsumedOnUtc);
        Assert.Equal(createdOnUtc, resumePoint.CreatedOnUtc);
        Assert.True(resumePoint.UpdatedOnUtc.HasValue);
        Assert.Equal(updatedOnUtc, resumePoint.UpdatedOnUtc.Value);
    }

    [Fact]
    public void ToDomainEntity_WhenTimeOffsetPositionIsExactlyTheMaximumRepresentableValue_ShouldMapTheMaximumPosition()
    {
        // Arrange
        long maximumTimeOffsetMilliseconds = TimeSpan.MaxValue.Ticks / TimeSpan.TicksPerMillisecond;
        MediaResumePointEntity entity = _mediaResumePointEntityFixture.Create(
            mediaKind: MediaKind.MusicTrack,
            lastPositionKind: MediaPositionKind.TimeOffset,
            lastPositionTimeOffsetMilliseconds: maximumTimeOffsetMilliseconds);

        // Act
        Result<MediaResumePoint> result = entity.ToDomainEntity();

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(MediaPositionKind.TimeOffset, result.Value.LastPosition.Kind);
        Assert.True(result.Value.LastPosition.TimeOffset.HasValue);
        Assert.Equal(TimeSpan.FromMilliseconds(maximumTimeOffsetMilliseconds), result.Value.LastPosition.TimeOffset.Value);
    }

    [Fact]
    public void ToDomainEntity_WhenLocationResumePointIsValid_ShouldMapAllPropertiesCorrectly()
    {
        // Arrange
        Guid id = Guid.NewGuid();
        Guid userId = Guid.NewGuid();
        Guid libraryId = Guid.NewGuid();
        Guid mediaItemId = Guid.NewGuid();
        DateTime lastConsumedOnUtc = new(2024, 10, 2, 9, 0, 0, DateTimeKind.Utc);
        DateTime createdOnUtc = new(2024, 10, 1, 9, 0, 0, DateTimeKind.Utc);
        MediaResumePointEntity entity = _mediaResumePointEntityFixture.Create(
            id: id,
            userId: userId,
            libraryId: libraryId,
            mediaKind: MediaKind.Book,
            mediaItemId: mediaItemId,
            lastPositionKind: MediaPositionKind.Location,
            lastPositionLocation: "chapter-1",
            lastProgressFraction: 0.4,
            lastConsumedOnUtc: lastConsumedOnUtc,
            createdOnUtc: createdOnUtc);

        // Act
        Result<MediaResumePoint> result = entity.ToDomainEntity();

        // Assert
        Assert.True(result.IsSuccess);
        MediaResumePoint resumePoint = result.Value;
        Assert.Equal(id, resumePoint.Id.Value);
        Assert.Equal(userId, resumePoint.UserId.Value);
        Assert.Equal(libraryId, resumePoint.LibraryId.Value);
        Assert.Equal(mediaItemId, resumePoint.MediaItemId.Value);
        Assert.Equal(MediaKind.Book, resumePoint.MediaKind);
        Assert.Equal(MediaPositionKind.Location, resumePoint.LastPosition.Kind);
        Assert.False(resumePoint.LastPosition.TimeOffset.HasValue);
        Assert.Equal("chapter-1", resumePoint.LastPosition.Location.Value);
        Assert.Equal(0.4, resumePoint.LastProgressFraction);
        Assert.Equal(lastConsumedOnUtc, resumePoint.LastConsumedOnUtc);
        Assert.Equal(createdOnUtc, resumePoint.CreatedOnUtc);
        Assert.False(resumePoint.UpdatedOnUtc.HasValue);
    }

    [Fact]
    public void ToDomainEntity_WhenUpdatedOnUtcIsMissing_ShouldMapNoUpdatedValue()
    {
        // Arrange
        MediaResumePointEntity entity = _mediaResumePointEntityFixture.Create(includeUpdatedOnUtc: false);

        // Act
        Result<MediaResumePoint> result = entity.ToDomainEntity();

        // Assert
        Assert.True(result.IsSuccess);
        Assert.False(result.Value.UpdatedOnUtc.HasValue);
    }

    [Fact]
    public void ToDomainEntity_WhenTimestampsAreUnspecified_ShouldSpecifyUtcWithoutChangingTicks()
    {
        // Arrange
        DateTime lastConsumedOnUtc = new(2024, 11, 3, 7, 45, 0, DateTimeKind.Unspecified);
        DateTime createdOnUtc = new(2024, 11, 2, 7, 45, 0, DateTimeKind.Unspecified);
        DateTime updatedOnUtc = new(2024, 11, 3, 7, 45, 30, DateTimeKind.Unspecified);
        MediaResumePointEntity entity = _mediaResumePointEntityFixture.Create(
            lastConsumedOnUtc: lastConsumedOnUtc,
            createdOnUtc: createdOnUtc,
            updatedOnUtc: updatedOnUtc);

        // Act
        Result<MediaResumePoint> result = entity.ToDomainEntity();

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(DateTimeKind.Utc, result.Value.LastConsumedOnUtc.Kind);
        Assert.Equal(lastConsumedOnUtc.Ticks, result.Value.LastConsumedOnUtc.Ticks);
        Assert.Equal(DateTimeKind.Utc, result.Value.CreatedOnUtc.Kind);
        Assert.Equal(createdOnUtc.Ticks, result.Value.CreatedOnUtc.Ticks);
        Assert.True(result.Value.UpdatedOnUtc.HasValue);
        Assert.Equal(DateTimeKind.Utc, result.Value.UpdatedOnUtc.Value.Kind);
        Assert.Equal(updatedOnUtc.Ticks, result.Value.UpdatedOnUtc.Value.Ticks);
    }

    [Fact]
    public void ToDomainEntity_WhenTimeOffsetPositionHasNoOffset_ShouldReturnInvalidMediaPosition()
    {
        // Arrange
        MediaResumePointEntity entity = _mediaResumePointEntityFixture.Create(
            mediaKind: MediaKind.MusicTrack,
            lastPositionKind: MediaPositionKind.TimeOffset,
            includeLastPositionTimeOffsetMilliseconds: false,
            includeLastPositionLocation: false);

        // Act
        Result<MediaResumePoint> result = entity.ToDomainEntity();

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.MediaConsumption.InvalidMediaPosition, result.FirstError);
    }

    [Fact]
    public void ToDomainEntity_WhenTimeOffsetPositionIsNegative_ShouldReturnInvalidMediaPosition()
    {
        // Arrange
        MediaResumePointEntity entity = _mediaResumePointEntityFixture.Create(
            mediaKind: MediaKind.MusicTrack,
            lastPositionKind: MediaPositionKind.TimeOffset,
            lastPositionTimeOffsetMilliseconds: -1);

        // Act
        Result<MediaResumePoint> result = entity.ToDomainEntity();

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.MediaConsumption.InvalidMediaPosition, result.FirstError);
    }

    [Fact]
    public void ToDomainEntity_WhenTimeOffsetPositionIsAboveTheRepresentableRange_ShouldReturnInvalidMediaPosition()
    {
        // Arrange
        long maximumTimeOffsetMilliseconds = TimeSpan.MaxValue.Ticks / TimeSpan.TicksPerMillisecond;
        MediaResumePointEntity entity = _mediaResumePointEntityFixture.Create(
            mediaKind: MediaKind.MusicTrack,
            lastPositionKind: MediaPositionKind.TimeOffset,
            lastPositionTimeOffsetMilliseconds: maximumTimeOffsetMilliseconds + 1);

        // Act
        Result<MediaResumePoint> result = entity.ToDomainEntity();

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.MediaConsumption.InvalidMediaPosition, result.FirstError);
    }

    [Fact]
    public void ToDomainEntity_WhenTimeOffsetPositionIsLongMaxValue_ShouldReturnInvalidMediaPosition()
    {
        // Arrange
        MediaResumePointEntity entity = _mediaResumePointEntityFixture.Create(
            mediaKind: MediaKind.MusicTrack,
            lastPositionKind: MediaPositionKind.TimeOffset,
            lastPositionTimeOffsetMilliseconds: long.MaxValue);

        // Act
        Result<MediaResumePoint> result = entity.ToDomainEntity();

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.MediaConsumption.InvalidMediaPosition, result.FirstError);
    }

    [Fact]
    public void ToDomainEntity_WhenTimeOffsetPositionAlsoHasALocation_ShouldReturnInvalidMediaPosition()
    {
        // Arrange
        MediaResumePointEntity entity = _mediaResumePointEntityFixture.Create(
            mediaKind: MediaKind.MusicTrack,
            lastPositionKind: MediaPositionKind.TimeOffset,
            lastPositionLocation: "chapter-1");

        // Act
        Result<MediaResumePoint> result = entity.ToDomainEntity();

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.MediaConsumption.InvalidMediaPosition, result.FirstError);
    }

    [Fact]
    public void ToDomainEntity_WhenLocationPositionHasNoLocation_ShouldReturnInvalidMediaPosition()
    {
        // Arrange
        MediaResumePointEntity entity = _mediaResumePointEntityFixture.Create(
            mediaKind: MediaKind.Book,
            lastPositionKind: MediaPositionKind.Location,
            includeLastPositionLocation: false,
            includeLastPositionTimeOffsetMilliseconds: false);

        // Act
        Result<MediaResumePoint> result = entity.ToDomainEntity();

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.MediaConsumption.InvalidMediaPosition, result.FirstError);
    }

    [Theory]
    [InlineData("")] // an empty location is not a usable position
    [InlineData("   ")] // a whitespace only location is not a usable position
    public void ToDomainEntity_WhenLocationPositionHasABlankLocation_ShouldReturnInvalidMediaPosition(string location)
    {
        // Arrange
        MediaResumePointEntity entity = _mediaResumePointEntityFixture.Create(
            mediaKind: MediaKind.Book,
            lastPositionKind: MediaPositionKind.Location,
            lastPositionLocation: location);

        // Act
        Result<MediaResumePoint> result = entity.ToDomainEntity();

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.MediaConsumption.InvalidMediaPosition, result.FirstError);
    }

    [Fact]
    public void ToDomainEntity_WhenLocationPositionAlsoHasATimeOffset_ShouldReturnInvalidMediaPosition()
    {
        // Arrange
        MediaResumePointEntity entity = _mediaResumePointEntityFixture.Create(
            mediaKind: MediaKind.Book,
            lastPositionKind: MediaPositionKind.Location,
            lastPositionTimeOffsetMilliseconds: 1_000,
            lastPositionLocation: "chapter-1");

        // Act
        Result<MediaResumePoint> result = entity.ToDomainEntity();

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.MediaConsumption.InvalidMediaPosition, result.FirstError);
    }

    [Fact]
    public void ToDomainEntity_WhenPositionKindIsUnsupported_ShouldReturnInvalidMediaPosition()
    {
        // Arrange
        MediaResumePointEntity entity = _mediaResumePointEntityFixture.Create(
            mediaKind: MediaKind.MusicTrack,
            lastPositionKind: (MediaPositionKind)int.MaxValue);

        // Act
        Result<MediaResumePoint> result = entity.ToDomainEntity();

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.MediaConsumption.InvalidMediaPosition, result.FirstError);
    }

    [Theory]
    [InlineData(MediaKind.MusicTrack, MediaPositionKind.Location, "chapter-1")] // music tracks are resumed from a time offset
    [InlineData(MediaKind.Book, MediaPositionKind.TimeOffset, null)] // books are resumed from a location
    public void ToDomainEntity_WhenPositionKindDoesNotMatchTheMediaKindRule_ShouldReturnInvalidMediaPosition(MediaKind mediaKind, MediaPositionKind positionKind, string? location)
    {
        // Arrange
        MediaResumePointEntity entity = _mediaResumePointEntityFixture.Create(
            mediaKind: mediaKind,
            lastPositionKind: positionKind,
            lastPositionLocation: location);

        // Act
        Result<MediaResumePoint> result = entity.ToDomainEntity();

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.MediaConsumption.InvalidMediaPosition, result.FirstError);
    }

    [Fact]
    public void ToDomainEntity_WhenProgressFractionIsOutsideItsBounds_ShouldReturnTheDomainError()
    {
        // Arrange
        MediaResumePointEntity entity = _mediaResumePointEntityFixture.Create(
            mediaKind: MediaKind.MusicTrack,
            lastProgressFraction: 1.5);

        // Act
        Result<MediaResumePoint> result = entity.ToDomainEntity();

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.MediaConsumption.InvalidProgressFraction, result.FirstError);
    }

    [Fact]
    public void ToDomainEntity_WhenMediaKindCannotBeResumed_ShouldReturnTheDomainError()
    {
        // Arrange
        MediaResumePointEntity entity = _mediaResumePointEntityFixture.Create(
            mediaKind: MediaKind.Photo,
            lastPositionKind: MediaPositionKind.TimeOffset);

        // Act
        Result<MediaResumePoint> result = entity.ToDomainEntity();

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.MediaConsumption.MediaKindCannotResume, result.FirstError);
    }

    [Fact]
    public void ToDto_WhenMappingEntity_ShouldMapAllPropertiesCorrectly()
    {
        // Arrange
        Guid id = Guid.NewGuid();
        Guid libraryId = Guid.NewGuid();
        Guid mediaItemId = Guid.NewGuid();
        DateTime lastConsumedOnUtc = new(2024, 12, 5, 21, 10, 0, DateTimeKind.Utc);
        MediaResumePointEntity entity = _mediaResumePointEntityFixture.Create(
            id: id,
            libraryId: libraryId,
            mediaKind: MediaKind.Movie,
            mediaItemId: mediaItemId,
            lastPositionKind: MediaPositionKind.TimeOffset,
            lastPositionTimeOffsetMilliseconds: 2_500,
            lastProgressFraction: 0.6,
            lastConsumedOnUtc: lastConsumedOnUtc);

        // Act
        ResumePointDto result = entity.ToDto();

        // Assert
        Assert.Equal(id, result.Id);
        Assert.Equal(libraryId, result.LibraryId);
        Assert.Equal(mediaItemId, result.MediaItemId);
        Assert.Equal(MediaKind.Movie, result.MediaKind);
        Assert.Equal(MediaPositionKind.TimeOffset, result.LastPosition.Kind);
        Assert.Equal(2_500L, result.LastPosition.TimeOffsetMilliseconds);
        Assert.Null(result.LastPosition.Location);
        Assert.Equal(0.6, result.LastProgressFraction);
        Assert.Equal(lastConsumedOnUtc, result.LastConsumedOnUtc);
    }

    [Fact]
    public void ToDto_WhenLastConsumedOnUtcIsUnspecified_ShouldSpecifyUtcWithoutChangingTicks()
    {
        // Arrange
        DateTime lastConsumedOnUtc = new(2024, 12, 5, 21, 10, 30, DateTimeKind.Unspecified);
        MediaResumePointEntity entity = _mediaResumePointEntityFixture.Create(lastConsumedOnUtc: lastConsumedOnUtc);

        // Act
        ResumePointDto result = entity.ToDto();

        // Assert
        Assert.Equal(DateTimeKind.Utc, result.LastConsumedOnUtc.Kind);
        Assert.Equal(lastConsumedOnUtc.Ticks, result.LastConsumedOnUtc.Ticks);
    }

    [Fact]
    public void ToDto_WhenPositionIsLocation_ShouldMapTheLocationPosition()
    {
        // Arrange
        MediaResumePointEntity entity = _mediaResumePointEntityFixture.Create(
            mediaKind: MediaKind.Book,
            lastPositionKind: MediaPositionKind.Location,
            lastPositionLocation: "chapter-1");

        // Act
        ResumePointDto result = entity.ToDto();

        // Assert
        Assert.Equal(MediaPositionKind.Location, result.LastPosition.Kind);
        Assert.Null(result.LastPosition.TimeOffsetMilliseconds);
        Assert.Equal("chapter-1", result.LastPosition.Location);
    }
}
