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
                                    IGetByIdRepositoryAction<TrackEntity, Guid>
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
}
