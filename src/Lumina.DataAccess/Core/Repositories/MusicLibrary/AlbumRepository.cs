#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.Common;
using Lumina.Application.Common.DataAccess.Entities.MediaLibrary.AudioLibrary.MusicLibrary;
using Lumina.Application.Common.DataAccess.Repositories.MusicLibrary;
using Lumina.Application.Common.DTO.Pagination;
using Lumina.Application.Common.DTO.MediaLibrary.AudioLibrary.MusicLibrary.Albums;
using Lumina.DataAccess.Core.UoW;
using Lumina.Domain.Common.Errors;
using Lumina.Domain.Common.Primitives;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.DataAccess.Core.Repositories.MusicLibrary;

/// <summary>
/// Repository for albums.
/// </summary>
internal sealed class AlbumRepository : IAlbumRepository
{
    private readonly LuminaDbContext _luminaDbContext;

    /// <summary>
    /// Initializes a new instance of the <see cref="AlbumRepository"/> class.
    /// </summary>
    /// <param name="luminaDbContext">Injected Entity Framework DbContext.</param>
    public AlbumRepository(LuminaDbContext luminaDbContext)
    {
        _luminaDbContext = luminaDbContext;
    }

    /// <summary>
    /// Adds a new album.
    /// </summary>
    /// <param name="album">The album to add.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="Result{TValue}"/> representing either a successful operation, or an error.</returns>
    public async Task<Result<Created>> InsertAsync(AlbumEntity album, CancellationToken cancellationToken)
    {
        bool doesAlbumExist = await _luminaDbContext.Albums.AnyAsync(repositoryAlbum => repositoryAlbum.Id == album.Id, cancellationToken: cancellationToken).ConfigureAwait(false);
        if (doesAlbumExist)
            return Errors.Music.AlbumAlreadyExists;

        // Fetch the tags and genres referenced anywhere in the aggregate, so that the shared tables are not duplicated.
        List<string> tagNames = [.. album.Tags.Select(tag => tag.Name!)
            .Concat(album.Tracks.SelectMany(track => track.Tags.Select(tag => tag.Name!)))
            .Distinct()];
        List<string> genreNames = [.. album.Genres.Select(genre => genre.Name!)
            .Concat(album.Tracks.SelectMany(track => track.Genres.Select(genre => genre.Name!)))
            .Distinct()];

        List<TagEntity> existingTags = await _luminaDbContext.Set<TagEntity>()
            .Where(tag => tagNames.Contains(tag.Name!))
            .ToListAsync(cancellationToken).ConfigureAwait(false);
        List<GenreEntity> existingGenres = await _luminaDbContext.Set<GenreEntity>()
            .Where(genre => genreNames.Contains(genre.Name!))
            .ToListAsync(cancellationToken).ConfigureAwait(false);

        // Replace the tags and genres of the album and of its tracks with the existing ones, so that the shared tables are not duplicated.
        album.Tags = [.. album.Tags.Select(tag => existingTags.FirstOrDefault(existingTag => existingTag.Name == tag.Name) ?? tag)];
        album.Genres = [.. album.Genres.Select(genre => existingGenres.FirstOrDefault(existingGenre => existingGenre.Name == genre.Name) ?? genre)];

        foreach (TrackEntity track in album.Tracks)
        {
            track.Tags = [.. track.Tags.Select(tag => existingTags.FirstOrDefault(existingTag => existingTag.Name == tag.Name) ?? tag)];
            track.Genres = [.. track.Genres.Select(genre => existingGenres.FirstOrDefault(existingGenre => existingGenre.Name == genre.Name) ?? genre)];
        }

        _luminaDbContext.Albums.Add(album);
        return Result.Created;
    }

    /// <summary>
    /// Updates an existing album, replacing its editable data while preserving its identity and audit columns.
    /// </summary>
    /// <param name="album">The album whose editable data is applied to the stored album.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="Result{TValue}"/> representing either a successful operation, or an error.</returns>
    public async Task<Result<Updated>> UpdateAsync(AlbumEntity album, CancellationToken cancellationToken)
    {
        AlbumEntity? foundAlbum = await _luminaDbContext.Albums
            .Include(repositoryAlbum => repositoryAlbum.Tags)
            .Include(repositoryAlbum => repositoryAlbum.Genres)
            .Include(repositoryAlbum => repositoryAlbum.Contributors)
            .FirstOrDefaultAsync(repositoryAlbum => repositoryAlbum.Id == album.Id, cancellationToken).ConfigureAwait(false);
        if (foundAlbum is null)
            return Errors.Music.AlbumNotFound;

        // The stored identity and audit columns are never overwritten by an edit.
        Guid artistId = foundAlbum.ArtistId;
        Guid libraryId = foundAlbum.LibraryId;
        DateTime createdOnUtc = foundAlbum.CreatedOnUtc;
        Guid createdBy = foundAlbum.CreatedBy;

        // Copy the editable scalar properties of the album, ignoring the unchanged collection columns.
        _luminaDbContext.Entry(foundAlbum).CurrentValues.SetValues(album);
        foundAlbum.ArtistId = artistId;
        foundAlbum.LibraryId = libraryId;
        foundAlbum.CreatedOnUtc = createdOnUtc;
        foundAlbum.CreatedBy = createdBy;

        // Reuse the stored tags and genres whose names already exist, so that the shared tables are not duplicated.
        List<TagEntity> existingTags = await _luminaDbContext.Set<TagEntity>()
            .Where(tag => album.Tags.Select(albumTag => albumTag.Name).Contains(tag.Name))
            .ToListAsync(cancellationToken).ConfigureAwait(false);
        List<GenreEntity> existingGenres = await _luminaDbContext.Set<GenreEntity>()
            .Where(genre => album.Genres.Select(albumGenre => albumGenre.Name).Contains(genre.Name))
            .ToListAsync(cancellationToken).ConfigureAwait(false);

        foundAlbum.Tags.Clear();
        foundAlbum.Tags.UnionWith(album.Tags.Select(tag => existingTags.FirstOrDefault(existingTag => existingTag.Name == tag.Name) ?? tag));
        foundAlbum.Genres.Clear();
        foundAlbum.Genres.UnionWith(album.Genres.Select(genre => existingGenres.FirstOrDefault(existingGenre => existingGenre.Name == genre.Name) ?? genre));

        // Replace the owned ratings and contributors collections of the album, so that removed entries are deleted and new entries are inserted.
        foundAlbum.Ratings.Clear();
        foundAlbum.Ratings.AddRange(album.Ratings);
        foundAlbum.Contributors.Clear();
        foundAlbum.Contributors.AddRange(album.Contributors);

        return Result.Updated;
    }

    /// <summary>
    /// Gets an album by its Id.
    /// </summary>
    /// <param name="id">The Id of the album to get.</param>
    /// <param name="shouldIncludeNavigationProperties">Whether the navigation properties of the album should be loaded together with the album itself.</param>
    /// <param name="shouldTrackEntities">Whether the retrieved album should be tracked by the persistence medium, so that changes to it can be saved.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="Result{TValue}"/> containing either an <see cref="AlbumEntity"/>, or an error.</returns>
    public async Task<Result<AlbumEntity?>> GetByIdAsync(Guid id, bool shouldIncludeNavigationProperties = true, bool shouldTrackEntities = true, CancellationToken cancellationToken = default)
    {
        IQueryable<AlbumEntity> query = _luminaDbContext.Albums;
        if (!shouldTrackEntities)
            query = query.AsNoTracking();
        if (shouldIncludeNavigationProperties)
        {
            query = query
                .Include(album => album.Artist)
                .Include(album => album.Ratings)
                .Include(album => album.Tags)
                .Include(album => album.Genres)
                .Include(album => album.Contributors)
                .Include(album => album.Tracks)
                    .ThenInclude(track => track.Ratings)
                .Include(album => album.Tracks)
                    .ThenInclude(track => track.Genres)
                .Include(album => album.Tracks)
                    .ThenInclude(track => track.Tags)
                .Include(album => album.Tracks)
                    .ThenInclude(track => track.Moods)
                .Include(album => album.Tracks)
                    .ThenInclude(track => track.Isrcs)
                .Include(album => album.Tracks)
                    .ThenInclude(track => track.Contributors)
                .AsSplitQuery();
        }
        return await query.FirstOrDefaultAsync(album => album.Id == id, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Deletes the album identified by <paramref name="id"/>.
    /// </summary>
    /// <param name="id">The Id of the album to delete.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="Result{TValue}"/> representing either a successful operation, or an error.</returns>
    public async Task<Result<Deleted>> DeleteByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        AlbumEntity? album = await _luminaDbContext.Albums
            .FirstOrDefaultAsync(repositoryAlbum => repositoryAlbum.Id == id, cancellationToken).ConfigureAwait(false);
        if (album is null)
            return Errors.Music.AlbumNotFound;

        _luminaDbContext.Albums.Remove(album);
        return Result.Deleted;
    }

    /// <summary>
    /// Gets all the albums of the artist identified by <paramref name="artistId"/>.
    /// </summary>
    /// <param name="artistId">The Id of the artist whose albums are retrieved.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="Result{TValue}"/> containing either a collection of <see cref="AlbumEntity"/>, or an error.</returns>
    public async Task<Result<IReadOnlyList<AlbumEntity>>> GetByArtistIdAsync(Guid artistId, CancellationToken cancellationToken)
    {
        return await _luminaDbContext.Albums
            .Include(album => album.Ratings)
            .Include(album => album.Tags)
            .Include(album => album.Genres)
            .Include(album => album.Contributors)
            .Include(album => album.Tracks)
                .ThenInclude(track => track.Ratings)
            .Include(album => album.Tracks)
                .ThenInclude(track => track.Genres)
            .Include(album => album.Tracks)
                .ThenInclude(track => track.Tags)
            .Include(album => album.Tracks)
                .ThenInclude(track => track.Moods)
            .Include(album => album.Tracks)
                .ThenInclude(track => track.Isrcs)
            .Include(album => album.Tracks)
                .ThenInclude(track => track.Contributors)
            .AsSplitQuery()
            .Where(album => album.ArtistId == artistId)
            .OrderBy(album => album.Title)
            .ThenBy(album => album.Id)
            .ToListAsync(cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Gets the lightweight read models of the albums of the artist identified by <paramref name="artistId"/>,
    /// or all of them when the pagination data is <see langword="null"/>, projecting only the fields needed to display the albums in a card-based grid.
    /// </summary>
    /// <param name="artistId">The Id of the artist whose albums are retrieved.</param>
    /// <param name="paginationData">The pagination data that includes the current page and the number of items per page to retrieve. If <see langword="null"/>, all matching albums are returned.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="Result{TValue}"/> containing either a <see cref="PaginatedResultDto{AlbumLiteRow}"/>, or an error.</returns>
    public async Task<Result<PaginatedResultDto<AlbumLiteRow>>> GetAlbumsLiteByArtistIdAsync(Guid artistId, PaginationDataDto? paginationData, CancellationToken cancellationToken)
    {
        // The lightweight read models are always retrieved without tracking, because they are never modified by the caller.
        IQueryable<AlbumLiteRow> liteRowsQuery = _luminaDbContext.Albums
            .AsNoTracking()
            .Where(album => album.ArtistId == artistId)
            .OrderBy(album => album.Title)
            .ThenBy(album => album.Id)
            .Select(album => new AlbumLiteRow
            {
                Id = album.Id,
                Title = album.Title,
                TotalTracks = album.TotalTracks
            });

        if (paginationData is null)
        {
            IReadOnlyList<AlbumLiteRow> allAlbums = await liteRowsQuery.ToListAsync(cancellationToken).ConfigureAwait(false);
            return new PaginatedResultDto<AlbumLiteRow>
            {
                Data = allAlbums,
                CurrentPage = 1,
                PerPage = allAlbums.Count,
                Count = allAlbums.Count,
                NumberOfPages = 1
            };
        }

        int count = await liteRowsQuery.CountAsync(cancellationToken).ConfigureAwait(false);
        int numberOfPages = (int)Math.Ceiling((double)count / paginationData.PerPage);
        int currentPage = Math.Min(paginationData.CurrentPage, Math.Max(1, numberOfPages));

        IReadOnlyList<AlbumLiteRow> paginatedResult = await liteRowsQuery
            .Skip((currentPage - 1) * paginationData.PerPage)
            .Take(paginationData.PerPage)
            .ToListAsync(cancellationToken).ConfigureAwait(false);

        return new PaginatedResultDto<AlbumLiteRow>
        {
            Data = paginatedResult,
            CurrentPage = currentPage,
            PerPage = paginationData.PerPage,
            Count = count,
            NumberOfPages = numberOfPages
        };
    }
}
