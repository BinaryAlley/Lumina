#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.MediaLibrary.AudioLibrary.MusicLibrary;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.MediaLibrary.AudioLibrary.MusicLibrary;
using Lumina.DataAccess.Core.Repositories.MusicLibrary;
using Lumina.DataAccess.Core.UoW;
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.SharedKernel.Common.Enums.AudioLibrary;
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

namespace Lumina.DataAccess.IntegrationTests.Core.Repositories.MusicLibrary;

/// <summary>
/// Contains integration tests for the <see cref="MusicArtworkRepository"/> class, exercising it against a real SQLite database.
/// </summary>
[ExcludeFromCodeCoverage]
public class MusicArtworkRepositoryTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly LuminaDbContext _context;
    private readonly MusicArtworkRepository _sut;
    private readonly MusicArtworkEntityFixture _musicArtworkEntityFixture = new();
    private readonly ArtistEntityFixture _artistEntityFixture = new();
    private readonly AlbumEntityFixture _albumEntityFixture = new();
    private readonly TrackEntityFixture _trackEntityFixture = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="MusicArtworkRepositoryTests"/> class.
    /// </summary>
    public MusicArtworkRepositoryTests()
    {
        // The repository deletes and updates in bulk, which translates to SQL, so the tests exercise it against a real SQLite database.
        _connection = new SqliteConnection("Data Source=:memory:");
        _connection.Open();
        _context = new LuminaDbContext(new DbContextOptionsBuilder<LuminaDbContext>().UseSqlite(_connection).Options);
        _context.Database.EnsureCreated();
        _sut = new MusicArtworkRepository(_context);
    }

    [Fact]
    public async Task InsertRangeAsync_WhenCalledWithArtwork_ShouldAddThemToContextAndReturnCreated()
    {
        // Arrange
        List<MusicArtworkEntity> artwork = _musicArtworkEntityFixture.CreateMany(3);

        // Act
        Result<Created> result = await _sut.InsertRangeAsync(artwork, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(Result.Created, result.Value);
        await _context.SaveChangesAsync();
        Assert.Equal(3, await _context.MusicArtwork.CountAsync());
    }

    [Fact]
    public async Task InsertRangeAsync_WhenCollectionIsEmpty_ShouldReturnCreatedWithoutAddingAnyArtwork()
    {
        // Act
        Result<Created> result = await _sut.InsertRangeAsync([], CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(Result.Created, result.Value);
        await _context.SaveChangesAsync();
        Assert.Empty(await _context.MusicArtwork.ToListAsync());
    }

    [Fact]
    public async Task GetByOwnerAsync_WhenCalled_ShouldReturnOnlyTheArtworkOfTheOwnerOrderedByTypeThenOrdinal()
    {
        // Arrange
        MusicArtworkOwnerType ownerType = MusicArtworkOwnerType.Album;
        Guid ownerId = Guid.NewGuid();
        MusicArtworkEntity frontSecond = _musicArtworkEntityFixture.Create(ownerType: ownerType, ownerId: ownerId, artworkType: ArtworkType.Front, ordinal: 1);
        MusicArtworkEntity frontFirst = _musicArtworkEntityFixture.Create(ownerType: ownerType, ownerId: ownerId, artworkType: ArtworkType.Front, ordinal: 0);
        MusicArtworkEntity cover = _musicArtworkEntityFixture.Create(ownerType: ownerType, ownerId: ownerId, artworkType: ArtworkType.Cover, ordinal: 0);
        MusicArtworkEntity artworkOfAnotherOwner = _musicArtworkEntityFixture.Create(ownerType: MusicArtworkOwnerType.Album, ownerId: Guid.NewGuid());
        MusicArtworkEntity artworkOfAnotherOwnerType = _musicArtworkEntityFixture.Create(ownerType: MusicArtworkOwnerType.Artist, ownerId: ownerId);
        _context.MusicArtwork.AddRange(frontSecond, frontFirst, cover, artworkOfAnotherOwner, artworkOfAnotherOwnerType);
        await _context.SaveChangesAsync();

        // Act
        Result<IReadOnlyList<MusicArtworkEntity>> result = await _sut.GetByOwnerAsync(ownerType, ownerId, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal([cover.Id, frontFirst.Id, frontSecond.Id], result.Value.Select(artwork => artwork.Id));
    }

    [Fact]
    public async Task GetByOwnerAsync_WhenTheOwnerHasNoArtwork_ShouldReturnAnEmptyCollection()
    {
        // Act
        Result<IReadOnlyList<MusicArtworkEntity>> result = await _sut.GetByOwnerAsync(MusicArtworkOwnerType.Track, Guid.NewGuid(), CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Empty(result.Value);
    }

    [Fact]
    public async Task DeleteByOwnerAsync_WhenCalled_ShouldDeleteOnlyTheArtworkOfTheOwner()
    {
        // Arrange
        MusicArtworkOwnerType ownerType = MusicArtworkOwnerType.Album;
        Guid ownerId = Guid.NewGuid();
        MusicArtworkEntity artworkOfOwner = _musicArtworkEntityFixture.Create(ownerType: ownerType, ownerId: ownerId);
        MusicArtworkEntity artworkOfAnotherOwner = _musicArtworkEntityFixture.Create(ownerType: ownerType, ownerId: Guid.NewGuid());
        MusicArtworkEntity artworkOfAnotherOwnerType = _musicArtworkEntityFixture.Create(ownerType: MusicArtworkOwnerType.Artist, ownerId: ownerId);
        _context.MusicArtwork.AddRange(artworkOfOwner, artworkOfAnotherOwner, artworkOfAnotherOwnerType);
        await _context.SaveChangesAsync();

        // Act
        Result<Deleted> result = await _sut.DeleteByOwnerAsync(ownerType, ownerId, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(Result.Deleted, result.Value);
        List<MusicArtworkEntity> remainingArtwork = await _context.MusicArtwork.ToListAsync();
        Assert.Equal(2, remainingArtwork.Count);
        Assert.DoesNotContain(remainingArtwork, artwork => artwork.Id == artworkOfOwner.Id);
        Assert.Contains(remainingArtwork, artwork => artwork.Id == artworkOfAnotherOwner.Id);
        Assert.Contains(remainingArtwork, artwork => artwork.Id == artworkOfAnotherOwnerType.Id);
    }

    [Fact]
    public async Task DeleteByOwnerAsync_WhenNoArtworkExistsForTheOwner_ShouldReturnDeleted()
    {
        // Act
        Result<Deleted> result = await _sut.DeleteByOwnerAsync(MusicArtworkOwnerType.Track, Guid.NewGuid(), CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(Result.Deleted, result.Value);
    }

    [Fact]
    public async Task ResetStatusForLibraryAsync_WhenCalled_ShouldResetTheArtworkOfTheLibraryItemsToPending()
    {
        // Arrange
        Guid libraryId = Guid.NewGuid();
        ArtistEntity artist = _artistEntityFixture.Create(libraryId: libraryId, includeAlbums: false, includeContributors: false);
        AlbumEntity album = _albumEntityFixture.Create(artistId: artist.Id, libraryId: libraryId, includeTracks: false, includeMetadata: false);
        TrackEntity track = _trackEntityFixture.Create(albumId: album.Id, libraryId: libraryId, includeMetadata: false);
        ArtistEntity artistOfAnotherLibrary = _artistEntityFixture.Create(includeAlbums: false, includeContributors: false);
        _context.Artists.AddRange(artist, artistOfAnotherLibrary);
        _context.Albums.Add(album);
        _context.Tracks.Add(track);
        MusicArtworkEntity artistArtwork = _musicArtworkEntityFixture.Create(ownerType: MusicArtworkOwnerType.Artist, ownerId: artist.Id, status: ArtworkStatus.Enriched);
        MusicArtworkEntity albumArtwork = _musicArtworkEntityFixture.Create(ownerType: MusicArtworkOwnerType.Album, ownerId: album.Id, status: ArtworkStatus.Enriched);
        MusicArtworkEntity trackArtwork = _musicArtworkEntityFixture.Create(ownerType: MusicArtworkOwnerType.Track, ownerId: track.Id, status: ArtworkStatus.Enriched);
        MusicArtworkEntity artworkOfAnotherLibrary = _musicArtworkEntityFixture.Create(ownerType: MusicArtworkOwnerType.Artist, ownerId: artistOfAnotherLibrary.Id, status: ArtworkStatus.Enriched);
        _context.MusicArtwork.AddRange(artistArtwork, albumArtwork, trackArtwork, artworkOfAnotherLibrary);
        await _context.SaveChangesAsync();

        // Act
        Result<Updated> result = await _sut.ResetStatusForLibraryAsync(libraryId, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(Result.Updated, result.Value);
        // The bulk update bypasses the change tracker, so the tracked entities are detached before the stored values are read back.
        _context.ChangeTracker.Clear();
        Assert.Equal(ArtworkStatus.Pending, (await _context.MusicArtwork.FindAsync(artistArtwork.Id))!.Status);
        Assert.Equal(ArtworkStatus.Pending, (await _context.MusicArtwork.FindAsync(albumArtwork.Id))!.Status);
        Assert.Equal(ArtworkStatus.Pending, (await _context.MusicArtwork.FindAsync(trackArtwork.Id))!.Status);
        Assert.Equal(ArtworkStatus.Enriched, (await _context.MusicArtwork.FindAsync(artworkOfAnotherLibrary.Id))!.Status);
    }

    [Fact]
    public async Task DeleteByLibraryIdAsync_WhenCalled_ShouldDeleteOnlyTheArtworkOfTheLibraryItems()
    {
        // Arrange
        Guid libraryId = Guid.NewGuid();
        ArtistEntity artist = _artistEntityFixture.Create(libraryId: libraryId, includeAlbums: false, includeContributors: false);
        AlbumEntity album = _albumEntityFixture.Create(artistId: artist.Id, libraryId: libraryId, includeTracks: false, includeMetadata: false);
        TrackEntity track = _trackEntityFixture.Create(albumId: album.Id, libraryId: libraryId, includeMetadata: false);
        ArtistEntity artistOfAnotherLibrary = _artistEntityFixture.Create(includeAlbums: false, includeContributors: false);
        AlbumEntity albumOfAnotherLibrary = _albumEntityFixture.Create(
            artistId: artistOfAnotherLibrary.Id, libraryId: artistOfAnotherLibrary.LibraryId, includeTracks: false, includeMetadata: false);
        TrackEntity trackOfAnotherLibrary = _trackEntityFixture.Create(
            albumId: albumOfAnotherLibrary.Id, libraryId: artistOfAnotherLibrary.LibraryId, includeMetadata: false);
        _context.Artists.AddRange(artist, artistOfAnotherLibrary);
        _context.Albums.AddRange(album, albumOfAnotherLibrary);
        _context.Tracks.AddRange(track, trackOfAnotherLibrary);
        MusicArtworkEntity artistArtwork = _musicArtworkEntityFixture.Create(ownerType: MusicArtworkOwnerType.Artist, ownerId: artist.Id);
        MusicArtworkEntity albumArtwork = _musicArtworkEntityFixture.Create(ownerType: MusicArtworkOwnerType.Album, ownerId: album.Id);
        MusicArtworkEntity trackArtwork = _musicArtworkEntityFixture.Create(ownerType: MusicArtworkOwnerType.Track, ownerId: track.Id);
        MusicArtworkEntity artworkOfAnotherLibrary = _musicArtworkEntityFixture.Create(ownerType: MusicArtworkOwnerType.Artist, ownerId: artistOfAnotherLibrary.Id);
        _context.MusicArtwork.AddRange(artistArtwork, albumArtwork, trackArtwork, artworkOfAnotherLibrary);
        await _context.SaveChangesAsync();

        // Act
        Result<Deleted> result = await _sut.DeleteByLibraryIdAsync(libraryId, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(Result.Deleted, result.Value);
        _context.ChangeTracker.Clear();
        // Every artwork owned by an artist, album or track of the library is deleted, while the artwork of another library survives.
        MusicArtworkEntity remainingArtwork = Assert.Single(await _context.MusicArtwork.ToListAsync());
        Assert.Equal(artworkOfAnotherLibrary.Id, remainingArtwork.Id);
    }

    [Fact]
    public async Task DeleteByLibraryIdAsync_WhenTheLibraryHasNoArtwork_ShouldReturnDeleted()
    {
        // Act
        Result<Deleted> result = await _sut.DeleteByLibraryIdAsync(Guid.NewGuid(), CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(Result.Deleted, result.Value);
    }

    [Fact]
    public async Task UpsertCoverAsync_WhenNoCoverExists_ShouldInsertANewCoverArtwork()
    {
        // Arrange
        Guid ownerId = Guid.NewGuid();
        DateTime before = DateTime.UtcNow;

        // Act
        Result<Updated> result = await _sut.UpsertCoverAsync(MusicArtworkOwnerType.Album, ownerId, "covers/new.jpg", CancellationToken.None);
        await _context.SaveChangesAsync();

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(Result.Updated, result.Value);
        MusicArtworkEntity cover = Assert.Single(await _context.MusicArtwork.ToListAsync());
        Assert.Equal(MusicArtworkOwnerType.Album, cover.OwnerType);
        Assert.Equal(ownerId, cover.OwnerId);
        Assert.Equal(ArtworkType.Cover, cover.ArtworkType);
        Assert.Equal(0, cover.Ordinal);
        Assert.Equal("covers/new.jpg", cover.FileName);
        Assert.Equal(ArtworkStatus.Enriched, cover.Status);
        Assert.Equal(Guid.Empty, cover.CreatedBy);
        Assert.True(cover.CreatedOnUtc >= before);
        Assert.Null(cover.UpdatedBy);
    }

    [Fact]
    public async Task UpsertCoverAsync_WhenCoverExists_ShouldUpdateItInPlace()
    {
        // Arrange
        Guid ownerId = Guid.NewGuid();
        MusicArtworkEntity existingCover = _musicArtworkEntityFixture.Create(
            ownerType: MusicArtworkOwnerType.Album, ownerId: ownerId, artworkType: ArtworkType.Cover, ordinal: 0, fileName: "covers/old.jpg",
            status: ArtworkStatus.Pending, includeLastUpdateUtc: false);
        _context.MusicArtwork.Add(existingCover);
        await _context.SaveChangesAsync();
        Guid existingCoverId = existingCover.Id;
        Guid existingCreatedBy = existingCover.CreatedBy;
        DateTime existingCreatedOnUtc = existingCover.CreatedOnUtc;
        _context.ChangeTracker.Clear();

        // Act
        Result<Updated> result = await _sut.UpsertCoverAsync(MusicArtworkOwnerType.Album, ownerId, "covers/new.jpg", CancellationToken.None);
        await _context.SaveChangesAsync();

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(Result.Updated, result.Value);
        // The existing cover is updated in place, so no new row is inserted and the creation audit columns are preserved.
        MusicArtworkEntity cover = Assert.Single(await _context.MusicArtwork.ToListAsync());
        Assert.Equal(existingCoverId, cover.Id);
        Assert.Equal("covers/new.jpg", cover.FileName);
        Assert.Equal(ArtworkStatus.Enriched, cover.Status);
        Assert.NotNull(cover.LastUpdateUtc);
        Assert.Equal(existingCreatedBy, cover.CreatedBy);
        Assert.Equal(existingCreatedOnUtc, cover.CreatedOnUtc);
    }

    [Fact]
    public async Task UpsertCoverAsync_WhenTheCoverHasANonZeroOrdinal_ShouldUpdateItInPlaceInsteadOfInsertingASecondCover()
    {
        // Arrange
        Guid ownerId = Guid.NewGuid();
        // The callers resolve the existing cover by artwork type only, so a cover stored with an ordinal other than zero must still be the one replaced.
        MusicArtworkEntity existingCover = _musicArtworkEntityFixture.Create(
            ownerType: MusicArtworkOwnerType.Album, ownerId: ownerId, artworkType: ArtworkType.Cover, ordinal: 3, fileName: "covers/old.jpg",
            status: ArtworkStatus.Pending, includeLastUpdateUtc: false);
        _context.MusicArtwork.Add(existingCover);
        await _context.SaveChangesAsync();
        Guid existingCoverId = existingCover.Id;
        _context.ChangeTracker.Clear();

        // Act
        Result<Updated> result = await _sut.UpsertCoverAsync(MusicArtworkOwnerType.Album, ownerId, "covers/new.jpg", CancellationToken.None);
        await _context.SaveChangesAsync();

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(Result.Updated, result.Value);
        MusicArtworkEntity cover = Assert.Single(await _context.MusicArtwork.ToListAsync());
        Assert.Equal(existingCoverId, cover.Id);
        Assert.Equal(3, cover.Ordinal);
        Assert.Equal("covers/new.jpg", cover.FileName);
        Assert.Equal(ArtworkStatus.Enriched, cover.Status);
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
