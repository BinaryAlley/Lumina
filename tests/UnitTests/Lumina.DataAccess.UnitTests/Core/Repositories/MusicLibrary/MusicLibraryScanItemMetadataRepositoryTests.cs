#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.MediaLibrary.AudioLibrary.MusicLibrary;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.MediaLibrary.AudioLibrary.MusicLibrary;
using Lumina.DataAccess.Core.Repositories.MusicLibrary;
using Lumina.DataAccess.Core.UoW;
using Lumina.Domain.Common.Primitives;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.DataAccess.UnitTests.Core.Repositories.MusicLibrary;

/// <summary>
/// Contains unit tests for the <see cref="MusicLibraryScanItemMetadataRepository"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class MusicLibraryScanItemMetadataRepositoryTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly LuminaDbContext _context;
    private readonly MusicLibraryScanItemMetadataRepository _sut;
    private readonly MusicLibraryScanItemMetadataEntityFixture _musicLibraryScanItemMetadataEntityFixture = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="MusicLibraryScanItemMetadataRepositoryTests"/> class.
    /// </summary>
    public MusicLibraryScanItemMetadataRepositoryTests()
    {
        // the repository deletes in bulk, which translates to SQL, so it can only be exercised against a real SQLite database
        _connection = new SqliteConnection("Data Source=:memory:");
        _connection.Open();
        _context = new LuminaDbContext(new DbContextOptionsBuilder<LuminaDbContext>().UseSqlite(_connection).Options);
        _context.Database.EnsureCreated();
        _sut = new MusicLibraryScanItemMetadataRepository(_context);
    }

    [Fact]
    public async Task InsertRangeAsync_WhenCalledWithItems_ShouldAddThemToContextAndReturnCreated()
    {
        // Arrange
        List<MusicLibraryScanItemMetadataEntity> items = _musicLibraryScanItemMetadataEntityFixture.CreateMany(3);

        // Act
        Result<Created> result = await _sut.InsertRangeAsync(items, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(Result.Created, result.Value);
        await _context.SaveChangesAsync();
        Assert.Equal(3, await _context.MusicLibraryScanItemMetadata.CountAsync());
    }

    [Fact]
    public async Task InsertRangeAsync_WhenCollectionIsEmpty_ShouldReturnCreatedWithoutAddingAnyItems()
    {
        // Act
        Result<Created> result = await _sut.InsertRangeAsync([], CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(Result.Created, result.Value);
        await _context.SaveChangesAsync();
        Assert.Empty(await _context.MusicLibraryScanItemMetadata.ToListAsync());
    }

    [Fact]
    public async Task GetByScanIdAsync_WhenCalled_ShouldReturnOnlyTheItemsOfTheScanOrderedByPath()
    {
        // Arrange
        Guid scanId = Guid.NewGuid();
        MusicLibraryScanItemMetadataEntity bicycleItem = _musicLibraryScanItemMetadataEntityFixture.Create(libraryScanId: scanId, path: "/music/queen/bicycle-race.flac");
        MusicLibraryScanItemMetadataEntity bohemianItem = _musicLibraryScanItemMetadataEntityFixture.Create(libraryScanId: scanId, path: "/music/queen/bohemian-rhapsody.flac");
        MusicLibraryScanItemMetadataEntity itemOfAnotherScan = _musicLibraryScanItemMetadataEntityFixture.Create(path: "/music/queen/dont-stop-me-now.flac");
        _context.MusicLibraryScanItemMetadata.AddRange(bicycleItem, bohemianItem, itemOfAnotherScan);
        await _context.SaveChangesAsync();

        // Act
        Result<IReadOnlyList<MusicLibraryScanItemMetadataEntity>> result = await _sut.GetByScanIdAsync(scanId, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal([bicycleItem.Id, bohemianItem.Id], result.Value.Select(item => item.Id));
    }

    [Fact]
    public async Task GetByScanIdAsync_WhenNoItemsExistForTheScan_ShouldReturnAnEmptyCollection()
    {
        // Act
        Result<IReadOnlyList<MusicLibraryScanItemMetadataEntity>> result = await _sut.GetByScanIdAsync(Guid.NewGuid(), CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Empty(result.Value);
    }

    [Fact]
    public async Task DeleteByScanIdAsync_WhenCalled_ShouldDeleteOnlyTheItemsOfTheScan()
    {
        // Arrange
        Guid scanId = Guid.NewGuid();
        MusicLibraryScanItemMetadataEntity itemOfScan = _musicLibraryScanItemMetadataEntityFixture.Create(libraryScanId: scanId, path: "/music/queen/bohemian-rhapsody.flac");
        MusicLibraryScanItemMetadataEntity itemOfAnotherScan = _musicLibraryScanItemMetadataEntityFixture.Create(path: "/music/queen/love-of-my-life.flac");
        _context.MusicLibraryScanItemMetadata.AddRange(itemOfScan, itemOfAnotherScan);
        await _context.SaveChangesAsync();

        // Act
        Result<Deleted> result = await _sut.DeleteByScanIdAsync(scanId, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(Result.Deleted, result.Value);
        MusicLibraryScanItemMetadataEntity remainingItem = Assert.Single(await _context.MusicLibraryScanItemMetadata.ToListAsync());
        Assert.Equal(itemOfAnotherScan.Id, remainingItem.Id);
    }

    /// <summary>
    /// Disposes the database context and its underlying connection.
    /// </summary>
    public void Dispose()
    {
        _context.Dispose();
        _connection.Dispose();
    }
}
