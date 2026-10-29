#region ========================================================================= USING =====================================================================================
using Lumina.Domain.Common.Errors;
using Lumina.Domain.Common.Events;
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.Core.BoundedContexts.MediaConsumptionBoundedContext.Common.ValueObjects;
using Lumina.Domain.Core.BoundedContexts.MediaConsumptionBoundedContext.ExternalIdentifiers.LibraryManagementBoundedContext.LibraryAggregate;
using Lumina.Domain.Core.BoundedContexts.MediaConsumptionBoundedContext.ExternalIdentifiers.MediaLibraryBoundedContext.MediaItemAggregate;
using Lumina.Domain.Core.BoundedContexts.MediaConsumptionBoundedContext.ExternalIdentifiers.UserManagementBoundedContext.UserAggregate;
using Lumina.Domain.Core.BoundedContexts.MediaConsumptionBoundedContext.MediaConsumptionRecordAggregate;
using Lumina.Domain.Core.BoundedContexts.MediaConsumptionBoundedContext.MediaConsumptionRecordAggregate.Events;
using Lumina.Domain.Core.BoundedContexts.MediaConsumptionBoundedContext.MediaConsumptionRecordAggregate.ValueObjects;
using Lumina.Domain.Fixtures.Core.BoundedContexts.MediaConsumptionBoundedContext.Common.ValueObjects;
using Lumina.Domain.Fixtures.Core.BoundedContexts.MediaConsumptionBoundedContext.ExternalIdentifiers.LibraryManagementBoundedContext.LibraryAggregate;
using Lumina.Domain.Fixtures.Core.BoundedContexts.MediaConsumptionBoundedContext.ExternalIdentifiers.MediaLibraryBoundedContext.MediaItemAggregate;
using Lumina.Domain.Fixtures.Core.BoundedContexts.MediaConsumptionBoundedContext.ExternalIdentifiers.UserManagementBoundedContext.UserAggregate;
using Lumina.Domain.Fixtures.Core.BoundedContexts.MediaConsumptionBoundedContext.MediaConsumptionRecordAggregate;
using Lumina.Domain.Fixtures.Core.BoundedContexts.MediaConsumptionBoundedContext.MediaConsumptionRecordAggregate.ValueObjects;
using Lumina.Domain.SharedKernel.Common.Enums.MediaConsumption;
using Lumina.Domain.SharedKernel.Common.Enums.MediaLibrary;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Domain.UnitTests.Core.BoundedContexts.MediaConsumptionBoundedContext.MediaConsumptionRecordAggregate;

/// <summary>
/// Contains unit tests for the <see cref="MediaConsumptionRecord"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class MediaConsumptionRecordTests
{
    private readonly MediaConsumptionRecordFixture _mediaConsumptionRecordFixture = new();
    private readonly MediaConsumptionRecordIdFixture _mediaConsumptionRecordIdFixture = new();
    private readonly UserIdFixture _userIdFixture = new();
    private readonly LibraryIdFixture _libraryIdFixture = new();
    private readonly MediaItemIdFixture _mediaItemIdFixture = new();
    private readonly MediaPositionFixture _mediaPositionFixture = new();

    [Fact]
    public void Start_WhenCalledWithValidData_ShouldCreateInProgressRecordWithAllPropertiesSet()
    {
        // Arrange
        UserId userId = _userIdFixture.Create();
        LibraryId libraryId = _libraryIdFixture.Create();
        MediaItemId mediaItemId = _mediaItemIdFixture.Create();
        MediaPosition lastPosition = _mediaPositionFixture.Create(kind: MediaPositionKind.TimeOffset);
        double lastProgressFraction = 0.25;
        DateTime beforeUtc = DateTime.UtcNow;

        // Act
        Result<MediaConsumptionRecord> result = MediaConsumptionRecord.Start(
            userId,
            libraryId,
            mediaItemId,
            MediaKind.Movie,
            Optional<MediaPosition>.Some(lastPosition),
            lastProgressFraction);

        // Assert
        Assert.False(result.IsFailure);
        MediaConsumptionRecord record = result.Value;
        Assert.NotEqual(Guid.Empty, record.Id.Value);
        Assert.Equal(userId, record.UserId);
        Assert.Equal(libraryId, record.LibraryId);
        Assert.Equal(mediaItemId, record.MediaItemId);
        Assert.Equal(MediaKind.Movie, record.MediaKind);
        Assert.Equal(ConsumptionStatus.InProgress, record.Status);
        Assert.InRange(record.StartedOnUtc, beforeUtc, DateTime.UtcNow);
        Assert.Equal(record.StartedOnUtc, record.LastActivityOnUtc);
        Assert.False(record.EndedOnUtc.HasValue);
        Assert.Equal(TimeSpan.Zero, record.ConsumedDuration);
        Assert.Equal(Optional<MediaPosition>.Some(lastPosition), record.LastPosition);
        Assert.Equal(lastProgressFraction, record.LastProgressFraction);
        Assert.Equal(0, record.ConsecutivePagesRead);
        Assert.Equal(record.StartedOnUtc, record.CreatedOnUtc);
        Assert.False(record.UpdatedOnUtc.HasValue);
        MediaConsumptionStartedDomainEvent startedEvent = Assert.IsType<MediaConsumptionStartedDomainEvent>(Assert.Single(record.GetDomainEvents()));
        Assert.NotEqual(Guid.Empty, startedEvent.Id);
        Assert.Equal(record.Id, startedEvent.MediaConsumptionRecordId);
        Assert.Equal(userId, startedEvent.UserId);
        Assert.Equal(libraryId, startedEvent.LibraryId);
        Assert.Equal(mediaItemId, startedEvent.MediaItemId);
        Assert.Equal(MediaKind.Movie, startedEvent.MediaKind);
        Assert.Equal(record.StartedOnUtc, startedEvent.OccurredOnUtc);
    }

    [Fact]
    public void Start_WhenCalledTwice_ShouldGenerateDistinctIds()
    {
        // Act
        Result<MediaConsumptionRecord> firstResult = MediaConsumptionRecord.Start(
            _userIdFixture.Create(),
            _libraryIdFixture.Create(),
            _mediaItemIdFixture.Create(),
            MediaKind.Movie,
            Optional<MediaPosition>.None(),
            0.5);
        Result<MediaConsumptionRecord> secondResult = MediaConsumptionRecord.Start(
            _userIdFixture.Create(),
            _libraryIdFixture.Create(),
            _mediaItemIdFixture.Create(),
            MediaKind.Movie,
            Optional<MediaPosition>.None(),
            0.5);

        // Assert
        Assert.False(firstResult.IsFailure);
        Assert.False(secondResult.IsFailure);
        Assert.NotEqual(firstResult.Value.Id.Value, secondResult.Value.Id.Value);
    }

    [Theory]
    [InlineData(MediaKind.None)] // the default value, which is not a valid kind for a media item
    [InlineData((MediaKind)999)] // a value outside the declared enumeration members
    public void Start_WhenMediaKindIsUnsupported_ShouldReturnUnsupportedMediaKindError(MediaKind mediaKind)
    {
        // Act
        Result<MediaConsumptionRecord> result = MediaConsumptionRecord.Start(
            _userIdFixture.Create(),
            _libraryIdFixture.Create(),
            _mediaItemIdFixture.Create(),
            mediaKind,
            Optional<MediaPosition>.None(),
            0.5);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.MediaConsumption.UnsupportedMediaKind, result.FirstError);
    }

    [Theory]
    [InlineData(double.NaN)] // a value that is not a number
    [InlineData(double.PositiveInfinity)] // a value beyond every finite fraction
    [InlineData(double.NegativeInfinity)] // a value below every finite fraction
    [InlineData(-0.01)] // a value below the zero lower boundary
    [InlineData(1.01)] // a value above the one upper boundary
    public void Start_WhenProgressFractionIsInvalid_ShouldReturnInvalidProgressFractionError(double lastProgressFraction)
    {
        // Act
        Result<MediaConsumptionRecord> result = MediaConsumptionRecord.Start(
            _userIdFixture.Create(),
            _libraryIdFixture.Create(),
            _mediaItemIdFixture.Create(),
            MediaKind.Movie,
            Optional<MediaPosition>.None(),
            lastProgressFraction);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.MediaConsumption.InvalidProgressFraction, result.FirstError);
    }

    [Theory]
    [InlineData(0.0)] // the lower boundary of the progress fraction
    [InlineData(1.0)] // the upper boundary of the progress fraction
    public void Start_WhenProgressFractionIsAtABoundary_ShouldCreateRecord(double lastProgressFraction)
    {
        // Act
        Result<MediaConsumptionRecord> result = MediaConsumptionRecord.Start(
            _userIdFixture.Create(),
            _libraryIdFixture.Create(),
            _mediaItemIdFixture.Create(),
            MediaKind.Movie,
            Optional<MediaPosition>.None(),
            lastProgressFraction);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(lastProgressFraction, result.Value.LastProgressFraction);
    }

    [Theory]
    [InlineData(MediaKind.Movie, MediaPositionKind.Location)] // a movie resumes from a time offset, not from a location
    [InlineData(MediaKind.Book, MediaPositionKind.TimeOffset)] // a book resumes from a location, not from a time offset
    [InlineData(MediaKind.Photo, MediaPositionKind.TimeOffset)] // a photo cannot be resumed, so it accepts no position at all
    public void Start_WhenPositionDoesNotMatchTheMediaKindRule_ShouldReturnInvalidMediaPositionError(MediaKind mediaKind, MediaPositionKind positionKind)
    {
        // Arrange
        MediaPosition lastPosition = _mediaPositionFixture.Create(kind: positionKind);

        // Act
        Result<MediaConsumptionRecord> result = MediaConsumptionRecord.Start(
            _userIdFixture.Create(),
            _libraryIdFixture.Create(),
            _mediaItemIdFixture.Create(),
            mediaKind,
            Optional<MediaPosition>.Some(lastPosition),
            0.5);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.MediaConsumption.InvalidMediaPosition, result.FirstError);
    }

    [Theory]
    [InlineData(MediaKind.Movie, MediaPositionKind.TimeOffset)] // a movie resumes from a time offset
    [InlineData(MediaKind.Book, MediaPositionKind.Location)] // a book resumes from a location
    public void Start_WhenPositionMatchesTheMediaKindRule_ShouldStoreThePosition(MediaKind mediaKind, MediaPositionKind positionKind)
    {
        // Arrange
        MediaPosition lastPosition = _mediaPositionFixture.Create(kind: positionKind);

        // Act
        Result<MediaConsumptionRecord> result = MediaConsumptionRecord.Start(
            _userIdFixture.Create(),
            _libraryIdFixture.Create(),
            _mediaItemIdFixture.Create(),
            mediaKind,
            Optional<MediaPosition>.Some(lastPosition),
            0.5);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(Optional<MediaPosition>.Some(lastPosition), result.Value.LastPosition);
    }

    [Fact]
    public void Start_WhenNoPositionIsSupplied_ShouldCreateRecordWithNoPositionOrEndTime()
    {
        // Act
        Result<MediaConsumptionRecord> result = MediaConsumptionRecord.Start(
            _userIdFixture.Create(),
            _libraryIdFixture.Create(),
            _mediaItemIdFixture.Create(),
            MediaKind.Movie,
            Optional<MediaPosition>.None(),
            0.5);

        // Assert
        Assert.False(result.IsFailure);
        Assert.False(result.Value.LastPosition.HasValue);
        Assert.False(result.Value.EndedOnUtc.HasValue);
        Assert.Equal(ConsumptionStatus.InProgress, result.Value.Status);
    }

    [Fact]
    public void StartCompleted_WhenMediaKindIsPhoto_ShouldCreateCompletedRecordAndRaiseStartedThenCompletedEvents()
    {
        // Arrange
        UserId userId = _userIdFixture.Create();
        LibraryId libraryId = _libraryIdFixture.Create();
        MediaItemId mediaItemId = _mediaItemIdFixture.Create();
        DateTime beforeUtc = DateTime.UtcNow;

        // Act
        Result<MediaConsumptionRecord> result = MediaConsumptionRecord.StartCompleted(userId, libraryId, mediaItemId, MediaKind.Photo);

        // Assert
        Assert.False(result.IsFailure);
        MediaConsumptionRecord record = result.Value;
        Assert.NotEqual(Guid.Empty, record.Id.Value);
        Assert.Equal(userId, record.UserId);
        Assert.Equal(libraryId, record.LibraryId);
        Assert.Equal(mediaItemId, record.MediaItemId);
        Assert.Equal(MediaKind.Photo, record.MediaKind);
        Assert.Equal(ConsumptionStatus.Completed, record.Status);
        Assert.InRange(record.StartedOnUtc, beforeUtc, DateTime.UtcNow);
        Assert.Equal(record.StartedOnUtc, record.LastActivityOnUtc);
        Assert.True(record.EndedOnUtc.HasValue);
        Assert.Equal(record.StartedOnUtc, record.EndedOnUtc.Value);
        Assert.Equal(TimeSpan.Zero, record.ConsumedDuration);
        Assert.False(record.LastPosition.HasValue);
        Assert.Equal(1, record.LastProgressFraction);
        Assert.Equal(0, record.ConsecutivePagesRead);
        Assert.Equal(record.StartedOnUtc, record.CreatedOnUtc);
        Assert.False(record.UpdatedOnUtc.HasValue);
        List<IDomainEvent> domainEvents = record.GetDomainEvents();
        Assert.Equal(2, domainEvents.Count);
        MediaConsumptionStartedDomainEvent startedEvent = Assert.IsType<MediaConsumptionStartedDomainEvent>(domainEvents[0]);
        MediaConsumptionCompletedDomainEvent completedEvent = Assert.IsType<MediaConsumptionCompletedDomainEvent>(domainEvents[1]);
        Assert.Equal(record.Id, startedEvent.MediaConsumptionRecordId);
        Assert.Equal(record.Id, completedEvent.MediaConsumptionRecordId);
        Assert.Equal(userId, completedEvent.UserId);
        Assert.Equal(libraryId, completedEvent.LibraryId);
        Assert.Equal(mediaItemId, completedEvent.MediaItemId);
        Assert.Equal(MediaKind.Photo, completedEvent.MediaKind);
        Assert.False(completedEvent.LastPosition.HasValue);
        Assert.Equal(1, completedEvent.LastProgressFraction);
        Assert.Equal(TimeSpan.Zero, completedEvent.ConsumedDuration);
        Assert.Equal(0, completedEvent.ConsecutivePagesRead);
        Assert.Equal(record.StartedOnUtc, startedEvent.OccurredOnUtc);
        Assert.Equal(record.StartedOnUtc, completedEvent.OccurredOnUtc);
    }

    [Theory]
    [InlineData(MediaKind.MusicTrack)] // a duration based kind that is started explicitly
    [InlineData(MediaKind.PodcastEpisode)] // a duration based kind that is started explicitly
    [InlineData(MediaKind.Audiobook)] // a duration based kind that is started explicitly
    [InlineData(MediaKind.Movie)] // a duration based kind that is started explicitly
    [InlineData(MediaKind.TvEpisode)] // a duration based kind that is started explicitly
    [InlineData(MediaKind.Book)] // a page based kind that is started explicitly
    [InlineData(MediaKind.Video)] // a duration based kind that is started explicitly
    public void StartCompleted_WhenMediaKindIsNotOneShot_ShouldReturnMediaKindCannotStartCompletedError(MediaKind mediaKind)
    {
        // Act
        Result<MediaConsumptionRecord> result = MediaConsumptionRecord.StartCompleted(
            _userIdFixture.Create(),
            _libraryIdFixture.Create(),
            _mediaItemIdFixture.Create(),
            mediaKind);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.MediaConsumption.MediaKindCannotStartCompleted, result.FirstError);
    }

    [Theory]
    [InlineData(MediaKind.None)] // the default value, which is not a valid kind for a media item
    [InlineData((MediaKind)999)] // a value outside the declared enumeration members
    public void StartCompleted_WhenMediaKindIsUnsupported_ShouldReturnUnsupportedMediaKindError(MediaKind mediaKind)
    {
        // Act
        Result<MediaConsumptionRecord> result = MediaConsumptionRecord.StartCompleted(
            _userIdFixture.Create(),
            _libraryIdFixture.Create(),
            _mediaItemIdFixture.Create(),
            mediaKind);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.MediaConsumption.UnsupportedMediaKind, result.FirstError);
    }

    [Fact]
    public void Reconstitute_WhenCalledWithInProgressData_ShouldPreserveAllPropertiesAndDeriveTimestamps()
    {
        // Arrange
        MediaConsumptionRecordId id = _mediaConsumptionRecordIdFixture.Create();
        UserId userId = _userIdFixture.Create();
        LibraryId libraryId = _libraryIdFixture.Create();
        MediaItemId mediaItemId = _mediaItemIdFixture.Create();
        MediaPosition lastPosition = _mediaPositionFixture.Create(kind: MediaPositionKind.TimeOffset);
        DateTime startedOnUtc = DateTime.UtcNow.AddHours(-3);
        DateTime lastActivityOnUtc = startedOnUtc.AddMinutes(45);
        TimeSpan consumedDuration = TimeSpan.FromMinutes(40);
        double lastProgressFraction = 0.6;

        // Act
        Result<MediaConsumptionRecord> result = MediaConsumptionRecord.Reconstitute(
            id,
            userId,
            libraryId,
            mediaItemId,
            MediaKind.Movie,
            ConsumptionStatus.InProgress,
            startedOnUtc,
            lastActivityOnUtc,
            Optional<DateTime>.None(),
            consumedDuration,
            Optional<MediaPosition>.Some(lastPosition),
            lastProgressFraction,
            0);

        // Assert
        Assert.False(result.IsFailure);
        MediaConsumptionRecord record = result.Value;
        Assert.Equal(id, record.Id);
        Assert.Equal(userId, record.UserId);
        Assert.Equal(libraryId, record.LibraryId);
        Assert.Equal(mediaItemId, record.MediaItemId);
        Assert.Equal(MediaKind.Movie, record.MediaKind);
        Assert.Equal(ConsumptionStatus.InProgress, record.Status);
        Assert.Equal(startedOnUtc, record.StartedOnUtc);
        Assert.Equal(lastActivityOnUtc, record.LastActivityOnUtc);
        Assert.False(record.EndedOnUtc.HasValue);
        Assert.Equal(consumedDuration, record.ConsumedDuration);
        Assert.Equal(Optional<MediaPosition>.Some(lastPosition), record.LastPosition);
        Assert.Equal(lastProgressFraction, record.LastProgressFraction);
        Assert.Equal(0, record.ConsecutivePagesRead);
        Assert.Equal(startedOnUtc, record.CreatedOnUtc);
        Assert.Equal(Optional<DateTime>.Some(lastActivityOnUtc), record.UpdatedOnUtc);
        Assert.Empty(record.GetDomainEvents());
    }

    [Theory]
    [InlineData(ConsumptionStatus.Completed)] // a session that reached its completion
    [InlineData(ConsumptionStatus.Stopped)] // a session that ended before completing
    public void Reconstitute_WhenCalledWithTerminalData_ShouldPreserveAllPropertiesAndDeriveTimestamps(ConsumptionStatus status)
    {
        // Arrange
        MediaConsumptionRecordId id = _mediaConsumptionRecordIdFixture.Create();
        UserId userId = _userIdFixture.Create();
        LibraryId libraryId = _libraryIdFixture.Create();
        MediaItemId mediaItemId = _mediaItemIdFixture.Create();
        MediaPosition lastPosition = _mediaPositionFixture.Create(kind: MediaPositionKind.TimeOffset);
        DateTime startedOnUtc = DateTime.UtcNow.AddHours(-3);
        DateTime lastActivityOnUtc = startedOnUtc.AddMinutes(90);
        DateTime endedOnUtc = lastActivityOnUtc.AddMinutes(5);
        TimeSpan consumedDuration = TimeSpan.FromMinutes(95);

        // Act
        Result<MediaConsumptionRecord> result = MediaConsumptionRecord.Reconstitute(
            id,
            userId,
            libraryId,
            mediaItemId,
            MediaKind.Movie,
            status,
            startedOnUtc,
            lastActivityOnUtc,
            Optional<DateTime>.Some(endedOnUtc),
            consumedDuration,
            Optional<MediaPosition>.Some(lastPosition),
            0.75,
            0);

        // Assert
        Assert.False(result.IsFailure);
        MediaConsumptionRecord record = result.Value;
        Assert.Equal(id, record.Id);
        Assert.Equal(status, record.Status);
        Assert.Equal(startedOnUtc, record.StartedOnUtc);
        Assert.Equal(lastActivityOnUtc, record.LastActivityOnUtc);
        Assert.Equal(Optional<DateTime>.Some(endedOnUtc), record.EndedOnUtc);
        Assert.Equal(consumedDuration, record.ConsumedDuration);
        Assert.Equal(Optional<MediaPosition>.Some(lastPosition), record.LastPosition);
        Assert.Equal(startedOnUtc, record.CreatedOnUtc);
        Assert.Equal(Optional<DateTime>.Some(lastActivityOnUtc), record.UpdatedOnUtc);
        Assert.Empty(record.GetDomainEvents());
    }

    [Fact]
    public void Reconstitute_WhenLastActivityEqualsStart_ShouldCreateRecordWithoutUpdatedOnUtc()
    {
        // Arrange
        DateTime startedOnUtc = DateTime.UtcNow.AddHours(-1);

        // Act
        Result<MediaConsumptionRecord> result = MediaConsumptionRecord.Reconstitute(
            _mediaConsumptionRecordIdFixture.Create(),
            _userIdFixture.Create(),
            _libraryIdFixture.Create(),
            _mediaItemIdFixture.Create(),
            MediaKind.Movie,
            ConsumptionStatus.InProgress,
            startedOnUtc,
            startedOnUtc,
            Optional<DateTime>.None(),
            TimeSpan.FromMinutes(10),
            Optional<MediaPosition>.None(),
            0.5,
            0);

        // Assert
        Assert.False(result.IsFailure);
        Assert.False(result.Value.UpdatedOnUtc.HasValue);
    }

    [Theory]
    [InlineData(MediaKind.None)] // the default value, which is not a valid kind for a media item
    [InlineData((MediaKind)999)] // a value outside the declared enumeration members
    public void Reconstitute_WhenMediaKindIsUnsupported_ShouldReturnUnsupportedMediaKindError(MediaKind mediaKind)
    {
        // Arrange
        MediaConsumptionRecord source = _mediaConsumptionRecordFixture.Create(mediaKind: MediaKind.Movie);

        // Act
        Result<MediaConsumptionRecord> result = MediaConsumptionRecord.Reconstitute(
            source.Id,
            source.UserId,
            source.LibraryId,
            source.MediaItemId,
            mediaKind,
            source.Status,
            source.StartedOnUtc,
            source.LastActivityOnUtc,
            source.EndedOnUtc,
            source.ConsumedDuration,
            Optional<MediaPosition>.None(),
            source.LastProgressFraction,
            source.ConsecutivePagesRead);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.MediaConsumption.UnsupportedMediaKind, result.FirstError);
    }

    [Theory]
    [InlineData(double.NaN)] // a value that is not a number
    [InlineData(double.PositiveInfinity)] // a value beyond every finite fraction
    [InlineData(double.NegativeInfinity)] // a value below every finite fraction
    [InlineData(-0.01)] // a value below the zero lower boundary
    [InlineData(1.01)] // a value above the one upper boundary
    public void Reconstitute_WhenProgressFractionIsInvalid_ShouldReturnInvalidProgressFractionError(double lastProgressFraction)
    {
        // Arrange
        MediaConsumptionRecord source = _mediaConsumptionRecordFixture.Create(mediaKind: MediaKind.Movie, status: ConsumptionStatus.InProgress);

        // Act
        Result<MediaConsumptionRecord> result = MediaConsumptionRecord.Reconstitute(
            source.Id,
            source.UserId,
            source.LibraryId,
            source.MediaItemId,
            source.MediaKind,
            source.Status,
            source.StartedOnUtc,
            source.LastActivityOnUtc,
            source.EndedOnUtc,
            source.ConsumedDuration,
            source.LastPosition,
            lastProgressFraction,
            source.ConsecutivePagesRead);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.MediaConsumption.InvalidProgressFraction, result.FirstError);
    }

    [Theory]
    [InlineData(MediaKind.Movie)] // a duration based kind does not count pages
    [InlineData(MediaKind.Photo)] // a one-shot kind does not count pages
    [InlineData(MediaKind.MusicTrack)] // a duration based kind does not count pages
    public void Reconstitute_WhenConsecutivePagesReadIsPositiveForANonPageBasedKind_ShouldReturnConsecutivePagesReadIsOnlySupportedForPageBasedMediaError(MediaKind mediaKind)
    {
        // Arrange
        MediaConsumptionRecord source = _mediaConsumptionRecordFixture.Create(
            mediaKind: mediaKind,
            status: ConsumptionStatus.InProgress,
            consecutivePagesRead: 0);

        // Act
        Result<MediaConsumptionRecord> result = MediaConsumptionRecord.Reconstitute(
            source.Id,
            source.UserId,
            source.LibraryId,
            source.MediaItemId,
            source.MediaKind,
            source.Status,
            source.StartedOnUtc,
            source.LastActivityOnUtc,
            source.EndedOnUtc,
            source.ConsumedDuration,
            source.LastPosition,
            source.LastProgressFraction,
            3);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.MediaConsumption.ConsecutivePagesReadIsOnlySupportedForPageBasedMedia, result.FirstError);
    }

    [Fact]
    public void Reconstitute_WhenConsecutivePagesReadIsPositiveForABook_ShouldCreateRecordWithThatCount()
    {
        // Arrange
        MediaConsumptionRecord source = _mediaConsumptionRecordFixture.Create(
            mediaKind: MediaKind.Book,
            status: ConsumptionStatus.InProgress,
            consecutivePagesRead: 0);

        // Act
        Result<MediaConsumptionRecord> result = MediaConsumptionRecord.Reconstitute(
            source.Id,
            source.UserId,
            source.LibraryId,
            source.MediaItemId,
            source.MediaKind,
            source.Status,
            source.StartedOnUtc,
            source.LastActivityOnUtc,
            source.EndedOnUtc,
            source.ConsumedDuration,
            source.LastPosition,
            source.LastProgressFraction,
            7);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(7, result.Value.ConsecutivePagesRead);
    }

    [Theory]
    [InlineData(MediaKind.Movie)] // a duration based kind accepts a zero page count
    [InlineData(MediaKind.Photo)] // a one-shot kind accepts a zero page count
    public void Reconstitute_WhenConsecutivePagesReadIsZeroForANonPageBasedKind_ShouldCreateRecord(MediaKind mediaKind)
    {
        // Arrange
        MediaConsumptionRecord source = _mediaConsumptionRecordFixture.Create(
            mediaKind: mediaKind,
            status: ConsumptionStatus.InProgress,
            consecutivePagesRead: 0);

        // Act
        Result<MediaConsumptionRecord> result = MediaConsumptionRecord.Reconstitute(
            source.Id,
            source.UserId,
            source.LibraryId,
            source.MediaItemId,
            source.MediaKind,
            source.Status,
            source.StartedOnUtc,
            source.LastActivityOnUtc,
            source.EndedOnUtc,
            source.ConsumedDuration,
            source.LastPosition,
            source.LastProgressFraction,
            0);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(0, result.Value.ConsecutivePagesRead);
    }

    [Theory]
    [InlineData(MediaKind.Movie, MediaPositionKind.Location)] // a movie resumes from a time offset, not from a location
    [InlineData(MediaKind.Book, MediaPositionKind.TimeOffset)] // a book resumes from a location, not from a time offset
    [InlineData(MediaKind.Photo, MediaPositionKind.Location)] // a photo cannot be resumed, so it accepts no position at all
    public void Reconstitute_WhenPositionDoesNotMatchTheMediaKindRule_ShouldReturnInvalidMediaPositionError(MediaKind mediaKind, MediaPositionKind positionKind)
    {
        // Arrange
        MediaConsumptionRecord source = _mediaConsumptionRecordFixture.Create(
            mediaKind: mediaKind,
            status: ConsumptionStatus.InProgress,
            includeLastPosition: false);
        MediaPosition mismatchedPosition = _mediaPositionFixture.Create(kind: positionKind);

        // Act
        Result<MediaConsumptionRecord> result = MediaConsumptionRecord.Reconstitute(
            source.Id,
            source.UserId,
            source.LibraryId,
            source.MediaItemId,
            source.MediaKind,
            source.Status,
            source.StartedOnUtc,
            source.LastActivityOnUtc,
            source.EndedOnUtc,
            source.ConsumedDuration,
            Optional<MediaPosition>.Some(mismatchedPosition),
            source.LastProgressFraction,
            source.ConsecutivePagesRead);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.MediaConsumption.InvalidMediaPosition, result.FirstError);
    }

    [Fact]
    public void Reconstitute_WhenStatusIsInProgressAndEndTimeIsSet_ShouldReturnInProgressConsumptionCannotHaveEndTimeError()
    {
        // Arrange
        MediaConsumptionRecord source = _mediaConsumptionRecordFixture.Create(
            mediaKind: MediaKind.Movie,
            status: ConsumptionStatus.InProgress);

        // Act
        Result<MediaConsumptionRecord> result = MediaConsumptionRecord.Reconstitute(
            source.Id,
            source.UserId,
            source.LibraryId,
            source.MediaItemId,
            source.MediaKind,
            source.Status,
            source.StartedOnUtc,
            source.LastActivityOnUtc,
            Optional<DateTime>.Some(source.LastActivityOnUtc),
            source.ConsumedDuration,
            source.LastPosition,
            source.LastProgressFraction,
            source.ConsecutivePagesRead);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.MediaConsumption.InProgressConsumptionCannotHaveEndTime, result.FirstError);
    }

    [Theory]
    [InlineData(ConsumptionStatus.Completed)] // a completed session must record when it ended
    [InlineData(ConsumptionStatus.Stopped)] // a stopped session must record when it ended
    public void Reconstitute_WhenTerminalStatusHasNoEndTime_ShouldReturnEndedConsumptionMustHaveEndTimeError(ConsumptionStatus status)
    {
        // Arrange
        MediaConsumptionRecord source = _mediaConsumptionRecordFixture.Create(
            mediaKind: MediaKind.Movie,
            status: status);

        // Act
        Result<MediaConsumptionRecord> result = MediaConsumptionRecord.Reconstitute(
            source.Id,
            source.UserId,
            source.LibraryId,
            source.MediaItemId,
            source.MediaKind,
            status,
            source.StartedOnUtc,
            source.LastActivityOnUtc,
            Optional<DateTime>.None(),
            source.ConsumedDuration,
            source.LastPosition,
            source.LastProgressFraction,
            source.ConsecutivePagesRead);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.MediaConsumption.EndedConsumptionMustHaveEndTime, result.FirstError);
    }

    [Fact]
    public void Reconstitute_WhenLastActivityPrecedesStart_ShouldReturnLastActivityCannotPrecedeStartError()
    {
        // Arrange
        MediaConsumptionRecord source = _mediaConsumptionRecordFixture.Create(
            mediaKind: MediaKind.Movie,
            status: ConsumptionStatus.InProgress);

        // Act
        Result<MediaConsumptionRecord> result = MediaConsumptionRecord.Reconstitute(
            source.Id,
            source.UserId,
            source.LibraryId,
            source.MediaItemId,
            source.MediaKind,
            source.Status,
            source.StartedOnUtc,
            source.StartedOnUtc.AddTicks(-1),
            source.EndedOnUtc,
            source.ConsumedDuration,
            source.LastPosition,
            source.LastProgressFraction,
            source.ConsecutivePagesRead);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.MediaConsumption.LastActivityCannotPrecedeStart, result.FirstError);
    }

    [Theory]
    [InlineData(ConsumptionStatus.Completed)] // a completed session cannot end before its last activity
    [InlineData(ConsumptionStatus.Stopped)] // a stopped session cannot end before its last activity
    public void Reconstitute_WhenEndTimePrecedesLastActivity_ShouldReturnEndedConsumptionCannotPrecedeLastActivityError(ConsumptionStatus status)
    {
        // Arrange
        MediaConsumptionRecord source = _mediaConsumptionRecordFixture.Create(
            mediaKind: MediaKind.Movie,
            status: status);

        // Act
        Result<MediaConsumptionRecord> result = MediaConsumptionRecord.Reconstitute(
            source.Id,
            source.UserId,
            source.LibraryId,
            source.MediaItemId,
            source.MediaKind,
            status,
            source.StartedOnUtc,
            source.LastActivityOnUtc,
            Optional<DateTime>.Some(source.LastActivityOnUtc.AddTicks(-1)),
            source.ConsumedDuration,
            source.LastPosition,
            source.LastProgressFraction,
            source.ConsecutivePagesRead);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.MediaConsumption.EndedConsumptionCannotPrecedeLastActivity, result.FirstError);
    }

    [Theory]
    [InlineData(ConsumptionStatus.Completed)] // a completed session may end exactly at its last activity
    [InlineData(ConsumptionStatus.Stopped)] // a stopped session may end exactly at its last activity
    public void Reconstitute_WhenEndTimeEqualsLastActivity_ShouldCreateRecord(ConsumptionStatus status)
    {
        // Arrange
        MediaConsumptionRecord source = _mediaConsumptionRecordFixture.Create(
            mediaKind: MediaKind.Movie,
            status: status);

        // Act
        Result<MediaConsumptionRecord> result = MediaConsumptionRecord.Reconstitute(
            source.Id,
            source.UserId,
            source.LibraryId,
            source.MediaItemId,
            source.MediaKind,
            status,
            source.StartedOnUtc,
            source.LastActivityOnUtc,
            Optional<DateTime>.Some(source.LastActivityOnUtc),
            source.ConsumedDuration,
            source.LastPosition,
            source.LastProgressFraction,
            source.ConsecutivePagesRead);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(Optional<DateTime>.Some(source.LastActivityOnUtc), result.Value.EndedOnUtc);
    }

    [Fact]
    public void ReportProgress_WhenCalledWithAValidPosition_ShouldUpdateProgressAndRaiseProgressedEvent()
    {
        // Arrange
        DateTime startedOnUtc = DateTime.UtcNow.AddHours(-2);
        DateTime lastActivityOnUtc = startedOnUtc.AddMinutes(30);
        MediaPosition previousPosition = _mediaPositionFixture.Create(kind: MediaPositionKind.TimeOffset);
        MediaConsumptionRecord record = _mediaConsumptionRecordFixture.Create(
            mediaKind: MediaKind.Movie,
            status: ConsumptionStatus.InProgress,
            startedOnUtc: startedOnUtc,
            lastActivityOnUtc: lastActivityOnUtc,
            consumedDuration: TimeSpan.FromMinutes(10),
            lastPosition: Optional<MediaPosition>.Some(previousPosition),
            lastProgressFraction: 0.2);
        MediaPosition newPosition = _mediaPositionFixture.Create(kind: MediaPositionKind.TimeOffset);

        // Act
        Result<Updated> result = record.ReportProgress(Optional<MediaPosition>.Some(newPosition), 0.55, TimeSpan.FromMinutes(20), 0);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(Result.Updated, result.Value);
        Assert.Equal(Optional<MediaPosition>.Some(newPosition), record.LastPosition);
        Assert.Equal(0.55, record.LastProgressFraction);
        Assert.Equal(TimeSpan.FromMinutes(20), record.ConsumedDuration);
        Assert.Equal(0, record.ConsecutivePagesRead);
        Assert.True(record.LastActivityOnUtc > lastActivityOnUtc);
        Assert.Equal(Optional<DateTime>.Some(record.LastActivityOnUtc), record.UpdatedOnUtc);
        MediaConsumptionProgressedDomainEvent progressedEvent = Assert.IsType<MediaConsumptionProgressedDomainEvent>(Assert.Single(record.GetDomainEvents()));
        Assert.Equal(record.Id, progressedEvent.MediaConsumptionRecordId);
        Assert.Equal(record.UserId, progressedEvent.UserId);
        Assert.Equal(record.LibraryId, progressedEvent.LibraryId);
        Assert.Equal(record.MediaItemId, progressedEvent.MediaItemId);
        Assert.Equal(MediaKind.Movie, progressedEvent.MediaKind);
        Assert.Equal(Optional<MediaPosition>.Some(newPosition), progressedEvent.LastPosition);
        Assert.Equal(0.55, progressedEvent.LastProgressFraction);
        Assert.Equal(TimeSpan.FromMinutes(20), progressedEvent.ConsumedDuration);
        Assert.Equal(0, progressedEvent.ConsecutivePagesRead);
        Assert.Equal(record.LastActivityOnUtc, progressedEvent.OccurredOnUtc);
    }

    [Fact]
    public void ReportProgress_WhenNoPositionIsSupplied_ShouldKeepTheLastKnownPosition()
    {
        // Arrange
        MediaPosition previousPosition = _mediaPositionFixture.Create(kind: MediaPositionKind.TimeOffset);
        MediaConsumptionRecord record = _mediaConsumptionRecordFixture.Create(
            mediaKind: MediaKind.Movie,
            status: ConsumptionStatus.InProgress,
            consumedDuration: TimeSpan.FromMinutes(10),
            lastPosition: Optional<MediaPosition>.Some(previousPosition));

        // Act
        Result<Updated> result = record.ReportProgress(Optional<MediaPosition>.None(), 0.55, TimeSpan.FromMinutes(20), 0);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(Optional<MediaPosition>.Some(previousPosition), record.LastPosition);
        MediaConsumptionProgressedDomainEvent progressedEvent = Assert.IsType<MediaConsumptionProgressedDomainEvent>(Assert.Single(record.GetDomainEvents()));
        Assert.Equal(Optional<MediaPosition>.Some(previousPosition), progressedEvent.LastPosition);
    }

    [Theory]
    [InlineData(ConsumptionStatus.Completed)] // a completed session accepted no further progress
    [InlineData(ConsumptionStatus.Stopped)] // a stopped session accepted no further progress
    public void ReportProgress_WhenStatusIsNotInProgress_ShouldReturnConsumptionIsNotInProgressError(ConsumptionStatus status)
    {
        // Arrange
        MediaConsumptionRecord record = _mediaConsumptionRecordFixture.Create(
            mediaKind: MediaKind.Movie,
            status: status,
            consumedDuration: TimeSpan.FromMinutes(10));
        Optional<MediaPosition> originalLastPosition = record.LastPosition;
        double originalLastProgressFraction = record.LastProgressFraction;
        TimeSpan originalConsumedDuration = record.ConsumedDuration;
        int originalConsecutivePagesRead = record.ConsecutivePagesRead;
        DateTime originalLastActivityOnUtc = record.LastActivityOnUtc;
        Optional<DateTime> originalUpdatedOnUtc = record.UpdatedOnUtc;
        MediaPosition position = _mediaPositionFixture.Create(kind: MediaPositionKind.TimeOffset);

        // Act
        Result<Updated> result = record.ReportProgress(Optional<MediaPosition>.Some(position), 0.9, TimeSpan.FromMinutes(30), 0);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.MediaConsumption.ConsumptionIsNotInProgress, result.FirstError);
        Assert.Equal(originalLastPosition, record.LastPosition);
        Assert.Equal(originalLastProgressFraction, record.LastProgressFraction);
        Assert.Equal(originalConsumedDuration, record.ConsumedDuration);
        Assert.Equal(originalConsecutivePagesRead, record.ConsecutivePagesRead);
        Assert.Equal(originalLastActivityOnUtc, record.LastActivityOnUtc);
        Assert.Equal(originalUpdatedOnUtc, record.UpdatedOnUtc);
        Assert.Empty(record.GetDomainEvents());
    }

    [Theory]
    [InlineData(double.NaN)] // a value that is not a number
    [InlineData(double.PositiveInfinity)] // a value beyond every finite fraction
    [InlineData(double.NegativeInfinity)] // a value below every finite fraction
    [InlineData(-0.01)] // a value below the zero lower boundary
    [InlineData(1.01)] // a value above the one upper boundary
    public void ReportProgress_WhenProgressFractionIsInvalid_ShouldReturnInvalidProgressFractionError(double lastProgressFraction)
    {
        // Arrange
        MediaConsumptionRecord record = _mediaConsumptionRecordFixture.Create(
            mediaKind: MediaKind.Movie,
            status: ConsumptionStatus.InProgress,
            consumedDuration: TimeSpan.FromMinutes(10));

        // Act
        Result<Updated> result = record.ReportProgress(Optional<MediaPosition>.None(), lastProgressFraction, TimeSpan.FromMinutes(20), 0);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.MediaConsumption.InvalidProgressFraction, result.FirstError);
    }

    [Fact]
    public void ReportProgress_WhenConsumedDurationIsLowerThanCurrent_ShouldReturnConsumedDurationCannotDecreaseError()
    {
        // Arrange
        MediaConsumptionRecord record = _mediaConsumptionRecordFixture.Create(
            mediaKind: MediaKind.Movie,
            status: ConsumptionStatus.InProgress,
            consumedDuration: TimeSpan.FromMinutes(10));

        // Act
        Result<Updated> result = record.ReportProgress(Optional<MediaPosition>.None(), 0.5, TimeSpan.FromMinutes(10).Subtract(TimeSpan.FromSeconds(1)), 0);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.MediaConsumption.ConsumedDurationCannotDecrease, result.FirstError);
        Assert.Equal(TimeSpan.FromMinutes(10), record.ConsumedDuration);
    }

    [Fact]
    public void ReportProgress_WhenConsumedDurationEqualsCurrent_ShouldUpdateProgress()
    {
        // Arrange
        MediaConsumptionRecord record = _mediaConsumptionRecordFixture.Create(
            mediaKind: MediaKind.Movie,
            status: ConsumptionStatus.InProgress,
            consumedDuration: TimeSpan.FromMinutes(10));

        // Act
        Result<Updated> result = record.ReportProgress(Optional<MediaPosition>.None(), 0.5, TimeSpan.FromMinutes(10), 0);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(TimeSpan.FromMinutes(10), record.ConsumedDuration);
        Assert.Single(record.GetDomainEvents());
    }

    [Theory]
    [InlineData(MediaKind.Movie)] // a duration based kind does not count pages
    [InlineData(MediaKind.Photo)] // a one-shot kind does not count pages
    public void ReportProgress_WhenConsecutivePagesReadIsPositiveForANonPageBasedKind_ShouldReturnConsecutivePagesReadIsOnlySupportedForPageBasedMediaError(MediaKind mediaKind)
    {
        // Arrange
        MediaConsumptionRecord record = _mediaConsumptionRecordFixture.Create(
            mediaKind: mediaKind,
            status: ConsumptionStatus.InProgress,
            consumedDuration: TimeSpan.FromMinutes(10),
            consecutivePagesRead: 0);

        // Act
        Result<Updated> result = record.ReportProgress(Optional<MediaPosition>.None(), 0.5, TimeSpan.FromMinutes(20), 3);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.MediaConsumption.ConsecutivePagesReadIsOnlySupportedForPageBasedMedia, result.FirstError);
        Assert.Equal(0, record.ConsecutivePagesRead);
    }

    [Fact]
    public void ReportProgress_WhenConsecutivePagesReadIsPositiveForABook_ShouldUpdateTheCount()
    {
        // Arrange
        MediaPosition location = _mediaPositionFixture.Create(kind: MediaPositionKind.Location);
        MediaConsumptionRecord record = _mediaConsumptionRecordFixture.Create(
            mediaKind: MediaKind.Book,
            status: ConsumptionStatus.InProgress,
            consumedDuration: TimeSpan.FromMinutes(10),
            lastPosition: Optional<MediaPosition>.Some(location),
            consecutivePagesRead: 4);
        MediaPosition newLocation = _mediaPositionFixture.Create(kind: MediaPositionKind.Location);

        // Act
        Result<Updated> result = record.ReportProgress(Optional<MediaPosition>.Some(newLocation), 0.5, TimeSpan.FromMinutes(20), 7);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(7, record.ConsecutivePagesRead);
        MediaConsumptionProgressedDomainEvent progressedEvent = Assert.IsType<MediaConsumptionProgressedDomainEvent>(Assert.Single(record.GetDomainEvents()));
        Assert.Equal(7, progressedEvent.ConsecutivePagesRead);
    }

    [Theory]
    [InlineData(MediaKind.Movie, MediaPositionKind.Location)] // a movie resumes from a time offset, not from a location
    [InlineData(MediaKind.Book, MediaPositionKind.TimeOffset)] // a book resumes from a location, not from a time offset
    [InlineData(MediaKind.Photo, MediaPositionKind.TimeOffset)] // a photo cannot be resumed, so it accepts no position at all
    public void ReportProgress_WhenPositionDoesNotMatchTheMediaKindRule_ShouldReturnInvalidMediaPositionError(MediaKind mediaKind, MediaPositionKind positionKind)
    {
        // Arrange
        MediaConsumptionRecord record = _mediaConsumptionRecordFixture.Create(
            mediaKind: mediaKind,
            status: ConsumptionStatus.InProgress,
            consumedDuration: TimeSpan.FromMinutes(10),
            includeLastPosition: false);
        MediaPosition mismatchedPosition = _mediaPositionFixture.Create(kind: positionKind);

        // Act
        Result<Updated> result = record.ReportProgress(Optional<MediaPosition>.Some(mismatchedPosition), 0.5, TimeSpan.FromMinutes(20), 0);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.MediaConsumption.InvalidMediaPosition, result.FirstError);
    }

    [Fact]
    public void ReportProgress_WhenItFails_ShouldLeaveTheRecordUnchangedAndRaiseNoEvents()
    {
        // Arrange
        MediaPosition previousPosition = _mediaPositionFixture.Create(kind: MediaPositionKind.TimeOffset);
        MediaConsumptionRecord record = _mediaConsumptionRecordFixture.Create(
            mediaKind: MediaKind.Movie,
            status: ConsumptionStatus.InProgress,
            consumedDuration: TimeSpan.FromMinutes(10),
            lastPosition: Optional<MediaPosition>.Some(previousPosition),
            lastProgressFraction: 0.2);
        Optional<MediaPosition> originalLastPosition = record.LastPosition;
        double originalLastProgressFraction = record.LastProgressFraction;
        TimeSpan originalConsumedDuration = record.ConsumedDuration;
        int originalConsecutivePagesRead = record.ConsecutivePagesRead;
        DateTime originalLastActivityOnUtc = record.LastActivityOnUtc;
        Optional<DateTime> originalUpdatedOnUtc = record.UpdatedOnUtc;
        MediaPosition mismatchedPosition = _mediaPositionFixture.Create(kind: MediaPositionKind.Location);

        // Act
        Result<Updated> result = record.ReportProgress(Optional<MediaPosition>.Some(mismatchedPosition), 0.9, TimeSpan.FromMinutes(30), 0);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.MediaConsumption.InvalidMediaPosition, result.FirstError);
        Assert.Equal(ConsumptionStatus.InProgress, record.Status);
        Assert.Equal(originalLastPosition, record.LastPosition);
        Assert.Equal(originalLastProgressFraction, record.LastProgressFraction);
        Assert.Equal(originalConsumedDuration, record.ConsumedDuration);
        Assert.Equal(originalConsecutivePagesRead, record.ConsecutivePagesRead);
        Assert.Equal(originalLastActivityOnUtc, record.LastActivityOnUtc);
        Assert.Equal(originalUpdatedOnUtc, record.UpdatedOnUtc);
        Assert.Empty(record.GetDomainEvents());
    }

    [Fact]
    public void ReportProgress_WhenLastActivityOnUtcIsInTheFuture_ShouldAdvanceActivityByExactlyOneTick()
    {
        // Arrange
        DateTime startedOnUtc = DateTime.UtcNow.AddHours(-1);
        DateTime futureLastActivityOnUtc = DateTime.UtcNow.AddDays(2);
        MediaConsumptionRecord record = _mediaConsumptionRecordFixture.Create(
            mediaKind: MediaKind.Movie,
            status: ConsumptionStatus.InProgress,
            startedOnUtc: startedOnUtc,
            lastActivityOnUtc: futureLastActivityOnUtc,
            consumedDuration: TimeSpan.FromMinutes(10));

        // Act
        Result<Updated> result = record.ReportProgress(Optional<MediaPosition>.None(), 0.5, TimeSpan.FromMinutes(20), 0);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(futureLastActivityOnUtc.AddTicks(1), record.LastActivityOnUtc);
        Assert.Equal(Optional<DateTime>.Some(futureLastActivityOnUtc.AddTicks(1)), record.UpdatedOnUtc);
    }

    [Fact]
    public void ReportProgress_WhenLastActivityOnUtcIsTheMaximumValue_ShouldKeepTheActivityAtThatValue()
    {
        // Arrange
        DateTime startedOnUtc = DateTime.UtcNow.AddHours(-1);
        MediaConsumptionRecord record = _mediaConsumptionRecordFixture.Create(
            mediaKind: MediaKind.Movie,
            status: ConsumptionStatus.InProgress,
            startedOnUtc: startedOnUtc,
            lastActivityOnUtc: DateTime.MaxValue,
            consumedDuration: TimeSpan.FromMinutes(10));

        // Act
        Result<Updated> result = record.ReportProgress(Optional<MediaPosition>.None(), 0.5, TimeSpan.FromMinutes(20), 0);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(DateTime.MaxValue, record.LastActivityOnUtc);
        Assert.Equal(Optional<DateTime>.Some(DateTime.MaxValue), record.UpdatedOnUtc);
        MediaConsumptionProgressedDomainEvent progressedEvent = Assert.IsType<MediaConsumptionProgressedDomainEvent>(Assert.Single(record.GetDomainEvents()));
        Assert.Equal(DateTime.MaxValue, progressedEvent.OccurredOnUtc);
    }

    [Fact]
    public void Complete_WhenCalledWithAValidPosition_ShouldCompleteTheRecordAndRaiseCompletedEvent()
    {
        // Arrange
        DateTime startedOnUtc = DateTime.UtcNow.AddHours(-2);
        DateTime lastActivityOnUtc = startedOnUtc.AddMinutes(30);
        MediaPosition previousPosition = _mediaPositionFixture.Create(kind: MediaPositionKind.TimeOffset);
        MediaConsumptionRecord record = _mediaConsumptionRecordFixture.Create(
            mediaKind: MediaKind.Movie,
            status: ConsumptionStatus.InProgress,
            startedOnUtc: startedOnUtc,
            lastActivityOnUtc: lastActivityOnUtc,
            consumedDuration: TimeSpan.FromMinutes(10),
            lastPosition: Optional<MediaPosition>.Some(previousPosition),
            lastProgressFraction: 0.2);
        MediaPosition finalPosition = _mediaPositionFixture.Create(kind: MediaPositionKind.TimeOffset);

        // Act
        Result<Updated> result = record.Complete(Optional<MediaPosition>.Some(finalPosition), 0.9, TimeSpan.FromMinutes(25), Optional<int>.None());

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(Result.Updated, result.Value);
        Assert.Equal(ConsumptionStatus.Completed, record.Status);
        Assert.True(record.EndedOnUtc.HasValue);
        Assert.True(record.LastActivityOnUtc > lastActivityOnUtc);
        Assert.Equal(record.LastActivityOnUtc, record.EndedOnUtc.Value);
        Assert.Equal(Optional<DateTime>.Some(record.LastActivityOnUtc), record.UpdatedOnUtc);
        Assert.Equal(Optional<MediaPosition>.Some(finalPosition), record.LastPosition);
        Assert.Equal(0.9, record.LastProgressFraction);
        Assert.Equal(TimeSpan.FromMinutes(25), record.ConsumedDuration);
        MediaConsumptionCompletedDomainEvent completedEvent = Assert.IsType<MediaConsumptionCompletedDomainEvent>(Assert.Single(record.GetDomainEvents()));
        Assert.Equal(record.Id, completedEvent.MediaConsumptionRecordId);
        Assert.Equal(record.UserId, completedEvent.UserId);
        Assert.Equal(record.LibraryId, completedEvent.LibraryId);
        Assert.Equal(record.MediaItemId, completedEvent.MediaItemId);
        Assert.Equal(MediaKind.Movie, completedEvent.MediaKind);
        Assert.Equal(Optional<MediaPosition>.Some(finalPosition), completedEvent.LastPosition);
        Assert.Equal(0.9, completedEvent.LastProgressFraction);
        Assert.Equal(TimeSpan.FromMinutes(25), completedEvent.ConsumedDuration);
        Assert.Equal(record.LastActivityOnUtc, completedEvent.OccurredOnUtc);
    }

    [Fact]
    public void Complete_WhenNoPositionIsSupplied_ShouldKeepTheLastKnownPosition()
    {
        // Arrange
        MediaPosition previousPosition = _mediaPositionFixture.Create(kind: MediaPositionKind.TimeOffset);
        MediaConsumptionRecord record = _mediaConsumptionRecordFixture.Create(
            mediaKind: MediaKind.Movie,
            status: ConsumptionStatus.InProgress,
            consumedDuration: TimeSpan.FromMinutes(10),
            lastPosition: Optional<MediaPosition>.Some(previousPosition));

        // Act
        Result<Updated> result = record.Complete(Optional<MediaPosition>.None(), 0.9, TimeSpan.FromMinutes(25), Optional<int>.None());

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(Optional<MediaPosition>.Some(previousPosition), record.LastPosition);
        MediaConsumptionCompletedDomainEvent completedEvent = Assert.IsType<MediaConsumptionCompletedDomainEvent>(Assert.Single(record.GetDomainEvents()));
        Assert.Equal(Optional<MediaPosition>.Some(previousPosition), completedEvent.LastPosition);
    }

    [Fact]
    public void Complete_WhenCalledOnABook_ShouldPreserveTheConsecutivePagesReadCount()
    {
        // Arrange
        MediaPosition previousPosition = _mediaPositionFixture.Create(kind: MediaPositionKind.Location);
        MediaConsumptionRecord record = _mediaConsumptionRecordFixture.Create(
            mediaKind: MediaKind.Book,
            status: ConsumptionStatus.InProgress,
            consumedDuration: TimeSpan.FromMinutes(10),
            lastPosition: Optional<MediaPosition>.Some(previousPosition),
            consecutivePagesRead: 5);

        // Act
        Result<Updated> result = record.Complete(Optional<MediaPosition>.None(), 1.0, TimeSpan.FromMinutes(30), Optional<int>.None());

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(5, record.ConsecutivePagesRead);
        MediaConsumptionCompletedDomainEvent completedEvent = Assert.IsType<MediaConsumptionCompletedDomainEvent>(Assert.Single(record.GetDomainEvents()));
        Assert.Equal(5, completedEvent.ConsecutivePagesRead);
    }

    [Fact]
    public void Complete_WhenConsecutivePagesReadIsSuppliedForABook_ShouldUpdateTheCount()
    {
        // Arrange
        MediaPosition previousPosition = _mediaPositionFixture.Create(kind: MediaPositionKind.Location);
        MediaConsumptionRecord record = _mediaConsumptionRecordFixture.Create(
            mediaKind: MediaKind.Book,
            status: ConsumptionStatus.InProgress,
            consumedDuration: TimeSpan.FromMinutes(10),
            lastPosition: Optional<MediaPosition>.Some(previousPosition),
            consecutivePagesRead: 5);

        // Act
        Result<Updated> result = record.Complete(Optional<MediaPosition>.None(), 1.0, TimeSpan.FromMinutes(30), Optional<int>.Some(7));

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(7, record.ConsecutivePagesRead);
        MediaConsumptionCompletedDomainEvent completedEvent = Assert.IsType<MediaConsumptionCompletedDomainEvent>(Assert.Single(record.GetDomainEvents()));
        Assert.Equal(7, completedEvent.ConsecutivePagesRead);
    }

    [Fact]
    public void Complete_WhenConsecutivePagesReadIsZeroIsSuppliedForABook_ShouldResetTheCount()
    {
        // Arrange
        MediaPosition previousPosition = _mediaPositionFixture.Create(kind: MediaPositionKind.Location);
        MediaConsumptionRecord record = _mediaConsumptionRecordFixture.Create(
            mediaKind: MediaKind.Book,
            status: ConsumptionStatus.InProgress,
            consumedDuration: TimeSpan.FromMinutes(10),
            lastPosition: Optional<MediaPosition>.Some(previousPosition),
            consecutivePagesRead: 5);

        // Act
        Result<Updated> result = record.Complete(Optional<MediaPosition>.None(), 1.0, TimeSpan.FromMinutes(30), Optional<int>.Some(0));

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(0, record.ConsecutivePagesRead);
        MediaConsumptionCompletedDomainEvent completedEvent = Assert.IsType<MediaConsumptionCompletedDomainEvent>(Assert.Single(record.GetDomainEvents()));
        Assert.Equal(0, completedEvent.ConsecutivePagesRead);
    }

    [Theory]
    [InlineData(MediaKind.Movie)] // a duration based kind does not count pages
    [InlineData(MediaKind.Photo)] // a one-shot kind does not count pages
    public void Complete_WhenConsecutivePagesReadIsPositiveForANonPageBasedKind_ShouldReturnConsecutivePagesReadIsOnlySupportedForPageBasedMediaError(MediaKind mediaKind)
    {
        // Arrange
        MediaConsumptionRecord record = _mediaConsumptionRecordFixture.Create(
            mediaKind: mediaKind,
            status: ConsumptionStatus.InProgress,
            consumedDuration: TimeSpan.FromMinutes(10),
            consecutivePagesRead: 0);

        // Act
        Result<Updated> result = record.Complete(Optional<MediaPosition>.None(), 1.0, TimeSpan.FromMinutes(20), Optional<int>.Some(3));

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.MediaConsumption.ConsecutivePagesReadIsOnlySupportedForPageBasedMedia, result.FirstError);
        Assert.Equal(ConsumptionStatus.InProgress, record.Status);
        Assert.Equal(0, record.ConsecutivePagesRead);
        Assert.Empty(record.GetDomainEvents());
    }

    [Theory]
    [InlineData(ConsumptionStatus.Completed)] // an already completed session cannot complete again
    [InlineData(ConsumptionStatus.Stopped)] // an already stopped session cannot complete
    public void Complete_WhenStatusIsTerminal_ShouldReturnConsumptionIsAlreadyTerminalError(ConsumptionStatus status)
    {
        // Arrange
        MediaConsumptionRecord record = _mediaConsumptionRecordFixture.Create(
            mediaKind: MediaKind.Movie,
            status: status,
            consumedDuration: TimeSpan.FromMinutes(10));
        ConsumptionStatus originalStatus = record.Status;
        Optional<DateTime> originalEndedOnUtc = record.EndedOnUtc;
        DateTime originalLastActivityOnUtc = record.LastActivityOnUtc;
        Optional<DateTime> originalUpdatedOnUtc = record.UpdatedOnUtc;
        MediaPosition position = _mediaPositionFixture.Create(kind: MediaPositionKind.TimeOffset);

        // Act
        Result<Updated> result = record.Complete(Optional<MediaPosition>.Some(position), 0.9, TimeSpan.FromMinutes(25), Optional<int>.None());

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.MediaConsumption.ConsumptionIsAlreadyTerminal, result.FirstError);
        Assert.Equal(originalStatus, record.Status);
        Assert.Equal(originalEndedOnUtc, record.EndedOnUtc);
        Assert.Equal(originalLastActivityOnUtc, record.LastActivityOnUtc);
        Assert.Equal(originalUpdatedOnUtc, record.UpdatedOnUtc);
        Assert.Empty(record.GetDomainEvents());
    }

    [Theory]
    [InlineData(double.NaN)] // a value that is not a number
    [InlineData(double.PositiveInfinity)] // a value beyond every finite fraction
    [InlineData(double.NegativeInfinity)] // a value below every finite fraction
    [InlineData(-0.01)] // a value below the zero lower boundary
    [InlineData(1.01)] // a value above the one upper boundary
    public void Complete_WhenProgressFractionIsInvalid_ShouldReturnInvalidProgressFractionError(double lastProgressFraction)
    {
        // Arrange
        MediaConsumptionRecord record = _mediaConsumptionRecordFixture.Create(
            mediaKind: MediaKind.Movie,
            status: ConsumptionStatus.InProgress,
            consumedDuration: TimeSpan.FromMinutes(10));

        // Act
        Result<Updated> result = record.Complete(Optional<MediaPosition>.None(), lastProgressFraction, TimeSpan.FromMinutes(20), Optional<int>.None());

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.MediaConsumption.InvalidProgressFraction, result.FirstError);
    }

    [Fact]
    public void Complete_WhenConsumedDurationIsLowerThanCurrent_ShouldReturnConsumedDurationCannotDecreaseError()
    {
        // Arrange
        MediaConsumptionRecord record = _mediaConsumptionRecordFixture.Create(
            mediaKind: MediaKind.Movie,
            status: ConsumptionStatus.InProgress,
            consumedDuration: TimeSpan.FromMinutes(10));

        // Act
        Result<Updated> result = record.Complete(Optional<MediaPosition>.None(), 0.9, TimeSpan.FromMinutes(10).Subtract(TimeSpan.FromSeconds(1)), Optional<int>.None());

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.MediaConsumption.ConsumedDurationCannotDecrease, result.FirstError);
        Assert.Equal(TimeSpan.FromMinutes(10), record.ConsumedDuration);
    }

    [Fact]
    public void Complete_WhenConsumedDurationEqualsCurrent_ShouldCompleteTheRecord()
    {
        // Arrange
        MediaConsumptionRecord record = _mediaConsumptionRecordFixture.Create(
            mediaKind: MediaKind.Movie,
            status: ConsumptionStatus.InProgress,
            consumedDuration: TimeSpan.FromMinutes(10));

        // Act
        Result<Updated> result = record.Complete(Optional<MediaPosition>.None(), 0.9, TimeSpan.FromMinutes(10), Optional<int>.None());

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(ConsumptionStatus.Completed, record.Status);
        Assert.Equal(TimeSpan.FromMinutes(10), record.ConsumedDuration);
        Assert.Single(record.GetDomainEvents());
    }

    [Theory]
    [InlineData(MediaKind.Movie, MediaPositionKind.Location)] // a movie resumes from a time offset, not from a location
    [InlineData(MediaKind.Book, MediaPositionKind.TimeOffset)] // a book resumes from a location, not from a time offset
    [InlineData(MediaKind.Photo, MediaPositionKind.Location)] // a photo cannot be resumed, so it accepts no position at all
    public void Complete_WhenPositionDoesNotMatchTheMediaKindRule_ShouldReturnInvalidMediaPositionError(MediaKind mediaKind, MediaPositionKind positionKind)
    {
        // Arrange
        MediaConsumptionRecord record = _mediaConsumptionRecordFixture.Create(
            mediaKind: mediaKind,
            status: ConsumptionStatus.InProgress,
            consumedDuration: TimeSpan.FromMinutes(10),
            includeLastPosition: false);
        MediaPosition mismatchedPosition = _mediaPositionFixture.Create(kind: positionKind);

        // Act
        Result<Updated> result = record.Complete(Optional<MediaPosition>.Some(mismatchedPosition), 0.9, TimeSpan.FromMinutes(20), Optional<int>.None());

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.MediaConsumption.InvalidMediaPosition, result.FirstError);
    }

    [Fact]
    public void Complete_WhenItFails_ShouldNotChangeStatusEndTimeOrRaiseEvents()
    {
        // Arrange
        MediaPosition previousPosition = _mediaPositionFixture.Create(kind: MediaPositionKind.TimeOffset);
        MediaConsumptionRecord record = _mediaConsumptionRecordFixture.Create(
            mediaKind: MediaKind.Movie,
            status: ConsumptionStatus.InProgress,
            consumedDuration: TimeSpan.FromMinutes(10),
            lastPosition: Optional<MediaPosition>.Some(previousPosition),
            lastProgressFraction: 0.2);
        DateTime originalLastActivityOnUtc = record.LastActivityOnUtc;
        Optional<DateTime> originalUpdatedOnUtc = record.UpdatedOnUtc;

        // Act
        Result<Updated> result = record.Complete(Optional<MediaPosition>.None(), 1.5, TimeSpan.FromMinutes(20), Optional<int>.None());

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.MediaConsumption.InvalidProgressFraction, result.FirstError);
        Assert.Equal(ConsumptionStatus.InProgress, record.Status);
        Assert.False(record.EndedOnUtc.HasValue);
        Assert.Equal(originalLastActivityOnUtc, record.LastActivityOnUtc);
        Assert.Equal(originalUpdatedOnUtc, record.UpdatedOnUtc);
        Assert.Equal(Optional<MediaPosition>.Some(previousPosition), record.LastPosition);
        Assert.Equal(0.2, record.LastProgressFraction);
        Assert.Equal(TimeSpan.FromMinutes(10), record.ConsumedDuration);
        Assert.Empty(record.GetDomainEvents());
    }

    [Fact]
    public void Stop_WhenCalledWithAValidPosition_ShouldStopTheRecordAndRaiseStoppedEvent()
    {
        // Arrange
        DateTime startedOnUtc = DateTime.UtcNow.AddHours(-2);
        DateTime lastActivityOnUtc = startedOnUtc.AddMinutes(30);
        MediaPosition previousPosition = _mediaPositionFixture.Create(kind: MediaPositionKind.TimeOffset);
        MediaConsumptionRecord record = _mediaConsumptionRecordFixture.Create(
            mediaKind: MediaKind.Movie,
            status: ConsumptionStatus.InProgress,
            startedOnUtc: startedOnUtc,
            lastActivityOnUtc: lastActivityOnUtc,
            consumedDuration: TimeSpan.FromMinutes(10),
            lastPosition: Optional<MediaPosition>.Some(previousPosition),
            lastProgressFraction: 0.2);
        MediaPosition finalPosition = _mediaPositionFixture.Create(kind: MediaPositionKind.TimeOffset);

        // Act
        Result<Updated> result = record.Stop(Optional<MediaPosition>.Some(finalPosition), 0.35, TimeSpan.FromMinutes(18), Optional<int>.None());

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(Result.Updated, result.Value);
        Assert.Equal(ConsumptionStatus.Stopped, record.Status);
        Assert.True(record.EndedOnUtc.HasValue);
        Assert.True(record.LastActivityOnUtc > lastActivityOnUtc);
        Assert.Equal(record.LastActivityOnUtc, record.EndedOnUtc.Value);
        Assert.Equal(Optional<DateTime>.Some(record.LastActivityOnUtc), record.UpdatedOnUtc);
        Assert.Equal(Optional<MediaPosition>.Some(finalPosition), record.LastPosition);
        Assert.Equal(0.35, record.LastProgressFraction);
        Assert.Equal(TimeSpan.FromMinutes(18), record.ConsumedDuration);
        MediaConsumptionStoppedDomainEvent stoppedEvent = Assert.IsType<MediaConsumptionStoppedDomainEvent>(Assert.Single(record.GetDomainEvents()));
        Assert.Equal(record.Id, stoppedEvent.MediaConsumptionRecordId);
        Assert.Equal(record.UserId, stoppedEvent.UserId);
        Assert.Equal(record.LibraryId, stoppedEvent.LibraryId);
        Assert.Equal(record.MediaItemId, stoppedEvent.MediaItemId);
        Assert.Equal(MediaKind.Movie, stoppedEvent.MediaKind);
        Assert.Equal(Optional<MediaPosition>.Some(finalPosition), stoppedEvent.LastPosition);
        Assert.Equal(0.35, stoppedEvent.LastProgressFraction);
        Assert.Equal(TimeSpan.FromMinutes(18), stoppedEvent.ConsumedDuration);
        Assert.Equal(record.LastActivityOnUtc, stoppedEvent.OccurredOnUtc);
    }

    [Fact]
    public void Stop_WhenNoPositionIsSupplied_ShouldKeepTheLastKnownPosition()
    {
        // Arrange
        MediaPosition previousPosition = _mediaPositionFixture.Create(kind: MediaPositionKind.TimeOffset);
        MediaConsumptionRecord record = _mediaConsumptionRecordFixture.Create(
            mediaKind: MediaKind.Movie,
            status: ConsumptionStatus.InProgress,
            consumedDuration: TimeSpan.FromMinutes(10),
            lastPosition: Optional<MediaPosition>.Some(previousPosition));

        // Act
        Result<Updated> result = record.Stop(Optional<MediaPosition>.None(), 0.35, TimeSpan.FromMinutes(18), Optional<int>.None());

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(Optional<MediaPosition>.Some(previousPosition), record.LastPosition);
        MediaConsumptionStoppedDomainEvent stoppedEvent = Assert.IsType<MediaConsumptionStoppedDomainEvent>(Assert.Single(record.GetDomainEvents()));
        Assert.Equal(Optional<MediaPosition>.Some(previousPosition), stoppedEvent.LastPosition);
    }

    [Fact]
    public void Stop_WhenCalledOnABook_ShouldPreserveTheConsecutivePagesReadCount()
    {
        // Arrange
        MediaPosition previousPosition = _mediaPositionFixture.Create(kind: MediaPositionKind.Location);
        MediaConsumptionRecord record = _mediaConsumptionRecordFixture.Create(
            mediaKind: MediaKind.Book,
            status: ConsumptionStatus.InProgress,
            consumedDuration: TimeSpan.FromMinutes(10),
            lastPosition: Optional<MediaPosition>.Some(previousPosition),
            consecutivePagesRead: 5);

        // Act
        Result<Updated> result = record.Stop(Optional<MediaPosition>.None(), 0.5, TimeSpan.FromMinutes(30), Optional<int>.None());

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(5, record.ConsecutivePagesRead);
        MediaConsumptionStoppedDomainEvent stoppedEvent = Assert.IsType<MediaConsumptionStoppedDomainEvent>(Assert.Single(record.GetDomainEvents()));
        Assert.Equal(5, stoppedEvent.ConsecutivePagesRead);
    }

    [Fact]
    public void Stop_WhenConsecutivePagesReadIsSuppliedForABook_ShouldUpdateTheCount()
    {
        // Arrange
        MediaPosition previousPosition = _mediaPositionFixture.Create(kind: MediaPositionKind.Location);
        MediaConsumptionRecord record = _mediaConsumptionRecordFixture.Create(
            mediaKind: MediaKind.Book,
            status: ConsumptionStatus.InProgress,
            consumedDuration: TimeSpan.FromMinutes(10),
            lastPosition: Optional<MediaPosition>.Some(previousPosition),
            consecutivePagesRead: 5);

        // Act
        Result<Updated> result = record.Stop(Optional<MediaPosition>.None(), 0.5, TimeSpan.FromMinutes(30), Optional<int>.Some(7));

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(7, record.ConsecutivePagesRead);
        MediaConsumptionStoppedDomainEvent stoppedEvent = Assert.IsType<MediaConsumptionStoppedDomainEvent>(Assert.Single(record.GetDomainEvents()));
        Assert.Equal(7, stoppedEvent.ConsecutivePagesRead);
    }

    [Fact]
    public void Stop_WhenConsecutivePagesReadIsZeroIsSuppliedForABook_ShouldResetTheCount()
    {
        // Arrange
        MediaPosition previousPosition = _mediaPositionFixture.Create(kind: MediaPositionKind.Location);
        MediaConsumptionRecord record = _mediaConsumptionRecordFixture.Create(
            mediaKind: MediaKind.Book,
            status: ConsumptionStatus.InProgress,
            consumedDuration: TimeSpan.FromMinutes(10),
            lastPosition: Optional<MediaPosition>.Some(previousPosition),
            consecutivePagesRead: 5);

        // Act
        Result<Updated> result = record.Stop(Optional<MediaPosition>.None(), 0.5, TimeSpan.FromMinutes(30), Optional<int>.Some(0));

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(0, record.ConsecutivePagesRead);
        MediaConsumptionStoppedDomainEvent stoppedEvent = Assert.IsType<MediaConsumptionStoppedDomainEvent>(Assert.Single(record.GetDomainEvents()));
        Assert.Equal(0, stoppedEvent.ConsecutivePagesRead);
    }

    [Theory]
    [InlineData(MediaKind.Movie)] // a duration based kind does not count pages
    [InlineData(MediaKind.Photo)] // a one-shot kind does not count pages
    public void Stop_WhenConsecutivePagesReadIsPositiveForANonPageBasedKind_ShouldReturnConsecutivePagesReadIsOnlySupportedForPageBasedMediaError(MediaKind mediaKind)
    {
        // Arrange
        MediaConsumptionRecord record = _mediaConsumptionRecordFixture.Create(
            mediaKind: mediaKind,
            status: ConsumptionStatus.InProgress,
            consumedDuration: TimeSpan.FromMinutes(10),
            consecutivePagesRead: 0);

        // Act
        Result<Updated> result = record.Stop(Optional<MediaPosition>.None(), 0.35, TimeSpan.FromMinutes(20), Optional<int>.Some(3));

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.MediaConsumption.ConsecutivePagesReadIsOnlySupportedForPageBasedMedia, result.FirstError);
        Assert.Equal(ConsumptionStatus.InProgress, record.Status);
        Assert.Equal(0, record.ConsecutivePagesRead);
        Assert.Empty(record.GetDomainEvents());
    }

    [Theory]
    [InlineData(ConsumptionStatus.Completed)] // an already completed session cannot stop
    [InlineData(ConsumptionStatus.Stopped)] // an already stopped session cannot stop again
    public void Stop_WhenStatusIsTerminal_ShouldReturnConsumptionIsAlreadyTerminalError(ConsumptionStatus status)
    {
        // Arrange
        MediaConsumptionRecord record = _mediaConsumptionRecordFixture.Create(
            mediaKind: MediaKind.Movie,
            status: status,
            consumedDuration: TimeSpan.FromMinutes(10));
        ConsumptionStatus originalStatus = record.Status;
        Optional<DateTime> originalEndedOnUtc = record.EndedOnUtc;
        DateTime originalLastActivityOnUtc = record.LastActivityOnUtc;
        Optional<DateTime> originalUpdatedOnUtc = record.UpdatedOnUtc;
        MediaPosition position = _mediaPositionFixture.Create(kind: MediaPositionKind.TimeOffset);

        // Act
        Result<Updated> result = record.Stop(Optional<MediaPosition>.Some(position), 0.35, TimeSpan.FromMinutes(18), Optional<int>.None());

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.MediaConsumption.ConsumptionIsAlreadyTerminal, result.FirstError);
        Assert.Equal(originalStatus, record.Status);
        Assert.Equal(originalEndedOnUtc, record.EndedOnUtc);
        Assert.Equal(originalLastActivityOnUtc, record.LastActivityOnUtc);
        Assert.Equal(originalUpdatedOnUtc, record.UpdatedOnUtc);
        Assert.Empty(record.GetDomainEvents());
    }

    [Theory]
    [InlineData(double.NaN)] // a value that is not a number
    [InlineData(double.PositiveInfinity)] // a value beyond every finite fraction
    [InlineData(double.NegativeInfinity)] // a value below every finite fraction
    [InlineData(-0.01)] // a value below the zero lower boundary
    [InlineData(1.01)] // a value above the one upper boundary
    public void Stop_WhenProgressFractionIsInvalid_ShouldReturnInvalidProgressFractionError(double lastProgressFraction)
    {
        // Arrange
        MediaConsumptionRecord record = _mediaConsumptionRecordFixture.Create(
            mediaKind: MediaKind.Movie,
            status: ConsumptionStatus.InProgress,
            consumedDuration: TimeSpan.FromMinutes(10));

        // Act
        Result<Updated> result = record.Stop(Optional<MediaPosition>.None(), lastProgressFraction, TimeSpan.FromMinutes(20), Optional<int>.None());

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.MediaConsumption.InvalidProgressFraction, result.FirstError);
    }

    [Fact]
    public void Stop_WhenConsumedDurationIsLowerThanCurrent_ShouldReturnConsumedDurationCannotDecreaseError()
    {
        // Arrange
        MediaConsumptionRecord record = _mediaConsumptionRecordFixture.Create(
            mediaKind: MediaKind.Movie,
            status: ConsumptionStatus.InProgress,
            consumedDuration: TimeSpan.FromMinutes(10));

        // Act
        Result<Updated> result = record.Stop(Optional<MediaPosition>.None(), 0.35, TimeSpan.FromMinutes(10).Subtract(TimeSpan.FromSeconds(1)), Optional<int>.None());

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.MediaConsumption.ConsumedDurationCannotDecrease, result.FirstError);
        Assert.Equal(TimeSpan.FromMinutes(10), record.ConsumedDuration);
    }

    [Fact]
    public void Stop_WhenConsumedDurationEqualsCurrent_ShouldStopTheRecord()
    {
        // Arrange
        MediaConsumptionRecord record = _mediaConsumptionRecordFixture.Create(
            mediaKind: MediaKind.Movie,
            status: ConsumptionStatus.InProgress,
            consumedDuration: TimeSpan.FromMinutes(10));

        // Act
        Result<Updated> result = record.Stop(Optional<MediaPosition>.None(), 0.35, TimeSpan.FromMinutes(10), Optional<int>.None());

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(ConsumptionStatus.Stopped, record.Status);
        Assert.Equal(TimeSpan.FromMinutes(10), record.ConsumedDuration);
        Assert.Single(record.GetDomainEvents());
    }

    [Theory]
    [InlineData(MediaKind.Movie, MediaPositionKind.Location)] // a movie resumes from a time offset, not from a location
    [InlineData(MediaKind.Book, MediaPositionKind.TimeOffset)] // a book resumes from a location, not from a time offset
    [InlineData(MediaKind.Photo, MediaPositionKind.Location)] // a photo cannot be resumed, so it accepts no position at all
    public void Stop_WhenPositionDoesNotMatchTheMediaKindRule_ShouldReturnInvalidMediaPositionError(MediaKind mediaKind, MediaPositionKind positionKind)
    {
        // Arrange
        MediaConsumptionRecord record = _mediaConsumptionRecordFixture.Create(
            mediaKind: mediaKind,
            status: ConsumptionStatus.InProgress,
            consumedDuration: TimeSpan.FromMinutes(10),
            includeLastPosition: false);
        MediaPosition mismatchedPosition = _mediaPositionFixture.Create(kind: positionKind);

        // Act
        Result<Updated> result = record.Stop(Optional<MediaPosition>.Some(mismatchedPosition), 0.35, TimeSpan.FromMinutes(20), Optional<int>.None());

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.MediaConsumption.InvalidMediaPosition, result.FirstError);
    }

    [Fact]
    public void Stop_WhenItFails_ShouldNotChangeStatusEndTimeOrRaiseEvents()
    {
        // Arrange
        MediaPosition previousPosition = _mediaPositionFixture.Create(kind: MediaPositionKind.TimeOffset);
        MediaConsumptionRecord record = _mediaConsumptionRecordFixture.Create(
            mediaKind: MediaKind.Movie,
            status: ConsumptionStatus.InProgress,
            consumedDuration: TimeSpan.FromMinutes(10),
            lastPosition: Optional<MediaPosition>.Some(previousPosition),
            lastProgressFraction: 0.2);
        DateTime originalLastActivityOnUtc = record.LastActivityOnUtc;
        Optional<DateTime> originalUpdatedOnUtc = record.UpdatedOnUtc;

        // Act
        Result<Updated> result = record.Stop(Optional<MediaPosition>.None(), 1.5, TimeSpan.FromMinutes(20), Optional<int>.None());

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.MediaConsumption.InvalidProgressFraction, result.FirstError);
        Assert.Equal(ConsumptionStatus.InProgress, record.Status);
        Assert.False(record.EndedOnUtc.HasValue);
        Assert.Equal(originalLastActivityOnUtc, record.LastActivityOnUtc);
        Assert.Equal(originalUpdatedOnUtc, record.UpdatedOnUtc);
        Assert.Equal(Optional<MediaPosition>.Some(previousPosition), record.LastPosition);
        Assert.Equal(0.2, record.LastProgressFraction);
        Assert.Equal(TimeSpan.FromMinutes(10), record.ConsumedDuration);
        Assert.Empty(record.GetDomainEvents());
    }
}
