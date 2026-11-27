#region ========================================================================= USING =====================================================================================
using Nemera.Domain.Common.Primitives;
using Nemera.Application.Common.DataAccess.Entities.MediaLibrary.Management;
using Nemera.Application.Common.DataAccess.Repositories.Common.Actions;
using Nemera.Application.Common.DataAccess.Repositories.Common.Base;
using Nemera.Domain.SharedKernel.Common.Enums.MediaLibrary;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Nemera.Application.Common.DataAccess.Repositories.MediaLibrary;

/// <summary>
/// Interface for the repository for media library scans.
/// </summary>
public interface ILibraryScanRepository : IRepository<LibraryScanEntity>,
                                          IGetByIdRepositoryAction<LibraryScanEntity, Guid>,
                                          IInsertRepositoryAction<LibraryScanEntity>,
                                          IUpdateRepositoryAction<LibraryScanEntity>
{
    /// <summary>
    /// Gets the media library scans that belong to a media library identified by <paramref name="libraryId"/>, for the previous month, from the storage medium.
    /// </summary>
    /// <param name="libraryId">The unique identifier of the media library whose scans are retrieved.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="Result{TValue}"/> containing either a collection of <see cref="LibraryScanEntity"/>, or an error.</returns>
    Task<Result<IEnumerable<LibraryScanEntity>>> GetPastMonthScansByLibraryIdAsync(Guid libraryId, CancellationToken cancellationToken);

    /// <summary>
    /// Gets the media library scans that have a <see cref="LibraryScanJobStatus.Pending"/> or a <see cref="LibraryScanJobStatus.Running"/> status, from the storage medium.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="Result{TValue}"/> containing either a collection of <see cref="LibraryScanEntity"/>, or an error.</returns>
    Task<Result<IEnumerable<LibraryScanEntity>>> GetActiveScansAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Marks the media library scans that were interrupted by an application restart, meaning the ones that are still
    /// <see cref="LibraryScanJobStatus.Pending"/> or <see cref="LibraryScanJobStatus.Running"/>, as failed.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="Result{TValue}"/> representing either a successful operation, or an error.</returns>
    Task<Result<Updated>> FailInterruptedScansAsync(CancellationToken cancellationToken);
}
