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
/// Contains integration tests for the <see cref="MediaResumePointRepository"/> class, exercising it against a real SQLite database.
/// </summary>
[ExcludeFromCodeCoverage]
public class MediaResumePointRepositoryTests
{
    private readonly MediaResumePointEntityFixture _mediaResumePointEntityFixture = new();
    private readonly UserEntityFixture _userEntityFixture = new();

    [Fact]
    public async Task UpsertForUserAsync_WhenTheResumePointDoesNotExist_ShouldInsertItWithTheAuthoritativeOwner()
    {
        // Arrange
        using SqliteConnection anchorConnection = new($"Data Source=luminadataccess-mediaresumerepo-insert-{Guid.NewGuid()};Mode=Memory;Cache=Shared");
        anchorConnection.Open();
        Guid userId = Guid.NewGuid();
        DateTime utcNow = new(2025, 1, 1, 12, 0, 0, DateTimeKind.Utc);
        using LuminaDbContext context = CreateAuditedContext(anchorConnection, userId, utcNow);
        MediaResumePointRepository sut = new(context);
        // The owner carried by the incoming resume point is deliberately different from the authoritative owner argument.
        MediaResumePointEntity incoming = _mediaResumePointEntityFixture.Create(
            userId: Guid.NewGuid(),
            libraryId: Guid.NewGuid(),
            mediaKind: MediaKind.Movie,
            mediaItemId: Guid.NewGuid(),
            lastPositionKind: MediaPositionKind.TimeOffset,
            lastPositionTimeOffsetMilliseconds: 120_000,
            includeLastPositionLocation: false,
            lastProgressFraction: 0.25,
            version: 2);
        context.Users.Add(_userEntityFixture.Create(id: userId));

        // Act
        Result<Updated> result = await sut.UpsertForUserAsync(incoming, userId, CancellationToken.None);
        await context.SaveChangesAsync();

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(Result.Updated, result.Value);
        using LuminaDbContext verificationContext = CreateContext(anchorConnection);
        MediaResumePointEntity storedResumePoint = await verificationContext.MediaResumePoints
            .AsNoTracking()
            .SingleAsync();
        Assert.Equal(userId, storedResumePoint.UserId);
        Assert.Equal(incoming.LibraryId, storedResumePoint.LibraryId);
        Assert.Equal(MediaKind.Movie, storedResumePoint.MediaKind);
        Assert.Equal(incoming.MediaItemId, storedResumePoint.MediaItemId);
        Assert.Equal(MediaPositionKind.TimeOffset, storedResumePoint.LastPositionKind);
        Assert.Equal(120_000L, storedResumePoint.LastPositionTimeOffsetMilliseconds);
        Assert.Null(storedResumePoint.LastPositionLocation);
        Assert.Equal(0.25, storedResumePoint.LastProgressFraction);
        Assert.Equal(2L, storedResumePoint.Version);
        Assert.Equal(utcNow, storedResumePoint.CreatedOnUtc);
        Assert.Equal(userId, storedResumePoint.CreatedBy);
        Assert.Null(storedResumePoint.UpdatedOnUtc);
        Assert.Null(storedResumePoint.UpdatedBy);
    }

    [Fact]
    public async Task UpsertForUserAsync_WhenCalledTwiceForTheSameMediaItem_ShouldUpdateTheExistingRow()
    {
        // Arrange
        using SqliteConnection anchorConnection = new($"Data Source=luminadataccess-mediaresumerepo-upsert-{Guid.NewGuid()};Mode=Memory;Cache=Shared");
        anchorConnection.Open();
        Guid userId = Guid.NewGuid();
        DateTime utcNow = new(2025, 1, 1, 12, 0, 0, DateTimeKind.Utc);
        DateTime laterUtcNow = new(2025, 1, 1, 13, 0, 0, DateTimeKind.Utc);
        Guid mediaItemId = Guid.NewGuid();
        Guid resumePointId;
        long firstVersion;
        Guid libraryId;
        using (LuminaDbContext seedContext = CreateAuditedContext(anchorConnection, userId, utcNow))
        {
            MediaResumePointRepository seedSut = new(seedContext);
            seedContext.Users.Add(_userEntityFixture.Create(id: userId));
            MediaResumePointEntity firstIncoming = _mediaResumePointEntityFixture.Create(
                userId: userId,
                libraryId: Guid.NewGuid(),
                mediaKind: MediaKind.Movie,
                mediaItemId: mediaItemId,
                lastPositionKind: MediaPositionKind.TimeOffset,
                lastPositionTimeOffsetMilliseconds: 60_000,
                includeLastPositionLocation: false,
                lastProgressFraction: 0.25,
                version: 4);
            Result<Updated> firstResult = await seedSut.UpsertForUserAsync(firstIncoming, userId, CancellationToken.None);
            Assert.False(firstResult.IsFailure);
            await seedContext.SaveChangesAsync();
            resumePointId = firstIncoming.Id;
            firstVersion = firstIncoming.Version;
            libraryId = firstIncoming.LibraryId;
        }
        using LuminaDbContext context = CreateAuditedContext(anchorConnection, userId, laterUtcNow);
        MediaResumePointRepository sut = new(context);
        MediaResumePointEntity secondIncoming = _mediaResumePointEntityFixture.Create(
            id: Guid.NewGuid(),
            userId: userId,
            libraryId: Guid.NewGuid(),
            mediaKind: MediaKind.Movie,
            mediaItemId: mediaItemId,
            lastPositionKind: MediaPositionKind.TimeOffset,
            lastPositionTimeOffsetMilliseconds: 300_000,
            includeLastPositionLocation: false,
            lastProgressFraction: 0.75,
            version: firstVersion);

        // Act
        Result<Updated> result = await sut.UpsertForUserAsync(secondIncoming, userId, CancellationToken.None);
        await context.SaveChangesAsync();

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(Result.Updated, result.Value);
        using LuminaDbContext verificationContext = CreateContext(anchorConnection);
        MediaResumePointEntity storedResumePoint = await verificationContext.MediaResumePoints
            .AsNoTracking()
            .SingleAsync();
        // The unique business key keeps the row count at one and preserves the stored identity and creation audit.
        Assert.Equal(resumePointId, storedResumePoint.Id);
        Assert.Equal(userId, storedResumePoint.UserId);
        Assert.Equal(MediaKind.Movie, storedResumePoint.MediaKind);
        Assert.Equal(mediaItemId, storedResumePoint.MediaItemId);
        Assert.Equal(utcNow, storedResumePoint.CreatedOnUtc);
        Assert.Equal(userId, storedResumePoint.CreatedBy);
        // The editable values are advanced by the second save.
        Assert.Equal(secondIncoming.LibraryId, storedResumePoint.LibraryId);
        Assert.NotEqual(libraryId, storedResumePoint.LibraryId);
        Assert.Equal(300_000L, storedResumePoint.LastPositionTimeOffsetMilliseconds);
        Assert.Equal(0.75, storedResumePoint.LastProgressFraction);
        Assert.Equal(firstVersion + 1, storedResumePoint.Version);
        Assert.Equal(laterUtcNow, storedResumePoint.UpdatedOnUtc);
        Assert.Equal(userId, storedResumePoint.UpdatedBy);
    }

    [Fact]
    public async Task UpsertForUserAsync_WhenADuplicateIsInsertedDirectly_ShouldBeRejectedByTheUniqueIndex()
    {
        // Arrange
        using SqliteConnection anchorConnection = new($"Data Source=luminadataccess-mediaresumerepo-duplicate-{Guid.NewGuid()};Mode=Memory;Cache=Shared");
        anchorConnection.Open();
        Guid userId = Guid.NewGuid();
        DateTime utcNow = new(2025, 1, 1, 12, 0, 0, DateTimeKind.Utc);
        using LuminaDbContext context = CreateAuditedContext(anchorConnection, userId, utcNow);
        context.Users.Add(_userEntityFixture.Create(id: userId));
        Guid mediaItemId = Guid.NewGuid();
        MediaResumePointEntity storedResumePoint = _mediaResumePointEntityFixture.Create(
            userId: userId,
            mediaKind: MediaKind.Movie,
            mediaItemId: mediaItemId);
        context.MediaResumePoints.Add(storedResumePoint);
        await context.SaveChangesAsync();
        MediaResumePointEntity duplicateResumePoint = _mediaResumePointEntityFixture.Create(
            userId: userId,
            mediaKind: MediaKind.Movie,
            mediaItemId: mediaItemId);
        context.MediaResumePoints.Add(duplicateResumePoint);

        // Act
        DbUpdateException exception = await Assert.ThrowsAsync<DbUpdateException>(() => context.SaveChangesAsync());

        // Assert
        Assert.NotNull(exception);
        using LuminaDbContext verificationContext = CreateContext(anchorConnection);
        Assert.Equal(1, await verificationContext.MediaResumePoints.AsNoTracking().CountAsync(resumePoint => resumePoint.UserId == userId && resumePoint.MediaItemId == mediaItemId));
    }

    [Fact]
    public async Task UpsertForUserAsync_WhenNothingChanged_ShouldNotBumpTheVersionOrStampTheAudit()
    {
        // Arrange
        using SqliteConnection anchorConnection = new($"Data Source=luminadataccess-mediaresumerepo-noop-{Guid.NewGuid()};Mode=Memory;Cache=Shared");
        anchorConnection.Open();
        Guid userId = Guid.NewGuid();
        DateTime utcNow = new(2025, 1, 1, 12, 0, 0, DateTimeKind.Utc);
        DateTime laterUtcNow = new(2025, 1, 1, 13, 0, 0, DateTimeKind.Utc);
        Guid mediaItemId = Guid.NewGuid();
        long storedVersion;
        using (LuminaDbContext seedContext = CreateAuditedContext(anchorConnection, userId, utcNow))
        {
            seedContext.Users.Add(_userEntityFixture.Create(id: userId));
            MediaResumePointEntity storedResumePoint = _mediaResumePointEntityFixture.Create(
                userId: userId,
                mediaKind: MediaKind.Movie,
                mediaItemId: mediaItemId,
                lastPositionKind: MediaPositionKind.TimeOffset,
                lastPositionTimeOffsetMilliseconds: 90_000,
                includeLastPositionLocation: false,
                lastProgressFraction: 0.4,
                version: 3);
            seedContext.MediaResumePoints.Add(storedResumePoint);
            await seedContext.SaveChangesAsync();
            storedVersion = storedResumePoint.Version;
        }
        using LuminaDbContext context = CreateAuditedContext(anchorConnection, userId, laterUtcNow);
        MediaResumePointRepository sut = new(context);
        // A detached copy is the shape the command mapping produces for a save, so this is a real no-op round-trip.
        MediaResumePointEntity incoming = await context.MediaResumePoints
            .AsNoTracking()
            .SingleAsync(resumePoint => resumePoint.UserId == userId && resumePoint.MediaItemId == mediaItemId);

        // Act
        Result<Updated> result = await sut.UpsertForUserAsync(incoming, userId, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(Result.Updated, result.Value);
        Assert.DoesNotContain(context.ChangeTracker.Entries<MediaResumePointEntity>(), entry => entry.State == EntityState.Modified);

        await context.SaveChangesAsync();
        MediaResumePointEntity storedResumePointAfterSave = await context.MediaResumePoints
            .AsNoTracking()
            .SingleAsync(resumePoint => resumePoint.UserId == userId && resumePoint.MediaItemId == mediaItemId);
        Assert.Equal(storedVersion, storedResumePointAfterSave.Version);
        Assert.Equal(0.4, storedResumePointAfterSave.LastProgressFraction);
        Assert.Null(storedResumePointAfterSave.UpdatedOnUtc);
        Assert.Null(storedResumePointAfterSave.UpdatedBy);
    }

    [Fact]
    public async Task GetForUserAndMediaItemAsync_WhenTheResumePointMatchesUserKindAndItem_ShouldReturnTheStoredResumePoint()
    {
        // Arrange
        using SqliteConnection anchorConnection = new($"Data Source=luminadataccess-mediaresumerepo-get-{Guid.NewGuid()};Mode=Memory;Cache=Shared");
        anchorConnection.Open();
        Guid userId = Guid.NewGuid();
        DateTime utcNow = new(2025, 1, 1, 12, 0, 0, DateTimeKind.Utc);
        using LuminaDbContext context = CreateAuditedContext(anchorConnection, userId, utcNow);
        MediaResumePointRepository sut = new(context);
        context.Users.Add(_userEntityFixture.Create(id: userId));
        Guid mediaItemId = Guid.NewGuid();
        MediaResumePointEntity resumePoint = _mediaResumePointEntityFixture.Create(
            userId: userId,
            mediaKind: MediaKind.Movie,
            mediaItemId: mediaItemId,
            lastProgressFraction: 0.6);
        context.MediaResumePoints.Add(resumePoint);
        await context.SaveChangesAsync();

        // Act
        Result<MediaResumePointEntity?> result = await sut.GetForUserAndMediaItemAsync(userId, MediaKind.Movie, mediaItemId, cancellationToken: CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.NotNull(result.Value);
        Assert.Equal(resumePoint.Id, result.Value!.Id);
        Assert.Equal(userId, result.Value.UserId);
        Assert.Equal(MediaKind.Movie, result.Value.MediaKind);
        Assert.Equal(mediaItemId, result.Value.MediaItemId);
        Assert.Equal(0.6, result.Value.LastProgressFraction);
    }

    [Fact]
    public async Task GetForUserAndMediaItemAsync_WhenTheUserKindOrItemDoesNotMatch_ShouldReturnNull()
    {
        // Arrange
        using SqliteConnection anchorConnection = new($"Data Source=luminadataccess-mediaresumerepo-get-miss-{Guid.NewGuid()};Mode=Memory;Cache=Shared");
        anchorConnection.Open();
        Guid userId = Guid.NewGuid();
        DateTime utcNow = new(2025, 1, 1, 12, 0, 0, DateTimeKind.Utc);
        using LuminaDbContext context = CreateAuditedContext(anchorConnection, userId, utcNow);
        MediaResumePointRepository sut = new(context);
        context.Users.Add(_userEntityFixture.Create(id: userId));
        Guid mediaItemId = Guid.NewGuid();
        MediaResumePointEntity resumePoint = _mediaResumePointEntityFixture.Create(
            userId: userId,
            mediaKind: MediaKind.Movie,
            mediaItemId: mediaItemId);
        context.MediaResumePoints.Add(resumePoint);
        await context.SaveChangesAsync();

        // Act
        Result<MediaResumePointEntity?> otherUserResult = await sut.GetForUserAndMediaItemAsync(Guid.NewGuid(), MediaKind.Movie, mediaItemId, cancellationToken: CancellationToken.None);
        Result<MediaResumePointEntity?> otherKindResult = await sut.GetForUserAndMediaItemAsync(userId, MediaKind.Book, mediaItemId, cancellationToken: CancellationToken.None);
        Result<MediaResumePointEntity?> otherItemResult = await sut.GetForUserAndMediaItemAsync(userId, MediaKind.Movie, Guid.NewGuid(), cancellationToken: CancellationToken.None);

        // Assert
        Assert.False(otherUserResult.IsFailure);
        Assert.Null(otherUserResult.Value);
        Assert.False(otherKindResult.IsFailure);
        Assert.Null(otherKindResult.Value);
        Assert.False(otherItemResult.IsFailure);
        Assert.Null(otherItemResult.Value);
    }

    [Fact]
    public async Task GetForUserAndMediaItemAsync_WhenEntitiesShouldNotBeTracked_ShouldReturnADetachedInstance()
    {
        // Arrange
        using SqliteConnection anchorConnection = new($"Data Source=luminadataccess-mediaresumerepo-get-detached-{Guid.NewGuid()};Mode=Memory;Cache=Shared");
        anchorConnection.Open();
        Guid userId = Guid.NewGuid();
        DateTime utcNow = new(2025, 1, 1, 12, 0, 0, DateTimeKind.Utc);
        using LuminaDbContext context = CreateAuditedContext(anchorConnection, userId, utcNow);
        MediaResumePointRepository sut = new(context);
        context.Users.Add(_userEntityFixture.Create(id: userId));
        Guid mediaItemId = Guid.NewGuid();
        MediaResumePointEntity resumePoint = _mediaResumePointEntityFixture.Create(
            userId: userId,
            mediaKind: MediaKind.Movie,
            mediaItemId: mediaItemId);
        context.MediaResumePoints.Add(resumePoint);
        await context.SaveChangesAsync();

        // Act
        Result<MediaResumePointEntity?> result = await sut.GetForUserAndMediaItemAsync(userId, MediaKind.Movie, mediaItemId, shouldTrackEntities: false, cancellationToken: CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.NotNull(result.Value);
        Assert.False(ReferenceEquals(resumePoint, result.Value));
        Assert.Equal(EntityState.Detached, context.Entry(result.Value!).State);
        Assert.Equal(EntityState.Unchanged, context.Entry(resumePoint).State);
    }

    [Fact]
    public async Task DeleteForUserAndMediaItemAsync_WhenTheResumePointMatches_ShouldDeleteOnlyThatRow()
    {
        // Arrange
        using SqliteConnection anchorConnection = new($"Data Source=luminadataccess-mediaresumerepo-delete-{Guid.NewGuid()};Mode=Memory;Cache=Shared");
        anchorConnection.Open();
        Guid userId = Guid.NewGuid();
        Guid otherUserId = Guid.NewGuid();
        DateTime utcNow = new(2025, 1, 1, 12, 0, 0, DateTimeKind.Utc);
        using LuminaDbContext context = CreateAuditedContext(anchorConnection, userId, utcNow);
        MediaResumePointRepository sut = new(context);
        context.Users.AddRange(_userEntityFixture.Create(id: userId), _userEntityFixture.Create(id: otherUserId));
        Guid mediaItemId = Guid.NewGuid();
        Guid otherMediaItemId = Guid.NewGuid();
        MediaResumePointEntity resumePoint = _mediaResumePointEntityFixture.Create(
            userId: userId,
            mediaKind: MediaKind.Movie,
            mediaItemId: mediaItemId);
        MediaResumePointEntity otherUserResumePoint = _mediaResumePointEntityFixture.Create(
            userId: otherUserId,
            mediaKind: MediaKind.Movie,
            mediaItemId: mediaItemId);
        MediaResumePointEntity otherItemResumePoint = _mediaResumePointEntityFixture.Create(
            userId: userId,
            mediaKind: MediaKind.Movie,
            mediaItemId: otherMediaItemId);
        context.MediaResumePoints.AddRange(resumePoint, otherUserResumePoint, otherItemResumePoint);
        await context.SaveChangesAsync();

        // Act
        Result<Deleted> result = await sut.DeleteForUserAndMediaItemAsync(userId, MediaKind.Movie, mediaItemId, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(Result.Deleted, result.Value);

        await context.SaveChangesAsync();
        using LuminaDbContext verificationContext = CreateContext(anchorConnection);
        List<Guid> remainingResumePointIds = await verificationContext.MediaResumePoints
            .AsNoTracking()
            .Select(candidate => candidate.Id)
            .ToListAsync();
        Assert.Equal(
            new[] { otherUserResumePoint.Id, otherItemResumePoint.Id }.OrderBy(id => id),
            remainingResumePointIds.OrderBy(id => id));
    }

    [Fact]
    public async Task DeleteAllForUserAsync_WhenTheUserHasResumePoints_ShouldRemoveOnlyTheirResumePoints()
    {
        // Arrange
        using SqliteConnection anchorConnection = new($"Data Source=luminadataccess-mediaresumerepo-delete-user-{Guid.NewGuid()};Mode=Memory;Cache=Shared");
        anchorConnection.Open();
        Guid userId = Guid.NewGuid();
        Guid otherUserId = Guid.NewGuid();
        DateTime utcNow = new(2025, 1, 1, 12, 0, 0, DateTimeKind.Utc);
        using LuminaDbContext context = CreateAuditedContext(anchorConnection, userId, utcNow);
        MediaResumePointRepository sut = new(context);
        context.Users.AddRange(_userEntityFixture.Create(id: userId), _userEntityFixture.Create(id: otherUserId));
        MediaResumePointEntity firstResumePoint = _mediaResumePointEntityFixture.Create(userId: userId, libraryId: Guid.NewGuid());
        MediaResumePointEntity secondResumePoint = _mediaResumePointEntityFixture.Create(userId: userId, libraryId: Guid.NewGuid());
        MediaResumePointEntity otherUserResumePoint = _mediaResumePointEntityFixture.Create(userId: otherUserId);
        context.MediaResumePoints.AddRange(firstResumePoint, secondResumePoint, otherUserResumePoint);
        await context.SaveChangesAsync();

        // Act
        Result<Deleted> result = await sut.DeleteAllForUserAsync(userId, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(Result.Deleted, result.Value);

        await context.SaveChangesAsync();
        using LuminaDbContext verificationContext = CreateContext(anchorConnection);
        List<Guid> remainingResumePointIds = await verificationContext.MediaResumePoints
            .AsNoTracking()
            .Select(candidate => candidate.Id)
            .ToListAsync();
        Assert.Equal([otherUserResumePoint.Id], remainingResumePointIds);
    }

    [Fact]
    public async Task DeleteAllForUserInLibraryAsync_WhenTheUserHasResumePointsInTheLibrary_ShouldRemoveOnlyThoseResumePoints()
    {
        // Arrange
        using SqliteConnection anchorConnection = new($"Data Source=luminadataccess-mediaresumerepo-delete-library-{Guid.NewGuid()};Mode=Memory;Cache=Shared");
        anchorConnection.Open();
        Guid userId = Guid.NewGuid();
        Guid otherUserId = Guid.NewGuid();
        DateTime utcNow = new(2025, 1, 1, 12, 0, 0, DateTimeKind.Utc);
        using LuminaDbContext context = CreateAuditedContext(anchorConnection, userId, utcNow);
        MediaResumePointRepository sut = new(context);
        context.Users.AddRange(_userEntityFixture.Create(id: userId), _userEntityFixture.Create(id: otherUserId));
        Guid libraryId = Guid.NewGuid();
        MediaResumePointEntity matchingResumePoint = _mediaResumePointEntityFixture.Create(userId: userId, libraryId: libraryId);
        MediaResumePointEntity otherLibraryResumePoint = _mediaResumePointEntityFixture.Create(userId: userId, libraryId: Guid.NewGuid());
        MediaResumePointEntity otherUserResumePoint = _mediaResumePointEntityFixture.Create(userId: otherUserId, libraryId: libraryId);
        context.MediaResumePoints.AddRange(matchingResumePoint, otherLibraryResumePoint, otherUserResumePoint);
        await context.SaveChangesAsync();

        // Act
        Result<Deleted> result = await sut.DeleteAllForUserInLibraryAsync(userId, libraryId, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(Result.Deleted, result.Value);

        await context.SaveChangesAsync();
        using LuminaDbContext verificationContext = CreateContext(anchorConnection);
        List<Guid> remainingResumePointIds = await verificationContext.MediaResumePoints
            .AsNoTracking()
            .Select(candidate => candidate.Id)
            .ToListAsync();
        Assert.Equal(
            new[] { otherLibraryResumePoint.Id, otherUserResumePoint.Id }.OrderBy(id => id),
            remainingResumePointIds.OrderBy(id => id));
    }

    [Fact]
    public async Task UpsertForUserAsync_WhenAnotherContextAlreadyBumpedTheVersion_ShouldThrowDbUpdateConcurrencyException()
    {
        // Arrange
        using SqliteConnection anchorConnection = new($"Data Source=luminadataccess-mediaresumerepo-concurrency-{Guid.NewGuid()};Mode=Memory;Cache=Shared");
        anchorConnection.Open();
        Guid userId = Guid.NewGuid();
        DateTime utcNow = new(2025, 1, 1, 12, 0, 0, DateTimeKind.Utc);
        DateTime laterUtcNow = new(2025, 1, 1, 13, 0, 0, DateTimeKind.Utc);
        Guid mediaItemId = Guid.NewGuid();
        using (LuminaDbContext seedContext = CreateAuditedContext(anchorConnection, userId, utcNow))
        {
            seedContext.Users.Add(_userEntityFixture.Create(id: userId));
            MediaResumePointEntity storedResumePoint = _mediaResumePointEntityFixture.Create(
                userId: userId,
                mediaKind: MediaKind.Movie,
                mediaItemId: mediaItemId,
                lastProgressFraction: 0.2,
                version: 4);
            seedContext.MediaResumePoints.Add(storedResumePoint);
            await seedContext.SaveChangesAsync();
        }
        using LuminaDbContext firstContext = CreateAuditedContext(anchorConnection, userId, utcNow);
        MediaResumePointRepository firstSut = new(firstContext);
        Result<MediaResumePointEntity?> readResult = await firstSut.GetForUserAndMediaItemAsync(userId, MediaKind.Movie, mediaItemId, cancellationToken: CancellationToken.None);
        Assert.False(readResult.IsFailure);
        Assert.NotNull(readResult.Value);
        using LuminaDbContext secondContext = CreateAuditedContext(anchorConnection, userId, laterUtcNow);
        MediaResumePointRepository secondSut = new(secondContext);
        MediaResumePointEntity secondIncoming = _mediaResumePointEntityFixture.Create(
            userId: userId,
            mediaKind: MediaKind.Movie,
            mediaItemId: mediaItemId,
            lastProgressFraction: 0.8,
            version: 4);
        Result<Updated> secondResult = await secondSut.UpsertForUserAsync(secondIncoming, userId, CancellationToken.None);
        Assert.False(secondResult.IsFailure);
        await secondContext.SaveChangesAsync();
        // The first context still holds the instance it read before the second writer committed its own version.
        MediaResumePointEntity firstIncoming = _mediaResumePointEntityFixture.Create(
            userId: userId,
            mediaKind: MediaKind.Movie,
            mediaItemId: mediaItemId,
            lastProgressFraction: 1.0,
            version: 4);
        Result<Updated> firstResult = await firstSut.UpsertForUserAsync(firstIncoming, userId, CancellationToken.None);
        Assert.False(firstResult.IsFailure);

        // Act
        DbUpdateConcurrencyException exception = await Assert.ThrowsAsync<DbUpdateConcurrencyException>(() => firstContext.SaveChangesAsync());

        // Assert
        Assert.NotNull(exception);
        secondContext.ChangeTracker.Clear();
        MediaResumePointEntity storedResumePointAfterSave = await secondContext.MediaResumePoints
            .AsNoTracking()
            .SingleAsync(resumePoint => resumePoint.UserId == userId && resumePoint.MediaItemId == mediaItemId);
        Assert.Equal(0.8, storedResumePointAfterSave.LastProgressFraction);
        Assert.Equal(5L, storedResumePointAfterSave.Version);
        Assert.Equal(laterUtcNow, storedResumePointAfterSave.UpdatedOnUtc);
    }

    [Fact]
    public async Task UserDeletion_WhenTheUserOwnsResumePoints_ShouldCascadeToTheResumePoints()
    {
        // Arrange
        using SqliteConnection anchorConnection = new($"Data Source=luminadataccess-mediaresumerepo-cascade-{Guid.NewGuid()};Mode=Memory;Cache=Shared");
        anchorConnection.Open();
        Guid userId = Guid.NewGuid();
        DateTime utcNow = new(2025, 1, 1, 12, 0, 0, DateTimeKind.Utc);
        using LuminaDbContext context = CreateAuditedContext(anchorConnection, userId, utcNow);
        UserEntity user = _userEntityFixture.Create(id: userId);
        context.Users.Add(user);
        MediaResumePointEntity firstResumePoint = _mediaResumePointEntityFixture.Create(userId: userId);
        MediaResumePointEntity secondResumePoint = _mediaResumePointEntityFixture.Create(userId: userId);
        context.MediaResumePoints.AddRange(firstResumePoint, secondResumePoint);
        await context.SaveChangesAsync();
        // A real deletion starts with nothing tracked, so the database cascade is the path that has to remove the resume points.
        context.ChangeTracker.Clear();

        // Act
        UserEntity storedUser = await context.Users.SingleAsync(candidate => candidate.Id == userId);
        context.Users.Remove(storedUser);
        await context.SaveChangesAsync();

        // Assert
        using LuminaDbContext verificationContext = CreateContext(anchorConnection);
        Assert.False(await verificationContext.MediaResumePoints.AsNoTracking().AnyAsync(resumePoint => resumePoint.UserId == userId));
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
}
