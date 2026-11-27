#region ========================================================================= USING =====================================================================================
using Nemera.Application.Common.DataAccess.Entities.MediaLibrary.Management;
using Nemera.Application.Common.DataAccess.Entities.UsersManagement;
using Nemera.Application.Fixtures.Common.DataAccess.Entities.MediaLibrary.Management;
using Nemera.Application.Fixtures.Common.DataAccess.Entities.UsersManagement;
using Nemera.DataAccess.Common.Dapper;
using Nemera.DataAccess.Core.Repositories.Libraries;
using Nemera.DataAccess.Core.UoW;
using Nemera.Domain.Common.Primitives;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Nemera.DataAccess.IntegrationTests.Core.Repositories.Libraries;

/// <summary>
/// Contains integration tests for the <see cref="DirectoryScanFingerprintRepository"/> class.
/// </summary>
/// <remarks>
/// The repository executes parameterized raw SQL on a dedicated database connection, so the tests exercise it against a real SQLite database
/// instead of the mocked in-memory context used by the Entity Framework based repositories.
/// </remarks>
[ExcludeFromCodeCoverage]
public class DirectoryScanFingerprintRepositoryTests : IDisposable
{
    private readonly string _connectionString;
    private readonly SqliteConnection _anchorConnection;
    private readonly NemeraDbContext _context;
    private readonly DirectoryScanFingerprintRepository _sut;
    private readonly DirectoryScanFingerprintEntityFixture _directoryScanFingerprintEntityFixture = new();
    private readonly UserEntityFixture _userEntityFixture = new();
    private readonly LibraryEntityFixture _libraryEntityFixture = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="DirectoryScanFingerprintRepositoryTests"/> class.
    /// </summary>
    public DirectoryScanFingerprintRepositoryTests()
    {
        DapperTypeHandlers.Register();
        _connectionString = $"Data Source=nemeradataccess-fingerprint-tests-{Guid.NewGuid()};Mode=Memory;Cache=Shared";
        _anchorConnection = new SqliteConnection(_connectionString);
        _anchorConnection.Open();
        _context = new NemeraDbContext(new DbContextOptionsBuilder<NemeraDbContext>().UseSqlite(_connectionString).Options);
        _context.Database.EnsureCreated();
        _sut = new DirectoryScanFingerprintRepository(_context);
    }

    [Fact]
    public async Task GetMappedByLibraryIdAsync_WhenCalled_ShouldReturnFingerprintsOfTheLibraryMappedByPath()
    {
        // Arrange
        (Guid libraryId, _) = await SeedLibraryGraphAsync();
        UserEntity otherUser = _userEntityFixture.Create();
        _context.Users.Add(otherUser);
        Guid otherLibraryId = Guid.NewGuid();
        LibraryEntity otherLibrary = _libraryEntityFixture.Create(id: otherLibraryId, userId: otherUser.Id);
        _context.Libraries.Add(otherLibrary);

        DirectoryScanFingerprintEntity firstFingerprint = _directoryScanFingerprintEntityFixture.Create(libraryId: libraryId, path: "/dir1", lastWriteTimeUtc: DateTime.UtcNow.AddDays(-1));
        DirectoryScanFingerprintEntity secondFingerprint = _directoryScanFingerprintEntityFixture.Create(libraryId: libraryId, path: "/dir2", lastWriteTimeUtc: DateTime.UtcNow.AddDays(-2));
        DirectoryScanFingerprintEntity fingerprintOfOtherLibrary = _directoryScanFingerprintEntityFixture.Create(libraryId: otherLibraryId, path: "/dir3");
        _context.DirectoryScanFingerprints.AddRange(firstFingerprint, secondFingerprint, fingerprintOfOtherLibrary);
        await _context.SaveChangesAsync();

        // Act
        Result<Dictionary<string, DirectoryScanFingerprintEntity>> result = await _sut.GetMappedByLibraryIdAsync(libraryId, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(2, result.Value.Count);
        Assert.True(result.Value.ContainsKey("/dir1"));
        Assert.True(result.Value.ContainsKey("/dir2"));
        Assert.False(result.Value.ContainsKey("/dir3"));
        Assert.Equal(firstFingerprint.Id, result.Value["/dir1"].Id);
        Assert.Equal(secondFingerprint.Id, result.Value["/dir2"].Id);
    }

    [Fact]
    public async Task GetMappedByLibraryIdAsync_WhenNoFingerprintsExist_ShouldReturnEmptyDictionary()
    {
        // Arrange
        (Guid libraryId, _) = await SeedLibraryGraphAsync();

        // Act
        Result<Dictionary<string, DirectoryScanFingerprintEntity>> result = await _sut.GetMappedByLibraryIdAsync(libraryId, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Empty(result.Value);
    }

    [Fact]
    public async Task DeleteByLibraryIdAsync_WhenCalled_ShouldDeleteOnlyTheFingerprintsOfThatLibrary()
    {
        // Arrange
        (Guid libraryId, _) = await SeedLibraryGraphAsync();
        UserEntity otherUser = _userEntityFixture.Create();
        _context.Users.Add(otherUser);
        Guid otherLibraryId = Guid.NewGuid();
        LibraryEntity otherLibrary = _libraryEntityFixture.Create(id: otherLibraryId, userId: otherUser.Id);
        _context.Libraries.Add(otherLibrary);

        DirectoryScanFingerprintEntity firstFingerprint = _directoryScanFingerprintEntityFixture.Create(libraryId: libraryId, path: "/dir1");
        DirectoryScanFingerprintEntity secondFingerprint = _directoryScanFingerprintEntityFixture.Create(libraryId: libraryId, path: "/dir2");
        DirectoryScanFingerprintEntity fingerprintOfOtherLibrary = _directoryScanFingerprintEntityFixture.Create(libraryId: otherLibraryId, path: "/dir3");
        _context.DirectoryScanFingerprints.AddRange(firstFingerprint, secondFingerprint, fingerprintOfOtherLibrary);
        await _context.SaveChangesAsync();

        // Act
        Result<Deleted> result = await _sut.DeleteByLibraryIdAsync(libraryId, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(Result.Deleted, result.Value);
        DirectoryScanFingerprintEntity remainingFingerprint = await _context.DirectoryScanFingerprints.AsNoTracking().SingleAsync();
        Assert.Equal(fingerprintOfOtherLibrary.Id, remainingFingerprint.Id);
    }

    [Fact]
    public async Task DeleteByLibraryIdAsync_WhenNoFingerprintsExist_ShouldReturnDeleted()
    {
        // Arrange
        (Guid libraryId, _) = await SeedLibraryGraphAsync();

        // Act
        Result<Deleted> result = await _sut.DeleteByLibraryIdAsync(libraryId, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(Result.Deleted, result.Value);
        Assert.False(await _context.DirectoryScanFingerprints.AnyAsync());
    }

    [Fact]
    public async Task UpsertRangeAsync_WhenCalledWithNewFingerprints_ShouldInsertThemAndReturnUpdated()
    {
        // Arrange
        (Guid libraryId, _) = await SeedLibraryGraphAsync();
        List<DirectoryScanFingerprintEntity> fingerprints =
        [
            _directoryScanFingerprintEntityFixture.Create(libraryId: libraryId, path: "/dir1"),
            _directoryScanFingerprintEntityFixture.Create(libraryId: libraryId, path: "/dir2")
        ];

        // Act
        Result<Updated> result = await _sut.UpsertRangeAsync(fingerprints, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(Result.Updated, result.Value);
        Assert.Equal(2, await _context.DirectoryScanFingerprints.CountAsync());
    }

    [Fact]
    public async Task UpsertRangeAsync_WhenCalledWithExistingFingerprints_ShouldUpdateTheirLastWriteTime()
    {
        // Arrange
        (Guid libraryId, _) = await SeedLibraryGraphAsync();
        DirectoryScanFingerprintEntity existingFingerprint = _directoryScanFingerprintEntityFixture.Create(libraryId: libraryId, path: "/dir1", lastWriteTimeUtc: DateTime.UtcNow.AddDays(-1));
        _context.DirectoryScanFingerprints.Add(existingFingerprint);
        await _context.SaveChangesAsync();

        DateTime newLastWriteTimeUtc = DateTime.UtcNow.AddDays(1);
        DirectoryScanFingerprintEntity updatedFingerprint = _directoryScanFingerprintEntityFixture.Create(libraryId: libraryId, path: "/dir1", lastWriteTimeUtc: newLastWriteTimeUtc);

        // Act
        Result<Updated> result = await _sut.UpsertRangeAsync([updatedFingerprint], CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(Result.Updated, result.Value);
        DirectoryScanFingerprintEntity retrievedFingerprint = await _context.DirectoryScanFingerprints.AsNoTracking().SingleAsync();
        Assert.Equal(newLastWriteTimeUtc, retrievedFingerprint.LastWriteTimeUtc);
    }

    [Fact]
    public async Task UpsertRangeAsync_WhenCollectionIsEmpty_ShouldReturnUpdatedWithoutUpserting()
    {
        // Arrange
        (Guid libraryId, _) = await SeedLibraryGraphAsync();

        // Act
        Result<Updated> result = await _sut.UpsertRangeAsync([], CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(Result.Updated, result.Value);
        Assert.False(await _context.DirectoryScanFingerprints.AnyAsync());
    }

    /// <summary>
    /// Seeds the user and media library required by the foreign key of the fingerprints table.
    /// </summary>
    /// <returns>The seeded library Id and user Id.</returns>
    private async Task<(Guid LibraryId, Guid UserId)> SeedLibraryGraphAsync()
    {
        UserEntity user = _userEntityFixture.Create();
        _context.Users.Add(user);
        LibraryEntity library = _libraryEntityFixture.Create(userId: user.Id);
        _context.Libraries.Add(library);
        await _context.SaveChangesAsync();
        return (library.Id, user.Id);
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        _context.Dispose();
        _anchorConnection.Dispose();
    }
}
