#region ========================================================================= USING =====================================================================================
using Nemera.Domain.Common.Primitives;
using Nemera.Application.Common.DataAccess.Entities.MediaLibrary.Management;
using Nemera.Application.Common.DataAccess.Repositories.MediaLibrary;
using Nemera.DataAccess.Core.UoW;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Nemera.DataAccess.Core.Repositories.Libraries;

/// <summary>
/// Repository for media library scan results.
/// </summary>
internal sealed class LibraryScanResultRepository : ILibraryScanResultRepository
{
    private readonly NemeraDbContext _nemeraDbContext;

    /// <summary>
    /// Initializes a new instance of the <see cref="LibraryScanResultRepository"/> class.
    /// </summary>
    /// <param name="nemeraDbContext">Injected Entity Framework DbContext.</param>
    public LibraryScanResultRepository(NemeraDbContext nemeraDbContext)
    {
        _nemeraDbContext = nemeraDbContext;
    }

    /// <summary>
    /// Adds a new library scan result.
    /// </summary>
    /// <param name="libraryScan">The library scan result to add.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="Result{TValue}"/> representing either a successful operation, or an error.</returns>
    public Task<Result<Created>> InsertAsync(LibraryScanResultEntity libraryScan, CancellationToken cancellationToken)
    {
        _nemeraDbContext.LibraryScanResults.Add(libraryScan);
        return Task.FromResult(Result.From(Result.Created));
    }
}
