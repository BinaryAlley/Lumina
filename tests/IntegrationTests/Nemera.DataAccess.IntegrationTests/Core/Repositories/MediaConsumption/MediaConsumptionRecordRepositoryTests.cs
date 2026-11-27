#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.MediaConsumption;
using Lumina.Application.Common.DataAccess.Entities.UsersManagement;
using Lumina.Application.Common.Infrastructure.Authentication;
using Lumina.Application.Common.Infrastructure.Time;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.MediaConsumption;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.UsersManagement;
using Lumina.DataAccess.Common.Interceptors;
using Lumina.DataAccess.Core.Repositories.MediaConsumption;
using Lumina.DataAccess.Core.UoW;
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.SharedKernel.Common.Enums.MediaConsumption;
using Lumina.Domain.SharedKernel.Common.Enums.MediaLibrary;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.DataAccess.IntegrationTests.Core.Repositories.MediaConsumption;

/// <summary>
/// Contains integration tests for the <see cref="MediaConsumptionRecordRepository"/> class, exercising it against a real SQLite database.
/// </summary>
[ExcludeFromCodeCoverage]
public class MediaConsumptionRecordRepositoryTests
{
    private readonly MediaConsumptionRecordEntityFixture _mediaConsumptionRecordEntityFixture = new();
    private readonly UserEntityFixture _userEntityFixture = new();

    [Fact]
    public async Task InsertAsync_WhenRecordsAreNew_ShouldPersistEveryStoredColumn()
    {
        // Arrange
        using SqliteConnection anchorConnection = new($"Data Source=luminadataccess-mediaconsumptionrepo-insert-{Guid.NewGuid()};Mode=Memory;Cache=Shared");
        anchorConnection.Open();
        Guid userId = Guid.NewGuid();
        DateTime utcNow = new(2025, 1, 1, 12, 0, 0, DateTimeKind.Utc);
        using LuminaDbContext context = CreateAuditedContext(anchorConnection, userId, utcNow);
        MediaConsumptionRecordRepository sut = new(context);
        context.Users.Add(_userEntityFixture.Create(id: userId));
        Guid libraryId = Guid.NewGuid();
        Guid timeOffsetMediaItemId = Guid.NewGuid();
        Guid locationMediaItemId = Guid.NewGuid();
        DateTime timeOffsetStartedOnUtc = new(2025, 1, 1, 8, 0, 0, DateTimeKind.Utc);
        DateTime timeOffsetLastActivityOnUtc = new(2025, 1, 1, 8, 45, 0, DateTimeKind.Utc);
        DateTime timeOffsetEndedOnUtc = new(2025, 1, 1, 9, 0, 0, DateTimeKind.Utc);
        MediaConsumptionRecordEntity timeOffsetRecord = _mediaConsumptionRecordEntityFixture.Create(
            userId: userId,
            libraryId: libraryId,
            mediaKind: MediaKind.Movie,
            mediaItemId: timeOffsetMediaItemId,
            status: ConsumptionStatus.Completed,
            startedOnUtc: timeOffsetStartedOnUtc,
            lastActivityOnUtc: timeOffsetLastActivityOnUtc,
            endedOnUtc: timeOffsetEndedOnUtc,
            consumedDurationMilliseconds: 1_800_000,
            lastPositionKind: MediaPositionKind.TimeOffset,
            lastPositionTimeOffsetMilliseconds: 1_234_567,
            includeLastPositionLocation: false,
            lastProgressFraction: 0.42,
            consecutivePagesRead: 0,
            version: 7);
        MediaConsumptionRecordEntity locationRecord = _mediaConsumptionRecordEntityFixture.Create(
            userId: userId,
            libraryId: libraryId,
            mediaKind: MediaKind.Book,
            mediaItemId: locationMediaItemId,
            status: ConsumptionStatus.InProgress,
            startedOnUtc: new(2025, 1, 1, 10, 0, 0, DateTimeKind.Utc),
            lastActivityOnUtc: new(2025, 1, 1, 10, 20, 0, DateTimeKind.Utc),
            includeEndedOnUtc: false,
            consumedDurationMilliseconds: 90_000,
            lastPositionKind: MediaPositionKind.Location,
            includeLastPositionTimeOffsetMilliseconds: false,
            lastPositionLocation: "chapter-3:paragraph-14",
            lastProgressFraction: 0.17,
            consecutivePagesRead: 12,
            version: 1);

        // Act
        Result<Created> timeOffsetResult = await sut.InsertAsync(timeOffsetRecord, CancellationToken.None);
        Result<Created> locationResult = await sut.InsertAsync(locationRecord, CancellationToken.None);
        await context.SaveChangesAsync();

        // Assert
        Assert.False(timeOffsetResult.IsFailure);
        Assert.Equal(Result.Created, timeOffsetResult.Value);
        Assert.False(locationResult.IsFailure);
        Assert.Equal(Result.Created, locationResult.Value);

        // A fresh context round-trips the stored rows, which proves the values went through the real provider instead of the identity map.
        using LuminaDbContext verificationContext = CreateContext(anchorConnection);
        MediaConsumptionRecordEntity storedTimeOffsetRecord = await verificationContext.MediaConsumptionRecords
            .AsNoTracking()
            .SingleAsync(record => record.Id == timeOffsetRecord.Id);
        Assert.Equal(userId, storedTimeOffsetRecord.UserId);
        Assert.Equal(libraryId, storedTimeOffsetRecord.LibraryId);
        Assert.Equal(MediaKind.Movie, storedTimeOffsetRecord.MediaKind);
        Assert.Equal(timeOffsetMediaItemId, storedTimeOffsetRecord.MediaItemId);
        Assert.Equal(ConsumptionStatus.Completed, storedTimeOffsetRecord.Status);
        Assert.Equal(timeOffsetStartedOnUtc, storedTimeOffsetRecord.StartedOnUtc);
        Assert.Equal(timeOffsetLastActivityOnUtc, storedTimeOffsetRecord.LastActivityOnUtc);
        Assert.Equal(timeOffsetEndedOnUtc, storedTimeOffsetRecord.EndedOnUtc);
        Assert.Equal(1_800_000L, storedTimeOffsetRecord.ConsumedDurationMilliseconds);
        Assert.Equal(MediaPositionKind.TimeOffset, storedTimeOffsetRecord.LastPositionKind);
        Assert.Equal(1_234_567L, storedTimeOffsetRecord.LastPositionTimeOffsetMilliseconds);
        Assert.Null(storedTimeOffsetRecord.LastPositionLocation);
        Assert.Equal(0.42, storedTimeOffsetRecord.LastProgressFraction);
        Assert.Equal(0, storedTimeOffsetRecord.ConsecutivePagesRead);
        Assert.Equal(7L, storedTimeOffsetRecord.Version);
        // The audit interceptor owns the creation columns.
        Assert.Equal(utcNow, storedTimeOffsetRecord.CreatedOnUtc);
        Assert.Equal(userId, storedTimeOffsetRecord.CreatedBy);
        Assert.Null(storedTimeOffsetRecord.UpdatedOnUtc);
        Assert.Null(storedTimeOffsetRecord.UpdatedBy);

        MediaConsumptionRecordEntity storedLocationRecord = await verificationContext.MediaConsumptionRecords
            .AsNoTracking()
            .SingleAsync(record => record.Id == locationRecord.Id);
        Assert.Equal(MediaKind.Book, storedLocationRecord.MediaKind);
        Assert.Equal(ConsumptionStatus.InProgress, storedLocationRecord.Status);
        Assert.Null(storedLocationRecord.EndedOnUtc);
        Assert.Equal(MediaPositionKind.Location, storedLocationRecord.LastPositionKind);
        Assert.Null(storedLocationRecord.LastPositionTimeOffsetMilliseconds);
        Assert.Equal("chapter-3:paragraph-14", storedLocationRecord.LastPositionLocation);
        Assert.Equal(0.17, storedLocationRecord.LastProgressFraction);
        Assert.Equal(12, storedLocationRecord.ConsecutivePagesRead);

        // The enums are stored as their names, not as their numeric values.
        (string timeOffsetMediaKind, string timeOffsetStatus) = await ReadStoredEnumColumnsAsync(verificationContext, timeOffsetRecord.Id);
        Assert.Equal(nameof(MediaKind.Movie), timeOffsetMediaKind);
        Assert.Equal(nameof(ConsumptionStatus.Completed), timeOffsetStatus);
        (string locationMediaKind, string locationStatus) = await ReadStoredEnumColumnsAsync(verificationContext, locationRecord.Id);
        Assert.Equal(nameof(MediaKind.Book), locationMediaKind);
        Assert.Equal(nameof(ConsumptionStatus.InProgress), locationStatus);
    }

    [Fact]
    public async Task GetByIdForUserAsync_WhenTheRecordBelongsToTheUser_ShouldReturnTheStoredRecord()
    {
        // Arrange
        using SqliteConnection anchorConnection = new($"Data Source=luminadataccess-mediaconsumptionrepo-get-{Guid.NewGuid()};Mode=Memory;Cache=Shared");
        anchorConnection.Open();
        Guid userId = Guid.NewGuid();
        DateTime utcNow = new(2025, 1, 1, 12, 0, 0, DateTimeKind.Utc);
        using LuminaDbContext context = CreateAuditedContext(anchorConnection, userId, utcNow);
        MediaConsumptionRecordRepository sut = new(context);
        context.Users.Add(_userEntityFixture.Create(id: userId));
        MediaConsumptionRecordEntity record = _mediaConsumptionRecordEntityFixture.Create(
            userId: userId,
            mediaKind: MediaKind.Movie,
            lastProgressFraction: 0.3);
        context.MediaConsumptionRecords.Add(record);
        await context.SaveChangesAsync();

        // Act
        Result<MediaConsumptionRecordEntity?> result = await sut.GetByIdForUserAsync(record.Id, userId, cancellationToken: CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.NotNull(result.Value);
        Assert.Equal(record.Id, result.Value!.Id);
        Assert.Equal(userId, result.Value.UserId);
        Assert.Equal(record.MediaItemId, result.Value.MediaItemId);
        Assert.Equal(record.LibraryId, result.Value.LibraryId);
        Assert.Equal(0.3, result.Value.LastProgressFraction);
        Assert.Equal(EntityState.Unchanged, context.Entry(result.Value).State);
    }

    [Fact]
    public async Task GetByIdForUserAsync_WhenTheRecordBelongsToAnotherUser_ShouldReturnNull()
    {
        // Arrange
        using SqliteConnection anchorConnection = new($"Data Source=luminadataccess-mediaconsumptionrepo-get-other-{Guid.NewGuid()};Mode=Memory;Cache=Shared");
        anchorConnection.Open();
        Guid userId = Guid.NewGuid();
        Guid otherUserId = Guid.NewGuid();
        DateTime utcNow = new(2025, 1, 1, 12, 0, 0, DateTimeKind.Utc);
        using LuminaDbContext context = CreateAuditedContext(anchorConnection, userId, utcNow);
        MediaConsumptionRecordRepository sut = new(context);
        context.Users.AddRange(_userEntityFixture.Create(id: userId), _userEntityFixture.Create(id: otherUserId));
        MediaConsumptionRecordEntity record = _mediaConsumptionRecordEntityFixture.Create(userId: userId);
        context.MediaConsumptionRecords.Add(record);
        await context.SaveChangesAsync();

        // Act
        Result<MediaConsumptionRecordEntity?> result = await sut.GetByIdForUserAsync(record.Id, otherUserId, cancellationToken: CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Null(result.Value);
    }

    [Fact]
    public async Task GetByIdForUserAsync_WhenEntitiesShouldNotBeTracked_ShouldReturnADetachedInstance()
    {
        // Arrange
        using SqliteConnection anchorConnection = new($"Data Source=luminadataccess-mediaconsumptionrepo-get-detached-{Guid.NewGuid()};Mode=Memory;Cache=Shared");
        anchorConnection.Open();
        Guid userId = Guid.NewGuid();
        DateTime utcNow = new(2025, 1, 1, 12, 0, 0, DateTimeKind.Utc);
        using LuminaDbContext context = CreateAuditedContext(anchorConnection, userId, utcNow);
        MediaConsumptionRecordRepository sut = new(context);
        context.Users.Add(_userEntityFixture.Create(id: userId));
        MediaConsumptionRecordEntity record = _mediaConsumptionRecordEntityFixture.Create(userId: userId);
        context.MediaConsumptionRecords.Add(record);
        await context.SaveChangesAsync();

        // Act
        Result<MediaConsumptionRecordEntity?> result = await sut.GetByIdForUserAsync(record.Id, userId, shouldTrackEntities: false, cancellationToken: CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.NotNull(result.Value);
        Assert.False(ReferenceEquals(record, result.Value));
        Assert.Equal(EntityState.Detached, context.Entry(result.Value!).State);
        Assert.Equal(EntityState.Unchanged, context.Entry(record).State);
    }

    [Fact]
    public async Task UpdateForUserAsync_WhenNothingChanged_ShouldNotWriteOrBumpTheVersion()
    {
        // Arrange
        using SqliteConnection anchorConnection = new($"Data Source=luminadataccess-mediaconsumptionrepo-update-noop-{Guid.NewGuid()};Mode=Memory;Cache=Shared");
        anchorConnection.Open();
        Guid userId = Guid.NewGuid();
        DateTime utcNow = new(2025, 1, 1, 12, 0, 0, DateTimeKind.Utc);
        DateTime laterUtcNow = new(2025, 1, 1, 13, 0, 0, DateTimeKind.Utc);
        MediaConsumptionRecordEntity record;
        long storedVersion;
        using (LuminaDbContext seedContext = CreateAuditedContext(anchorConnection, userId, utcNow))
        {
            seedContext.Users.Add(_userEntityFixture.Create(id: userId));
            record = _mediaConsumptionRecordEntityFixture.Create(userId: userId, mediaKind: MediaKind.Movie, lastProgressFraction: 0.4);
            seedContext.MediaConsumptionRecords.Add(record);
            await seedContext.SaveChangesAsync();
            storedVersion = record.Version;
        }
        using LuminaDbContext context = CreateAuditedContext(anchorConnection, userId, laterUtcNow);
        MediaConsumptionRecordRepository sut = new(context);
        // A detached copy is the shape the command mapping produces for an update, so this is a real no-op round-trip.
        MediaConsumptionRecordEntity incoming = await context.MediaConsumptionRecords
            .AsNoTracking()
            .SingleAsync(candidate => candidate.Id == record.Id);

        // Act
        Result<Updated> result = await sut.UpdateForUserAsync(incoming, userId, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(Result.Updated, result.Value);
        Assert.DoesNotContain(context.ChangeTracker.Entries<MediaConsumptionRecordEntity>(), entry => entry.State == EntityState.Modified);

        await context.SaveChangesAsync();
        MediaConsumptionRecordEntity storedRecord = await context.MediaConsumptionRecords
            .AsNoTracking()
            .SingleAsync(candidate => candidate.Id == record.Id);
        Assert.Equal(storedVersion, storedRecord.Version);
        Assert.Equal(0.4, storedRecord.LastProgressFraction);
        Assert.Equal(utcNow, storedRecord.CreatedOnUtc);
        Assert.Null(storedRecord.UpdatedOnUtc);
        Assert.Null(storedRecord.UpdatedBy);
    }

    [Fact]
    public async Task UpdateForUserAsync_WhenEditableValuesChanged_ShouldIncrementTheVersionAndStampTheAudit()
    {
        // Arrange
        using SqliteConnection anchorConnection = new($"Data Source=luminadataccess-mediaconsumptionrepo-update-changed-{Guid.NewGuid()};Mode=Memory;Cache=Shared");
        anchorConnection.Open();
        Guid userId = Guid.NewGuid();
        DateTime utcNow = new(2025, 1, 1, 12, 0, 0, DateTimeKind.Utc);
        DateTime laterUtcNow = new(2025, 1, 1, 13, 0, 0, DateTimeKind.Utc);
        Guid libraryId = Guid.NewGuid();
        Guid mediaItemId = Guid.NewGuid();
        DateTime startedOnUtc = new(2025, 1, 1, 8, 0, 0, DateTimeKind.Utc);
        MediaConsumptionRecordEntity record;
        long storedVersion;
        using (LuminaDbContext seedContext = CreateAuditedContext(anchorConnection, userId, utcNow))
        {
            seedContext.Users.Add(_userEntityFixture.Create(id: userId));
            record = _mediaConsumptionRecordEntityFixture.Create(
                userId: userId,
                libraryId: libraryId,
                mediaKind: MediaKind.Movie,
                mediaItemId: mediaItemId,
                status: ConsumptionStatus.InProgress,
                startedOnUtc: startedOnUtc,
                lastActivityOnUtc: startedOnUtc.AddMinutes(10),
                includeEndedOnUtc: false,
                consumedDurationMilliseconds: 60_000,
                lastPositionKind: MediaPositionKind.TimeOffset,
                lastPositionTimeOffsetMilliseconds: 30_000,
                includeLastPositionLocation: false,
                lastProgressFraction: 0.5,
                consecutivePagesRead: 0,
                version: 3);
            seedContext.MediaConsumptionRecords.Add(record);
            await seedContext.SaveChangesAsync();
            storedVersion = record.Version;
        }
        using LuminaDbContext context = CreateAuditedContext(anchorConnection, userId, laterUtcNow);
        MediaConsumptionRecordRepository sut = new(context);
        DateTime endedOnUtc = new(2025, 1, 1, 11, 0, 0, DateTimeKind.Utc);
        MediaConsumptionRecordEntity incoming = _mediaConsumptionRecordEntityFixture.Create(
            id: record.Id,
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
            version: storedVersion);

        // Act
        Result<Updated> result = await sut.UpdateForUserAsync(incoming, userId, CancellationToken.None);
        await context.SaveChangesAsync();

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(Result.Updated, result.Value);
        MediaConsumptionRecordEntity storedRecord = await context.MediaConsumptionRecords
            .AsNoTracking()
            .SingleAsync(candidate => candidate.Id == record.Id);
        // The stored identity is preserved even when the incoming record carries different values for it.
        Assert.Equal(userId, storedRecord.UserId);
        Assert.Equal(libraryId, storedRecord.LibraryId);
        Assert.Equal(MediaKind.Movie, storedRecord.MediaKind);
        Assert.Equal(mediaItemId, storedRecord.MediaItemId);
        Assert.Equal(startedOnUtc, storedRecord.StartedOnUtc);
        // The editable values are copied from the incoming record.
        Assert.Equal(ConsumptionStatus.Completed, storedRecord.Status);
        Assert.Equal(new DateTime(2025, 1, 1, 10, 55, 0, DateTimeKind.Utc), storedRecord.LastActivityOnUtc);
        Assert.Equal(endedOnUtc, storedRecord.EndedOnUtc);
        Assert.Equal(600_000L, storedRecord.ConsumedDurationMilliseconds);
        Assert.Equal(MediaPositionKind.Location, storedRecord.LastPositionKind);
        Assert.Null(storedRecord.LastPositionTimeOffsetMilliseconds);
        Assert.Equal("chapter-12", storedRecord.LastPositionLocation);
        Assert.Equal(1.0, storedRecord.LastProgressFraction);
        Assert.Equal(42, storedRecord.ConsecutivePagesRead);
        // The version is incremented by exactly one, and the audit interceptor stamps the modification columns.
        Assert.Equal(storedVersion + 1, storedRecord.Version);
        Assert.Equal(utcNow, storedRecord.CreatedOnUtc);
        Assert.Equal(userId, storedRecord.CreatedBy);
        Assert.Equal(laterUtcNow, storedRecord.UpdatedOnUtc);
        Assert.Equal(userId, storedRecord.UpdatedBy);
    }

    [Fact]
    public async Task UpdateForUserAsync_WhenAnotherContextAlreadyBumpedTheVersion_ShouldThrowDbUpdateConcurrencyException()
    {
        // Arrange
        using SqliteConnection anchorConnection = new($"Data Source=luminadataccess-mediaconsumptionrepo-update-concurrency-{Guid.NewGuid()};Mode=Memory;Cache=Shared");
        anchorConnection.Open();
        Guid userId = Guid.NewGuid();
        DateTime utcNow = new(2025, 1, 1, 12, 0, 0, DateTimeKind.Utc);
        DateTime laterUtcNow = new(2025, 1, 1, 13, 0, 0, DateTimeKind.Utc);
        MediaConsumptionRecordEntity record;
        using (LuminaDbContext seedContext = CreateAuditedContext(anchorConnection, userId, utcNow))
        {
            seedContext.Users.Add(_userEntityFixture.Create(id: userId));
            record = _mediaConsumptionRecordEntityFixture.Create(
                userId: userId,
                lastProgressFraction: 0.2,
                includeEndedOnUtc: false,
                status: ConsumptionStatus.InProgress,
                version: 4);
            seedContext.MediaConsumptionRecords.Add(record);
            await seedContext.SaveChangesAsync();
        }
        using LuminaDbContext firstContext = CreateAuditedContext(anchorConnection, userId, utcNow);
        MediaConsumptionRecordRepository firstSut = new(firstContext);
        Result<MediaConsumptionRecordEntity?> readResult = await firstSut.GetByIdForUserAsync(record.Id, userId, cancellationToken: CancellationToken.None);
        Assert.False(readResult.IsFailure);
        Assert.NotNull(readResult.Value);
        using LuminaDbContext secondContext = CreateAuditedContext(anchorConnection, userId, laterUtcNow);
        MediaConsumptionRecordRepository secondSut = new(secondContext);
        MediaConsumptionRecordEntity secondIncoming = _mediaConsumptionRecordEntityFixture.Create(
            id: record.Id,
            userId: userId,
            status: ConsumptionStatus.Stopped,
            endedOnUtc: new(2025, 1, 1, 12, 30, 0, DateTimeKind.Utc),
            lastProgressFraction: 0.8,
            version: 4);
        Result<Updated> secondUpdateResult = await secondSut.UpdateForUserAsync(secondIncoming, userId, CancellationToken.None);
        Assert.False(secondUpdateResult.IsFailure);
        await secondContext.SaveChangesAsync();
        // The first context still holds the instance it read before the second writer committed its own version.
        MediaConsumptionRecordEntity firstIncoming = _mediaConsumptionRecordEntityFixture.Create(
            id: record.Id,
            userId: userId,
            status: ConsumptionStatus.Completed,
            endedOnUtc: new(2025, 1, 1, 12, 45, 0, DateTimeKind.Utc),
            lastProgressFraction: 1.0,
            version: 4);
        Result<Updated> firstUpdateResult = await firstSut.UpdateForUserAsync(firstIncoming, userId, CancellationToken.None);
        Assert.False(firstUpdateResult.IsFailure);

        // Act
        DbUpdateConcurrencyException exception = await Assert.ThrowsAsync<DbUpdateConcurrencyException>(() => firstContext.SaveChangesAsync());

        // Assert
        Assert.NotNull(exception);
        // The stale writer did not overwrite the row the other context already committed.
        secondContext.ChangeTracker.Clear();
        MediaConsumptionRecordEntity storedRecord = await secondContext.MediaConsumptionRecords
            .AsNoTracking()
            .SingleAsync(candidate => candidate.Id == record.Id);
        Assert.Equal(ConsumptionStatus.Stopped, storedRecord.Status);
        Assert.Equal(0.8, storedRecord.LastProgressFraction);
        Assert.Equal(5L, storedRecord.Version);
        Assert.Equal(laterUtcNow, storedRecord.UpdatedOnUtc);
    }

    [Fact]
    public async Task DeleteForUserAsync_WhenTheRecordBelongsToTheUser_ShouldDeleteOnlyThatRow()
    {
        // Arrange
        using SqliteConnection anchorConnection = new($"Data Source=luminadataccess-mediaconsumptionrepo-delete-{Guid.NewGuid()};Mode=Memory;Cache=Shared");
        anchorConnection.Open();
        Guid userId = Guid.NewGuid();
        Guid otherUserId = Guid.NewGuid();
        DateTime utcNow = new(2025, 1, 1, 12, 0, 0, DateTimeKind.Utc);
        using LuminaDbContext context = CreateAuditedContext(anchorConnection, userId, utcNow);
        MediaConsumptionRecordRepository sut = new(context);
        context.Users.AddRange(_userEntityFixture.Create(id: userId), _userEntityFixture.Create(id: otherUserId));
        MediaConsumptionRecordEntity record = _mediaConsumptionRecordEntityFixture.Create(userId: userId);
        MediaConsumptionRecordEntity otherUserRecord = _mediaConsumptionRecordEntityFixture.Create(userId: otherUserId);
        context.MediaConsumptionRecords.AddRange(record, otherUserRecord);
        await context.SaveChangesAsync();

        // Act
        Result<Deleted> result = await sut.DeleteForUserAsync(record.Id, userId, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(Result.Deleted, result.Value);
        Assert.Equal(EntityState.Deleted, context.Entry(record).State);

        await context.SaveChangesAsync();
        using LuminaDbContext verificationContext = CreateContext(anchorConnection);
        Assert.False(await verificationContext.MediaConsumptionRecords.AsNoTracking().AnyAsync(candidate => candidate.Id == record.Id));
        Assert.True(await verificationContext.MediaConsumptionRecords.AsNoTracking().AnyAsync(candidate => candidate.Id == otherUserRecord.Id));
    }

    [Fact]
    public async Task DeleteAllByMediaItemIdForUserAsync_WhenRecordsMatchAllFourParts_ShouldRemoveOnlyTheMatchingRecords()
    {
        // Arrange
        using SqliteConnection anchorConnection = new($"Data Source=luminadataccess-mediaconsumptionrepo-delete-item-{Guid.NewGuid()};Mode=Memory;Cache=Shared");
        anchorConnection.Open();
        Guid userId = Guid.NewGuid();
        Guid otherUserId = Guid.NewGuid();
        DateTime utcNow = new(2025, 1, 1, 12, 0, 0, DateTimeKind.Utc);
        using LuminaDbContext context = CreateAuditedContext(anchorConnection, userId, utcNow);
        MediaConsumptionRecordRepository sut = new(context);
        context.Users.AddRange(_userEntityFixture.Create(id: userId), _userEntityFixture.Create(id: otherUserId));
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
        context.MediaConsumptionRecords.AddRange(matchingFirst, matchingSecond, otherUser, otherKind, otherItem, otherLibrary);
        await context.SaveChangesAsync();

        // Act
        Result<Deleted> result = await sut.DeleteAllByMediaItemIdForUserAsync(userId, MediaKind.Movie, mediaItemId, libraryId, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(Result.Deleted, result.Value);

        await context.SaveChangesAsync();
        using LuminaDbContext verificationContext = CreateContext(anchorConnection);
        List<Guid> remainingRecordIds = await verificationContext.MediaConsumptionRecords
            .AsNoTracking()
            .Select(record => record.Id)
            .ToListAsync();
        Assert.Equal(
            new[] { otherUser.Id, otherKind.Id, otherItem.Id, otherLibrary.Id }.OrderBy(id => id),
            remainingRecordIds.OrderBy(id => id));
    }

    [Fact]
    public async Task DeleteAllForUserAsync_WhenTheUserHasRecords_ShouldRemoveOnlyTheirRecords()
    {
        // Arrange
        using SqliteConnection anchorConnection = new($"Data Source=luminadataccess-mediaconsumptionrepo-delete-user-{Guid.NewGuid()};Mode=Memory;Cache=Shared");
        anchorConnection.Open();
        Guid userId = Guid.NewGuid();
        Guid otherUserId = Guid.NewGuid();
        DateTime utcNow = new(2025, 1, 1, 12, 0, 0, DateTimeKind.Utc);
        using LuminaDbContext context = CreateAuditedContext(anchorConnection, userId, utcNow);
        MediaConsumptionRecordRepository sut = new(context);
        context.Users.AddRange(_userEntityFixture.Create(id: userId), _userEntityFixture.Create(id: otherUserId));
        MediaConsumptionRecordEntity firstRecord = _mediaConsumptionRecordEntityFixture.Create(userId: userId);
        MediaConsumptionRecordEntity secondRecord = _mediaConsumptionRecordEntityFixture.Create(userId: userId);
        MediaConsumptionRecordEntity otherUserRecord = _mediaConsumptionRecordEntityFixture.Create(userId: otherUserId);
        context.MediaConsumptionRecords.AddRange(firstRecord, secondRecord, otherUserRecord);
        await context.SaveChangesAsync();

        // Act
        Result<Deleted> result = await sut.DeleteAllForUserAsync(userId, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(Result.Deleted, result.Value);

        await context.SaveChangesAsync();
        using LuminaDbContext verificationContext = CreateContext(anchorConnection);
        List<Guid> remainingRecordIds = await verificationContext.MediaConsumptionRecords
            .AsNoTracking()
            .Select(record => record.Id)
            .ToListAsync();
        Assert.Equal([otherUserRecord.Id], remainingRecordIds);
    }

    [Fact]
    public async Task UserDeletion_WhenTheUserOwnsConsumptionRecords_ShouldCascadeToTheRecords()
    {
        // Arrange
        using SqliteConnection anchorConnection = new($"Data Source=luminadataccess-mediaconsumptionrepo-cascade-{Guid.NewGuid()};Mode=Memory;Cache=Shared");
        anchorConnection.Open();
        Guid userId = Guid.NewGuid();
        DateTime utcNow = new(2025, 1, 1, 12, 0, 0, DateTimeKind.Utc);
        using LuminaDbContext context = CreateAuditedContext(anchorConnection, userId, utcNow);
        UserEntity user = _userEntityFixture.Create(id: userId);
        context.Users.Add(user);
        MediaConsumptionRecordEntity firstRecord = _mediaConsumptionRecordEntityFixture.Create(userId: userId);
        MediaConsumptionRecordEntity secondRecord = _mediaConsumptionRecordEntityFixture.Create(userId: userId);
        context.MediaConsumptionRecords.AddRange(firstRecord, secondRecord);
        await context.SaveChangesAsync();
        // A real deletion starts with nothing tracked, so the database cascade is the path that has to remove the records.
        context.ChangeTracker.Clear();

        // Act
        UserEntity storedUser = await context.Users.SingleAsync(candidate => candidate.Id == userId);
        context.Users.Remove(storedUser);
        await context.SaveChangesAsync();

        // Assert
        using LuminaDbContext verificationContext = CreateContext(anchorConnection);
        Assert.False(await verificationContext.MediaConsumptionRecords.AsNoTracking().AnyAsync(record => record.UserId == userId));
    }

    /// <summary>
    /// Creates a real SQLite backed context with the auditing interceptor attached, so that audit columns behave exactly like in production.
    /// </summary>
    /// <param name="anchorConnection">The open in-memory SQLite connection that keeps the database alive for the duration of the test.</param>
    /// <param name="userId">The Id of the user reported as the current user.</param>
    /// <param name="utcNow">The current UTC time reported by the time provider.</param>
    /// <returns>The created context.</returns>
    private static LuminaDbContext CreateAuditedContext(SqliteConnection anchorConnection, Guid userId, DateTime utcNow)
    {
        ICurrentUserService currentUserService = Substitute.For<ICurrentUserService>();
        currentUserService.UserId.Returns(userId);
        IDateTimeProvider dateTimeProvider = Substitute.For<IDateTimeProvider>();
        dateTimeProvider.UtcNow.Returns(utcNow);
        LuminaDbContext context = new(new DbContextOptionsBuilder<LuminaDbContext>()
            .UseSqlite(anchorConnection.ConnectionString)
            .AddInterceptors(new UpdateAuditableEntitiesInterceptor(currentUserService, dateTimeProvider))
            .Options);
        context.Database.EnsureCreated();
        return context;
    }

    /// <summary>
    /// Creates a real SQLite backed context without the auditing interceptor, so that stored values are read exactly as they were written.
    /// </summary>
    /// <param name="anchorConnection">The open in-memory SQLite connection that keeps the database alive for the duration of the test.</param>
    /// <returns>The created context.</returns>
    private static LuminaDbContext CreateContext(SqliteConnection anchorConnection)
    {
        LuminaDbContext context = new(new DbContextOptionsBuilder<LuminaDbContext>()
            .UseSqlite(anchorConnection.ConnectionString)
            .Options);
        context.Database.EnsureCreated();
        return context;
    }

    /// <summary>
    /// Reads the raw stored text of the enum columns of a consumption record, which proves the enum to string conversion is applied by the provider.
    /// </summary>
    /// <param name="context">The context to read through.</param>
    /// <param name="recordId">The Id of the record to read.</param>
    /// <returns>The stored media kind and status names.</returns>
    private static async Task<(string MediaKind, string Status)> ReadStoredEnumColumnsAsync(LuminaDbContext context, Guid recordId)
    {
        string mediaKind = await context.Database
            .SqlQueryRaw<string>("SELECT MediaKind AS \"Value\" FROM MediaConsumptionRecords WHERE Id = {0}", recordId)
            .SingleAsync();
        string status = await context.Database
            .SqlQueryRaw<string>("SELECT Status AS \"Value\" FROM MediaConsumptionRecords WHERE Id = {0}", recordId)
            .SingleAsync();
        return (mediaKind, status);
    }
}
