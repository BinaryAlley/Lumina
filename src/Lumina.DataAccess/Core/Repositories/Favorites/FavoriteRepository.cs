#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.Favorites;
using Lumina.Application.Common.DataAccess.Repositories.Favorites;
using Lumina.Application.Common.DTO.Favorites;
using Lumina.Application.Common.DTO.Pagination;
using Lumina.DataAccess.Core.UoW;
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.SharedKernel.Common.Enums.AudioLibrary;
using Lumina.Domain.SharedKernel.Common.Enums.Favorites;
using Lumina.Domain.SharedKernel.Common.Enums.MediaContributors;
using Lumina.Domain.SharedKernel.Common.Enums.MediaLibrary;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DomainErrors = Lumina.Domain.Common.Errors.Errors;
#endregion

namespace Lumina.DataAccess.Core.Repositories.Favorites;

/// <summary>
/// Repository for favorites.
/// </summary>
/// <remarks>
/// A favorite references its media item by kind and Id without a foreign key, so reading a page of favorites resolves the display data of
/// the referenced items with one batched query per media kind present in the page, and a favorite whose item no longer exists is kept
/// without display data. Duplicate favorites of the same user on the same item are rejected by the unique (UserId, MediaKind, MediaItemId) index.
/// </remarks>
internal sealed class FavoriteRepository : IFavoriteRepository
{
    private readonly LuminaDbContext _luminaDbContext;

    /// <summary>
    /// Initializes a new instance of the <see cref="FavoriteRepository"/> class.
    /// </summary>
    /// <param name="luminaDbContext">Injected Entity Framework DbContext.</param>
    public FavoriteRepository(LuminaDbContext luminaDbContext)
    {
        _luminaDbContext = luminaDbContext;
    }

    /// <summary>
    /// Adds a new favorite.
    /// </summary>
    /// <param name="data">The favorite to add.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="Result{TValue}"/> representing either a successful operation, or an error.</returns>
    public Task<Result<Created>> InsertAsync(FavoriteEntity data, CancellationToken cancellationToken)
    {
        // The handler checks whether the user already favorited the item and creates a new favorite with a new Id, so this always adds a
        // row. A retried or concurrent add that passed the handler's check is rejected by the unique (UserId, MediaKind, MediaItemId)
        // index when the changes are saved, which is the only check that can observe another request's write.
        _luminaDbContext.Favorites.Add(data);
        return Task.FromResult<Result<Created>>(Result.Created);
    }

    /// <summary>
    /// Deletes the favorite identified by <paramref name="id"/>.
    /// </summary>
    /// <param name="id">The Id of the favorite to delete.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="Result{TValue}"/> representing either a successful operation, or an error.</returns>
    public async Task<Result<Deleted>> DeleteByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        // A handler may already have loaded the same favorite in this unit of work, so the tracked instance is preferred over materializing a second copy.
        FavoriteEntity? favorite = _luminaDbContext.Favorites.Local.FirstOrDefault(trackedFavorite => trackedFavorite.Id == id);
        favorite ??= await _luminaDbContext.Favorites.FirstOrDefaultAsync(foundFavorite => foundFavorite.Id == id, cancellationToken).ConfigureAwait(false);
        if (favorite is null)
            return DomainErrors.Favorites.FavoriteNotFound;

        _luminaDbContext.Favorites.Remove(favorite);
        return Result.Deleted;
    }

    /// <summary>
    /// Gets the favorite of the user identified by <paramref name="userId"/> on the media item identified by <paramref name="mediaKind"/> and <paramref name="mediaItemId"/>, if it exists.
    /// </summary>
    /// <param name="userId">The Id of the user the favorite must belong to.</param>
    /// <param name="mediaKind">The kind of the favorited media item.</param>
    /// <param name="mediaItemId">The Id of the favorited media item.</param>
    /// <param name="shouldTrackEntities">Whether the retrieved entity should be tracked by the persistence medium, so that changes to it can be saved.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>
    /// An <see cref="Result{TValue}"/> containing either the favorite, <see langword="null"/> when the user has not favorited the media item, or an error.
    /// </returns>
    public async Task<Result<FavoriteEntity?>> GetByMediaItemIdForUserAsync(Guid userId, FavoriteMediaKind mediaKind, Guid mediaItemId, bool shouldTrackEntities = true, CancellationToken cancellationToken = default)
    {
        // The user is part of the filter rather than a check performed after the read, so the storage medium never returns a favorite that belongs to another user in the first place.
        IQueryable<FavoriteEntity> query = _luminaDbContext.Favorites;
        if (!shouldTrackEntities)
            query = query.AsNoTracking();
        return await query
            .FirstOrDefaultAsync(favorite => favorite.UserId == userId && favorite.MediaKind == mediaKind && favorite.MediaItemId == mediaItemId, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Gets the Id of the media library that contains the media item identified by <paramref name="mediaKind"/> and <paramref name="mediaItemId"/>, if the media item exists.
    /// </summary>
    /// <param name="mediaKind">The kind of the media item.</param>
    /// <param name="mediaItemId">The Id of the media item.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>
    /// An <see cref="Result{TValue}"/> containing either the Id of the library that contains the media item, <see langword="null"/> when the
    /// media item does not exist or its kind has no catalogued library yet, or an error.
    /// </returns>
    public async Task<Result<Guid?>> GetMediaItemLibraryIdAsync(FavoriteMediaKind mediaKind, Guid mediaItemId, CancellationToken cancellationToken)
    {
        // Only the library Id is projected, so the check never materializes the media item. The planned kinds have no catalogue table yet, so they have no library.
        Guid? libraryId = mediaKind switch
        {
            FavoriteMediaKind.MusicArtist => await _luminaDbContext.Artists
                .AsNoTracking()
                .Where(artist => artist.Id == mediaItemId)
                .Select(artist => (Guid?)artist.LibraryId)
                .FirstOrDefaultAsync(cancellationToken).ConfigureAwait(false),
            FavoriteMediaKind.MusicAlbum => await _luminaDbContext.Albums
                .AsNoTracking()
                .Where(album => album.Id == mediaItemId)
                .Select(album => (Guid?)album.LibraryId)
                .FirstOrDefaultAsync(cancellationToken).ConfigureAwait(false),
            FavoriteMediaKind.MusicTrack => await _luminaDbContext.Tracks
                .AsNoTracking()
                .Where(track => track.Id == mediaItemId)
                .Select(track => (Guid?)track.LibraryId)
                .FirstOrDefaultAsync(cancellationToken).ConfigureAwait(false),
            FavoriteMediaKind.Book => await _luminaDbContext.Books
                .AsNoTracking()
                .Where(book => book.Id == mediaItemId)
                .Select(book => (Guid?)book.LibraryId)
                .FirstOrDefaultAsync(cancellationToken).ConfigureAwait(false),
            _ => null,
        };
        return Result<Guid?>.Success(libraryId);
    }

    /// <summary>
    /// Gets the paginated favorites of the user identified by <paramref name="userId"/>, from the most recently favorited, together with the display data of the media items they reference,
    /// or all the matching favorites when the pagination data is <see langword="null"/>.
    /// </summary>
    /// <param name="userId">The Id of the user whose favorites are retrieved.</param>
    /// <param name="mediaKind">The kind of media item to restrict the favorites to, or <see langword="null"/> for every kind.</param>
    /// <param name="paginationData">The pagination data that includes the current page and the number of items per page to retrieve. If <see langword="null"/>, all matching favorites are returned.</param>
    /// <param name="shouldTrackEntities">Whether the retrieved entities should be tracked by the persistence medium, so that changes to them can be saved. Pass <see langword="false"/> for read-only scenarios, to avoid the tracking overhead.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="Result{TValue}"/> containing either a <see cref="PaginatedResultDto{FavoriteItemDto}"/>, or an error.</returns>
    public async Task<Result<PaginatedResultDto<FavoriteItemDto>>> GetAllForUserAsync(Guid userId, FavoriteMediaKind? mediaKind, PaginationDataDto? paginationData = null, bool shouldTrackEntities = false, CancellationToken cancellationToken = default)
    {
        // The user is part of the filter rather than a check performed after the read, so the storage medium never returns a favorite that belongs to another user in the first place.
        IQueryable<FavoriteEntity> favoritesQuery = _luminaDbContext.Favorites;
        if (!shouldTrackEntities)
            favoritesQuery = favoritesQuery.AsNoTracking();
        favoritesQuery = favoritesQuery.Where(favorite => favorite.UserId == userId);
        if (mediaKind is not null)
        {
            FavoriteMediaKind kindFilter = mediaKind.Value;
            favoritesQuery = favoritesQuery.Where(favorite => favorite.MediaKind == kindFilter);
        }

        // The Id is the tiebreaker, so favorites created at the same instant keep a stable order between pages.
        IOrderedQueryable<FavoriteEntity> orderedFavoritesQuery = favoritesQuery
            .OrderByDescending(favorite => favorite.CreatedOnUtc)
            .ThenBy(favorite => favorite.Id);

        // If no pagination was requested, return all the favorites of the user.
        if (paginationData is null)
        {
            IReadOnlyList<FavoriteItemDto> allFavorites = await ToFavoriteItemsAsync(orderedFavoritesQuery, cancellationToken).ConfigureAwait(false);
            return new PaginatedResultDto<FavoriteItemDto>
            {
                Data = allFavorites,
                CurrentPage = 1,
                PerPage = allFavorites.Count,
                Count = allFavorites.Count,
                NumberOfPages = 1
            };
        }

        int count = await orderedFavoritesQuery.Select(favorite => favorite.Id).CountAsync(cancellationToken).ConfigureAwait(false);
        int numberOfPages = (int)Math.Ceiling((double)count / paginationData.PerPage);
        int currentPage = Math.Min(paginationData.CurrentPage, Math.Max(1, numberOfPages)); // Make sure current page doesn't exceed maximum number of pages.

        // Only the requested page is materialized, because a user can favorite any number of items.
        IQueryable<FavoriteEntity> pageQuery = orderedFavoritesQuery
            .Skip((currentPage - 1) * paginationData.PerPage)
            .Take(paginationData.PerPage);
        IReadOnlyList<FavoriteItemDto> paginatedResult = await ToFavoriteItemsAsync(pageQuery, cancellationToken).ConfigureAwait(false);

        return new PaginatedResultDto<FavoriteItemDto>
        {
            Data = paginatedResult,
            CurrentPage = currentPage,
            PerPage = paginationData.PerPage,
            Count = count,
            NumberOfPages = numberOfPages
        };
    }

    /// <summary>
    /// Reads the display data of the media items referenced by the favorites of <paramref name="favoritesQuery"/>, one query per media kind present.
    /// </summary>
    /// <param name="favoritesQuery">The query of the favorites to read the display data for.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>The favorites together with the display data of the media items they reference.</returns>
    private async Task<IReadOnlyList<FavoriteItemDto>> ToFavoriteItemsAsync(IQueryable<FavoriteEntity> favoritesQuery, CancellationToken cancellationToken)
    {
        List<FavoriteEntity> favorites = await favoritesQuery.ToListAsync(cancellationToken).ConfigureAwait(false);

        Dictionary<FavoriteMediaKind, Dictionary<Guid, FavoriteItemDisplayDto>> displayDataByKind = [];
        foreach (IGrouping<FavoriteMediaKind, FavoriteEntity> kindGroup in favorites.GroupBy(favorite => favorite.MediaKind))
        {
            List<Guid> mediaItemIds = [.. kindGroup.Select(favorite => favorite.MediaItemId).Distinct()];
            displayDataByKind[kindGroup.Key] = await GetDisplayDataAsync(kindGroup.Key, mediaItemIds, cancellationToken).ConfigureAwait(false);
        }

        // A favorite whose item was deleted is kept with no display data, so its owner can still see and remove it instead of it silently disappearing.
        List<FavoriteItemDto> items = [];
        foreach (FavoriteEntity favorite in favorites)
        {
            FavoriteItemDisplayDto? display = null;
            if (displayDataByKind[favorite.MediaKind].TryGetValue(favorite.MediaItemId, out FavoriteItemDisplayDto? foundDisplay))
                display = foundDisplay;
            items.Add(new FavoriteItemDto(favorite, display));
        }
        return items;
    }

    /// <summary>
    /// Gets the Ids, among <paramref name="mediaItemIds"/>, of the media items of kind <paramref name="mediaKind"/> that the user identified by <paramref name="userId"/> has favorited.
    /// </summary>
    /// <param name="userId">The Id of the user whose favorites are checked.</param>
    /// <param name="mediaKind">The kind of the media items.</param>
    /// <param name="mediaItemIds">The Ids of the media items to check.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="Result{TValue}"/> containing either the subset of the Ids that are favorites of the user, or an error.</returns>
    public async Task<Result<IReadOnlyList<Guid>>> GetFavoritedMediaItemIdsForUserAsync(Guid userId, FavoriteMediaKind mediaKind, IReadOnlyCollection<Guid> mediaItemIds, CancellationToken cancellationToken)
    {
        Guid[] requestedIds = [.. mediaItemIds];
        // The unique (UserId, MediaKind, MediaItemId) index serves this lookup, and the caller bounds the number of requested Ids.
        IReadOnlyList<Guid> favoritedIds = await _luminaDbContext.Favorites
            .AsNoTracking()
            .Where(favorite => favorite.UserId == userId && favorite.MediaKind == mediaKind && requestedIds.Contains(favorite.MediaItemId))
            .Select(favorite => favorite.MediaItemId)
            .Distinct()
            .ToListAsync(cancellationToken).ConfigureAwait(false);
        return Result.From(favoritedIds);
    }

    /// <summary>
    /// Reads the display data of the media items of kind <paramref name="mediaKind"/> identified by <paramref name="mediaItemIds"/> in one query per bounded chunk.
    /// </summary>
    /// <param name="mediaKind">The kind of the media items.</param>
    /// <param name="mediaItemIds">The Ids of the media items whose display data is read.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>The display data keyed by media item Id. A media item that no longer exists, or whose kind has no catalogue yet, is absent.</returns>
    private async Task<Dictionary<Guid, FavoriteItemDisplayDto>> GetDisplayDataAsync(FavoriteMediaKind mediaKind, IReadOnlyCollection<Guid> mediaItemIds, CancellationToken cancellationToken)
    {
        if (mediaItemIds.Count == 0)
            return [];

        // The lookup is split into bounded chunks, so even an unpaginated listing never exceeds the storage parameter limit.
        const int LOOKUP_CHUNK_SIZE = 500;
        List<DisplayRow> rows = [];
        foreach (Guid[] chunk in mediaItemIds.Chunk(LOOKUP_CHUNK_SIZE))
            rows.AddRange(await GetDisplayRowsAsync(mediaKind, chunk, cancellationToken).ConfigureAwait(false));
        return rows.ToDictionary(
            row => row.MediaItemId,
            row => new FavoriteItemDisplayDto(
                row.LibraryId,
                row.Title,
                row.CollectionId ?? Guid.Empty,
                row.CollectionTitle ?? string.Empty,
                row.CreatorId ?? Guid.Empty,
                row.CreatorName ?? string.Empty,
                row.CoverPath));
    }

    /// <summary>
    /// Reads the display data of one bounded chunk of media items of kind <paramref name="mediaKind"/> in a single query.
    /// </summary>
    /// <param name="mediaKind">The kind of the media items.</param>
    /// <param name="requestedIds">The bounded chunk of media item Ids whose display data is read.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>The display rows of the media items that exist.</returns>
    private async Task<List<DisplayRow>> GetDisplayRowsAsync(FavoriteMediaKind mediaKind, Guid[] requestedIds, CancellationToken cancellationToken)
    {
        List<DisplayRow> rows;
        switch (mediaKind)
        {
            case FavoriteMediaKind.MusicArtist:
                // The cover of the artist is the cover artwork stored for it, if any, the same one the artist listing exposes.
                rows = await _luminaDbContext.Artists
                    .AsNoTracking()
                    .Where(artist => requestedIds.Contains(artist.Id))
                    .Select(artist => new DisplayRow(
                        artist.Id,
                        artist.LibraryId,
                        artist.Name,
                        null,
                        null,
                        null,
                        null,
                        _luminaDbContext.MusicArtwork
                            .Where(artwork => artwork.OwnerType == MusicArtworkOwnerType.Artist && artwork.OwnerId == artist.Id && artwork.ArtworkType == ArtworkType.Cover)
                            .Select(artwork => artwork.FileName)
                            .FirstOrDefault()))
                    .ToListAsync(cancellationToken).ConfigureAwait(false);
                break;
            case FavoriteMediaKind.MusicAlbum:
                rows = await (from album in _luminaDbContext.Albums.AsNoTracking()
                              join artist in _luminaDbContext.Artists on album.ArtistId equals artist.Id
                              where requestedIds.Contains(album.Id)
                              select new DisplayRow(
                                  album.Id,
                                  album.LibraryId,
                                  album.Title,
                                  null,
                                  null,
                                  artist.Id,
                                  artist.Name,
                                  _luminaDbContext.MusicArtwork
                                      .Where(artwork => artwork.OwnerType == MusicArtworkOwnerType.Album && artwork.OwnerId == album.Id && artwork.ArtworkType == ArtworkType.Cover)
                                      .Select(artwork => artwork.FileName)
                                      .FirstOrDefault()))
                    .ToListAsync(cancellationToken).ConfigureAwait(false);
                break;
            case FavoriteMediaKind.MusicTrack:
                // The cover of a track is the one stored for its album, like in the playlist entries.
                rows = await (from track in _luminaDbContext.Tracks.AsNoTracking()
                              join album in _luminaDbContext.Albums on track.AlbumId equals album.Id
                              join artist in _luminaDbContext.Artists on album.ArtistId equals artist.Id
                              where requestedIds.Contains(track.Id)
                              select new DisplayRow(
                                  track.Id,
                                  track.LibraryId,
                                  track.Title,
                                  album.Id,
                                  album.Title,
                                  artist.Id,
                                  artist.Name,
                                  _luminaDbContext.MusicArtwork
                                      .Where(artwork => artwork.OwnerType == MusicArtworkOwnerType.Album && artwork.OwnerId == album.Id && artwork.ArtworkType == ArtworkType.Cover)
                                      .Select(artwork => artwork.FileName)
                                      .FirstOrDefault()))
                    .ToListAsync(cancellationToken).ConfigureAwait(false);
                break;
            case FavoriteMediaKind.Book:
                // The book catalogue does not persist series yet, so a book has no collection. The creator is the first author by display name,
                // the same author the book listing shows, and the Id is the tiebreaker between authors that share a display name.
                rows = await _luminaDbContext.Books
                    .AsNoTracking()
                    .Where(book => requestedIds.Contains(book.Id))
                    .Select(book => new DisplayRow(
                        book.Id,
                        book.LibraryId,
                        book.Title,
                        null,
                        null,
                        (from bookContributor in _luminaDbContext.BookContributors
                         join contributor in _luminaDbContext.MediaContributors on bookContributor.MediaContributorId equals contributor.Id
                         where bookContributor.BookId == book.Id && bookContributor.Role == MediaContributorRole.Author
                         orderby contributor.DisplayName, contributor.Id
                         select (Guid?)contributor.Id).FirstOrDefault(),
                        (from bookContributor in _luminaDbContext.BookContributors
                         join contributor in _luminaDbContext.MediaContributors on bookContributor.MediaContributorId equals contributor.Id
                         where bookContributor.BookId == book.Id && bookContributor.Role == MediaContributorRole.Author
                         orderby contributor.DisplayName, contributor.Id
                         select contributor.DisplayName).FirstOrDefault(),
                        book.Artwork
                            .Where(artwork => artwork.ArtworkType == ArtworkType.Cover)
                            .Select(artwork => artwork.FileName)
                            .FirstOrDefault()))
                    .ToListAsync(cancellationToken).ConfigureAwait(false);
                break;
            default:
                return [];
        }
        return rows;
    }

    /// <summary>
    /// The display data of one favorited media item as read from storage, before the absent values are normalized to the generic "no value" form.
    /// </summary>
    /// <param name="MediaItemId">The Id of the media item.</param>
    /// <param name="LibraryId">The Id of the media library that contains the media item.</param>
    /// <param name="Title">The title of the media item.</param>
    /// <param name="CollectionId">The Id of the collection that contains the media item, if any.</param>
    /// <param name="CollectionTitle">The title of the collection that contains the media item, if any.</param>
    /// <param name="CreatorId">The Id of the creator of the media item, if any.</param>
    /// <param name="CreatorName">The name of the creator of the media item, if any.</param>
    /// <param name="CoverPath">The path of the cover art of the media item, if available.</param>
    private sealed record DisplayRow(
        Guid MediaItemId,
        Guid LibraryId,
        string Title,
        Guid? CollectionId,
        string? CollectionTitle,
        Guid? CreatorId,
        string? CreatorName,
        string? CoverPath
    );
}
