#region ========================================================================= USING =====================================================================================
using ErrorOr;
using Lumina.Application.Common.DataAccess.Entities.MediaLibrary.Management;
using Lumina.Application.Common.DataAccess.Repositories.MediaLibrary;
using Lumina.DataAccess.Core.UoW;
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
}
