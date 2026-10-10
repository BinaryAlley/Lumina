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
                                    IApplyUpdateRepositoryAction<AlbumEntity>,
                                    IGetByIdRepositoryAction<AlbumEntity, Guid>,
                                    IDeleteByIdRepositoryAction<Guid>
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

    /// <summary>
    /// Gets the album of the artist identified by <paramref name="artistId"/> that has the provided <paramref name="title"/>.
    /// </summary>
    /// <param name="artistId">The Id of the artist whose album is retrieved.</param>
    /// <param name="title">The title of the album to retrieve.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="Result{TValue}"/> containing either the <see cref="AlbumEntity"/> with the provided title, or an error.</returns>
    Task<Result<AlbumEntity?>> GetByTitleAsync(Guid artistId, string title, CancellationToken cancellationToken);

    /// <summary>
    /// Gets the album of the artist identified by <paramref name="artistId"/> that owns a track stored in the provided <paramref name="directoryPath"/>.
    /// </summary>
    /// <param name="artistId">The Id of the artist whose album is retrieved.</param>
    /// <param name="directoryPath">The file system path of the album directory the album owns.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="Result{TValue}"/> containing either the <see cref="AlbumEntity"/> owning the directory, or an error.</returns>
    Task<Result<AlbumEntity?>> GetByTrackDirectoryAsync(Guid artistId, string directoryPath, CancellationToken cancellationToken);

    /// <summary>
    /// Gets the number of albums of the media library identified by <paramref name="libraryId"/> whose metadata has not been enriched yet.
    /// </summary>
    /// <param name="libraryId">The Id of the media library whose albums are counted.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="Result{TValue}"/> containing either the number of albums needing their metadata enriched, or an error.</returns>
    Task<Result<int>> GetAlbumsNeedingMetadataCountAsync(Guid libraryId, CancellationToken cancellationToken);

    /// <summary>
    /// Gets the number of albums of the media library identified by <paramref name="libraryId"/> whose artwork has not been resolved yet,
    /// meaning they have no artwork that is either enriched or confirmed as unavailable.
    /// </summary>
    /// <param name="libraryId">The Id of the media library whose albums are counted.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="Result{TValue}"/> containing either the number of albums needing their artwork resolved, or an error.</returns>
    Task<Result<int>> GetAlbumsNeedingArtworkCountAsync(Guid libraryId, CancellationToken cancellationToken);

    /// <summary>
    /// Gets a page of the albums of the media library identified by <paramref name="libraryId"/> whose artwork has not been resolved yet,
    /// together with their artist, ordered by Id, excluding the provided albums.
    /// </summary>
    /// <param name="libraryId">The Id of the media library whose albums are retrieved.</param>
    /// <param name="excludedAlbumIds">The Ids of the albums that were already processed in the current run and must not be retrieved again.</param>
    /// <param name="pageSize">The maximum number of albums to retrieve.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="Result{TValue}"/> containing either a page of albums needing their artwork resolved, or an error.</returns>
    Task<Result<IReadOnlyList<AlbumEntity>>> GetAlbumsNeedingArtworkAsync(Guid libraryId, IReadOnlyCollection<Guid> excludedAlbumIds, int pageSize, CancellationToken cancellationToken);

    /// <summary>
    /// Gets the file system path of one track of each of the albums identified by <paramref name="albumIds"/>, keyed by the Id of the album.
    /// </summary>
    /// <param name="albumIds">The Ids of the albums whose track path is retrieved.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="Result{TValue}"/> containing either the file system path of a track of each album, or an error.</returns>
    Task<Result<IReadOnlyDictionary<Guid, string>>> GetFirstTrackPathsByAlbumIdsAsync(IReadOnlyCollection<Guid> albumIds, CancellationToken cancellationToken);

    /// <summary>
    /// Resets the metadata enrichment status of all the albums of the media library identified by <paramref name="libraryId"/>,
    /// so that they are re-enriched, because the metadata provider configuration of the library changed.
    /// </summary>
    /// <param name="libraryId">The Id of the media library whose albums are reset.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="Result{TValue}"/> representing either a successful operation, or an error.</returns>
    Task<Result<Updated>> ResetMetadataStatusForLibraryAsync(Guid libraryId, CancellationToken cancellationToken);
}
