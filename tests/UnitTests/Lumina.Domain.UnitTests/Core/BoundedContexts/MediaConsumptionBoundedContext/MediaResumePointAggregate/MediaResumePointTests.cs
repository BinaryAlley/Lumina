#region ========================================================================= USING =====================================================================================
using Lumina.Domain.Common.Errors;
using Lumina.Domain.Common.Primitives;
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

namespace Lumina.Domain.UnitTests.Core.BoundedContexts.MediaConsumptionBoundedContext.MediaResumePointAggregate;

/// <summary>
/// Contains unit tests for the <see cref="MediaResumePoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class MediaResumePointTests
{
    private readonly MediaResumePointFixture _mediaResumePointFixture = new();
    private readonly MediaResumePointIdFixture _mediaResumePointIdFixture = new();
    private readonly UserIdFixture _userIdFixture = new();
    private readonly LibraryIdFixture _libraryIdFixture = new();
    private readonly MediaItemIdFixture _mediaItemIdFixture = new();
    private readonly MediaPositionFixture _mediaPositionFixture = new();

    [Fact]
    public void Create_WhenCalledWithValidData_ShouldCreateResumePointWithAllPropertiesSet()
    {
        // Arrange
        UserId userId = _userIdFixture.Create();
        LibraryId libraryId = _libraryIdFixture.Create();
        MediaItemId mediaItemId = _mediaItemIdFixture.Create();
        MediaPosition lastPosition = _mediaPositionFixture.Create(kind: MediaPositionKind.TimeOffset);
        double lastProgressFraction = 0.25;
        DateTime lastConsumedOnUtc = DateTime.UtcNow.AddHours(-2);

        // Act
        Result<MediaResumePoint> result = MediaResumePoint.Create(
            userId,
            libraryId,
            mediaItemId,
            MediaKind.Movie,
            lastPosition,
            lastProgressFraction,
            lastConsumedOnUtc);

        // Assert
        Assert.False(result.IsFailure);
        MediaResumePoint resumePoint = result.Value;
        Assert.NotEqual(Guid.Empty, resumePoint.Id.Value);
        Assert.Equal(userId, resumePoint.UserId);
        Assert.Equal(libraryId, resumePoint.LibraryId);
        Assert.Equal(mediaItemId, resumePoint.MediaItemId);
        Assert.Equal(MediaKind.Movie, resumePoint.MediaKind);
        Assert.Equal(lastPosition, resumePoint.LastPosition);
        Assert.Equal(lastProgressFraction, resumePoint.LastProgressFraction);
        Assert.Equal(lastConsumedOnUtc, resumePoint.LastConsumedOnUtc);
        Assert.Equal(lastConsumedOnUtc, resumePoint.CreatedOnUtc);
        Assert.False(resumePoint.UpdatedOnUtc.HasValue);
        Assert.Empty(resumePoint.GetDomainEvents());
    }

    [Fact]
    public void Create_WhenCalledTwice_ShouldGenerateDistinctIds()
    {
        // Arrange
        MediaPosition firstPosition = _mediaPositionFixture.Create(kind: MediaPositionKind.TimeOffset);
        MediaPosition secondPosition = _mediaPositionFixture.Create(kind: MediaPositionKind.TimeOffset);

        // Act
        Result<MediaResumePoint> firstResult = MediaResumePoint.Create(
            _userIdFixture.Create(),
            _libraryIdFixture.Create(),
            _mediaItemIdFixture.Create(),
            MediaKind.Movie,
            firstPosition,
            0.5,
            DateTime.UtcNow.AddHours(-1));
        Result<MediaResumePoint> secondResult = MediaResumePoint.Create(
            _userIdFixture.Create(),
            _libraryIdFixture.Create(),
            _mediaItemIdFixture.Create(),
            MediaKind.Movie,
            secondPosition,
            0.5,
            DateTime.UtcNow.AddHours(-1));

        // Assert
        Assert.False(firstResult.IsFailure);
        Assert.False(secondResult.IsFailure);
        Assert.NotEqual(firstResult.Value.Id.Value, secondResult.Value.Id.Value);
    }

    [Theory]
    [InlineData(MediaKind.None)] // the default value, which is not a valid kind for a media item
    [InlineData((MediaKind)999)] // a value outside the declared enumeration members
    public void Create_WhenMediaKindIsUnsupported_ShouldReturnUnsupportedMediaKindError(MediaKind mediaKind)
    {
        // Arrange
        MediaPosition lastPosition = _mediaPositionFixture.Create(kind: MediaPositionKind.TimeOffset);

        // Act
        Result<MediaResumePoint> result = MediaResumePoint.Create(
            _userIdFixture.Create(),
            _libraryIdFixture.Create(),
            _mediaItemIdFixture.Create(),
            mediaKind,
            lastPosition,
            0.5,
            DateTime.UtcNow.AddHours(-1));

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.MediaConsumption.UnsupportedMediaKind, result.FirstError);
    }

    [Fact]
    public void Create_WhenMediaKindCannotResume_ShouldReturnMediaKindCannotResumeError()
    {
        // Arrange
        MediaPosition lastPosition = _mediaPositionFixture.Create(kind: MediaPositionKind.TimeOffset);

        // Act
        Result<MediaResumePoint> result = MediaResumePoint.Create(
            _userIdFixture.Create(),
            _libraryIdFixture.Create(),
            _mediaItemIdFixture.Create(),
            MediaKind.Photo,
            lastPosition,
            0.5,
            DateTime.UtcNow.AddHours(-1));

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.MediaConsumption.MediaKindCannotResume, result.FirstError);
    }

    [Fact]
    public void Create_WhenMediaKindCannotResumeAndTheOtherInputsAreInvalid_ShouldReturnMediaKindCannotResumeError()
    {
        // Arrange
        MediaPosition mismatchedPosition = _mediaPositionFixture.Create(kind: MediaPositionKind.Location);

        // Act
        Result<MediaResumePoint> result = MediaResumePoint.Create(
            _userIdFixture.Create(),
            _libraryIdFixture.Create(),
            _mediaItemIdFixture.Create(),
            MediaKind.Photo,
            mismatchedPosition,
            2.0,
            DateTime.UtcNow.AddHours(-1));

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.MediaConsumption.MediaKindCannotResume, result.FirstError);
    }

    [Theory]
    [InlineData(double.NaN)] // a value that is not a number
    [InlineData(double.PositiveInfinity)] // a value beyond every finite fraction
    [InlineData(double.NegativeInfinity)] // a value below every finite fraction
    [InlineData(-0.01)] // a value below the zero lower boundary
    [InlineData(1.01)] // a value above the one upper boundary
    public void Create_WhenProgressFractionIsInvalid_ShouldReturnInvalidProgressFractionError(double lastProgressFraction)
    {
        // Arrange
        MediaPosition lastPosition = _mediaPositionFixture.Create(kind: MediaPositionKind.TimeOffset);

        // Act
        Result<MediaResumePoint> result = MediaResumePoint.Create(
            _userIdFixture.Create(),
            _libraryIdFixture.Create(),
            _mediaItemIdFixture.Create(),
            MediaKind.Movie,
            lastPosition,
            lastProgressFraction,
            DateTime.UtcNow.AddHours(-1));

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.MediaConsumption.InvalidProgressFraction, result.FirstError);
    }

    [Theory]
    [InlineData(0.0)] // the lower boundary of the progress fraction
    [InlineData(1.0)] // the upper boundary of the progress fraction
    public void Create_WhenProgressFractionIsAtABoundary_ShouldCreateResumePoint(double lastProgressFraction)
    {
        // Arrange
        MediaPosition lastPosition = _mediaPositionFixture.Create(kind: MediaPositionKind.TimeOffset);

        // Act
        Result<MediaResumePoint> result = MediaResumePoint.Create(
            _userIdFixture.Create(),
            _libraryIdFixture.Create(),
            _mediaItemIdFixture.Create(),
            MediaKind.Movie,
            lastPosition,
            lastProgressFraction,
            DateTime.UtcNow.AddHours(-1));

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(lastProgressFraction, result.Value.LastProgressFraction);
    }

    [Theory]
    [InlineData(MediaKind.Movie, MediaPositionKind.Location)] // a movie resumes from a time offset, not from a location
    [InlineData(MediaKind.Book, MediaPositionKind.TimeOffset)] // a book resumes from a location, not from a time offset
    public void Create_WhenPositionDoesNotMatchTheMediaKindRule_ShouldReturnInvalidMediaPositionError(MediaKind mediaKind, MediaPositionKind positionKind)
    {
        // Arrange
        MediaPosition lastPosition = _mediaPositionFixture.Create(kind: positionKind);

        // Act
        Result<MediaResumePoint> result = MediaResumePoint.Create(
            _userIdFixture.Create(),
            _libraryIdFixture.Create(),
            _mediaItemIdFixture.Create(),
            mediaKind,
            lastPosition,
            0.5,
            DateTime.UtcNow.AddHours(-1));

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.MediaConsumption.InvalidMediaPosition, result.FirstError);
    }

    [Theory]
    [InlineData(MediaKind.Movie, MediaPositionKind.TimeOffset)] // a movie resumes from a time offset
    [InlineData(MediaKind.Book, MediaPositionKind.Location)] // a book resumes from a location
    public void Create_WhenPositionMatchesTheMediaKindRule_ShouldStoreThePosition(MediaKind mediaKind, MediaPositionKind positionKind)
    {
        // Arrange
        MediaPosition lastPosition = _mediaPositionFixture.Create(kind: positionKind);

        // Act
        Result<MediaResumePoint> result = MediaResumePoint.Create(
            _userIdFixture.Create(),
            _libraryIdFixture.Create(),
            _mediaItemIdFixture.Create(),
            mediaKind,
            lastPosition,
            0.5,
            DateTime.UtcNow.AddHours(-1));

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(lastPosition, result.Value.LastPosition);
    }

    [Fact]
    public void Reconstitute_WhenCalledWithValidData_ShouldPreserveAllPropertiesAndRaiseNoEvents()
    {
        // Arrange
        MediaResumePointId id = _mediaResumePointIdFixture.Create();
        UserId userId = _userIdFixture.Create();
        LibraryId libraryId = _libraryIdFixture.Create();
        MediaItemId mediaItemId = _mediaItemIdFixture.Create();
        MediaPosition lastPosition = _mediaPositionFixture.Create(kind: MediaPositionKind.TimeOffset);
        DateTime lastConsumedOnUtc = DateTime.UtcNow.AddHours(-5);
        DateTime createdOnUtc = DateTime.UtcNow.AddHours(-10);

        // Act
        Result<MediaResumePoint> result = MediaResumePoint.Reconstitute(
            id,
            userId,
            libraryId,
            mediaItemId,
            MediaKind.Movie,
            lastPosition,
            0.6,
            lastConsumedOnUtc,
            createdOnUtc,
            Optional<DateTime>.None());

        // Assert
        Assert.False(result.IsFailure);
        MediaResumePoint resumePoint = result.Value;
        Assert.Equal(id, resumePoint.Id);
        Assert.Equal(userId, resumePoint.UserId);
        Assert.Equal(libraryId, resumePoint.LibraryId);
        Assert.Equal(mediaItemId, resumePoint.MediaItemId);
        Assert.Equal(MediaKind.Movie, resumePoint.MediaKind);
        Assert.Equal(lastPosition, resumePoint.LastPosition);
        Assert.Equal(0.6, resumePoint.LastProgressFraction);
        Assert.Equal(lastConsumedOnUtc, resumePoint.LastConsumedOnUtc);
        Assert.Equal(createdOnUtc, resumePoint.CreatedOnUtc);
        Assert.False(resumePoint.UpdatedOnUtc.HasValue);
        Assert.Empty(resumePoint.GetDomainEvents());
    }

    [Fact]
    public void Reconstitute_WhenUpdatedOnUtcHasAValue_ShouldPreserveIt()
    {
        // Arrange
        MediaResumePointId id = _mediaResumePointIdFixture.Create();
        MediaPosition lastPosition = _mediaPositionFixture.Create(kind: MediaPositionKind.TimeOffset);
        DateTime lastConsumedOnUtc = DateTime.UtcNow.AddHours(-4);
        DateTime createdOnUtc = DateTime.UtcNow.AddHours(-8);
        DateTime updatedOnUtc = DateTime.UtcNow.AddHours(-2);

        // Act
        Result<MediaResumePoint> result = MediaResumePoint.Reconstitute(
            id,
            _userIdFixture.Create(),
            _libraryIdFixture.Create(),
            _mediaItemIdFixture.Create(),
            MediaKind.Movie,
            lastPosition,
            0.75,
            lastConsumedOnUtc,
            createdOnUtc,
            Optional<DateTime>.Some(updatedOnUtc));

        // Assert
        Assert.False(result.IsFailure);
        MediaResumePoint resumePoint = result.Value;
        Assert.Equal(lastConsumedOnUtc, resumePoint.LastConsumedOnUtc);
        Assert.Equal(createdOnUtc, resumePoint.CreatedOnUtc);
        Assert.Equal(Optional<DateTime>.Some(updatedOnUtc), resumePoint.UpdatedOnUtc);
        Assert.Empty(resumePoint.GetDomainEvents());
    }

    [Theory]
    [InlineData(MediaKind.None)] // the default value, which is not a valid kind for a media item
    [InlineData((MediaKind)999)] // a value outside the declared enumeration members
    public void Reconstitute_WhenMediaKindIsUnsupported_ShouldReturnUnsupportedMediaKindError(MediaKind mediaKind)
    {
        // Arrange
        MediaPosition lastPosition = _mediaPositionFixture.Create(kind: MediaPositionKind.TimeOffset);

        // Act
        Result<MediaResumePoint> result = MediaResumePoint.Reconstitute(
            _mediaResumePointIdFixture.Create(),
            _userIdFixture.Create(),
            _libraryIdFixture.Create(),
            _mediaItemIdFixture.Create(),
            mediaKind,
            lastPosition,
            0.5,
            DateTime.UtcNow.AddHours(-4),
            DateTime.UtcNow.AddHours(-8),
            Optional<DateTime>.None());

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.MediaConsumption.UnsupportedMediaKind, result.FirstError);
    }

    [Fact]
    public void Reconstitute_WhenMediaKindCannotResume_ShouldReturnMediaKindCannotResumeError()
    {
        // Arrange
        MediaPosition lastPosition = _mediaPositionFixture.Create(kind: MediaPositionKind.TimeOffset);

        // Act
        Result<MediaResumePoint> result = MediaResumePoint.Reconstitute(
            _mediaResumePointIdFixture.Create(),
            _userIdFixture.Create(),
            _libraryIdFixture.Create(),
            _mediaItemIdFixture.Create(),
            MediaKind.Photo,
            lastPosition,
            0.5,
            DateTime.UtcNow.AddHours(-4),
            DateTime.UtcNow.AddHours(-8),
            Optional<DateTime>.None());

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.MediaConsumption.MediaKindCannotResume, result.FirstError);
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
        MediaPosition lastPosition = _mediaPositionFixture.Create(kind: MediaPositionKind.TimeOffset);

        // Act
        Result<MediaResumePoint> result = MediaResumePoint.Reconstitute(
            _mediaResumePointIdFixture.Create(),
            _userIdFixture.Create(),
            _libraryIdFixture.Create(),
            _mediaItemIdFixture.Create(),
            MediaKind.Movie,
            lastPosition,
            lastProgressFraction,
            DateTime.UtcNow.AddHours(-4),
            DateTime.UtcNow.AddHours(-8),
            Optional<DateTime>.None());

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.MediaConsumption.InvalidProgressFraction, result.FirstError);
    }

    [Theory]
    [InlineData(0.0)] // the lower boundary of the progress fraction
    [InlineData(1.0)] // the upper boundary of the progress fraction
    public void Reconstitute_WhenProgressFractionIsAtABoundary_ShouldReconstituteResumePoint(double lastProgressFraction)
    {
        // Arrange
        MediaPosition lastPosition = _mediaPositionFixture.Create(kind: MediaPositionKind.TimeOffset);

        // Act
        Result<MediaResumePoint> result = MediaResumePoint.Reconstitute(
            _mediaResumePointIdFixture.Create(),
            _userIdFixture.Create(),
            _libraryIdFixture.Create(),
            _mediaItemIdFixture.Create(),
            MediaKind.Movie,
            lastPosition,
            lastProgressFraction,
            DateTime.UtcNow.AddHours(-4),
            DateTime.UtcNow.AddHours(-8),
            Optional<DateTime>.None());

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(lastProgressFraction, result.Value.LastProgressFraction);
    }

    [Theory]
    [InlineData(MediaKind.Movie, MediaPositionKind.Location)] // a movie resumes from a time offset, not from a location
    [InlineData(MediaKind.Book, MediaPositionKind.TimeOffset)] // a book resumes from a location, not from a time offset
    public void Reconstitute_WhenPositionDoesNotMatchTheMediaKindRule_ShouldReturnInvalidMediaPositionError(MediaKind mediaKind, MediaPositionKind positionKind)
    {
        // Arrange
        MediaPosition lastPosition = _mediaPositionFixture.Create(kind: positionKind);

        // Act
        Result<MediaResumePoint> result = MediaResumePoint.Reconstitute(
            _mediaResumePointIdFixture.Create(),
            _userIdFixture.Create(),
            _libraryIdFixture.Create(),
            _mediaItemIdFixture.Create(),
            mediaKind,
            lastPosition,
            0.5,
            DateTime.UtcNow.AddHours(-4),
            DateTime.UtcNow.AddHours(-8),
            Optional<DateTime>.None());

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.MediaConsumption.InvalidMediaPosition, result.FirstError);
    }

    [Theory]
    [InlineData(MediaKind.Movie, MediaPositionKind.TimeOffset)] // a movie resumes from a time offset
    [InlineData(MediaKind.Book, MediaPositionKind.Location)] // a book resumes from a location
    public void Reconstitute_WhenPositionMatchesTheMediaKindRule_ShouldStoreThePosition(MediaKind mediaKind, MediaPositionKind positionKind)
    {
        // Arrange
        MediaPosition lastPosition = _mediaPositionFixture.Create(kind: positionKind);

        // Act
        Result<MediaResumePoint> result = MediaResumePoint.Reconstitute(
            _mediaResumePointIdFixture.Create(),
            _userIdFixture.Create(),
            _libraryIdFixture.Create(),
            _mediaItemIdFixture.Create(),
            mediaKind,
            lastPosition,
            0.5,
            DateTime.UtcNow.AddHours(-4),
            DateTime.UtcNow.AddHours(-8),
            Optional<DateTime>.None());

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(lastPosition, result.Value.LastPosition);
    }

    [Theory]
    [InlineData(MediaKind.Movie, MediaPositionKind.TimeOffset)] // a movie resumes from a time offset
    [InlineData(MediaKind.Book, MediaPositionKind.Location)] // a book resumes from a location
    public void Advance_WhenCalledWithAValidPosition_ShouldReplaceTheResumePointState(MediaKind mediaKind, MediaPositionKind positionKind)
    {
        // Arrange
        MediaPosition previousPosition = _mediaPositionFixture.Create(kind: positionKind, timeOffset: TimeSpan.FromMinutes(1), location: "chapter-1");
        DateTime previousLastConsumedOnUtc = DateTime.UtcNow.AddHours(-3);
        MediaResumePoint resumePoint = _mediaResumePointFixture.Create(
            mediaKind: mediaKind,
            lastPosition: previousPosition,
            lastProgressFraction: 0.2,
            lastConsumedOnUtc: previousLastConsumedOnUtc);
        MediaPosition newPosition = _mediaPositionFixture.Create(kind: positionKind, timeOffset: TimeSpan.FromMinutes(42), location: "chapter-9");
        DateTime newLastConsumedOnUtc = DateTime.UtcNow.AddMinutes(-5);

        // Act
        Result<Updated> result = resumePoint.Advance(newPosition, 0.85, newLastConsumedOnUtc);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(Result.Updated, result.Value);
        Assert.Equal(newPosition, resumePoint.LastPosition);
        Assert.Equal(0.85, resumePoint.LastProgressFraction);
        Assert.Equal(newLastConsumedOnUtc, resumePoint.LastConsumedOnUtc);
        Assert.Equal(Optional<DateTime>.Some(newLastConsumedOnUtc), resumePoint.UpdatedOnUtc);
        Assert.NotEqual(previousPosition, resumePoint.LastPosition);
        Assert.NotEqual(previousLastConsumedOnUtc, resumePoint.LastConsumedOnUtc);
        Assert.Equal(previousLastConsumedOnUtc, resumePoint.CreatedOnUtc);
        Assert.Empty(resumePoint.GetDomainEvents());
    }

    [Theory]
    [InlineData(0.0)] // the lower boundary of the progress fraction
    [InlineData(1.0)] // the upper boundary of the progress fraction
    public void Advance_WhenProgressFractionIsAtABoundary_ShouldAdvanceResumePoint(double progressFraction)
    {
        // Arrange
        MediaPosition previousPosition = _mediaPositionFixture.Create(kind: MediaPositionKind.TimeOffset);
        MediaResumePoint resumePoint = _mediaResumePointFixture.Create(
            mediaKind: MediaKind.Movie,
            lastPosition: previousPosition,
            lastProgressFraction: 0.5);
        MediaPosition newPosition = _mediaPositionFixture.Create(kind: MediaPositionKind.TimeOffset);
        DateTime newLastConsumedOnUtc = DateTime.UtcNow.AddMinutes(-5);

        // Act
        Result<Updated> result = resumePoint.Advance(newPosition, progressFraction, newLastConsumedOnUtc);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(Result.Updated, result.Value);
        Assert.Equal(progressFraction, resumePoint.LastProgressFraction);
        Assert.Equal(Optional<DateTime>.Some(newLastConsumedOnUtc), resumePoint.UpdatedOnUtc);
    }

    [Theory]
    [InlineData(double.NaN)] // a value that is not a number
    [InlineData(double.PositiveInfinity)] // a value beyond every finite fraction
    [InlineData(double.NegativeInfinity)] // a value below every finite fraction
    [InlineData(-0.01)] // a value below the zero lower boundary
    [InlineData(1.01)] // a value above the one upper boundary
    public void Advance_WhenProgressFractionIsInvalid_ShouldReturnInvalidProgressFractionErrorAndLeaveTheResumePointUnchanged(double progressFraction)
    {
        // Arrange
        MediaPosition previousPosition = _mediaPositionFixture.Create(kind: MediaPositionKind.TimeOffset);
        DateTime previousLastConsumedOnUtc = DateTime.UtcNow.AddHours(-3);
        MediaResumePoint resumePoint = _mediaResumePointFixture.Create(
            mediaKind: MediaKind.Movie,
            lastPosition: previousPosition,
            lastProgressFraction: 0.2,
            lastConsumedOnUtc: previousLastConsumedOnUtc);
        MediaPosition originalLastPosition = resumePoint.LastPosition;
        double originalLastProgressFraction = resumePoint.LastProgressFraction;
        DateTime originalLastConsumedOnUtc = resumePoint.LastConsumedOnUtc;
        Optional<DateTime> originalUpdatedOnUtc = resumePoint.UpdatedOnUtc;
        DateTime originalCreatedOnUtc = resumePoint.CreatedOnUtc;
        MediaPosition newPosition = _mediaPositionFixture.Create(kind: MediaPositionKind.TimeOffset);

        // Act
        Result<Updated> result = resumePoint.Advance(newPosition, progressFraction, DateTime.UtcNow.AddMinutes(-5));

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.MediaConsumption.InvalidProgressFraction, result.FirstError);
        Assert.Equal(originalLastPosition, resumePoint.LastPosition);
        Assert.Equal(originalLastProgressFraction, resumePoint.LastProgressFraction);
        Assert.Equal(originalLastConsumedOnUtc, resumePoint.LastConsumedOnUtc);
        Assert.Equal(originalUpdatedOnUtc, resumePoint.UpdatedOnUtc);
        Assert.Equal(originalCreatedOnUtc, resumePoint.CreatedOnUtc);
        Assert.Empty(resumePoint.GetDomainEvents());
    }

    [Theory]
    [InlineData(MediaKind.Movie, MediaPositionKind.Location)] // a movie resumes from a time offset, not from a location
    [InlineData(MediaKind.Book, MediaPositionKind.TimeOffset)] // a book resumes from a location, not from a time offset
    public void Advance_WhenPositionDoesNotMatchTheMediaKindRule_ShouldReturnInvalidMediaPositionErrorAndLeaveTheResumePointUnchanged(MediaKind mediaKind, MediaPositionKind positionKind)
    {
        // Arrange
        MediaResumePoint resumePoint = _mediaResumePointFixture.Create(mediaKind: mediaKind, lastProgressFraction: 0.2);
        MediaPosition originalLastPosition = resumePoint.LastPosition;
        double originalLastProgressFraction = resumePoint.LastProgressFraction;
        DateTime originalLastConsumedOnUtc = resumePoint.LastConsumedOnUtc;
        Optional<DateTime> originalUpdatedOnUtc = resumePoint.UpdatedOnUtc;
        DateTime originalCreatedOnUtc = resumePoint.CreatedOnUtc;
        MediaPosition mismatchedPosition = _mediaPositionFixture.Create(kind: positionKind);

        // Act
        Result<Updated> result = resumePoint.Advance(mismatchedPosition, 0.9, DateTime.UtcNow.AddMinutes(-5));

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.MediaConsumption.InvalidMediaPosition, result.FirstError);
        Assert.Equal(originalLastPosition, resumePoint.LastPosition);
        Assert.Equal(originalLastProgressFraction, resumePoint.LastProgressFraction);
        Assert.Equal(originalLastConsumedOnUtc, resumePoint.LastConsumedOnUtc);
        Assert.Equal(originalUpdatedOnUtc, resumePoint.UpdatedOnUtc);
        Assert.Equal(originalCreatedOnUtc, resumePoint.CreatedOnUtc);
        Assert.Empty(resumePoint.GetDomainEvents());
    }

    [Fact]
    public void Advance_WhenMovingBackwards_ShouldReplaceTheStateWithTheEarlierValues()
    {
        // Arrange
        MediaPosition laterPosition = _mediaPositionFixture.Create(kind: MediaPositionKind.TimeOffset, timeOffset: TimeSpan.FromHours(2));
        MediaResumePoint resumePoint = _mediaResumePointFixture.Create(
            mediaKind: MediaKind.Movie,
            lastPosition: laterPosition,
            lastProgressFraction: 0.8,
            lastConsumedOnUtc: DateTime.UtcNow.AddHours(-3));
        MediaPosition earlierPosition = _mediaPositionFixture.Create(kind: MediaPositionKind.TimeOffset, timeOffset: TimeSpan.FromMinutes(5));
        DateTime newLastConsumedOnUtc = DateTime.UtcNow.AddMinutes(-5);

        // Act
        Result<Updated> result = resumePoint.Advance(earlierPosition, 0.1, newLastConsumedOnUtc);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(Result.Updated, result.Value);
        Assert.Equal(earlierPosition, resumePoint.LastPosition);
        Assert.Equal(0.1, resumePoint.LastProgressFraction);
        Assert.Equal(newLastConsumedOnUtc, resumePoint.LastConsumedOnUtc);
        Assert.Equal(Optional<DateTime>.Some(newLastConsumedOnUtc), resumePoint.UpdatedOnUtc);
        Assert.NotEqual(laterPosition, resumePoint.LastPosition);
        Assert.Empty(resumePoint.GetDomainEvents());
    }

    [Fact]
    public void Advance_WhenUpdatedOnUtcWasAlreadySet_ShouldOverwriteItWithTheNewConsumedTime()
    {
        // Arrange
        DateTime previousUpdatedOnUtc = DateTime.UtcNow.AddHours(-6);
        MediaResumePoint resumePoint = _mediaResumePointFixture.Create(
            mediaKind: MediaKind.Movie,
            lastProgressFraction: 0.2,
            lastConsumedOnUtc: DateTime.UtcNow.AddHours(-7),
            updatedOnUtc: Optional<DateTime>.Some(previousUpdatedOnUtc));
        MediaPosition newPosition = _mediaPositionFixture.Create(kind: MediaPositionKind.TimeOffset);
        DateTime newLastConsumedOnUtc = DateTime.UtcNow.AddMinutes(-5);
        DateTime originalCreatedOnUtc = resumePoint.CreatedOnUtc;

        // Act
        Result<Updated> result = resumePoint.Advance(newPosition, 0.6, newLastConsumedOnUtc);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(Optional<DateTime>.Some(newLastConsumedOnUtc), resumePoint.UpdatedOnUtc);
        Assert.NotEqual(Optional<DateTime>.Some(previousUpdatedOnUtc), resumePoint.UpdatedOnUtc);
        Assert.Equal(originalCreatedOnUtc, resumePoint.CreatedOnUtc);
        Assert.Empty(resumePoint.GetDomainEvents());
    }
}
