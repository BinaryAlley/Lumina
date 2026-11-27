#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.Favorites;
using Lumina.Application.Common.DataAccess.Repositories.Common.Actions;
using Lumina.Application.Common.DataAccess.Repositories.Common.Base;
using Lumina.Application.Common.DTO.Favorites;
using Lumina.Application.Common.DTO.Pagination;
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.SharedKernel.Common.Enums.Favorites;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Application.Common.DataAccess.Repositories.Favorites;

/// <summary>
/// Interface for the repository for favorites.
/// </summary>
public interface IFavoriteRepository : IRepository<FavoriteEntity>,
                                       IInsertRepositoryAction<FavoriteEntity>,
                                       IDeleteByIdRepositoryAction<Guid>
{
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
    Task<Result<FavoriteEntity?>> GetByMediaItemIdForUserAsync(Guid userId, FavoriteMediaKind mediaKind, Guid mediaItemId, bool shouldTrackEntities = true, CancellationToken cancellationToken = default);

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
    Task<Result<Guid?>> GetMediaItemLibraryIdAsync(FavoriteMediaKind mediaKind, Guid mediaItemId, CancellationToken cancellationToken);

    /// <summary>
    /// Gets the paginated favorites of the user identified by <paramref name="userId"/>, together with the display data of the media items they reference,
    /// or all the matching favorites when the pagination data is <see langword="null"/>.
    /// </summary>
    /// <param name="userId">The Id of the user whose favorites are retrieved.</param>
    /// <param name="mediaKind">The kind of media item to restrict the favorites to, or <see langword="null"/> for every kind.</param>
    /// <param name="paginationData">The pagination data that includes the current page and the number of items per page to retrieve. If <see langword="null"/>, all matching favorites are returned.</param>
    /// <param name="shouldTrackEntities">Whether the retrieved entities should be tracked by the persistence medium, so that changes to them can be saved. Pass <see langword="false"/> for read-only scenarios, to avoid the tracking overhead.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="Result{TValue}"/> containing either a <see cref="PaginatedResultDto{FavoriteItemDto}"/>, or an error.</returns>
    Task<Result<PaginatedResultDto<FavoriteItemDto>>> GetAllForUserAsync(Guid userId, FavoriteMediaKind? mediaKind, PaginationDataDto? paginationData = null, bool shouldTrackEntities = false, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the Ids, among <paramref name="mediaItemIds"/>, of the media items of kind <paramref name="mediaKind"/> that the user identified by <paramref name="userId"/> has favorited.
    /// </summary>
    /// <param name="userId">The Id of the user whose favorites are checked.</param>
    /// <param name="mediaKind">The kind of the media items.</param>
    /// <param name="mediaItemIds">The Ids of the media items to check.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="Result{TValue}"/> containing either the subset of the Ids that are favorites of the user, or an error.</returns>
    Task<Result<IReadOnlyList<Guid>>> GetFavoritedMediaItemIdsForUserAsync(Guid userId, FavoriteMediaKind mediaKind, IReadOnlyCollection<Guid> mediaItemIds, CancellationToken cancellationToken);
}
