#region ========================================================================= USING =====================================================================================
using ErrorOr;
using Lumina.Application.Common.DataAccess.Entities.MediaLibrary.Management;
using Lumina.Application.Common.DataAccess.Repositories.Common.Actions;
using Lumina.Application.Common.DataAccess.Repositories.Common.Base;
using Lumina.Domain.Common.Enums.MediaLibrary;
using Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryScanAggregate;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Application.Common.DataAccess.Repositories.MediaLibrary;

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
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="ErrorOr{TValue}"/> containing either a collection of <see cref="LibraryScanEntity"/>, or an error.</returns>
    Task<ErrorOr<IEnumerable<LibraryScanEntity>>> GetPastMonthScansByLibraryIdAsync(Guid libraryId, CancellationToken cancellationToken);

    /// <summary>
    /// Gets the media library scans have a <see cref="LibraryScanJobStatus.Running"/> status, from the storage medium.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="ErrorOr{TValue}"/> containing either a collection of <see cref="LibraryScanEntity"/>, or an error.</returns>
    Task<ErrorOr<IEnumerable<LibraryScanEntity>>> GetRunningScansAsync(CancellationToken cancellationToken);
}
