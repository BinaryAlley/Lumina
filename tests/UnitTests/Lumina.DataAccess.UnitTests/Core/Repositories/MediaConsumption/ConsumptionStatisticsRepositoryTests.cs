#region ========================================================================= USING =====================================================================================
using EntityFrameworkCore.Testing.NSubstitute;
using Lumina.Application.Common.DataAccess.Entities.MediaConsumption;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.MediaConsumption;
using Lumina.DataAccess.Core.Repositories.MediaConsumption;
using Lumina.DataAccess.Core.UoW;
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.SharedKernel.Common.Enums.MediaLibrary;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.DataAccess.UnitTests.Core.Repositories.MediaConsumption;

/// <summary>
/// Contains unit tests for the <see cref="ConsumptionStatisticsRepository"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class ConsumptionStatisticsRepositoryTests
{
    private readonly LuminaDbContext _mockContext;
    private readonly ConsumptionStatisticsRepository _sut;
    private readonly MediaConsumptionRecordEntityFixture _mediaConsumptionRecordEntityFixture = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="ConsumptionStatisticsRepositoryTests"/> class.
    /// </summary>
    public ConsumptionStatisticsRepositoryTests()
    {
        _mockContext = Create.MockedDbContextFor<LuminaDbContext>();
        _sut = new ConsumptionStatisticsRepository(_mockContext);
    }

    [Fact]
    public async Task GetForUserAsync_WhenCalled_ShouldReturnOnlyTheRecordsOfTheUser()
    {
        // Arrange
        Guid userId = Guid.NewGuid();
        MediaConsumptionRecordEntity firstRecord = _mediaConsumptionRecordEntityFixture.Create(userId: userId);
        MediaConsumptionRecordEntity secondRecord = _mediaConsumptionRecordEntityFixture.Create(userId: userId);
        MediaConsumptionRecordEntity recordOfAnotherUser = _mediaConsumptionRecordEntityFixture.Create(userId: Guid.NewGuid());
        _mockContext.MediaConsumptionRecords.AddRange(firstRecord, secondRecord, recordOfAnotherUser);
        await _mockContext.SaveChangesAsync();

        // Act
        Result<IReadOnlyList<MediaConsumptionRecordEntity>> result = await _sut.GetForUserAsync(userId, null, null, null, null, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(2, result.Value.Count);
        Assert.All(result.Value, record => Assert.Equal(userId, record.UserId));
        Assert.DoesNotContain(result.Value, record => record.Id == recordOfAnotherUser.Id);
    }

    [Fact]
    public async Task GetForUserAsync_WhenNoFilterIsProvided_ShouldReturnEveryRecordOfTheUser()
    {
        // Arrange
        Guid userId = Guid.NewGuid();
        MediaConsumptionRecordEntity movieRecord = _mediaConsumptionRecordEntityFixture.Create(
            userId: userId,
            libraryId: Guid.NewGuid(),
            mediaKind: MediaKind.Movie,
            startedOnUtc: new DateTime(2024, 6, 1, 10, 0, 0, DateTimeKind.Utc));
        MediaConsumptionRecordEntity bookRecord = _mediaConsumptionRecordEntityFixture.Create(
            userId: userId,
            libraryId: Guid.NewGuid(),
            mediaKind: MediaKind.Book,
            startedOnUtc: new DateTime(2025, 6, 1, 10, 0, 0, DateTimeKind.Utc));
        MediaConsumptionRecordEntity trackRecord = _mediaConsumptionRecordEntityFixture.Create(
            userId: userId,
            libraryId: Guid.NewGuid(),
            mediaKind: MediaKind.MusicTrack,
            startedOnUtc: new DateTime(2026, 6, 1, 10, 0, 0, DateTimeKind.Utc));
        _mockContext.MediaConsumptionRecords.AddRange(movieRecord, bookRecord, trackRecord);
        await _mockContext.SaveChangesAsync();

        // Act
        Result<IReadOnlyList<MediaConsumptionRecordEntity>> result = await _sut.GetForUserAsync(userId, null, null, null, null, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(3, result.Value.Count);
        Assert.Equal(
            new[] { movieRecord.Id, bookRecord.Id, trackRecord.Id }.OrderBy(id => id),
            result.Value.Select(record => record.Id).OrderBy(id => id));
    }

    [Fact]
    public async Task GetForUserAsync_WhenLibraryFilterIsProvided_ShouldReturnOnlyTheRecordsOfThatLibrary()
    {
        // Arrange
        Guid userId = Guid.NewGuid();
        Guid libraryId = Guid.NewGuid();
        MediaConsumptionRecordEntity firstRecordOfLibrary = _mediaConsumptionRecordEntityFixture.Create(userId: userId, libraryId: libraryId);
        MediaConsumptionRecordEntity secondRecordOfLibrary = _mediaConsumptionRecordEntityFixture.Create(userId: userId, libraryId: libraryId);
        MediaConsumptionRecordEntity recordOfAnotherLibrary = _mediaConsumptionRecordEntityFixture.Create(userId: userId, libraryId: Guid.NewGuid());
        MediaConsumptionRecordEntity recordOfAnotherUserInTheLibrary = _mediaConsumptionRecordEntityFixture.Create(userId: Guid.NewGuid(), libraryId: libraryId);
        _mockContext.MediaConsumptionRecords.AddRange(
            firstRecordOfLibrary, secondRecordOfLibrary, recordOfAnotherLibrary, recordOfAnotherUserInTheLibrary);
        await _mockContext.SaveChangesAsync();

        // Act
        Result<IReadOnlyList<MediaConsumptionRecordEntity>> result = await _sut.GetForUserAsync(userId, libraryId, null, null, null, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(2, result.Value.Count);
        Assert.All(result.Value, record => Assert.Equal(libraryId, record.LibraryId));
        Assert.DoesNotContain(result.Value, record => record.Id == recordOfAnotherLibrary.Id);
        Assert.DoesNotContain(result.Value, record => record.Id == recordOfAnotherUserInTheLibrary.Id);
    }

    [Fact]
    public async Task GetForUserAsync_WhenMediaKindFilterIsProvided_ShouldReturnOnlyTheRecordsOfThatKind()
    {
        // Arrange
        Guid userId = Guid.NewGuid();
        MediaConsumptionRecordEntity movieRecord = _mediaConsumptionRecordEntityFixture.Create(userId: userId, mediaKind: MediaKind.Movie);
        MediaConsumptionRecordEntity anotherMovieRecord = _mediaConsumptionRecordEntityFixture.Create(userId: userId, mediaKind: MediaKind.Movie);
        MediaConsumptionRecordEntity bookRecord = _mediaConsumptionRecordEntityFixture.Create(userId: userId, mediaKind: MediaKind.Book);
        MediaConsumptionRecordEntity recordOfAnotherUser = _mediaConsumptionRecordEntityFixture.Create(userId: Guid.NewGuid(), mediaKind: MediaKind.Movie);
        _mockContext.MediaConsumptionRecords.AddRange(movieRecord, anotherMovieRecord, bookRecord, recordOfAnotherUser);
        await _mockContext.SaveChangesAsync();

        // Act
        Result<IReadOnlyList<MediaConsumptionRecordEntity>> result = await _sut.GetForUserAsync(userId, null, MediaKind.Movie, null, null, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(2, result.Value.Count);
        Assert.All(result.Value, record => Assert.Equal(MediaKind.Movie, record.MediaKind));
        Assert.DoesNotContain(result.Value, record => record.Id == bookRecord.Id);
        Assert.DoesNotContain(result.Value, record => record.Id == recordOfAnotherUser.Id);
    }

    [Fact]
    public async Task GetForUserAsync_WhenFromUtcIsProvided_ShouldReturnOnlyTheRecordsStartedAtOrAfterIt()
    {
        // Arrange
        Guid userId = Guid.NewGuid();
        DateTime fromUtc = new(2025, 1, 10, 12, 0, 0, DateTimeKind.Utc);
        MediaConsumptionRecordEntity recordBeforeTheRange = _mediaConsumptionRecordEntityFixture.Create(
            userId: userId, startedOnUtc: fromUtc.AddMilliseconds(-1));
        MediaConsumptionRecordEntity recordAtTheBoundary = _mediaConsumptionRecordEntityFixture.Create(
            userId: userId, startedOnUtc: fromUtc);
        MediaConsumptionRecordEntity recordAfterTheBoundary = _mediaConsumptionRecordEntityFixture.Create(
            userId: userId, startedOnUtc: fromUtc.AddDays(1));
        _mockContext.MediaConsumptionRecords.AddRange(recordBeforeTheRange, recordAtTheBoundary, recordAfterTheBoundary);
        await _mockContext.SaveChangesAsync();

        // Act
        Result<IReadOnlyList<MediaConsumptionRecordEntity>> result = await _sut.GetForUserAsync(userId, null, null, fromUtc, null, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        // The boundary is inclusive, in the same way the indexed range scan of the storage medium treats it.
        Assert.Equal(2, result.Value.Count);
        Assert.Contains(result.Value, record => record.Id == recordAtTheBoundary.Id);
        Assert.Contains(result.Value, record => record.Id == recordAfterTheBoundary.Id);
        Assert.DoesNotContain(result.Value, record => record.Id == recordBeforeTheRange.Id);
    }

    [Fact]
    public async Task GetForUserAsync_WhenToUtcIsProvided_ShouldReturnOnlyTheRecordsStartedAtOrBeforeIt()
    {
        // Arrange
        Guid userId = Guid.NewGuid();
        DateTime toUtc = new(2025, 1, 20, 12, 0, 0, DateTimeKind.Utc);
        MediaConsumptionRecordEntity recordBeforeTheBoundary = _mediaConsumptionRecordEntityFixture.Create(
            userId: userId, startedOnUtc: toUtc.AddDays(-1));
        MediaConsumptionRecordEntity recordAtTheBoundary = _mediaConsumptionRecordEntityFixture.Create(
            userId: userId, startedOnUtc: toUtc);
        MediaConsumptionRecordEntity recordAfterTheRange = _mediaConsumptionRecordEntityFixture.Create(
            userId: userId, startedOnUtc: toUtc.AddMilliseconds(1));
        _mockContext.MediaConsumptionRecords.AddRange(recordBeforeTheBoundary, recordAtTheBoundary, recordAfterTheRange);
        await _mockContext.SaveChangesAsync();

        // Act
        Result<IReadOnlyList<MediaConsumptionRecordEntity>> result = await _sut.GetForUserAsync(userId, null, null, null, toUtc, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        // The boundary is inclusive, in the same way the indexed range scan of the storage medium treats it.
        Assert.Equal(2, result.Value.Count);
        Assert.Contains(result.Value, record => record.Id == recordBeforeTheBoundary.Id);
        Assert.Contains(result.Value, record => record.Id == recordAtTheBoundary.Id);
        Assert.DoesNotContain(result.Value, record => record.Id == recordAfterTheRange.Id);
    }

    [Fact]
    public async Task GetForUserAsync_WhenAllFiltersAreProvided_ShouldApplyEveryFilter()
    {
        // Arrange
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
        MediaConsumptionRecordEntity recordBeforeTheRange = _mediaConsumptionRecordEntityFixture.Create(
            userId: userId, libraryId: libraryId, mediaKind: MediaKind.Movie, startedOnUtc: fromUtc.AddDays(-1));
        MediaConsumptionRecordEntity recordAfterTheRange = _mediaConsumptionRecordEntityFixture.Create(
            userId: userId, libraryId: libraryId, mediaKind: MediaKind.Movie, startedOnUtc: toUtc.AddDays(1));
        MediaConsumptionRecordEntity matchingRecordOfAnotherUser = _mediaConsumptionRecordEntityFixture.Create(
            userId: Guid.NewGuid(), libraryId: libraryId, mediaKind: MediaKind.Movie, startedOnUtc: new DateTime(2025, 1, 15, 0, 0, 0, DateTimeKind.Utc));
        _mockContext.MediaConsumptionRecords.AddRange(
            matchingRecord, recordOfAnotherLibrary, recordOfAnotherKind, recordBeforeTheRange, recordAfterTheRange, matchingRecordOfAnotherUser);
        await _mockContext.SaveChangesAsync();

        // Act
        Result<IReadOnlyList<MediaConsumptionRecordEntity>> result = await _sut.GetForUserAsync(userId, libraryId, MediaKind.Movie, fromUtc, toUtc, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        MediaConsumptionRecordEntity returnedRecord = Assert.Single(result.Value);
        Assert.Equal(matchingRecord.Id, returnedRecord.Id);
    }

    [Fact]
    public async Task GetForUserAsync_WhenNothingMatches_ShouldReturnAnEmptyList()
    {
        // Arrange
        Guid userId = Guid.NewGuid();
        MediaConsumptionRecordEntity record = _mediaConsumptionRecordEntityFixture.Create(
            userId: userId, libraryId: Guid.NewGuid(), mediaKind: MediaKind.Book, startedOnUtc: new DateTime(2025, 1, 15, 0, 0, 0, DateTimeKind.Utc));
        _mockContext.MediaConsumptionRecords.Add(record);
        await _mockContext.SaveChangesAsync();

        // Act
        Result<IReadOnlyList<MediaConsumptionRecordEntity>> result = await _sut.GetForUserAsync(
            userId, Guid.NewGuid(), MediaKind.Movie, new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 1, 31, 0, 0, 0, DateTimeKind.Utc), CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.NotNull(result.Value);
        Assert.Empty(result.Value);
    }

    [Fact]
    public async Task GetForAllUsersAsync_WhenCalled_ShouldReturnTheRecordsOfEveryUser()
    {
        // Arrange
        Guid firstUserId = Guid.NewGuid();
        Guid secondUserId = Guid.NewGuid();
        MediaConsumptionRecordEntity firstUserRecord = _mediaConsumptionRecordEntityFixture.Create(userId: firstUserId, mediaKind: MediaKind.Movie);
        MediaConsumptionRecordEntity secondUserMovieRecord = _mediaConsumptionRecordEntityFixture.Create(userId: secondUserId, mediaKind: MediaKind.Movie);
        MediaConsumptionRecordEntity secondUserBookRecord = _mediaConsumptionRecordEntityFixture.Create(userId: secondUserId, mediaKind: MediaKind.Book);
        _mockContext.MediaConsumptionRecords.AddRange(firstUserRecord, secondUserMovieRecord, secondUserBookRecord);
        await _mockContext.SaveChangesAsync();

        // Act
        Result<IReadOnlyList<MediaConsumptionRecordEntity>> result = await _sut.GetForAllUsersAsync(null, null, null, null, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(3, result.Value.Count);
        Assert.Contains(result.Value, record => record.Id == firstUserRecord.Id);
        Assert.Contains(result.Value, record => record.Id == secondUserMovieRecord.Id);
        Assert.Contains(result.Value, record => record.Id == secondUserBookRecord.Id);
    }

    [Fact]
    public async Task GetForAllUsersAsync_WhenLibraryFilterIsProvided_ShouldReturnTheRecordsOfThatLibraryOfEveryUser()
    {
        // Arrange
        Guid libraryId = Guid.NewGuid();
        MediaConsumptionRecordEntity firstUserRecord = _mediaConsumptionRecordEntityFixture.Create(userId: Guid.NewGuid(), libraryId: libraryId);
        MediaConsumptionRecordEntity secondUserRecord = _mediaConsumptionRecordEntityFixture.Create(userId: Guid.NewGuid(), libraryId: libraryId);
        MediaConsumptionRecordEntity otherLibraryRecord = _mediaConsumptionRecordEntityFixture.Create(userId: Guid.NewGuid(), libraryId: Guid.NewGuid());
        _mockContext.MediaConsumptionRecords.AddRange(firstUserRecord, secondUserRecord, otherLibraryRecord);
        await _mockContext.SaveChangesAsync();

        // Act
        Result<IReadOnlyList<MediaConsumptionRecordEntity>> result = await _sut.GetForAllUsersAsync(libraryId, null, null, null, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(2, result.Value.Count);
        Assert.All(result.Value, record => Assert.Equal(libraryId, record.LibraryId));
        // The filter is applied across every user, instead of one user at a time.
        Assert.Contains(result.Value, record => record.UserId == firstUserRecord.UserId);
        Assert.Contains(result.Value, record => record.UserId == secondUserRecord.UserId);
        Assert.DoesNotContain(result.Value, record => record.Id == otherLibraryRecord.Id);
    }

    [Fact]
    public async Task GetForAllUsersAsync_WhenMediaKindFilterIsProvided_ShouldReturnTheRecordsOfThatKindOfEveryUser()
    {
        // Arrange
        MediaConsumptionRecordEntity firstUserMovieRecord = _mediaConsumptionRecordEntityFixture.Create(userId: Guid.NewGuid(), mediaKind: MediaKind.Movie);
        MediaConsumptionRecordEntity secondUserMovieRecord = _mediaConsumptionRecordEntityFixture.Create(userId: Guid.NewGuid(), mediaKind: MediaKind.Movie);
        MediaConsumptionRecordEntity bookRecord = _mediaConsumptionRecordEntityFixture.Create(userId: Guid.NewGuid(), mediaKind: MediaKind.Book);
        _mockContext.MediaConsumptionRecords.AddRange(firstUserMovieRecord, secondUserMovieRecord, bookRecord);
        await _mockContext.SaveChangesAsync();

        // Act
        Result<IReadOnlyList<MediaConsumptionRecordEntity>> result = await _sut.GetForAllUsersAsync(null, MediaKind.Movie, null, null, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(2, result.Value.Count);
        Assert.All(result.Value, record => Assert.Equal(MediaKind.Movie, record.MediaKind));
        Assert.Contains(result.Value, record => record.UserId == firstUserMovieRecord.UserId);
        Assert.Contains(result.Value, record => record.UserId == secondUserMovieRecord.UserId);
        Assert.DoesNotContain(result.Value, record => record.Id == bookRecord.Id);
    }

    [Fact]
    public async Task GetForAllUsersAsync_WhenFromUtcIsProvided_ShouldReturnOnlyTheRecordsStartedAtOrAfterIt()
    {
        // Arrange
        DateTime fromUtc = new(2025, 1, 10, 12, 0, 0, DateTimeKind.Utc);
        MediaConsumptionRecordEntity recordBeforeTheRange = _mediaConsumptionRecordEntityFixture.Create(
            userId: Guid.NewGuid(), startedOnUtc: fromUtc.AddMilliseconds(-1));
        MediaConsumptionRecordEntity recordAtTheBoundary = _mediaConsumptionRecordEntityFixture.Create(
            userId: Guid.NewGuid(), startedOnUtc: fromUtc);
        MediaConsumptionRecordEntity recordAfterTheBoundary = _mediaConsumptionRecordEntityFixture.Create(
            userId: Guid.NewGuid(), startedOnUtc: fromUtc.AddDays(1));
        _mockContext.MediaConsumptionRecords.AddRange(recordBeforeTheRange, recordAtTheBoundary, recordAfterTheBoundary);
        await _mockContext.SaveChangesAsync();

        // Act
        Result<IReadOnlyList<MediaConsumptionRecordEntity>> result = await _sut.GetForAllUsersAsync(null, null, fromUtc, null, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(2, result.Value.Count);
        Assert.Contains(result.Value, record => record.Id == recordAtTheBoundary.Id);
        Assert.Contains(result.Value, record => record.Id == recordAfterTheBoundary.Id);
        Assert.DoesNotContain(result.Value, record => record.Id == recordBeforeTheRange.Id);
    }

    [Fact]
    public async Task GetForAllUsersAsync_WhenToUtcIsProvided_ShouldReturnOnlyTheRecordsStartedAtOrBeforeIt()
    {
        // Arrange
        DateTime toUtc = new(2025, 1, 20, 12, 0, 0, DateTimeKind.Utc);
        MediaConsumptionRecordEntity recordBeforeTheBoundary = _mediaConsumptionRecordEntityFixture.Create(
            userId: Guid.NewGuid(), startedOnUtc: toUtc.AddDays(-1));
        MediaConsumptionRecordEntity recordAtTheBoundary = _mediaConsumptionRecordEntityFixture.Create(
            userId: Guid.NewGuid(), startedOnUtc: toUtc);
        MediaConsumptionRecordEntity recordAfterTheRange = _mediaConsumptionRecordEntityFixture.Create(
            userId: Guid.NewGuid(), startedOnUtc: toUtc.AddMilliseconds(1));
        _mockContext.MediaConsumptionRecords.AddRange(recordBeforeTheBoundary, recordAtTheBoundary, recordAfterTheRange);
        await _mockContext.SaveChangesAsync();

        // Act
        Result<IReadOnlyList<MediaConsumptionRecordEntity>> result = await _sut.GetForAllUsersAsync(null, null, null, toUtc, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(2, result.Value.Count);
        Assert.Contains(result.Value, record => record.Id == recordBeforeTheBoundary.Id);
        Assert.Contains(result.Value, record => record.Id == recordAtTheBoundary.Id);
        Assert.DoesNotContain(result.Value, record => record.Id == recordAfterTheRange.Id);
    }

    [Fact]
    public async Task GetForAllUsersAsync_WhenAllFiltersAreProvided_ShouldApplyEveryFilterAcrossUsers()
    {
        // Arrange
        Guid libraryId = Guid.NewGuid();
        DateTime fromUtc = new(2025, 1, 10, 0, 0, 0, DateTimeKind.Utc);
        DateTime toUtc = new(2025, 1, 20, 0, 0, 0, DateTimeKind.Utc);
        MediaConsumptionRecordEntity matchingFirstUserRecord = _mediaConsumptionRecordEntityFixture.Create(
            userId: Guid.NewGuid(), libraryId: libraryId, mediaKind: MediaKind.Movie, startedOnUtc: new DateTime(2025, 1, 15, 0, 0, 0, DateTimeKind.Utc));
        MediaConsumptionRecordEntity matchingSecondUserRecord = _mediaConsumptionRecordEntityFixture.Create(
            userId: Guid.NewGuid(), libraryId: libraryId, mediaKind: MediaKind.Movie, startedOnUtc: new DateTime(2025, 1, 16, 0, 0, 0, DateTimeKind.Utc));
        MediaConsumptionRecordEntity recordOfAnotherLibrary = _mediaConsumptionRecordEntityFixture.Create(
            userId: Guid.NewGuid(), libraryId: Guid.NewGuid(), mediaKind: MediaKind.Movie, startedOnUtc: new DateTime(2025, 1, 15, 0, 0, 0, DateTimeKind.Utc));
        MediaConsumptionRecordEntity recordOfAnotherKind = _mediaConsumptionRecordEntityFixture.Create(
            userId: Guid.NewGuid(), libraryId: libraryId, mediaKind: MediaKind.Book, startedOnUtc: new DateTime(2025, 1, 15, 0, 0, 0, DateTimeKind.Utc));
        MediaConsumptionRecordEntity recordOutsideTheRange = _mediaConsumptionRecordEntityFixture.Create(
            userId: Guid.NewGuid(), libraryId: libraryId, mediaKind: MediaKind.Movie, startedOnUtc: toUtc.AddDays(1));
        _mockContext.MediaConsumptionRecords.AddRange(
            matchingFirstUserRecord, matchingSecondUserRecord, recordOfAnotherLibrary, recordOfAnotherKind, recordOutsideTheRange);
        await _mockContext.SaveChangesAsync();

        // Act
        Result<IReadOnlyList<MediaConsumptionRecordEntity>> result = await _sut.GetForAllUsersAsync(libraryId, MediaKind.Movie, fromUtc, toUtc, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(2, result.Value.Count);
        Assert.Contains(result.Value, record => record.Id == matchingFirstUserRecord.Id);
        Assert.Contains(result.Value, record => record.Id == matchingSecondUserRecord.Id);
    }

    [Fact]
    public async Task GetForAllUsersAsync_WhenNothingMatches_ShouldReturnAnEmptyList()
    {
        // Arrange
        MediaConsumptionRecordEntity record = _mediaConsumptionRecordEntityFixture.Create(
            userId: Guid.NewGuid(), libraryId: Guid.NewGuid(), mediaKind: MediaKind.Book, startedOnUtc: new DateTime(2025, 1, 15, 0, 0, 0, DateTimeKind.Utc));
        _mockContext.MediaConsumptionRecords.Add(record);
        await _mockContext.SaveChangesAsync();

        // Act
        Result<IReadOnlyList<MediaConsumptionRecordEntity>> result = await _sut.GetForAllUsersAsync(
            Guid.NewGuid(), MediaKind.Movie, new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 1, 31, 0, 0, 0, DateTimeKind.Utc), CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.NotNull(result.Value);
        Assert.Empty(result.Value);
    }
}
