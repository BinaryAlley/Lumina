#region ========================================================================= USING =====================================================================================
using ErrorOr;
using Lumina.Application.Common.DataAccess.Entities.MediaLibrary.Management;
using Lumina.Application.Common.DataAccess.Repositories.Common.Actions;
using Lumina.Application.Common.DataAccess.Repositories.Common.Base;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Application.Common.DataAccess.Repositories.MediaLibrary;

/// <summary>
/// Interface for the repository for media library scan results.
/// </summary>
public interface ILibraryScanResultRepository : IRepository<LibraryScanResultEntity>,
                                                IInsertRepositoryAction<LibraryScanResultEntity>
{
    /// <summary>
    /// Gets all media library scan results that belong to the last scan of a library identified by <paramref name="libraryId"/>, from the storage medium.
    /// </summary>
    /// <param name="libraryId">The unique identifier of the library for which to get the results of the last scans.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="ErrorOr{TValue}"/> containing either a path mapped dictionary of <see cref="LibraryScanResultEntity"/>, or an error.</returns>
    Task<ErrorOr<Dictionary<string, LibraryScanResultEntity>>> GetPathMappedByLibraryIdAsync(Guid libraryId, CancellationToken cancellationToken);
}
