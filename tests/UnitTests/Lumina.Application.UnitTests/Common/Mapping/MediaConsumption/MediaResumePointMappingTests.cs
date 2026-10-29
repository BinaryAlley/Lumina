#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.MediaConsumption;
using Lumina.Application.Common.DTO.MediaConsumption;
using Lumina.Application.Common.Mapping.MediaConsumption;
using Lumina.Contracts.Responses.MediaConsumption;
using Lumina.Domain.Core.BoundedContexts.MediaConsumptionBoundedContext.Common.ValueObjects;
using Lumina.Domain.Core.BoundedContexts.MediaConsumptionBoundedContext.ExternalIdentifiers.LibraryManagementBoundedContext.LibraryAggregate;
using Lumina.Domain.Core.BoundedContexts.MediaConsumptionBoundedContext.ExternalIdentifiers.MediaLibraryBoundedContext.MediaItemAggregate;
using Lumina.Domain.Core.BoundedContexts.MediaConsumptionBoundedContext.ExternalIdentifiers.UserManagementBoundedContext.UserAggregate;
using Lumina.Domain.Core.BoundedContexts.MediaConsumptionBoundedContext.MediaResumePointAggregate;
using Lumina.Domain.Core.BoundedContexts.MediaConsumptionBoundedContext.MediaResumePointAggregate.ValueObjects;
using Lumina.Domain.Fixtures.Core.BoundedContexts.MediaConsumptionBoundedContext.Common.ValueObjects;
using Lumina.Domain.Fixtures.Core.BoundedContexts.MediaConsumptionBoundedContext.ExternalIdentifiers.LibraryManagementBoundedContext.LibraryAggregate;
using Lumina.Domain.Fixtures.Core.BoundedContexts.MediaConsumptionBoundedContext.ExternalIdentifiers.MediaLibraryBoundedContext.MediaItemAggregate;
using Lumina.Domain.Fixtures.Core.BoundedContexts.MediaConsumptionBoundedContext.ExternalIdentifiers.UserManagementBoundedContext.UserAggregate;
using Lumina.Domain.Fixtures.Core.BoundedContexts.MediaConsumptionBoundedContext.MediaResumePointAggregate;
using Lumina.Domain.Fixtures.Core.BoundedContexts.MediaConsumptionBoundedContext.MediaResumePointAggregate.ValueObjects;
using Lumina.Domain.SharedKernel.Common.Enums.MediaConsumption;
using Lumina.Domain.SharedKernel.Common.Enums.MediaLibrary;
using System;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Application.UnitTests.Common.Mapping.MediaConsumption;

/// <summary>
/// Contains unit tests for the <see cref="MediaResumePointMapping"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class MediaResumePointMappingTests
{
    private readonly MediaResumePointFixture _mediaResumePointFixture = new();
    private readonly MediaPositionFixture _mediaPositionFixture = new();
    private readonly MediaResumePointIdFixture _mediaResumePointIdFixture = new();
    private readonly UserIdFixture _userIdFixture = new();
    private readonly LibraryIdFixture _libraryIdFixture = new();
    private readonly MediaItemIdFixture _mediaItemIdFixture = new();

    [Fact]
    public void ToRepositoryEntity_WhenMappingCompleteResumePoint_ShouldMapAllPropertiesCorrectly()
    {
        // Arrange
        MediaResumePointId id = _mediaResumePointIdFixture.Create();
        UserId userId = _userIdFixture.Create();
        LibraryId libraryId = _libraryIdFixture.Create();
        MediaItemId mediaItemId = _mediaItemIdFixture.Create();
        DateTime lastConsumedOnUtc = new(2024, 6, 10, 19, 0, 0, DateTimeKind.Utc);
        DateTime createdOnUtc = new(2024, 6, 1, 19, 0, 0, DateTimeKind.Utc);
        MediaPosition position = _mediaPositionFixture.Create(
            kind: MediaPositionKind.TimeOffset,
            timeOffset: TimeSpan.FromMilliseconds(2_500));
        MediaResumePoint resumePoint = _mediaResumePointFixture.Create(
            id: id,
            userId: userId,
            libraryId: libraryId,
            mediaItemId: mediaItemId,
            mediaKind: MediaKind.MusicTrack,
            lastPosition: position,
            lastProgressFraction: 0.6,
            lastConsumedOnUtc: lastConsumedOnUtc,
            createdOnUtc: createdOnUtc);

        // Act
        MediaResumePointEntity result = resumePoint.ToRepositoryEntity();

        // Assert
        Assert.Equal(id.Value, result.Id);
        Assert.Equal(userId.Value, result.UserId);
        Assert.Equal(libraryId.Value, result.LibraryId);
        Assert.Equal(mediaItemId.Value, result.MediaItemId);
        Assert.Equal(MediaKind.MusicTrack, result.MediaKind);
        Assert.Equal(MediaPositionKind.TimeOffset, result.LastPositionKind);
        Assert.Equal(2_500L, result.LastPositionTimeOffsetMilliseconds);
        Assert.Null(result.LastPositionLocation);
        Assert.Equal(0.6, result.LastProgressFraction);
        Assert.Equal(lastConsumedOnUtc, result.LastConsumedOnUtc);
        Assert.Equal(1L, result.Version);
        Assert.Equal(createdOnUtc, result.CreatedOnUtc);
        Assert.Equal(Guid.Empty, result.CreatedBy);
        Assert.Null(result.UpdatedOnUtc);
        Assert.Null(result.UpdatedBy);
    }

    [Fact]
    public void ToRepositoryEntity_WhenPositionIsTimeOffset_ShouldMapKindAndTruncatedMilliseconds()
    {
        // Arrange
        MediaPosition position = _mediaPositionFixture.Create(
            kind: MediaPositionKind.TimeOffset,
            timeOffset: TimeSpan.FromTicks(15_600));
        MediaResumePoint resumePoint = _mediaResumePointFixture.Create(
            mediaKind: MediaKind.MusicTrack,
            lastPosition: position);

        // Act
        MediaResumePointEntity result = resumePoint.ToRepositoryEntity();

        // Assert
        Assert.Equal(MediaPositionKind.TimeOffset, result.LastPositionKind);
        Assert.Equal(1L, result.LastPositionTimeOffsetMilliseconds);
        Assert.Null(result.LastPositionLocation);
    }

    [Fact]
    public void ToRepositoryEntity_WhenPositionIsLocation_ShouldMapKindAndLocation()
    {
        // Arrange
        MediaPosition position = _mediaPositionFixture.Create(
            kind: MediaPositionKind.Location,
            location: "chapter-1");
        MediaResumePoint resumePoint = _mediaResumePointFixture.Create(
            mediaKind: MediaKind.Book,
            lastPosition: position);

        // Act
        MediaResumePointEntity result = resumePoint.ToRepositoryEntity();

        // Assert
        Assert.Equal(MediaPositionKind.Location, result.LastPositionKind);
        Assert.Null(result.LastPositionTimeOffsetMilliseconds);
        Assert.Equal("chapter-1", result.LastPositionLocation);
    }

    [Fact]
    public void ToDto_WhenMappingCompleteResumePoint_ShouldMapAllPropertiesCorrectly()
    {
        // Arrange
        MediaResumePointId id = _mediaResumePointIdFixture.Create();
        LibraryId libraryId = _libraryIdFixture.Create();
        MediaItemId mediaItemId = _mediaItemIdFixture.Create();
        DateTime lastConsumedOnUtc = new(2024, 7, 1, 21, 0, 0, DateTimeKind.Utc);
        MediaPosition position = _mediaPositionFixture.Create(
            kind: MediaPositionKind.TimeOffset,
            timeOffset: TimeSpan.FromMilliseconds(2_500));
        MediaResumePoint resumePoint = _mediaResumePointFixture.Create(
            id: id,
            libraryId: libraryId,
            mediaItemId: mediaItemId,
            mediaKind: MediaKind.Movie,
            lastPosition: position,
            lastProgressFraction: 0.6,
            lastConsumedOnUtc: lastConsumedOnUtc);

        // Act
        ResumePointDto result = resumePoint.ToDto();

        // Assert
        Assert.Equal(id.Value, result.Id);
        Assert.Equal(libraryId.Value, result.LibraryId);
        Assert.Equal(mediaItemId.Value, result.MediaItemId);
        Assert.Equal(MediaKind.Movie, result.MediaKind);
        Assert.Equal(MediaPositionKind.TimeOffset, result.LastPosition.Kind);
        Assert.Equal(2_500L, result.LastPosition.TimeOffsetMilliseconds);
        Assert.Null(result.LastPosition.Location);
        Assert.Equal(0.6, result.LastProgressFraction);
        Assert.Equal(lastConsumedOnUtc, result.LastConsumedOnUtc);
    }

    [Fact]
    public void ToDto_WhenPositionIsLocation_ShouldMapTheLocationPosition()
    {
        // Arrange
        MediaPosition position = _mediaPositionFixture.Create(
            kind: MediaPositionKind.Location,
            location: "chapter-1");
        MediaResumePoint resumePoint = _mediaResumePointFixture.Create(
            mediaKind: MediaKind.Book,
            lastPosition: position);

        // Act
        ResumePointDto result = resumePoint.ToDto();

        // Assert
        Assert.Equal(MediaPositionKind.Location, result.LastPosition.Kind);
        Assert.Null(result.LastPosition.TimeOffsetMilliseconds);
        Assert.Equal("chapter-1", result.LastPosition.Location);
    }

    [Fact]
    public void ToDto_WhenLastConsumedOnUtcIsUnspecified_ShouldSpecifyUtcWithoutChangingTicks()
    {
        // Arrange
        DateTime lastConsumedOnUtc = new(2024, 7, 2, 21, 0, 15, DateTimeKind.Unspecified);
        MediaResumePoint resumePoint = _mediaResumePointFixture.Create(lastConsumedOnUtc: lastConsumedOnUtc);

        // Act
        ResumePointDto result = resumePoint.ToDto();

        // Assert
        Assert.Equal(DateTimeKind.Utc, result.LastConsumedOnUtc.Kind);
        Assert.Equal(lastConsumedOnUtc.Ticks, result.LastConsumedOnUtc.Ticks);
    }

    [Fact]
    public void ToResponse_WhenMappingCompleteResumePoint_ShouldMapAllPropertiesCorrectly()
    {
        // Arrange
        MediaResumePointId id = _mediaResumePointIdFixture.Create();
        LibraryId libraryId = _libraryIdFixture.Create();
        MediaItemId mediaItemId = _mediaItemIdFixture.Create();
        DateTime lastConsumedOnUtc = new(2024, 8, 1, 22, 0, 0, DateTimeKind.Utc);
        MediaPosition position = _mediaPositionFixture.Create(
            kind: MediaPositionKind.TimeOffset,
            timeOffset: TimeSpan.FromMilliseconds(2_500));
        MediaResumePoint resumePoint = _mediaResumePointFixture.Create(
            id: id,
            libraryId: libraryId,
            mediaItemId: mediaItemId,
            mediaKind: MediaKind.Movie,
            lastPosition: position,
            lastProgressFraction: 0.6,
            lastConsumedOnUtc: lastConsumedOnUtc);

        // Act
        ResumePointResponse result = resumePoint.ToResponse();

        // Assert
        Assert.Equal(id.Value, result.Id);
        Assert.Equal(libraryId.Value, result.LibraryId);
        Assert.Equal(mediaItemId.Value, result.MediaItemId);
        Assert.Equal(MediaKind.Movie, result.MediaKind);
        Assert.Equal(MediaPositionKind.TimeOffset, result.LastPosition.Kind);
        Assert.Equal(2.5, result.LastPosition.TimeOffsetSeconds);
        Assert.Null(result.LastPosition.Location);
        Assert.Equal(0.6, result.LastProgressFraction);
        Assert.Equal(lastConsumedOnUtc, result.LastConsumedOnUtc);
    }

    [Fact]
    public void ToResponse_WhenPositionIsLocation_ShouldMapTheLocationPosition()
    {
        // Arrange
        MediaPosition position = _mediaPositionFixture.Create(
            kind: MediaPositionKind.Location,
            location: "chapter-1");
        MediaResumePoint resumePoint = _mediaResumePointFixture.Create(
            mediaKind: MediaKind.Book,
            lastPosition: position);

        // Act
        ResumePointResponse result = resumePoint.ToResponse();

        // Assert
        Assert.Equal(MediaPositionKind.Location, result.LastPosition.Kind);
        Assert.Null(result.LastPosition.TimeOffsetSeconds);
        Assert.Equal("chapter-1", result.LastPosition.Location);
    }

    [Fact]
    public void ToResponse_WhenLastConsumedOnUtcIsUnspecified_ShouldSpecifyUtcWithoutChangingTicks()
    {
        // Arrange
        DateTime lastConsumedOnUtc = new(2024, 8, 2, 22, 0, 30, DateTimeKind.Unspecified);
        MediaResumePoint resumePoint = _mediaResumePointFixture.Create(lastConsumedOnUtc: lastConsumedOnUtc);

        // Act
        ResumePointResponse result = resumePoint.ToResponse();

        // Assert
        Assert.Equal(DateTimeKind.Utc, result.LastConsumedOnUtc.Kind);
        Assert.Equal(lastConsumedOnUtc.Ticks, result.LastConsumedOnUtc.Ticks);
    }
}
