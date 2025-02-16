#region ========================================================================= USING =====================================================================================
using ErrorOr;
using Lumina.Application.Common.DataAccess.Entities.MediaLibrary.Management;
using Lumina.Application.Common.DataAccess.Repositories.MediaLibrary;
using Lumina.DataAccess.Core.UoW;
using Lumina.Domain.Common.Enums.MediaLibrary;
using Lumina.Domain.Common.Errors;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.DataAccess.Core.Repositories.Libraries;

/// <summary>
/// Repository for media library scans.
/// </summary>
internal sealed class LibraryScanRepository : ILibraryScanRepository
{
    private readonly LuminaDbContext _luminaDbContext;

    /// <summary>
    /// Initializes a new instance of the <see cref="LibraryScanRepository"/> class.
    /// </summary>
    /// <param name="luminaDbContext">Injected Entity Framework DbContext.</param>
    public LibraryScanRepository(LuminaDbContext luminaDbContext)
    {
        _luminaDbContext = luminaDbContext;
    }

    /// <summary>
    /// Adds a new library scan.
    /// </summary>
    /// <param name="libraryScan">The library scan to add.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="ErrorOr{TValue}"/> representing either a successful operation, or an error.</returns>
    public async Task<ErrorOr<Created>> InsertAsync(LibraryScanEntity libraryScan, CancellationToken cancellationToken)
    {
        bool libraryScanExists = await _luminaDbContext.LibraryScans.AnyAsync(
            repositoryScanLibrary => repositoryScanLibrary.Id == libraryScan.Id, cancellationToken: cancellationToken).ConfigureAwait(false);
        if (libraryScanExists)
            return Errors.LibraryScanning.LibraryScanAlreadyExists;

        _luminaDbContext.LibraryScans.Add(libraryScan);
        return Result.Created;
    }

    /// <summary>
    /// Gets a <see cref="LibraryScanEntity"/> identified by <paramref name="id"/> from the storage medium.
    /// </summary>
    /// <param name="id">The id of the library scan to get.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="ErrorOr{TValue}"/> containing either a <see cref="LibraryScanEntity"/> identified by <paramref name="id"/>, or an error.</returns>
    public async Task<ErrorOr<LibraryScanEntity?>> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return await _luminaDbContext.LibraryScans
            .Include(libraryScan => libraryScan.Library)
            .Include(libraryScan => libraryScan.User)
            .FirstOrDefaultAsync(libraryScan => libraryScan.Id == id, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Gets the media library scans that belong to a media library identified by <paramref name="libraryId"/>, for the previous month, from the storage medium.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="ErrorOr{TValue}"/> containing either a collection of <see cref="LibraryScanEntity"/>, or an error.</returns>
    public async Task<ErrorOr<IEnumerable<LibraryScanEntity>>> GetPastMonthScansByLibraryIdAsync(Guid libraryId, CancellationToken cancellationToken)
    {
        return await _luminaDbContext.LibraryScans
            .Include(library => library.Library)
            .Where(library => library.LibraryId == libraryId && library.CreatedOnUtc >= DateTime.UtcNow.AddMonths(-1))
            .ToListAsync(cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Gets the media library scans that have a <see cref="LibraryScanJobStatus.Running"/> status, from the storage medium.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="ErrorOr{TValue}"/> containing either a collection of <see cref="LibraryScanEntity"/>, or an error.</returns>
    public async Task<ErrorOr<IEnumerable<LibraryScanEntity>>> GetRunningScansAsync(CancellationToken cancellationToken)
    {
        return await _luminaDbContext.LibraryScans
            .Include(library => library.Library)
            .Where(library => library.Status == LibraryScanJobStatus.Running)
            .ToListAsync(cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Updates a media library scan.
    /// </summary>
    /// <param name="data">The media library scan to update.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="ErrorOr{TValue}"/> representing either a successful operation, or an error.</returns>
    public async Task<ErrorOr<Updated>> UpdateAsync(LibraryScanEntity data, CancellationToken cancellationToken)
    {
        LibraryScanEntity? foundLibraryScan = await _luminaDbContext.LibraryScans
            .Include(libraryScan => libraryScan.Library)
            .FirstOrDefaultAsync(libraryScan => libraryScan.Id == data.Id, cancellationToken).ConfigureAwait(false);
        if (foundLibraryScan is null)
            return Errors.LibraryScanning.LibraryScanNotFound;
        // update scalar properties
        _luminaDbContext.Entry(foundLibraryScan).CurrentValues.SetValues(data);
        return Result.Updated;
    }
}
