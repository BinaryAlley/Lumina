#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.Playlists;
using Lumina.Application.Common.DataAccess.Repositories.Common.Actions;
using Lumina.Application.Common.DataAccess.Repositories.Common.Base;
using Lumina.Application.Common.DTO.Playlists;
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.SharedKernel.Common.Enums.Playlists;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Application.Common.DataAccess.Repositories.Playlists;

/// <summary>
/// Interface for the repository for playlists.
/// </summary>
public interface IPlaylistRepository : IRepository<PlaylistEntity>,
                                       IInsertRepositoryAction<PlaylistEntity>,
                                       IUpdateRepositoryAction<PlaylistEntity>,
                                       IDeleteByIdRepositoryAction<Guid>
{
    /// <summary>
    /// Gets the playlist identified by <paramref name="id"/> when it belongs to the user identified by <paramref name="userId"/>, if it exists.
    /// </summary>
    /// <param name="id">The Id of the playlist to get.</param>
    /// <param name="userId">The Id of the user the playlist must belong to.</param>
    /// <param name="shouldIncludeNavigationProperties">Whether the entries of the playlist should be loaded together with the playlist itself.</param>
    /// <param name="shouldTrackEntities">Whether the retrieved entities should be tracked by the persistence medium, so that changes to them can be saved.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>
    /// An <see cref="Result{TValue}"/> containing either the playlist identified by <paramref name="id"/> and owned by <paramref name="userId"/>,
    /// <see langword="null"/> when no such playlist exists, or an error.
    /// </returns>
    Task<Result<PlaylistEntity?>> GetByIdForUserAsync(Guid id, Guid userId, bool shouldIncludeNavigationProperties = true, bool shouldTrackEntities = true, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the playlists of the user identified by <paramref name="userId"/>, ordered by their most recent activity.
    /// </summary>
    /// <param name="userId">The Id of the user whose playlists are retrieved.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="Result{TValue}"/> containing either the playlists of the user, or an error.</returns>
    Task<Result<IReadOnlyList<PlaylistSummaryDto>>> GetAllForUserAsync(Guid userId, CancellationToken cancellationToken);

    /// <summary>
    /// Gets the playlist identified by <paramref name="id"/> together with the display details of the media items its entries reference,
    /// when the playlist belongs to the user identified by <paramref name="userId"/>.
    /// </summary>
    /// <param name="id">The Id of the playlist to get.</param>
    /// <param name="userId">The Id of the user the playlist must belong to.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>
    /// An <see cref="Result{TValue}"/> containing either the playlist details, <see langword="null"/> when no such playlist exists, or an error.
    /// </returns>
    Task<Result<PlaylistDetailsDto?>> GetDetailsForUserAsync(Guid id, Guid userId, int offset, int limit, CancellationToken cancellationToken);

    /// <summary>
    /// Appends a batch of media items of one kind to the end of the ordered entries of a playlist of the user identified by
    /// <paramref name="userId"/>, using batched bulk inserts, and updates the activity time of the playlist in the same transaction.
    /// </summary>
    /// <param name="playlistId">The Id of the playlist to append to.</param>
    /// <param name="userId">The Id of the user the playlist must belong to.</param>
    /// <param name="mediaItemIds">The Ids of the media items to append, in order. Duplicates are kept.</param>
    /// <param name="mediaItemType">The kind of media item referenced by every appended entry.</param>
    /// <param name="entryIds">The Ids to assign to the appended entries, in the same order as <paramref name="mediaItemIds"/>, or <see langword="null"/> to generate them.</param>
    /// <param name="updatedOnUtc">The activity time to store on the playlist.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>
    /// An <see cref="Result{TValue}"/> containing either the sparse order key of the first appended entry and the added count, or an error.
    /// </returns>
    Task<Result<PlaylistBulkAddResultDto>> AddEntriesAsync(Guid playlistId, Guid userId, IReadOnlyList<Guid> mediaItemIds, PlaylistMediaItemType mediaItemType, IReadOnlyList<Guid>? entryIds, DateTime updatedOnUtc, CancellationToken cancellationToken);

    /// <summary>
    /// Appends every media item of a library, an artist or an album to the end of the ordered entries of a playlist of the user identified by
    /// <paramref name="userId"/>, in catalogue order, without materializing the scope.
    /// </summary>
    /// <param name="playlistId">The Id of the playlist to append to.</param>
    /// <param name="userId">The Id of the user the playlist must belong to.</param>
    /// <param name="libraryId">The Id of the library whose media items are appended.</param>
    /// <param name="artistId">The Id of the artist the scope is limited to, or <see langword="null"/> for every artist of the library.</param>
    /// <param name="albumId">The Id of the album the scope is limited to, or <see langword="null"/> for every album of the scope.</param>
    /// <param name="mediaItemType">The kind of media item referenced by every appended entry.</param>
    /// <param name="updatedOnUtc">The activity time to store on the playlist.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>
    /// An <see cref="Result{TValue}"/> containing either the sparse order key of the first appended entry and the added count, or an error.
    /// </returns>
    Task<Result<PlaylistBulkAddResultDto>> AddScopeEntriesAsync(Guid playlistId, Guid userId, Guid libraryId, Guid? artistId, Guid? albumId, PlaylistMediaItemType mediaItemType, DateTime updatedOnUtc, CancellationToken cancellationToken);

    /// <summary>
    /// Moves one entry of a playlist of the user identified by <paramref name="userId"/> to a new dense position by changing only its
    /// sparse order key, and updates the activity time of the playlist in the same transaction.
    /// </summary>
    /// <param name="playlistId">The Id of the playlist whose entry is moved.</param>
    /// <param name="userId">The Id of the user the playlist must belong to.</param>
    /// <param name="entryId">The Id of the entry to move.</param>
    /// <param name="targetPosition">The new zero based position of the entry.</param>
    /// <param name="updatedOnUtc">The activity time to store on the playlist.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="Result{TValue}"/> representing either a successful move, or an error.</returns>
    Task<Result<Updated>> MoveEntryAsync(Guid playlistId, Guid userId, Guid entryId, int targetPosition, DateTime updatedOnUtc, CancellationToken cancellationToken);

    /// <summary>
    /// Removes one entry of a playlist of the user identified by <paramref name="userId"/> by deleting only that row, and updates the
    /// activity time of the playlist in the same transaction.
    /// </summary>
    /// <param name="playlistId">The Id of the playlist whose entry is removed.</param>
    /// <param name="userId">The Id of the user the playlist must belong to.</param>
    /// <param name="entryId">The Id of the entry to remove.</param>
    /// <param name="updatedOnUtc">The activity time to store on the playlist.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>
    /// An <see cref="Result{TValue}"/> containing either zero, because sparse positions need no repositioning, or an error.
    /// </returns>
    Task<Result<int>> RemoveEntryAsync(Guid playlistId, Guid userId, Guid entryId, DateTime updatedOnUtc, CancellationToken cancellationToken);

    /// <summary>
    /// Gets the entry that was just appended to a playlist of the user identified by <paramref name="userId"/>, together with the display details of the track it references.
    /// </summary>
    /// <param name="playlistId">The Id of the playlist the entry must belong to.</param>
    /// <param name="userId">The Id of the user the playlist must belong to.</param>
    /// <param name="entryId">The Id of the appended entry to get.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>
    /// An <see cref="Result{TValue}"/> containing either the entry details, <see langword="null"/> when the entry does not exist in that playlist of the user, or an error.
    /// </returns>
    Task<Result<PlaylistEntryDto?>> GetAppendedEntryDetailsForUserAsync(Guid playlistId, Guid userId, Guid entryId, CancellationToken cancellationToken);
}
