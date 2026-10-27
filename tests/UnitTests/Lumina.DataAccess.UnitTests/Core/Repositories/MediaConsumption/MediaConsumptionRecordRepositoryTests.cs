#region ========================================================================= USING =====================================================================================
using EntityFrameworkCore.Testing.NSubstitute;
using Lumina.Application.Common.DataAccess.Entities.MediaConsumption;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.MediaConsumption;
using Lumina.DataAccess.Core.Repositories.MediaConsumption;
using Lumina.DataAccess.Core.UoW;
using Lumina.Domain.Common.Errors;
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
/// Contains unit tests for the <see cref="MediaConsumptionRecordRepository"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class MediaConsumptionRecordRepositoryTests
{
    private readonly LuminaDbContext _mockContext;
    private readonly MediaConsumptionRecordRepository _sut;
    private readonly MediaConsumptionRecordEntityFixture _mediaConsumptionRecordEntityFixture = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="MediaConsumptionRecordRepositoryTests"/> class.
    /// </summary>
    public MediaConsumptionRecordRepositoryTests()
    {
        _mockContext = Create.MockedDbContextFor<LuminaDbContext>();
        _sut = new MediaConsumptionRecordRepository(_mockContext);
    }

    [Fact]
    public async Task InsertAsync_WhenRecordIsNew_ShouldAddTheRecordToTheContextAndReturnCreated()
    {
        // Arrange
        MediaConsumptionRecordEntity record = _mediaConsumptionRecordEntityFixture.Create();

        // Act
        Result<Created> result = await _sut.InsertAsync(record, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(Result.Created, result.Value);

        EntityEntry<MediaConsumptionRecordEntity>? addedRecord = _mockContext.ChangeTracker.Entries<MediaConsumptionRecordEntity>()
            .FirstOrDefault(entry => entry.State == EntityState.Added && entry.Entity.Id == record.Id);
        Assert.NotNull(addedRecord);
        Assert.Same(record, addedRecord.Entity);
        Assert.Equal(record.UserId, addedRecord.Entity.UserId);
        Assert.Equal(record.MediaItemId, addedRecord.Entity.MediaItemId);
    }

    [Fact]
    public async Task GetByIdForUserAsync_WhenRecordBelongsToTheUser_ShouldReturnTheRecord()
    {
        // Arrange
        Guid userId = Guid.NewGuid();
        MediaConsumptionRecordEntity record = _mediaConsumptionRecordEntityFixture.Create(userId: userId, mediaKind: MediaKind.Movie);
        _mockContext.MediaConsumptionRecords.Add(record);
        await _mockContext.SaveChangesAsync();

        // Act
        Result<MediaConsumptionRecordEntity?> result = await _sut.GetByIdForUserAsync(record.Id, userId, cancellationToken: CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.NotNull(result.Value);
        Assert.Equal(record.Id, result.Value!.Id);
        Assert.Equal(userId, result.Value.UserId);
        Assert.Equal(MediaKind.Movie, result.Value.MediaKind);
        Assert.Equal(record.MediaItemId, result.Value.MediaItemId);
        Assert.Equal(record.LibraryId, result.Value.LibraryId);
    }

    [Fact]
    public async Task GetByIdForUserAsync_WhenRecordBelongsToAnotherUser_ShouldReturnNull()
    {
        // Arrange
        MediaConsumptionRecordEntity record = _mediaConsumptionRecordEntityFixture.Create(userId: Guid.NewGuid());
        _mockContext.MediaConsumptionRecords.Add(record);
        await _mockContext.SaveChangesAsync();

        // Act
        Result<MediaConsumptionRecordEntity?> result = await _sut.GetByIdForUserAsync(record.Id, Guid.NewGuid(), cancellationToken: CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Null(result.Value);
    }

    [Fact]
    public async Task GetByIdForUserAsync_WhenRecordDoesNotExist_ShouldReturnNull()
    {
        // Act
        Result<MediaConsumptionRecordEntity?> result = await _sut.GetByIdForUserAsync(Guid.NewGuid(), Guid.NewGuid(), cancellationToken: CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Null(result.Value);
    }

    [Fact]
    public async Task GetByIdForUserAsync_WhenEntitiesShouldNotBeTracked_ShouldReturnTheRecordAndLeaveTheTrackedRowUnchanged()
    {
        // Arrange
        Guid userId = Guid.NewGuid();
        MediaConsumptionRecordEntity record = _mediaConsumptionRecordEntityFixture.Create(userId: userId);
        _mockContext.MediaConsumptionRecords.Add(record);
        await _mockContext.SaveChangesAsync();

        // Act
        Result<MediaConsumptionRecordEntity?> result = await _sut.GetByIdForUserAsync(record.Id, userId, shouldTrackEntities: false, cancellationToken: CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.NotNull(result.Value);
        Assert.Equal(record.Id, result.Value!.Id);
        Assert.Equal(record.UserId, result.Value.UserId);
        // The in-memory medium behind the mocked context does not materialize a distinct instance for a no-tracking query, so the detached
        // identity of the returned record is asserted against the real SQLite provider in the integration tests. What is asserted here is
        // that the read is served and leaves the row the context already tracks exactly as it was stored.
        EntityEntry<MediaConsumptionRecordEntity> seededEntry = _mockContext.Entry(record);
        Assert.Equal(EntityState.Unchanged, seededEntry.State);
        Assert.Equal(record.Id, seededEntry.Entity.Id);
    }

    [Fact]
    public async Task GetByIdForUserAsync_WhenEntitiesShouldBeTracked_ShouldReturnTheTrackedInstance()
    {
        // Arrange
        Guid userId = Guid.NewGuid();
        MediaConsumptionRecordEntity record = _mediaConsumptionRecordEntityFixture.Create(userId: userId);
        _mockContext.MediaConsumptionRecords.Add(record);
        await _mockContext.SaveChangesAsync();

        // Act
        Result<MediaConsumptionRecordEntity?> result = await _sut.GetByIdForUserAsync(record.Id, userId, cancellationToken: CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.NotNull(result.Value);
        Assert.Same(record, result.Value);
        Assert.Equal(EntityState.Unchanged, _mockContext.Entry(result.Value!).State);
    }

    [Fact]
    public async Task UpdateForUserAsync_WhenNoEditableValueChanged_ShouldReturnUpdatedWithoutBumpingTheVersion()
    {
        // Arrange
        Guid recordId = Guid.NewGuid();
        Guid userId = Guid.NewGuid();
        Guid libraryId = Guid.NewGuid();
        Guid mediaItemId = Guid.NewGuid();
        DateTime startedOnUtc = new(2025, 1, 1, 10, 0, 0, DateTimeKind.Utc);
        DateTime lastActivityOnUtc = new(2025, 1, 1, 10, 30, 0, DateTimeKind.Utc);
        MediaConsumptionRecordEntity storedRecord = _mediaConsumptionRecordEntityFixture.Create(
            id: recordId,
            userId: userId,
            libraryId: libraryId,
            mediaKind: MediaKind.Movie,
            mediaItemId: mediaItemId,
            status: ConsumptionStatus.InProgress,
            startedOnUtc: startedOnUtc,
            lastActivityOnUtc: lastActivityOnUtc,
            includeEndedOnUtc: false,
            consumedDurationMilliseconds: 60_000,
            lastPositionKind: MediaPositionKind.TimeOffset,
            lastPositionTimeOffsetMilliseconds: 30_000,
            includeLastPositionLocation: false,
            lastProgressFraction: 0.5,
            consecutivePagesRead: 0,
            version: 3);
        _mockContext.MediaConsumptionRecords.Add(storedRecord);
        await _mockContext.SaveChangesAsync();
        MediaConsumptionRecordEntity incoming = _mediaConsumptionRecordEntityFixture.Create(
            id: recordId,
            userId: userId,
            libraryId: libraryId,
            mediaKind: MediaKind.Movie,
            mediaItemId: mediaItemId,
            status: ConsumptionStatus.InProgress,
            startedOnUtc: startedOnUtc,
            lastActivityOnUtc: lastActivityOnUtc,
            includeEndedOnUtc: false,
            consumedDurationMilliseconds: 60_000,
            lastPositionKind: MediaPositionKind.TimeOffset,
            lastPositionTimeOffsetMilliseconds: 30_000,
            includeLastPositionLocation: false,
            lastProgressFraction: 0.5,
            consecutivePagesRead: 0,
            version: 3);

        // Act
        Result<Updated> result = await _sut.UpdateForUserAsync(incoming, userId, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(Result.Updated, result.Value);

        EntityEntry<MediaConsumptionRecordEntity> storedEntry = _mockContext.Entry(storedRecord);
        Assert.Equal(EntityState.Unchanged, storedEntry.State);
        Assert.Equal(3L, storedEntry.Entity.Version);
        Assert.DoesNotContain(_mockContext.ChangeTracker.Entries<MediaConsumptionRecordEntity>(), entry => entry.State == EntityState.Modified);
    }

    [Fact]
    public async Task UpdateForUserAsync_WhenAnEditableValueChanged_ShouldCopyTheValuesAndIncrementTheVersion()
    {
        // Arrange
        Guid recordId = Guid.NewGuid();
        Guid userId = Guid.NewGuid();
        Guid libraryId = Guid.NewGuid();
        Guid mediaItemId = Guid.NewGuid();
        DateTime startedOnUtc = new(2025, 1, 1, 10, 0, 0, DateTimeKind.Utc);
        DateTime lastActivityOnUtc = new(2025, 1, 1, 10, 30, 0, DateTimeKind.Utc);
        MediaConsumptionRecordEntity storedRecord = _mediaConsumptionRecordEntityFixture.Create(
            id: recordId,
            userId: userId,
            libraryId: libraryId,
            mediaKind: MediaKind.Movie,
            mediaItemId: mediaItemId,
            status: ConsumptionStatus.InProgress,
            startedOnUtc: startedOnUtc,
            lastActivityOnUtc: lastActivityOnUtc,
            includeEndedOnUtc: false,
            consumedDurationMilliseconds: 60_000,
            lastPositionKind: MediaPositionKind.TimeOffset,
            lastPositionTimeOffsetMilliseconds: 30_000,
            includeLastPositionLocation: false,
            lastProgressFraction: 0.5,
            consecutivePagesRead: 0,
            version: 3);
        _mockContext.MediaConsumptionRecords.Add(storedRecord);
        await _mockContext.SaveChangesAsync();
        DateTime endedOnUtc = new(2025, 1, 1, 11, 0, 0, DateTimeKind.Utc);
        MediaConsumptionRecordEntity incoming = _mediaConsumptionRecordEntityFixture.Create(
            id: recordId,
            userId: Guid.NewGuid(),
            libraryId: Guid.NewGuid(),
            mediaKind: MediaKind.Book,
            mediaItemId: Guid.NewGuid(),
            status: ConsumptionStatus.Completed,
            startedOnUtc: new(2024, 12, 31, 9, 0, 0, DateTimeKind.Utc),
            lastActivityOnUtc: new(2025, 1, 1, 10, 55, 0, DateTimeKind.Utc),
            endedOnUtc: endedOnUtc,
            consumedDurationMilliseconds: 600_000,
            lastPositionKind: MediaPositionKind.Location,
            includeLastPositionTimeOffsetMilliseconds: false,
            lastPositionLocation: "chapter-12",
            lastProgressFraction: 1.0,
            consecutivePagesRead: 42,
            version: 3);

        // Act
        Result<Updated> result = await _sut.UpdateForUserAsync(incoming, userId, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(Result.Updated, result.Value);

        EntityEntry<MediaConsumptionRecordEntity> storedEntry = _mockContext.Entry(storedRecord);
        Assert.Equal(EntityState.Modified, storedEntry.State);
        MediaConsumptionRecordEntity updatedRecord = storedEntry.Entity;
        // The stored identity is never reassigned by an edit.
        Assert.Equal(recordId, updatedRecord.Id);
        Assert.Equal(userId, updatedRecord.UserId);
        Assert.Equal(libraryId, updatedRecord.LibraryId);
        Assert.Equal(MediaKind.Movie, updatedRecord.MediaKind);
        Assert.Equal(mediaItemId, updatedRecord.MediaItemId);
        Assert.Equal(startedOnUtc, updatedRecord.StartedOnUtc);
        // The editable values are copied from the incoming record.
        Assert.Equal(ConsumptionStatus.Completed, updatedRecord.Status);
        Assert.Equal(new DateTime(2025, 1, 1, 10, 55, 0, DateTimeKind.Utc), updatedRecord.LastActivityOnUtc);
        Assert.Equal(endedOnUtc, updatedRecord.EndedOnUtc);
        Assert.Equal(600_000L, updatedRecord.ConsumedDurationMilliseconds);
        Assert.Equal(MediaPositionKind.Location, updatedRecord.LastPositionKind);
        Assert.Null(updatedRecord.LastPositionTimeOffsetMilliseconds);
        Assert.Equal("chapter-12", updatedRecord.LastPositionLocation);
        Assert.Equal(1.0, updatedRecord.LastProgressFraction);
        Assert.Equal(42, updatedRecord.ConsecutivePagesRead);
        // The version is incremented by exactly one for a single edit.
        Assert.Equal(4L, updatedRecord.Version);
    }

    [Fact]
    public async Task UpdateForUserAsync_WhenOnlyIdentityValuesDiffer_ShouldReturnUpdatedWithoutBumpingTheVersion()
    {
        // Arrange
        Guid recordId = Guid.NewGuid();
        Guid userId = Guid.NewGuid();
        Guid libraryId = Guid.NewGuid();
        Guid mediaItemId = Guid.NewGuid();
        DateTime startedOnUtc = new(2025, 1, 1, 10, 0, 0, DateTimeKind.Utc);
        DateTime lastActivityOnUtc = new(2025, 1, 1, 10, 30, 0, DateTimeKind.Utc);
        MediaConsumptionRecordEntity storedRecord = _mediaConsumptionRecordEntityFixture.Create(
            id: recordId,
            userId: userId,
            libraryId: libraryId,
            mediaKind: MediaKind.Movie,
            mediaItemId: mediaItemId,
            status: ConsumptionStatus.InProgress,
            startedOnUtc: startedOnUtc,
            lastActivityOnUtc: lastActivityOnUtc,
            includeEndedOnUtc: false,
            consumedDurationMilliseconds: 60_000,
            lastPositionKind: MediaPositionKind.TimeOffset,
            lastPositionTimeOffsetMilliseconds: 30_000,
            includeLastPositionLocation: false,
            lastProgressFraction: 0.5,
            consecutivePagesRead: 0,
            version: 3);
        _mockContext.MediaConsumptionRecords.Add(storedRecord);
        await _mockContext.SaveChangesAsync();
        MediaConsumptionRecordEntity incoming = _mediaConsumptionRecordEntityFixture.Create(
            id: recordId,
            userId: Guid.NewGuid(),
            libraryId: Guid.NewGuid(),
            mediaKind: MediaKind.Book,
            mediaItemId: Guid.NewGuid(),
            status: ConsumptionStatus.InProgress,
            startedOnUtc: new(2024, 12, 31, 9, 0, 0, DateTimeKind.Utc),
            lastActivityOnUtc: lastActivityOnUtc,
            includeEndedOnUtc: false,
            consumedDurationMilliseconds: 60_000,
            lastPositionKind: MediaPositionKind.TimeOffset,
            lastPositionTimeOffsetMilliseconds: 30_000,
            includeLastPositionLocation: false,
            lastProgressFraction: 0.5,
            consecutivePagesRead: 0,
            version: 3);

        // Act
        Result<Updated> result = await _sut.UpdateForUserAsync(incoming, userId, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(Result.Updated, result.Value);

        EntityEntry<MediaConsumptionRecordEntity> storedEntry = _mockContext.Entry(storedRecord);
        Assert.Equal(EntityState.Unchanged, storedEntry.State);
        Assert.Equal(3L, storedEntry.Entity.Version);
        Assert.Equal(userId, storedEntry.Entity.UserId);
        Assert.Equal(libraryId, storedEntry.Entity.LibraryId);
        Assert.Equal(MediaKind.Movie, storedEntry.Entity.MediaKind);
        Assert.Equal(mediaItemId, storedEntry.Entity.MediaItemId);
        Assert.Equal(startedOnUtc, storedEntry.Entity.StartedOnUtc);
    }

    [Fact]
    public async Task UpdateForUserAsync_WhenRecordDoesNotExist_ShouldReturnConsumptionRecordNotFound()
    {
        // Arrange
        MediaConsumptionRecordEntity incoming = _mediaConsumptionRecordEntityFixture.Create();

        // Act
        Result<Updated> result = await _sut.UpdateForUserAsync(incoming, incoming.UserId, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.MediaConsumption.ConsumptionRecordNotFound, result.FirstError);
        Assert.Empty(_mockContext.ChangeTracker.Entries<MediaConsumptionRecordEntity>());
    }

    [Fact]
    public async Task UpdateForUserAsync_WhenRecordBelongsToAnotherUser_ShouldReturnConsumptionRecordNotFoundAndNotModifyIt()
    {
        // Arrange
        Guid ownerId = Guid.NewGuid();
        MediaConsumptionRecordEntity storedRecord = _mediaConsumptionRecordEntityFixture.Create(
            userId: ownerId,
            status: ConsumptionStatus.InProgress,
            includeEndedOnUtc: false,
            lastProgressFraction: 0.5,
            version: 3);
        _mockContext.MediaConsumptionRecords.Add(storedRecord);
        await _mockContext.SaveChangesAsync();
        MediaConsumptionRecordEntity incoming = _mediaConsumptionRecordEntityFixture.Create(
            id: storedRecord.Id,
            userId: Guid.NewGuid(),
            status: ConsumptionStatus.Completed,
            lastProgressFraction: 1.0,
            version: 3);

        // Act
        Result<Updated> result = await _sut.UpdateForUserAsync(incoming, Guid.NewGuid(), CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.MediaConsumption.ConsumptionRecordNotFound, result.FirstError);
        EntityEntry<MediaConsumptionRecordEntity> storedEntry = _mockContext.Entry(storedRecord);
        Assert.Equal(EntityState.Unchanged, storedEntry.State);
        Assert.Equal(ConsumptionStatus.InProgress, storedEntry.Entity.Status);
        Assert.Equal(0.5, storedEntry.Entity.LastProgressFraction);
        Assert.Equal(3L, storedEntry.Entity.Version);
        Assert.Equal(ownerId, storedEntry.Entity.UserId);
    }

    [Fact]
    public async Task DeleteForUserAsync_WhenRecordBelongsToTheUser_ShouldRemoveTheRecordAndReturnDeleted()
    {
        // Arrange
        Guid userId = Guid.NewGuid();
        MediaConsumptionRecordEntity record = _mediaConsumptionRecordEntityFixture.Create(userId: userId);
        _mockContext.MediaConsumptionRecords.Add(record);
        await _mockContext.SaveChangesAsync();

        // Act
        Result<Deleted> result = await _sut.DeleteForUserAsync(record.Id, userId, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(Result.Deleted, result.Value);

        EntityEntry<MediaConsumptionRecordEntity>? deletedRecord = _mockContext.ChangeTracker.Entries<MediaConsumptionRecordEntity>()
            .FirstOrDefault(entry => entry.State == EntityState.Deleted && entry.Entity.Id == record.Id);
        Assert.NotNull(deletedRecord);

        await _mockContext.SaveChangesAsync();
        Assert.Null(await _mockContext.MediaConsumptionRecords.FirstOrDefaultAsync(candidate => candidate.Id == record.Id));
    }

    [Fact]
    public async Task DeleteForUserAsync_WhenRecordBelongsToAnotherUser_ShouldLeaveTheRecordUntouched()
    {
        // Arrange
        MediaConsumptionRecordEntity record = _mediaConsumptionRecordEntityFixture.Create(userId: Guid.NewGuid());
        _mockContext.MediaConsumptionRecords.Add(record);
        await _mockContext.SaveChangesAsync();

        // Act
        Result<Deleted> result = await _sut.DeleteForUserAsync(record.Id, Guid.NewGuid(), CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(Result.Deleted, result.Value);
        Assert.DoesNotContain(_mockContext.ChangeTracker.Entries<MediaConsumptionRecordEntity>(), entry => entry.State == EntityState.Deleted);
        Assert.Equal(EntityState.Unchanged, _mockContext.Entry(record).State);

        await _mockContext.SaveChangesAsync();
        Assert.NotNull(await _mockContext.MediaConsumptionRecords.FirstOrDefaultAsync(candidate => candidate.Id == record.Id));
    }

    [Fact]
    public async Task DeleteForUserAsync_WhenRecordDoesNotExist_ShouldReturnDeletedWithoutChanges()
    {
        // Act
        Result<Deleted> result = await _sut.DeleteForUserAsync(Guid.NewGuid(), Guid.NewGuid(), CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(Result.Deleted, result.Value);
        Assert.Empty(_mockContext.ChangeTracker.Entries<MediaConsumptionRecordEntity>());
    }

    [Fact]
    public async Task DeleteAllByMediaItemIdForUserAsync_WhenRecordsMatchAllFourParts_ShouldRemoveOnlyTheMatchingRecords()
    {
        // Arrange
        Guid userId = Guid.NewGuid();
        Guid otherUserId = Guid.NewGuid();
        Guid libraryId = Guid.NewGuid();
        Guid otherLibraryId = Guid.NewGuid();
        Guid mediaItemId = Guid.NewGuid();
        Guid otherMediaItemId = Guid.NewGuid();
        MediaConsumptionRecordEntity matchingFirst = _mediaConsumptionRecordEntityFixture.Create(
            userId: userId,
            libraryId: libraryId,
            mediaKind: MediaKind.Movie,
            mediaItemId: mediaItemId);
        MediaConsumptionRecordEntity matchingSecond = _mediaConsumptionRecordEntityFixture.Create(
            userId: userId,
            libraryId: libraryId,
            mediaKind: MediaKind.Movie,
            mediaItemId: mediaItemId);
        MediaConsumptionRecordEntity otherUser = _mediaConsumptionRecordEntityFixture.Create(
            userId: otherUserId,
            libraryId: libraryId,
            mediaKind: MediaKind.Movie,
            mediaItemId: mediaItemId);
        MediaConsumptionRecordEntity otherKind = _mediaConsumptionRecordEntityFixture.Create(
            userId: userId,
            libraryId: libraryId,
            mediaKind: MediaKind.Book,
            mediaItemId: mediaItemId);
        MediaConsumptionRecordEntity otherItem = _mediaConsumptionRecordEntityFixture.Create(
            userId: userId,
            libraryId: libraryId,
            mediaKind: MediaKind.Movie,
            mediaItemId: otherMediaItemId);
        MediaConsumptionRecordEntity otherLibrary = _mediaConsumptionRecordEntityFixture.Create(
            userId: userId,
            libraryId: otherLibraryId,
            mediaKind: MediaKind.Movie,
            mediaItemId: mediaItemId);
        _mockContext.MediaConsumptionRecords.AddRange(matchingFirst, matchingSecond, otherUser, otherKind, otherItem, otherLibrary);
        await _mockContext.SaveChangesAsync();

        // Act
        Result<Deleted> result = await _sut.DeleteAllByMediaItemIdForUserAsync(userId, MediaKind.Movie, mediaItemId, libraryId, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(Result.Deleted, result.Value);

        List<Guid> deletedRecordIds = [.. _mockContext.ChangeTracker.Entries<MediaConsumptionRecordEntity>()
            .Where(entry => entry.State == EntityState.Deleted)
            .Select(entry => entry.Entity.Id)
            .OrderBy(id => id)];
        Assert.Equal(
            new[] { matchingFirst.Id, matchingSecond.Id }.OrderBy(id => id),
            deletedRecordIds);

        await _mockContext.SaveChangesAsync();
        List<Guid> remainingRecordIds = await _mockContext.MediaConsumptionRecords
            .AsNoTracking()
            .Select(record => record.Id)
            .ToListAsync();
        Assert.Equal(
            new[] { otherUser.Id, otherKind.Id, otherItem.Id, otherLibrary.Id }.OrderBy(id => id),
            remainingRecordIds.OrderBy(id => id));
    }

    [Fact]
    public async Task DeleteAllByMediaItemIdForUserAsync_WhenNoRecordMatches_ShouldReturnDeletedWithoutChanges()
    {
        // Arrange
        MediaConsumptionRecordEntity record = _mediaConsumptionRecordEntityFixture.Create(
            userId: Guid.NewGuid(),
            mediaKind: MediaKind.Movie,
            mediaItemId: Guid.NewGuid());
        _mockContext.MediaConsumptionRecords.Add(record);
        await _mockContext.SaveChangesAsync();

        // Act
        Result<Deleted> result = await _sut.DeleteAllByMediaItemIdForUserAsync(Guid.NewGuid(), MediaKind.Movie, Guid.NewGuid(), Guid.NewGuid(), CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(Result.Deleted, result.Value);
        Assert.DoesNotContain(_mockContext.ChangeTracker.Entries<MediaConsumptionRecordEntity>(), entry => entry.State == EntityState.Deleted);

        await _mockContext.SaveChangesAsync();
        Assert.NotNull(await _mockContext.MediaConsumptionRecords.FirstOrDefaultAsync(candidate => candidate.Id == record.Id));
    }

    [Fact]
    public async Task DeleteAllForUserAsync_WhenTheUserHasRecords_ShouldRemoveOnlyTheirRecords()
    {
        // Arrange
        Guid userId = Guid.NewGuid();
        MediaConsumptionRecordEntity firstRecord = _mediaConsumptionRecordEntityFixture.Create(userId: userId);
        MediaConsumptionRecordEntity secondRecord = _mediaConsumptionRecordEntityFixture.Create(userId: userId);
        MediaConsumptionRecordEntity otherUserRecord = _mediaConsumptionRecordEntityFixture.Create(userId: Guid.NewGuid());
        _mockContext.MediaConsumptionRecords.AddRange(firstRecord, secondRecord, otherUserRecord);
        await _mockContext.SaveChangesAsync();

        // Act
        Result<Deleted> result = await _sut.DeleteAllForUserAsync(userId, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(Result.Deleted, result.Value);

        List<Guid> deletedRecordIds = [.. _mockContext.ChangeTracker.Entries<MediaConsumptionRecordEntity>()
            .Where(entry => entry.State == EntityState.Deleted)
            .Select(entry => entry.Entity.Id)
            .OrderBy(id => id)];
        Assert.Equal(
            new[] { firstRecord.Id, secondRecord.Id }.OrderBy(id => id),
            deletedRecordIds);

        await _mockContext.SaveChangesAsync();
        List<Guid> remainingRecordIds = await _mockContext.MediaConsumptionRecords
            .AsNoTracking()
            .Select(record => record.Id)
            .ToListAsync();
        Assert.Equal([otherUserRecord.Id], remainingRecordIds);
    }

    [Fact]
    public async Task DeleteAllForUserAsync_WhenTheUserHasNoRecords_ShouldReturnDeletedWithoutChanges()
    {
        // Arrange
        MediaConsumptionRecordEntity otherUserRecord = _mediaConsumptionRecordEntityFixture.Create(userId: Guid.NewGuid());
        _mockContext.MediaConsumptionRecords.Add(otherUserRecord);
        await _mockContext.SaveChangesAsync();

        // Act
        Result<Deleted> result = await _sut.DeleteAllForUserAsync(Guid.NewGuid(), CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(Result.Deleted, result.Value);
        Assert.DoesNotContain(_mockContext.ChangeTracker.Entries<MediaConsumptionRecordEntity>(), entry => entry.State == EntityState.Deleted);

        await _mockContext.SaveChangesAsync();
        Assert.NotNull(await _mockContext.MediaConsumptionRecords.FirstOrDefaultAsync(candidate => candidate.Id == otherUserRecord.Id));
    }
}
