#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.MediaConsumption;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.MediaConsumption;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.UsersManagement;
using Lumina.DataAccess.Core.Repositories.MediaConsumption;
using Lumina.DataAccess.Core.UoW;
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.SharedKernel.Common.Enums.MediaConsumption;
using Lumina.Domain.SharedKernel.Common.Enums.MediaLibrary;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.DataAccess.IntegrationTests.Core.Repositories.MediaConsumption;

/// <summary>
/// Contains integration tests for the <see cref="ConsumptionStatisticsRepository"/> class, exercising it against a real SQLite database.
/// </summary>
[ExcludeFromCodeCoverage]
public class ConsumptionStatisticsRepositoryTests
{
    private readonly MediaConsumptionRecordEntityFixture _mediaConsumptionRecordEntityFixture = new();
    private readonly UserEntityFixture _userEntityFixture = new();

    [Fact]
    public async Task GetForUserAsync_WhenTheUserHasRecords_ShouldReturnOnlyTheirRecordsWithEveryPersistedValue()
    {
        // Arrange
        using SqliteConnection anchorConnection = new($"Data Source=luminadataccess-consumptionstats-get-{Guid.NewGuid()};Mode=Memory;Cache=Shared");
        anchorConnection.Open();
        using LuminaDbContext context = CreateContext(anchorConnection);
        ConsumptionStatisticsRepository sut = new(context);

        Guid userId = Guid.NewGuid();
        Guid otherUserId = Guid.NewGuid();
        Guid libraryId = Guid.NewGuid();
        Guid movieMediaItemId = Guid.NewGuid();
        Guid bookMediaItemId = Guid.NewGuid();
        DateTime movieStartedOnUtc = new(2025, 1, 1, 8, 0, 0, DateTimeKind.Utc);
        DateTime movieLastActivityOnUtc = new(2025, 1, 1, 8, 45, 0, DateTimeKind.Utc);
        DateTime movieEndedOnUtc = new(2025, 1, 1, 9, 0, 0, DateTimeKind.Utc);
        MediaConsumptionRecordEntity movieRecord = _mediaConsumptionRecordEntityFixture.Create(
            userId: userId,
            libraryId: libraryId,
            mediaKind: MediaKind.Movie,
            mediaItemId: movieMediaItemId,
            status: ConsumptionStatus.Completed,
            startedOnUtc: movieStartedOnUtc,
            lastActivityOnUtc: movieLastActivityOnUtc,
            endedOnUtc: movieEndedOnUtc,
            consumedDurationMilliseconds: 1_800_000,
            lastPositionKind: MediaPositionKind.TimeOffset,
            lastPositionTimeOffsetMilliseconds: 1_234_567,
            includeLastPositionLocation: false,
            lastProgressFraction: 0.42,
            consecutivePagesRead: 0,
            version: 7);
        MediaConsumptionRecordEntity bookRecord = _mediaConsumptionRecordEntityFixture.Create(
            userId: userId,
            libraryId: libraryId,
            mediaKind: MediaKind.Book,
            mediaItemId: bookMediaItemId,
            status: ConsumptionStatus.InProgress,
            startedOnUtc: new DateTime(2025, 1, 1, 10, 0, 0, DateTimeKind.Utc),
            lastActivityOnUtc: new DateTime(2025, 1, 1, 10, 20, 0, DateTimeKind.Utc),
            includeEndedOnUtc: false,
            consumedDurationMilliseconds: 90_000,
            lastPositionKind: MediaPositionKind.Location,
            includeLastPositionTimeOffsetMilliseconds: false,
            lastPositionLocation: "chapter-3:paragraph-14",
            lastProgressFraction: 0.17,
            consecutivePagesRead: 12,
            version: 1);
        MediaConsumptionRecordEntity otherUserRecord = _mediaConsumptionRecordEntityFixture.Create(
            userId: otherUserId, libraryId: libraryId, mediaKind: MediaKind.Movie);
        context.Users.AddRange(_userEntityFixture.Create(id: userId), _userEntityFixture.Create(id: otherUserId));
        context.MediaConsumptionRecords.AddRange(movieRecord, bookRecord, otherUserRecord);
        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        // Act
        Result<IReadOnlyList<MediaConsumptionRecordEntity>> result = await sut.GetForUserAsync(userId, null, null, null, null, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(2, result.Value.Count);
        Assert.DoesNotContain(result.Value, record => record.Id == otherUserRecord.Id);

        MediaConsumptionRecordEntity storedMovieRecord = result.Value.Single(record => record.Id == movieRecord.Id);
        Assert.Equal(userId, storedMovieRecord.UserId);
        Assert.Equal(libraryId, storedMovieRecord.LibraryId);
        Assert.Equal(MediaKind.Movie, storedMovieRecord.MediaKind);
        Assert.Equal(movieMediaItemId, storedMovieRecord.MediaItemId);
        Assert.Equal(ConsumptionStatus.Completed, storedMovieRecord.Status);
        Assert.Equal(movieStartedOnUtc, storedMovieRecord.StartedOnUtc);
        Assert.Equal(movieLastActivityOnUtc, storedMovieRecord.LastActivityOnUtc);
        Assert.Equal(movieEndedOnUtc, storedMovieRecord.EndedOnUtc);
        Assert.Equal(1_800_000L, storedMovieRecord.ConsumedDurationMilliseconds);
        Assert.Equal(MediaPositionKind.TimeOffset, storedMovieRecord.LastPositionKind);
        Assert.Equal(1_234_567L, storedMovieRecord.LastPositionTimeOffsetMilliseconds);
        Assert.Null(storedMovieRecord.LastPositionLocation);
        Assert.Equal(0.42, storedMovieRecord.LastProgressFraction);
        Assert.Equal(0, storedMovieRecord.ConsecutivePagesRead);
        Assert.Equal(7L, storedMovieRecord.Version);

        MediaConsumptionRecordEntity storedBookRecord = result.Value.Single(record => record.Id == bookRecord.Id);
        Assert.Equal(MediaKind.Book, storedBookRecord.MediaKind);
        Assert.Equal(bookMediaItemId, storedBookRecord.MediaItemId);
        Assert.Equal(ConsumptionStatus.InProgress, storedBookRecord.Status);
        Assert.Null(storedBookRecord.EndedOnUtc);
        Assert.Equal(90_000L, storedBookRecord.ConsumedDurationMilliseconds);
        Assert.Equal(MediaPositionKind.Location, storedBookRecord.LastPositionKind);
        Assert.Null(storedBookRecord.LastPositionTimeOffsetMilliseconds);
        Assert.Equal("chapter-3:paragraph-14", storedBookRecord.LastPositionLocation);
        Assert.Equal(0.17, storedBookRecord.LastProgressFraction);
        Assert.Equal(12, storedBookRecord.ConsecutivePagesRead);
    }

    [Fact]
    public async Task GetForUserAsync_WhenAnotherUserHasAMatchingRecord_ShouldNotReturnIt()
    {
        // Arrange
        using SqliteConnection anchorConnection = new($"Data Source=luminadataccess-consumptionstats-otheruser-{Guid.NewGuid()};Mode=Memory;Cache=Shared");
        anchorConnection.Open();
        using LuminaDbContext context = CreateContext(anchorConnection);
        ConsumptionStatisticsRepository sut = new(context);

        Guid userId = Guid.NewGuid();
        Guid otherUserId = Guid.NewGuid();
        Guid libraryId = Guid.NewGuid();
        DateTime startedOnUtc = new(2025, 1, 15, 0, 0, 0, DateTimeKind.Utc);
        MediaConsumptionRecordEntity userRecord = _mediaConsumptionRecordEntityFixture.Create(
            userId: userId, libraryId: libraryId, mediaKind: MediaKind.Movie, startedOnUtc: startedOnUtc);
        MediaConsumptionRecordEntity otherUserRecord = _mediaConsumptionRecordEntityFixture.Create(
            userId: otherUserId, libraryId: libraryId, mediaKind: MediaKind.Movie, startedOnUtc: startedOnUtc);
        context.Users.AddRange(_userEntityFixture.Create(id: userId), _userEntityFixture.Create(id: otherUserId));
        context.MediaConsumptionRecords.AddRange(userRecord, otherUserRecord);
        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        // Act
        // Every other filter matches the foreign record too, so the user scope has to be the reason it is excluded.
        Result<IReadOnlyList<MediaConsumptionRecordEntity>> result = await sut.GetForUserAsync(
            userId, libraryId, MediaKind.Movie, startedOnUtc.AddDays(-1), startedOnUtc.AddDays(1), CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        MediaConsumptionRecordEntity returnedRecord = Assert.Single(result.Value);
        Assert.Equal(userRecord.Id, returnedRecord.Id);
        Assert.Equal(userId, returnedRecord.UserId);
    }

    [Fact]
    public async Task GetForUserAsync_WhenTheRangeBoundariesMatchExactly_ShouldIncludeTheBoundaryRecords()
    {
        // Arrange
        using SqliteConnection anchorConnection = new($"Data Source=luminadataccess-consumptionstats-boundary-{Guid.NewGuid()};Mode=Memory;Cache=Shared");
        anchorConnection.Open();
        using LuminaDbContext context = CreateContext(anchorConnection);
        ConsumptionStatisticsRepository sut = new(context);

        Guid userId = Guid.NewGuid();
        DateTime fromUtc = new(2025, 1, 10, 12, 0, 0, DateTimeKind.Utc);
        DateTime toUtc = new(2025, 1, 20, 12, 0, 0, DateTimeKind.Utc);
        MediaConsumptionRecordEntity recordBeforeTheRange = _mediaConsumptionRecordEntityFixture.Create(
            userId: userId, startedOnUtc: fromUtc.AddMilliseconds(-1));
        MediaConsumptionRecordEntity recordAtTheStartBoundary = _mediaConsumptionRecordEntityFixture.Create(
            userId: userId, startedOnUtc: fromUtc);
        MediaConsumptionRecordEntity recordInsideTheRange = _mediaConsumptionRecordEntityFixture.Create(
            userId: userId, startedOnUtc: fromUtc.AddDays(5));
        MediaConsumptionRecordEntity recordAtTheEndBoundary = _mediaConsumptionRecordEntityFixture.Create(
            userId: userId, startedOnUtc: toUtc);
        MediaConsumptionRecordEntity recordAfterTheRange = _mediaConsumptionRecordEntityFixture.Create(
            userId: userId, startedOnUtc: toUtc.AddMilliseconds(1));
        context.Users.Add(_userEntityFixture.Create(id: userId));
        context.MediaConsumptionRecords.AddRange(
            recordBeforeTheRange, recordAtTheStartBoundary, recordInsideTheRange, recordAtTheEndBoundary, recordAfterTheRange);
        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        // Act
        Result<IReadOnlyList<MediaConsumptionRecordEntity>> result = await sut.GetForUserAsync(userId, null, null, fromUtc, toUtc, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(3, result.Value.Count);
        Assert.Contains(result.Value, record => record.Id == recordAtTheStartBoundary.Id);
        Assert.Contains(result.Value, record => record.Id == recordInsideTheRange.Id);
        Assert.Contains(result.Value, record => record.Id == recordAtTheEndBoundary.Id);
        Assert.DoesNotContain(result.Value, record => record.Id == recordBeforeTheRange.Id);
        Assert.DoesNotContain(result.Value, record => record.Id == recordAfterTheRange.Id);
    }

    [Fact]
    public async Task GetForUserAsync_WhenAllFiltersAreProvided_ShouldApplyEveryFilter()
    {
        // Arrange
        using SqliteConnection anchorConnection = new($"Data Source=luminadataccess-consumptionstats-allfilters-{Guid.NewGuid()};Mode=Memory;Cache=Shared");
        anchorConnection.Open();
        using LuminaDbContext context = CreateContext(anchorConnection);
        ConsumptionStatisticsRepository sut = new(context);

        Guid userId = Guid.NewGuid();
        Guid libraryId = Guid.NewGuid();
        DateTime fromUtc = new(2025, 1, 10, 0, 0, 0, DateTimeKind.Utc);
        DateTime toUtc = new(2025, 1, 20, 0, 0, 0, DateTimeKind.Utc);
        MediaConsumptionRecordEntity matchingRecord = _mediaConsumptionRecordEntityFixture.Create(
            userId: userId, libraryId: libraryId, mediaKind: MediaKind.Movie, startedOnUtc: new DateTime(2025, 1, 15, 0, 0, 0, DateTimeKind.Utc));
        MediaConsumptionRecordEntity recordOfAnotherLibrary = _mediaConsumptionRecordEntityFixture.Create(
            userId: userId, libraryId: Guid.NewGuid(), mediaKind: MediaKind.Movie, startedOnUtc: new DateTime(2025, 1, 15, 0, 0, 0, DateTimeKind.Utc));
        MediaConsumptionRecordEntity recordOfAnotherKind = _mediaConsumptionRecordEntityFixture.Create(
            userId: userId, libraryId: libraryId, mediaKind: MediaKind.Book, startedOnUtc: new DateTime(2025, 1, 15, 0, 0, 0, DateTimeKind.Utc));
        MediaConsumptionRecordEntity recordOutsideTheRange = _mediaConsumptionRecordEntityFixture.Create(
            userId: userId, libraryId: libraryId, mediaKind: MediaKind.Movie, startedOnUtc: toUtc.AddDays(1));
        context.Users.Add(_userEntityFixture.Create(id: userId));
        context.MediaConsumptionRecords.AddRange(matchingRecord, recordOfAnotherLibrary, recordOfAnotherKind, recordOutsideTheRange);
        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        // Act
        Result<IReadOnlyList<MediaConsumptionRecordEntity>> result = await sut.GetForUserAsync(
            userId, libraryId, MediaKind.Movie, fromUtc, toUtc, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        MediaConsumptionRecordEntity returnedRecord = Assert.Single(result.Value);
        Assert.Equal(matchingRecord.Id, returnedRecord.Id);
    }

    [Fact]
    public async Task GetForUserAsync_WhenNothingMatches_ShouldReturnAnEmptyList()
    {
        // Arrange
        using SqliteConnection anchorConnection = new($"Data Source=luminadataccess-consumptionstats-emptymatch-{Guid.NewGuid()};Mode=Memory;Cache=Shared");
        anchorConnection.Open();
        using LuminaDbContext context = CreateContext(anchorConnection);
        ConsumptionStatisticsRepository sut = new(context);

        Guid userId = Guid.NewGuid();
        MediaConsumptionRecordEntity record = _mediaConsumptionRecordEntityFixture.Create(
            userId: userId, libraryId: Guid.NewGuid(), mediaKind: MediaKind.Book, startedOnUtc: new DateTime(2025, 1, 15, 0, 0, 0, DateTimeKind.Utc));
        context.Users.Add(_userEntityFixture.Create(id: userId));
        context.MediaConsumptionRecords.Add(record);
        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        // Act
        Result<IReadOnlyList<MediaConsumptionRecordEntity>> result = await sut.GetForUserAsync(
            userId, Guid.NewGuid(), MediaKind.Movie, new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 1, 31, 0, 0, 0, DateTimeKind.Utc), CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.NotNull(result.Value);
        Assert.Empty(result.Value);
    }

    [Fact]
    public async Task GetForUserAsync_WhenCalled_ShouldReturnDetachedRecordsAndAllowRepeatedReads()
    {
        // Arrange
        using SqliteConnection anchorConnection = new($"Data Source=luminadataccess-consumptionstats-notracking-{Guid.NewGuid()};Mode=Memory;Cache=Shared");
        anchorConnection.Open();
        using LuminaDbContext context = CreateContext(anchorConnection);
        ConsumptionStatisticsRepository sut = new(context);

        Guid userId = Guid.NewGuid();
        MediaConsumptionRecordEntity record = _mediaConsumptionRecordEntityFixture.Create(userId: userId);
        context.Users.Add(_userEntityFixture.Create(id: userId));
        context.MediaConsumptionRecords.Add(record);
        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        // Act
        Result<IReadOnlyList<MediaConsumptionRecordEntity>> firstResult = await sut.GetForUserAsync(userId, null, null, null, null, CancellationToken.None);
        Result<IReadOnlyList<MediaConsumptionRecordEntity>> secondResult = await sut.GetForUserAsync(userId, null, null, null, null, CancellationToken.None);

        // Assert
        Assert.False(firstResult.IsFailure);
        Assert.False(secondResult.IsFailure);
        // The read is untracked, so the context never holds the returned statistics records, and a second read works the same way.
        Assert.Empty(context.ChangeTracker.Entries<MediaConsumptionRecordEntity>());
        Assert.Equal(EntityState.Detached, context.Entry(firstResult.Value.Single()).State);
        MediaConsumptionRecordEntity firstReturnedRecord = Assert.Single(firstResult.Value);
        MediaConsumptionRecordEntity secondReturnedRecord = Assert.Single(secondResult.Value);
        Assert.Equal(record.Id, firstReturnedRecord.Id);
        Assert.Equal(record.Id, secondReturnedRecord.Id);
        Assert.Empty(context.ChangeTracker.Entries<MediaConsumptionRecordEntity>());
    }

    [Fact]
    public async Task GetForAllUsersAsync_WhenCalled_ShouldReturnTheRecordsOfEveryUser()
    {
        // Arrange
        using SqliteConnection anchorConnection = new($"Data Source=luminadataccess-consumptionstats-allusers-{Guid.NewGuid()};Mode=Memory;Cache=Shared");
        anchorConnection.Open();
        using LuminaDbContext context = CreateContext(anchorConnection);
        ConsumptionStatisticsRepository sut = new(context);

        Guid firstUserId = Guid.NewGuid();
        Guid secondUserId = Guid.NewGuid();
        MediaConsumptionRecordEntity firstUserRecord = _mediaConsumptionRecordEntityFixture.Create(userId: firstUserId, mediaKind: MediaKind.Movie);
        MediaConsumptionRecordEntity secondUserMovieRecord = _mediaConsumptionRecordEntityFixture.Create(userId: secondUserId, mediaKind: MediaKind.Movie);
        MediaConsumptionRecordEntity secondUserBookRecord = _mediaConsumptionRecordEntityFixture.Create(userId: secondUserId, mediaKind: MediaKind.Book);
        context.Users.AddRange(_userEntityFixture.Create(id: firstUserId), _userEntityFixture.Create(id: secondUserId));
        context.MediaConsumptionRecords.AddRange(firstUserRecord, secondUserMovieRecord, secondUserBookRecord);
        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        // Act
        Result<IReadOnlyList<MediaConsumptionRecordEntity>> result = await sut.GetForAllUsersAsync(null, null, null, null, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(3, result.Value.Count);
        Assert.Contains(result.Value, record => record.Id == firstUserRecord.Id);
        Assert.Contains(result.Value, record => record.Id == secondUserMovieRecord.Id);
        Assert.Contains(result.Value, record => record.Id == secondUserBookRecord.Id);
    }

    [Fact]
    public async Task GetForAllUsersAsync_WhenAllFiltersAreProvided_ShouldApplyEveryFilterAcrossUsers()
    {
        // Arrange
        using SqliteConnection anchorConnection = new($"Data Source=luminadataccess-consumptionstats-allusers-filters-{Guid.NewGuid()};Mode=Memory;Cache=Shared");
        anchorConnection.Open();
        using LuminaDbContext context = CreateContext(anchorConnection);
        ConsumptionStatisticsRepository sut = new(context);

        Guid firstUserId = Guid.NewGuid();
        Guid secondUserId = Guid.NewGuid();
        Guid libraryId = Guid.NewGuid();
        DateTime fromUtc = new(2025, 1, 10, 0, 0, 0, DateTimeKind.Utc);
        DateTime toUtc = new(2025, 1, 20, 0, 0, 0, DateTimeKind.Utc);
        MediaConsumptionRecordEntity matchingFirstUserRecord = _mediaConsumptionRecordEntityFixture.Create(
            userId: firstUserId, libraryId: libraryId, mediaKind: MediaKind.Movie, startedOnUtc: new DateTime(2025, 1, 15, 0, 0, 0, DateTimeKind.Utc));
        MediaConsumptionRecordEntity matchingSecondUserRecord = _mediaConsumptionRecordEntityFixture.Create(
            userId: secondUserId, libraryId: libraryId, mediaKind: MediaKind.Movie, startedOnUtc: new DateTime(2025, 1, 16, 0, 0, 0, DateTimeKind.Utc));
        MediaConsumptionRecordEntity recordOfAnotherLibrary = _mediaConsumptionRecordEntityFixture.Create(
            userId: firstUserId, libraryId: Guid.NewGuid(), mediaKind: MediaKind.Movie, startedOnUtc: new DateTime(2025, 1, 15, 0, 0, 0, DateTimeKind.Utc));
        MediaConsumptionRecordEntity recordOfAnotherKind = _mediaConsumptionRecordEntityFixture.Create(
            userId: secondUserId, libraryId: libraryId, mediaKind: MediaKind.Book, startedOnUtc: new DateTime(2025, 1, 15, 0, 0, 0, DateTimeKind.Utc));
        MediaConsumptionRecordEntity recordOutsideTheRange = _mediaConsumptionRecordEntityFixture.Create(
            userId: secondUserId, libraryId: libraryId, mediaKind: MediaKind.Movie, startedOnUtc: toUtc.AddDays(1));
        context.Users.AddRange(_userEntityFixture.Create(id: firstUserId), _userEntityFixture.Create(id: secondUserId));
        context.MediaConsumptionRecords.AddRange(
            matchingFirstUserRecord, matchingSecondUserRecord, recordOfAnotherLibrary, recordOfAnotherKind, recordOutsideTheRange);
        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        // Act
        Result<IReadOnlyList<MediaConsumptionRecordEntity>> result = await sut.GetForAllUsersAsync(
            libraryId, MediaKind.Movie, fromUtc, toUtc, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(2, result.Value.Count);
        // The filters are applied across every user, instead of one user at a time.
        Assert.Contains(result.Value, record => record.Id == matchingFirstUserRecord.Id);
        Assert.Contains(result.Value, record => record.Id == matchingSecondUserRecord.Id);
    }

    [Fact]
    public async Task GetForAllUsersAsync_WhenNothingMatches_ShouldReturnAnEmptyList()
    {
        // Arrange
        using SqliteConnection anchorConnection = new($"Data Source=luminadataccess-consumptionstats-allusers-empty-{Guid.NewGuid()};Mode=Memory;Cache=Shared");
        anchorConnection.Open();
        using LuminaDbContext context = CreateContext(anchorConnection);
        ConsumptionStatisticsRepository sut = new(context);

        Guid userId = Guid.NewGuid();
        MediaConsumptionRecordEntity record = _mediaConsumptionRecordEntityFixture.Create(
            userId: userId, libraryId: Guid.NewGuid(), mediaKind: MediaKind.Book, startedOnUtc: new DateTime(2025, 1, 15, 0, 0, 0, DateTimeKind.Utc));
        context.Users.Add(_userEntityFixture.Create(id: userId));
        context.MediaConsumptionRecords.Add(record);
        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        // Act
        Result<IReadOnlyList<MediaConsumptionRecordEntity>> result = await sut.GetForAllUsersAsync(
            Guid.NewGuid(), MediaKind.Movie, new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 1, 31, 0, 0, 0, DateTimeKind.Utc), CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.NotNull(result.Value);
        Assert.Empty(result.Value);
    }

    /// <summary>
    /// Creates a real SQLite backed context, so that the stored rows are read exactly as they were written.
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
}
