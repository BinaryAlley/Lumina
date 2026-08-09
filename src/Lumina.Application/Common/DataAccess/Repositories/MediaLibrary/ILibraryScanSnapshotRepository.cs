#region ========================================================================= USING =====================================================================================
using ErrorOr;
using Lumina.Application.Common.DataAccess.Entities.MediaLibrary.Management;
using Lumina.Application.Common.DataAccess.Repositories.Common.Base;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Application.Common.DataAccess.Repositories.MediaLibrary;

/// <summary>
/// Interface for the repository for media library scan snapshots.
/// </summary>
public interface ILibraryScanSnapshotRepository : IRepository<LibraryScanSnapshotEntity>
{
    /// <summary>
    /// Gets the paths of the media library scan snapshot items that are no longer present in the current scan, meaning they were deleted from the storage medium.
    /// </summary>
    /// <param name="libraryId">The unique identifier of the library for which to get the deleted media library scan snapshot item paths.</param>
    /// <param name="scanId">The unique identifier of the media library scan for which to determine the deleted media library scan snapshot item paths.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="ErrorOr{TValue}"/> containing either a collection of deleted media library scan snapshot item paths, or an error.</returns>
    Task<ErrorOr<IReadOnlyList<string>>> GetDeletedPathsAsync(Guid libraryId, Guid scanId, CancellationToken cancellationToken);

    /// <summary>
    /// Atomically applies the results of the current scan to the storage medium, by replacing the media library scan snapshot of the previous scan with the snapshot of the current scan.
    /// The operation updates the changed media library scan snapshot items, deletes the ones that are no longer present on disk, adds audit entries for all changed, new and deleted items,
    /// and clears the staging results of the current scan.
    /// </summary>
    /// <param name="libraryId">The unique identifier of the library for which to apply the scan results.</param>
    /// <param name="scanId">The unique identifier of the media library scan whose results are applied.</param>
    /// <param name="userId">The unique identifier of the user that initiated the media library scan, used for audit purposes.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="ErrorOr{TValue}"/> representing either a successful operation, or an error.</returns>
    Task<ErrorOr<Updated>> ApplySnapshotSwapAsync(Guid libraryId, Guid scanId, Guid userId, CancellationToken cancellationToken);
}
