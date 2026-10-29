#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.Authorization;
using Lumina.Application.Common.DataAccess.Entities.Favorites;
using Lumina.Application.Common.DataAccess.Entities.MediaLibrary.WrittenContentLibrary.BookLibrary;
using Lumina.Application.Common.DataAccess.Entities.UsersManagement;
using Lumina.Application.Common.DataAccess.Repositories.Favorites;
using Lumina.Application.Common.Infrastructure.Authentication;
using Lumina.Application.Common.Infrastructure.Time;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.Authorization;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.Favorites;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.MediaLibrary.WrittenContentLibrary.BookLibrary;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.UsersManagement;
using Lumina.DataAccess.Common.Interceptors;
using Lumina.DataAccess.Core.UoW;
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.SharedKernel.Common.Enums.Favorites;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using NSubstitute;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ApplicationErrors = Lumina.Application.Common.Errors.Errors;
#endregion

namespace Lumina.DataAccess.IntegrationTests.Core.UoW;

/// <summary>
/// Contains integration tests for the <see cref="UnitOfWork"/> class, exercising its transaction handling against a real SQLite database.
/// </summary>
[ExcludeFromCodeCoverage]
public class UnitOfWorkTests
{
    private readonly RoleEntityFixture _roleEntityFixture = new();
    private readonly BookEntityFixture _bookEntityFixture = new();
    private readonly FavoriteEntityFixture _favoriteEntityFixture = new();
    private readonly UserEntityFixture _userEntityFixture = new();

    [Fact]
    public async Task BeginTransactionAsync_WhenCalled_ShouldBeginTransactionOnTheDatabase()
    {
        // Arrange
        (SqliteConnection anchorConnection, LuminaDbContext dbContext) = CreateSqliteContext();
        using (anchorConnection)
        {
            using (dbContext)
            {
                UnitOfWork sut = new(dbContext);

                // Act
                await sut.BeginTransactionAsync(CancellationToken.None);

                // Assert
                Assert.NotNull(dbContext.Database.CurrentTransaction);
            }
        }
    }

    [Fact]
    public async Task CommitTransactionAsync_WhenTransactionIsActive_ShouldPersistChangesAndClearCurrentTransaction()
    {
        // Arrange
        (SqliteConnection anchorConnection, LuminaDbContext dbContext) = CreateSqliteContext();
        using (anchorConnection)
        {
            using (dbContext)
            {
                UnitOfWork sut = new(dbContext);
                RoleEntity role = _roleEntityFixture.Create(roleName: "Admin");

                // Act
                await sut.BeginTransactionAsync(CancellationToken.None);
                dbContext.Roles.Add(role);
                await sut.SaveChangesAsync(CancellationToken.None);
                await sut.CommitTransactionAsync(CancellationToken.None);

                // Assert
                Assert.Null(dbContext.Database.CurrentTransaction);
                dbContext.ChangeTracker.Clear();
                RoleEntity? persistedRole = await dbContext.Roles.AsNoTracking().FirstOrDefaultAsync(existingRole => existingRole.Id == role.Id);
                Assert.NotNull(persistedRole);
                Assert.Equal(role.RoleName, persistedRole!.RoleName);
            }
        }
    }

    [Fact]
    public async Task CommitTransactionAsync_WhenNoTransactionIsActive_ShouldCompleteWithoutError()
    {
        // Arrange
        (SqliteConnection anchorConnection, LuminaDbContext dbContext) = CreateSqliteContext();
        using (anchorConnection)
        {
            using (dbContext)
            {
                UnitOfWork sut = new(dbContext);

                // Act
                await sut.CommitTransactionAsync(CancellationToken.None);

                // Assert
                Assert.Null(dbContext.Database.CurrentTransaction);
            }
        }
    }

    [Fact]
    public async Task RollbackTransactionAsync_WhenTransactionIsActive_ShouldDiscardChangesAndClearCurrentTransaction()
    {
        // Arrange
        (SqliteConnection anchorConnection, LuminaDbContext dbContext) = CreateSqliteContext();
        using (anchorConnection)
        {
            using (dbContext)
            {
                UnitOfWork sut = new(dbContext);
                RoleEntity role = _roleEntityFixture.Create(roleName: "Admin");

                // Act
                await sut.BeginTransactionAsync(CancellationToken.None);
                dbContext.Roles.Add(role);
                await sut.SaveChangesAsync(CancellationToken.None);
                await sut.RollbackTransactionAsync(CancellationToken.None);

                // Assert
                Assert.Null(dbContext.Database.CurrentTransaction);
                dbContext.ChangeTracker.Clear();
                RoleEntity? persistedRole = await dbContext.Roles.AsNoTracking().FirstOrDefaultAsync(existingRole => existingRole.Id == role.Id);
                Assert.Null(persistedRole);
            }
        }
    }

    [Fact]
    public async Task RollbackTransactionAsync_WhenNoTransactionIsActive_ShouldCompleteWithoutError()
    {
        // Arrange
        (SqliteConnection anchorConnection, LuminaDbContext dbContext) = CreateSqliteContext();
        using (anchorConnection)
        {
            using (dbContext)
            {
                UnitOfWork sut = new(dbContext);

                // Act
                await sut.RollbackTransactionAsync(CancellationToken.None);

                // Assert
                Assert.Null(dbContext.Database.CurrentTransaction);
            }
        }
    }

    [Fact]
    public async Task Dispose_WhenTransactionIsActive_ShouldDisposeTheTransactionAndTheDbContext()
    {
        // Arrange
        (SqliteConnection anchorConnection, LuminaDbContext dbContext) = CreateSqliteContext();
        using (anchorConnection)
        {
            using (dbContext)
            {
                UnitOfWork sut = new(dbContext);

                // Act
                await sut.BeginTransactionAsync(CancellationToken.None);
                sut.Dispose();

                // Assert
                Assert.Throws<ObjectDisposedException>(() => dbContext.Roles.Count());
            }
        }
    }

    [Fact]
    public void Dispose_WhenNoTransactionIsActive_ShouldDisposeTheDbContext()
    {
        // Arrange
        (SqliteConnection anchorConnection, LuminaDbContext dbContext) = CreateSqliteContext();
        using (anchorConnection)
        {
            using (dbContext)
            {
                UnitOfWork sut = new(dbContext);

                // Act
                sut.Dispose();

                // Assert
                Assert.Throws<ObjectDisposedException>(() => dbContext.Roles.Count());
            }
        }
    }

    [Fact]
    public async Task SaveChangesAsync_WhenInsertingTwoBooksWithTheSameLibraryAndPath_ShouldReturnUniqueConstraintViolationError()
    {
        // Arrange
        (SqliteConnection anchorConnection, LuminaDbContext dbContext) = CreateSqliteContext();
        using (anchorConnection)
        {
            using (dbContext)
            {
                UnitOfWork sut = new(dbContext);
                Guid libraryId = Guid.NewGuid();
                BookEntity firstBook = _bookEntityFixture.Create();
                firstBook.LibraryId = libraryId;
                firstBook.Path = "/books/duplicate.epub";
                _ = await sut.BookRepository.InsertAsync(firstBook, CancellationToken.None);
                Result<Success> firstSaveResult = await sut.SaveChangesAsync(CancellationToken.None);
                Assert.False(firstSaveResult.IsFailure);

                // stage a second book with the same library and path directly, bypassing the application level pre-check, so that the unique index of the storage medium is the one that rejects it
                BookEntity secondBook = _bookEntityFixture.Create();
                secondBook.LibraryId = libraryId;
                secondBook.Path = "/books/duplicate.epub";
                dbContext.Books.Add(secondBook);

                // Act
                Result<Success> result = await sut.SaveChangesAsync(CancellationToken.None);

                // Assert
                Assert.True(result.IsFailure);
                Assert.Equal(ApplicationErrors.Persistence.UniqueConstraintViolation, result.FirstError);
            }
        }
    }

    [Fact]
    public void FavoriteRepository_WhenAccessedRepeatedly_ShouldReturnTheSameCachedInstance()
    {
        // Arrange
        (SqliteConnection anchorConnection, LuminaDbContext dbContext) = CreateSqliteContext();
        using (anchorConnection)
        {
            using (dbContext)
            {
                UnitOfWork sut = new(dbContext);

                // Act
                IFavoriteRepository firstRepository = sut.FavoriteRepository;
                IFavoriteRepository secondRepository = sut.FavoriteRepository;

                // Assert
                Assert.NotNull(firstRepository);
                Assert.Same(firstRepository, secondRepository);
            }
        }
    }

    [Fact]
    public async Task SaveChangesAsync_WhenInsertingTwoFavoritesForTheSameUserKindAndMediaItem_ShouldReturnUniqueConstraintViolationError()
    {
        // Arrange
        (SqliteConnection anchorConnection, LuminaDbContext dbContext) = CreateSqliteContext();
        using (anchorConnection)
        {
            using (dbContext)
            {
                UnitOfWork sut = new(dbContext);
                Guid userId = Guid.NewGuid();
                dbContext.Users.Add(_userEntityFixture.Create(id: userId));
                Result<Success> userSaveResult = await sut.SaveChangesAsync(CancellationToken.None);
                Assert.False(userSaveResult.IsFailure);
                Guid mediaItemId = Guid.NewGuid();
                FavoriteEntity firstFavorite = _favoriteEntityFixture.Create(
                    userId: userId,
                    mediaKind: FavoriteMediaKind.MusicArtist,
                    mediaItemId: mediaItemId,
                    includeUpdatedOnUtc: false,
                    includeUpdatedBy: false);
                _ = await sut.FavoriteRepository.InsertAsync(firstFavorite, CancellationToken.None);
                Result<Success> firstSaveResult = await sut.SaveChangesAsync(CancellationToken.None);
                Assert.False(firstSaveResult.IsFailure);

                // stage a duplicate directly through the repository, bypassing the application level existence check, so that the unique index of the storage medium is the one that rejects it
                FavoriteEntity duplicateFavorite = _favoriteEntityFixture.Create(
                    userId: userId,
                    mediaKind: FavoriteMediaKind.MusicArtist,
                    mediaItemId: mediaItemId,
                    includeUpdatedOnUtc: false,
                    includeUpdatedBy: false);
                _ = await sut.FavoriteRepository.InsertAsync(duplicateFavorite, CancellationToken.None);

                // Act
                Result<Success> result = await sut.SaveChangesAsync(CancellationToken.None);

                // Assert
                Assert.True(result.IsFailure);
                Assert.Equal(ApplicationErrors.Persistence.UniqueConstraintViolation, result.FirstError);
            }
        }
    }

    [Fact]
    public async Task ClearTrackedEntities_WhenEntitiesAreTracked_ShouldDetachEveryEntity()
    {
        // Arrange
        (SqliteConnection anchorConnection, LuminaDbContext dbContext) = CreateSqliteContext();
        using (anchorConnection)
        {
            using (dbContext)
            {
                UnitOfWork sut = new(dbContext);
                Guid userId = Guid.NewGuid();
                UserEntity user = _userEntityFixture.Create(id: userId);
                FavoriteEntity favorite = _favoriteEntityFixture.Create(
                    userId: userId,
                    mediaKind: FavoriteMediaKind.MusicTrack,
                    includeUpdatedOnUtc: false,
                    includeUpdatedBy: false);
                dbContext.Users.Add(user);
                dbContext.Favorites.Add(favorite);
                await dbContext.SaveChangesAsync();
                Assert.Equal(2, dbContext.ChangeTracker.Entries().Count());

                // Act
                sut.ClearTrackedEntities();

                // Assert
                Assert.Empty(dbContext.ChangeTracker.Entries());
                Assert.Equal(EntityState.Detached, dbContext.Entry(favorite).State);
            }
        }
    }

    [Fact]
    public async Task SaveChangesAsync_WhenInsertingAValidFavorite_ShouldPersistEveryFieldThroughTheAuditInterceptor()
    {
        // Arrange
        Guid userId = Guid.NewGuid();
        DateTime utcNow = new(2025, 1, 1, 12, 0, 0, DateTimeKind.Utc);
        (SqliteConnection anchorConnection, LuminaDbContext dbContext) = CreateAuditedSqliteContext(userId, utcNow);
        using (anchorConnection)
        {
            using (dbContext)
            {
                UnitOfWork sut = new(dbContext);
                dbContext.Users.Add(_userEntityFixture.Create(id: userId));
                Guid mediaItemId = Guid.NewGuid();
                // The fixture populates the update audit columns, so the test also proves the interceptor owns them on insert.
                FavoriteEntity favorite = _favoriteEntityFixture.Create(
                    userId: userId,
                    mediaKind: FavoriteMediaKind.Book,
                    mediaItemId: mediaItemId,
                    createdOnUtc: DateTime.MinValue,
                    createdBy: Guid.Empty,
                    updatedOnUtc: DateTime.MaxValue,
                    updatedBy: Guid.NewGuid(),
                    includeUpdatedOnUtc: true,
                    includeUpdatedBy: true);
                Result<Created> insertResult = await sut.FavoriteRepository.InsertAsync(favorite, CancellationToken.None);
                Assert.False(insertResult.IsFailure);

                // Act
                Result<Success> result = await sut.SaveChangesAsync(CancellationToken.None);

                // Assert
                Assert.False(result.IsFailure);
                dbContext.ChangeTracker.Clear();
                FavoriteEntity storedFavorite = await dbContext.Favorites.AsNoTracking().SingleAsync(candidate => candidate.Id == favorite.Id);
                Assert.Equal(userId, storedFavorite.UserId);
                Assert.Equal(FavoriteMediaKind.Book, storedFavorite.MediaKind);
                Assert.Equal(mediaItemId, storedFavorite.MediaItemId);
                Assert.Equal(utcNow, storedFavorite.CreatedOnUtc);
                Assert.Equal(userId, storedFavorite.CreatedBy);
                Assert.Null(storedFavorite.UpdatedOnUtc);
                Assert.Null(storedFavorite.UpdatedBy);
            }
        }
    }

    /// <summary>
    /// Creates a real SQLite in-memory context, kept alive by an anchor connection, because transactions are not supported by the in-memory provider.
    /// </summary>
    /// <returns>A tuple containing the anchor connection and the database context.</returns>
    private static (SqliteConnection AnchorConnection, LuminaDbContext Context) CreateSqliteContext()
    {
        SqliteConnection anchorConnection = new($"Data Source=luminadataccess-unitofwork-tests-{Guid.NewGuid()};Mode=Memory;Cache=Shared");
        anchorConnection.Open();
        LuminaDbContext dbContext = new(new DbContextOptionsBuilder<LuminaDbContext>().UseSqlite(anchorConnection.ConnectionString).Options);
        dbContext.Database.EnsureCreated();
        return (anchorConnection, dbContext);
    }

    /// <summary>
    /// Creates a real SQLite in-memory context with the auditing interceptor attached, so that audit columns behave exactly like in production.
    /// </summary>
    /// <param name="userId">The Id of the user reported as the current user.</param>
    /// <param name="utcNow">The current UTC time reported by the time provider.</param>
    /// <returns>A tuple containing the anchor connection and the database context.</returns>
    private static (SqliteConnection AnchorConnection, LuminaDbContext Context) CreateAuditedSqliteContext(Guid userId, DateTime utcNow)
    {
        SqliteConnection anchorConnection = new($"Data Source=luminadataccess-unitofwork-audit-tests-{Guid.NewGuid()};Mode=Memory;Cache=Shared");
        anchorConnection.Open();
        ICurrentUserService currentUserService = Substitute.For<ICurrentUserService>();
        currentUserService.UserId.Returns(userId);
        IDateTimeProvider dateTimeProvider = Substitute.For<IDateTimeProvider>();
        dateTimeProvider.UtcNow.Returns(utcNow);
        LuminaDbContext dbContext = new(new DbContextOptionsBuilder<LuminaDbContext>()
            .UseSqlite(anchorConnection.ConnectionString)
            .AddInterceptors(new UpdateAuditableEntitiesInterceptor(currentUserService, dateTimeProvider))
            .Options);
        dbContext.Database.EnsureCreated();
        return (anchorConnection, dbContext);
    }
}
