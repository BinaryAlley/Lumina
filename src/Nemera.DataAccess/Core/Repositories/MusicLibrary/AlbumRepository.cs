#region ========================================================================= USING =====================================================================================
using Nemera.Application.Common.DataAccess.Entities.Common;
using Nemera.Application.Common.DataAccess.Entities.MediaLibrary.AudioLibrary.MusicLibrary;
using Nemera.Application.Common.DataAccess.Repositories.MusicLibrary;
using Nemera.Application.Common.DTO.Pagination;
using Nemera.Application.Common.Infrastructure.Time;
using Nemera.Application.Common.DTO.MediaLibrary.AudioLibrary.MusicLibrary.Albums;
using Nemera.DataAccess.Common.Persistence;
using Nemera.DataAccess.Core.UoW;
using Nemera.Domain.Common.Errors;
using Nemera.Domain.Common.Primitives;
using Nemera.Domain.SharedKernel.Common.Enums.AudioLibrary;
using Nemera.Domain.SharedKernel.Common.Enums.MediaLibrary;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Nemera.DataAccess.Core.Repositories.MusicLibrary;

/// <summary>
/// Repository for albums.
/// </summary>
internal sealed class AlbumRepository : IAlbumRepository
{
    private readonly NemeraDbContext _nemeraDbContext;
    private readonly ITrackRepository _trackRepository;
    private readonly IDateTimeProvider _dateTimeProvider;

    /// <summary>
    /// Initializes a new instance of the <see cref="AlbumRepository"/> class.
    /// </summary>
    /// <param name="nemeraDbContext">Injected Entity Framework DbContext.</param>
    /// <param name="trackRepository">Injected repository for tracks.</param>
    /// <param name="dateTimeProvider">Injected provider for the current UTC time, used to stamp deletions deterministically.</param>
    public AlbumRepository(NemeraDbContext nemeraDbContext, ITrackRepository trackRepository, IDateTimeProvider dateTimeProvider)
    {
        _nemeraDbContext = nemeraDbContext;
        _trackRepository = trackRepository;
        _dateTimeProvider = dateTimeProvider;
    }

    /// <summary>
    /// Adds a new album.
    /// </summary>
    /// <param name="album">The album to add.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="Result{TValue}"/> representing either a successful operation, or an error.</returns>
    public async Task<Result<Created>> InsertAsync(AlbumEntity album, CancellationToken cancellationToken)
    {
        bool doesAlbumExist = await _nemeraDbContext.Albums.AnyAsync(repositoryAlbum => repositoryAlbum.Id == album.Id, cancellationToken: cancellationToken).ConfigureAwait(false);
        if (doesAlbumExist)
            return Errors.Music.AlbumAlreadyExists;

        // A track is unique within its library by its file system path, so the same file can never be registered twice, neither twice in the same request,
        // nor once when it is already registered in the library. The check runs here, for the whole album, because the insert of the album inserts its tracks as well.
        Result<Success> checkTrackPathsResult = await CheckTrackPathsAsync(album, cancellationToken).ConfigureAwait(false);
        if (checkTrackPathsResult.IsFailure)
            return checkTrackPathsResult.Errors;

        // Resolve the tags and genres referenced by the album and its tracks to a single instance per name, replacing the stored ones where they exist, so that
        // neither a stored row nor a name shared by the album and its tracks of the same request is tracked more than once.
        Result<IReadOnlyDictionary<string, TagEntity>> resolveTagsResult = await SharedReferenceResolver.ResolveAsync(
            _nemeraDbContext, album.Tags.Concat(album.Tracks.SelectMany(track => track.Tags)), Errors.Metadata.TagNameCannotBeEmpty, cancellationToken).ConfigureAwait(false);
        if (resolveTagsResult.IsFailure)
            return resolveTagsResult.Errors;
        Result<IReadOnlyDictionary<string, GenreEntity>> resolveGenresResult = await SharedReferenceResolver.ResolveAsync(
            _nemeraDbContext, album.Genres.Concat(album.Tracks.SelectMany(track => track.Genres)), Errors.Metadata.GenreNameCannotBeEmpty, cancellationToken).ConfigureAwait(false);
        if (resolveGenresResult.IsFailure)
            return resolveGenresResult.Errors;

        album.Tags = [.. SharedReferenceResolver.Normalize(album.Tags, resolveTagsResult.Value)];
        album.Genres = [.. SharedReferenceResolver.Normalize(album.Genres, resolveGenresResult.Value)];

        foreach (TrackEntity track in album.Tracks)
        {
            track.Tags = [.. SharedReferenceResolver.Normalize(track.Tags, resolveTagsResult.Value)];
            track.Genres = [.. SharedReferenceResolver.Normalize(track.Genres, resolveGenresResult.Value)];
        }

        _nemeraDbContext.Albums.Add(album);
        return Result.Created;
    }

    /// <summary>
    /// Checks whether the tracks carried by <paramref name="album"/> reference a file system path that is already used by a track of the same library.
    /// </summary>
    /// <param name="album">The album whose track paths are checked.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="Result{TValue}"/> representing either a successful check, or an error.</returns>
    private async Task<Result<Success>> CheckTrackPathsAsync(AlbumEntity album, CancellationToken cancellationToken)
    {
        List<string> trackPaths = [.. album.Tracks.Select(track => track.Path!)];
        if (trackPaths.Count == 0)
            return Result.Success;

        // The same path cannot be referenced by two tracks of the request itself, not only by a track that is already stored.
        if (trackPaths.Count != trackPaths.Distinct(StringComparer.Ordinal).Count())
            return Errors.Music.TrackAlreadyExists;

        Result<IReadOnlyCollection<string>> getExistingPathsResult = await _trackRepository.GetExistingPathsAsync(album.LibraryId, trackPaths, cancellationToken).ConfigureAwait(false);
        if (getExistingPathsResult.IsFailure)
            return getExistingPathsResult.Errors;
        if (getExistingPathsResult.Value.Count > 0)
            return Errors.Music.TrackAlreadyExists;
        return Result.Success;
    }

    /// <summary>
    /// Updates an existing album, replacing only the editable data that actually changed, while preserving its identity, the identity of its children, and its audit columns.
    /// </summary>
    /// <param name="data">The album whose editable data is applied to the stored album.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="Result{TValue}"/> representing either a successful operation, or an error.</returns>
    public async Task<Result<Updated>> UpdateAsync(AlbumEntity data, CancellationToken cancellationToken)
    {
        // Every collection that is reconciled must be loaded, so that the reconciliation can see the rows that are already stored and leave the unchanged ones alone.
        AlbumEntity? foundAlbum = await _nemeraDbContext.Albums
            .Include(repositoryAlbum => repositoryAlbum.Tags)
            .Include(repositoryAlbum => repositoryAlbum.Genres)
            .Include(repositoryAlbum => repositoryAlbum.Ratings)
            .Include(repositoryAlbum => repositoryAlbum.Contributors)
            .AsSplitQuery()
            .FirstOrDefaultAsync(repositoryAlbum => repositoryAlbum.Id == data.Id && !repositoryAlbum.IsDeleted && !repositoryAlbum.Artist!.IsDeleted, cancellationToken).ConfigureAwait(false);
        if (foundAlbum is null)
            return Errors.Music.AlbumNotFound;

        return await ApplyUpdateAsync(foundAlbum, data, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Applies the editable data of <paramref name="data"/> onto the already tracked <paramref name="foundAlbum"/>, without loading it, so that a caller that already
    /// loaded the album graph, like the artist repository, does not make the album repository load it again.
    /// </summary>
    /// <param name="foundAlbum">The tracked album whose editable data is applied.</param>
    /// <param name="data">The album carrying the desired data.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="Result{TValue}"/> representing either a successful operation, or an error.</returns>
    public async Task<Result<Updated>> ApplyUpdateAsync(AlbumEntity foundAlbum, AlbumEntity data, CancellationToken cancellationToken)
    {
        // The stored identity is never overwritten by an edit, and the audit columns are only ever written by the auditing interceptor.
        Guid artistId = foundAlbum.ArtistId;
        Guid libraryId = foundAlbum.LibraryId;
        bool isDeleted = foundAlbum.IsDeleted;
        DateTime? deletedOnUtc = foundAlbum.DeletedOnUtc;
        EditableValuesCopier.CopyEditableValues(_nemeraDbContext, foundAlbum, data);
        foundAlbum.ArtistId = artistId;
        foundAlbum.LibraryId = libraryId;
        // The deletion state is owned by the explicit delete and restore operations. Clearing the modified flags keeps it out of the UPDATE statement,
        // so an edit that raced with a delete can never write the deletion state back and resurrect the album.
        foundAlbum.IsDeleted = isDeleted;
        foundAlbum.DeletedOnUtc = deletedOnUtc;
        _nemeraDbContext.Entry(foundAlbum).Property(album => album.IsDeleted).IsModified = false;
        _nemeraDbContext.Entry(foundAlbum).Property(album => album.DeletedOnUtc).IsModified = false;

        // Tags and genres are shared across the whole database, so the stored rows whose names already exist are reused, and only the missing names are inserted.
        Result<Updated> reconcileTagsResult = await SharedReferenceResolver.ReconcileAsync(_nemeraDbContext, foundAlbum.Tags, data.Tags, Errors.Metadata.TagNameCannotBeEmpty, cancellationToken).ConfigureAwait(false);
        if (reconcileTagsResult.IsFailure)
            return reconcileTagsResult.Errors;
        Result<Updated> reconcileGenresResult = await SharedReferenceResolver.ReconcileAsync(_nemeraDbContext, foundAlbum.Genres, data.Genres, Errors.Metadata.GenreNameCannotBeEmpty, cancellationToken).ConfigureAwait(false);
        if (reconcileGenresResult.IsFailure)
            return reconcileGenresResult.Errors;

        // Ratings are owned value objects with no identity anyone could reference, so a changed rating is replaced as a whole, while the ratings that did not change
        // are left exactly as they are stored. Matching by the rating source keeps an update in place instead of recreating every rating of the album.
        CollectionReconciler.Reconcile(
            foundAlbum.Ratings,
            data.Ratings,
            existingRating => existingRating.Source?.ToString() ?? string.Empty,
            incomingRating => incomingRating.Source?.ToString() ?? string.Empty,
            shouldReplace: (existingRating, incomingRating) => !existingRating.Equals(incomingRating),
            createNew: incomingRating => incomingRating);

        // A contributor participation is matched by the contributor and the role they played. Matched participations keep their identity and their audit columns, so a
        // contributor that is displayed or linked elsewhere is never deleted and re-inserted just because another album of the same artist was edited.
        CollectionReconciler.Reconcile(
            foundAlbum.Contributors,
            data.Contributors,
            existingContributor => (existingContributor.MediaContributorId, existingContributor.Role),
            incomingContributor => (incomingContributor.MediaContributorId, incomingContributor.Role),
            shouldReplace: (existingContributor, incomingContributor) => false,
            createNew: incomingContributor => incomingContributor);

        // The types of the release are owned value objects whose value is also their identity, so a match means there is nothing to change.
        CollectionReconciler.Reconcile(
            foundAlbum.ReleaseTypes,
            data.ReleaseTypes,
            existingReleaseType => existingReleaseType.ReleaseType,
            incomingReleaseType => incomingReleaseType.ReleaseType,
            shouldReplace: (existingReleaseType, incomingReleaseType) => false,
            createNew: incomingReleaseType => incomingReleaseType);

        // The catalog numbers of the release are owned value objects whose value is also their identity, so a match means there is nothing to change.
        CollectionReconciler.Reconcile(
            foundAlbum.CatalogNumbers,
            data.CatalogNumbers,
            existingCatalogNumber => existingCatalogNumber.CatalogNumber,
            incomingCatalogNumber => incomingCatalogNumber.CatalogNumber,
            shouldReplace: (existingCatalogNumber, incomingCatalogNumber) => false,
            createNew: incomingCatalogNumber => incomingCatalogNumber);

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
        // A deleted album, or an album of a deleted artist, is kept in the storage medium only so that its paths stay reserved and it can be restored, so it is never returned to a caller.
        IQueryable<AlbumEntity> query = _nemeraDbContext.Albums.Where(album => !album.IsDeleted && !album.Artist!.IsDeleted);
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
                .Include(album => album.Tracks.Where(track => !track.IsDeleted))
                    .ThenInclude(track => track.Ratings)
                .Include(album => album.Tracks.Where(track => !track.IsDeleted))
                    .ThenInclude(track => track.Genres)
                .Include(album => album.Tracks.Where(track => !track.IsDeleted))
                    .ThenInclude(track => track.Tags)
                .Include(album => album.Tracks.Where(track => !track.IsDeleted))
                    .ThenInclude(track => track.Moods)
                .Include(album => album.Tracks.Where(track => !track.IsDeleted))
                    .ThenInclude(track => track.Isrcs)
                .Include(album => album.Tracks.Where(track => !track.IsDeleted))
                    .ThenInclude(track => track.Contributors)
                .AsSplitQuery();
        }
        return await query.FirstOrDefaultAsync(album => album.Id == id, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Gets the albums of the artist identified by <paramref name="artistId"/> that are not soft deleted.
    /// </summary>
    /// <param name="artistId">The Id of the artist whose albums are retrieved.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="Result{TValue}"/> containing either a collection of <see cref="AlbumEntity"/>, or an error.</returns>
    public async Task<Result<IReadOnlyList<AlbumEntity>>> GetByArtistIdAsync(Guid artistId, CancellationToken cancellationToken)
    {
        return await _nemeraDbContext.Albums
            .Include(album => album.Ratings)
            .Include(album => album.Tags)
            .Include(album => album.Genres)
            .Include(album => album.Contributors)
            .Include(album => album.Tracks.Where(track => !track.IsDeleted))
                .ThenInclude(track => track.Ratings)
            .Include(album => album.Tracks.Where(track => !track.IsDeleted))
                .ThenInclude(track => track.Genres)
            .Include(album => album.Tracks.Where(track => !track.IsDeleted))
                .ThenInclude(track => track.Tags)
            .Include(album => album.Tracks.Where(track => !track.IsDeleted))
                .ThenInclude(track => track.Moods)
            .Include(album => album.Tracks.Where(track => !track.IsDeleted))
                .ThenInclude(track => track.Isrcs)
            .Include(album => album.Tracks.Where(track => !track.IsDeleted))
                .ThenInclude(track => track.Contributors)
            .AsSplitQuery()
            .Where(album => album.ArtistId == artistId && !album.IsDeleted && !album.Artist!.IsDeleted)
            .OrderBy(album => album.Title)
            .ThenBy(album => album.Id)
            .ToListAsync(cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Gets all the albums of the artist identified by <paramref name="artistId"/>, whether they are soft deleted or not.
    /// </summary>
    /// <param name="artistId">The Id of the artist whose albums are retrieved.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="Result{TValue}"/> containing either a collection of <see cref="AlbumEntity"/>, or an error.</returns>
    public async Task<Result<IReadOnlyList<AlbumEntity>>> GetByArtistIdIncludingDeletedAsync(Guid artistId, CancellationToken cancellationToken)
    {
        return await _nemeraDbContext.Albums
            .AsNoTracking()
            .Where(album => album.ArtistId == artistId)
            .OrderBy(album => album.Title)
            .ThenBy(album => album.Id)
            .ToListAsync(cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Gets the album identified by <paramref name="id"/>, whether it is soft deleted or not, without loading its navigation properties.
    /// </summary>
    /// <param name="id">The Id of the album to get.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="Result{TValue}"/> containing either the <see cref="AlbumEntity"/>, or an error.</returns>
    public async Task<Result<AlbumEntity?>> GetIncludingDeletedByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return await _nemeraDbContext.Albums
            .AsNoTracking()
            .FirstOrDefaultAsync(album => album.Id == id, cancellationToken).ConfigureAwait(false);
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
        // The release year and the release types are exposed so that the clients can group and order the releases of an artist.
        // The cover of the album is the cover artwork stored for it, if any, and it is exposed so that the clients can display it without loading the whole album.
        IQueryable<AlbumLiteRow> liteRowsQuery = _nemeraDbContext.Albums
            .AsNoTracking()
            .Where(album => album.ArtistId == artistId && !album.IsDeleted && !album.Artist!.IsDeleted)
            .OrderBy(album => album.Title)
            .ThenBy(album => album.Id)
            .Select(album => new AlbumLiteRow(
                album.Id,
                album.Title,
                album.TotalTracks,
                _nemeraDbContext.MusicArtwork
                    .Where(artwork => artwork.OwnerType == MusicArtworkOwnerType.Album && artwork.OwnerId == album.Id && artwork.ArtworkType == ArtworkType.Cover)
                    .Select(artwork => artwork.FileName)
                    .FirstOrDefault(),
                album.ReReleaseYear ?? album.OriginalReleaseYear,
                album.ReleaseTypes.Select(releaseType => releaseType.ReleaseType).ToList()));

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

    /// <summary>
    /// Gets the album of the artist identified by <paramref name="artistId"/> that has the provided <paramref name="title"/>.
    /// </summary>
    /// <param name="artistId">The Id of the artist whose album is retrieved.</param>
    /// <param name="title">The title of the album to retrieve.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="Result{TValue}"/> containing either the <see cref="AlbumEntity"/> with the provided title, or an error.</returns>
    public async Task<Result<AlbumEntity?>> GetByTitleAsync(Guid artistId, string title, CancellationToken cancellationToken)
    {
        AlbumEntity? album = await _nemeraDbContext.Albums
            .FirstOrDefaultAsync(repositoryAlbum => repositoryAlbum.ArtistId == artistId && repositoryAlbum.Title == title && !repositoryAlbum.IsDeleted && !repositoryAlbum.Artist!.IsDeleted, cancellationToken).ConfigureAwait(false);
        return album;
    }

    /// <summary>
    /// Gets the album of the artist identified by <paramref name="artistId"/> that owns a track stored in the provided <paramref name="directoryPath"/>.
    /// </summary>
    /// <param name="artistId">The Id of the artist whose album is retrieved.</param>
    /// <param name="directoryPath">The file system path of the album directory the album owns.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="Result{TValue}"/> containing either the <see cref="AlbumEntity"/> owning the directory, or an error.</returns>
    public async Task<Result<AlbumEntity?>> GetByTrackDirectoryAsync(Guid artistId, string directoryPath, CancellationToken cancellationToken)
    {
        // The trailing separator is part of the prefix, so a directory whose name is a prefix of another directory does not match it.
        string directoryPrefix = directoryPath.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar) + Path.DirectorySeparatorChar;
        AlbumEntity? album = await _nemeraDbContext.Albums
            .FirstOrDefaultAsync(repositoryAlbum => repositoryAlbum.ArtistId == artistId
                && !repositoryAlbum.IsDeleted
                && !repositoryAlbum.Artist!.IsDeleted
                && repositoryAlbum.Tracks.Any(track => track.Path.StartsWith(directoryPrefix)), cancellationToken).ConfigureAwait(false);
        return album;
    }

    /// <summary>
    /// Gets the soft deleted album identified by <paramref name="id"/>, when its artist is not soft deleted.
    /// </summary>
    /// <param name="id">The Id of the soft deleted album to get.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="Result{TValue}"/> containing either the soft deleted <see cref="AlbumEntity"/>, <see langword="null"/> when the album does not exist, is not soft deleted, or its artist is soft deleted, or an error.</returns>
    public async Task<Result<AlbumEntity?>> GetSoftDeletedByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return await _nemeraDbContext.Albums
            .AsNoTracking()
            .FirstOrDefaultAsync(album => album.Id == id && album.IsDeleted && !album.Artist!.IsDeleted, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Gets the soft deleted albums of the media library identified by <paramref name="libraryId"/>, whose artist is not soft deleted.
    /// </summary>
    /// <param name="libraryId">The Id of the media library whose soft deleted albums are retrieved.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="Result{TValue}"/> containing either the soft deleted albums, or an error.</returns>
    public async Task<Result<IReadOnlyList<AlbumEntity>>> GetSoftDeletedByLibraryIdAsync(Guid libraryId, CancellationToken cancellationToken)
    {
        return await _nemeraDbContext.Albums
            .AsNoTracking()
            .Include(album => album.Artist)
            .Where(album => album.LibraryId == libraryId && album.IsDeleted && !album.Artist!.IsDeleted)
            // The deletion time is stored when the album is deleted, so later writes to the hidden row cannot change the order of the deleted items,
            // and every deleted row carries a deletion time because the delete operation always sets it. All terms sort descending, so the filtered
            // (LibraryId, IsDeleted, DeletedOnUtc, Title, Id) index can be scanned backwards and the database does not build a temporary B-tree.
            .OrderByDescending(album => album.DeletedOnUtc)
            .ThenByDescending(album => album.Title)
            .ThenByDescending(album => album.Id)
            .ToListAsync(cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Soft deletes the album identified by <paramref name="id"/>, without removing its stored metadata or artwork, when another visible album of its artist remains.
    /// </summary>
    /// <remarks>
    /// The update is executed directly against the storage medium, so an entity already tracked by the caller is not refreshed, and the audit fields
    /// stamped by the change tracker are not updated; the deletion time is stored in <see cref="AlbumEntity.DeletedOnUtc"/> instead.
    /// </remarks>
    /// <param name="id">The Id of the album to soft delete.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="Result{TValue}"/> representing either a successful operation, or an error.</returns>
    public async Task<Result<Updated>> SoftDeleteByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        DateTime deletedOnUtc = _dateTimeProvider.UtcNow;

        // The condition that an artist must have at least one remaining album is re-checked here so a delete that raced with another cannot leave the artist without albums.
        int updatedRows = await _nemeraDbContext.Albums
            .Where(album => album.Id == id
                && !album.IsDeleted
                && _nemeraDbContext.Albums.Any(sibling => sibling.ArtistId == album.ArtistId && sibling.Id != album.Id && !sibling.IsDeleted))
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(album => album.IsDeleted, true)
                .SetProperty(album => album.DeletedOnUtc, deletedOnUtc), cancellationToken).ConfigureAwait(false);
        if (updatedRows > 0)
            return Result.Updated;

        // The update matched nothing, so the album is missing, is already deleted, or is the last visible album of its artist. The row is read
        // only to report the reason, never to decide the transition.
        AlbumEntity? album = await _nemeraDbContext.Albums
            .AsNoTracking()
            .FirstOrDefaultAsync(candidate => candidate.Id == id, cancellationToken).ConfigureAwait(false);
        if (album is null || album.IsDeleted)
            return Errors.Music.AlbumNotFound;
        return Errors.Music.ArtistMustHaveAtLeastOneAlbum;
    }

    /// <summary>
    /// Restores the soft deleted album identified by <paramref name="id"/>.
    /// </summary>
    /// <param name="id">The Id of the album to restore.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="Result{TValue}"/> representing either a successful operation, or an error.</returns>
    public async Task<Result<Updated>> RestoreSoftDeletedByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        AlbumEntity? album = await _nemeraDbContext.Albums.FirstOrDefaultAsync(repositoryAlbum => repositoryAlbum.Id == id, cancellationToken).ConfigureAwait(false);
        if (album is null || !album.IsDeleted)
            return Errors.Music.AlbumNotFound;

        album.IsDeleted = false;
        album.DeletedOnUtc = null;
        return Result.Updated;
    }

    /// <summary>
    /// Deletes the album identified by <paramref name="id"/>, together with its tracks and its contributions, by the database cascade.
    /// </summary>
    /// <param name="id">The Id of the album to delete.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="Result{TValue}"/> representing either a successful operation, or an error.</returns>
    public async Task<Result<Deleted>> DeleteByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        AlbumEntity? album = await _nemeraDbContext.Albums
            .FirstOrDefaultAsync(repositoryAlbum => repositoryAlbum.Id == id, cancellationToken).ConfigureAwait(false);
        if (album is null)
            return Errors.Music.AlbumNotFound;

        _nemeraDbContext.Albums.Remove(album);
        return Result.Deleted;
    }

    /// <summary>
    /// Gets the number of albums of the media library identified by <paramref name="libraryId"/> whose metadata has not been enriched yet.
    /// </summary>
    /// <param name="libraryId">The Id of the media library whose albums are counted.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="Result{TValue}"/> containing either the number of albums needing their metadata enriched, or an error.</returns>
    public async Task<Result<int>> GetAlbumsNeedingMetadataCountAsync(Guid libraryId, CancellationToken cancellationToken)
    {
        return await _nemeraDbContext.Albums
            .CountAsync(album => album.LibraryId == libraryId && !album.IsDeleted && !album.Artist!.IsDeleted && album.MetadataStatus != MetadataStatus.Enriched, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Gets the number of albums of the media library identified by <paramref name="libraryId"/> whose artwork has not been resolved yet.
    /// </summary>
    /// <param name="libraryId">The Id of the media library whose albums are counted.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="Result{TValue}"/> containing either the number of albums needing their artwork resolved, or an error.</returns>
    public async Task<Result<int>> GetAlbumsNeedingArtworkCountAsync(Guid libraryId, CancellationToken cancellationToken)
    {
        return await _nemeraDbContext.Albums
            .CountAsync(album => album.LibraryId == libraryId
                        && !album.IsDeleted
                        && !album.Artist!.IsDeleted
                        && !_nemeraDbContext.MusicArtwork.Any(musicArtwork => musicArtwork.OwnerType == MusicArtworkOwnerType.Album
                            && musicArtwork.OwnerId == album.Id
                            && (musicArtwork.Status == ArtworkStatus.Enriched || musicArtwork.Status == ArtworkStatus.NotAvailable)), cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Gets a page of the albums of the media library identified by <paramref name="libraryId"/> whose artwork has not been resolved yet,
    /// together with their artist, ordered by Id, excluding the provided albums.
    /// </summary>
    /// <param name="libraryId">The Id of the media library whose albums are retrieved.</param>
    /// <param name="excludedAlbumIds">The Ids of the albums that were already processed in the current run and must not be retrieved again.</param>
    /// <param name="pageSize">The maximum number of albums to retrieve.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="Result{TValue}"/> containing either a page of albums needing their artwork resolved, or an error.</returns>
    public async Task<Result<IReadOnlyList<AlbumEntity>>> GetAlbumsNeedingArtworkAsync(Guid libraryId, IReadOnlyCollection<Guid> excludedAlbumIds, int pageSize, CancellationToken cancellationToken)
    {
        List<Guid> excludedIds = [.. excludedAlbumIds];
        return await _nemeraDbContext.Albums
            .AsNoTracking()
            .Include(album => album.Artist)
            .Where(album => album.LibraryId == libraryId
                        && !album.IsDeleted
                        && !album.Artist!.IsDeleted
                        && !excludedIds.Contains(album.Id)
                        && !_nemeraDbContext.MusicArtwork.Any(musicArtwork => musicArtwork.OwnerType == MusicArtworkOwnerType.Album
                            && musicArtwork.OwnerId == album.Id
                            && (musicArtwork.Status == ArtworkStatus.Enriched || musicArtwork.Status == ArtworkStatus.NotAvailable)))
            .OrderBy(album => album.Id)
            .Take(pageSize)
            .ToListAsync(cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Gets the file system path of one track of each of the albums identified by <paramref name="albumIds"/>, keyed by the Id of the album.
    /// </summary>
    /// <param name="albumIds">The Ids of the albums whose track path is retrieved.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="Result{TValue}"/> containing either the file system path of a track of each album, or an error.</returns>
    public async Task<Result<IReadOnlyDictionary<Guid, string>>> GetFirstTrackPathsByAlbumIdsAsync(IReadOnlyCollection<Guid> albumIds, CancellationToken cancellationToken)
    {
        List<Guid> albumIdsList = [.. albumIds];
        if (albumIdsList.Count == 0)
            return new Dictionary<Guid, string>();

        List<AlbumTrackPathRow> rows = await _nemeraDbContext.Tracks
            .AsNoTracking()
            .Where(track => albumIdsList.Contains(track.AlbumId) && !track.IsDeleted)
            .OrderBy(track => track.AlbumId)
            .ThenBy(track => track.Path)
            .Select(track => new AlbumTrackPathRow(track.AlbumId, track.Path))
            .ToListAsync(cancellationToken).ConfigureAwait(false);

        // Only one track path is needed per album, so the first one, in the deterministic order above, is kept for each album.
        Dictionary<Guid, string> firstTrackPaths = [];
        foreach (AlbumTrackPathRow row in rows)
            firstTrackPaths.TryAdd(row.AlbumId, row.Path);
        return firstTrackPaths;
    }

    /// <summary>
    /// Resets the metadata enrichment status of all the albums of the media library identified by <paramref name="libraryId"/>.
    /// </summary>
    /// <param name="libraryId">The Id of the media library whose albums are reset.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="Result{TValue}"/> representing either a successful operation, or an error.</returns>
    public async Task<Result<Updated>> ResetMetadataStatusForLibraryAsync(Guid libraryId, CancellationToken cancellationToken)
    {
        await _nemeraDbContext.Albums
            .Where(album => album.LibraryId == libraryId)
            .ExecuteUpdateAsync(setters => setters.SetProperty(album => album.MetadataStatus, MetadataStatus.Pending), cancellationToken).ConfigureAwait(false);
        return Result.Updated;
    }

    /// <summary>
    /// The file system path of one track of an album, used to describe the album in an artwork lookup.
    /// </summary>
    /// <param name="AlbumId">The Id of the album the track belongs to.</param>
    /// <param name="Path">The file system path of the track.</param>
    private sealed record AlbumTrackPathRow(Guid AlbumId, string Path);
}
