#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.MediaLibrary.Management;
using Lumina.Application.Common.DataAccess.Entities.UsersManagement;
using Lumina.Application.Common.Infrastructure.Models.DTO.MediaLibraryScanJobPayloads;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.MediaLibrary.Management;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.UsersManagement;
using Lumina.Application.Fixtures.Common.Infrastructure.Models.DTO.MediaLibraryScanJobPayloads;
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

namespace Lumina.DataAccess.UnitTests.Core.Repositories.Libraries;

/// <summary>
/// Contains unit tests for the <see cref="LibraryScanStagingResultsRepository"/> class.
/// </summary>
/// <remarks>
/// The repository executes parameterized raw SQL on a dedicated database connection, so the tests exercise it against a real SQLite database
/// instead of the mocked in-memory context used by the Entity Framework based repositories.
/// </remarks>
[ExcludeFromCodeCoverage]
public class LibraryScanStagingResultsRepositoryTests : IDisposable
{
    private readonly string _connectionString;
    private readonly SqliteConnection _anchorConnection;
    private readonly LuminaDbContext _context;
    private readonly LibraryScanStagingResultsRepository _sut;
    private readonly LibraryScanStagingResultsEntityFixture _libraryScanStagingResultsEntityFixture = new();
    private readonly LibraryScanSnapshotEntityFixture _libraryScanSnapshotEntityFixture = new();
    private readonly UserEntityFixture _userEntityFixture = new();
    private readonly LibraryEntityFixture _libraryEntityFixture = new();
    private readonly LibraryScanEntityFixture _libraryScanEntityFixture = new();
    private readonly HashedFileSystemFileDtoFixture _hashedFileSystemFileDtoFixture = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="LibraryScanStagingResultsRepositoryTests"/> class.
    /// </summary>
    public LibraryScanStagingResultsRepositoryTests()
    {
        _connectionString = $"Data Source=luminadataccess-staging-tests-{Guid.NewGuid()};Mode=Memory;Cache=Shared";
        _anchorConnection = new SqliteConnection(_connectionString);
        _anchorConnection.Open();
        _context = new LuminaDbContext(new DbContextOptionsBuilder<LuminaDbContext>().UseSqlite(_connectionString).Options);
        _context.Database.EnsureCreated();
        _sut = new LibraryScanStagingResultsRepository(_context);
    }

    [Fact]
    public async Task InsertRangeAsync_WhenCalled_ShouldInsertAllEntitiesAndReturnCreated()
    {
        // Arrange
        (_, _, Guid scanId) = await SeedScanGraphAsync();
        List<LibraryScanStagingResultsEntity> stagingResults = _libraryScanStagingResultsEntityFixture.CreateMany(3);
        for (int i = 0; i < stagingResults.Count; i++)
            stagingResults[i] = _libraryScanStagingResultsEntityFixture.Create(libraryScanId: scanId);

        // Act
        Result<Created> result = await _sut.InsertRangeAsync(stagingResults, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(Result.Created, result.Value);
        Assert.Equal(3, await _context.LibraryScanStagingResults.CountAsync());
    }

    [Fact]
    public async Task InsertRangeAsync_WhenCollectionIsEmpty_ShouldReturnCreatedWithoutInserting()
    {
        // Arrange
        await SeedScanGraphAsync();

        // Act
        Result<Created> result = await _sut.InsertRangeAsync([], CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(Result.Created, result.Value);
        Assert.False(await _context.LibraryScanStagingResults.AnyAsync());
    }

    [Fact]
    public async Task MarkChangesAgainstSnapshotAsync_WhenSnapshotHasSameSizeAndTicks_ShouldMarkAsUnchanged()
    {
        // Arrange
        (_, LibraryEntity library, Guid scanId) = await SeedScanGraphAsync();
        LibraryScanSnapshotEntity snapshot = _libraryScanSnapshotEntityFixture.Create(libraryId: library.Id, path: "/books/a.epub", contentHash: 111, fileSize: 100, ticks: 1000);
        _context.LibraryScanSnapshots.Add(snapshot);
        LibraryScanStagingResultsEntity staging = _libraryScanStagingResultsEntityFixture.Create(libraryScanId: scanId, path: "/books/a.epub", size: 100, ticks: 1000);
        _context.LibraryScanStagingResults.Add(staging);
        await _context.SaveChangesAsync();

        // Act
        Result<Updated> result = await _sut.MarkChangesAgainstSnapshotAsync(scanId, library.Id, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(Result.Updated, result.Value);

        LibraryScanStagingResultsEntity markedStaging = await _context.LibraryScanStagingResults.AsNoTracking().SingleAsync();
        Assert.Equal(111UL, markedStaging.ContentHash);
        Assert.Equal(111UL, markedStaging.PreviousContentHash);
        Assert.False(markedStaging.NeedsRehash);
        Assert.False(markedStaging.IsNew);
    }

    [Fact]
    public async Task MarkChangesAgainstSnapshotAsync_WhenSnapshotHasDifferentSizeOrTicks_ShouldMarkAsChanged()
    {
        // Arrange
        (_, LibraryEntity library, Guid scanId) = await SeedScanGraphAsync();
        LibraryScanSnapshotEntity snapshot = _libraryScanSnapshotEntityFixture.Create(libraryId: library.Id, path: "/books/a.epub", contentHash: 222, fileSize: 100, ticks: 1000);
        _context.LibraryScanSnapshots.Add(snapshot);
        LibraryScanStagingResultsEntity staging = _libraryScanStagingResultsEntityFixture.Create(libraryScanId: scanId, path: "/books/a.epub", size: 500, ticks: 5000);
        _context.LibraryScanStagingResults.Add(staging);
        await _context.SaveChangesAsync();

        // Act
        Result<Updated> result = await _sut.MarkChangesAgainstSnapshotAsync(scanId, library.Id, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(Result.Updated, result.Value);

        LibraryScanStagingResultsEntity markedStaging = await _context.LibraryScanStagingResults.AsNoTracking().SingleAsync();
        Assert.Equal(222UL, markedStaging.ContentHash);
        Assert.Equal(222UL, markedStaging.PreviousContentHash);
        Assert.True(markedStaging.NeedsRehash);
        Assert.False(markedStaging.IsNew);
    }

    [Fact]
    public async Task MarkChangesAgainstSnapshotAsync_WhenNoSnapshotExists_ShouldKeepTheItemMarkedAsNew()
    {
        // Arrange
        (_, LibraryEntity library, Guid scanId) = await SeedScanGraphAsync();
        // the file system discovery job seeds new items with no hash and marks them as new, needing a rehash
        LibraryScanStagingResultsEntity staging = _libraryScanStagingResultsEntityFixture.Create(libraryScanId: scanId, path: "/books/a.epub", contentHash: 0, previousContentHash: 0, needsRehash: true, isNew: true);
        _context.LibraryScanStagingResults.Add(staging);
        await _context.SaveChangesAsync();

        // Act
        Result<Updated> result = await _sut.MarkChangesAgainstSnapshotAsync(scanId, library.Id, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(Result.Updated, result.Value);

        LibraryScanStagingResultsEntity markedStaging = await _context.LibraryScanStagingResults.AsNoTracking().SingleAsync();
        Assert.Equal(0UL, markedStaging.ContentHash);
        Assert.Equal(0UL, markedStaging.PreviousContentHash);
        Assert.True(markedStaging.NeedsRehash);
        Assert.True(markedStaging.IsNew);
    }

    [Fact]
    public async Task GetFilesToHashCountAsync_WhenCalled_ShouldReturnOnlyStagingResultsNeedingRehash()
    {
        // Arrange
        (_, _, Guid scanId) = await SeedScanGraphAsync();
        LibraryScanStagingResultsEntity firstRehash = _libraryScanStagingResultsEntityFixture.Create(libraryScanId: scanId, needsRehash: true);
        LibraryScanStagingResultsEntity secondRehash = _libraryScanStagingResultsEntityFixture.Create(libraryScanId: scanId, needsRehash: true);
        LibraryScanStagingResultsEntity noRehash = _libraryScanStagingResultsEntityFixture.Create(libraryScanId: scanId, needsRehash: false);
        _context.LibraryScanStagingResults.AddRange(firstRehash, secondRehash, noRehash);
        await _context.SaveChangesAsync();

        // Act
        Result<int> result = await _sut.GetFilesToHashCountAsync(scanId, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(2, result.Value);
    }

    [Fact]
    public async Task GetFilesToHashPageAsync_WhenCalled_ShouldReturnOrderedPageOfStagingResultsNeedingRehash()
    {
        // Arrange
        (_, _, Guid scanId) = await SeedScanGraphAsync();
        LibraryScanStagingResultsEntity aFile = _libraryScanStagingResultsEntityFixture.Create(libraryScanId: scanId, path: "/books/a.epub", size: 10, ticks: 100, needsRehash: true);
        LibraryScanStagingResultsEntity bFile = _libraryScanStagingResultsEntityFixture.Create(libraryScanId: scanId, path: "/books/b.epub", size: 20, ticks: 200, needsRehash: true);
        LibraryScanStagingResultsEntity noRehash = _libraryScanStagingResultsEntityFixture.Create(libraryScanId: scanId, path: "/books/z.epub", needsRehash: false);
        _context.LibraryScanStagingResults.AddRange(aFile, bFile, noRehash);
        await _context.SaveChangesAsync();

        // Act
        Result<IReadOnlyList<HashedFileSystemFileDto>> result = await _sut.GetFilesToHashPageAsync(scanId, null, 10, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(2, result.Value.Count);
        Assert.Equal("/books/a.epub", result.Value[0].Path);
        Assert.Equal("/books/b.epub", result.Value[1].Path);
        Assert.Equal(10, result.Value[0].Size);
        Assert.Equal(20, result.Value[1].Size);
        Assert.Equal(100, result.Value[0].Ticks);
        Assert.Equal(200, result.Value[1].Ticks);
    }

    [Fact]
    public async Task GetFilesToHashPageAsync_WhenLastPathProvided_ShouldReturnOnlyResultsAfterIt()
    {
        // Arrange
        (_, _, Guid scanId) = await SeedScanGraphAsync();
        LibraryScanStagingResultsEntity aFile = _libraryScanStagingResultsEntityFixture.Create(libraryScanId: scanId, path: "/books/a.epub", needsRehash: true);
        LibraryScanStagingResultsEntity bFile = _libraryScanStagingResultsEntityFixture.Create(libraryScanId: scanId, path: "/books/b.epub", needsRehash: true);
        _context.LibraryScanStagingResults.AddRange(aFile, bFile);
        await _context.SaveChangesAsync();

        // Act
        Result<IReadOnlyList<HashedFileSystemFileDto>> result = await _sut.GetFilesToHashPageAsync(scanId, "/books/a.epub", 10, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        HashedFileSystemFileDto retrievedFile = Assert.Single(result.Value);
        Assert.Equal("/books/b.epub", retrievedFile.Path);
    }

    [Fact]
    public async Task UpdateFileHashesAsync_WhenCalled_ShouldUpdateTheContentHashes()
    {
        // Arrange
        (_, _, Guid scanId) = await SeedScanGraphAsync();
        LibraryScanStagingResultsEntity firstStaging = _libraryScanStagingResultsEntityFixture.Create(libraryScanId: scanId, path: "/books/a.epub", contentHash: 111);
        LibraryScanStagingResultsEntity secondStaging = _libraryScanStagingResultsEntityFixture.Create(libraryScanId: scanId, path: "/books/b.epub", contentHash: 222);
        _context.LibraryScanStagingResults.AddRange(firstStaging, secondStaging);
        await _context.SaveChangesAsync();

        List<HashedFileSystemFileDto> hashedFiles =
        [
            _hashedFileSystemFileDtoFixture.Create(path: "/books/a.epub", size: 10, ticks: 100, currentHash: 999, oldHash: 0),
            _hashedFileSystemFileDtoFixture.Create(path: "/books/b.epub", size: 20, ticks: 200, currentHash: 888, oldHash: 0)
        ];

        // Act
        Result<Updated> result = await _sut.UpdateFileHashesAsync(scanId, hashedFiles, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(Result.Updated, result.Value);

        List<LibraryScanStagingResultsEntity> stagingResults = await _context.LibraryScanStagingResults.AsNoTracking().OrderBy(staging => staging.Path).ToListAsync();
        Assert.Equal(999UL, stagingResults[0].ContentHash);
        Assert.Equal(888UL, stagingResults[1].ContentHash);
    }

    [Fact]
    public async Task ClearForScanAsync_WhenCalled_ShouldRemoveOnlyTheStagingResultsOfTheScan()
    {
        // Arrange
        (UserEntity user, LibraryEntity library, Guid scanId) = await SeedScanGraphAsync();
        LibraryScanEntity otherScan = _libraryScanEntityFixture.Create(libraryId: library.Id, libraryEntity: library, userId: user.Id, status: LibraryScanJobStatus.Completed);
        _context.LibraryScans.Add(otherScan);
        LibraryScanStagingResultsEntity stagingOfScan = _libraryScanStagingResultsEntityFixture.Create(libraryScanId: scanId);
        LibraryScanStagingResultsEntity stagingOfOtherScan = _libraryScanStagingResultsEntityFixture.Create(libraryScanId: otherScan.Id);
        _context.LibraryScanStagingResults.AddRange(stagingOfScan, stagingOfOtherScan);
        await _context.SaveChangesAsync();

        // Act
        Result<Success> result = await _sut.ClearForScanAsync(scanId, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(Result.Success, result.Value);
        Assert.False(await _context.LibraryScanStagingResults.AsNoTracking().AnyAsync(staging => staging.LibraryScanId == scanId));
        Assert.Single(await _context.LibraryScanStagingResults.AsNoTracking().Where(staging => staging.LibraryScanId == otherScan.Id).ToListAsync());
    }

    /// <summary>
    /// Seeds the user, media library and media library scan required by the foreign keys of the staging table.
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
