#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.Common;
using Lumina.Application.Common.DataAccess.Entities.MediaLibrary.AudioLibrary.MusicLibrary;
using Lumina.Application.Common.DataAccess.Repositories.MusicLibrary;
using Lumina.Application.Common.DTO.Filtering;
using Lumina.Application.Common.DTO.MediaLibrary.AudioLibrary.MusicLibrary.Artists;
using Lumina.Application.Common.DTO.Pagination;
using Lumina.Application.Common.Specifications;
using Lumina.DataAccess.Common.Persistence;
using Lumina.DataAccess.Core.Repositories.MusicLibrary.Specifications;
using Lumina.DataAccess.Core.UoW;
using Lumina.Domain.Common.Errors;
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.SharedKernel.Common.Enums.AudioLibrary;
using Lumina.Domain.SharedKernel.Common.Enums.Common;
using Lumina.Domain.SharedKernel.Common.Enums.MediaLibrary;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.DataAccess.Core.Repositories.MusicLibrary;

/// <summary>
/// Repository for artists.
/// </summary>
internal sealed class ArtistRepository : IArtistRepository
{
    private readonly LuminaDbContext _luminaDbContext;
    private readonly IAlbumRepository _albumRepository;
    private readonly ITrackRepository _trackRepository;

    /// <summary>
    /// Initializes a new instance of the <see cref="ArtistRepository"/> class.
    /// </summary>
    /// <param name="luminaDbContext">Injected Entity Framework DbContext.</param>
    /// <param name="albumRepository">Injected repository for albums.</param>
    /// <param name="trackRepository">Injected repository for tracks.</param>
    public ArtistRepository(LuminaDbContext luminaDbContext, IAlbumRepository albumRepository, ITrackRepository trackRepository)
    {
        _luminaDbContext = luminaDbContext;
        _albumRepository = albumRepository;
        _trackRepository = trackRepository;
    }

    /// <summary>
    /// Adds a new artist.
    /// </summary>
    /// <param name="artist">The artist to add.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="Result{TValue}"/> representing either a successful operation, or an error.</returns>
    public async Task<Result<Created>> InsertAsync(ArtistEntity artist, CancellationToken cancellationToken)
    {
        bool doesArtistExist = await _luminaDbContext.Artists.AnyAsync(repositoryArtist => repositoryArtist.Id == artist.Id, cancellationToken: cancellationToken).ConfigureAwait(false);
        if (doesArtistExist)
            return Errors.Music.ArtistAlreadyExists;

        // An artist is unique within its library by its name, so the same artist can never be registered twice in the same library.
        bool doesArtistNameExist = await _luminaDbContext.Artists.AnyAsync(repositoryArtist => repositoryArtist.LibraryId == artist.LibraryId && repositoryArtist.Name == artist.Name, cancellationToken: cancellationToken).ConfigureAwait(false);
        if (doesArtistNameExist)
            return Errors.Music.ArtistAlreadyExists;

        // A track is unique within its library by its file system path, so the same file can never be registered twice, neither twice in the same request,
        // nor once when it is already registered in the library. The check runs here, for the whole artist, because the insert of the artist inserts its albums and their tracks as well.
        List<string> trackPaths = [.. artist.Albums.SelectMany(album => album.Tracks).Select(track => track.Path!)];
        Result<Success> checkTrackPathsResult = await CheckTrackPathsAsync(artist.LibraryId, trackPaths, cancellationToken).ConfigureAwait(false);
        if (checkTrackPathsResult.IsFailure)
            return checkTrackPathsResult.Errors;

        // Resolve the tags and genres referenced anywhere in the aggregate to a single instance per name, replacing the stored ones where they exist, so that
        // neither a stored row nor a name shared by several albums or tracks of the same request is tracked more than once.
        Result<IReadOnlyDictionary<string, TagEntity>> resolveTagsResult = await SharedReferenceResolver.ResolveAsync(
            _luminaDbContext,
            artist.Tags.Concat(artist.Albums.SelectMany(album => album.Tags)).Concat(artist.Albums.SelectMany(album => album.Tracks.SelectMany(track => track.Tags))),
            Errors.Metadata.TagNameCannotBeEmpty,
            cancellationToken).ConfigureAwait(false);
        if (resolveTagsResult.IsFailure)
            return resolveTagsResult.Errors;

        Result<IReadOnlyDictionary<string, GenreEntity>> resolveGenresResult = await SharedReferenceResolver.ResolveAsync(
            _luminaDbContext,
            artist.Genres.Concat(artist.Albums.SelectMany(album => album.Genres)).Concat(artist.Albums.SelectMany(album => album.Tracks.SelectMany(track => track.Genres))),
            Errors.Metadata.GenreNameCannotBeEmpty,
            cancellationToken).ConfigureAwait(false);
        if (resolveGenresResult.IsFailure)
            return resolveGenresResult.Errors;

        artist.Tags = [.. SharedReferenceResolver.Normalize(artist.Tags, resolveTagsResult.Value)];
        artist.Genres = [.. SharedReferenceResolver.Normalize(artist.Genres, resolveGenresResult.Value)];

        foreach (AlbumEntity album in artist.Albums)
        {
            album.Tags = [.. SharedReferenceResolver.Normalize(album.Tags, resolveTagsResult.Value)];
            album.Genres = [.. SharedReferenceResolver.Normalize(album.Genres, resolveGenresResult.Value)];

            foreach (TrackEntity track in album.Tracks)
            {
                track.Tags = [.. SharedReferenceResolver.Normalize(track.Tags, resolveTagsResult.Value)];
                track.Genres = [.. SharedReferenceResolver.Normalize(track.Genres, resolveGenresResult.Value)];
            }
        }

        _luminaDbContext.Artists.Add(artist);
        return Result.Created;
    }

    /// <summary>
    /// Updates an existing artist, replacing only the editable data that actually changed, while preserving its identity, the identity of its children, and its audit columns.
    /// </summary>
    /// <param name="data">The artist whose editable data is applied to the stored artist.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="Result{TValue}"/> representing either a successful operation, or an error.</returns>
    public async Task<Result<Updated>> UpdateAsync(ArtistEntity data, CancellationToken cancellationToken)
    {
        // The whole aggregate is loaded once, including every collection that the album and track repositories reconcile, so that those repositories can apply the
        // edit to the already tracked children instead of loading each album and each track again.
        ArtistEntity? foundArtist = await _luminaDbContext.Artists
            .Include(repositoryArtist => repositoryArtist.Tags)
            .Include(repositoryArtist => repositoryArtist.Genres)
            .Include(repositoryArtist => repositoryArtist.Contributors)
            .Include(repositoryArtist => repositoryArtist.Albums)
                .ThenInclude(album => album.Tags)
            .Include(repositoryArtist => repositoryArtist.Albums)
                .ThenInclude(album => album.Genres)
            .Include(repositoryArtist => repositoryArtist.Albums)
                .ThenInclude(album => album.Ratings)
            .Include(repositoryArtist => repositoryArtist.Albums)
                .ThenInclude(album => album.Contributors)
            .Include(repositoryArtist => repositoryArtist.Albums)
                .ThenInclude(album => album.Tracks)
                    .ThenInclude(track => track.Tags)
            .Include(repositoryArtist => repositoryArtist.Albums)
                .ThenInclude(album => album.Tracks)
                    .ThenInclude(track => track.Genres)
            .Include(repositoryArtist => repositoryArtist.Albums)
                .ThenInclude(album => album.Tracks)
                    .ThenInclude(track => track.Ratings)
            .Include(repositoryArtist => repositoryArtist.Albums)
                .ThenInclude(album => album.Tracks)
                    .ThenInclude(track => track.Moods)
            .Include(repositoryArtist => repositoryArtist.Albums)
                .ThenInclude(album => album.Tracks)
                    .ThenInclude(track => track.Isrcs)
            .Include(repositoryArtist => repositoryArtist.Albums)
                .ThenInclude(album => album.Tracks)
                    .ThenInclude(track => track.Contributors)
            .AsSplitQuery()
            .FirstOrDefaultAsync(repositoryArtist => repositoryArtist.Id == data.Id, cancellationToken).ConfigureAwait(false);
        if (foundArtist is null)
            return Errors.Music.ArtistNotFound;

        // The stored identity is never overwritten by an edit, and the audit columns are only ever written by the auditing interceptor.
        Guid libraryId = foundArtist.LibraryId;
        EditableValuesCopier.CopyEditableValues(_luminaDbContext, foundArtist, data);
        foundArtist.LibraryId = libraryId;

        // A contributor participation is matched by the contributor and the role they played. Matched participations keep their identity and their audit columns, so a
        // contributor that is displayed or linked elsewhere is never deleted and re-inserted just because another field of the artist was edited.
        CollectionReconciler.Reconcile(
            foundArtist.Contributors,
            data.Contributors,
            existingContributor => (existingContributor.MediaContributorId, existingContributor.Role),
            incomingContributor => (incomingContributor.MediaContributorId, incomingContributor.Role),
            shouldReplace: (existingContributor, incomingContributor) => false,
            createNew: incomingContributor => incomingContributor);

        // The tags and the genres of the artist are shared across the whole database, so the stored rows whose names already exist are reused.
        Result<Updated> reconcileTagsResult = await SharedReferenceResolver.ReconcileAsync(_luminaDbContext, foundArtist.Tags, data.Tags, Errors.Metadata.TagNameCannotBeEmpty, cancellationToken).ConfigureAwait(false);
        if (reconcileTagsResult.IsFailure)
            return reconcileTagsResult.Errors;
        Result<Updated> reconcileGenresResult = await SharedReferenceResolver.ReconcileAsync(_luminaDbContext, foundArtist.Genres, data.Genres, Errors.Metadata.GenreNameCannotBeEmpty, cancellationToken).ConfigureAwait(false);
        if (reconcileGenresResult.IsFailure)
            return reconcileGenresResult.Errors;

        // The ratings, the aliases and the identifiers of the artist are owned value objects with no identity anyone could reference, so a changed value is replaced as a whole.
        CollectionReconciler.Reconcile(
            foundArtist.Ratings,
            data.Ratings,
            existingRating => existingRating.Source?.ToString() ?? string.Empty,
            incomingRating => incomingRating.Source?.ToString() ?? string.Empty,
            shouldReplace: (existingRating, incomingRating) => !existingRating.Equals(incomingRating),
            createNew: incomingRating => incomingRating);

        CollectionReconciler.Reconcile(
            foundArtist.Aliases,
            data.Aliases,
            existingAlias => existingAlias.Name,
            incomingAlias => incomingAlias.Name,
            shouldReplace: (existingAlias, incomingAlias) => !existingAlias.Equals(incomingAlias),
            createNew: incomingAlias => incomingAlias);

        CollectionReconciler.Reconcile(
            foundArtist.Ipis,
            data.Ipis,
            existingIpi => existingIpi.Value,
            incomingIpi => incomingIpi.Value,
            shouldReplace: (existingIpi, incomingIpi) => false,
            createNew: incomingIpi => incomingIpi);

        CollectionReconciler.Reconcile(
            foundArtist.Isnis,
            data.Isnis,
            existingIsni => existingIsni.Value,
            incomingIsni => incomingIsni.Value,
            shouldReplace: (existingIsni, incomingIsni) => false,
            createNew: incomingIsni => incomingIsni);

        // A track is unique within its library by its file system path, so a track that is about to be inserted during this update can never reference a file
        // that is already registered, nor a file that another track of the same update already references. The check runs before anything is written.
        List<string> newTrackPaths = [.. GetNewTrackPaths(foundArtist, data)];
        Result<Success> checkNewTrackPathsResult = await CheckTrackPathsAsync(foundArtist.LibraryId, newTrackPaths, cancellationToken).ConfigureAwait(false);
        if (checkNewTrackPathsResult.IsFailure)
            return checkNewTrackPathsResult.Errors;

        // Normalize the tags and genres carried by the desired aggregate, so that a name shared by several albums or tracks is represented by a single instance,
        // and a name that is already stored is reused, before the album and track repositories reconcile their own collections.
        Result<IReadOnlyDictionary<string, TagEntity>> resolveTagsResult = await SharedReferenceResolver.ResolveAsync(
            _luminaDbContext,
            data.Tags.Concat(data.Albums.SelectMany(album => album.Tags)).Concat(data.Albums.SelectMany(album => album.Tracks.SelectMany(track => track.Tags))),
            Errors.Metadata.TagNameCannotBeEmpty,
            cancellationToken).ConfigureAwait(false);
        if (resolveTagsResult.IsFailure)
            return resolveTagsResult.Errors;
        Result<IReadOnlyDictionary<string, GenreEntity>> resolveGenresResult = await SharedReferenceResolver.ResolveAsync(
            _luminaDbContext,
            data.Genres.Concat(data.Albums.SelectMany(album => album.Genres)).Concat(data.Albums.SelectMany(album => album.Tracks.SelectMany(track => track.Genres))),
            Errors.Metadata.GenreNameCannotBeEmpty,
            cancellationToken).ConfigureAwait(false);
        if (resolveGenresResult.IsFailure)
            return resolveGenresResult.Errors;

        data.Tags = [.. SharedReferenceResolver.Normalize(data.Tags, resolveTagsResult.Value)];
        data.Genres = [.. SharedReferenceResolver.Normalize(data.Genres, resolveGenresResult.Value)];

        foreach (AlbumEntity dataAlbum in data.Albums)
        {
            dataAlbum.Tags = [.. SharedReferenceResolver.Normalize(dataAlbum.Tags, resolveTagsResult.Value)];
            dataAlbum.Genres = [.. SharedReferenceResolver.Normalize(dataAlbum.Genres, resolveGenresResult.Value)];

            foreach (TrackEntity dataTrack in dataAlbum.Tracks)
            {
                dataTrack.Tags = [.. SharedReferenceResolver.Normalize(dataTrack.Tags, resolveTagsResult.Value)];
                dataTrack.Genres = [.. SharedReferenceResolver.Normalize(dataTrack.Genres, resolveGenresResult.Value)];
            }
        }

        // Reconcile the albums by their Id: albums that are no longer present are removed, new albums are inserted, and existing
        // albums and their tracks are updated in place, so that persisted entities keep their identity across an edit. An album whose data did
        // not change produces no write statement, because its own repository only applies the values that actually changed.
        List<Guid> incomingAlbumIds = [.. data.Albums.Select(album => album.Id)];
        foreach (AlbumEntity removedAlbum in foundArtist.Albums.Where(album => !incomingAlbumIds.Contains(album.Id)).ToList())
            foundArtist.Albums.Remove(removedAlbum);

        foreach (AlbumEntity incomingAlbum in data.Albums)
        {
            AlbumEntity? existingAlbum = foundArtist.Albums.FirstOrDefault(album => album.Id == incomingAlbum.Id);
            if (existingAlbum is null)
            {
                Result<Created> insertAlbumResult = await _albumRepository.InsertAsync(incomingAlbum, cancellationToken).ConfigureAwait(false);
                if (insertAlbumResult.IsFailure)
                    return insertAlbumResult.Errors;
                continue;
            }

            Result<Updated> updateAlbumResult = await _albumRepository.ApplyUpdateAsync(existingAlbum, incomingAlbum, cancellationToken).ConfigureAwait(false);
            if (updateAlbumResult.IsFailure)
                return updateAlbumResult.Errors;

            Result<Updated> updateTracksResult = await ReconcileTracksAsync(existingAlbum, incomingAlbum, cancellationToken).ConfigureAwait(false);
            if (updateTracksResult.IsFailure)
                return updateTracksResult.Errors;
        }

        return Result.Updated;
    }

    /// <summary>
    /// Gets the file system paths of the tracks that are not yet stored and will therefore be inserted when <paramref name="data"/> is applied over <paramref name="foundArtist"/>.
    /// </summary>
    /// <param name="foundArtist">The tracked artist that is being updated.</param>
    /// <param name="data">The artist carrying the desired albums and tracks.</param>
    /// <returns>The file system paths of the tracks that are about to be inserted.</returns>
    private static IEnumerable<string> GetNewTrackPaths(ArtistEntity foundArtist, ArtistEntity data)
    {
        foreach (AlbumEntity incomingAlbum in data.Albums)
        {
            AlbumEntity? existingAlbum = foundArtist.Albums.FirstOrDefault(album => album.Id == incomingAlbum.Id);
            if (existingAlbum is null)
            {
                foreach (TrackEntity incomingTrack in incomingAlbum.Tracks)
                    yield return incomingTrack.Path!;
                continue;
            }

            foreach (TrackEntity incomingTrack in incomingAlbum.Tracks)
                if (existingAlbum.Tracks.All(existingTrack => existingTrack.Id != incomingTrack.Id))
                    yield return incomingTrack.Path!;
        }
    }

    /// <summary>
    /// Checks whether any of <paramref name="trackPaths"/> is referenced more than once, or is already used by a track of the library identified by <paramref name="libraryId"/>.
    /// </summary>
    /// <param name="libraryId">The Id of the library whose tracks are searched.</param>
    /// <param name="trackPaths">The file system paths of the tracks that are about to be inserted.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="Result{TValue}"/> representing either a successful check, or an error.</returns>
    private async Task<Result<Success>> CheckTrackPathsAsync(Guid libraryId, IReadOnlyCollection<string> trackPaths, CancellationToken cancellationToken)
    {
        if (trackPaths.Count == 0)
            return Result.Success;

        // The same path cannot be referenced by two tracks of the request itself, not only by a track that is already stored.
        if (trackPaths.Count != trackPaths.Distinct(StringComparer.Ordinal).Count())
            return Errors.Music.TrackAlreadyExists;

        Result<IReadOnlyCollection<string>> getExistingPathsResult = await _trackRepository.GetExistingPathsAsync(libraryId, trackPaths, cancellationToken).ConfigureAwait(false);
        if (getExistingPathsResult.IsFailure)
            return getExistingPathsResult.Errors;
        if (getExistingPathsResult.Value.Count > 0)
            return Errors.Music.TrackAlreadyExists;
        return Result.Success;
    }

    /// <summary>
    /// Reconciles the tracks of an existing album against the provided tracks, by their Id.
    /// </summary>
    /// <param name="existingAlbum">The tracked album whose tracks are reconciled.</param>
    /// <param name="incomingAlbum">The album carrying the tracks to reconcile with.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="Result{TValue}"/> representing either a successful operation, or an error.</returns>
    private async Task<Result<Updated>> ReconcileTracksAsync(AlbumEntity existingAlbum, AlbumEntity incomingAlbum, CancellationToken cancellationToken)
    {
        List<Guid> incomingTrackIds = [.. incomingAlbum.Tracks.Select(track => track.Id)];
        foreach (TrackEntity removedTrack in existingAlbum.Tracks.Where(track => !incomingTrackIds.Contains(track.Id)).ToList())
            existingAlbum.Tracks.Remove(removedTrack);

        foreach (TrackEntity incomingTrack in incomingAlbum.Tracks)
        {
            incomingTrack.AlbumId = existingAlbum.Id;
            incomingTrack.LibraryId = existingAlbum.LibraryId;
            TrackEntity? existingTrack = existingAlbum.Tracks.FirstOrDefault(track => track.Id == incomingTrack.Id);
            if (existingTrack is null)
            {
                Result<Created> insertTrackResult = await _trackRepository.InsertAsync(incomingTrack, cancellationToken).ConfigureAwait(false);
                if (insertTrackResult.IsFailure)
                    return insertTrackResult.Errors;
                continue;
            }

            Result<Updated> updateTrackResult = await _trackRepository.ApplyUpdateAsync(existingTrack, incomingTrack, cancellationToken).ConfigureAwait(false);
            if (updateTrackResult.IsFailure)
                return updateTrackResult.Errors;
        }

        return Result.Updated;
    }

    /// <summary>
    /// Gets an artist by its Id.
    /// </summary>
    /// <param name="id">The Id of the artist to get.</param>
    /// <param name="shouldIncludeNavigationProperties">Whether the navigation properties of the artist should be loaded together with the artist itself.</param>
    /// <param name="shouldTrackEntities">Whether the retrieved artist should be tracked by the persistence medium, so that changes to it can be saved.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="Result{TValue}"/> containing either an <see cref="ArtistEntity"/>, or an error.</returns>
    public async Task<Result<ArtistEntity?>> GetByIdAsync(Guid id, bool shouldIncludeNavigationProperties = true, bool shouldTrackEntities = true, CancellationToken cancellationToken = default)
    {
        IQueryable<ArtistEntity> query = _luminaDbContext.Artists;
        if (!shouldTrackEntities)
            query = query.AsNoTracking();
        if (shouldIncludeNavigationProperties)
        {
            query = query
                .Include(artist => artist.Tags)
                .Include(artist => artist.Genres)
                .Include(artist => artist.Contributors)
                .Include(artist => artist.Albums)
                    .ThenInclude(album => album.Ratings)
                .Include(artist => artist.Albums)
                    .ThenInclude(album => album.Genres)
                .Include(artist => artist.Albums)
                    .ThenInclude(album => album.Tags)
                .Include(artist => artist.Albums)
                    .ThenInclude(album => album.Contributors)
                .Include(artist => artist.Albums)
                    .ThenInclude(album => album.Tracks)
                    .ThenInclude(track => track.Ratings)
                .Include(artist => artist.Albums)
                    .ThenInclude(album => album.Tracks)
                    .ThenInclude(track => track.Genres)
                .Include(artist => artist.Albums)
                    .ThenInclude(album => album.Tracks)
                    .ThenInclude(track => track.Tags)
                .Include(artist => artist.Albums)
                    .ThenInclude(album => album.Tracks)
                    .ThenInclude(track => track.Moods)
                .Include(artist => artist.Albums)
                    .ThenInclude(album => album.Tracks)
                    .ThenInclude(track => track.Isrcs)
                .Include(artist => artist.Albums)
                    .ThenInclude(album => album.Tracks)
                    .ThenInclude(track => track.Contributors)
                .AsSplitQuery();
        }
        return await query.FirstOrDefaultAsync(artist => artist.Id == id, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Deletes the artist identified by <paramref name="id"/>.
    /// </summary>
    /// <param name="id">The Id of the artist to delete.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="Result{TValue}"/> representing either a successful operation, or an error.</returns>
    public async Task<Result<Deleted>> DeleteByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        ArtistEntity? artist = await _luminaDbContext.Artists
            .FirstOrDefaultAsync(repositoryArtist => repositoryArtist.Id == id, cancellationToken).ConfigureAwait(false);
        if (artist is null)
            return Errors.Music.ArtistNotFound;

        _luminaDbContext.Artists.Remove(artist);
        return Result.Deleted;
    }

    /// <summary>
    /// Gets paginated artists, or all the artists of the matching filters when the pagination data is <see langword="null"/>.
    /// </summary>
    /// <typeparam name="TFilter">The type of the filter used for filtering the data.</typeparam>
    /// <param name="paginationData">The pagination data that includes the current page and the number of items per page to retrieve. If <see langword="null"/>, all matching artists are returned.</param>
    /// <param name="sortBy">The name of the field by which to sort the results.</param>
    /// <param name="sortOrder">The direction in which to sort the results.</param>
    /// <param name="filterModel">The model containing the parameters used to filter the results.</param>
    /// <param name="shouldIncludeNavigationProperties">Whether the navigation properties of the entities should be loaded together with the entities themselves. Pass <see langword="false"/> to retrieve only the data stored directly on the entity rows.</param>
    /// <param name="shouldTrackEntities">Whether the retrieved entities should be tracked by the persistence medium, so that changes to them can be saved. Pass <see langword="false"/> for read-only scenarios, to avoid the tracking overhead.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="Result{TValue}"/> containing either a <see cref="PaginatedResultDto{ArtistEntity}"/>, or an error.</returns>
    public async Task<Result<PaginatedResultDto<ArtistEntity>>> GetAllAsync<TFilter>(PaginationDataDto? paginationData = null, string? sortBy = null, SortOrder? sortOrder = null, TFilter? filterModel = null, bool shouldIncludeNavigationProperties = true, bool shouldTrackEntities = true, CancellationToken cancellationToken = default) where TFilter : BaseFilterDto
    {
        IQueryable<ArtistEntity> artistsQuery = _luminaDbContext.Artists;
        if (!shouldTrackEntities)
            artistsQuery = artistsQuery.AsNoTracking();
        if (shouldIncludeNavigationProperties)
        {
            artistsQuery = artistsQuery
                .Include(artist => artist.Tags)
                .Include(artist => artist.Genres)
                .Include(artist => artist.Contributors)
                .Include(artist => artist.Albums)
                    .ThenInclude(album => album.Ratings)
                .Include(artist => artist.Albums)
                    .ThenInclude(album => album.Genres)
                .Include(artist => artist.Albums)
                    .ThenInclude(album => album.Tags)
                .Include(artist => artist.Albums)
                    .ThenInclude(album => album.Contributors)
                .Include(artist => artist.Albums)
                    .ThenInclude(album => album.Tracks)
                    .ThenInclude(track => track.Ratings)
                .Include(artist => artist.Albums)
                    .ThenInclude(album => album.Tracks)
                    .ThenInclude(track => track.Genres)
                .Include(artist => artist.Albums)
                    .ThenInclude(album => album.Tracks)
                    .ThenInclude(track => track.Tags)
                .Include(artist => artist.Albums)
                    .ThenInclude(album => album.Tracks)
                    .ThenInclude(track => track.Moods)
                .Include(artist => artist.Albums)
                    .ThenInclude(album => album.Tracks)
                    .ThenInclude(track => track.Isrcs)
                .Include(artist => artist.Albums)
                    .ThenInclude(album => album.Tracks)
                    .ThenInclude(track => track.Contributors)
                .AsSplitQuery();
        }

        // Artists should always be retrieved only per owning libraries.
        if (filterModel is not LibraryFilterDto libraryFilter || libraryFilter.LibraryId == Guid.Empty)
            return Errors.Library.FilterMustIncludeLibraryId;

        artistsQuery = artistsQuery.Where(artist => artist.LibraryId == libraryFilter.LibraryId);

        FilterSpecification<ArtistEntity>? filterSpecification = BuildFilterSpecification(libraryFilter);
        if (filterSpecification is not null)
            artistsQuery = artistsQuery.Where(filterSpecification.ToExpression());

        IQueryable<ArtistEntity> orderedArtistsQuery = ApplySorting(artistsQuery, sortBy, sortOrder ?? SortOrder.Ascending);

        // If no pagination was requested, return all the artists of the library.
        if (paginationData is null)
        {
            IReadOnlyList<ArtistEntity> allArtists = await orderedArtistsQuery.ToListAsync(cancellationToken).ConfigureAwait(false);
            return new PaginatedResultDto<ArtistEntity>
            {
                Data = allArtists,
                CurrentPage = 1,
                PerPage = allArtists.Count,
                Count = allArtists.Count,
                NumberOfPages = 1
            };
        }

        int count = await orderedArtistsQuery.Select(artist => artist.Id).CountAsync(cancellationToken).ConfigureAwait(false);
        int numberOfPages = (int)Math.Ceiling((double)count / paginationData.PerPage);
        int currentPage = Math.Min(paginationData.CurrentPage, Math.Max(1, numberOfPages));

        // Apply pagination.
        IReadOnlyList<ArtistEntity> paginatedResult = await orderedArtistsQuery
            .Skip((currentPage - 1) * paginationData.PerPage)
            .Take(paginationData.PerPage)
            .ToListAsync(cancellationToken).ConfigureAwait(false);

        return new PaginatedResultDto<ArtistEntity>
        {
            Data = paginatedResult,
            CurrentPage = currentPage,
            PerPage = paginationData.PerPage,
            Count = count,
            NumberOfPages = numberOfPages
        };
    }

    /// <summary>
    /// Gets paginated lightweight read models of the artists of the media library identified by the provided <paramref name="filterModel"/>,
    /// or all of them when the pagination data is <see langword="null"/>, projecting only the fields needed to display the artists in a card-based grid.
    /// </summary>
    /// <typeparam name="TFilter">The type of the filter used for filtering the data.</typeparam>
    /// <param name="paginationData">The pagination data that includes the current page and the number of items per page to retrieve. If <see langword="null"/>, all matching artists are returned.</param>
    /// <param name="sortBy">The name of the field by which to sort the results.</param>
    /// <param name="sortOrder">The direction in which to sort the results.</param>
    /// <param name="filterModel">The model containing the parameters used to filter the results.</param>
    /// <param name="shouldIncludeNavigationProperties">Whether the navigation properties of the entities should be loaded together with the entities themselves. Pass <see langword="false"/> to retrieve only the data stored directly on the entity rows.</param>
    /// <param name="shouldTrackEntities">Whether the retrieved entities should be tracked by the persistence medium, so that changes to them can be saved. Pass <see langword="false"/> for read-only scenarios, to avoid the tracking overhead.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="Result{TValue}"/> containing either a <see cref="PaginatedResultDto{ArtistLiteRow}"/>, or an error.</returns>
    public async Task<Result<PaginatedResultDto<ArtistLiteRow>>> GetAllLiteAsync<TFilter>(PaginationDataDto? paginationData = null, string? sortBy = null, SortOrder? sortOrder = null, TFilter? filterModel = null, bool shouldIncludeNavigationProperties = false, bool shouldTrackEntities = false, CancellationToken cancellationToken = default) where TFilter : BaseFilterDto
    {
        // Artists should always be retrieved only per owning libraries.
        if (filterModel is not LibraryFilterDto libraryFilter || libraryFilter.LibraryId == Guid.Empty)
            return Errors.Library.FilterMustIncludeLibraryId;

        // The lightweight read models are retrieved without tracking by default, because they are never modified by the caller.
        IQueryable<ArtistEntity> artistsQuery = _luminaDbContext.Artists;
        if (!shouldTrackEntities)
            artistsQuery = artistsQuery.AsNoTracking();
        if (shouldIncludeNavigationProperties)
            artistsQuery = artistsQuery
                .Include(artist => artist.Contributors)
                .Include(artist => artist.Albums)
                .AsSplitQuery();

        artistsQuery = artistsQuery.Where(artist => artist.LibraryId == libraryFilter.LibraryId);

        FilterSpecification<ArtistEntity>? filterSpecification = BuildFilterSpecification(libraryFilter);
        if (filterSpecification is not null)
            artistsQuery = artistsQuery.Where(filterSpecification.ToExpression());

        IQueryable<ArtistLiteRow> liteRowsQuery = ApplySorting(artistsQuery, sortBy, sortOrder ?? SortOrder.Ascending)
            .Select(artist => new ArtistLiteRow
            {
                Id = artist.Id,
                Name = artist.Name
            });

        // If no pagination was requested, return all the artists of the library.
        if (paginationData is null)
        {
            IReadOnlyList<ArtistLiteRow> allArtists = await liteRowsQuery.ToListAsync(cancellationToken).ConfigureAwait(false);
            return new PaginatedResultDto<ArtistLiteRow>
            {
                Data = allArtists,
                CurrentPage = 1,
                PerPage = allArtists.Count,
                Count = allArtists.Count,
                NumberOfPages = 1
            };
        }

        int count = await artistsQuery.Select(artist => artist.Id).CountAsync(cancellationToken).ConfigureAwait(false);
        int numberOfPages = (int)Math.Ceiling((double)count / paginationData.PerPage);
        int currentPage = Math.Min(paginationData.CurrentPage, Math.Max(1, numberOfPages));

        // Apply pagination.
        IReadOnlyList<ArtistLiteRow> paginatedResult = await liteRowsQuery
            .Skip((currentPage - 1) * paginationData.PerPage)
            .Take(paginationData.PerPage)
            .ToListAsync(cancellationToken).ConfigureAwait(false);

        return new PaginatedResultDto<ArtistLiteRow>
        {
            Data = paginatedResult,
            CurrentPage = currentPage,
            PerPage = paginationData.PerPage,
            Count = count,
            NumberOfPages = numberOfPages
        };
    }

    /// <summary>
    /// Builds a filter specification for querying artists.
    /// </summary>
    /// <param name="libraryFilter">The model containing the parameters used to filter the results.</param>
    /// <returns>A filter specification that can be used to query artists matching the provided criteria.</returns>
    private static FilterSpecification<ArtistEntity>? BuildFilterSpecification(LibraryFilterDto libraryFilter)
    {
        // Include the search term filter, if provided.
        if (!string.IsNullOrWhiteSpace(libraryFilter.SearchTerm))
            return new ArtistSearchSpecification(libraryFilter.SearchTerm);

        return null;
    }

    /// <summary>
    /// Sorts an artists query by the given field name, defaulting to <see cref="ArtistEntity.Name"/>.
    /// </summary>
    /// <param name="artistsQuery">The query to sort.</param>
    /// <param name="sortBy">The field to sort by (case-insensitive).</param>
    /// <param name="sortOrder">The direction of the sorting.</param>
    /// <returns>An ordered artists query.</returns>
    private static IOrderedQueryable<ArtistEntity> ApplySorting(IQueryable<ArtistEntity> artistsQuery, string? sortBy, SortOrder sortOrder)
    {
        return sortBy?.ToLowerInvariant() switch
        {
            "website" => sortOrder == SortOrder.Descending
                ? artistsQuery.OrderByDescending(artist => artist.Website).ThenBy(artist => artist.Id)
                : artistsQuery.OrderBy(artist => artist.Website).ThenBy(artist => artist.Id),
            "musicbrainzartistid" => sortOrder == SortOrder.Descending
                ? artistsQuery.OrderByDescending(artist => artist.MusicBrainzArtistId).ThenBy(artist => artist.Id)
                : artistsQuery.OrderBy(artist => artist.MusicBrainzArtistId).ThenBy(artist => artist.Id),
            "createdonutc" => sortOrder == SortOrder.Descending
                ? artistsQuery.OrderByDescending(artist => artist.CreatedOnUtc).ThenBy(artist => artist.Id)
                : artistsQuery.OrderBy(artist => artist.CreatedOnUtc).ThenBy(artist => artist.Id),
            "updatedonutc" => sortOrder == SortOrder.Descending
                ? artistsQuery.OrderByDescending(artist => artist.UpdatedOnUtc).ThenBy(artist => artist.Id)
                : artistsQuery.OrderBy(artist => artist.UpdatedOnUtc).ThenBy(artist => artist.Id),
            _ => sortOrder == SortOrder.Descending
                ? artistsQuery.OrderByDescending(artist => artist.Name).ThenBy(artist => artist.Id)
                : artistsQuery.OrderBy(artist => artist.Name).ThenBy(artist => artist.Id),
        };
    }

    /// <summary>
    /// Gets the artist of the library identified by <paramref name="libraryId"/> that has the provided <paramref name="name"/>.
    /// </summary>
    /// <param name="libraryId">The Id of the library whose artist is retrieved.</param>
    /// <param name="name">The name of the artist to retrieve.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="Result{TValue}"/> containing either the <see cref="ArtistEntity"/> with the provided name, or an error.</returns>
    public async Task<Result<ArtistEntity?>> GetByNameAsync(Guid libraryId, string name, CancellationToken cancellationToken)
    {
        ArtistEntity? artist = await _luminaDbContext.Artists
            .FirstOrDefaultAsync(repositoryArtist => repositoryArtist.LibraryId == libraryId && repositoryArtist.Name == name, cancellationToken).ConfigureAwait(false);
        return artist;
    }

    /// <summary>
    /// Gets a page of the artists of the media library identified by <paramref name="libraryId"/> whose metadata has not been enriched yet,
    /// together with their albums and tracks and the collections needed to enrich them, ordered by name, using keyset pagination.
    /// </summary>
    /// <param name="libraryId">The Id of the media library whose artists are retrieved.</param>
    /// <param name="lastName">The name of the last retrieved artist, used for keyset pagination. Pass <see langword="null"/> to get the first page.</param>
    /// <param name="pageSize">The maximum number of artists to retrieve.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="Result{TValue}"/> containing either a page of artists needing their metadata enriched, or an error.</returns>
    public async Task<Result<IReadOnlyList<ArtistEntity>>> GetArtistsNeedingMetadataAsync(Guid libraryId, string? lastName, int pageSize, CancellationToken cancellationToken)
    {
        return await _luminaDbContext.Artists
            .Include(artist => artist.Area)
            .Include(artist => artist.BeginArea)
            .Include(artist => artist.EndArea)
            .Include(artist => artist.Aliases)
            .Include(artist => artist.Ipis)
            .Include(artist => artist.Isnis)
            .Include(artist => artist.Genres)
            .Include(artist => artist.Tags)
            .Include(artist => artist.Ratings)
            .Include(artist => artist.Contributors)
            .Include(artist => artist.Albums)
                .ThenInclude(album => album.ReleaseTypes)
            .Include(artist => artist.Albums)
                .ThenInclude(album => album.Genres)
            .Include(artist => artist.Albums)
                .ThenInclude(album => album.Tags)
            .Include(artist => artist.Albums)
                .ThenInclude(album => album.Ratings)
            .Include(artist => artist.Albums)
                .ThenInclude(album => album.Contributors)
            .Include(artist => artist.Albums)
                .ThenInclude(album => album.Tracks)
                    .ThenInclude(track => track.Genres)
            .Include(artist => artist.Albums)
                .ThenInclude(album => album.Tracks)
                    .ThenInclude(track => track.Tags)
            .Include(artist => artist.Albums)
                .ThenInclude(album => album.Tracks)
                    .ThenInclude(track => track.Ratings)
            .Include(artist => artist.Albums)
                .ThenInclude(album => album.Tracks)
                    .ThenInclude(track => track.Moods)
            .Include(artist => artist.Albums)
                .ThenInclude(album => album.Tracks)
                    .ThenInclude(track => track.Isrcs)
            .Include(artist => artist.Albums)
                .ThenInclude(album => album.Tracks)
                    .ThenInclude(track => track.Contributors)
            .Include(artist => artist.Albums)
                .ThenInclude(album => album.Tracks)
                    .ThenInclude(track => track.WorkLanguages)
            .Include(artist => artist.Albums)
                .ThenInclude(album => album.Tracks)
                    .ThenInclude(track => track.WorkIswcs)
            .AsSplitQuery()
            .Where(artist => artist.LibraryId == libraryId
                        && (artist.MetadataStatus != MetadataStatus.Enriched
                            || artist.Albums.Any(album => album.MetadataStatus != MetadataStatus.Enriched)
                            || artist.Albums.Any(album => album.Tracks.Any(track => track.MetadataStatus != MetadataStatus.Enriched)))
                        && (lastName == null || artist.Name.CompareTo(lastName) > 0))
            .OrderBy(artist => artist.Name)
            .Take(pageSize)
            .ToListAsync(cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Gets the number of artists of the media library identified by <paramref name="libraryId"/> whose metadata has not been enriched yet.
    /// </summary>
    /// <param name="libraryId">The Id of the media library whose artists are counted.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="Result{TValue}"/> containing either the number of artists needing their metadata enriched, or an error.</returns>
    public async Task<Result<int>> GetArtistsNeedingMetadataCountAsync(Guid libraryId, CancellationToken cancellationToken)
    {
        return await _luminaDbContext.Artists
            .CountAsync(artist => artist.LibraryId == libraryId
                        && (artist.MetadataStatus != MetadataStatus.Enriched
                            || artist.Albums.Any(album => album.MetadataStatus != MetadataStatus.Enriched)
                            || artist.Albums.Any(album => album.Tracks.Any(track => track.MetadataStatus != MetadataStatus.Enriched))), cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Gets the number of artists of the media library identified by <paramref name="libraryId"/> whose artwork has not been resolved yet.
    /// </summary>
    /// <param name="libraryId">The Id of the media library whose artists are counted.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="Result{TValue}"/> containing either the number of artists needing their artwork resolved, or an error.</returns>
    public async Task<Result<int>> GetArtistsNeedingArtworkCountAsync(Guid libraryId, CancellationToken cancellationToken)
    {
        return await _luminaDbContext.Artists
            .CountAsync(artist => artist.LibraryId == libraryId
                        && !_luminaDbContext.MusicArtwork.Any(musicArtwork => musicArtwork.OwnerType == MusicArtworkOwnerType.Artist
                            && musicArtwork.OwnerId == artist.Id
                            && (musicArtwork.Status == ArtworkStatus.Enriched || musicArtwork.Status == ArtworkStatus.NotAvailable)), cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Gets a page of the artists of the media library identified by <paramref name="libraryId"/> whose artwork has not been resolved yet,
    /// together with their albums and tracks, ordered by name, using keyset pagination.
    /// </summary>
    /// <param name="libraryId">The Id of the media library whose artists are retrieved.</param>
    /// <param name="lastName">The name of the last retrieved artist, used for keyset pagination. Pass <see langword="null"/> to get the first page.</param>
    /// <param name="pageSize">The maximum number of artists to retrieve.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="Result{TValue}"/> containing either a page of artists needing their artwork resolved, or an error.</returns>
    public async Task<Result<IReadOnlyList<ArtistEntity>>> GetArtistsNeedingArtworkAsync(Guid libraryId, string? lastName, int pageSize, CancellationToken cancellationToken)
    {
        return await _luminaDbContext.Artists
            .AsNoTracking()
            .Include(artist => artist.Albums)
                .ThenInclude(album => album.Tracks)
            .AsSplitQuery()
            .Where(artist => artist.LibraryId == libraryId
                        && !_luminaDbContext.MusicArtwork.Any(musicArtwork => musicArtwork.OwnerType == MusicArtworkOwnerType.Artist
                            && musicArtwork.OwnerId == artist.Id
                            && (musicArtwork.Status == ArtworkStatus.Enriched || musicArtwork.Status == ArtworkStatus.NotAvailable))
                        && (lastName == null || artist.Name.CompareTo(lastName) > 0))
            .OrderBy(artist => artist.Name)
            .Take(pageSize)
            .ToListAsync(cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Resets the metadata enrichment status of all the artists of the media library identified by <paramref name="libraryId"/>.
    /// </summary>
    /// <param name="libraryId">The Id of the media library whose artists are reset.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="Result{TValue}"/> representing either a successful operation, or an error.</returns>
    public async Task<Result<Updated>> ResetMetadataStatusForLibraryAsync(Guid libraryId, CancellationToken cancellationToken)
    {
        await _luminaDbContext.Artists
            .Where(artist => artist.LibraryId == libraryId)
            .ExecuteUpdateAsync(setters => setters.SetProperty(artist => artist.MetadataStatus, MetadataStatus.Pending), cancellationToken).ConfigureAwait(false);
        return Result.Updated;
    }
}
