#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.Authorization;
using Lumina.Application.Common.DataAccess.Entities.MediaLibrary.WrittenContentLibrary.BookLibrary;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.Authorization;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.MediaLibrary.WrittenContentLibrary.BookLibrary;
using Lumina.DataAccess.Core.UoW;
using Lumina.Domain.Common.Primitives;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
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
}
