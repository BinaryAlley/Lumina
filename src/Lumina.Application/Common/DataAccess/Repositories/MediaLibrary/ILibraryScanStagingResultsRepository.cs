#region ========================================================================= USING =====================================================================================
using Lumina.Domain.Common.Primitives;
using Lumina.Application.Common.DataAccess.Entities.MediaLibrary.Management;
using Lumina.Application.Common.DataAccess.Repositories.Common.Actions;
using Lumina.Application.Common.DataAccess.Repositories.Common.Base;
using Lumina.Application.Common.Infrastructure.Models.DTO.MediaLibraryScanJobPayloads;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Application.Common.DataAccess.Repositories.MediaLibrary;

/// <summary>
/// Interface for the repository for media library scan staging results.
/// </summary>
public interface ILibraryScanStagingResultsRepository : IRepository<LibraryScanStagingResultsEntity>,
                                                        IInsertRangeRepositoryAction<LibraryScanStagingResultsEntity>
{
    /// <summary>
    /// Marks the media library scan staging results of the current scan by comparing them against the media library scan snapshot of a previous scan, determining which file system items
    /// are new, which ones changed and need their content hashed, and which ones are unchanged.
    /// </summary>
    /// <param name="scanId">The unique identifier of the media library scan whose staging results are marked.</param>
    /// <param name="libraryId">The unique identifier of the library whose media library scan snapshot is compared against.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="Result{TValue}"/> representing either a successful operation, or an error.</returns>
    Task<Result<Updated>> MarkChangesAgainstSnapshotAsync(Guid scanId, Guid libraryId, CancellationToken cancellationToken);

    /// <summary>
    /// Gets the number of media library scan staging results of the current scan that need their content hashed.
    /// </summary>
    /// <param name="scanId">The unique identifier of the media library scan whose staging results are counted.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="Result{TValue}"/> containing either the number of staging results that need hashing, or an error.</returns>
    Task<Result<int>> GetFilesToHashCountAsync(Guid scanId, CancellationToken cancellationToken);

    /// <summary>
    /// Gets a page of the media library scan staging results that need their content hashed, ordered by path, using keyset pagination.
    /// </summary>
    /// <param name="scanId">The unique identifier of the media library scan whose staging results are retrieved.</param>
    /// <param name="lastPath">The path of the last retrieved file system item, used for keyset pagination. Pass <see langword="null"/> to get the first page.</param>
    /// <param name="pageSize">The maximum number of staging results to retrieve.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="Result{TValue}"/> containing either a page of staging results that need hashing, or an error.</returns>
    Task<Result<IReadOnlyList<HashedFileSystemFileDto>>> GetFilesToHashPageAsync(Guid scanId, string? lastPath, int pageSize, CancellationToken cancellationToken);

    /// <summary>
    /// Gets the paths of the media library scan staging results of the current scan that changed, meaning their content needs to be re-hashed
    /// and the books stored at those paths need to be re-enriched, excluding the staging results that are new.
    /// </summary>
    /// <param name="scanId">The unique identifier of the media library scan whose staging results are retrieved.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="Result{TValue}"/> containing either the paths of the changed staging results, or an error.</returns>
    Task<Result<IReadOnlyList<string>>> GetChangedPathsAsync(Guid scanId, CancellationToken cancellationToken);

    /// <summary>
    /// Updates the content hashes of the provided media library scan staging results.
    /// </summary>
    /// <param name="scanId">The unique identifier of the media library scan whose staging results are updated.</param>
    /// <param name="hashedFiles">The file system items whose content hashes are updated.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="Result{TValue}"/> representing either a successful operation, or an error.</returns>
    Task<Result<Updated>> UpdateFileHashesAsync(Guid scanId, IReadOnlyCollection<HashedFileSystemFileDto> hashedFiles, CancellationToken cancellationToken);

    /// <summary>
    /// Clears all the media library scan staging results of the provided media library scan from the storage medium.
    /// </summary>
    /// <param name="scanId">The unique identifier of the media library scan whose staging results are cleared.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="Result{TValue}"/> representing either a successful operation, or an error.</returns>
    Task<Result<Success>> ClearForScanAsync(Guid scanId, CancellationToken cancellationToken);
}
