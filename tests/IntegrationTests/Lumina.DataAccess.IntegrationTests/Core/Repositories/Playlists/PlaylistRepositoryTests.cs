#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.MediaLibrary.AudioLibrary.MusicLibrary;
using Lumina.Application.Common.DataAccess.Entities.Playlists;
using Lumina.Application.Common.DataAccess.Entities.UsersManagement;
using Lumina.Application.Common.DTO.Pagination;
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
using Lumina.Domain.SharedKernel.Common.Enums.AudioLibrary;
using Lumina.Domain.SharedKernel.Common.Enums.MediaLibrary;
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
    private readonly MusicArtworkEntityFixture _musicArtworkEntityFixture = new();

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
        // A paginated read reports the count of the whole playlist while materializing only the requested page.
        Result<PlaylistDetailsDto?> detailsResult = await sut.GetDetailsForUserAsync(
            playlist.Id,
            userId,
            new PaginationDataDto { CurrentPage = 1, PerPage = 2 },
            CancellationToken.None);
        Assert.False(detailsResult.IsFailure);
        Assert.NotNull(detailsResult.Value);
        Assert.Equal(orderedTracks.Count, detailsResult.Value!.Playlist.EntryCount);
        Assert.Equal(orderedTracks.Count, detailsResult.Value.Entries.Count);
        Assert.Equal(2, detailsResult.Value.Entries.Data.Count);
        Assert.Equal(1, detailsResult.Value.Entries.CurrentPage);
        Assert.Equal(2, detailsResult.Value.Entries.PerPage);
        Assert.Equal((int)Math.Ceiling(orderedTracks.Count / 2d), detailsResult.Value.Entries.NumberOfPages);
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

    [Fact]
    public async Task GetDetailsForUserAsync_WhenThePlaylistDoesNotExist_ShouldReturnSuccessWithNull()
    {
        // Arrange
        using SqliteConnection anchorConnection = new($"Data Source=luminadataccess-playlistrepo-details-missing-{Guid.NewGuid()};Mode=Memory;Cache=Shared");
        anchorConnection.Open();
        using LuminaDbContext context = CreateContext(anchorConnection);
        PlaylistRepository sut = new(context);

        // Act
        Result<PlaylistDetailsDto?> result = await sut.GetDetailsForUserAsync(Guid.NewGuid(), Guid.NewGuid(), paginationData: null, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Null(result.Value);
    }

    [Fact]
    public async Task GetDetailsForUserAsync_WhenThePlaylistBelongsToAnotherUser_ShouldReturnSuccessWithNull()
    {
        // Arrange
        using SqliteConnection anchorConnection = new($"Data Source=luminadataccess-playlistrepo-details-other-user-{Guid.NewGuid()};Mode=Memory;Cache=Shared");
        anchorConnection.Open();
        using LuminaDbContext context = CreateContext(anchorConnection);
        PlaylistRepository sut = new(context);
        Guid ownerUserId = Guid.NewGuid();
        context.Users.Add(_userEntityFixture.Create(id: ownerUserId));
        (PlaylistEntity playlist, _) = await SeedOrderedPlaylistAsync(context, ownerUserId, entryCount: 2);

        // Act
        Result<PlaylistDetailsDto?> result = await sut.GetDetailsForUserAsync(playlist.Id, Guid.NewGuid(), paginationData: null, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Null(result.Value);
    }

    [Fact]
    public async Task GetDetailsForUserAsync_WhenNoPaginationIsRequested_ShouldReturnEveryEntryWithDenseDisplayPositions()
    {
        // Arrange
        using SqliteConnection anchorConnection = new($"Data Source=luminadataccess-playlistrepo-details-all-{Guid.NewGuid()};Mode=Memory;Cache=Shared");
        anchorConnection.Open();
        using LuminaDbContext context = CreateContext(anchorConnection);
        PlaylistRepository sut = new(context);
        Guid userId = Guid.NewGuid();
        context.Users.Add(_userEntityFixture.Create(id: userId));
        (PlaylistEntity playlist, List<PlaylistEntryEntity> entries) = await SeedOrderedPlaylistAsync(context, userId, entryCount: 3);

        // Act
        Result<PlaylistDetailsDto?> result = await sut.GetDetailsForUserAsync(playlist.Id, userId, paginationData: null, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.NotNull(result.Value);
        Assert.Equal([0, 1, 2], result.Value!.Entries.Data.Select(entry => entry.DisplayPosition));
        Assert.Equal(entries.Select(entry => entry.Id), result.Value.Entries.Data.Select(entry => entry.Entry.Id));
        Assert.Equal(1, result.Value.Entries.CurrentPage);
        Assert.Equal(3, result.Value.Entries.PerPage);
        Assert.Equal(3, result.Value.Entries.Count);
        Assert.Equal(1, result.Value.Entries.NumberOfPages);
    }

    [Fact]
    public async Task GetDetailsForUserAsync_WhenAPageIsRequested_ShouldReturnOnlyThatPageWithDenseDisplayPositions()
    {
        // Arrange
        using SqliteConnection anchorConnection = new($"Data Source=luminadataccess-playlistrepo-details-page-{Guid.NewGuid()};Mode=Memory;Cache=Shared");
        anchorConnection.Open();
        using LuminaDbContext context = CreateContext(anchorConnection);
        PlaylistRepository sut = new(context);
        Guid userId = Guid.NewGuid();
        context.Users.Add(_userEntityFixture.Create(id: userId));
        (PlaylistEntity playlist, List<PlaylistEntryEntity> entries) = await SeedOrderedPlaylistAsync(context, userId, entryCount: 5);
        PaginationDataDto firstPage = new() { CurrentPage = 1, PerPage = 2 };
        PaginationDataDto secondPage = new() { CurrentPage = 2, PerPage = 2 };

        // Act
        Result<PlaylistDetailsDto?> firstPageResult = await sut.GetDetailsForUserAsync(playlist.Id, userId, firstPage, CancellationToken.None);
        Result<PlaylistDetailsDto?> secondPageResult = await sut.GetDetailsForUserAsync(playlist.Id, userId, secondPage, CancellationToken.None);

        // Assert
        Assert.False(firstPageResult.IsFailure);
        Assert.NotNull(firstPageResult.Value);
        Assert.Equal([0, 1], firstPageResult.Value!.Entries.Data.Select(entry => entry.DisplayPosition));
        Assert.Equal([entries[0].Id, entries[1].Id], firstPageResult.Value.Entries.Data.Select(entry => entry.Entry.Id));
        Assert.Equal(1, firstPageResult.Value.Entries.CurrentPage);
        Assert.Equal(2, firstPageResult.Value.Entries.PerPage);
        Assert.Equal(5, firstPageResult.Value.Entries.Count);
        Assert.Equal(3, firstPageResult.Value.Entries.NumberOfPages);

        Assert.False(secondPageResult.IsFailure);
        Assert.NotNull(secondPageResult.Value);
        // The display positions continue from the offset of the page, so the second page starts at 2 and not at 0.
        Assert.Equal([2, 3], secondPageResult.Value!.Entries.Data.Select(entry => entry.DisplayPosition));
        Assert.Equal([entries[2].Id, entries[3].Id], secondPageResult.Value.Entries.Data.Select(entry => entry.Entry.Id));
        Assert.Equal(2, secondPageResult.Value.Entries.CurrentPage);
        Assert.Equal(5, secondPageResult.Value.Entries.Count);
        Assert.Equal(3, secondPageResult.Value.Entries.NumberOfPages);
    }

    [Fact]
    public async Task GetDetailsForUserAsync_WhenTheRequestedPageExceedsTheLastPage_ShouldClampToTheLastPage()
    {
        // Arrange
        using SqliteConnection anchorConnection = new($"Data Source=luminadataccess-playlistrepo-details-clamp-{Guid.NewGuid()};Mode=Memory;Cache=Shared");
        anchorConnection.Open();
        using LuminaDbContext context = CreateContext(anchorConnection);
        PlaylistRepository sut = new(context);
        Guid userId = Guid.NewGuid();
        context.Users.Add(_userEntityFixture.Create(id: userId));
        (PlaylistEntity playlist, List<PlaylistEntryEntity> entries) = await SeedOrderedPlaylistAsync(context, userId, entryCount: 5);
        PaginationDataDto beyondLastPage = new() { CurrentPage = 99, PerPage = 2 };

        // Act
        Result<PlaylistDetailsDto?> result = await sut.GetDetailsForUserAsync(playlist.Id, userId, beyondLastPage, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.NotNull(result.Value);
        Assert.Equal(3, result.Value!.Entries.CurrentPage);
        PlaylistEntryDto lastEntry = Assert.Single(result.Value.Entries.Data);
        Assert.Equal(entries[4].Id, lastEntry.Entry.Id);
        Assert.Equal(4, lastEntry.DisplayPosition);
    }

    [Fact]
    public async Task GetDetailsForUserAsync_WhenThePlaylistIsEmptyAndAPageIsRequested_ShouldReturnAnEmptyPage()
    {
        // Arrange
        using SqliteConnection anchorConnection = new($"Data Source=luminadataccess-playlistrepo-details-empty-page-{Guid.NewGuid()};Mode=Memory;Cache=Shared");
        anchorConnection.Open();
        using LuminaDbContext context = CreateContext(anchorConnection);
        PlaylistRepository sut = new(context);
        Guid userId = Guid.NewGuid();
        context.Users.Add(_userEntityFixture.Create(id: userId));
        (PlaylistEntity playlist, _) = await SeedOrderedPlaylistAsync(context, userId, entryCount: 0);
        PaginationDataDto paginationData = new() { CurrentPage = 2, PerPage = 3 };

        // Act
        Result<PlaylistDetailsDto?> result = await sut.GetDetailsForUserAsync(playlist.Id, userId, paginationData, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.NotNull(result.Value);
        Assert.Empty(result.Value!.Entries.Data);
        Assert.Equal(0, result.Value.Entries.Count);
        Assert.Equal(0, result.Value.Entries.NumberOfPages);
        Assert.Equal(1, result.Value.Entries.CurrentPage);
        Assert.Equal(3, result.Value.Entries.PerPage);
    }

    [Fact]
    public async Task GetDetailsForUserAsync_WhenThePlaylistIsEmptyAndNoPaginationIsRequested_ShouldReturnAnEmptyResultWithOnePage()
    {
        // Arrange
        using SqliteConnection anchorConnection = new($"Data Source=luminadataccess-playlistrepo-details-empty-{Guid.NewGuid()};Mode=Memory;Cache=Shared");
        anchorConnection.Open();
        using LuminaDbContext context = CreateContext(anchorConnection);
        PlaylistRepository sut = new(context);
        Guid userId = Guid.NewGuid();
        context.Users.Add(_userEntityFixture.Create(id: userId));
        (PlaylistEntity playlist, _) = await SeedOrderedPlaylistAsync(context, userId, entryCount: 0);

        // Act
        Result<PlaylistDetailsDto?> result = await sut.GetDetailsForUserAsync(playlist.Id, userId, paginationData: null, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.NotNull(result.Value);
        Assert.Empty(result.Value!.Entries.Data);
        Assert.Equal(0, result.Value.Entries.PerPage);
        Assert.Equal(0, result.Value.Entries.Count);
        Assert.Equal(1, result.Value.Entries.NumberOfPages);
        Assert.Equal(1, result.Value.Entries.CurrentPage);
    }

    [Fact]
    public async Task GetDetailsForUserAsync_WhenAnEntryReferencesAMissingTrack_ShouldKeepTheEntryWithoutDisplayData()
    {
        // Arrange
        using SqliteConnection anchorConnection = new($"Data Source=luminadataccess-playlistrepo-details-missing-track-{Guid.NewGuid()};Mode=Memory;Cache=Shared");
        anchorConnection.Open();
        using LuminaDbContext context = CreateContext(anchorConnection);
        PlaylistRepository sut = new(context);
        Guid userId = Guid.NewGuid();
        Guid libraryId = Guid.NewGuid();
        context.Users.Add(_userEntityFixture.Create(id: userId));
        (_, _, TrackEntity track) = await SeedTrackAsync(context, libraryId, artistName: "Existing Artist", albumTitle: "Existing Album", trackTitle: "Existing Track", coverFileName: "existing-cover.jpg");
        Guid playlistId = Guid.NewGuid();
        PlaylistEntryEntity existingEntry = _playlistEntryEntityFixture.Create(
            playlistId: playlistId,
            mediaItemId: track.Id,
            mediaItemType: PlaylistMediaItemType.MusicTrack,
            position: 1_000_000_000L);
        PlaylistEntryEntity missingEntry = _playlistEntryEntityFixture.Create(
            playlistId: playlistId,
            mediaItemId: Guid.NewGuid(),
            mediaItemType: PlaylistMediaItemType.MusicTrack,
            position: 2_000_000_000L);
        PlaylistEntity playlist = _playlistEntityFixture.Create(
            id: playlistId,
            userId: userId,
            type: PlaylistType.Music,
            entries: [existingEntry, missingEntry],
            entryCount: 2,
            version: 0);
        context.Playlists.Add(playlist);
        await context.SaveChangesAsync();

        // Act
        Result<PlaylistDetailsDto?> result = await sut.GetDetailsForUserAsync(playlistId, userId, paginationData: null, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.NotNull(result.Value);
        Assert.Equal(2, result.Value!.Entries.Data.Count);
        PlaylistEntryDto missingItem = Assert.Single(result.Value.Entries.Data, entry => entry.Entry.Id == missingEntry.Id);
        Assert.Null(missingItem.Display);
        PlaylistEntryDto existingItem = Assert.Single(result.Value.Entries.Data, entry => entry.Entry.Id == existingEntry.Id);
        Assert.NotNull(existingItem.Display);
    }

    [Fact]
    public async Task GetDetailsForUserAsync_WhenTheEntriesReferenceMusicTracks_ShouldMapTheirDisplayData()
    {
        // Arrange
        using SqliteConnection anchorConnection = new($"Data Source=luminadataccess-playlistrepo-details-display-{Guid.NewGuid()};Mode=Memory;Cache=Shared");
        anchorConnection.Open();
        using LuminaDbContext context = CreateContext(anchorConnection);
        PlaylistRepository sut = new(context);
        Guid userId = Guid.NewGuid();
        Guid libraryId = Guid.NewGuid();
        context.Users.Add(_userEntityFixture.Create(id: userId));
        string coverFileName = "album-cover.jpg";
        (ArtistEntity artist, AlbumEntity album, TrackEntity track) = await SeedTrackAsync(
            context,
            libraryId,
            artistName: "Display Artist",
            albumTitle: "Display Album",
            trackTitle: "Display Track",
            coverFileName: coverFileName);
        Guid playlistId = Guid.NewGuid();
        PlaylistEntryEntity entry = _playlistEntryEntityFixture.Create(
            playlistId: playlistId,
            mediaItemId: track.Id,
            mediaItemType: PlaylistMediaItemType.MusicTrack,
            position: 1_000_000_000L);
        PlaylistEntity playlist = _playlistEntityFixture.Create(
            id: playlistId,
            userId: userId,
            type: PlaylistType.Music,
            entries: [entry],
            entryCount: 1,
            version: 0);
        context.Playlists.Add(playlist);
        await context.SaveChangesAsync();

        // Act
        Result<PlaylistDetailsDto?> result = await sut.GetDetailsForUserAsync(playlistId, userId, paginationData: null, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.NotNull(result.Value);
        PlaylistEntryDto item = Assert.Single(result.Value!.Entries.Data);
        Assert.NotNull(item.Display);
        Assert.Equal(track.Id, item.Display!.TrackId);
        Assert.Equal("Display Track", item.Display.TrackTitle);
        Assert.Equal(track.DurationInSeconds, item.Display.DurationInSeconds);
        Assert.Equal(libraryId, item.Display.LibraryId);
        Assert.Equal(album.Id, item.Display.AlbumId);
        Assert.Equal("Display Album", item.Display.AlbumTitle);
        Assert.Equal(artist.Id, item.Display.ArtistId);
        Assert.Equal("Display Artist", item.Display.ArtistName);
        Assert.Equal(coverFileName, item.Display.CoverPath);
    }

    [Fact]
    public async Task GetDetailsForUserAsync_WhenAnEntryReferencesANonMusicMediaItem_ShouldReturnNoDisplayData()
    {
        // Arrange
        using SqliteConnection anchorConnection = new($"Data Source=luminadataccess-playlistrepo-details-video-{Guid.NewGuid()};Mode=Memory;Cache=Shared");
        anchorConnection.Open();
        using LuminaDbContext context = CreateContext(anchorConnection);
        PlaylistRepository sut = new(context);
        Guid userId = Guid.NewGuid();
        Guid libraryId = Guid.NewGuid();
        context.Users.Add(_userEntityFixture.Create(id: userId));
        (_, _, TrackEntity track) = await SeedTrackAsync(context, libraryId, artistName: "Video Artist", albumTitle: "Video Album", trackTitle: "Video Track", coverFileName: "video-cover.jpg");
        Guid playlistId = Guid.NewGuid();
        // The media item Id belongs to a real track, so a null display proves the entry type is what excludes it, not a missing row.
        PlaylistEntryEntity entry = _playlistEntryEntityFixture.Create(
            playlistId: playlistId,
            mediaItemId: track.Id,
            mediaItemType: PlaylistMediaItemType.VideoFile,
            position: 1_000_000_000L);
        PlaylistEntity playlist = _playlistEntityFixture.Create(
            id: playlistId,
            userId: userId,
            type: PlaylistType.Music,
            entries: [entry],
            entryCount: 1,
            version: 0);
        context.Playlists.Add(playlist);
        await context.SaveChangesAsync();

        // Act
        Result<PlaylistDetailsDto?> result = await sut.GetDetailsForUserAsync(playlistId, userId, paginationData: null, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.NotNull(result.Value);
        PlaylistEntryDto item = Assert.Single(result.Value!.Entries.Data);
        Assert.Null(item.Display);
    }

    /// <summary>
    /// Seeds a playlist of the given user with the requested number of entries, each referencing a different generated media item.
    /// </summary>
    /// <param name="context">The context to seed.</param>
    /// <param name="userId">The Id of the user that owns the playlist.</param>
    /// <param name="entryCount">The number of entries the playlist receives.</param>
    /// <returns>The seeded playlist and its entries, in position order.</returns>
    private async Task<(PlaylistEntity Playlist, List<PlaylistEntryEntity> Entries)> SeedOrderedPlaylistAsync(LuminaDbContext context, Guid userId, int entryCount)
    {
        Guid playlistId = Guid.NewGuid();
        List<PlaylistEntryEntity> entries = [.. Enumerable.Range(0, entryCount).Select(index => _playlistEntryEntityFixture.Create(
            playlistId: playlistId,
            mediaItemId: Guid.NewGuid(),
            mediaItemType: PlaylistMediaItemType.MusicTrack,
            position: (index + 1) * 1_000_000_000L))];
        PlaylistEntity playlist = _playlistEntityFixture.Create(
            id: playlistId,
            userId: userId,
            type: PlaylistType.Music,
            entries: entries,
            entryCount: entries.Count,
            version: 0);
        context.Playlists.Add(playlist);
        await context.SaveChangesAsync();
        return (playlist, entries);
    }

    /// <summary>
    /// Seeds one artist, one album of that artist and one track of that album, together with the cover artwork of the album.
    /// </summary>
    /// <param name="context">The context to seed.</param>
    /// <param name="libraryId">The Id of the library the catalogue belongs to.</param>
    /// <param name="artistName">The name of the artist.</param>
    /// <param name="albumTitle">The title of the album.</param>
    /// <param name="trackTitle">The title of the track.</param>
    /// <param name="coverFileName">The file name of the cover artwork of the album.</param>
    /// <returns>The seeded artist, album and track.</returns>
    private async Task<(ArtistEntity Artist, AlbumEntity Album, TrackEntity Track)> SeedTrackAsync(
        LuminaDbContext context,
        Guid libraryId,
        string artistName,
        string albumTitle,
        string trackTitle,
        string coverFileName)
    {
        ArtistEntity artist = _artistEntityFixture.Create(libraryId: libraryId, name: artistName, includeAlbums: false, includeContributors: false);
        context.Artists.Add(artist);
        await context.SaveChangesAsync();

        AlbumEntity album = _albumEntityFixture.Create(artistId: artist.Id, libraryId: libraryId, title: albumTitle, includeTracks: false, includeMetadata: false);
        context.Albums.Add(album);
        await context.SaveChangesAsync();

        TrackEntity track = _trackEntityFixture.Create(albumId: album.Id, libraryId: libraryId, title: trackTitle, includeMetadata: false);
        context.Tracks.Add(track);
        context.MusicArtwork.Add(_musicArtworkEntityFixture.Create(
            ownerType: MusicArtworkOwnerType.Album,
            ownerId: album.Id,
            artworkType: ArtworkType.Cover,
            ordinal: 0,
            fileName: coverFileName));
        await context.SaveChangesAsync();

        return (artist, album, track);
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
