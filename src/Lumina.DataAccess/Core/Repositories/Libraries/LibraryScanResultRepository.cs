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
/// Repository for media library scan results.
/// </summary>
internal sealed class LibraryScanResultRepository : ILibraryScanResultRepository
{
    private readonly LuminaDbContext _luminaDbContext;

    /// <summary>
    /// Initializes a new instance of the <see cref="LibraryScanResultRepository"/> class.
    /// </summary>
    /// <param name="luminaDbContext">Injected Entity Framework DbContext.</param>
    public LibraryScanResultRepository(LuminaDbContext luminaDbContext)
    {
        _luminaDbContext = luminaDbContext;
    }

    /// <summary>
    /// Adds a new library scan result.
    /// </summary>
    /// <param name="libraryScan">The library scan result to add.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="ErrorOr{TValue}"/> representing either a successful operation, or an error.</returns>
    public Task<ErrorOr<Created>> InsertAsync(LibraryScanResultEntity libraryScan, CancellationToken cancellationToken)
    {
        _luminaDbContext.LibraryScanResults.Add(libraryScan);
        return Task.FromResult(ErrorOrFactory.From(Result.Created));
    }

    /// <summary>
    /// Gets all media library scan results that belong to the last scan of a library identified by <paramref name="libraryId"/>, from the storage medium.
    /// </summary>
    /// <param name="libraryId">The unique identifier of the library for which to get the results of the last scans.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="ErrorOr{TValue}"/> containing either a path mapped dictionary of <see cref="LibraryScanResultEntity"/>, or an error.</returns>
    public async Task<ErrorOr<Dictionary<string, LibraryScanResultEntity>>> GetPathMappedByLibraryIdAsync(Guid libraryId, CancellationToken cancellationToken)
    {
        return await _luminaDbContext.LibraryScanResults
            .Where(libraryScanResult =>
                libraryScanResult.LibraryScan.LibraryId == libraryId && // navigation property
                libraryScanResult.LibraryScanId == _luminaDbContext.LibraryScans // subquery for latest scan Id
                    .Where(libraryScan => libraryScan.LibraryId == libraryId)
                    .OrderByDescending(libraryScan => libraryScan.CreatedOnUtc)
                    .Select(libraryScan => libraryScan.Id)
                    .FirstOrDefault())
            .ToDictionaryAsync( // since these are used in large quantity comparisons, dictionary allows for O(1) lookups, instead of lists, with O(n * m)
                libraryScanResult => libraryScanResult.Path,
                libraryScanResult => libraryScanResult,
                StringComparer.Ordinal, // UNIX systems allow for case sensitive file paths
                cancellationToken).ConfigureAwait(false);
    }
}
