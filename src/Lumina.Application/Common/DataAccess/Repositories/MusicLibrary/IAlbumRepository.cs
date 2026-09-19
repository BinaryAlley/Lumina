#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.MediaLibrary.AudioLibrary.MusicLibrary;
using Lumina.Application.Common.DataAccess.Repositories.Common.Actions;
using Lumina.Application.Common.DataAccess.Repositories.Common.Base;
using Lumina.Application.Common.DTO.MediaLibrary.AudioLibrary.MusicLibrary.Albums;
using Lumina.Application.Common.DTO.Pagination;
using Lumina.Domain.Common.Primitives;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Application.Common.DataAccess.Repositories.MusicLibrary;

/// <summary>
/// Interface for the repository for albums.
/// </summary>
public interface IAlbumRepository : IRepository<AlbumEntity>,
                                    IInsertRepositoryAction<AlbumEntity>,
                                    IUpdateRepositoryAction<AlbumEntity>,
                                    IGetByIdRepositoryAction<AlbumEntity, Guid>
{
    /// <summary>
    /// Gets all the albums of the artist identified by <paramref name="artistId"/>.
    /// </summary>
    /// <param name="artistId">The Id of the artist whose albums are retrieved.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="Result{TValue}"/> containing either a collection of <see cref="AlbumEntity"/>, or an error.</returns>
    Task<Result<IReadOnlyList<AlbumEntity>>> GetByArtistIdAsync(Guid artistId, CancellationToken cancellationToken);

    /// <summary>
    /// Gets the lightweight read models of the albums of the artist identified by <paramref name="artistId"/>, projecting only the fields needed to display the albums in a card-based grid or a list.
    /// </summary>
    /// <param name="artistId">The Id of the artist whose albums are retrieved.</param>
    /// <param name="paginationData">The pagination data that includes the current page and the number of items per page to retrieve. If <see langword="null"/>, all matching albums are returned.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="Result{TValue}"/> containing either a paginated result of <see cref="AlbumLiteRow"/>, or an error.</returns>
    Task<Result<PaginatedResultDto<AlbumLiteRow>>> GetAlbumsLiteByArtistIdAsync(Guid artistId, PaginationDataDto? paginationData, CancellationToken cancellationToken);
}
