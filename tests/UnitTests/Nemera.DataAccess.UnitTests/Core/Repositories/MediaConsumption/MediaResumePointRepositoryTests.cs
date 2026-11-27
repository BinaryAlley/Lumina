#region ========================================================================= USING =====================================================================================
using EntityFrameworkCore.Testing.NSubstitute;
using Lumina.Application.Common.DataAccess.Entities.MediaConsumption;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.MediaConsumption;
using Lumina.DataAccess.Core.Repositories.MediaConsumption;
using Lumina.DataAccess.Core.UoW;
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.SharedKernel.Common.Enums.MediaConsumption;
using Lumina.Domain.SharedKernel.Common.Enums.MediaLibrary;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.DataAccess.UnitTests.Core.Repositories.MediaConsumption;

/// <summary>
/// Contains unit tests for the <see cref="MediaResumePointRepository"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class MediaResumePointRepositoryTests
{
    private readonly LuminaDbContext _mockContext;
    private readonly MediaResumePointRepository _sut;
    private readonly MediaResumePointEntityFixture _mediaResumePointEntityFixture = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="MediaResumePointRepositoryTests"/> class.
    /// </summary>
    public MediaResumePointRepositoryTests()
    {
        _mockContext = Create.MockedDbContextFor<LuminaDbContext>();
        _sut = new MediaResumePointRepository(_mockContext);
    }

    [Fact]
    public async Task GetForUserAndMediaItemAsync_WhenResumePointMatchesUserKindAndItem_ShouldReturnTheResumePoint()
    {
        // Arrange
        Guid userId = Guid.NewGuid();
        Guid mediaItemId = Guid.NewGuid();
        MediaResumePointEntity resumePoint = _mediaResumePointEntityFixture.Create(
            userId: userId,
            mediaKind: MediaKind.Movie,
            mediaItemId: mediaItemId);
        _mockContext.MediaResumePoints.Add(resumePoint);
        await _mockContext.SaveChangesAsync();

        // Act
        Result<MediaResumePointEntity?> result = await _sut.GetForUserAndMediaItemAsync(userId, MediaKind.Movie, mediaItemId, cancellationToken: CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.NotNull(result.Value);
        Assert.Equal(resumePoint.Id, result.Value!.Id);
        Assert.Equal(userId, result.Value.UserId);
        Assert.Equal(MediaKind.Movie, result.Value.MediaKind);
        Assert.Equal(mediaItemId, result.Value.MediaItemId);
        Assert.Equal(resumePoint.LibraryId, result.Value.LibraryId);
    }

    [Fact]
    public async Task GetForUserAndMediaItemAsync_WhenTheResumePointBelongsToAnotherUser_ShouldReturnNull()
    {
        // Arrange
        Guid mediaItemId = Guid.NewGuid();
        MediaResumePointEntity resumePoint = _mediaResumePointEntityFixture.Create(
            userId: Guid.NewGuid(),
            mediaKind: MediaKind.Movie,
            mediaItemId: mediaItemId);
        _mockContext.MediaResumePoints.Add(resumePoint);
        await _mockContext.SaveChangesAsync();

        // Act
        Result<MediaResumePointEntity?> result = await _sut.GetForUserAndMediaItemAsync(Guid.NewGuid(), MediaKind.Movie, mediaItemId, cancellationToken: CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Null(result.Value);
    }

    [Fact]
    public async Task GetForUserAndMediaItemAsync_WhenTheResumePointIsForAnotherMediaKind_ShouldReturnNull()
    {
        // Arrange
        Guid userId = Guid.NewGuid();
        Guid mediaItemId = Guid.NewGuid();
        MediaResumePointEntity resumePoint = _mediaResumePointEntityFixture.Create(
            userId: userId,
            mediaKind: MediaKind.Book,
            mediaItemId: mediaItemId);
        _mockContext.MediaResumePoints.Add(resumePoint);
        await _mockContext.SaveChangesAsync();

        // Act
        Result<MediaResumePointEntity?> result = await _sut.GetForUserAndMediaItemAsync(userId, MediaKind.Movie, mediaItemId, cancellationToken: CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Null(result.Value);
    }

    [Fact]
    public async Task GetForUserAndMediaItemAsync_WhenTheResumePointIsForAnotherMediaItem_ShouldReturnNull()
    {
        // Arrange
        Guid userId = Guid.NewGuid();
        MediaResumePointEntity resumePoint = _mediaResumePointEntityFixture.Create(
            userId: userId,
            mediaKind: MediaKind.Movie,
            mediaItemId: Guid.NewGuid());
        _mockContext.MediaResumePoints.Add(resumePoint);
        await _mockContext.SaveChangesAsync();

        // Act
        Result<MediaResumePointEntity?> result = await _sut.GetForUserAndMediaItemAsync(userId, MediaKind.Movie, Guid.NewGuid(), cancellationToken: CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Null(result.Value);
    }

    [Fact]
    public async Task GetForUserAndMediaItemAsync_WhenEntitiesShouldNotBeTracked_ShouldReturnTheResumePointAndLeaveTheTrackedRowUnchanged()
    {
        // Arrange
        Guid userId = Guid.NewGuid();
        Guid mediaItemId = Guid.NewGuid();
        MediaResumePointEntity resumePoint = _mediaResumePointEntityFixture.Create(
            userId: userId,
            mediaKind: MediaKind.Movie,
            mediaItemId: mediaItemId);
        _mockContext.MediaResumePoints.Add(resumePoint);
        await _mockContext.SaveChangesAsync();

        // Act
        Result<MediaResumePointEntity?> result = await _sut.GetForUserAndMediaItemAsync(userId, MediaKind.Movie, mediaItemId, shouldTrackEntities: false, cancellationToken: CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.NotNull(result.Value);
        Assert.Equal(resumePoint.Id, result.Value!.Id);
        Assert.Equal(resumePoint.UserId, result.Value.UserId);
        // The in-memory medium behind the mocked context does not materialize a distinct instance for a no-tracking query, so the detached
        // identity of the returned resume point is asserted against the real SQLite provider in the integration tests.
        Assert.Equal(EntityState.Unchanged, _mockContext.Entry(resumePoint).State);
    }

    [Fact]
    public async Task GetForUserAndMediaItemAsync_WhenEntitiesShouldBeTracked_ShouldReturnTheTrackedInstance()
    {
        // Arrange
        Guid userId = Guid.NewGuid();
        Guid mediaItemId = Guid.NewGuid();
        MediaResumePointEntity resumePoint = _mediaResumePointEntityFixture.Create(
            userId: userId,
            mediaKind: MediaKind.Movie,
            mediaItemId: mediaItemId);
        _mockContext.MediaResumePoints.Add(resumePoint);
        await _mockContext.SaveChangesAsync();

        // Act
        Result<MediaResumePointEntity?> result = await _sut.GetForUserAndMediaItemAsync(userId, MediaKind.Movie, mediaItemId, cancellationToken: CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.NotNull(result.Value);
        Assert.Same(resumePoint, result.Value);
        Assert.Equal(EntityState.Unchanged, _mockContext.Entry(result.Value!).State);
    }

    [Fact]
    public async Task UpsertForUserAsync_WhenResumePointDoesNotExist_ShouldAddItWithTheAuthoritativeUserId()
    {
        // Arrange
        Guid userId = Guid.NewGuid();
        MediaResumePointEntity resumePoint = _mediaResumePointEntityFixture.Create(
            userId: Guid.NewGuid(),
            mediaKind: MediaKind.Movie,
            mediaItemId: Guid.NewGuid(),
            libraryId: Guid.NewGuid(),
            lastPositionKind: MediaPositionKind.TimeOffset,
            lastPositionTimeOffsetMilliseconds: 120_000,
            lastProgressFraction: 0.25,
            version: 2);

        // Act
        Result<Updated> result = await _sut.UpsertForUserAsync(resumePoint, userId, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(Result.Updated, result.Value);

        EntityEntry<MediaResumePointEntity>? addedResumePoint = _mockContext.ChangeTracker.Entries<MediaResumePointEntity>()
            .FirstOrDefault(entry => entry.State == EntityState.Added && entry.Entity.Id == resumePoint.Id);
        Assert.NotNull(addedResumePoint);
        Assert.Same(resumePoint, addedResumePoint.Entity);
        // The owner argument is authoritative for a new row, not the owner carried by the incoming entity.
        Assert.Equal(userId, addedResumePoint.Entity.UserId);
    }

    [Fact]
    public async Task UpsertForUserAsync_WhenTheBusinessKeyExists_ShouldUpdateTheStoredRowAndIncrementTheVersion()
    {
        // Arrange
        Guid resumePointId = Guid.NewGuid();
        Guid userId = Guid.NewGuid();
        Guid mediaItemId = Guid.NewGuid();
        DateTime lastConsumedOnUtc = new(2025, 1, 1, 10, 0, 0, DateTimeKind.Utc);
        DateTime createdOnUtc = new(2024, 12, 31, 9, 0, 0, DateTimeKind.Utc);
        Guid createdBy = Guid.NewGuid();
        Guid incomingLibraryId = Guid.NewGuid();
        MediaResumePointEntity storedResumePoint = _mediaResumePointEntityFixture.Create(
            id: resumePointId,
            userId: userId,
            libraryId: Guid.NewGuid(),
            mediaKind: MediaKind.Movie,
            mediaItemId: mediaItemId,
            lastPositionKind: MediaPositionKind.TimeOffset,
            lastPositionTimeOffsetMilliseconds: 60_000,
            includeLastPositionLocation: false,
            lastProgressFraction: 0.25,
            lastConsumedOnUtc: lastConsumedOnUtc,
            version: 3,
            createdOnUtc: createdOnUtc,
            createdBy: createdBy);
        _mockContext.MediaResumePoints.Add(storedResumePoint);
        await _mockContext.SaveChangesAsync();
        // The tracker is cleared so that the business key is located through the persistence medium instead of the local cache.
        _mockContext.ChangeTracker.Clear();
        MediaResumePointEntity incoming = _mediaResumePointEntityFixture.Create(
            id: Guid.NewGuid(),
            userId: Guid.NewGuid(),
            libraryId: incomingLibraryId,
            mediaKind: MediaKind.Movie,
            mediaItemId: mediaItemId,
            lastPositionKind: MediaPositionKind.Location,
            includeLastPositionTimeOffsetMilliseconds: false,
            lastPositionLocation: "chapter-4",
            lastProgressFraction: 0.75,
            lastConsumedOnUtc: new(2025, 1, 2, 8, 0, 0, DateTimeKind.Utc),
            version: 3);

        // Act
        Result<Updated> result = await _sut.UpsertForUserAsync(incoming, userId, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(Result.Updated, result.Value);

        EntityEntry<MediaResumePointEntity> storedEntry = Assert.Single(_mockContext.ChangeTracker.Entries<MediaResumePointEntity>());
        Assert.Equal(EntityState.Modified, storedEntry.State);
        MediaResumePointEntity updatedResumePoint = storedEntry.Entity;
        // The business key and the creation audit are never reassigned by an upsert.
        Assert.Equal(resumePointId, updatedResumePoint.Id);
        Assert.Equal(userId, updatedResumePoint.UserId);
        Assert.Equal(MediaKind.Movie, updatedResumePoint.MediaKind);
        Assert.Equal(mediaItemId, updatedResumePoint.MediaItemId);
        Assert.Equal(createdOnUtc, updatedResumePoint.CreatedOnUtc);
        Assert.Equal(createdBy, updatedResumePoint.CreatedBy);
        // The editable values are copied from the incoming resume point.
        Assert.Equal(MediaPositionKind.Location, updatedResumePoint.LastPositionKind);
        Assert.Null(updatedResumePoint.LastPositionTimeOffsetMilliseconds);
        Assert.Equal("chapter-4", updatedResumePoint.LastPositionLocation);
        Assert.Equal(0.75, updatedResumePoint.LastProgressFraction);
        Assert.Equal(new DateTime(2025, 1, 2, 8, 0, 0, DateTimeKind.Utc), updatedResumePoint.LastConsumedOnUtc);
        Assert.Equal(incomingLibraryId, updatedResumePoint.LibraryId);
        // The version is incremented by exactly one for a single save.
        Assert.Equal(4L, updatedResumePoint.Version);
    }

    [Fact]
    public async Task UpsertForUserAsync_WhenNoEditableValueChanged_ShouldReturnUpdatedWithoutBumpingTheVersion()
    {
        // Arrange
        Guid userId = Guid.NewGuid();
        Guid mediaItemId = Guid.NewGuid();
        MediaResumePointEntity storedResumePoint = _mediaResumePointEntityFixture.Create(
            userId: userId,
            libraryId: Guid.NewGuid(),
            mediaKind: MediaKind.Movie,
            mediaItemId: mediaItemId,
            lastPositionKind: MediaPositionKind.TimeOffset,
            lastPositionTimeOffsetMilliseconds: 90_000,
            includeLastPositionLocation: false,
            lastProgressFraction: 0.4,
            lastConsumedOnUtc: new(2025, 1, 1, 10, 0, 0, DateTimeKind.Utc),
            version: 3);
        _mockContext.MediaResumePoints.Add(storedResumePoint);
        await _mockContext.SaveChangesAsync();
        // A separate instance carrying exactly the stored editable values is the shape of a save that changes nothing.
        MediaResumePointEntity incoming = _mediaResumePointEntityFixture.Create(
            id: storedResumePoint.Id,
            userId: userId,
            libraryId: storedResumePoint.LibraryId,
            mediaKind: MediaKind.Movie,
            mediaItemId: mediaItemId,
            lastPositionKind: MediaPositionKind.TimeOffset,
            lastPositionTimeOffsetMilliseconds: 90_000,
            includeLastPositionLocation: false,
            lastProgressFraction: 0.4,
            lastConsumedOnUtc: new(2025, 1, 1, 10, 0, 0, DateTimeKind.Utc),
            version: 3);

        // Act
        Result<Updated> result = await _sut.UpsertForUserAsync(incoming, userId, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(Result.Updated, result.Value);

        EntityEntry<MediaResumePointEntity> storedEntry = _mockContext.Entry(storedResumePoint);
        Assert.Equal(EntityState.Unchanged, storedEntry.State);
        Assert.Equal(3L, storedEntry.Entity.Version);
        Assert.DoesNotContain(_mockContext.ChangeTracker.Entries<MediaResumePointEntity>(), entry => entry.State == EntityState.Modified);
    }

    [Fact]
    public async Task UpsertForUserAsync_WhenAnotherUserHasTheSameMediaItem_ShouldInsertASecondRow()
    {
        // Arrange
        Guid mediaItemId = Guid.NewGuid();
        MediaResumePointEntity otherUserResumePoint = _mediaResumePointEntityFixture.Create(
            userId: Guid.NewGuid(),
            mediaKind: MediaKind.Movie,
            mediaItemId: mediaItemId,
            lastProgressFraction: 0.3);
        _mockContext.MediaResumePoints.Add(otherUserResumePoint);
        await _mockContext.SaveChangesAsync();
        Guid userId = Guid.NewGuid();
        MediaResumePointEntity incoming = _mediaResumePointEntityFixture.Create(
            userId: userId,
            mediaKind: MediaKind.Movie,
            mediaItemId: mediaItemId,
            lastProgressFraction: 0.6);

        // Act
        Result<Updated> result = await _sut.UpsertForUserAsync(incoming, userId, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(Result.Updated, result.Value);
        // The business key includes the user, so another user owning the same item does not turn the insert into an update.
        Assert.Equal(2, _mockContext.ChangeTracker.Entries<MediaResumePointEntity>().Count());
        Assert.Contains(_mockContext.ChangeTracker.Entries<MediaResumePointEntity>(), entry => entry.State == EntityState.Added && entry.Entity.UserId == userId);
        Assert.Equal(EntityState.Unchanged, _mockContext.Entry(otherUserResumePoint).State);
        Assert.Equal(0.3, otherUserResumePoint.LastProgressFraction);
    }

    [Fact]
    public async Task UpsertForUserAsync_WhenTheResumePointIsAlreadyTracked_ShouldUpdateThatInstanceInsteadOfAddingADuplicate()
    {
        // Arrange
        Guid userId = Guid.NewGuid();
        Guid mediaItemId = Guid.NewGuid();
        MediaResumePointEntity trackedResumePoint = _mediaResumePointEntityFixture.Create(
            userId: userId,
            mediaKind: MediaKind.Movie,
            mediaItemId: mediaItemId,
            lastProgressFraction: 0.3,
            version: 5);
        _mockContext.MediaResumePoints.Add(trackedResumePoint);
        await _mockContext.SaveChangesAsync();
        MediaResumePointEntity incoming = _mediaResumePointEntityFixture.Create(
            id: Guid.NewGuid(),
            userId: userId,
            mediaKind: MediaKind.Movie,
            mediaItemId: mediaItemId,
            lastProgressFraction: 0.9,
            version: 5);

        // Act
        Result<Updated> result = await _sut.UpsertForUserAsync(incoming, userId, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(Result.Updated, result.Value);
        EntityEntry<MediaResumePointEntity> singleEntry = Assert.Single(_mockContext.ChangeTracker.Entries<MediaResumePointEntity>());
        Assert.Same(trackedResumePoint, singleEntry.Entity);
        Assert.Equal(EntityState.Modified, singleEntry.State);
        Assert.Equal(0.9, trackedResumePoint.LastProgressFraction);
        Assert.Equal(6L, trackedResumePoint.Version);
    }

    [Fact]
    public async Task DeleteForUserAndMediaItemAsync_WhenTheResumePointMatches_ShouldRemoveItAndReturnDeleted()
    {
        // Arrange
        Guid userId = Guid.NewGuid();
        Guid mediaItemId = Guid.NewGuid();
        MediaResumePointEntity resumePoint = _mediaResumePointEntityFixture.Create(
            userId: userId,
            mediaKind: MediaKind.Movie,
            mediaItemId: mediaItemId);
        _mockContext.MediaResumePoints.Add(resumePoint);
        await _mockContext.SaveChangesAsync();

        // Act
        Result<Deleted> result = await _sut.DeleteForUserAndMediaItemAsync(userId, MediaKind.Movie, mediaItemId, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(Result.Deleted, result.Value);

        EntityEntry<MediaResumePointEntity>? deletedResumePoint = _mockContext.ChangeTracker.Entries<MediaResumePointEntity>()
            .FirstOrDefault(entry => entry.State == EntityState.Deleted && entry.Entity.Id == resumePoint.Id);
        Assert.NotNull(deletedResumePoint);

        await _mockContext.SaveChangesAsync();
        Assert.Null(await _mockContext.MediaResumePoints.FirstOrDefaultAsync(candidate => candidate.Id == resumePoint.Id));
    }

    [Fact]
    public async Task DeleteForUserAndMediaItemAsync_WhenTheResumePointBelongsToAnotherUser_ShouldLeaveItUntouched()
    {
        // Arrange
        Guid mediaItemId = Guid.NewGuid();
        MediaResumePointEntity resumePoint = _mediaResumePointEntityFixture.Create(
            userId: Guid.NewGuid(),
            mediaKind: MediaKind.Movie,
            mediaItemId: mediaItemId);
        _mockContext.MediaResumePoints.Add(resumePoint);
        await _mockContext.SaveChangesAsync();

        // Act
        Result<Deleted> result = await _sut.DeleteForUserAndMediaItemAsync(Guid.NewGuid(), MediaKind.Movie, mediaItemId, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(Result.Deleted, result.Value);
        Assert.DoesNotContain(_mockContext.ChangeTracker.Entries<MediaResumePointEntity>(), entry => entry.State == EntityState.Deleted);
        Assert.Equal(EntityState.Unchanged, _mockContext.Entry(resumePoint).State);

        await _mockContext.SaveChangesAsync();
        Assert.NotNull(await _mockContext.MediaResumePoints.FirstOrDefaultAsync(candidate => candidate.Id == resumePoint.Id));
    }

    [Fact]
    public async Task DeleteForUserAndMediaItemAsync_WhenTheResumePointDoesNotExist_ShouldReturnDeletedWithoutChanges()
    {
        // Act
        Result<Deleted> result = await _sut.DeleteForUserAndMediaItemAsync(Guid.NewGuid(), MediaKind.Movie, Guid.NewGuid(), CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(Result.Deleted, result.Value);
        Assert.Empty(_mockContext.ChangeTracker.Entries<MediaResumePointEntity>());
    }

    [Fact]
    public async Task DeleteAllForUserAsync_WhenTheUserHasResumePoints_ShouldRemoveOnlyTheirResumePoints()
    {
        // Arrange
        Guid userId = Guid.NewGuid();
        Guid libraryId = Guid.NewGuid();
        MediaResumePointEntity firstResumePoint = _mediaResumePointEntityFixture.Create(userId: userId, libraryId: libraryId);
        MediaResumePointEntity secondResumePoint = _mediaResumePointEntityFixture.Create(userId: userId, libraryId: Guid.NewGuid());
        MediaResumePointEntity otherUserResumePoint = _mediaResumePointEntityFixture.Create(userId: Guid.NewGuid(), libraryId: libraryId);
        _mockContext.MediaResumePoints.AddRange(firstResumePoint, secondResumePoint, otherUserResumePoint);
        await _mockContext.SaveChangesAsync();

        // Act
        Result<Deleted> result = await _sut.DeleteAllForUserAsync(userId, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(Result.Deleted, result.Value);

        List<Guid> deletedResumePointIds = [.. _mockContext.ChangeTracker.Entries<MediaResumePointEntity>()
            .Where(entry => entry.State == EntityState.Deleted)
            .Select(entry => entry.Entity.Id)
            .OrderBy(id => id)];
        Assert.Equal(
            new[] { firstResumePoint.Id, secondResumePoint.Id }.OrderBy(id => id),
            deletedResumePointIds);

        await _mockContext.SaveChangesAsync();
        List<Guid> remainingResumePointIds = await _mockContext.MediaResumePoints
            .AsNoTracking()
            .Select(resumePoint => resumePoint.Id)
            .ToListAsync();
        Assert.Equal([otherUserResumePoint.Id], remainingResumePointIds);
    }

    [Fact]
    public async Task DeleteAllForUserInLibraryAsync_WhenTheUserHasResumePointsInTheLibrary_ShouldRemoveOnlyThoseResumePoints()
    {
        // Arrange
        Guid userId = Guid.NewGuid();
        Guid libraryId = Guid.NewGuid();
        MediaResumePointEntity matchingResumePoint = _mediaResumePointEntityFixture.Create(userId: userId, libraryId: libraryId);
        MediaResumePointEntity otherLibraryResumePoint = _mediaResumePointEntityFixture.Create(userId: userId, libraryId: Guid.NewGuid());
        MediaResumePointEntity otherUserResumePoint = _mediaResumePointEntityFixture.Create(userId: Guid.NewGuid(), libraryId: libraryId);
        _mockContext.MediaResumePoints.AddRange(matchingResumePoint, otherLibraryResumePoint, otherUserResumePoint);
        await _mockContext.SaveChangesAsync();

        // Act
        Result<Deleted> result = await _sut.DeleteAllForUserInLibraryAsync(userId, libraryId, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(Result.Deleted, result.Value);

        List<Guid> deletedResumePointIds = [.. _mockContext.ChangeTracker.Entries<MediaResumePointEntity>()
            .Where(entry => entry.State == EntityState.Deleted)
            .Select(entry => entry.Entity.Id)];
        Assert.Equal([matchingResumePoint.Id], deletedResumePointIds);

        await _mockContext.SaveChangesAsync();
        List<Guid> remainingResumePointIds = await _mockContext.MediaResumePoints
            .AsNoTracking()
            .Select(resumePoint => resumePoint.Id)
            .ToListAsync();
        Assert.Equal(
            new[] { otherLibraryResumePoint.Id, otherUserResumePoint.Id }.OrderBy(id => id),
            remainingResumePointIds.OrderBy(id => id));
    }
}
