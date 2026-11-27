#region ========================================================================= USING =====================================================================================
using Nemera.Application.Common.DataAccess.Entities.Authorization;
using Nemera.Application.Common.DataAccess.Entities.Common;
using Nemera.Application.Common.DataAccess.Entities.Favorites;
using Nemera.Application.Common.DataAccess.Entities.MediaLibrary.WrittenContentLibrary.BookLibrary;
using Nemera.Application.Common.DataAccess.Entities.UsersManagement;
using Nemera.Application.Common.DataAccess.Repositories.Favorites;
using Nemera.Application.Common.Infrastructure.Authentication;
using Nemera.Application.Common.Infrastructure.Time;
using Nemera.Application.Fixtures.Common.DataAccess.Entities.Authorization;
using Nemera.Application.Fixtures.Common.DataAccess.Entities.Favorites;
using Nemera.Application.Fixtures.Common.DataAccess.Entities.MediaLibrary.WrittenContentLibrary.BookLibrary;
using Nemera.Application.Fixtures.Common.DataAccess.Entities.UsersManagement;
using Nemera.DataAccess.Common.Interceptors;
using Nemera.DataAccess.Core.UoW;
using Nemera.Domain.Common.Primitives;
using Nemera.Domain.SharedKernel.Common.Enums.Favorites;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using NSubstitute;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ApplicationErrors = Nemera.Application.Common.Errors.Errors;
#endregion

namespace Nemera.DataAccess.IntegrationTests.Core.UoW;

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
        (SqliteConnection anchorConnection, NemeraDbContext dbContext) = CreateSqliteContext();
        using (anchorConnection)
        {
            using (dbContext)
            {
                UnitOfWork sut = new(dbContext, Substitute.For<IDateTimeProvider>());

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
        (SqliteConnection anchorConnection, NemeraDbContext dbContext) = CreateSqliteContext();
        using (anchorConnection)
        {
            using (dbContext)
            {
                UnitOfWork sut = new(dbContext, Substitute.For<IDateTimeProvider>());
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
        (SqliteConnection anchorConnection, NemeraDbContext dbContext) = CreateSqliteContext();
        using (anchorConnection)
        {
            using (dbContext)
            {
                UnitOfWork sut = new(dbContext, Substitute.For<IDateTimeProvider>());

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
        (SqliteConnection anchorConnection, NemeraDbContext dbContext) = CreateSqliteContext();
        using (anchorConnection)
        {
            using (dbContext)
            {
                UnitOfWork sut = new(dbContext, Substitute.For<IDateTimeProvider>());
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
        (SqliteConnection anchorConnection, NemeraDbContext dbContext) = CreateSqliteContext();
        using (anchorConnection)
        {
            using (dbContext)
            {
                UnitOfWork sut = new(dbContext, Substitute.For<IDateTimeProvider>());

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
        (SqliteConnection anchorConnection, NemeraDbContext dbContext) = CreateSqliteContext();
        using (anchorConnection)
        {
            using (dbContext)
            {
                UnitOfWork sut = new(dbContext, Substitute.For<IDateTimeProvider>());

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
        (SqliteConnection anchorConnection, NemeraDbContext dbContext) = CreateSqliteContext();
        using (anchorConnection)
        {
            using (dbContext)
            {
                UnitOfWork sut = new(dbContext, Substitute.For<IDateTimeProvider>());

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
        (SqliteConnection anchorConnection, NemeraDbContext dbContext) = CreateSqliteContext();
        using (anchorConnection)
        {
            using (dbContext)
            {
                UnitOfWork sut = new(dbContext, Substitute.For<IDateTimeProvider>());
                Guid libraryId = Guid.NewGuid();
                BookEntity firstBook = _bookEntityFixture.Create();
                firstBook.LibraryId = libraryId;
                firstBook.Path = "/books/duplicate.epub";
                _ = await sut.BookRepository.InsertAsync(firstBook, CancellationToken.None);
                Result<Success> firstSaveResult = await sut.SaveChangesAsync(CancellationToken.None);
                Assert.False(firstSaveResult.IsFailure);

                // Stage a second book with the same library and path directly, bypassing the application level pre-check, so that the unique index of the storage medium is the one that rejects it.
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
        (SqliteConnection anchorConnection, NemeraDbContext dbContext) = CreateSqliteContext();
        using (anchorConnection)
        {
            using (dbContext)
            {
                UnitOfWork sut = new(dbContext, Substitute.For<IDateTimeProvider>());

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
        (SqliteConnection anchorConnection, NemeraDbContext dbContext) = CreateSqliteContext();
        using (anchorConnection)
        {
            using (dbContext)
            {
                UnitOfWork sut = new(dbContext, Substitute.For<IDateTimeProvider>());
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

                // Stage a duplicate directly through the repository, bypassing the application level existence check, so that the unique index of the storage medium is the one that rejects it.
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
        (SqliteConnection anchorConnection, NemeraDbContext dbContext) = CreateSqliteContext();
        using (anchorConnection)
        {
            using (dbContext)
            {
                UnitOfWork sut = new(dbContext, Substitute.For<IDateTimeProvider>());
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
        (SqliteConnection anchorConnection, NemeraDbContext dbContext) = CreateAuditedSqliteContext(userId, utcNow);
        using (anchorConnection)
        {
            using (dbContext)
            {
                UnitOfWork sut = new(dbContext, Substitute.For<IDateTimeProvider>());
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

    [Fact]
    public async Task SaveChangesAsInitializationAsync_WhenExplicitAuditValuesAreProvided_ShouldPreserveThemThroughTheAuditInterceptor()
    {
        // Arrange
        Guid userId = Guid.NewGuid();
        DateTime currentUtcNow = new(2025, 1, 1, 12, 0, 0, DateTimeKind.Utc);
        DateTime initializationUtcNow = new(2025, 1, 2, 12, 0, 0, DateTimeKind.Utc);
        (SqliteConnection anchorConnection, NemeraDbContext dbContext) = CreateAuditedSqliteContext(userId, currentUtcNow);
        using (anchorConnection)
        {
            using (dbContext)
            {
                UnitOfWork sut = new(dbContext, Substitute.For<IDateTimeProvider>());
                RoleEntity role = _roleEntityFixture.Create(roleName: "Admin");
                role.CreatedOnUtc = initializationUtcNow;
                role.CreatedBy = AuditIdentities.s_systemUserId;
                role.UpdatedOnUtc = null;
                role.UpdatedBy = null;
                dbContext.Roles.Add(role);

                // Act
                Result<Success> result = await sut.SaveChangesAsInitializationAsync(CancellationToken.None);

                // Assert
                Assert.False(result.IsFailure);
                dbContext.ChangeTracker.Clear();
                RoleEntity storedRole = await dbContext.Roles.AsNoTracking().SingleAsync(candidate => candidate.Id == role.Id);
                Assert.Equal(initializationUtcNow, storedRole.CreatedOnUtc);
                Assert.Equal(AuditIdentities.s_systemUserId, storedRole.CreatedBy);
                Assert.Null(storedRole.UpdatedOnUtc);
                Assert.Null(storedRole.UpdatedBy);
                Assert.False(dbContext.IsInitializationSave);

                RoleEntity normalRole = _roleEntityFixture.Create(roleName: "NormalRole");
                dbContext.Roles.Add(normalRole);
                Result<Success> normalSaveResult = await sut.SaveChangesAsync(CancellationToken.None);
                Assert.False(normalSaveResult.IsFailure);

                dbContext.ChangeTracker.Clear();
                RoleEntity storedNormalRole = await dbContext.Roles.AsNoTracking().SingleAsync(candidate => candidate.Id == normalRole.Id);
                Assert.Equal(currentUtcNow, storedNormalRole.CreatedOnUtc);
                Assert.Equal(userId, storedNormalRole.CreatedBy);
            }
        }
    }

    [Fact]
    public async Task SaveChangesAsInitializationAsync_WhenSaveFails_ShouldResetTheFlagAndStampTheNextNormalSave()
    {
        // Arrange
        Guid userId = Guid.NewGuid();
        DateTime currentUtcNow = new(2025, 1, 1, 12, 0, 0, DateTimeKind.Utc);
        (SqliteConnection anchorConnection, NemeraDbContext dbContext) = CreateAuditedSqliteContext(userId, currentUtcNow);
        using (anchorConnection)
        {
            using (dbContext)
            {
                UnitOfWork sut = new(dbContext, Substitute.For<IDateTimeProvider>());
                Guid libraryId = Guid.NewGuid();
                BookEntity firstBook = _bookEntityFixture.Create();
                firstBook.LibraryId = libraryId;
                firstBook.Path = "/books/duplicate.epub";
                BookEntity secondBook = _bookEntityFixture.Create();
                secondBook.LibraryId = libraryId;
                secondBook.Path = "/books/duplicate.epub";
                dbContext.Books.AddRange(firstBook, secondBook);

                // Act
                Result<Success> result = await sut.SaveChangesAsInitializationAsync(CancellationToken.None);

                // Assert
                Assert.True(result.IsFailure);
                Assert.Equal(ApplicationErrors.Persistence.UniqueConstraintViolation, result.FirstError);
                Assert.False(dbContext.IsInitializationSave);

                dbContext.ChangeTracker.Clear();
                RoleEntity role = _roleEntityFixture.Create(roleName: "NormalRole");
                dbContext.Roles.Add(role);
                Result<Success> normalSaveResult = await sut.SaveChangesAsync(CancellationToken.None);
                Assert.False(normalSaveResult.IsFailure);

                dbContext.ChangeTracker.Clear();
                RoleEntity storedRole = await dbContext.Roles.AsNoTracking().SingleAsync(candidate => candidate.Id == role.Id);
                Assert.Equal(currentUtcNow, storedRole.CreatedOnUtc);
                Assert.Equal(userId, storedRole.CreatedBy);
            }
        }
    }

    /// <summary>
    /// Creates a real SQLite in-memory context, kept alive by an anchor connection, because transactions are not supported by the in-memory provider.
    /// </summary>
    /// <returns>A tuple containing the anchor connection and the database context.</returns>
    private static (SqliteConnection AnchorConnection, NemeraDbContext Context) CreateSqliteContext()
    {
        SqliteConnection anchorConnection = new($"Data Source=nemeradataccess-unitofwork-tests-{Guid.NewGuid()};Mode=Memory;Cache=Shared");
        anchorConnection.Open();
        NemeraDbContext dbContext = new(new DbContextOptionsBuilder<NemeraDbContext>().UseSqlite(anchorConnection.ConnectionString).Options);
        dbContext.Database.EnsureCreated();
        return (anchorConnection, dbContext);
    }

    /// <summary>
    /// Creates a real SQLite in-memory context with the auditing interceptor attached, so that audit columns behave exactly like in production.
    /// </summary>
    /// <param name="userId">The Id of the user reported as the current user.</param>
    /// <param name="utcNow">The current UTC time reported by the time provider.</param>
    /// <returns>A tuple containing the anchor connection and the database context.</returns>
    private static (SqliteConnection AnchorConnection, NemeraDbContext Context) CreateAuditedSqliteContext(Guid userId, DateTime utcNow)
    {
        SqliteConnection anchorConnection = new($"Data Source=nemeradataccess-unitofwork-audit-tests-{Guid.NewGuid()};Mode=Memory;Cache=Shared");
        anchorConnection.Open();
        ICurrentUserService currentUserService = Substitute.For<ICurrentUserService>();
        currentUserService.UserId.Returns(userId);
        IDateTimeProvider dateTimeProvider = Substitute.For<IDateTimeProvider>();
        dateTimeProvider.UtcNow.Returns(utcNow);
        NemeraDbContext dbContext = new(new DbContextOptionsBuilder<NemeraDbContext>()
            .UseSqlite(anchorConnection.ConnectionString)
            .AddInterceptors(new UpdateAuditableEntitiesInterceptor(currentUserService, dateTimeProvider))
            .Options);
        dbContext.Database.EnsureCreated();
        return (anchorConnection, dbContext);
    }
}
