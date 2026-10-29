#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.Favorites;
using Lumina.Application.Common.DataAccess.Entities.MediaContributors;
using Lumina.Application.Common.DataAccess.Entities.MediaLibrary.AudioLibrary.MusicLibrary;
using Lumina.Application.Common.DataAccess.Entities.MediaLibrary.WrittenContentLibrary.BookLibrary;
using Lumina.Application.Common.DataAccess.Entities.UsersManagement;
using Lumina.Application.Common.DTO.Favorites;
using Lumina.Application.Common.DTO.Pagination;
using Lumina.Application.Common.Infrastructure.Authentication;
using Lumina.Application.Common.Infrastructure.Time;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.Favorites;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.MediaContributors;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.MediaLibrary.AudioLibrary.MusicLibrary;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.MediaLibrary.WrittenContentLibrary.BookLibrary;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.UsersManagement;
using Lumina.DataAccess.Common.Interceptors;
using Lumina.DataAccess.Core.Repositories.Favorites;
using Lumina.DataAccess.Core.UoW;
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.SharedKernel.Common.Enums.AudioLibrary;
using Lumina.Domain.SharedKernel.Common.Enums.Favorites;
using Lumina.Domain.SharedKernel.Common.Enums.MediaContributors;
using Lumina.Domain.SharedKernel.Common.Enums.MediaLibrary;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DomainErrors = Lumina.Domain.Common.Errors.Errors;
#endregion

namespace Lumina.DataAccess.IntegrationTests.Core.Repositories.Favorites;

/// <summary>
/// Contains integration tests for the <see cref="FavoriteRepository"/> class, exercising it against a real SQLite database.
/// </summary>
[ExcludeFromCodeCoverage]
public class FavoriteRepositoryTests
{
    private readonly FavoriteEntityFixture _favoriteEntityFixture = new();
    private readonly UserEntityFixture _userEntityFixture = new();
    private readonly ArtistEntityFixture _artistEntityFixture = new();
    private readonly AlbumEntityFixture _albumEntityFixture = new();
    private readonly TrackEntityFixture _trackEntityFixture = new();
    private readonly BookEntityFixture _bookEntityFixture = new();
    private readonly BookArtworkEntityFixture _bookArtworkEntityFixture = new();
    private readonly MediaContributorEntityFixture _mediaContributorEntityFixture = new();
    private readonly BookContributorEntityFixture _bookContributorEntityFixture = new();
    private readonly MusicArtworkEntityFixture _musicArtworkEntityFixture = new();

    [Fact]
    public async Task InsertAsync_WhenTheFavoriteIsNew_ShouldReturnCreatedAndPersistEveryField()
    {
        // Arrange
        using SqliteConnection anchorConnection = CreateAnchorConnection("insert");
        Guid userId = Guid.NewGuid();
        DateTime utcNow = new(2025, 1, 1, 12, 0, 0, DateTimeKind.Utc);
        using LuminaDbContext context = CreateAuditedContext(anchorConnection, userId, utcNow);
        FavoriteRepository sut = new(context);
        context.Users.Add(_userEntityFixture.Create(id: userId));
        Guid mediaItemId = Guid.NewGuid();
        // The fixture populates the update audit columns, so the test also proves the interceptor owns them on insert.
        FavoriteEntity favorite = _favoriteEntityFixture.Create(
            userId: userId,
            mediaKind: FavoriteMediaKind.MusicTrack,
            mediaItemId: mediaItemId,
            createdOnUtc: DateTime.MinValue,
            createdBy: Guid.Empty,
            updatedOnUtc: DateTime.MaxValue,
            updatedBy: Guid.NewGuid(),
            includeUpdatedOnUtc: true,
            includeUpdatedBy: true);

        // Act
        Result<Created> result = await sut.InsertAsync(favorite, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(Result.Created, result.Value);
        Assert.Equal(EntityState.Added, context.Entry(favorite).State);

        await context.SaveChangesAsync();

        context.ChangeTracker.Clear();
        FavoriteEntity storedFavorite = await context.Favorites.AsNoTracking().SingleAsync();
        Assert.Equal(favorite.Id, storedFavorite.Id);
        Assert.Equal(userId, storedFavorite.UserId);
        Assert.Equal(FavoriteMediaKind.MusicTrack, storedFavorite.MediaKind);
        Assert.Equal(mediaItemId, storedFavorite.MediaItemId);
        Assert.Equal(utcNow, storedFavorite.CreatedOnUtc);
        Assert.Equal(userId, storedFavorite.CreatedBy);
        Assert.Null(storedFavorite.UpdatedOnUtc);
        Assert.Null(storedFavorite.UpdatedBy);
    }

    [Fact]
    public async Task DeleteByIdAsync_WhenTheFavoriteIsAlreadyTracked_ShouldDeleteTheTrackedInstanceWithoutCreatingASecondCopy()
    {
        // Arrange
        using SqliteConnection anchorConnection = CreateAnchorConnection("delete-tracked");
        using LuminaDbContext context = CreateContext(anchorConnection);
        FavoriteRepository sut = new(context);
        Guid userId = Guid.NewGuid();
        context.Users.Add(_userEntityFixture.Create(id: userId));
        FavoriteEntity seededFavorite = CreateFavorite(userId);
        context.Favorites.Add(seededFavorite);
        await context.SaveChangesAsync();
        // Loading through the repository's own DbSet is what a handler that already checked the favorite does.
        FavoriteEntity trackedFavorite = await context.Favorites.SingleAsync(candidate => candidate.Id == seededFavorite.Id);

        // Act
        Result<Deleted> result = await sut.DeleteByIdAsync(seededFavorite.Id, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(Result.Deleted, result.Value);
        EntityEntry<FavoriteEntity> trackedEntry = Assert.Single(context.ChangeTracker.Entries<FavoriteEntity>());
        Assert.Same(trackedFavorite, trackedEntry.Entity);
        Assert.Equal(EntityState.Deleted, trackedEntry.State);

        await context.SaveChangesAsync();

        Assert.False(await context.Favorites.AsNoTracking().AnyAsync(candidate => candidate.Id == seededFavorite.Id));
    }

    [Fact]
    public async Task DeleteByIdAsync_WhenTheFavoriteIsOnlyInTheDatabase_ShouldDeleteIt()
    {
        // Arrange
        using SqliteConnection anchorConnection = CreateAnchorConnection("delete-stored");
        using LuminaDbContext context = CreateContext(anchorConnection);
        FavoriteRepository sut = new(context);
        Guid userId = Guid.NewGuid();
        context.Users.Add(_userEntityFixture.Create(id: userId));
        FavoriteEntity seededFavorite = CreateFavorite(userId);
        context.Favorites.Add(seededFavorite);
        await context.SaveChangesAsync();
        // A delete request starts with nothing tracked, so the repository has to materialize the row itself.
        context.ChangeTracker.Clear();

        // Act
        Result<Deleted> result = await sut.DeleteByIdAsync(seededFavorite.Id, CancellationToken.None);
        await context.SaveChangesAsync();

        // Assert
        Assert.False(result.IsFailure);
        using LuminaDbContext verificationContext = CreateContext(anchorConnection);
        Assert.False(await verificationContext.Favorites.AsNoTracking().AnyAsync(candidate => candidate.Id == seededFavorite.Id));
    }

    [Fact]
    public async Task DeleteByIdAsync_WhenTheFavoriteDoesNotExist_ShouldReturnFavoriteNotFound()
    {
        // Arrange
        using SqliteConnection anchorConnection = CreateAnchorConnection("delete-missing");
        using LuminaDbContext context = CreateContext(anchorConnection);
        FavoriteRepository sut = new(context);
        Guid userId = Guid.NewGuid();
        context.Users.Add(_userEntityFixture.Create(id: userId));
        FavoriteEntity seededFavorite = CreateFavorite(userId);
        context.Favorites.Add(seededFavorite);
        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        // Act
        Result<Deleted> result = await sut.DeleteByIdAsync(Guid.NewGuid(), CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(DomainErrors.Favorites.FavoriteNotFound, result.FirstError);
        Assert.True(await context.Favorites.AsNoTracking().AnyAsync(candidate => candidate.Id == seededFavorite.Id));
    }

    [Fact]
    public async Task GetByMediaItemIdForUserAsync_WhenTheFavoriteBelongsToTheUserAndMatchesTheMediaItem_ShouldReturnIt()
    {
        // Arrange
        using SqliteConnection anchorConnection = CreateAnchorConnection("get-matching");
        using LuminaDbContext context = CreateContext(anchorConnection);
        FavoriteRepository sut = new(context);
        Guid userId = Guid.NewGuid();
        Guid otherUserId = Guid.NewGuid();
        context.Users.AddRange(_userEntityFixture.Create(id: userId), _userEntityFixture.Create(id: otherUserId));
        Guid mediaItemId = Guid.NewGuid();
        FavoriteEntity expectedFavorite = CreateFavorite(userId, FavoriteMediaKind.MusicTrack, mediaItemId);
        // The other favorites are the ones a filter that forgot one of its predicate parts would return.
        FavoriteEntity otherUserFavorite = CreateFavorite(otherUserId, FavoriteMediaKind.MusicTrack, mediaItemId);
        FavoriteEntity otherKindFavorite = CreateFavorite(userId, FavoriteMediaKind.MusicAlbum, mediaItemId);
        FavoriteEntity otherItemFavorite = CreateFavorite(userId, FavoriteMediaKind.MusicTrack, Guid.NewGuid());
        context.Favorites.AddRange(expectedFavorite, otherUserFavorite, otherKindFavorite, otherItemFavorite);
        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        // Act
        Result<FavoriteEntity?> result = await sut.GetByMediaItemIdForUserAsync(userId, FavoriteMediaKind.MusicTrack, mediaItemId, cancellationToken: CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.NotNull(result.Value);
        Assert.Equal(expectedFavorite.Id, result.Value!.Id);
    }

    [Fact]
    public async Task GetByMediaItemIdForUserAsync_WhenAnotherUserFavoritedTheSameMediaItem_ShouldNotReturnIt()
    {
        // Arrange
        using SqliteConnection anchorConnection = CreateAnchorConnection("get-other-user");
        using LuminaDbContext context = CreateContext(anchorConnection);
        FavoriteRepository sut = new(context);
        Guid userId = Guid.NewGuid();
        Guid otherUserId = Guid.NewGuid();
        context.Users.AddRange(_userEntityFixture.Create(id: userId), _userEntityFixture.Create(id: otherUserId));
        Guid mediaItemId = Guid.NewGuid();
        context.Favorites.Add(CreateFavorite(otherUserId, FavoriteMediaKind.MusicArtist, mediaItemId));
        await context.SaveChangesAsync();

        // Act
        Result<FavoriteEntity?> result = await sut.GetByMediaItemIdForUserAsync(userId, FavoriteMediaKind.MusicArtist, mediaItemId, cancellationToken: CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Null(result.Value);
    }

    [Fact]
    public async Task GetByMediaItemIdForUserAsync_WhenTheFavoriteIsOfAnotherKindOrItem_ShouldNotReturnIt()
    {
        // Arrange
        using SqliteConnection anchorConnection = CreateAnchorConnection("get-other-kind");
        using LuminaDbContext context = CreateContext(anchorConnection);
        FavoriteRepository sut = new(context);
        Guid userId = Guid.NewGuid();
        context.Users.Add(_userEntityFixture.Create(id: userId));
        Guid mediaItemId = Guid.NewGuid();
        context.Favorites.AddRange(
            CreateFavorite(userId, FavoriteMediaKind.Book, mediaItemId),
            CreateFavorite(userId, FavoriteMediaKind.MusicArtist, Guid.NewGuid()));
        await context.SaveChangesAsync();

        // Act
        Result<FavoriteEntity?> result = await sut.GetByMediaItemIdForUserAsync(userId, FavoriteMediaKind.MusicArtist, mediaItemId, cancellationToken: CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Null(result.Value);
    }

    [Fact]
    public async Task GetByMediaItemIdForUserAsync_WhenTrackingIsRequested_ShouldTrackTheReturnedEntity()
    {
        // Arrange
        using SqliteConnection anchorConnection = CreateAnchorConnection("get-tracked");
        using LuminaDbContext context = CreateContext(anchorConnection);
        FavoriteRepository sut = new(context);
        Guid userId = Guid.NewGuid();
        context.Users.Add(_userEntityFixture.Create(id: userId));
        Guid mediaItemId = Guid.NewGuid();
        context.Favorites.Add(CreateFavorite(userId, FavoriteMediaKind.MusicAlbum, mediaItemId));
        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        // Act
        Result<FavoriteEntity?> result = await sut.GetByMediaItemIdForUserAsync(userId, FavoriteMediaKind.MusicAlbum, mediaItemId, shouldTrackEntities: true, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.NotNull(result.Value);
        EntityEntry<FavoriteEntity> trackedEntry = Assert.Single(context.ChangeTracker.Entries<FavoriteEntity>());
        Assert.Same(result.Value, trackedEntry.Entity);
        Assert.Equal(EntityState.Unchanged, trackedEntry.State);
    }

    [Fact]
    public async Task GetByMediaItemIdForUserAsync_WhenTrackingIsNotRequested_ShouldNotTrackAnything()
    {
        // Arrange
        using SqliteConnection anchorConnection = CreateAnchorConnection("get-untracked");
        using LuminaDbContext context = CreateContext(anchorConnection);
        FavoriteRepository sut = new(context);
        Guid userId = Guid.NewGuid();
        context.Users.Add(_userEntityFixture.Create(id: userId));
        Guid mediaItemId = Guid.NewGuid();
        context.Favorites.Add(CreateFavorite(userId, FavoriteMediaKind.Book, mediaItemId));
        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        // Act
        Result<FavoriteEntity?> result = await sut.GetByMediaItemIdForUserAsync(userId, FavoriteMediaKind.Book, mediaItemId, shouldTrackEntities: false, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.NotNull(result.Value);
        Assert.Empty(context.ChangeTracker.Entries<FavoriteEntity>());
    }

    [Fact]
    public async Task GetByMediaItemIdForUserAsync_WhenTheFavoriteDoesNotExist_ShouldReturnSuccessWithNull()
    {
        // Arrange
        using SqliteConnection anchorConnection = CreateAnchorConnection("get-missing");
        using LuminaDbContext context = CreateContext(anchorConnection);
        FavoriteRepository sut = new(context);

        // Act
        Result<FavoriteEntity?> result = await sut.GetByMediaItemIdForUserAsync(Guid.NewGuid(), FavoriteMediaKind.MusicTrack, Guid.NewGuid(), cancellationToken: CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Null(result.Value);
    }

    [Theory]
    [InlineData(FavoriteMediaKind.MusicArtist)] // the artist catalogue serves the library Id of an artist
    [InlineData(FavoriteMediaKind.MusicAlbum)] // the album catalogue serves the library Id of an album
    [InlineData(FavoriteMediaKind.MusicTrack)] // the track catalogue serves the library Id of a track
    [InlineData(FavoriteMediaKind.Book)] // the book catalogue serves the library Id of a book
    public async Task GetMediaItemLibraryIdAsync_WhenTheMediaItemExists_ShouldReturnTheLibraryIdOfItsCatalogue(FavoriteMediaKind mediaKind)
    {
        // Arrange
        using SqliteConnection anchorConnection = CreateAnchorConnection($"library-id-{mediaKind}");
        using LuminaDbContext context = CreateContext(anchorConnection);
        FavoriteRepository sut = new(context);
        Guid artistLibraryId = Guid.NewGuid();
        Guid albumLibraryId = Guid.NewGuid();
        Guid trackLibraryId = Guid.NewGuid();
        Guid bookLibraryId = Guid.NewGuid();
        (ArtistEntity artist, AlbumEntity album, TrackEntity track, BookEntity book) = await SeedCatalogueAsync(context, artistLibraryId, albumLibraryId, trackLibraryId, bookLibraryId);
        // Every catalogue row carries a different library Id, so a lookup that queries the wrong table returns a wrong value instead of passing by accident.
        Dictionary<FavoriteMediaKind, (Guid MediaItemId, Guid LibraryId)> catalogueByKind = new()
        {
            [FavoriteMediaKind.MusicArtist] = (artist.Id, artistLibraryId),
            [FavoriteMediaKind.MusicAlbum] = (album.Id, albumLibraryId),
            [FavoriteMediaKind.MusicTrack] = (track.Id, trackLibraryId),
            [FavoriteMediaKind.Book] = (book.Id, bookLibraryId)
        };
        (Guid mediaItemId, Guid expectedLibraryId) = catalogueByKind[mediaKind];

        // Act
        Result<Guid?> result = await sut.GetMediaItemLibraryIdAsync(mediaKind, mediaItemId, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal((Guid?)expectedLibraryId, result.Value);
    }

    [Fact]
    public async Task GetMediaItemLibraryIdAsync_WhenTheMediaItemDoesNotExist_ShouldReturnSuccessWithNull()
    {
        // Arrange
        using SqliteConnection anchorConnection = CreateAnchorConnection("library-id-missing");
        using LuminaDbContext context = CreateContext(anchorConnection);
        FavoriteRepository sut = new(context);

        // Act
        Result<Guid?> result = await sut.GetMediaItemLibraryIdAsync(FavoriteMediaKind.MusicTrack, Guid.NewGuid(), CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Null(result.Value);
    }

    [Theory]
    [InlineData(FavoriteMediaKind.Video)] // planned kind, no video catalogue exists
    [InlineData(FavoriteMediaKind.Photo)] // planned kind, no photo catalogue exists
    [InlineData((FavoriteMediaKind)999)] // an undefined value that is neither catalogued nor planned
    public async Task GetMediaItemLibraryIdAsync_WhenTheKindHasNoCatalogue_ShouldReturnSuccessWithNullWithoutReadingTheCatalogue(FavoriteMediaKind mediaKind)
    {
        // Arrange
        using SqliteConnection anchorConnection = CreateAnchorConnection($"library-id-planned-{(int)mediaKind}");
        using LuminaDbContext context = CreateContext(anchorConnection);
        FavoriteRepository sut = new(context);
        Guid libraryId = Guid.NewGuid();
        ArtistEntity artist = _artistEntityFixture.Create(libraryId: libraryId, includeAlbums: false, includeContributors: false);
        context.Artists.Add(artist);
        await context.SaveChangesAsync();

        // Act
        // The seed makes the artist the media item of the requested Id, so a value returned here would prove the catalogue was consulted.
        Result<Guid?> result = await sut.GetMediaItemLibraryIdAsync(mediaKind, artist.Id, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Null(result.Value);
    }

    [Fact]
    public async Task GetAllForUserAsync_WhenTheUserHasFavorites_ShouldReturnOnlyTheFavoritesOfThatUser()
    {
        // Arrange
        using SqliteConnection anchorConnection = CreateAnchorConnection("list-user");
        using LuminaDbContext context = CreateContext(anchorConnection);
        FavoriteRepository sut = new(context);
        Guid userId = Guid.NewGuid();
        Guid otherUserId = Guid.NewGuid();
        context.Users.AddRange(_userEntityFixture.Create(id: userId), _userEntityFixture.Create(id: otherUserId));
        context.Favorites.AddRange(
            CreateFavorite(userId),
            CreateFavorite(userId),
            CreateFavorite(otherUserId));
        await context.SaveChangesAsync();

        // Act
        Result<PaginatedResultDto<FavoriteItemDto>> result = await sut.GetAllForUserAsync(userId, mediaKind: null, paginationData: null, cancellationToken: CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(2, result.Value.Count);
        Assert.All(result.Value.Data, item => Assert.Equal(userId, item.Favorite.UserId));
    }

    [Fact]
    public async Task GetAllForUserAsync_WhenNoMediaKindIsGiven_ShouldReturnEveryKind()
    {
        // Arrange
        using SqliteConnection anchorConnection = CreateAnchorConnection("list-all-kinds");
        using LuminaDbContext context = CreateContext(anchorConnection);
        FavoriteRepository sut = new(context);
        Guid userId = Guid.NewGuid();
        context.Users.Add(_userEntityFixture.Create(id: userId));
        context.Favorites.AddRange(
            CreateFavorite(userId, FavoriteMediaKind.MusicArtist),
            CreateFavorite(userId, FavoriteMediaKind.MusicAlbum),
            CreateFavorite(userId, FavoriteMediaKind.MusicTrack),
            CreateFavorite(userId, FavoriteMediaKind.Book));
        await context.SaveChangesAsync();

        // Act
        Result<PaginatedResultDto<FavoriteItemDto>> result = await sut.GetAllForUserAsync(userId, mediaKind: null, paginationData: null, cancellationToken: CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(4, result.Value.Count);
        List<FavoriteMediaKind> returnedKinds = [.. result.Value.Data.Select(item => item.Favorite.MediaKind).OrderBy(mediaKind => mediaKind)];
        Assert.Equal(
            [FavoriteMediaKind.MusicArtist, FavoriteMediaKind.MusicAlbum, FavoriteMediaKind.MusicTrack, FavoriteMediaKind.Book],
            returnedKinds);
    }

    [Fact]
    public async Task GetAllForUserAsync_WhenAMediaKindIsGiven_ShouldReturnOnlyThatKind()
    {
        // Arrange
        using SqliteConnection anchorConnection = CreateAnchorConnection("list-one-kind");
        using LuminaDbContext context = CreateContext(anchorConnection);
        FavoriteRepository sut = new(context);
        Guid userId = Guid.NewGuid();
        context.Users.Add(_userEntityFixture.Create(id: userId));
        context.Favorites.AddRange(
            CreateFavorite(userId, FavoriteMediaKind.MusicArtist),
            CreateFavorite(userId, FavoriteMediaKind.MusicArtist),
            CreateFavorite(userId, FavoriteMediaKind.MusicTrack),
            CreateFavorite(userId, FavoriteMediaKind.Book));
        await context.SaveChangesAsync();

        // Act
        Result<PaginatedResultDto<FavoriteItemDto>> result = await sut.GetAllForUserAsync(userId, FavoriteMediaKind.MusicArtist, paginationData: null, cancellationToken: CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(2, result.Value.Count);
        Assert.All(result.Value.Data, item => Assert.Equal(FavoriteMediaKind.MusicArtist, item.Favorite.MediaKind));
    }

    [Fact]
    public async Task GetAllForUserAsync_WhenNoPaginationIsRequested_ShouldReturnEveryFavoriteWithOnePage()
    {
        // Arrange
        using SqliteConnection anchorConnection = CreateAnchorConnection("list-no-pagination");
        using LuminaDbContext context = CreateContext(anchorConnection);
        FavoriteRepository sut = new(context);
        Guid userId = Guid.NewGuid();
        context.Users.Add(_userEntityFixture.Create(id: userId));
        context.Favorites.AddRange(CreateFavorite(userId), CreateFavorite(userId), CreateFavorite(userId));
        await context.SaveChangesAsync();

        // Act
        Result<PaginatedResultDto<FavoriteItemDto>> result = await sut.GetAllForUserAsync(userId, mediaKind: null, paginationData: null, cancellationToken: CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(3, result.Value.Data.Count);
        Assert.Equal(1, result.Value.CurrentPage);
        Assert.Equal(3, result.Value.PerPage);
        Assert.Equal(3, result.Value.Count);
        Assert.Equal(1, result.Value.NumberOfPages);
    }

    [Fact]
    public async Task GetAllForUserAsync_WhenAPageIsRequested_ShouldReturnOnlyThatPage()
    {
        // Arrange
        using SqliteConnection anchorConnection = CreateAnchorConnection("list-pages");
        using LuminaDbContext context = CreateContext(anchorConnection);
        FavoriteRepository sut = new(context);
        Guid userId = Guid.NewGuid();
        context.Users.Add(_userEntityFixture.Create(id: userId));
        DateTime baseCreatedOnUtc = new(2025, 1, 1, 8, 0, 0, DateTimeKind.Utc);
        List<FavoriteEntity> favorites = [.. Enumerable.Range(0, 5).Select(index => CreateFavorite(
            userId,
            FavoriteMediaKind.MusicArtist,
            Guid.NewGuid(),
            baseCreatedOnUtc.AddMinutes(index)))];
        context.Favorites.AddRange(favorites);
        await context.SaveChangesAsync();
        PaginationDataDto firstPage = new() { CurrentPage = 1, PerPage = 2 };
        PaginationDataDto secondPage = new() { CurrentPage = 2, PerPage = 2 };

        // Act
        Result<PaginatedResultDto<FavoriteItemDto>> firstPageResult = await sut.GetAllForUserAsync(userId, mediaKind: null, firstPage, cancellationToken: CancellationToken.None);
        Result<PaginatedResultDto<FavoriteItemDto>> secondPageResult = await sut.GetAllForUserAsync(userId, mediaKind: null, secondPage, cancellationToken: CancellationToken.None);

        // Assert
        Assert.False(firstPageResult.IsFailure);
        Assert.Equal(2, firstPageResult.Value.Data.Count);
        Assert.Equal(5, firstPageResult.Value.Count);
        Assert.Equal(2, firstPageResult.Value.PerPage);
        Assert.Equal(3, firstPageResult.Value.NumberOfPages);
        Assert.Equal(1, firstPageResult.Value.CurrentPage);
        Assert.Equal([favorites[4].Id, favorites[3].Id], firstPageResult.Value.Data.Select(item => item.Favorite.Id));

        Assert.False(secondPageResult.IsFailure);
        Assert.Equal(2, secondPageResult.Value.Data.Count);
        Assert.Equal(2, secondPageResult.Value.CurrentPage);
        Assert.Equal([favorites[2].Id, favorites[1].Id], secondPageResult.Value.Data.Select(item => item.Favorite.Id));
        // A page must never repeat a row of another page.
        Assert.Empty(firstPageResult.Value.Data.Select(item => item.Favorite.Id).Intersect(secondPageResult.Value.Data.Select(item => item.Favorite.Id)));
    }

    [Fact]
    public async Task GetAllForUserAsync_WhenTheRequestedPageExceedsTheLastPage_ShouldClampToTheLastPage()
    {
        // Arrange
        using SqliteConnection anchorConnection = CreateAnchorConnection("list-page-clamp");
        using LuminaDbContext context = CreateContext(anchorConnection);
        FavoriteRepository sut = new(context);
        Guid userId = Guid.NewGuid();
        context.Users.Add(_userEntityFixture.Create(id: userId));
        DateTime baseCreatedOnUtc = new(2025, 1, 1, 8, 0, 0, DateTimeKind.Utc);
        List<FavoriteEntity> favorites = [.. Enumerable.Range(0, 5).Select(index => CreateFavorite(
            userId,
            FavoriteMediaKind.MusicTrack,
            Guid.NewGuid(),
            baseCreatedOnUtc.AddMinutes(index)))];
        context.Favorites.AddRange(favorites);
        await context.SaveChangesAsync();
        PaginationDataDto beyondLastPage = new() { CurrentPage = 99, PerPage = 2 };

        // Act
        Result<PaginatedResultDto<FavoriteItemDto>> result = await sut.GetAllForUserAsync(userId, mediaKind: null, beyondLastPage, cancellationToken: CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(3, result.Value.CurrentPage);
        FavoriteItemDto lastPageItem = Assert.Single(result.Value.Data);
        Assert.Equal(favorites[0].Id, lastPageItem.Favorite.Id);
    }

    [Fact]
    public async Task GetAllForUserAsync_WhenTheFavoritesWereCreatedAtDifferentTimes_ShouldOrderThemByCreationTimeDescendingWithIdAscending()
    {
        // Arrange
        using SqliteConnection anchorConnection = CreateAnchorConnection("list-order");
        using LuminaDbContext context = CreateContext(anchorConnection);
        FavoriteRepository sut = new(context);
        Guid userId = Guid.NewGuid();
        context.Users.Add(_userEntityFixture.Create(id: userId));
        DateTime oldest = new(2025, 1, 1, 8, 0, 0, DateTimeKind.Utc);
        DateTime middle = new(2025, 2, 1, 8, 0, 0, DateTimeKind.Utc);
        DateTime newest = new(2025, 3, 1, 8, 0, 0, DateTimeKind.Utc);
        // The two favorites created at the same instant are pinned to Ids whose ascending order is unambiguous.
        FavoriteEntity lowIdFavorite = CreateFavorite(userId, FavoriteMediaKind.MusicArtist, Guid.NewGuid(), middle, id: Guid.Parse("aaaaaaaa-0000-0000-0000-000000000001"));
        FavoriteEntity highIdFavorite = CreateFavorite(userId, FavoriteMediaKind.MusicArtist, Guid.NewGuid(), middle, id: Guid.Parse("aaaaaaaa-0000-0000-0000-000000000002"));
        FavoriteEntity oldestFavorite = CreateFavorite(userId, FavoriteMediaKind.MusicArtist, Guid.NewGuid(), oldest);
        FavoriteEntity newestFavorite = CreateFavorite(userId, FavoriteMediaKind.MusicArtist, Guid.NewGuid(), newest);
        context.Favorites.AddRange(lowIdFavorite, highIdFavorite, oldestFavorite, newestFavorite);
        await context.SaveChangesAsync();

        // Act
        Result<PaginatedResultDto<FavoriteItemDto>> result = await sut.GetAllForUserAsync(userId, mediaKind: null, paginationData: null, cancellationToken: CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(
            [newestFavorite.Id, lowIdFavorite.Id, highIdFavorite.Id, oldestFavorite.Id],
            result.Value.Data.Select(item => item.Favorite.Id));
    }

    [Fact]
    public async Task GetAllForUserAsync_WhenTheUserHasNoFavoritesAndAPageIsRequested_ShouldReturnAnEmptyPage()
    {
        // Arrange
        using SqliteConnection anchorConnection = CreateAnchorConnection("list-empty-page");
        using LuminaDbContext context = CreateContext(anchorConnection);
        FavoriteRepository sut = new(context);
        Guid userId = Guid.NewGuid();
        context.Users.Add(_userEntityFixture.Create(id: userId));
        await context.SaveChangesAsync();
        PaginationDataDto paginationData = new() { CurrentPage = 4, PerPage = 2 };

        // Act
        Result<PaginatedResultDto<FavoriteItemDto>> result = await sut.GetAllForUserAsync(userId, mediaKind: null, paginationData, cancellationToken: CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Empty(result.Value.Data);
        Assert.Equal(0, result.Value.Count);
        Assert.Equal(0, result.Value.NumberOfPages);
        Assert.Equal(1, result.Value.CurrentPage);
        Assert.Equal(2, result.Value.PerPage);
    }

    [Fact]
    public async Task GetAllForUserAsync_WhenTheUserHasNoFavoritesAndNoPaginationIsRequested_ShouldReturnAnEmptyResultWithOnePage()
    {
        // Arrange
        using SqliteConnection anchorConnection = CreateAnchorConnection("list-empty");
        using LuminaDbContext context = CreateContext(anchorConnection);
        FavoriteRepository sut = new(context);
        Guid userId = Guid.NewGuid();
        context.Users.Add(_userEntityFixture.Create(id: userId));
        await context.SaveChangesAsync();

        // Act
        Result<PaginatedResultDto<FavoriteItemDto>> result = await sut.GetAllForUserAsync(userId, mediaKind: null, paginationData: null, cancellationToken: CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Empty(result.Value.Data);
        Assert.Equal(0, result.Value.PerPage);
        Assert.Equal(0, result.Value.Count);
        Assert.Equal(1, result.Value.NumberOfPages);
        Assert.Equal(1, result.Value.CurrentPage);
    }

    [Fact]
    public async Task GetAllForUserAsync_WhenTheFavoriteIsAMusicArtist_ShouldMapTheArtistDisplayData()
    {
        // Arrange
        using SqliteConnection anchorConnection = CreateAnchorConnection("list-display-artist");
        using LuminaDbContext context = CreateContext(anchorConnection);
        FavoriteRepository sut = new(context);
        Guid userId = Guid.NewGuid();
        Guid libraryId = Guid.NewGuid();
        context.Users.Add(_userEntityFixture.Create(id: userId));
        ArtistEntity artistWithCover = _artistEntityFixture.Create(libraryId: libraryId, name: "Artist With Cover", includeAlbums: false, includeContributors: false);
        ArtistEntity artistWithoutCover = _artistEntityFixture.Create(libraryId: libraryId, name: "Artist Without Cover", includeAlbums: false, includeContributors: false);
        string coverFileName = "artist-cover.jpg";
        context.Artists.AddRange(artistWithCover, artistWithoutCover);
        context.MusicArtwork.Add(_musicArtworkEntityFixture.Create(
            ownerType: MusicArtworkOwnerType.Artist,
            ownerId: artistWithCover.Id,
            artworkType: ArtworkType.Cover,
            ordinal: 0,
            fileName: coverFileName));
        FavoriteEntity artistWithCoverFavorite = CreateFavorite(userId, FavoriteMediaKind.MusicArtist, artistWithCover.Id);
        FavoriteEntity artistWithoutCoverFavorite = CreateFavorite(userId, FavoriteMediaKind.MusicArtist, artistWithoutCover.Id);
        context.Favorites.AddRange(artistWithCoverFavorite, artistWithoutCoverFavorite);
        await context.SaveChangesAsync();

        // Act
        Result<PaginatedResultDto<FavoriteItemDto>> result = await sut.GetAllForUserAsync(userId, FavoriteMediaKind.MusicArtist, paginationData: null, cancellationToken: CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        FavoriteItemDto withCoverItem = Assert.Single(result.Value.Data, item => item.Favorite.Id == artistWithCoverFavorite.Id);
        Assert.NotNull(withCoverItem.Display);
        Assert.Equal(libraryId, withCoverItem.Display!.LibraryId);
        Assert.Equal("Artist With Cover", withCoverItem.Display.Title);
        Assert.Equal(Guid.Empty, withCoverItem.Display.CollectionId);
        Assert.Equal(string.Empty, withCoverItem.Display.CollectionTitle);
        Assert.Equal(Guid.Empty, withCoverItem.Display.CreatorId);
        Assert.Equal(string.Empty, withCoverItem.Display.CreatorName);
        Assert.Equal(coverFileName, withCoverItem.Display.CoverPath);

        FavoriteItemDto withoutCoverItem = Assert.Single(result.Value.Data, item => item.Favorite.Id == artistWithoutCoverFavorite.Id);
        Assert.NotNull(withoutCoverItem.Display);
        Assert.Equal("Artist Without Cover", withoutCoverItem.Display!.Title);
        Assert.Null(withoutCoverItem.Display.CoverPath);
    }

    [Fact]
    public async Task GetAllForUserAsync_WhenTheFavoriteIsAMusicAlbum_ShouldMapTheAlbumDisplayData()
    {
        // Arrange
        using SqliteConnection anchorConnection = CreateAnchorConnection("list-display-album");
        using LuminaDbContext context = CreateContext(anchorConnection);
        FavoriteRepository sut = new(context);
        Guid userId = Guid.NewGuid();
        Guid libraryId = Guid.NewGuid();
        context.Users.Add(_userEntityFixture.Create(id: userId));
        ArtistEntity artist = _artistEntityFixture.Create(libraryId: libraryId, name: "Album Artist", includeAlbums: false, includeContributors: false);
        context.Artists.Add(artist);
        await context.SaveChangesAsync();
        AlbumEntity album = _albumEntityFixture.Create(artistId: artist.Id, libraryId: libraryId, title: "Album Title", includeTracks: false, includeMetadata: false);
        context.Albums.Add(album);
        string coverFileName = "album-cover.jpg";
        context.MusicArtwork.Add(_musicArtworkEntityFixture.Create(
            ownerType: MusicArtworkOwnerType.Album,
            ownerId: album.Id,
            artworkType: ArtworkType.Cover,
            ordinal: 0,
            fileName: coverFileName));
        FavoriteEntity favorite = CreateFavorite(userId, FavoriteMediaKind.MusicAlbum, album.Id);
        context.Favorites.Add(favorite);
        await context.SaveChangesAsync();

        // Act
        Result<PaginatedResultDto<FavoriteItemDto>> result = await sut.GetAllForUserAsync(userId, FavoriteMediaKind.MusicAlbum, paginationData: null, cancellationToken: CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        FavoriteItemDto item = Assert.Single(result.Value.Data);
        Assert.NotNull(item.Display);
        Assert.Equal(libraryId, item.Display!.LibraryId);
        Assert.Equal("Album Title", item.Display.Title);
        Assert.Equal(Guid.Empty, item.Display.CollectionId);
        Assert.Equal(string.Empty, item.Display.CollectionTitle);
        Assert.Equal(artist.Id, item.Display.CreatorId);
        Assert.Equal("Album Artist", item.Display.CreatorName);
        Assert.Equal(coverFileName, item.Display.CoverPath);
    }

    [Fact]
    public async Task GetAllForUserAsync_WhenTheFavoriteIsAMusicTrack_ShouldMapTheTrackDisplayData()
    {
        // Arrange
        using SqliteConnection anchorConnection = CreateAnchorConnection("list-display-track");
        using LuminaDbContext context = CreateContext(anchorConnection);
        FavoriteRepository sut = new(context);
        Guid userId = Guid.NewGuid();
        Guid libraryId = Guid.NewGuid();
        context.Users.Add(_userEntityFixture.Create(id: userId));
        ArtistEntity artist = _artistEntityFixture.Create(libraryId: libraryId, name: "Track Artist", includeAlbums: false, includeContributors: false);
        context.Artists.Add(artist);
        await context.SaveChangesAsync();
        AlbumEntity album = _albumEntityFixture.Create(artistId: artist.Id, libraryId: libraryId, title: "Track Album", includeTracks: false, includeMetadata: false);
        context.Albums.Add(album);
        await context.SaveChangesAsync();
        TrackEntity track = _trackEntityFixture.Create(albumId: album.Id, libraryId: libraryId, title: "Track Title", includeMetadata: false);
        context.Tracks.Add(track);
        string coverFileName = "track-album-cover.jpg";
        context.MusicArtwork.Add(_musicArtworkEntityFixture.Create(
            ownerType: MusicArtworkOwnerType.Album,
            ownerId: album.Id,
            artworkType: ArtworkType.Cover,
            ordinal: 0,
            fileName: coverFileName));
        FavoriteEntity favorite = CreateFavorite(userId, FavoriteMediaKind.MusicTrack, track.Id);
        context.Favorites.Add(favorite);
        await context.SaveChangesAsync();

        // Act
        Result<PaginatedResultDto<FavoriteItemDto>> result = await sut.GetAllForUserAsync(userId, FavoriteMediaKind.MusicTrack, paginationData: null, cancellationToken: CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        FavoriteItemDto item = Assert.Single(result.Value.Data);
        Assert.NotNull(item.Display);
        Assert.Equal(libraryId, item.Display!.LibraryId);
        Assert.Equal("Track Title", item.Display.Title);
        Assert.Equal(album.Id, item.Display.CollectionId);
        Assert.Equal("Track Album", item.Display.CollectionTitle);
        Assert.Equal(artist.Id, item.Display.CreatorId);
        Assert.Equal("Track Artist", item.Display.CreatorName);
        Assert.Equal(coverFileName, item.Display.CoverPath);
    }

    [Fact]
    public async Task GetAllForUserAsync_WhenTheFavoriteIsABook_ShouldMapTheFirstAuthorByDisplayNameAndTheCover()
    {
        // Arrange
        using SqliteConnection anchorConnection = CreateAnchorConnection("list-display-book");
        using LuminaDbContext context = CreateContext(anchorConnection);
        FavoriteRepository sut = new(context);
        Guid userId = Guid.NewGuid();
        Guid libraryId = Guid.NewGuid();
        context.Users.Add(_userEntityFixture.Create(id: userId));
        string coverFileName = "book-cover.jpg";
        BookEntity bookWithAuthors = _bookEntityFixture.Create(
            libraryId: libraryId,
            path: $"/books/{Guid.NewGuid()}.epub",
            title: "Book With Authors",
            includeMetadata: false);
        bookWithAuthors.Artwork = [_bookArtworkEntityFixture.Create(bookId: bookWithAuthors.Id, artworkType: ArtworkType.Cover, ordinal: 0, fileName: coverFileName)];
        // The author inserted first is deliberately the one that sorts last by display name, so insertion order cannot satisfy the assertion.
        MediaContributorEntity zuluAuthor = _mediaContributorEntityFixture.Create(displayName: "Zulu Author");
        MediaContributorEntity alphaAuthor = _mediaContributorEntityFixture.Create(displayName: "Alpha Author");
        bookWithAuthors.Contributors =
        [
            _bookContributorEntityFixture.Create(bookId: bookWithAuthors.Id, mediaContributorId: zuluAuthor.Id, role: MediaContributorRole.Author),
            _bookContributorEntityFixture.Create(bookId: bookWithAuthors.Id, mediaContributorId: alphaAuthor.Id, role: MediaContributorRole.Author)
        ];
        BookEntity bookWithoutAuthors = _bookEntityFixture.Create(
            libraryId: libraryId,
            path: $"/books/{Guid.NewGuid()}.epub",
            title: "Book Without Authors",
            includeMetadata: false);
        context.MediaContributors.AddRange(zuluAuthor, alphaAuthor);
        context.Books.AddRange(bookWithAuthors, bookWithoutAuthors);
        FavoriteEntity bookWithAuthorsFavorite = CreateFavorite(userId, FavoriteMediaKind.Book, bookWithAuthors.Id);
        FavoriteEntity bookWithoutAuthorsFavorite = CreateFavorite(userId, FavoriteMediaKind.Book, bookWithoutAuthors.Id);
        context.Favorites.AddRange(bookWithAuthorsFavorite, bookWithoutAuthorsFavorite);
        await context.SaveChangesAsync();

        // Act
        Result<PaginatedResultDto<FavoriteItemDto>> result = await sut.GetAllForUserAsync(userId, FavoriteMediaKind.Book, paginationData: null, cancellationToken: CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        FavoriteItemDto withAuthorsItem = Assert.Single(result.Value.Data, item => item.Favorite.Id == bookWithAuthorsFavorite.Id);
        Assert.NotNull(withAuthorsItem.Display);
        Assert.Equal(libraryId, withAuthorsItem.Display!.LibraryId);
        Assert.Equal("Book With Authors", withAuthorsItem.Display.Title);
        Assert.Equal(Guid.Empty, withAuthorsItem.Display.CollectionId);
        Assert.Equal(string.Empty, withAuthorsItem.Display.CollectionTitle);
        Assert.Equal(alphaAuthor.Id, withAuthorsItem.Display.CreatorId);
        Assert.Equal("Alpha Author", withAuthorsItem.Display.CreatorName);
        Assert.Equal(coverFileName, withAuthorsItem.Display.CoverPath);

        FavoriteItemDto withoutAuthorsItem = Assert.Single(result.Value.Data, item => item.Favorite.Id == bookWithoutAuthorsFavorite.Id);
        Assert.NotNull(withoutAuthorsItem.Display);
        Assert.Equal("Book Without Authors", withoutAuthorsItem.Display!.Title);
        Assert.Equal(Guid.Empty, withoutAuthorsItem.Display.CreatorId);
        Assert.Equal(string.Empty, withoutAuthorsItem.Display.CreatorName);
        Assert.Null(withoutAuthorsItem.Display.CoverPath);
    }

    [Fact]
    public async Task GetAllForUserAsync_WhenTheFavoritedMediaItemNoLongerExists_ShouldKeepTheFavoriteWithoutDisplayData()
    {
        // Arrange
        using SqliteConnection anchorConnection = CreateAnchorConnection("list-missing-item");
        using LuminaDbContext context = CreateContext(anchorConnection);
        FavoriteRepository sut = new(context);
        Guid userId = Guid.NewGuid();
        context.Users.Add(_userEntityFixture.Create(id: userId));
        FavoriteEntity favorite = CreateFavorite(userId, FavoriteMediaKind.MusicAlbum, Guid.NewGuid());
        context.Favorites.Add(favorite);
        await context.SaveChangesAsync();

        // Act
        Result<PaginatedResultDto<FavoriteItemDto>> result = await sut.GetAllForUserAsync(userId, FavoriteMediaKind.MusicAlbum, paginationData: null, cancellationToken: CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        FavoriteItemDto item = Assert.Single(result.Value.Data);
        Assert.Equal(favorite.Id, item.Favorite.Id);
        Assert.Null(item.Display);
    }

    [Theory]
    [InlineData(FavoriteMediaKind.Video)] // planned kind, so no display data can be resolved yet
    [InlineData(FavoriteMediaKind.Photo)] // planned kind, so no display data can be resolved yet
    public async Task GetAllForUserAsync_WhenTheFavoriteIsOfAPlannedKind_ShouldReturnItWithoutDisplayData(FavoriteMediaKind mediaKind)
    {
        // Arrange
        using SqliteConnection anchorConnection = CreateAnchorConnection($"list-planned-{mediaKind}");
        using LuminaDbContext context = CreateContext(anchorConnection);
        FavoriteRepository sut = new(context);
        Guid userId = Guid.NewGuid();
        context.Users.Add(_userEntityFixture.Create(id: userId));
        FavoriteEntity favorite = CreateFavorite(userId, mediaKind, Guid.NewGuid());
        context.Favorites.Add(favorite);
        await context.SaveChangesAsync();

        // Act
        Result<PaginatedResultDto<FavoriteItemDto>> result = await sut.GetAllForUserAsync(userId, mediaKind, paginationData: null, cancellationToken: CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        FavoriteItemDto item = Assert.Single(result.Value.Data);
        Assert.Equal(favorite.Id, item.Favorite.Id);
        Assert.Null(item.Display);
    }

    [Fact]
    public async Task GetAllForUserAsync_WhenThePageContainsMultipleKinds_ShouldResolveEveryRowIndependently()
    {
        // Arrange
        using SqliteConnection anchorConnection = CreateAnchorConnection("list-multi-kind");
        using LuminaDbContext context = CreateContext(anchorConnection);
        FavoriteRepository sut = new(context);
        Guid userId = Guid.NewGuid();
        Guid libraryId = Guid.NewGuid();
        context.Users.Add(_userEntityFixture.Create(id: userId));
        ArtistEntity artist = _artistEntityFixture.Create(libraryId: libraryId, name: "Mixed Artist", includeAlbums: false, includeContributors: false);
        context.Artists.Add(artist);
        await context.SaveChangesAsync();
        AlbumEntity album = _albumEntityFixture.Create(artistId: artist.Id, libraryId: libraryId, title: "Mixed Album", includeTracks: false, includeMetadata: false);
        context.Albums.Add(album);
        await context.SaveChangesAsync();
        TrackEntity track = _trackEntityFixture.Create(albumId: album.Id, libraryId: libraryId, title: "Mixed Track", includeMetadata: false);
        context.Tracks.Add(track);
        BookEntity book = _bookEntityFixture.Create(libraryId: libraryId, path: $"/books/{Guid.NewGuid()}.epub", title: "Mixed Book", includeMetadata: false);
        context.Books.Add(book);
        await context.SaveChangesAsync();
        DateTime baseCreatedOnUtc = new(2025, 1, 1, 8, 0, 0, DateTimeKind.Utc);
        context.Favorites.AddRange(
            CreateFavorite(userId, FavoriteMediaKind.MusicArtist, artist.Id, baseCreatedOnUtc),
            CreateFavorite(userId, FavoriteMediaKind.MusicAlbum, album.Id, baseCreatedOnUtc.AddMinutes(1)),
            CreateFavorite(userId, FavoriteMediaKind.MusicTrack, track.Id, baseCreatedOnUtc.AddMinutes(2)),
            CreateFavorite(userId, FavoriteMediaKind.Book, book.Id, baseCreatedOnUtc.AddMinutes(3)));
        await context.SaveChangesAsync();
        PaginationDataDto paginationData = new() { CurrentPage = 1, PerPage = 10 };

        // Act
        Result<PaginatedResultDto<FavoriteItemDto>> result = await sut.GetAllForUserAsync(userId, mediaKind: null, paginationData, cancellationToken: CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(4, result.Value.Data.Count);
        Assert.All(result.Value.Data, item => Assert.NotNull(item.Display));
        List<string> collectionAndCreatorTitles = [.. result.Value.Data
            .Where(item => item.Favorite.MediaKind is FavoriteMediaKind.Book or FavoriteMediaKind.MusicTrack)
            .OrderBy(item => item.Favorite.MediaKind)
            .Select(item => item.Display!.Title)];
        Assert.Equal(["Mixed Track", "Mixed Book"], collectionAndCreatorTitles);
        List<string> singleItemTitles = [.. result.Value.Data
            .Where(item => item.Favorite.MediaKind is FavoriteMediaKind.MusicAlbum or FavoriteMediaKind.MusicArtist)
            .OrderBy(item => item.Favorite.MediaKind)
            .Select(item => item.Display!.Title)];
        Assert.Equal(["Mixed Artist", "Mixed Album"], singleItemTitles);
    }

    [Fact]
    public async Task GetAllForUserAsync_WhenThePageExceedsTheDisplayLookupChunkSize_ShouldResolveEveryRow()
    {
        // Arrange
        using SqliteConnection anchorConnection = CreateAnchorConnection("list-chunk");
        using LuminaDbContext context = CreateContext(anchorConnection);
        FavoriteRepository sut = new(context);
        Guid userId = Guid.NewGuid();
        Guid libraryId = Guid.NewGuid();
        context.Users.Add(_userEntityFixture.Create(id: userId));
        // One more than the lookup chunk size, so the resolution runs over two chunks.
        const int ARTIST_COUNT = 501;
        List<ArtistEntity> artists = [.. Enumerable.Range(0, ARTIST_COUNT).Select(index => _artistEntityFixture.Create(
            libraryId: libraryId,
            name: $"Chunk Artist {index:D3}",
            includeAlbums: false,
            includeContributors: false))];
        context.Artists.AddRange(artists);
        // The favorites are ordered ascending so that the resolution receives the exact order of the seeded names.
        context.Favorites.AddRange(artists.Select((artist, index) => CreateFavorite(
            userId,
            FavoriteMediaKind.MusicArtist,
            artist.Id,
            new DateTime(2025, 1, 1, 8, 0, 0, DateTimeKind.Utc).AddSeconds(index))));
        await context.SaveChangesAsync();

        // Act
        Result<PaginatedResultDto<FavoriteItemDto>> result = await sut.GetAllForUserAsync(userId, FavoriteMediaKind.MusicArtist, paginationData: null, cancellationToken: CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(ARTIST_COUNT, result.Value.Count);
        Assert.Equal(ARTIST_COUNT, result.Value.Data.Count);
        Assert.Equal(ARTIST_COUNT, result.Value.Data.Select(item => item.Favorite.MediaItemId).Distinct().Count());
        Assert.All(result.Value.Data, item => Assert.NotNull(item.Display));
        Dictionary<Guid, string> expectedTitles = artists.ToDictionary(artist => artist.Id, artist => artist.Name);
        Assert.All(result.Value.Data, item => Assert.Equal(expectedTitles[item.Favorite.MediaItemId], item.Display!.Title));
    }

    [Fact]
    public async Task GetFavoritedMediaItemIdsForUserAsync_WhenSomeRequestedMediaItemsAreFavorited_ShouldReturnOnlyThose()
    {
        // Arrange
        using SqliteConnection anchorConnection = CreateAnchorConnection("favorited-ids");
        using LuminaDbContext context = CreateContext(anchorConnection);
        FavoriteRepository sut = new(context);
        Guid userId = Guid.NewGuid();
        context.Users.Add(_userEntityFixture.Create(id: userId));
        Guid firstFavoritedId = Guid.NewGuid();
        Guid secondFavoritedId = Guid.NewGuid();
        Guid notFavoritedId = Guid.NewGuid();
        context.Favorites.AddRange(
            CreateFavorite(userId, FavoriteMediaKind.MusicTrack, firstFavoritedId),
            CreateFavorite(userId, FavoriteMediaKind.MusicTrack, secondFavoritedId));
        await context.SaveChangesAsync();
        Guid[] requestedIds = [firstFavoritedId, notFavoritedId, secondFavoritedId];

        // Act
        Result<IReadOnlyList<Guid>> result = await sut.GetFavoritedMediaItemIdsForUserAsync(userId, FavoriteMediaKind.MusicTrack, requestedIds, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(2, result.Value.Count);
        Assert.Contains(firstFavoritedId, result.Value);
        Assert.Contains(secondFavoritedId, result.Value);
        Assert.DoesNotContain(notFavoritedId, result.Value);
    }

    [Fact]
    public async Task GetFavoritedMediaItemIdsForUserAsync_WhenOtherUsersOrKindsFavoritedTheRequestedItems_ShouldIgnoreThem()
    {
        // Arrange
        using SqliteConnection anchorConnection = CreateAnchorConnection("favorited-ids-filters");
        using LuminaDbContext context = CreateContext(anchorConnection);
        FavoriteRepository sut = new(context);
        Guid userId = Guid.NewGuid();
        Guid otherUserId = Guid.NewGuid();
        context.Users.AddRange(_userEntityFixture.Create(id: userId), _userEntityFixture.Create(id: otherUserId));
        Guid otherUserFavoritedId = Guid.NewGuid();
        Guid otherKindFavoritedId = Guid.NewGuid();
        context.Favorites.AddRange(
            CreateFavorite(otherUserId, FavoriteMediaKind.MusicTrack, otherUserFavoritedId),
            CreateFavorite(userId, FavoriteMediaKind.MusicAlbum, otherKindFavoritedId));
        await context.SaveChangesAsync();
        Guid[] requestedIds = [otherUserFavoritedId, otherKindFavoritedId];

        // Act
        Result<IReadOnlyList<Guid>> result = await sut.GetFavoritedMediaItemIdsForUserAsync(userId, FavoriteMediaKind.MusicTrack, requestedIds, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Empty(result.Value);
    }

    [Fact]
    public async Task GetFavoritedMediaItemIdsForUserAsync_WhenARequestedIdIsDuplicated_ShouldReturnItOnlyOnce()
    {
        // Arrange
        using SqliteConnection anchorConnection = CreateAnchorConnection("favorited-ids-duplicate");
        using LuminaDbContext context = CreateContext(anchorConnection);
        FavoriteRepository sut = new(context);
        Guid userId = Guid.NewGuid();
        context.Users.Add(_userEntityFixture.Create(id: userId));
        Guid favoritedId = Guid.NewGuid();
        context.Favorites.Add(CreateFavorite(userId, FavoriteMediaKind.MusicArtist, favoritedId));
        await context.SaveChangesAsync();
        Guid[] requestedIds = [favoritedId, favoritedId, favoritedId];

        // Act
        Result<IReadOnlyList<Guid>> result = await sut.GetFavoritedMediaItemIdsForUserAsync(userId, FavoriteMediaKind.MusicArtist, requestedIds, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(favoritedId, Assert.Single(result.Value));
    }

    [Fact]
    public async Task GetFavoritedMediaItemIdsForUserAsync_WhenNoMediaItemIdsAreRequested_ShouldReturnSuccessWithAnEmptyList()
    {
        // Arrange
        using SqliteConnection anchorConnection = CreateAnchorConnection("favorited-ids-empty");
        using LuminaDbContext context = CreateContext(anchorConnection);
        FavoriteRepository sut = new(context);
        Guid userId = Guid.NewGuid();
        context.Users.Add(_userEntityFixture.Create(id: userId));
        context.Favorites.Add(CreateFavorite(userId, FavoriteMediaKind.Book, Guid.NewGuid()));
        await context.SaveChangesAsync();

        // Act
        Result<IReadOnlyList<Guid>> result = await sut.GetFavoritedMediaItemIdsForUserAsync(userId, FavoriteMediaKind.Book, [], CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.Empty(result.Value);
    }

    /// <summary>
    /// Creates an open in-memory SQLite anchor connection with a database name unique to the test.
    /// </summary>
    /// <param name="testName">The name of the test, used to make the database name readable.</param>
    /// <returns>The open anchor connection that keeps the database alive for the duration of the test.</returns>
    private static SqliteConnection CreateAnchorConnection(string testName)
    {
        SqliteConnection anchorConnection = new($"Data Source=luminadataccess-favoriterepo-{testName}-{Guid.NewGuid()};Mode=Memory;Cache=Shared");
        anchorConnection.Open();
        return anchorConnection;
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
    /// Creates a favorite with the given owner, kind and media item, defaulting the remaining values.
    /// </summary>
    /// <param name="userId">The Id of the user that owns the favorite.</param>
    /// <param name="mediaKind">The kind of the favorited media item.</param>
    /// <param name="mediaItemId">The Id of the favorited media item.</param>
    /// <param name="createdOnUtc">The time the favorite was created, or <see langword="null"/> for a generated one.</param>
    /// <param name="id">The Id of the favorite, or <see langword="null"/> for a generated one.</param>
    /// <returns>The created favorite.</returns>
    private FavoriteEntity CreateFavorite(
        Guid userId,
        FavoriteMediaKind mediaKind = FavoriteMediaKind.MusicArtist,
        Guid? mediaItemId = null,
        DateTime? createdOnUtc = null,
        Guid? id = null)
    {
        return _favoriteEntityFixture.Create(
            id: id,
            userId: userId,
            mediaKind: mediaKind,
            mediaItemId: mediaItemId,
            createdOnUtc: createdOnUtc,
            createdBy: userId,
            includeUpdatedOnUtc: false,
            includeUpdatedBy: false);
    }

    /// <summary>
    /// Seeds one artist, one album of that artist, one track of that album and one book, each in its own library.
    /// </summary>
    /// <param name="context">The context to seed.</param>
    /// <param name="artistLibraryId">The Id of the library that owns the artist.</param>
    /// <param name="albumLibraryId">The Id of the library that owns the album.</param>
    /// <param name="trackLibraryId">The Id of the library that owns the track.</param>
    /// <param name="bookLibraryId">The Id of the library that owns the book.</param>
    /// <returns>The seeded catalogue entities.</returns>
    private async Task<(ArtistEntity Artist, AlbumEntity Album, TrackEntity Track, BookEntity Book)> SeedCatalogueAsync(
        LuminaDbContext context,
        Guid artistLibraryId,
        Guid albumLibraryId,
        Guid trackLibraryId,
        Guid bookLibraryId)
    {
        ArtistEntity artist = _artistEntityFixture.Create(libraryId: artistLibraryId, includeAlbums: false, includeContributors: false);
        context.Artists.Add(artist);
        await context.SaveChangesAsync();

        AlbumEntity album = _albumEntityFixture.Create(artistId: artist.Id, libraryId: albumLibraryId, includeTracks: false, includeMetadata: false);
        context.Albums.Add(album);
        await context.SaveChangesAsync();

        TrackEntity track = _trackEntityFixture.Create(albumId: album.Id, libraryId: trackLibraryId, includeMetadata: false);
        BookEntity book = _bookEntityFixture.Create(libraryId: bookLibraryId, path: $"/books/{Guid.NewGuid()}.epub", includeMetadata: false);
        context.Tracks.Add(track);
        context.Books.Add(book);
        await context.SaveChangesAsync();

        return (artist, album, track, book);
    }
}
