#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.MediaLibrary.AudioLibrary.MusicLibrary;
using Lumina.Application.Common.DataAccess.Repositories.Common.Actions;
using Lumina.Application.Common.DataAccess.Repositories.Common.Base;
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.SharedKernel.Common.Enums.AudioLibrary;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Application.Common.DataAccess.Repositories.MusicLibrary;

/// <summary>
/// Interface for the repository for the artwork of the music library items.
/// </summary>
public interface IMusicArtworkRepository : IRepository<MusicArtworkEntity>,
                                           IInsertRangeRepositoryAction<MusicArtworkEntity>
{
    /// <summary>
    /// Gets the pieces of artwork owned by the music library item of the provided <paramref name="ownerType"/> identified by <paramref name="ownerId"/>.
    /// </summary>
    /// <param name="ownerType">The type of the music library item that owns the artwork.</param>
    /// <param name="ownerId">The Id of the music library item that owns the artwork.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="Result{TValue}"/> containing either a collection of <see cref="MusicArtworkEntity"/>, or an error.</returns>
    Task<Result<IReadOnlyList<MusicArtworkEntity>>> GetByOwnerAsync(MusicArtworkOwnerType ownerType, Guid ownerId, CancellationToken cancellationToken);

    /// <summary>
    /// Gets the pieces of artwork owned by the music library items of the provided <paramref name="ownerType"/> identified by any of <paramref name="ownerIds"/>.
    /// </summary>
    /// <param name="ownerType">The type of the music library items that own the artwork.</param>
    /// <param name="ownerIds">The Ids of the music library items that own the artwork.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="Result{TValue}"/> containing either the artwork of the existing owners, or an error.</returns>
    Task<Result<IReadOnlyList<MusicArtworkEntity>>> GetByOwnersAsync(MusicArtworkOwnerType ownerType, IReadOnlyCollection<Guid> ownerIds, CancellationToken cancellationToken);

    /// <summary>
    /// Deletes all the pieces of artwork owned by the music library item of the provided <paramref name="ownerType"/> identified by <paramref name="ownerId"/>.
    /// </summary>
    /// <param name="ownerType">The type of the music library item that owns the artwork.</param>
    /// <param name="ownerId">The Id of the music library item that owns the artwork.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="Result{TValue}"/> representing either a successful operation, or an error.</returns>
    Task<Result<Deleted>> DeleteByOwnerAsync(MusicArtworkOwnerType ownerType, Guid ownerId, CancellationToken cancellationToken);

    /// <summary>
    /// Resets the artwork enrichment status of all the pieces of artwork owned by the music library items of the media library identified by <paramref name="libraryId"/>, so that they are re-resolved.
    /// </summary>
    /// <remarks>
    /// The status is reset because the artwork provider configuration of the library changed.
    /// </remarks>
    /// <param name="libraryId">The Id of the media library whose artwork is reset.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="Result{TValue}"/> representing either a successful operation, or an error.</returns>
    Task<Result<Updated>> ResetStatusForLibraryAsync(Guid libraryId, CancellationToken cancellationToken);

    /// <summary>
    /// Deletes all the pieces of artwork owned by the artists, albums and tracks of the media library identified by <paramref name="libraryId"/>.
    /// </summary>
    /// <param name="libraryId">The Id of the media library whose artwork is deleted.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="Result{TValue}"/> representing either a successful operation, or an error.</returns>
    Task<Result<Deleted>> DeleteByLibraryIdAsync(Guid libraryId, CancellationToken cancellationToken);

    /// <summary>
    /// Stores the cover image of the music library item of the provided <paramref name="ownerType"/> identified by <paramref name="ownerId"/>,
    /// replacing the file of its existing cover, or adding a new cover artwork when it has none.
    /// </summary>
    /// <param name="ownerType">The type of the music library item that owns the artwork.</param>
    /// <param name="ownerId">The Id of the music library item that owns the artwork.</param>
    /// <param name="fileName">The relative file name of the stored cover image.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="Result{TValue}"/> representing either a successful operation, or an error.</returns>
    Task<Result<Updated>> UpsertCoverAsync(MusicArtworkOwnerType ownerType, Guid ownerId, string fileName, CancellationToken cancellationToken);
}
