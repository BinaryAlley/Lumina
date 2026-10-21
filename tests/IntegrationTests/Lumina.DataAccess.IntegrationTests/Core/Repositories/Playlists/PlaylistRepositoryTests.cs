#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.MediaLibrary.AudioLibrary.MusicLibrary;
using Lumina.Application.Common.DataAccess.Entities.Playlists;
using Lumina.Application.Common.DataAccess.Entities.UsersManagement;
using Lumina.Application.Common.DTO.Playlists;
using Lumina.Application.Common.Infrastructure.Authentication;
using Lumina.Application.Common.Infrastructure.Time;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.MediaLibrary.AudioLibrary.MusicLibrary;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.Playlists;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.UsersManagement;
using Lumina.DataAccess.Common.Interceptors;
using Lumina.DataAccess.Core.Repositories.Playlists;
using Lumina.DataAccess.Core.UoW;
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.SharedKernel.Common.Enums.Playlists;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DomainErrors = Lumina.Domain.Common.Errors.Errors;
#endregion

namespace Lumina.DataAccess.IntegrationTests.Core.Repositories.Playlists;

/// <summary>
/// Contains integration tests for the <see cref="PlaylistRepository"/> class, exercising it against a real SQLite database.
/// </summary>
[ExcludeFromCodeCoverage]
public class PlaylistRepositoryTests
{
    private readonly PlaylistEntityFixture _playlistEntityFixture = new();
    private readonly PlaylistEntryEntityFixture _playlistEntryEntityFixture = new();
    private readonly UserEntityFixture _userEntityFixture = new();
    private readonly ArtistEntityFixture _artistEntityFixture = new();
    private readonly AlbumEntityFixture _albumEntityFixture = new();
    private readonly TrackEntityFixture _trackEntityFixture = new();

    [Fact]
    public async Task UpdateAsync_WhenNoEditableValueChanged_ShouldNotModifyTheStoredPlaylist()
    {
        // Arrange
        using SqliteConnection anchorConnection = new($"Data Source=luminadataccess-playlistrepo-update-noop-{Guid.NewGuid()};Mode=Memory;Cache=Shared");
        anchorConnection.Open();
        Guid userId = Guid.NewGuid();
        DateTime utcNow = new(2025, 1, 1, 12, 0, 0, DateTimeKind.Utc);
        using LuminaDbContext context = CreateAuditedContext(anchorConnection, userId, utcNow);
        PlaylistRepository sut = new(context);
        context.Users.Add(_userEntityFixture.Create(id: userId));
        PlaylistEntity playlist = _playlistEntityFixture.Create(userId: userId, type: PlaylistType.Music, entryCount: 7);
        context.Playlists.Add(playlist);
        await context.SaveChangesAsync();
        long storedVersion = playlist.Version;
        // A detached copy is the same shape the command mapping produces for an edit, so this is a real no-op round-trip instead of an object built only to satisfy the repository signature.
        PlaylistEntity incoming = await context.Playlists
            .AsNoTracking()
            .FirstAsync(candidate => candidate.Id == playlist.Id);

        // Act
        Result<Updated> result = await sut.UpdateAsync(incoming, CancellationToken.None);
        await context.SaveChangesAsync();

        // Assert
        Assert.False(result.IsFailure);
        PlaylistEntity storedPlaylist = await context.Playlists
            .AsNoTracking()
            .FirstAsync(candidate => candidate.Id == playlist.Id);
        Assert.Equal(storedVersion, storedPlaylist.Version);
        Assert.Equal(7, storedPlaylist.EntryCount);
        Assert.Null(storedPlaylist.UpdatedOnUtc);
    }

    [Fact]
    public async Task UpdateAsync_WhenOnlyTheNameChanged_ShouldIncrementTheVersionAndPreserveTheEntryCount()
    {
        // Arrange
        using SqliteConnection anchorConnection = new($"Data Source=luminadataccess-playlistrepo-update-name-{Guid.NewGuid()};Mode=Memory;Cache=Shared");
        anchorConnection.Open();
        Guid userId = Guid.NewGuid();
        DateTime utcNow = new(2025, 1, 1, 12, 0, 0, DateTimeKind.Utc);
        using LuminaDbContext context = CreateAuditedContext(anchorConnection, userId, utcNow);
        PlaylistRepository sut = new(context);
        context.Users.Add(_userEntityFixture.Create(id: userId));
        PlaylistEntity playlist = _playlistEntityFixture.Create(userId: userId, type: PlaylistType.Music, entryCount: 7);
        context.Playlists.Add(playlist);
        await context.SaveChangesAsync();
        long storedVersion = playlist.Version;
        // The mapped edit path does not load the entries, so its entry count is zero. The test proves the repository preserves the stored count instead of copying that zero onto the playlist.
        PlaylistEntity incoming = _playlistEntityFixture.Create(
            id: playlist.Id,
            userId: userId,
            name: "Renamed playlist",
            type: PlaylistType.Music,
            entryCount: 0);

        // Act
        Result<Updated> result = await sut.UpdateAsync(incoming, CancellationToken.None);
        await context.SaveChangesAsync();

        // Assert
        Assert.False(result.IsFailure);
        PlaylistEntity storedPlaylist = await context.Playlists
            .AsNoTracking()
            .FirstAsync(candidate => candidate.Id == playlist.Id);
        Assert.Equal("Renamed playlist", storedPlaylist.Name);
        Assert.Equal(storedVersion + 1, storedPlaylist.Version);
        Assert.Equal(7, storedPlaylist.EntryCount);
        Assert.Equal(utcNow, storedPlaylist.UpdatedOnUtc);
    }

    [Fact]
    public async Task GetAllForUserAsync_WhenTheUserHasPlaylists_ShouldReturnTheirStoredEntryCounts()
    {
        // Arrange
        using SqliteConnection anchorConnection = new($"Data Source=luminadataccess-playlistrepo-list-{Guid.NewGuid()};Mode=Memory;Cache=Shared");
        anchorConnection.Open();
        using LuminaDbContext context = CreateContext(anchorConnection);
        PlaylistRepository sut = new(context);
        Guid userId = Guid.NewGuid();
        context.Users.Add(_userEntityFixture.Create(id: userId));
        PlaylistEntity firstPlaylist = _playlistEntityFixture.Create(userId: userId, type: PlaylistType.Music, entryCount: 7);
        PlaylistEntity secondPlaylist = _playlistEntityFixture.Create(userId: userId, type: PlaylistType.Music, entryCount: 0);
        context.Playlists.AddRange(firstPlaylist, secondPlaylist);
        await context.SaveChangesAsync();

        // Act
        Result<IReadOnlyList<PlaylistSummaryDto>> result = await sut.GetAllForUserAsync(userId, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(2, result.Value.Count);
        Assert.Contains(result.Value, playlist => playlist.Id == firstPlaylist.Id && playlist.EntryCount == 7);
        Assert.Contains(result.Value, playlist => playlist.Id == secondPlaylist.Id && playlist.EntryCount == 0);
    }

    [Fact]
    public async Task AddScopeEntriesAsync_WhenTheWholeLibraryIsAppended_ShouldInsertCatalogueOrderAndUpdateTheEntryCount()
    {
        // Arrange
        using SqliteConnection anchorConnection = new($"Data Source=luminadataccess-playlistrepo-scope-{Guid.NewGuid()};Mode=Memory;Cache=Shared");
        anchorConnection.Open();
        using LuminaDbContext context = CreateContext(anchorConnection);
        PlaylistRepository sut = new(context);
        Guid userId = Guid.NewGuid();
        context.Users.Add(_userEntityFixture.Create(id: userId));
        Guid libraryId = Guid.NewGuid();
        List<TrackEntity> orderedTracks = await SeedCatalogueAsync(context, libraryId);
        PlaylistEntity playlist = _playlistEntityFixture.Create(userId: userId, type: PlaylistType.Music, entryCount: 0, version: 0);
        context.Playlists.Add(playlist);
        await context.SaveChangesAsync();
        DateTime updatedOnUtc = new(2025, 1, 1, 12, 0, 0, DateTimeKind.Utc);

        // Act
        Result<PlaylistBulkAddResultDto> result = await sut.AddScopeEntriesAsync(
            playlist.Id,
            userId,
            libraryId,
            artistId: null,
            albumId: null,
            PlaylistMediaItemType.MusicTrack,
            updatedOnUtc,
            CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(orderedTracks.Count, result.Value.AddedCount);
        List<PlaylistEntryEntity> storedEntries = await context.PlaylistEntries
            .AsNoTracking()
            .Where(entry => entry.PlaylistId == playlist.Id)
            .OrderBy(entry => entry.Position)
            .ToListAsync();
        Assert.Equal(orderedTracks.Select(track => track.Id), storedEntries.Select(entry => entry.MediaItemId));
        Assert.All(storedEntries, entry => Assert.NotEqual(Guid.Empty, entry.Id));
        // The generated entry Ids must be stored in the same text form the provider uses, so the appended entry can still be read back by
        // its Id through a normal Entity Framework parameter. It is the last entry, so its display position comes from the stored count.
        Result<PlaylistEntryDto?> storedEntryResult = await sut.GetAppendedEntryDetailsForUserAsync(playlist.Id, userId, storedEntries[^1].Id, CancellationToken.None);
        Assert.False(storedEntryResult.IsFailure);
        Assert.NotNull(storedEntryResult.Value);
        Assert.Equal(orderedTracks.Count - 1, storedEntryResult.Value!.DisplayPosition);
        Assert.NotNull(storedEntryResult.Value.Display);
        PlaylistEntity storedPlaylist = await context.Playlists
            .AsNoTracking()
            .FirstAsync(candidate => candidate.Id == playlist.Id);
        Assert.Equal(orderedTracks.Count, storedPlaylist.EntryCount);
        Assert.Equal(1L, storedPlaylist.Version);
        Assert.Equal(updatedOnUtc, storedPlaylist.UpdatedOnUtc);
        // A window read reports the stored count of the whole playlist while materializing only the requested window.
        Result<PlaylistDetailsDto?> detailsResult = await sut.GetDetailsForUserAsync(playlist.Id, userId, offset: 0, limit: 2, CancellationToken.None);
        Assert.False(detailsResult.IsFailure);
        Assert.NotNull(detailsResult.Value);
        Assert.Equal(orderedTracks.Count, detailsResult.Value!.Playlist.EntryCount);
        Assert.Equal(2, detailsResult.Value.Entries.Count);
    }

    [Fact]
    public async Task AddScopeEntriesAsync_WhenAnAlbumIsSpecified_ShouldAppendOnlyThatAlbum()
    {
        // Arrange
        using SqliteConnection anchorConnection = new($"Data Source=luminadataccess-playlistrepo-scope-album-{Guid.NewGuid()};Mode=Memory;Cache=Shared");
        anchorConnection.Open();
        using LuminaDbContext context = CreateContext(anchorConnection);
        PlaylistRepository sut = new(context);
        Guid userId = Guid.NewGuid();
        context.Users.Add(_userEntityFixture.Create(id: userId));
        Guid libraryId = Guid.NewGuid();
        List<TrackEntity> orderedTracks = await SeedCatalogueAsync(context, libraryId);
        AlbumEntity album = await context.Albums.FirstAsync(candidate => candidate.Id == orderedTracks[0].AlbumId);
        PlaylistEntity playlist = _playlistEntityFixture.Create(userId: userId, type: PlaylistType.Music, entryCount: 0, version: 0);
        context.Playlists.Add(playlist);
        await context.SaveChangesAsync();

        // Act
        Result<PlaylistBulkAddResultDto> result = await sut.AddScopeEntriesAsync(
            playlist.Id,
            userId,
            libraryId,
            artistId: album.ArtistId,
            albumId: album.Id,
            PlaylistMediaItemType.MusicTrack,
            DateTime.UtcNow,
            CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        List<Guid> storedMediaItemIds = await context.PlaylistEntries
            .AsNoTracking()
            .Where(entry => entry.PlaylistId == playlist.Id)
            .OrderBy(entry => entry.Position)
            .Select(entry => entry.MediaItemId)
            .ToListAsync();
        Assert.Equal(
            orderedTracks.Where(track => track.AlbumId == album.Id).Select(track => track.Id),
            storedMediaItemIds);
    }

    [Fact]
    public async Task AddEntriesAsync_WhenALaterBatchFails_ShouldLeaveNoPrefixWhenTheAmbientTransactionIsRolledBack()
    {
        // Arrange
        using SqliteConnection anchorConnection = new($"Data Source=luminadataccess-playlistrepo-batch-rollback-{Guid.NewGuid()};Mode=Memory;Cache=Shared");
        anchorConnection.Open();
        using LuminaDbContext context = CreateContext(anchorConnection);
        PlaylistRepository sut = new(context);
        Guid userId = Guid.NewGuid();
        context.Users.Add(_userEntityFixture.Create(id: userId));
        PlaylistEntity playlist = _playlistEntityFixture.Create(userId: userId, type: PlaylistType.Music, entryCount: 0, version: 0);
        context.Playlists.Add(playlist);
        await context.SaveChangesAsync();
        List<Guid> mediaItemIds = [.. Enumerable.Range(0, 501).Select(_ => Guid.NewGuid())];
        List<Guid> entryIds = [.. mediaItemIds.Select(_ => Guid.NewGuid())];
        // The duplicate in the second batch violates the primary key after the first batch was already inserted.
        entryIds[500] = entryIds[0];

        // Act
        // This is the transaction the request middleware owns in production. The repository must not take a decision about it: the middleware rolls it back once the response is known to be unsuccessful.
        using IDbContextTransaction ambientTransaction = await context.Database.BeginTransactionAsync();
        // A primary key violation is not one of the mapped persistence conflicts, so it takes the generic failure path.
        await Assert.ThrowsAsync<SqliteException>(() => sut.AddEntriesAsync(
            playlist.Id,
            userId,
            mediaItemIds,
            PlaylistMediaItemType.MusicTrack,
            entryIds,
            DateTime.UtcNow,
            CancellationToken.None));

        // The middleware rolls the request transaction back after a failed response.
        await ambientTransaction.RollbackAsync();

        // Assert
        // The ambient transaction has been rolled back, so another connection can read the database and sees no partial prefix.
        using LuminaDbContext verificationContext = CreateContext(anchorConnection);
        Assert.Equal(0, await verificationContext.PlaylistEntries.CountAsync(entry => entry.PlaylistId == playlist.Id));
        PlaylistEntity storedPlaylist = await verificationContext.Playlists
            .AsNoTracking()
            .FirstAsync(candidate => candidate.Id == playlist.Id);
        Assert.Equal(0, storedPlaylist.EntryCount);
        Assert.Equal(0L, storedPlaylist.Version);
    }

    [Fact]
    public async Task DeleteByIdAsync_WhenThePlaylistExists_ShouldRemoveThePlaylistAndItsEntries()
    {
        // Arrange
        using SqliteConnection anchorConnection = new($"Data Source=luminadataccess-playlistrepo-delete-{Guid.NewGuid()};Mode=Memory;Cache=Shared");
        anchorConnection.Open();
        using LuminaDbContext context = CreateContext(anchorConnection);
        PlaylistRepository sut = new(context);
        Guid userId = Guid.NewGuid();
        context.Users.Add(_userEntityFixture.Create(id: userId));
        Guid playlistId = Guid.NewGuid();
        PlaylistEntryEntity firstEntry = _playlistEntryEntityFixture.Create(playlistId: playlistId, position: 1_000_000_000L);
        PlaylistEntryEntity secondEntry = _playlistEntryEntityFixture.Create(playlistId: playlistId, position: 2_000_000_000L);
        PlaylistEntity playlist = _playlistEntityFixture.Create(
            id: playlistId,
            userId: userId,
            type: PlaylistType.Music,
            entries: [firstEntry, secondEntry],
            entryCount: 2);
        context.Playlists.Add(playlist);
        await context.SaveChangesAsync();
        // A real delete request starts with no tracked entries, so the database cascade is the path that has to remove them.
        context.ChangeTracker.Clear();

        // Act
        Result<Deleted> result = await sut.DeleteByIdAsync(playlistId, CancellationToken.None);
        await context.SaveChangesAsync();

        // Assert
        Assert.False(result.IsFailure);
        Assert.False(await context.Playlists.AsNoTracking().AnyAsync(candidate => candidate.Id == playlistId));
        Assert.False(await context.PlaylistEntries.AsNoTracking().AnyAsync(entry => entry.PlaylistId == playlistId));
    }

    [Fact]
    public async Task DeleteByIdAsync_WhenThePlaylistDoesNotExist_ShouldReturnPlaylistNotFound()
    {
        // Arrange
        using SqliteConnection anchorConnection = new($"Data Source=luminadataccess-playlistrepo-delete-missing-{Guid.NewGuid()};Mode=Memory;Cache=Shared");
        anchorConnection.Open();
        using LuminaDbContext context = CreateContext(anchorConnection);
        PlaylistRepository sut = new(context);

        // Act
        Result<Deleted> result = await sut.DeleteByIdAsync(Guid.NewGuid(), CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(DomainErrors.Playlists.PlaylistNotFound, result.FirstError);
    }

    [Fact]
    public async Task MoveEntryAsync_WhenMovingAnEntry_ShouldUpdateItsPositionWithoutChangingTheEntryCount()
    {
        // Arrange
        using SqliteConnection anchorConnection = new($"Data Source=luminadataccess-playlistrepo-move-{Guid.NewGuid()};Mode=Memory;Cache=Shared");
        anchorConnection.Open();
        using LuminaDbContext context = CreateContext(anchorConnection);
        PlaylistRepository sut = new(context);
        Guid userId = Guid.NewGuid();
        context.Users.Add(_userEntityFixture.Create(id: userId));
        Guid playlistId = Guid.NewGuid();
        PlaylistEntryEntity firstEntry = _playlistEntryEntityFixture.Create(playlistId: playlistId, position: 1_000_000_000L);
        PlaylistEntryEntity secondEntry = _playlistEntryEntityFixture.Create(playlistId: playlistId, position: 2_000_000_000L);
        PlaylistEntryEntity thirdEntry = _playlistEntryEntityFixture.Create(playlistId: playlistId, position: 3_000_000_000L);
        PlaylistEntity playlist = _playlistEntityFixture.Create(
            id: playlistId,
            userId: userId,
            type: PlaylistType.Music,
            entries: [firstEntry, secondEntry, thirdEntry],
            entryCount: 3,
            version: 0);
        context.Playlists.Add(playlist);
        await context.SaveChangesAsync();

        // Act
        Result<Updated> result = await sut.MoveEntryAsync(playlistId, userId, thirdEntry.Id, targetPosition: 0, DateTime.UtcNow, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        List<Guid> orderedMediaItemIds = await context.PlaylistEntries
            .AsNoTracking()
            .Where(entry => entry.PlaylistId == playlistId)
            .OrderBy(entry => entry.Position)
            .Select(entry => entry.MediaItemId)
            .ToListAsync();
        Assert.Equal([thirdEntry.MediaItemId, firstEntry.MediaItemId, secondEntry.MediaItemId], orderedMediaItemIds);
        PlaylistEntity storedPlaylist = await context.Playlists
            .AsNoTracking()
            .FirstAsync(candidate => candidate.Id == playlistId);
        Assert.Equal(3, storedPlaylist.EntryCount);
        Assert.Equal(1L, storedPlaylist.Version);
    }

    [Fact]
    public async Task RemoveEntryAsync_WhenTheEntryExists_ShouldDecrementTheStoredEntryCount()
    {
        // Arrange
        using SqliteConnection anchorConnection = new($"Data Source=luminadataccess-playlistrepo-remove-{Guid.NewGuid()};Mode=Memory;Cache=Shared");
        anchorConnection.Open();
        using LuminaDbContext context = CreateContext(anchorConnection);
        PlaylistRepository sut = new(context);
        Guid userId = Guid.NewGuid();
        context.Users.Add(_userEntityFixture.Create(id: userId));
        Guid playlistId = Guid.NewGuid();
        PlaylistEntryEntity firstEntry = _playlistEntryEntityFixture.Create(playlistId: playlistId, position: 1_000_000_000L);
        PlaylistEntryEntity secondEntry = _playlistEntryEntityFixture.Create(playlistId: playlistId, position: 2_000_000_000L);
        PlaylistEntity playlist = _playlistEntityFixture.Create(
            id: playlistId,
            userId: userId,
            type: PlaylistType.Music,
            entries: [firstEntry, secondEntry],
            entryCount: 2,
            version: 0);
        context.Playlists.Add(playlist);
        await context.SaveChangesAsync();

        // Act
        Result<int> result = await sut.RemoveEntryAsync(playlistId, userId, firstEntry.Id, DateTime.UtcNow, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        PlaylistEntity storedPlaylist = await context.Playlists
            .AsNoTracking()
            .FirstAsync(candidate => candidate.Id == playlistId);
        Assert.Equal(1, storedPlaylist.EntryCount);
        Assert.Equal(1L, storedPlaylist.Version);
        Assert.False(await context.PlaylistEntries.AsNoTracking().AnyAsync(entry => entry.Id == firstEntry.Id));
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
    /// Creates a real SQLite backed context without the auditing interceptor.
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

    /// <summary>
    /// Seeds two artists and three albums with four tracks in total, and returns the tracks in the order a playlist must receive them.
    /// </summary>
    /// <param name="context">The context to seed.</param>
    /// <param name="libraryId">The Id of the library the catalogue belongs to.</param>
    /// <returns>The tracks in artist, album, disc and track order.</returns>
    private async Task<List<TrackEntity>> SeedCatalogueAsync(LuminaDbContext context, Guid libraryId)
    {
        ArtistEntity betaArtist = _artistEntityFixture.Create(libraryId: libraryId, name: "Beta Artist", includeAlbums: false, includeContributors: false);
        ArtistEntity alphaArtist = _artistEntityFixture.Create(libraryId: libraryId, name: "Alpha Artist", includeAlbums: false, includeContributors: false);
        context.Artists.AddRange(betaArtist, alphaArtist);
        await context.SaveChangesAsync();

        AlbumEntity betaAlbum = _albumEntityFixture.Create(artistId: betaArtist.Id, libraryId: libraryId, title: "Only Album", includeTracks: false, includeMetadata: false);
        AlbumEntity alphaZetaAlbum = _albumEntityFixture.Create(artistId: alphaArtist.Id, libraryId: libraryId, title: "Zeta", includeTracks: false, includeMetadata: false);
        AlbumEntity alphaAlphaAlbum = _albumEntityFixture.Create(artistId: alphaArtist.Id, libraryId: libraryId, title: "Alpha", includeTracks: false, includeMetadata: false);
        context.Albums.AddRange(betaAlbum, alphaZetaAlbum, alphaAlphaAlbum);
        await context.SaveChangesAsync();

        TrackEntity betaTrack = _trackEntityFixture.Create(albumId: betaAlbum.Id, libraryId: libraryId, title: "Beta One", trackNumber: 1, includeMetadata: false);
        TrackEntity alphaZetaTrack = _trackEntityFixture.Create(albumId: alphaZetaAlbum.Id, libraryId: libraryId, title: "Alpha Zeta One", trackNumber: 1, includeMetadata: false);
        TrackEntity alphaAlphaSecondTrack = _trackEntityFixture.Create(albumId: alphaAlphaAlbum.Id, libraryId: libraryId, title: "Alpha Alpha Two", trackNumber: 2, includeMetadata: false);
        TrackEntity alphaAlphaFirstTrack = _trackEntityFixture.Create(albumId: alphaAlphaAlbum.Id, libraryId: libraryId, title: "Alpha Alpha One", trackNumber: 1, includeMetadata: false);
        // The fixture randomizes the disc number, so it is pinned here to make the track order of the album deterministic.
        betaTrack.DiscNumber = 1;
        alphaZetaTrack.DiscNumber = 1;
        alphaAlphaSecondTrack.DiscNumber = 1;
        alphaAlphaFirstTrack.DiscNumber = 1;
        // The tracks are deliberately inserted out of catalogue order, so the test proves that the database produces the playlist order.
        context.Tracks.AddRange(betaTrack, alphaZetaTrack, alphaAlphaSecondTrack, alphaAlphaFirstTrack);
        await context.SaveChangesAsync();

        return [alphaAlphaFirstTrack, alphaAlphaSecondTrack, alphaZetaTrack, betaTrack];
    }
}
