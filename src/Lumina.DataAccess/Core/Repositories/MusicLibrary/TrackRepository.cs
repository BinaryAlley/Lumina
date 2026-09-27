#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.Common;
using Lumina.Application.Common.DataAccess.Entities.MediaLibrary.AudioLibrary.MusicLibrary;
using Lumina.Application.Common.DataAccess.Repositories.MusicLibrary;
using Lumina.Application.Common.DTO.Pagination;
using Lumina.Application.Common.DTO.MediaLibrary.AudioLibrary.MusicLibrary.Tracks;
using Lumina.DataAccess.Common.Persistence;
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
/// Repository for tracks.
/// </summary>
internal sealed class TrackRepository : ITrackRepository
{
    private readonly LuminaDbContext _luminaDbContext;

    /// <summary>
    /// Initializes a new instance of the <see cref="TrackRepository"/> class.
    /// </summary>
    /// <param name="luminaDbContext">Injected Entity Framework DbContext.</param>
    public TrackRepository(LuminaDbContext luminaDbContext)
    {
        _luminaDbContext = luminaDbContext;
    }

    /// <summary>
    /// Adds a new track.
    /// </summary>
    /// <param name="track">The track to add.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="Result{TValue}"/> representing either a successful operation, or an error.</returns>
    public async Task<Result<Created>> InsertAsync(TrackEntity track, CancellationToken cancellationToken)
    {
        bool doesTrackExist = await _luminaDbContext.Tracks.AnyAsync(repositoryTrack => repositoryTrack.Id == track.Id, cancellationToken: cancellationToken).ConfigureAwait(false);
        if (doesTrackExist)
            return Errors.Music.TrackAlreadyExists;

        // A track is unique within its library by its file system path, so the same file can never be registered twice in the same library.
        Result<IReadOnlyCollection<string>> getExistingPathsResult = await GetExistingPathsAsync(track.LibraryId, [track.Path], cancellationToken).ConfigureAwait(false);
        if (getExistingPathsResult.IsFailure)
            return getExistingPathsResult.Errors;
        if (getExistingPathsResult.Value.Count > 0)
            return Errors.Music.TrackAlreadyExists;

        // Resolve the tags and genres of the track to a single instance per name, replacing the stored ones where they exist, so that the shared tables are not duplicated.
        Result<IReadOnlyDictionary<string, TagEntity>> resolveTagsResult = await SharedReferenceResolver.ResolveAsync(_luminaDbContext, track.Tags, Errors.Metadata.TagNameCannotBeEmpty, cancellationToken).ConfigureAwait(false);
        if (resolveTagsResult.IsFailure)
            return resolveTagsResult.Errors;
        Result<IReadOnlyDictionary<string, GenreEntity>> resolveGenresResult = await SharedReferenceResolver.ResolveAsync(_luminaDbContext, track.Genres, Errors.Metadata.GenreNameCannotBeEmpty, cancellationToken).ConfigureAwait(false);
        if (resolveGenresResult.IsFailure)
            return resolveGenresResult.Errors;

        track.Tags = [.. SharedReferenceResolver.Normalize(track.Tags, resolveTagsResult.Value)];
        track.Genres = [.. SharedReferenceResolver.Normalize(track.Genres, resolveGenresResult.Value)];

        _luminaDbContext.Tracks.Add(track);
        return Result.Created;
    }

    /// <summary>
    /// Gets the subset of <paramref name="paths"/> that is already used by a track of the library identified by <paramref name="libraryId"/>.
    /// </summary>
    /// <param name="libraryId">The Id of the library whose tracks are searched.</param>
    /// <param name="paths">The track paths to check.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="Result{TValue}"/> containing either the paths that are already used, or an error.</returns>
    public async Task<Result<IReadOnlyCollection<string>>> GetExistingPathsAsync(Guid libraryId, IReadOnlyCollection<string> paths, CancellationToken cancellationToken)
    {
        if (paths.Count == 0)
            return Result.From<IReadOnlyCollection<string>>([]);

        // The stored paths are compared ordinally, matching the case sensitive comparison used by the unique index of the storage medium.
        List<string> distinctPaths = [.. paths.Distinct(StringComparer.Ordinal)];
        List<string> existingPaths = await _luminaDbContext.Tracks
            .Where(repositoryTrack => repositoryTrack.LibraryId == libraryId && distinctPaths.Contains(repositoryTrack.Path))
            .Select(repositoryTrack => repositoryTrack.Path)
            .ToListAsync(cancellationToken).ConfigureAwait(false);
        return Result.From<IReadOnlyCollection<string>>(existingPaths);
    }

    /// <summary>
    /// Updates an existing track, replacing only the editable data that actually changed, while preserving its identity, the identity of its children, and its audit columns.
    /// </summary>
    /// <param name="data">The track whose editable data is applied to the stored track.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="Result{TValue}"/> representing either a successful operation, or an error.</returns>
    public async Task<Result<Updated>> UpdateAsync(TrackEntity data, CancellationToken cancellationToken)
    {
        // Every collection that is reconciled must be loaded, so that the reconciliation can see the rows that are already stored and leave the unchanged ones alone.
        TrackEntity? foundTrack = await _luminaDbContext.Tracks
            .Include(repositoryTrack => repositoryTrack.Tags)
            .Include(repositoryTrack => repositoryTrack.Genres)
            .Include(repositoryTrack => repositoryTrack.Ratings)
            .Include(repositoryTrack => repositoryTrack.Contributors)
            .Include(repositoryTrack => repositoryTrack.Moods)
            .Include(repositoryTrack => repositoryTrack.Isrcs)
            .AsSplitQuery()
            .FirstOrDefaultAsync(repositoryTrack => repositoryTrack.Id == data.Id, cancellationToken).ConfigureAwait(false);
        if (foundTrack is null)
            return Errors.Music.TrackNotFound;

        return await ApplyUpdateAsync(foundTrack, data, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Applies the editable data of <paramref name="data"/> onto the already tracked <paramref name="foundTrack"/>, without loading it, so that a caller that already
    /// loaded the track graph, like the artist repository, does not make the track repository load it again.
    /// </summary>
    /// <param name="foundTrack">The tracked track whose editable data is applied.</param>
    /// <param name="data">The track carrying the desired data.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="Result{TValue}"/> representing either a successful operation, or an error.</returns>
    public async Task<Result<Updated>> ApplyUpdateAsync(TrackEntity foundTrack, TrackEntity data, CancellationToken cancellationToken)
    {
        // The stored identity is never overwritten by an edit, and the audit columns are only ever written by the auditing interceptor.
        Guid albumId = foundTrack.AlbumId;
        Guid libraryId = foundTrack.LibraryId;
        EditableValuesCopier.CopyEditableValues(_luminaDbContext, foundTrack, data);
        foundTrack.AlbumId = albumId;
        foundTrack.LibraryId = libraryId;

        // Tags and genres are shared across the whole database, so the stored rows whose names already exist are reused, and only the missing names are inserted.
        Result<Updated> reconcileTagsResult = await SharedReferenceResolver.ReconcileAsync(_luminaDbContext, foundTrack.Tags, data.Tags, Errors.Metadata.TagNameCannotBeEmpty, cancellationToken).ConfigureAwait(false);
        if (reconcileTagsResult.IsFailure)
            return reconcileTagsResult.Errors;
        Result<Updated> reconcileGenresResult = await SharedReferenceResolver.ReconcileAsync(_luminaDbContext, foundTrack.Genres, data.Genres, Errors.Metadata.GenreNameCannotBeEmpty, cancellationToken).ConfigureAwait(false);
        if (reconcileGenresResult.IsFailure)
            return reconcileGenresResult.Errors;

        // Ratings are owned value objects with no identity anyone could reference, so a changed rating is replaced as a whole, while the ratings that did not change
        // are left exactly as they are stored. Matching by the rating source keeps an update in place instead of recreating every rating of the track.
        CollectionReconciler.Reconcile(
            foundTrack.Ratings,
            data.Ratings,
            existingRating => existingRating.Source?.ToString() ?? string.Empty,
            incomingRating => incomingRating.Source?.ToString() ?? string.Empty,
            shouldReplace: (existingRating, incomingRating) => !existingRating.Equals(incomingRating),
            createNew: incomingRating => incomingRating);

        // A contributor participation is matched by the contributor and the role they played. Matched participations keep their identity and their audit columns, so a
        // contributor that is displayed or linked elsewhere is never deleted and re-inserted just because another track of the same album was edited.
        CollectionReconciler.Reconcile(
            foundTrack.Contributors,
            data.Contributors,
            existingContributor => (existingContributor.MediaContributorId, existingContributor.Role),
            incomingContributor => (incomingContributor.MediaContributorId, incomingContributor.Role),
            shouldReplace: (existingContributor, incomingContributor) => false,
            createNew: incomingContributor => incomingContributor);

        // Moods and ISRCs are owned value objects whose value is also their identity, so a match means there is nothing to change.
        CollectionReconciler.Reconcile(
            foundTrack.Moods,
            data.Moods,
            existingMood => existingMood.Name,
            incomingMood => incomingMood.Name,
            shouldReplace: (existingMood, incomingMood) => false,
            createNew: incomingMood => incomingMood);

        CollectionReconciler.Reconcile(
            foundTrack.Isrcs,
            data.Isrcs,
            existingIsrc => existingIsrc.Value,
            incomingIsrc => incomingIsrc.Value,
            shouldReplace: (existingIsrc, incomingIsrc) => false,
            createNew: incomingIsrc => incomingIsrc);

        return Result.Updated;
    }

    /// <summary>
    /// Gets a track by its Id.
    /// </summary>
    /// <param name="id">The Id of the track to get.</param>
    /// <param name="shouldIncludeNavigationProperties">Whether the navigation properties of the track should be loaded together with the track itself.</param>
    /// <param name="shouldTrackEntities">Whether the retrieved track should be tracked by the persistence medium, so that changes to it can be saved.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="Result{TValue}"/> containing either a <see cref="TrackEntity"/>, or an error.</returns>
    public async Task<Result<TrackEntity?>> GetByIdAsync(Guid id, bool shouldIncludeNavigationProperties = true, bool shouldTrackEntities = true, CancellationToken cancellationToken = default)
    {
        IQueryable<TrackEntity> query = _luminaDbContext.Tracks;
        if (!shouldTrackEntities)
            query = query.AsNoTracking();
        if (shouldIncludeNavigationProperties)
        {
            query = query
                .Include(track => track.Album)
                .ThenInclude(album => album!.Artist)
                .Include(track => track.Ratings)
                .Include(track => track.Tags)
                .Include(track => track.Genres)
                .Include(track => track.Moods)
                .Include(track => track.Isrcs)
                .Include(track => track.Contributors)
                .AsSplitQuery();
        }
        return await query.FirstOrDefaultAsync(track => track.Id == id, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Gets all the tracks of the album identified by <paramref name="albumId"/>.
    /// </summary>
    /// <param name="albumId">The Id of the album whose tracks are retrieved.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="Result{TValue}"/> containing either a collection of <see cref="TrackEntity"/>, or an error.</returns>
    public async Task<Result<IReadOnlyList<TrackEntity>>> GetByAlbumIdAsync(Guid albumId, CancellationToken cancellationToken)
    {
        return await _luminaDbContext.Tracks
            .Include(track => track.Ratings)
            .Include(track => track.Tags)
            .Include(track => track.Genres)
            .Include(track => track.Moods)
            .Include(track => track.Isrcs)
            .Include(track => track.Contributors)
            .AsSplitQuery()
            .Where(track => track.AlbumId == albumId)
            .OrderBy(track => track.DiscNumber)
            .ThenBy(track => track.TrackNumber)
            .ThenBy(track => track.Id)
            .ToListAsync(cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Gets the lightweight read models of the tracks of the album identified by <paramref name="albumId"/>,
    /// or all of them when the pagination data is <see langword="null"/>, projecting only the fields needed to display the tracks of an album.
    /// </summary>
    /// <param name="albumId">The Id of the album whose tracks are retrieved.</param>
    /// <param name="paginationData">The pagination data that includes the current page and the number of items per page to retrieve. If <see langword="null"/>, all matching tracks are returned.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="Result{TValue}"/> containing either a <see cref="PaginatedResultDto{TrackLiteRow}"/>, or an error.</returns>
    public async Task<Result<PaginatedResultDto<TrackLiteRow>>> GetTracksLiteByAlbumIdAsync(Guid albumId, PaginationDataDto? paginationData, CancellationToken cancellationToken)
    {
        // The lightweight read models are always retrieved without tracking, because they are never modified by the caller.
        IQueryable<TrackLiteRow> liteRowsQuery = _luminaDbContext.Tracks
            .AsNoTracking()
            .Where(track => track.AlbumId == albumId)
            .OrderBy(track => track.DiscNumber)
            .ThenBy(track => track.TrackNumber)
            .ThenBy(track => track.Id)
            .Select(track => new TrackLiteRow
            {
                Id = track.Id,
                Title = track.Title,
                TrackNumber = track.TrackNumber,
                DiscNumber = track.DiscNumber
            });

        if (paginationData is null)
        {
            IReadOnlyList<TrackLiteRow> allTracks = await liteRowsQuery.ToListAsync(cancellationToken).ConfigureAwait(false);
            return new PaginatedResultDto<TrackLiteRow>
            {
                Data = allTracks,
                CurrentPage = 1,
                PerPage = allTracks.Count,
                Count = allTracks.Count,
                NumberOfPages = 1
            };
        }

        int count = await liteRowsQuery.CountAsync(cancellationToken).ConfigureAwait(false);
        int numberOfPages = (int)Math.Ceiling((double)count / paginationData.PerPage);
        int currentPage = Math.Min(paginationData.CurrentPage, Math.Max(1, numberOfPages));

        IReadOnlyList<TrackLiteRow> paginatedResult = await liteRowsQuery
            .Skip((currentPage - 1) * paginationData.PerPage)
            .Take(paginationData.PerPage)
            .ToListAsync(cancellationToken).ConfigureAwait(false);

        return new PaginatedResultDto<TrackLiteRow>
        {
            Data = paginatedResult,
            CurrentPage = currentPage,
            PerPage = paginationData.PerPage,
            Count = count,
            NumberOfPages = numberOfPages
        };
    }
}
