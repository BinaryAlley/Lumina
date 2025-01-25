#region ========================================================================= USING =====================================================================================
using ErrorOr;
using Lumina.Application.Common.DataAccess.Entities.MediaLibrary.Management;
using Lumina.Application.Common.DataAccess.Repositories.MediaLibrary;
using Lumina.DataAccess.Core.UoW;
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
/// Repository for media libraries.
/// </summary>
internal sealed class LibraryRepository : ILibraryRepository
{
    private readonly LuminaDbContext _luminaDbContext;

    /// <summary>
    /// Initializes a new instance of the <see cref="LibraryRepository"/> class.
    /// </summary>
    /// <param name="luminaDbContext">Injected Entity Framework DbContext.</param>
    public LibraryRepository(LuminaDbContext luminaDbContext)
    {
        _luminaDbContext = luminaDbContext;
    }

    /// <summary>
    /// Adds a new library.
    /// </summary>
    /// <param name="library">The library to add.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="ErrorOr{TValue}"/> representing either a successful operation, or an error.</returns>
    public async Task<ErrorOr<Created>> InsertAsync(LibraryEntity library, CancellationToken cancellationToken)
    {
        bool libraryExists = await _luminaDbContext.Libraries.AnyAsync(repositoryLibrary => repositoryLibrary.Id == library.Id, cancellationToken: cancellationToken).ConfigureAwait(false);
        if (libraryExists)
            return Errors.Library.LibraryAlreadyExists;

        _luminaDbContext.Libraries.Add(library);
        return Result.Created;
    }

    /// <summary>
    /// Gets a <see cref="LibraryEntity"/> identified by <paramref name="id"/> from the storage medium.
    /// </summary>
    /// <param name="id">The id of the library to get.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="ErrorOr{TValue}"/> containing either a <see cref="LibraryEntity"/> identified by <paramref name="id"/>, or an error.</returns>
    public async Task<ErrorOr<LibraryEntity?>> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return await _luminaDbContext.Libraries
            .Include(library => library.ContentLocations)
            .FirstOrDefaultAsync(library => library.Id == id, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Gets all media libraries that are marked as enabled, from the storage medium.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="ErrorOr{TValue}"/> containing either a collection of <see cref="LibraryEntity"/>, or an error.</returns>
    public async Task<ErrorOr<IEnumerable<LibraryEntity>>> GetAllEnabledAsync(CancellationToken cancellationToken)
    {
        return await _luminaDbContext.Libraries
            .Include(library => library.ContentLocations)
            .Where(library => library.IsEnabled)
            .ToListAsync(cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Gets all media libraries that are marked as enabled and unlocked, from the storage medium.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="ErrorOr{TValue}"/> containing either a collection of <see cref="LibraryEntity"/>, or an error.</returns>
    public async Task<ErrorOr<IEnumerable<LibraryEntity>>> GetAllEnabledAndUnlockedAsync(CancellationToken cancellationToken)
    {
        return await _luminaDbContext.Libraries
            .Include(library => library.ContentLocations)
            .Where(library => library.IsEnabled && !library.IsLocked)
            .ToListAsync(cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Gets all media libraries.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="ErrorOr{TValue}"/> containing either a collection of <see cref="LibraryEntity"/>, or an error.</returns>
    public async Task<ErrorOr<IEnumerable<LibraryEntity>>> GetAllAsync(CancellationToken cancellationToken)
    {
        return await _luminaDbContext.Libraries
            .Include(library => library.ContentLocations)
            .ToListAsync(cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Updates a media library.
    /// </summary>
    /// <param name="data">The media library to update.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="ErrorOr{TValue}"/> representing either a successful operation, or an error.</returns>
    public async Task<ErrorOr<Updated>> UpdateAsync(LibraryEntity data, CancellationToken cancellationToken)
    {
        LibraryEntity? foundLibrary = await _luminaDbContext.Libraries
            .Include(library => library.ContentLocations)
            .FirstOrDefaultAsync(library => library.Id == data.Id, cancellationToken).ConfigureAwait(false);
        if (foundLibrary is null)
            return Errors.Library.LibraryNotFound;
        // update scalar properties
        _luminaDbContext.Entry(foundLibrary).CurrentValues.SetValues(data);
        // update owned entities (their changes are not automatically tracked by EF)
        foundLibrary.ContentLocations.Clear();
        foreach (LibraryContentLocationEntity contentLocation in data.ContentLocations)
            foundLibrary.ContentLocations.Add(contentLocation);
        return Result.Updated;
    }

    /// <summary>
    /// Deletes a library by its ID.
    /// </summary>
    /// <param name="id">The ID of the library to delete.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="ErrorOr{TValue}"/> representing either a successful operation, or an error.</returns>
    public async Task<ErrorOr<Deleted>> DeleteByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        LibraryEntity? library = await _luminaDbContext.Libraries
            .Include(library => library.ContentLocations)
            .FirstOrDefaultAsync(library => library.Id == id, cancellationToken)
            .ConfigureAwait(false);

        if (library is null)
            return Errors.Library.LibraryNotFound;

        _luminaDbContext.Libraries.Remove(library);
        return Result.Deleted;
    }
}
