#region ========================================================================= USING =====================================================================================
using Lumina.Domain.Common.Primitives;
using Lumina.Application.Common.DataAccess.Entities.MediaLibrary.Management;
using Lumina.Application.Common.DataAccess.Repositories.Common.Base;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Application.Common.DataAccess.Repositories.MediaLibrary;

/// <summary>
/// Interface for the repository for directory scan fingerprints.
/// </summary>
public interface IDirectoryScanFingerprintRepository : IRepository<DirectoryScanFingerprintEntity>
{
    /// <summary>
    /// Gets the directory scan fingerprints of a media library, mapped by the directory path.
    /// </summary>
    /// <param name="libraryId">The unique identifier of the library for which to get the directory scan fingerprints.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="Result{TValue}"/> containing either a path mapped dictionary of directory scan fingerprints, or an error.</returns>
    Task<Result<Dictionary<string, DirectoryScanFingerprintEntity>>> GetMappedByLibraryIdAsync(Guid libraryId, CancellationToken cancellationToken);

    /// <summary>
    /// Inserts or updates the provided directory scan fingerprints in the storage medium.
    /// </summary>
    /// <param name="entities">The directory scan fingerprints to insert or update.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="Result{TValue}"/> representing either a successful operation, or an error.</returns>
    Task<Result<Updated>> UpsertRangeAsync(IReadOnlyCollection<DirectoryScanFingerprintEntity> entities, CancellationToken cancellationToken);
}
