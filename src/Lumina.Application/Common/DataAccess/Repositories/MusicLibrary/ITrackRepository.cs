#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.MediaLibrary.AudioLibrary.MusicLibrary;
using Lumina.Application.Common.DataAccess.Repositories.Common.Actions;
using Lumina.Application.Common.DataAccess.Repositories.Common.Base;
using Lumina.Application.Common.DTO.MediaLibrary.AudioLibrary.MusicLibrary.Tracks;
using Lumina.Application.Common.DTO.Pagination;
using Lumina.Domain.Common.Primitives;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Application.Common.DataAccess.Repositories.MusicLibrary;

/// <summary>
/// Interface for the repository for tracks.
/// </summary>
public interface ITrackRepository : IRepository<TrackEntity>,
                                    IInsertRepositoryAction<TrackEntity>,
                                    IUpdateRepositoryAction<TrackEntity>,
                                    IApplyUpdateRepositoryAction<TrackEntity>,
                                    IGetByIdRepositoryAction<TrackEntity, Guid>,
                                    IDeleteByIdRepositoryAction<Guid>
{
    /// <summary>
    /// Gets the subset of <paramref name="paths"/> that is already used by a track of the library identified by <paramref name="libraryId"/>.
    /// </summary>
    /// <param name="libraryId">The Id of the library whose tracks are searched.</param>
    /// <param name="paths">The track paths to check.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="Result{TValue}"/> containing either the paths that are already used, or an error.</returns>
    Task<Result<IReadOnlyCollection<string>>> GetExistingPathsAsync(Guid libraryId, IReadOnlyCollection<string> paths, CancellationToken cancellationToken);

    /// <summary>
    /// Gets all the tracks of the album identified by <paramref name="albumId"/>.
    /// </summary>
    /// <param name="albumId">The Id of the album whose tracks are retrieved.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="Result{TValue}"/> containing either a collection of <see cref="TrackEntity"/>, or an error.</returns>
    Task<Result<IReadOnlyList<TrackEntity>>> GetByAlbumIdAsync(Guid albumId, CancellationToken cancellationToken);

    /// <summary>
    /// Gets the lightweight read models of the tracks of the album identified by <paramref name="albumId"/>, projecting only the fields needed to display the tracks of an album.
    /// </summary>
    /// <param name="albumId">The Id of the album whose tracks are retrieved.</param>
    /// <param name="paginationData">The pagination data that includes the current page and the number of items per page to retrieve. If <see langword="null"/>, all matching tracks are returned.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="Result{TValue}"/> containing either a paginated result of <see cref="TrackLiteRow"/>, or an error.</returns>
    Task<Result<PaginatedResultDto<TrackLiteRow>>> GetTracksLiteByAlbumIdAsync(Guid albumId, PaginationDataDto? paginationData, CancellationToken cancellationToken);

    /// <summary>
    /// Gets the track of the library identified by <paramref name="libraryId"/> that is stored at the provided <paramref name="path"/>.
    /// </summary>
    /// <param name="libraryId">The Id of the library whose track is retrieved.</param>
    /// <param name="path">The file system path of the track to retrieve.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="Result{TValue}"/> containing either the <see cref="TrackEntity"/> stored at the provided path, or an error.</returns>
    Task<Result<TrackEntity?>> GetByPathAsync(Guid libraryId, string path, CancellationToken cancellationToken);

    /// <summary>
    /// Resets the enrichment state of the tracks stored at the provided <paramref name="paths"/> in the media library identified by
    /// <paramref name="libraryId"/>, together with their albums and artists, so that they are re-enriched, because their content changed since the last scan.
    /// </summary>
    /// <param name="libraryId">The Id of the media library whose tracks are reset.</param>
    /// <param name="paths">The file system paths of the tracks whose enrichment state is reset.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="Result{TValue}"/> representing either a successful operation, or an error.</returns>
    Task<Result<Updated>> ResetEnrichmentStateForPathsAsync(Guid libraryId, IReadOnlyCollection<string> paths, CancellationToken cancellationToken);

    /// <summary>
    /// Gets the number of tracks of the media library identified by <paramref name="libraryId"/> whose metadata has not been enriched yet.
    /// </summary>
    /// <param name="libraryId">The Id of the media library whose tracks are counted.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="Result{TValue}"/> containing either the number of tracks needing their metadata enriched, or an error.</returns>
    Task<Result<int>> GetTracksNeedingMetadataCountAsync(Guid libraryId, CancellationToken cancellationToken);

    /// <summary>
    /// Resets the metadata enrichment status of all the tracks of the media library identified by <paramref name="libraryId"/>,
    /// so that they are re-enriched, because the metadata provider configuration of the library changed.
    /// </summary>
    /// <param name="libraryId">The Id of the media library whose tracks are reset.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="Result{TValue}"/> representing either a successful operation, or an error.</returns>
    Task<Result<Updated>> ResetMetadataStatusForLibraryAsync(Guid libraryId, CancellationToken cancellationToken);
}
