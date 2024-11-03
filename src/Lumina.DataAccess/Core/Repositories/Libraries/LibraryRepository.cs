#region ========================================================================= USING =====================================================================================
using ErrorOr;
using Lumina.Application.Common.DataAccess.Repositories.MediaLibrary;
using Lumina.Contracts.Entities.MediaLibrary.Management;
using Lumina.DataAccess.Core.UoW;
using Lumina.Domain.Common.Errors;
using Microsoft.EntityFrameworkCore;
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
    /// <returns>An <see cref="ErrorOr{TValue}"/> representing either a successfull operation, or an error.</returns>
    public async Task<ErrorOr<Created>> InsertAsync(LibraryEntity library, CancellationToken cancellationToken)
    {
        bool libraryExists = await _luminaDbContext.Libraries.AnyAsync(repositoryLibrary => repositoryLibrary.Id == library.Id, cancellationToken: cancellationToken).ConfigureAwait(false);
        if (libraryExists)
            return Errors.Library.LibraryAlreadyExists;

        _luminaDbContext.Libraries.Add(library);
        return Result.Created;
    }
}
