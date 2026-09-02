#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.MediaLibrary.Management;
using Lumina.Application.Common.DataAccess.Entities.UsersManagement;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.MediaLibrary.Management;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.UsersManagement;
using Lumina.DataAccess.Core.Repositories.Libraries;
using Lumina.DataAccess.Core.UoW;
using Lumina.Domain.Common.Primitives;
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

namespace Lumina.DataAccess.IntegrationTests.Core.Repositories.Libraries;

/// <summary>
/// Contains integration tests for the <see cref="LibraryScanSnapshotRepository"/> class.
/// </summary>
/// <remarks>
/// The repository executes parameterized raw SQL on a dedicated database connection, so the tests exercise it against a real SQLite database
/// instead of the mocked in-memory context used by the Entity Framework based repositories.
/// </remarks>
[ExcludeFromCodeCoverage]
public class LibraryScanSnapshotRepositoryTests : IDisposable
{
    private readonly string _connectionString;
    private readonly SqliteConnection _anchorConnection;
    private readonly LuminaDbContext _context;
    private readonly LibraryScanSnapshotRepository _sut;
    private readonly LibraryScanSnapshotEntityFixture _libraryScanSnapshotEntityFixture = new();
    private readonly LibraryScanStagingResultsEntityFixture _libraryScanStagingResultsEntityFixture = new();
    private readonly UserEntityFixture _userEntityFixture = new();
    private readonly LibraryEntityFixture _libraryEntityFixture = new();
    private readonly LibraryScanEntityFixture _libraryScanEntityFixture = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="LibraryScanSnapshotRepositoryTests"/> class.
    /// </summary>
    public LibraryScanSnapshotRepositoryTests()
    {
        _connectionString = $"Data Source=luminadataccess-snapshot-tests-{Guid.NewGuid()};Mode=Memory;Cache=Shared";
        _anchorConnection = new SqliteConnection(_connectionString);
        _anchorConnection.Open();
        _context = new LuminaDbContext(new DbContextOptionsBuilder<LuminaDbContext>().UseSqlite(_connectionString).Options);
        _context.Database.EnsureCreated();
        _sut = new LibraryScanSnapshotRepository(_context);
    }

    [Fact]
    public async Task GetDeletedPathsAsync_WhenSnapshotPathsAreMissingFromStaging_ShouldReturnThosePaths()
    {
        // Arrange
        (_, LibraryEntity library, Guid scanId) = await SeedScanGraphAsync();
        LibraryScanSnapshotEntity keptSnapshot = _libraryScanSnapshotEntityFixture.Create(libraryId: library.Id, path: "/books/keep.epub");
        LibraryScanSnapshotEntity deletedSnapshot = _libraryScanSnapshotEntityFixture.Create(libraryId: library.Id, path: "/books/delete.epub");
        _context.LibraryScanSnapshots.AddRange(keptSnapshot, deletedSnapshot);
        LibraryScanStagingResultsEntity staging = _libraryScanStagingResultsEntityFixture.Create(libraryScanId: scanId, path: "/books/keep.epub");
        _context.LibraryScanStagingResults.Add(staging);
        await _context.SaveChangesAsync();

        // Act
        Result<IReadOnlyList<string>> result = await _sut.GetDeletedPathsAsync(library.Id, scanId, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        string deletedPath = Assert.Single(result.Value);
        Assert.Equal("/books/delete.epub", deletedPath);
    }

    [Fact]
    public async Task GetDeletedPathsAsync_WhenAllSnapshotPathsArePresentInStaging_ShouldReturnEmptyList()
    {
        // Arrange
        (_, LibraryEntity library, Guid scanId) = await SeedScanGraphAsync();
        LibraryScanSnapshotEntity snapshot = _libraryScanSnapshotEntityFixture.Create(libraryId: library.Id, path: "/books/keep.epub");
        _context.LibraryScanSnapshots.Add(snapshot);
        LibraryScanStagingResultsEntity staging = _libraryScanStagingResultsEntityFixture.Create(libraryScanId: scanId, path: "/books/keep.epub");
        _context.LibraryScanStagingResults.Add(staging);
        await _context.SaveChangesAsync();

        // Act
        Result<IReadOnlyList<string>> result = await _sut.GetDeletedPathsAsync(library.Id, scanId, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Empty(result.Value);
    }

    [Fact]
    public async Task GetDeletedPathsAsync_WhenStagingOfAnotherScanMatches_ShouldStillReturnThePaths()
    {
        // Arrange
        (UserEntity user, LibraryEntity library, Guid scanId) = await SeedScanGraphAsync();
        LibraryScanEntity otherScan = _libraryScanEntityFixture.Create(libraryId: library.Id, libraryEntity: library, userId: user.Id, status: LibraryScanJobStatus.Completed);
        _context.LibraryScans.Add(otherScan);
        LibraryScanSnapshotEntity snapshot = _libraryScanSnapshotEntityFixture.Create(libraryId: library.Id, path: "/books/a.epub");
        _context.LibraryScanSnapshots.Add(snapshot);
        LibraryScanStagingResultsEntity stagingOfOtherScan = _libraryScanStagingResultsEntityFixture.Create(libraryScanId: otherScan.Id, path: "/books/a.epub");
        _context.LibraryScanStagingResults.Add(stagingOfOtherScan);
        await _context.SaveChangesAsync();

        // Act
        Result<IReadOnlyList<string>> result = await _sut.GetDeletedPathsAsync(library.Id, scanId, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        string deletedPath = Assert.Single(result.Value);
        Assert.Equal("/books/a.epub", deletedPath);
    }

    [Fact]
    public async Task GetPathsAsync_WhenCalled_ShouldReturnAllSnapshotPathsOfTheLibrary()
    {
        // Arrange
        (_, LibraryEntity library, _) = await SeedScanGraphAsync();
        LibraryScanSnapshotEntity firstSnapshot = _libraryScanSnapshotEntityFixture.Create(libraryId: library.Id, path: "/books/a.epub");
        LibraryScanSnapshotEntity secondSnapshot = _libraryScanSnapshotEntityFixture.Create(libraryId: library.Id, path: "/books/b.epub");
        _context.LibraryScanSnapshots.AddRange(firstSnapshot, secondSnapshot);
        await _context.SaveChangesAsync();

        // Act
        Result<IReadOnlyList<string>> result = await _sut.GetPathsAsync(library.Id, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(2, result.Value.Count);
        Assert.Contains("/books/a.epub", result.Value);
        Assert.Contains("/books/b.epub", result.Value);
    }

    [Fact]
    public async Task GetPathsAsync_WhenNoSnapshotsExist_ShouldReturnEmptyList()
    {
        // Arrange
        (_, LibraryEntity library, _) = await SeedScanGraphAsync();

        // Act
        Result<IReadOnlyList<string>> result = await _sut.GetPathsAsync(library.Id, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Empty(result.Value);
    }

    [Fact]
    public async Task ApplySnapshotSwapAsync_WhenCalled_ShouldAuditDeletedChangedAndNewItemsAndUpdateSnapshot()
    {
        // Arrange
        (UserEntity user, LibraryEntity library, Guid scanId) = await SeedScanGraphAsync();

        LibraryScanSnapshotEntity keptSnapshot = _libraryScanSnapshotEntityFixture.Create(libraryId: library.Id, path: "/books/keep.epub", contentHash: 111, fileSize: 100, ticks: 1000);
        LibraryScanSnapshotEntity deletedSnapshot = _libraryScanSnapshotEntityFixture.Create(libraryId: library.Id, path: "/books/delete.epub", contentHash: 222, fileSize: 200, ticks: 2000);
        _context.LibraryScanSnapshots.AddRange(keptSnapshot, deletedSnapshot);

        LibraryScanStagingResultsEntity unchangedStaging = _libraryScanStagingResultsEntityFixture.Create(
            libraryScanId: scanId, path: "/books/keep.epub", size: 100, ticks: 1000, contentHash: 999, needsRehash: false, isNew: false);
        LibraryScanStagingResultsEntity newStaging = _libraryScanStagingResultsEntityFixture.Create(
            libraryScanId: scanId, path: "/books/new.epub", size: 300, ticks: 3000, contentHash: 300, needsRehash: true, isNew: true);
        LibraryScanStagingResultsEntity modifiedStaging = _libraryScanStagingResultsEntityFixture.Create(
            libraryScanId: scanId, path: "/books/modified.epub", size: 400, ticks: 4000, contentHash: 400, needsRehash: true, isNew: false);
        _context.LibraryScanStagingResults.AddRange(unchangedStaging, newStaging, modifiedStaging);
        await _context.SaveChangesAsync();

        // Act
        Result<Updated> result = await _sut.ApplySnapshotSwapAsync(library.Id, scanId, user.Id, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(Result.Updated, result.Value);

        // Audit entries: deleted, new and modified items.
        List<LibraryScanResultEntity> scanResults = await _context.LibraryScanResults.AsNoTracking()
            .Where(scanResult => scanResult.LibraryScanId == scanId)
            .ToListAsync();
        Assert.Equal(3, scanResults.Count);
        Assert.Contains(scanResults, scanResult => scanResult.Path == "/books/delete.epub" && scanResult.Status == LibraryScanFileStatus.Deleted);
        Assert.Contains(scanResults, scanResult => scanResult.Path == "/books/new.epub" && scanResult.Status == LibraryScanFileStatus.New);
        Assert.Contains(scanResults, scanResult => scanResult.Path == "/books/modified.epub" && scanResult.Status == LibraryScanFileStatus.Modified);
        Assert.DoesNotContain(scanResults, scanResult => scanResult.Path == "/books/keep.epub");

        // The snapshot now contains the kept, new and modified items and no longer contains the deleted one.
        List<LibraryScanSnapshotEntity> snapshots = await _context.LibraryScanSnapshots.AsNoTracking()
            .Where(snapshot => snapshot.LibraryId == library.Id)
            .ToListAsync();
        Assert.Equal(3, snapshots.Count);
        Assert.Contains(snapshots, snapshot => snapshot.Path == "/books/keep.epub" && snapshot.ContentHash == 111);
        Assert.Contains(snapshots, snapshot => snapshot.Path == "/books/new.epub" && snapshot.ContentHash == 300);
        Assert.Contains(snapshots, snapshot => snapshot.Path == "/books/modified.epub" && snapshot.ContentHash == 400);
        Assert.DoesNotContain(snapshots, snapshot => snapshot.Path == "/books/delete.epub");

        // The staging results of the scan have been cleared.
        Assert.False(await _context.LibraryScanStagingResults.AsNoTracking().AnyAsync(staging => staging.LibraryScanId == scanId));
    }

    [Fact]
    public async Task ApplySnapshotSwapAsync_WhenNoStagingNeedsRehash_ShouldOnlyClearStagingAndAuditDeletedItems()
    {
        // Arrange
        (UserEntity user, LibraryEntity library, Guid scanId) = await SeedScanGraphAsync();

        LibraryScanSnapshotEntity keptSnapshot = _libraryScanSnapshotEntityFixture.Create(libraryId: library.Id, path: "/books/keep.epub", contentHash: 111, fileSize: 100, ticks: 1000);
        _context.LibraryScanSnapshots.Add(keptSnapshot);
        LibraryScanStagingResultsEntity unchangedStaging = _libraryScanStagingResultsEntityFixture.Create(
            libraryScanId: scanId, path: "/books/keep.epub", size: 100, ticks: 1000, needsRehash: false, isNew: false);
        _context.LibraryScanStagingResults.Add(unchangedStaging);
        await _context.SaveChangesAsync();

        // Act
        Result<Updated> result = await _sut.ApplySnapshotSwapAsync(library.Id, scanId, user.Id, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(Result.Updated, result.Value);
        Assert.False(await _context.LibraryScanResults.AsNoTracking().AnyAsync());
        Assert.False(await _context.LibraryScanStagingResults.AsNoTracking().AnyAsync(staging => staging.LibraryScanId == scanId));
    }

    /// <summary>
    /// Seeds the user, media library and media library scan required by the foreign keys of the snapshot and staging tables.
    /// </summary>
    /// <returns>The seeded user, media library and scan Id.</returns>
    private async Task<(UserEntity User, LibraryEntity Library, Guid ScanId)> SeedScanGraphAsync()
    {
        UserEntity user = _userEntityFixture.Create();
        _context.Users.Add(user);
        LibraryEntity library = _libraryEntityFixture.Create(userId: user.Id);
        _context.Libraries.Add(library);
        LibraryScanEntity scan = _libraryScanEntityFixture.Create(libraryId: library.Id, libraryEntity: library, userId: user.Id, status: LibraryScanJobStatus.Running);
        _context.LibraryScans.Add(scan);
        await _context.SaveChangesAsync();
        return (user, library, scan.Id);
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        _context.Dispose();
        _anchorConnection.Dispose();
    }
}
